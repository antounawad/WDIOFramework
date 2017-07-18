using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        //private const string UPDATE_WORKER_BIN_FILE = "UpdateWorker.exe";
        //private const string UPDATE_WORKER_XML_FILE = "UpdateWorker.xml";
        private const string RESET_FILE_TAG = ".$EulgReset$";
        private const string FETCH_UPDATE_DATA_METHOD = "FilesUpdateCheck";
        private const string DOWNLOAD_FILE_METHOD = "FilesUpdateGetFileGz";
        //private const string DOWNLOAD_FILES_STREAM_METHOD = "FileUpdateGetFiles";
        private const string UPLOAD_LOG_METHOD = "FilesUpdateUploadLog";
        private const string RESET_CLIENT_ID_METHOD = "FilesUpdateResetClientId";
        private const int STREAM_BUFFER_SIZE = 81920;
        //private const bool UseDeflate = false;

        public UpdateConfig UpdateConf = new UpdateConfig();
        public readonly WorkerConfig WorkerConfig = new WorkerConfig();

        public Branding.EUpdateChannel UpdateChannel { get; }
        public string ServiceUrl { get; }

        public string ApplicationPath { get; set; }
        public string LastError { get; private set; }

        private readonly WebClient _webClient = new WebClient();

        public UpdateClient(string serviceUrl, Branding.EUpdateChannel channel)
        {
            ServiceUrl = serviceUrl;
            UpdateChannel = channel;

            _webClient.Encoding = Encoding.UTF8;
            _webClient.Headers["User-Agent"] = "XbavSetup";
        }

        public readonly List<string> LogMessages = new List<string>();
        public readonly List<string> LogErrorMessages = new List<string>();

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
                var uri = SetupHelper.GetUpdateApi(ServiceUrl, UpdateChannel, FETCH_UPDATE_DATA_METHOD);
                if (uri == null) return EUpdateCheckResult.NoService;

                var url = uri + "?updateChannel=" + UpdateChannel + "&userName=" + Uri.EscapeDataString(UserName ?? String.Empty)
                          + "&password=" + Uri.EscapeDataString(Password ?? String.Empty);

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                try
                {
                    request.Headers.Add("ClientID", FingerPrint.ClientId);
                }
                catch (Exception e)
                {
                    Log(ELogTypeEnum.Warning, e.GetMessagesTree());
                }
                request.Headers.Add("ClientOSUsername", Environment.UserName + "@" + Environment.UserDomainName);
                request.Headers.Add("ClientHostname", Environment.MachineName);
                request.Headers.Add("ClientUpdateType", "SETUP");

                var result = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

                if (string.IsNullOrWhiteSpace(result))
                {
                    return EUpdateCheckResult.Error;
                }
                if (result.StartsWith("INITIALPASSWORDNOTSET", StringComparison.Ordinal))
                {
                    return EUpdateCheckResult.InitialPasswordNotSet;
                }
                if (result.StartsWith("AUTHFAIL", StringComparison.Ordinal))
                {
                    return EUpdateCheckResult.AuthFail;
                }
                if (result.StartsWith("CLIENTIDFAIL", StringComparison.Ordinal))
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
                Log(ELogTypeEnum.Error, ServiceUrl + ": " + ex.Message + ": " + ex.StackTrace);
            }
            return EUpdateCheckResult.Error;
        }

        public EUpdateCheckResult CheckForUpdates()
        {
            if (CompareFiles())
            {
                Log(ELogTypeEnum.Info, "Setup Files available on " + ServiceUrl);
                return EUpdateCheckResult.UpdatesAvailable;
            }
            Log(ELogTypeEnum.Info, "Setup UpToDate on " + ServiceUrl);
            return EUpdateCheckResult.UpToDate;
        }

        private long _inProgress;
        private Uri _downloadFileUri;
        public bool DownloadUpdates()
        {
            try
            {
                #region Outer Try/Catch

                var filteredWorkerFiles = WorkerConfig.WorkerFiles.Where(f =>
                {
                    var fileInfo = new FileInfo(f.Destination);
                    return !fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, f.FileDateTime) || fileInfo.Length != f.FileSize;
                }).ToList();

                // TODO: GetUpdateClient();

                DownloadSizeTotal = filteredWorkerFiles.Sum(s => s.FileSize);
                DownloadFilesTotal = filteredWorkerFiles.Count;
                DownloadSizeCompleted = 0;
                DownloadFilesCompleted = 0;
                var currentFile = 0;
                var currentFileDiff = 0;

                _downloadFileUri = SetupHelper.GetUpdateApi(ServiceUrl, UpdateChannel, DOWNLOAD_FILE_METHOD, true);

                var log = new List<Tuple<ELogTypeEnum, string>>();
                Parallel.ForEach(filteredWorkerFiles, new ParallelOptions { MaxDegreeOfParallelism = 4 }, workerFile =>
                {
                    if (SetupHelper.CancelRequested) return;
                    var trialNumber = 0;
                    DownloadCurrentFilename = workerFile.FileName;
                    DownloadCurrentFileSize = workerFile.FileSize;
                    DownloadCurrentFileSizeGz = workerFile.FileSizeGz;
                    NotifyProgressChanged(DownloadSizeCompleted + _inProgress, DownloadSizeTotal, currentFile + currentFileDiff, filteredWorkerFiles.Count, workerFile.FileName);
                    while (true)
                    {
                        try
                        {
                            log.Add(new Tuple<ELogTypeEnum, string>(ELogTypeEnum.Info, $"Download {workerFile.Source} ({workerFile.FileDateTime:dd.MM.yy HH:mm:ss})"));
                            var localFile = workerFile.Destination;
                            if (!Directory.Exists(Path.GetDirectoryName(localFile)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(localFile) ?? string.Empty);
                            }
                            DownloadFile(workerFile.Source, localFile, workerFile.FileDateTime, workerFile.FileSize);
                            if (SetupHelper.CancelRequested) break;
                            DownloadSizeCompleted += workerFile.FileSize;
                            workerFile.Done = true;
                            currentFile++;
                            DownloadFilesCompleted++;
                            break;
                        }
                        catch (Exception)
                        {
                            if (trialNumber++ > 2)
                            {
                                foreach (var l in log) Log(l.Item1, l.Item2); log.Clear();
                                throw;
                            }
                        }
                    }
                });
                foreach (var l in log) Log(l.Item1, l.Item2); log.Clear();
                NotifyProgressChanged(DownloadSizeTotal, DownloadSizeTotal, filteredWorkerFiles.Count, filteredWorkerFiles.Count, string.Empty);

                return !SetupHelper.CancelRequested;
                #endregion
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(ELogTypeEnum.Error, ex.ToString());
                return false;
            }
        }
        private void DownloadFile(string fileName, string localFile, DateTime dateTime, long fileSize)
        {
            if (!Directory.Exists(Path.GetDirectoryName(localFile) ?? string.Empty)) Directory.CreateDirectory(Path.GetDirectoryName(localFile) ?? string.Empty);
            var baseUri = _downloadFileUri;
            var uri = new Uri(baseUri, DOWNLOAD_FILE_METHOD);
            var url = uri + "?updateChannel=" + UpdateChannel + "&fileName=" + Uri.EscapeDataString(fileName);

            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            //webRequest.AllowReadStreamBuffering = false;
            using (var response = webRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) throw new Exception("Fehler beim öffnen der URL: " + url);
                    using (var sw = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        sw.SetLength(fileSize); // Wichtig damit die Dateien auf nicht fragmentiert werden!
                        using (var gz = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            var buffer = new byte[STREAM_BUFFER_SIZE];
                            int read;
                            while ((read = gz.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                sw.Write(buffer, 0, read);
                                _inProgress += read;
                                NotifyProgressChanged(DownloadSizeCompleted + _inProgress, DownloadSizeTotal, DownloadFilesCompleted, DownloadFilesTotal, fileName);
                            }
                        }
                    }
                }
            }
            _inProgress -= fileSize;
            File.SetLastWriteTime(localFile, dateTime);
            if (localFile.EndsWith(RESET_FILE_TAG, StringComparison.InvariantCultureIgnoreCase))
            {
                File.Copy(localFile, localFile.Replace(RESET_FILE_TAG, string.Empty));
            }
        }

        public void DeleteClientIdOnServer()
        {
            try
            {
                var uri = SetupHelper.GetUpdateApi(ServiceUrl, UpdateChannel, RESET_CLIENT_ID_METHOD);

                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                try
                {
                    request.Headers.Add("ClientID", FingerPrint.ClientId);
                }
                catch
                {
                    // ignored
                }
                request.Headers.Add("ClientOSUsername", Environment.UserName + "@" + Environment.UserDomainName);
                request.Headers.Add("ClientHostname", Environment.MachineName);

                var result = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();
            }
            catch
            {
                // ignored
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
                var uri = SetupHelper.GetUpdateApi(ServiceUrl, UpdateChannel, UPLOAD_LOG_METHOD, true);
                if (uri == null) return false;

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

            var updateService = UpdateConf.UpdateFiles.Where(w => w.FilePath == "" && w.FileName.Equals("UpdateService.exe", StringComparison.OrdinalIgnoreCase));
            CompareDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "xbAV Beratungssoftware GmbH", "UpdateService"), updateService);

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

        private void NotifyProgressChanged(long position, long total, long currentFile, long totalFiles, string fileName) => ProgressChanged?.Invoke(this, new FractionalProgressChangedEventArgs(position, total, currentFile, totalFiles, fileName));

        public event EventHandler<FractionalProgressChangedEventArgs> ProgressChanged;

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
