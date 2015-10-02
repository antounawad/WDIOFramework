using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Eulg.Setup.Shared;
using Eulg.Shared;

namespace Eulg.Setup.WebInstaller
{
    public static class Program
    {
        // ReSharper disable UnusedMember.Local
        private enum EBrandingProfile
        {
            Release,
            Demo,
            PreRel,
            Test,
            DemoTest,
            Beta,
            Proof,
            ProofTest,
            EulgDeTestRelease
        }
        // ReSharper restore UnusedMember.Local

        private const EBrandingProfile BRANDING_PROFILE = EBrandingProfile.EulgDeTestRelease;

        //public static WebClient WebClient;
        private const string DOWNLOAD_FILE_METHOD = "FilesUpdateGetFileDeflate";
        private const string FETCH_UPDATE_DATA_METHOD = "FilesUpdateCheck";
        private const int STREAM_BUFFER_SIZE = 81920;

        private static Mutex _appInstanceMutex;
        private static UpdateConfig _updateConfig = new UpdateConfig();

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ProxyConfig.Init();

            //WebClient = new WebClient { Encoding = Encoding.UTF8, Headers = {["User-Agent"] = "WebSetup" } };

            Branding.Current = GetBranding(BRANDING_PROFILE);

            if (Environment.GetCommandLineArgs().Any(_ => _.Equals("/info", StringComparison.InvariantCultureIgnoreCase)))
            {
                var msg = "BrandingProfile: " + BRANDING_PROFILE + Environment.NewLine + "Tag: " + Branding.Current.Info.BuildTag;
                MessageBox.Show(msg, "EULG WebInstaller", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (AlreadyRunning())
                {
                    MessageBox.Show("Setup läuft bereits!", "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                var frmStatus = new FrmStatus();
                frmStatus.Show();
                frmStatus.Refresh();
                Application.DoEvents();
                Cursor.Current = Cursors.WaitCursor;

                // Temp. Ordner vorbereiten
                var tmpFolder = Path.Combine(Path.GetTempPath(), "EulgWebInstaller");
                DelTree(tmpFolder);
                if (!Directory.Exists(tmpFolder))
                {
                    Directory.CreateDirectory(tmpFolder);
                }

                // Check Network
                if (!CheckConnectivity())
                {
                    MessageBox.Show("Bitte überprüfen Sie Ihre Internetverbindung und versuchen Sie es erneut!", "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                // Download Manifest
                var manifestDone = false;
                while (!manifestDone)
                {
                    try
                    {
                        DownloadManifest();
                        manifestDone = true;
                    }
                    catch (Exception exception)
                    {
                        using (var frmShowError = new FrmShowError())
                        {
                            frmShowError.TxtMessage.Text = exception.GetMessagesTree();
                            var result = frmShowError.ShowDialog();
                            if (result == DialogResult.Cancel)
                            {
                                Application.Exit();
                                return;
                            }
                        }
                    }
                }

                // Download Files
                DownloadFiles(tmpFolder);

                // Write Config File
                using (var file = File.Create(Path.Combine(tmpFolder, "Setup.xml")))
                {
                    var xser = new XmlSerializer(typeof(SetupConfig));
                    xser.Serialize(file, new SetupConfig
                    {
                        Version = _updateConfig.Version,
                        Branding = Branding.Current,
                    });
                }

                Cursor.Current = Cursors.Default;
                frmStatus.Close();
                var p = new Process
                {
                    StartInfo =
                            {
                                FileName = Path.Combine(tmpFolder, "Setup.exe"),
                                Arguments = "/W",
                                //Verb = "runas"
                            }
                };
                p.Start();
            }
            catch (Exception exception)
            {
                using (var frmShowError = new FrmShowError())
                {
                    frmShowError.TxtMessage.Text = exception.GetMessagesTree();
                    frmShowError.BtnProxySettings.Visible = false;
                    frmShowError.BtnRetry.Visible = false;
                    frmShowError.ShowDialog();
                }
            }
            Application.Exit();
        }

        private static Branding GetBranding(EBrandingProfile profile)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var res = "Eulg.Setup.WebInstaller.Profiles." + profile + ".xml";
            using (var stream = assembly.GetManifestResourceStream(res))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlSerializer = new XmlSerializer(typeof(Branding));
                        return xmlSerializer.Deserialize(reader) as Branding;
                    }
                }
            }
            return null;
        }
        private static bool AlreadyRunning()
        {
            _appInstanceMutex = MutexManager.Instance.Acquire("WebInstaller", TimeSpan.Zero);
            return _appInstanceMutex == null;
        }
        private static void DelTree(string path)
        {
            // ReSharper disable EmptyGeneralCatchClause
            try
            {
                foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(file);
                    }

                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            try
            {
                foreach (var directory in Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    DelTree(directory);
                    try
                    {
                        Directory.Delete(directory);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            try
            {
                Directory.Delete(path);
            }
            catch
            {
            }
            // ReSharper restore EmptyGeneralCatchClause
        }
        private static bool CheckConnectivity()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        private static void DownloadManifest()
        {
            var baseUri = new Uri(GetUpdateUrl(true));
            var uri = new Uri(baseUri, FETCH_UPDATE_DATA_METHOD);
            var url = uri + "?updateChannel=" + Branding.Current.Update.Channel;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            var responseStream = request.GetResponse().GetResponseStream();
            if (responseStream == null)
            {
                return;
            }
            var result = new StreamReader(responseStream).ReadToEnd();

            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }
            var xmlSer = new XmlSerializer(typeof(UpdateConfig));
            using (var reader = new StringReader(result))
            {
                _updateConfig = (UpdateConfig)xmlSer.Deserialize(reader);
            }
            if (_updateConfig.BrandingVersion > Branding.BrandingVersion)
            {
                const string MSG = "Dieser Installationsassistent ist leider nicht mehr aktuell. " +
                                   "Bitte verwenden Sie einen aktuellen Assistenten, den Sie in Ihrer Webverwaltung herunterladen können!";
                MessageBox.Show(MSG, "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }
        private static void DownloadFiles(string toPath)
        {
            foreach (var file in _updateConfig.UpdateFiles.Where(w => w.FilePath.Equals("Setup", StringComparison.CurrentCultureIgnoreCase)))
            {
                var localFile = Path.Combine(toPath, file.FileName);
                DownloadFile(Path.Combine(file.FilePath, file.FileName), localFile, file.FileDateTime);
            }
        }
        private static void DownloadFile(string fileName, string localFile, DateTime dateTime)
        {
            if (!Directory.Exists(Path.GetDirectoryName(localFile) ?? string.Empty))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(localFile) ?? string.Empty);
            }
            var baseUri = new Uri(GetUpdateUrl(false));
            var uri = new Uri(baseUri, DOWNLOAD_FILE_METHOD);
            var url = uri + "?updateChannel=" + Branding.Current.Update.Channel + "&fileName=" + Uri.UnescapeDataString(fileName);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = WebRequest.DefaultWebProxy;
            var response = request.GetResponse();
            using (var sr = response.GetResponseStream())
            {
                if (sr == null)
                {
                    throw new Exception("Fehler beim öffnen der URL: " + url);
                }
                using (var sw = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var gz = new DeflateStream(sr, CompressionMode.Decompress))
                    {
                        var buffer = new byte[STREAM_BUFFER_SIZE];
                        int read;
                        while ((read = gz.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            sw.Write(buffer, 0, read);
                        }
                    }
                }
            }
            File.SetLastWriteTime(localFile, dateTime);
        }
        private static string GetUpdateUrl(bool httpsIfAvailable)
        {
            var t = ((httpsIfAvailable && Branding.Current.Update.UseHttps) ? "https://" : "http://") + Branding.Current.Urls.Update;
            if (!t.EndsWith("/"))
            {
                t += "/";
            }
            return t;
        }

    }
}
