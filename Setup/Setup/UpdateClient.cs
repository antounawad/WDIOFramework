using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml.Serialization;
using Eulg.Setup.Shared;
using Eulg.Shared;

namespace Eulg.Setup
{
    public class UpdateClient
    {
        public enum EUpdateCheckResult
        {
            NoConnection,
            NoService,
            Error,
            AuthFail,
            UpToDate,
            UpdatesAvailable,
            ClientIdFail,
            InitialPasswordNotSet,
        }

        public enum ELogTypeEnum
        {
            Info,
            Warning,
            Error
        }

        private const string UPDATE_WORKER_BIN_FILE = "UpdateWorker.exe";
        private const string UPDATE_WORKER_XML_FILE = "UpdateWorker.xml";
        private const string RESET_FILE_TAG = ".$EulgReset$";
        private const string FETCH_UPDATE_DATA_METHOD = "FilesUpdateCheck";
        private const string DOWNLOAD_FILE_METHOD = "FilesUpdateGetFileGz";
        private const string DOWNLOAD_FILES_STREAM_METHOD = "FileUpdateGetFiles";
        private const string UPLOAD_LOG_METHOD = "FilesUpdateUploadLog";
        private const string RESET_CLIENT_ID_METHOD = "FilesUpdateResetClientId";
        private const int STREAM_BUFFER_SIZE = 81920;
        private const bool UseDeflate = false;

        public bool UseHttps { get; set; }
        public UpdateConfig UpdateConf = new UpdateConfig();
        public readonly WorkerConfig WorkerConfig = new WorkerConfig();
        public Branding.EUpdateChannel UpdateChannel { get; set; }
        public string ApplicationPath { get; set; }
        public string LastError { get; private set; }

