using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Eulg.Shared;
using Eulg.Update.Shared;
using Octodiff.Core;

namespace Eulg.Update.Common
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
            ServiceBusy
        }
        public enum ELogTypeEnum
        {
            Info,
            Warning,
            Error
        }

        private const string UPDATE_WORKER_BIN_FILE = "UpdateWorker.exe";
        private const string UPDATE_WORKER_XML_FILE = "UpdateWorker.xml";
        public const string RESET_FILE_TAG = ".$EulgReset$";
        private const string FETCH_UPDATE_DATA_METHOD = "FilesUpdateCheck";
        private const string DOWNLOAD_FILE_METHOD = "FilesUpdateGetFileGz";
        private const string DOWNLOAD_FILES_STREAM_METHOD = "FileUpdateGetFiles";
        private const string UPLOAD_LOG_METHOD = "FilesUpdateUploadLog";
        private const string RESET_CLIENT_ID_METHOD = "FilesUpdateResetClientId";
        private const int STREAM_BUFFER_SIZE = 81920;
        private const int diffThreshold = 256 * 1024;
        private const int diffChunkSize = 8192;

        public bool UseHttps { get; set; }
        public UpdateConfig UpdateConf = new UpdateConfig();
        public readonly WorkerConfig WorkerConfig = new WorkerConfig();
        public Branding.EUpdateChannel UpdateChannel { get; set; }
        public string ApplicationPath { get; set; }
        public string LastError { get; private set; }
        public string UpdateUrl
        {
            get => _updateUrl;
            set
            {
                _updateUrl = value;
                if (_updateUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                {
                    _updateUrl = _updateUrl.Substring(7);
                    UseHttps = false; //HACK Workaround um größere Anpassungen zu vermeiden
                }
                if (_updateUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    _updateUrl = _updateUrl.Substring(8);
                    UseHttps = true; //HACK Workaround um größere Anpassungen zu vermeiden
                }
                if (!_updateUrl.EndsWith("/", StringComparison.InvariantCultureIgnoreCase)) { _updateUrl += "/"; }
            }
        }

        private string _updateUrl;
        private readonly bool _forceNoShortcuts;

        /// <summary>
        /// <paramref name="forceNoShortcuts"/> erzwingt eine vollständige Prüfung aller Dateien ohne Optimierungen und Abkürzungen.
        /// </summary>
        public UpdateClient(bool forceNoShortcuts = false)
        {
            _forceNoShortcuts = forceNoShortcuts;

            UseHttps = true;
            ApplicationPath = AppDomain.CurrentDomain.BaseDirectory;
            LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Update.log");
        }

        public string DownloadPath { get; set; }
        public string CheckProcesses { get; set; }
        public string KillProcesses { get; set; }
        public string LogFile { get; set; }

        public string[] UserNames { get; set; }
        public string[] Passwords { get; set; }

        // Progress indication
        public long DownloadSizeTotal;
        public long DownloadSizeTotalGz;
        public long DownloadSizeCompleted;
        public long DownloadFilesTotal;
        public long DownloadFilesCompleted;
        public long DownloadCurrentFileSize;
        public long DownloadCurrentFileSizeGz;
        public string DownloadCurrentFilename { get; private set; }
        public bool SkipWaitForProcess { get; set; }
        public bool SkipRestartApplication { get; set; }

        public DateTime? ClientIdUsedDateTime { get; private set; }
        public string ClientIdUsedOsUsername { get; private set; }
        public string ClientIdUsedHostname { get; private set; }

        public EUpdateCheckResult FetchManifest(string clientId)
        {
            try
            {
                if (!CheckConnectivity())
                {
                    Log(ELogTypeEnum.Warning, "NoConnection");
                    return EUpdateCheckResult.NoConnection;
                }
                var baseUri = new Uri((UseHttps ? "https://" : "http://") + UpdateUrl);
                var uri = new Uri(baseUri, FETCH_UPDATE_DATA_METHOD);
                var url = uri + "?updateChannel=" + UpdateChannel + "&userName=" + Uri.EscapeDataString(UserNames.Length > 0 ? string.Join("\t", UserNames) : "") + "&password=" + Uri.EscapeDataString(Passwords.Length > 0 ? string.Join("\t", Passwords) : "");

                var request = WebRequest.CreateHttp(url);
                request.KeepAlive = false;
                request.AllowReadStreamBuffering = false;
                request.AllowWriteStreamBuffering = false;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                if (!string.IsNullOrEmpty(clientId)) request.Headers.Add("ClientID", clientId);
                request.Headers.Add("ClientOSUsername", Environment.UserName + "@" + Environment.UserDomainName);
                request.Headers.Add("ClientHostname", Environment.MachineName);
                request.Headers.Add("ClientUpdateType", "UPDATE");

                var result = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

                if (string.IsNullOrWhiteSpace(result) || result.StartsWith("CREATECACHE", StringComparison.Ordinal))
                {
                    return _forceNoShortcuts ? EUpdateCheckResult.ServiceBusy : EUpdateCheckResult.UpToDate;
                }
                if (result.StartsWith("AUTHFAIL", StringComparison.Ordinal) || result.StartsWith("INITIALPASSWORDNOTSET", StringComparison.Ordinal))
                {
                    return EUpdateCheckResult.AuthFail;
                }
                if (result.StartsWith("CLIENTIDFAIL", StringComparison.Ordinal))
                {
                    var used = result.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (used.Length > 3)
                    {
                        DateTime dt;
                        if (DateTime.TryParse(used[1], out dt)) ClientIdUsedDateTime = dt;
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
            }
            catch (WebException ex)
            {
                LastError = ex.Message;
                Log(ELogTypeEnum.Error, UpdateUrl + ": " + ex.Message);
                return EUpdateCheckResult.NoService;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(ELogTypeEnum.Error, UpdateUrl + ": " + ex.Message);
                return EUpdateCheckResult.Error;
            }
            return EUpdateCheckResult.UpdatesAvailable;
        }

        public static bool CheckConnectivity()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        private void PatchFile(WorkerConfig.WorkerFile diffFile, out long signatureSize, out long deltaSize)
        {
            var webRequest = WebRequest.CreateHttp(new Uri(new Uri((UseHttps ? "https://" : "http://") + UpdateUrl), "FilesUpdateGetDelta"));
            webRequest.SendChunked = true;
            webRequest.Method = "POST";
            webRequest.Headers.Add("Channel", UpdateChannel.ToString());
            webRequest.Headers.Add("FileName", diffFile.Source);
            webRequest.Headers.Add("GZip", "true");
            webRequest.ContentType = "application/octet-stream";

            using (var requestStream = webRequest.GetRequestStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var deflateStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
                    {
                        using (var sourceFileStream = new FileStream(diffFile.Destination, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var signatureBuilder = new SignatureBuilder { ChunkSize = diffChunkSize };
                            signatureBuilder.Build(sourceFileStream, new SignatureWriter(deflateStream));
                        }
                    }
                    var ms = memoryStream.ToArray();
                    requestStream.Write(ms, 0, ms.Length);
                    signatureSize = ms.Length;
                }
            }
            var destFile = Path.Combine(DownloadPath, diffFile.Source);
            using (var response = webRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        responseStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        deltaSize = memoryStream.Length;
                        using (var deflateStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                        {
                            using (var basisStream = new FileStream(diffFile.Destination, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(destFile))) Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                                using (var newFileStream = new FileStream(Path.Combine(DownloadPath, diffFile.Source), FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                                {
                                    var delta = new DeltaApplier { SkipHashCheck = false };
                                    delta.Apply(basisStream, new BinaryDeltaReader(deflateStream, null), newFileStream);
                                }
                            }
                        }
                    }
                }
            }
            File.SetLastWriteTime(destFile, diffFile.FileDateTime);
        }

        public bool DownloadUpdates(string startAppAfterUpdate = null, string appAfterUpdateCmdLine = null)
        {
            try
            {
                #region Outer Try/Catch

                var filteredWorkerFilesAll = WorkerConfig.WorkerFiles.Where(f =>
                {
                    var fileInfo = new FileInfo(Path.Combine(DownloadPath, f.Source));
                    return !fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, f.FileDateTime) || fileInfo.Length != f.FileSize;
                }).ToList();
                var diffFiles = filteredWorkerFilesAll.Where(w => !w.NewFile && w.FileSizeGz >= diffThreshold).ToList();
                var filteredWorkerFiles = filteredWorkerFilesAll.Where(w => w.NewFile || w.FileSizeGz < diffThreshold).ToList();

                DownloadSizeTotal = filteredWorkerFilesAll.Sum(s => s.FileSize);
                DownloadSizeTotalGz = filteredWorkerFilesAll.Sum(s => s.FileSizeGz);
                DownloadFilesTotal = filteredWorkerFilesAll.Count;
                DownloadSizeCompleted = 0;
                DownloadFilesCompleted = 0;
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                if (!Directory.Exists(DownloadPath))
                {
                    Log(ELogTypeEnum.Info, "Create Directory " + DownloadPath);
                    Directory.CreateDirectory(DownloadPath);
                }

                GetUpdateClient();

                Parallel.ForEach(diffFiles, new ParallelOptions { MaxDegreeOfParallelism = 4 }, diffFile =>
                {
                    var trialNumber = 0;
                    NotifyProgressChanged(DownloadSizeCompleted, DownloadFilesCompleted, diffFile.FileName);
                    while (true)
                    {
                        try
                        {
                            Log(ELogTypeEnum.Info, $"Diff/Patch {diffFile.Source} ({diffFile.FileDateTime:dd.MM.yy HH:mm:ss}, {diffFile.FileSizeGz:N0} -> {diffFile.FileSize:N0}) {(trialNumber > 0 ? $"Trial: {trialNumber + 1}" : "")}");
                            long signatureSize;
                            long deltaSize;
                            PatchFile(diffFile, out signatureSize, out deltaSize);
#if DEBUG
                            Log(ELogTypeEnum.Info, $"{(signatureSize + deltaSize):N0} = {(100 * (signatureSize + deltaSize)) / diffFile.FileSizeGz}% ({signatureSize:N0} + {deltaSize:N0})");
#endif
                            diffFile.Done = true;
                            Interlocked.Add(ref DownloadFilesCompleted, 1);
                            break;
                        }
                        catch (Exception exception)
                        {
                            Log(ELogTypeEnum.Warning, exception.GetMessagesTree());
                            if (trialNumber++ > 2)
                            {
                                throw;
                            }
                        }
                    }
                });

                Parallel.ForEach(filteredWorkerFiles, new ParallelOptions { MaxDegreeOfParallelism = 4 }, workerFile =>
                {
                    var trialNumber = 0;
                    var tmpFile = Path.Combine(DownloadPath, workerFile.Source);
                    DownloadCurrentFilename = workerFile.FileName;
                    DownloadCurrentFileSize = workerFile.FileSize;
                    DownloadCurrentFileSizeGz = workerFile.FileSizeGz;
                    NotifyProgressChanged(DownloadSizeCompleted, DownloadFilesCompleted, workerFile.FileName);

                    var fileInfo = new FileInfo(tmpFile);
                    if (!fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, workerFile.FileDateTime) || fileInfo.Length != workerFile.FileSize)
                    {
                        while (true)
                        {
                            try
                            {
                                Log(ELogTypeEnum.Info, $"Download {workerFile.Source} ({workerFile.FileDateTime:dd.MM.yy HH:mm:ss}, {workerFile.FileSizeGz:N0} -> {workerFile.FileSize:N0}) {(trialNumber > 0 ? $"Trial: {trialNumber + 1}" : "")}");
                                DownloadFile(workerFile.Source, Path.Combine(DownloadPath, workerFile.Source), workerFile.FileDateTime, workerFile.FileSize, workerFile.Checksum);
                                workerFile.Done = true;
                                Interlocked.Add(ref DownloadFilesCompleted, 1);
                                break;
                            }
                            catch (Exception exception)
                            {
                                Log(ELogTypeEnum.Warning, exception.GetMessagesTree());
                                if (trialNumber++ > 2)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                });
                stopWatch.Stop();
                if (stopWatch.Elapsed.TotalSeconds > 0) Log(ELogTypeEnum.Info, $"{((DownloadSizeTotalGz / 1024.0) / stopWatch.Elapsed.TotalSeconds):N0} kB/s");

                NotifyProgressChanged(DownloadSizeTotal, filteredWorkerFilesAll.Count, string.Empty);

                // Write Worker Config
                WorkerConfig.AppPath = ApplicationPath;
                WorkerConfig.TempPath = DownloadPath;
                if (startAppAfterUpdate == null && appAfterUpdateCmdLine == null)
                {
                    WorkerConfig.ApplicationFile = SkipRestartApplication ? null : System.Reflection.Assembly.GetEntryAssembly().Location;
                    WorkerConfig.CommandLineArgs = Tools.GetCommandLineArgs();
                }
                else
                {
                    WorkerConfig.ApplicationFile = startAppAfterUpdate;
                    WorkerConfig.CommandLineArgs = appAfterUpdateCmdLine;
                }
                WorkerConfig.WaitForProcess = SkipWaitForProcess ? 0 : Process.GetCurrentProcess().Id;
                WorkerConfig.CheckProcesses = CheckProcesses;
                WorkerConfig.KillProcesses = KillProcesses;
                WorkerConfig.LogFile = LogFile;

                var xmlSer = new XmlSerializer(typeof(WorkerConfig));
                using (var writer = new StreamWriter(Path.Combine(DownloadPath, UPDATE_WORKER_XML_FILE)))
                {
                    xmlSer.Serialize(writer, WorkerConfig);
                }
                return true;
                #endregion
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                Log(ELogTypeEnum.Error, exception.GetMessagesTree());
                return false;
            }

        }

        public Process RunUpdateWorker(bool runAs = false)
        {
            try
            {
                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = Path.Combine(DownloadPath, UPDATE_WORKER_BIN_FILE),
                        Verb = runAs ? "runas" : null
                    }
                };
                p.Start();
                return p;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(ELogTypeEnum.Error, ex.ToString());
                return null;
            }
        }

        private readonly object _logFileLock = new object();
        public void Log(ELogTypeEnum logType, string message)
        {
            lock (_logFileLock)
            {
                File.AppendAllText(LogFile, $"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {logType.ToString().PadRight(7)}: UpdateClient: {message.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", " ")}" + Environment.NewLine);
            }
        }

        public bool UploadLogfile()
        {
            try
            {
                if (!File.Exists(LogFile)) return true;
                if (!CheckConnectivity()) return true;

                var baseUri = new Uri((UseHttps ? "https://" : "http://") + UpdateUrl);
                var uri = new Uri(baseUri, UPLOAD_LOG_METHOD);

                var nvc = new NameValueCollection
                {
                    {"updateChannel", UpdateChannel.ToString()},
                    {"userName", UserNames.Length>0 && !string.IsNullOrWhiteSpace(UserNames[0]) ? UserNames[0] : $"{Environment.UserName}@{Environment.MachineName}" },
                    {"logFileDateTime", $"{File.GetLastWriteTime(LogFile):dd.MM.yyyy HH:mm:ss}" },
                    {"logFileContent", CompressStringGz(File.ReadAllText(LogFile))}
                };
                var parameters = new StringBuilder();
                foreach (string key in nvc.Keys)
                {
                    parameters.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]));
                }
                parameters.Length -= 1;

                var webRequest = WebRequest.CreateHttp(uri);
                webRequest.Method = "POST";
                using (var requestStream = webRequest.GetRequestStream())
                {
                    using (var deflateStream = new GZipStream(requestStream, CompressionLevel.Optimal))
                    {
                        using (var writer = new StreamWriter(deflateStream))
                        {
                            writer.Write(parameters.ToString());
                        }
                    }
                }
                File.Delete(LogFile);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(ELogTypeEnum.Error, ex.ToString());
                return false;
            }
        }

        private void DownloadFile(string fileName, string localFile, DateTime dateTime, long fileSize, string checksum)
        {
            if (!Directory.Exists(Path.GetDirectoryName(localFile) ?? string.Empty)) Directory.CreateDirectory(Path.GetDirectoryName(localFile) ?? string.Empty);
            var baseUri = new Uri((UseHttps ? "https://" : "http://") + UpdateUrl);
            var uri = new Uri(baseUri, DOWNLOAD_FILE_METHOD);
            var url = uri + "?updateChannel=" + UpdateChannel + "&fileName=" + Uri.EscapeDataString(fileName);
            if (!string.IsNullOrWhiteSpace(checksum)) url = url + "&v=" + Uri.EscapeDataString(checksum);

            var webRequest = WebRequest.CreateHttp(url);
            webRequest.AllowReadStreamBuffering = false;
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
                            var total = 0;
                            try
                            {
                                int read;
                                while ((read = gz.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    sw.Write(buffer, 0, read);
                                    total += read;
                                    Interlocked.Add(ref DownloadSizeCompleted, read);
                                    NotifyProgressChanged(DownloadSizeCompleted, DownloadFilesCompleted, fileName);
                                }
                            }
                            catch
                            {
                                Interlocked.Add(ref DownloadSizeCompleted, -total);
                                throw;
                            }
                        }
                    }
                }
            }
            File.SetLastWriteTime(localFile, dateTime);
        }

        private void GetUpdateClient()
        {
            var binFileInAppPath = Path.Combine(ApplicationPath, UPDATE_WORKER_BIN_FILE);
            var binFileInTemp = Path.Combine(DownloadPath, UPDATE_WORKER_BIN_FILE);
            if (File.Exists(binFileInAppPath) && !File.Exists(binFileInTemp))
            {
                File.Copy(binFileInAppPath, binFileInTemp);
            }
            var updateFile = UpdateConf.UpdateFiles.FirstOrDefault(f => f.FilePath == string.Empty && f.FileName.Equals(UPDATE_WORKER_BIN_FILE, StringComparison.InvariantCultureIgnoreCase));
            if (updateFile != null)
            {
                var fileInfo = new FileInfo(binFileInTemp);
                if (!fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, updateFile.FileDateTime) || fileInfo.Length != updateFile.FileSize)
                {
                    Log(ELogTypeEnum.Info, $"Download UpdateWorker {binFileInTemp} ({updateFile.FileDateTime:dd.MM.yy HH:mm:ss})");
                    DownloadFilesTotal++;
                    DownloadSizeTotal += updateFile.FileSizeGz;
                    DownloadCurrentFilename = updateFile.FileName;
                    DownloadCurrentFileSize = updateFile.FileSize;
                    DownloadCurrentFileSizeGz = updateFile.FileSizeGz;
                    NotifyProgressChanged(0, 0, updateFile.FileName);
                    DownloadFile(UPDATE_WORKER_BIN_FILE, binFileInTemp, updateFile.FileDateTime, updateFile.FileSize, updateFile.CheckSum);
                    Interlocked.Add(ref DownloadFilesCompleted, 1);
                    NotifyProgressChanged(DownloadSizeCompleted, 1, string.Empty);
                }
            }
        }

        private void NotifyProgressChanged(long downloadSizeCompleted, long downloadFilesCompleted, string currentFilename)
        {
            ProgressChanged?.Invoke(this, new FractionalProgressChangedEventArgs(downloadSizeCompleted, DownloadSizeTotal, downloadFilesCompleted, DownloadFilesTotal, currentFilename));
        }

        public event EventHandler<FractionalProgressChangedEventArgs> ProgressChanged;

        private static string CompressStringGz(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionLevel.Optimal))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }
        private static byte[] CompressStringDeflate(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(mso, CompressionLevel.Optimal))
                {
                    msi.CopyTo(gs);
                }
                return mso.ToArray();
            }
        }

    }
}
