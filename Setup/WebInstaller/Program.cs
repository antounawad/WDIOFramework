using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Eulg.Setup.Shared;
using Eulg.Shared;

namespace Eulg.Setup.WebInstaller
{
    public static class Program
    {
        #region Profiles

        private sealed class BrandingProfile
        {
            public string ApiManifestUri { get; }
            public Branding.EUpdateChannel Channel { get; }

            private BrandingProfile(string apiManifestUri, Branding.EUpdateChannel channel = Branding.EUpdateChannel.Release)
            {
                ApiManifestUri = apiManifestUri;
                Channel = channel;
            }

            // ReSharper disable UnusedMember.Local
            public static readonly BrandingProfile Local = new BrandingProfile("http://localhost:1591/ApiManifest/JsonGet");
            public static readonly BrandingProfile Release = new BrandingProfile("https://service.xbav-berater.de/ApiManifest/JsonGet");
            public static readonly BrandingProfile Test = new BrandingProfile("http://192.168.0.4/Service/ApiManifest/JsonGet");
            public static readonly BrandingProfile EulgDeTest = new BrandingProfile("https://test.eulg.de/Service/ApiManifest/JsonGet");
            // ReSharper restore UnusedMember.Local
        }

        #endregion

        #region SetupManifest

        public class SetupFile
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public int Length { get; set; }
        }

        public class SetupFiles : List<SetupFile> { }

        [XmlRoot]
        public class SetupManifest
        {
            [XmlAttribute]
            public string SetupExe { get; set; }

            [XmlElement]
            public SetupFiles Files { get; set; }
        }

        #endregion

        private static readonly BrandingProfile Profile = BrandingProfile.Release;
        private static Mutex _appInstanceMutex;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ProxyConfig.Instance.Init();

            if (Environment.GetCommandLineArgs().Any(_ => _.Equals("/info", StringComparison.InvariantCultureIgnoreCase)))
            {
                var msg = "API Manifest URL: " + Profile.ApiManifestUri + Environment.NewLine + "Channel: " + Profile.Channel;
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
                var tmpFolder = Path.Combine(Path.GetTempPath(), "xbAV_WebInstaller");
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
                SetupManifest manifest = null;
                while (manifest == null)
                {
                    try
                    {
                        var apiClient = new ApiResourceClient(Profile.ApiManifestUri, Profile.Channel);
                        var endpoints = apiClient.Fetch();

                        manifest = DownloadSetup(tmpFolder, endpoints[EApiResource.UpdateService]);
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

                // Write Config File
                using (var file = File.Create(Path.Combine(tmpFolder, "Setup.xml")))
                {
                    var xser = new XmlSerializer(typeof(SetupConfig));
                    xser.Serialize(file, new SetupConfig
                    {
                        ApiManifestUri = Profile.ApiManifestUri,
                        Channel = Profile.Channel
                    });
                }

                Cursor.Current = Cursors.Default;
                frmStatus.Close();
                using (var p = new Process
                {
                    StartInfo =
                    {
                        FileName = Path.Combine(tmpFolder, manifest.SetupExe),
                        Arguments = "/W"
                    }
                })
                {
                    p.Start();
                }
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
        private static SetupManifest DownloadSetup(string tempPath, Uri updateService)
        {
            var uri = new Uri(updateService, "WebInstGetSetup");

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.AutomaticDecompression = DecompressionMethods.None;
            var responseStream = request.GetResponse().GetResponseStream();
            if (responseStream == null)
            {
                return null;
            }

            SetupManifest manifest;
            using (var package = File.Create(Path.Combine(tempPath, Path.GetRandomFileName()), 8192, FileOptions.DeleteOnClose))
            {
                using (responseStream)
                {
                    using(var inflate = new DeflateStream(responseStream, CompressionMode.Decompress))
                    {
                        inflate.CopyTo(package);
                    }
                }

                package.Position = 0;

                var manifestLengthBytes = new byte[4];
                package.Read(manifestLengthBytes, 0, 4);

                var manifestLength = BitConverter.ToInt32(manifestLengthBytes, 0);
                using (var manifestStream = new MemoryStream(package.Read(manifestLength)))
                {
                    manifest = (SetupManifest)new XmlSerializer(typeof(SetupManifest)).Deserialize(manifestStream);
                }

                foreach (var file in manifest.Files)
                {
                    using (var stream = File.Open(Path.Combine(tempPath, file.Name), FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        package.Copy(stream, file.Length);
                    }
                }
            }
            return manifest;
        }
    }
}