        public string UpdateUrl
        {
            get { return _updateUrl; }
            set
            {
                _updateUrl = value;
                if (_updateUrl.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
                {
                    _updateUrl = _updateUrl.Substring(7);
                }
                if (_updateUrl.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
                {
                    _updateUrl = _updateUrl.Substring(8);
                }
                if (!_updateUrl.EndsWith("/"))
                {
                    _updateUrl += "/";
                }
            }
        }

        private string _updateUrl;
        private readonly WebClient _webClient = new WebClient();

        public UpdateClient()
        {
            _webClient.Encoding = Encoding.UTF8;
            _webClient.Headers["User-Agent"] = "EulgSetup";
            UseHttps = false;
        }

        public readonly List<string> LogMessages = new List<string>();
        public readonly List<string> LogErrorMessages = new List<string>();
        public event ProgressChangedEventHandler ProgressChanged;
        public string UserName { get; set; }
        public string Password { get; set; }

        // Progress indication
        public long DownloadSizeTotal { get; private set; }
        public long DownloadSizeCompleted { get; private set; }
        public long DownloadFilesTotal { get; private set; }
        public long DownloadFilesCompleted { get; private set; }
        public long DownloadCurrentFileSize { get; private set; }
        public long DownloadCurrentFileSizeGz { get; private set; }
        public string DownloadCurrentFilename { get; private set; }

        public DateTime? ClientIdUsedDateTime { get; private set; }
        public string ClientIdUsedOsUsername { get; private set; }
        public string ClientIdUsedHostname { get; private set; }

        public static bool CheckConnectivity()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public EUpdateCheckResult DownloadManifest()
        {
            try
            {
                var baseUri = new Uri((UseHttps ? "https://" : "http://") + UpdateUrl);
                var uri = new Uri(baseUri, FETCH_UPDATE_DATA_METHOD);
                var url = uri + "?updateChannel=" + UpdateChannel + "&userName=" + Uri.EscapeDataString(UserName ?? String.Empty)
                          + "&password=" + Uri.EscapeDataString(Password ?? String.Empty);

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                request.Headers.Add("ClientID", FingerPrint.ClientId);
                request.Headers.Add("ClientOSUsername", Environment.UserName + "@" + Environment.UserDomainName);
                request.Headers.Add("ClientHostname", Environment.MachineName);
                request.Headers.Add("ClientUpdateType", "SETUP");

                var result = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

                if (string.IsNullOrWhiteSpace(result))
                {
                    return EUpdateCheckResult.Error;
                }
                if (result.StartsWith("INITIALPASSWORDNOTSET"))
                {
                    return EUpdateCheckResult.InitialPasswordNotSet;
                }
                if (result.StartsWith("AUTHFAIL"))
                {
                    return EUpdateCheckResult.AuthFail;
                }
                if (result.StartsWith("CLIENTIDFAIL"))
                {
                    var used = result.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (used.Length > 3)
                    {
                        DateTime dt;
                        if (DateTime.TryParse(used[1], out dt))
                        {
                            ClientIdUsedDateTime = dt;
                        }
                        ClientIdUsedOsUsername = used[2];
                        ClientIdUsedHostname = used[3];
                    }
                    return EUpdateCheckResult.ClientIdFail;
                }
                var xmlSer = new XmlSerializer(typeof(UpdateConfig));
                using (var reader = new StringReader(result))
                {
                    UpdateConf = (UpdateConfig)xmlSer.Deserialize(reader);
                }
                return EUpdateCheckResult.UpdatesAvailable;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(ELogTypeEnum.Error, UpdateUrl + ": " + ex.Message);
            }
            return EUpdateCheckResult.Error;
        }

        public EUpdateCheckResult CheckForUpdates()
        {
            if (CompareFiles())
            {
                Log(ELogTypeEnum.Info, "Setup Files available on " + UpdateUrl);
                return EUpdateCheckResult.UpdatesAvailable;
            }
            Log(ELogTypeEnum.Info, "Setup UpToDate on " + UpdateUrl);
            return EUpdateCheckResult.UpToDate;
        }

        public bool DownloadUpdatesStream()
        {
            try
            {
                var filteredWorkerFiles = WorkerConfig.WorkerFiles.Where(f =>
                {
                    var fileInfo = new FileInfo(f.Destination);
                    return !fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, f.FileDateTime) || fileInfo.Length != f.FileSize;
                }).ToList();

                DownloadSizeTotal = filteredWorkerFiles.Sum(s => s.FileSizeGz - (UseDeflate ? 18 : 0));
                DownloadFilesTotal = filteredWorkerFiles.Count;
                DownloadSizeCompleted = 0;
                DownloadFilesCompleted = 0;

                if (DownloadFilesTotal > 0)
                {
                    var baseUri = new Uri("http://" + UpdateUrl);
                    var uri = new Uri(baseUri, DOWNLOAD_FILES_STREAM_METHOD);

                    ServicePointManager.Expect100Continue = false;
                    //ServicePointManager.SetTcpKeepAlive(false, 0, 0);

                    var webRequest = (HttpWebRequest)WebRequest.Create(uri);
                    //webRequest.KeepAlive = false;
                    //webRequest.AllowWriteStreamBuffering = false;
                    //webRequest.ProtocolVersion = HttpVersion.Version10;

                    webRequest.Method = "POST";
                    webRequest.Headers.Add("Channel", UpdateChannel.ToString());
                    webRequest.Headers.Add("ManifestTimestamp", $"{DateTime.Now:yyyyMMddHHmmss}");
                    webRequest.Headers.Add("CompressionType", (UseDeflate ? "Deflate" : "GZipHack"));
                    webRequest.Headers.Add("ContentLength", DownloadSizeTotal.ToString());

                    var byteArray = CompressStringDeflate(string.Join(";", filteredWorkerFiles.Select(_ => _.Source).ToArray()));
                    webRequest.ContentType = "application/octet-stream";
                    webRequest.ContentLength = byteArray.Length;
                    using (var requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(byteArray, 0, byteArray.Length);
                        requestStream.Close();
                    }

                    using (var response = webRequest.GetResponse())
                    {
                        //Console.WriteLine(((HttpWebResponse)response).StatusCode);
                        using (var responseStream = response.GetResponseStream())
                        {
                            var currentFile = 0;
                            while (currentFile < filteredWorkerFiles.Count)
                            {
                                var workerFile = filteredWorkerFiles[currentFile];
                                DownloadCurrentFilename = workerFile.FileName;
                                DownloadCurrentFileSize = workerFile.FileSize;
                                DownloadCurrentFileSizeGz = workerFile.FileSizeGz;
                                NotifyProgressChanged();
                                var localFile = workerFile.Destination;
                                if (!Directory.Exists(Path.GetDirectoryName(localFile) ?? string.Empty))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(localFile) ?? string.Empty);
                                }
                                var baseDownloadSize = DownloadSizeCompleted;
                                using (var sw = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    using (var view = new SectionStream(responseStream, workerFile.FileSizeGz - (UseDeflate ? 18 : 0), !UseDeflate))
                                    {
                                        if (UseDeflate)
                                        {
                                            using (var gz = new DeflateStream(view, CompressionMode.Decompress))
                                            {
                                                var buffer = new byte[STREAM_BUFFER_SIZE];
                                                var total = 0;
                                                while (total < workerFile.FileSize)
                                                {
                                                    var read = gz.Read(buffer, 0, Math.Min(STREAM_BUFFER_SIZE, (int)workerFile.FileSize - total));
                                                    sw.Write(buffer, 0, read);
                                                    total += read;
                                                    DownloadSizeCompleted = baseDownloadSize + view.Position;
                                                    NotifyProgressChanged(view.Position);
                                                    if (SetupHelper.CancelRequested)
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            using (var gz = new GZipStream(view, CompressionMode.Decompress))
                                            {
                                                var buffer = new byte[STREAM_BUFFER_SIZE];
                                                var total = 0;
                                                while (total < workerFile.FileSize)
                                                {
                                                    var read = gz.Read(buffer, 0, Math.Min(STREAM_BUFFER_SIZE, (int)workerFile.FileSize - total));
                                                    sw.Write(buffer, 0, read);
                                                    total += read;
                                                    DownloadSizeCompleted = baseDownloadSize + view.Position;
                                                    NotifyProgressChanged(view.Position);
                                                    if (SetupHelper.CancelRequested)
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                        DownloadSizeCompleted = baseDownloadSize + workerFile.FileSizeGz;
                                        NotifyProgressChanged(view.Position);
                                    }
                                }
                                File.SetLastWriteTime(localFile, workerFile.FileDateTime);
                                currentFile++;
                                DownloadFilesCompleted++;
                                // Reset-Files:
                                if (workerFile.Destination.EndsWith(RESET_FILE_TAG, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var activeFile = localFile.Substring(0, localFile.Length - RESET_FILE_TAG.Length);
                                    File.Copy(localFile, activeFile, true);
                                }
                            }
                            //responseStream.Close();
                        }
                        //response.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(ELogTypeEnum.Error, ex.ToString());
                return false;
            }
        }

        public void DeleteClientIdOnServer()
        {
            try
            {
                UpdateChannel = SetupHelper.Config.Branding.Update.Channel;
                UpdateUrl = SetupHelper.Config.Branding.Urls.Update;
                UseHttps = SetupHelper.Config.Branding.Update.UseHttps;

                var baseUri = new Uri((UseHttps ? "https://" : "http://") + UpdateUrl);
                var uri = new Uri(baseUri, RESET_CLIENT_ID_METHOD);

                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                request.Headers.Add("ClientID", FingerPrint.ClientId);
                request.Headers.Add("ClientOSUsername", Environment.UserName + "@" + Environment.UserDomainName);
                request.Headers.Add("ClientHostname", Environment.MachineName);

                var result = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();
            }
            catch
            {
            }
        }

        public void Log(ELogTypeEnum logType, string message)
        {
            var msg = $"{logType.ToString().PadRight(7)}: {message.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", " ")}";
            LogMessages.Add(msg);
            if (logType != ELogTypeEnum.Info)
            {
                LogErrorMessages.Add(msg);
            }
        }

        public bool UploadLogfile()
        {
            try
            {
                if (LogMessages.Count == 0)
                {
                    return true;
                }
                if (!CheckConnectivity())
                {
                    return true;
                }
                var baseUri = new Uri("http://" + UpdateUrl);
                var uri = new Uri(baseUri, UPLOAD_LOG_METHOD);
                var tmp = LogMessages.Aggregate(string.Empty, (current, logMessage) => current + (logMessage + Environment.NewLine));

                var nvc = new NameValueCollection
                          {
                              {"updateChannel", UpdateChannel.ToString()},
                              {"userName", string.IsNullOrWhiteSpace(UserName) ? Environment.UserName + "@" + Environment.UserDomainName : UserName},
                              {"logFileDateTime", $"{DateTime.Now:dd.MM.yyyy HH:mm:ss}"},
                              {"logFileContent", CompressStringGz(tmp)}
                          };
                _webClient.UploadValues(uri.AbsoluteUri, "POST", nvc);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(ELogTypeEnum.Error, ex.Message);
                return false;
            }
        }

        private bool CompareFiles()
        {
            WorkerConfig.WorkerFiles.Clear();
            // AppDir
            CompareDirectory(ApplicationPath, UpdateConf.UpdateFiles.Where(w => w.FilePath == "AppDir"));
            // Support
            CompareDirectory(Path.Combine(ApplicationPath, "Support"), UpdateConf.UpdateFiles.Where(w => w.FilePath == "Support"));
            // Plugins
            CompareDirectory(Path.Combine(ApplicationPath, "Plugins"), UpdateConf.UpdateFiles.Where(w => w.FilePath == "Plugins"));
            // AppDir Deletes
            CompareDeletes(ApplicationPath, UpdateConf.UpdateDeletes.Where(w => w.FilePath == "AppDir"));

            var updateService = UpdateConf.UpdateFiles.Where(w => w.FilePath == "" && w.FileName.Equals("UpdateService.exe", StringComparison.CurrentCultureIgnoreCase));
            CompareDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH", "UpdateService"), updateService);

            return WorkerConfig.WorkerFiles.Any();
        }

        private void CompareDirectory(string path, IEnumerable<UpdateConfig.UpdateFile> updateFiles)
        {
            foreach (var updateFile in updateFiles)
            {
                var localFile = Path.Combine(path, updateFile.FileName);
                CompareFile(updateFile, localFile);
            }
        }

        private void CompareFile(UpdateConfig.UpdateFile updateFile, string localFile)
        {
            // Wenn Branding.xml lokal schon vorhanden - auf keinen fall überschreiben!!!
            if (updateFile.FilePath.Equals("AppDir", StringComparison.CurrentCultureIgnoreCase) &&
                updateFile.FileName.Equals("Branding.xml", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            var fileInfo = new FileInfo(localFile);
            if (!fileInfo.Exists ||
                !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, updateFile.FileDateTime) ||
                fileInfo.Length != updateFile.FileSize)
            {
                WorkerConfig.WorkerFiles.Add(new WorkerConfig.WorkerFile
                {
                    Source = Path.Combine(updateFile.FilePath, updateFile.FileName),
                    Destination = localFile,
                    CheckSum = updateFile.CheckSum,
                    FileDateTime = updateFile.FileDateTime,
                    FileSize = updateFile.FileSize,
                    FileSizeGz = updateFile.FileSizeGz,
                    FileName = updateFile.FileName
                });
            }
        }

        private void CompareDeletes(string path, IEnumerable<UpdateConfig.UpdateDelete> updateDeletes)
        {
            foreach (var updateDelete in updateDeletes)
            {
                var localPath = Path.Combine(path, updateDelete.FileName);
                if (File.Exists(localPath) || Directory.Exists(localPath))
                {
                    WorkerConfig.WorkerDeletes.Add(new WorkerConfig.WorkerDelete
                    {
                        Path = localPath
                    });
                }
            }
        }

        private int _oldPercentage;

        private void NotifyProgressChanged(long currentFilePosition = 0)
        {
            try
            {
                if (ProgressChanged != null)
                {
                    if (currentFilePosition == 0)
                    {
                        _oldPercentage = 0;
                    }
                    var percent = Convert.ToInt32(100f * DownloadSizeCompleted / DownloadSizeTotal);
                    if (currentFilePosition == 0 || percent != _oldPercentage)
                    {
                        ProgressChanged(this, new ProgressChangedEventArgs(percent, DownloadCurrentFilename));
                    }
                    _oldPercentage = percent;
                }
            }
            catch (Exception exception)
            {
                Log(ELogTypeEnum.Error, exception.Message);
            }
        }

        private static string CompressStringGz(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        msi.CopyTo(gs);
                    }
                    return Convert.ToBase64String(mso.ToArray());
                }
            }
        }

        private static byte[] CompressStringDeflate(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new DeflateStream(mso, CompressionMode.Compress))
                    {
                        msi.CopyTo(gs);
                    }
                    return mso.ToArray();
                }
            }
        }
    }
}
