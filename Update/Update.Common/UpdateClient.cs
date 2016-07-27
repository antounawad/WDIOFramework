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
using System.Xml.Serialization;
using Eulg.Shared;
using Eulg.Update.Shared;
using Microsoft.Win32;
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
            ClientIdFail
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

        internal const string REGISTRY_GROUP_NAME_OBSOLETE = @"Software\KS Software GmbH";
        internal const string REGISTRY_GROUP_NAME = @"Software\EULG Software GmbH";

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
                if (_updateUrl.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase)) { _updateUrl = _updateUrl.Substring(7); }
                if (_updateUrl.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase)) { _updateUrl = _updateUrl.Substring(8); }
                if (!_updateUrl.EndsWith("/")) { _updateUrl += "/"; }
            }
        }

        private string _updateUrl;
        private readonly WebClient _webClient = new WebClient();

        public UpdateClient()
        {
            _webClient.Encoding = Encoding.UTF8;
            _webClient.Headers["User-Agent"] = "UpdateClient";
            _webClient.Proxy = WebRequest.DefaultWebProxy;
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
        //public List<string> Plugins { get; private set; }

        // Progress indication
        public long DownloadSizeTotal { get; private set; }
        public long DownloadSizeCompleted { get; private set; }
        public long DownloadFilesTotal { get; private set; }
        public long DownloadFilesCompleted { get; private set; }
        public long DownloadCurrentFileSize { get; private set; }
        public long DownloadCurrentFileSizeGz { get; private set; }
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
                    Log(LogTypeEnum.Warning, "NoConnection");
                    return EUpdateCheckResult.NoConnection;
                }
                var baseUri = new Uri((UseHttps ? "https://" : "http://") + UpdateUrl);
                var uri = new Uri(baseUri, FETCH_UPDATE_DATA_METHOD);
                var url = uri + "?updateChannel=" + UpdateChannel + "&userName=" + Uri.EscapeDataString(UserNames.Length > 0 ? String.Join("\t", UserNames) : "") + "&password=" + Uri.EscapeDataString(Passwords.Length > 0 ? String.Join("\t", Passwords) : "");

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.AllowReadStreamBuffering = false;
                request.AllowWriteStreamBuffering = false;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                request.Headers.Add("ClientID", clientId);
                request.Headers.Add("ClientOSUsername", Environment.UserName + "@" + Environment.UserDomainName);
                request.Headers.Add("ClientHostname", Environment.MachineName);
                request.Headers.Add("ClientUpdateType", "UPDATE");

                var result = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();

                if (string.IsNullOrWhiteSpace(result) || result.StartsWith("CREATECACHE"))
                {
                    return EUpdateCheckResult.UpToDate;
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
                Log(LogTypeEnum.Error, UpdateUrl + ": " + ex.Message);
                return EUpdateCheckResult.NoService;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(LogTypeEnum.Error, UpdateUrl + ": " + ex.Message);
                return EUpdateCheckResult.Error;
            }
            return EUpdateCheckResult.UpdatesAvailable;
        }
        public EUpdateCheckResult CheckForUpdates(string clientId, bool autoDelete)
        {
            var start = DateTime.Now;
            try
            {
                var result = FetchManifest(clientId);
                if (result != EUpdateCheckResult.UpdatesAvailable)
                {
                    return result;
                }
            }
            finally
            {
                Log(LogTypeEnum.Info, $"Time to fetch manifest: {DateTime.Now - start}");
            }

            start = DateTime.Now;
            try
            {
                if (CompareFiles(autoDelete))
                {
                    Log(LogTypeEnum.Info, "UpdatesAvailable on " + UpdateUrl);
                    return EUpdateCheckResult.UpdatesAvailable;
                }
                return EUpdateCheckResult.UpToDate;
            }
            finally
            {
                Log(LogTypeEnum.Info, $"Time to compare files: {DateTime.Now - start}");
            }
        }

        public static bool CheckConnectivity()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        private void PatchFile(WorkerConfig.WorkerFile diffFile)
        {
            var webRequest = WebRequest.CreateHttp(new Uri(new Uri("http://" + UpdateUrl), "FilesUpdateGetDelta"));
            //webRequest.AllowReadStreamBuffering = false;
            //webRequest.AllowWriteStreamBuffering = false;
            webRequest.SendChunked = true;
            webRequest.Method = "POST";
            webRequest.Headers.Add("Channel", UpdateChannel.ToString());
            webRequest.Headers.Add("FileName", diffFile.Source);

            webRequest.ContentType = "application/octet-stream";
            //webRequest.ContentLength = signature.Length;

            using (var requestStream = webRequest.GetRequestStream())
            {
                using (var deflateStream = new DeflateStream(requestStream, CompressionLevel.Optimal))
                {
                    using (var sourceFileStream = new FileStream(diffFile.Destination, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var signatureBuilder = new SignatureBuilder { ChunkSize = diffChunkSize };
                        signatureBuilder.Build(sourceFileStream, new SignatureWriter(deflateStream));
                    }
                }
            }
            var destFile = Path.Combine(DownloadPath, diffFile.Source);
            using (var response = webRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var deflateStream = new DeflateStream(responseStream, CompressionMode.Decompress))
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
            File.SetLastWriteTime(destFile, diffFile.FileDateTime);
        }

        private const int diffThreshold = 256*1024;
        private const int diffChunkSize = 8192;

        public bool DownloadUpdates()
        {
            DownloadSizeTotal = WorkerConfig.WorkerFiles.Sum(s => s.FileSizeGz);
            DownloadFilesTotal = WorkerConfig.WorkerFiles.Count();
            DownloadSizeCompleted = 0;
            DownloadFilesCompleted = 0;
            try
            {
                if (!Directory.Exists(DownloadPath))
                {
                    Log(LogTypeEnum.Info, "Create Directory " + DownloadPath);
                    Directory.CreateDirectory(DownloadPath);
                }

                GetUpdateClient();

                foreach (var workerFile in WorkerConfig.WorkerFiles)
                {
                    var tmpFile = Path.Combine(DownloadPath, workerFile.Source);
                    DownloadCurrentFilename = workerFile.FileName;
                    DownloadCurrentFileSize = workerFile.FileSize;
                    DownloadCurrentFileSizeGz = workerFile.FileSizeGz;
                    NotifyProgressChanged(DownloadSizeCompleted, DownloadSizeTotal, DownloadFilesCompleted, DownloadFilesTotal, workerFile.FileName);

                    var fileInfo = new FileInfo(tmpFile);
                    if (!fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, workerFile.FileDateTime) || fileInfo.Length != workerFile.FileSize)
                    {
                        var retryCount = 0;
                        var downloadSuccess = false;
                        while (!downloadSuccess)
                        {
                            try
                            {
                                Log(LogTypeEnum.Info, $"Download {workerFile.Source} ({workerFile.FileDateTime:dd.MM.yy HH:mm:ss})");
                                DownloadFile(workerFile.Source, tmpFile, workerFile.FileDateTime, workerFile.FileSize, workerFile.FileSizeGz);
                                downloadSuccess = true;
                            }
                            catch (Exception)
                            {
                                retryCount++;
                                if (retryCount > 2)
                                {
                                    //if (MessageBox.Show("Fehler beim Download der Datei: " + workerFile.FileName + Environment.NewLine + Environment.NewLine + "Bitte überprüfen Sie ihre Internet-Verbindung. Nochmal versuchen?", "Fehler", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel)
                                    //{
                                    throw;
                                    //}
                                    //retryCount--;
                                }
                            }
                        }
                    }
                    DownloadSizeCompleted += workerFile.FileSizeGz;
                    DownloadFilesCompleted++;
                    NotifyProgressChanged(DownloadSizeCompleted, DownloadSizeTotal, DownloadFilesCompleted, DownloadFilesTotal, workerFile.FileName);
                }
                // Write Worker Config
                WorkerConfig.AppPath = ApplicationPath;
                WorkerConfig.TempPath = DownloadPath;
                WorkerConfig.ApplicationFile = SkipRestartApplication ? null : System.Reflection.Assembly.GetEntryAssembly().Location;
                WorkerConfig.CommandLineArgs = Tools.GetCommandLineArgs();
                WorkerConfig.StartProcess = ObsoleteRegistryKeysUsed() ? WorkerProcess.CreateFromProcessStartInfo(new ProcessStartInfo(Path.Combine(ApplicationPath, "Update.Registry.exe"))) : null;
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
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(LogTypeEnum.Error, ex.ToString());
                return false;
            }
        }
        public bool DownloadUpdatesStream()
        {
            try
            {
                var filteredWorkerFilesAll = WorkerConfig.WorkerFiles.Where(f =>
                   {
                       var fileInfo = new FileInfo(Path.Combine(DownloadPath, f.Source));
                       return !fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, f.FileDateTime) || fileInfo.Length != f.FileSize;
                   }).ToList();

                var diffFiles = filteredWorkerFilesAll.Where(w => !w.NewFile && w.FileSizeGz >= diffThreshold).ToList();
                var filteredWorkerFiles = filteredWorkerFilesAll.Where(w => w.NewFile || w.FileSizeGz < diffThreshold).ToList();

                if (!Directory.Exists(DownloadPath))
                {
                    Log(LogTypeEnum.Info, "Create Directory " + DownloadPath);
                    Directory.CreateDirectory(DownloadPath);
                }

                GetUpdateClient();

                //var DownloadSizeTotalAll = filteredWorkerFilesAll.Sum(s => s.FileSizeGz);
                DownloadSizeTotal = filteredWorkerFilesAll.Sum(s => s.FileSizeGz);
                DownloadFilesTotal = filteredWorkerFilesAll.Count;
                DownloadSizeCompleted = 0;
                DownloadFilesCompleted = 0;
                var currentFile = 0;
                var currentFileDiff = 0;

                //Parallel.ForEach(diffFiles, diffFile =>
                //{
                //    NotifyProgressChanged(DownloadSizeCompleted, DownloadSizeTotal, currentFileDiff, filteredWorkerFilesAll.Count, "DIFF:" + diffFile.FileName);
                //    PatchFile(diffFile);
                //    NotifyProgressChanged(DownloadSizeCompleted, DownloadSizeTotal, currentFileDiff, filteredWorkerFilesAll.Count, "DIFF:" + diffFile.FileName);
                //    DownloadSizeCompleted += diffFile.FileSizeGz;
                //    currentFileDiff++;
                //});

                foreach (var diffFile in diffFiles)
                {
                    NotifyProgressChanged(DownloadSizeCompleted, DownloadSizeTotal, currentFileDiff, filteredWorkerFilesAll.Count, diffFile.FileName);
                    Log(LogTypeEnum.Info, $"Diff/Patch {diffFile.Source} ({diffFile.FileDateTime:dd.MM.yy HH:mm:ss})");
                    PatchFile(diffFile);
                    DownloadSizeCompleted += diffFile.FileSizeGz;
                    currentFileDiff++;
                }

                if (filteredWorkerFiles.Count > 0)
                {
                    var baseUri = new Uri("http://" + UpdateUrl);
                    var uri = new Uri(baseUri, "FileUpdateGetFiles");

                    ServicePointManager.Expect100Continue = false;

                    var webRequest = WebRequest.CreateHttp(uri);
                    //webRequest.KeepAlive = false;
                    webRequest.AllowReadStreamBuffering = false;
                    //webRequest.AllowWriteStreamBuffering = false;
                    //webRequest.ProtocolVersion = HttpVersion.Version10;
                    webRequest.Method = "POST";
                    webRequest.Headers.Add("Channel", UpdateChannel.ToString());
                    webRequest.Headers.Add("ManifestTimestamp", $"{DateTime.Now:yyyyMMddHHmmss}");
                    webRequest.Headers.Add("CompressionType", "GZipHack");
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
                            while (currentFile < filteredWorkerFiles.Count)
                            {
                                var workerFile = filteredWorkerFiles[currentFile];
                                DownloadCurrentFilename = workerFile.FileName;
                                DownloadCurrentFileSize = workerFile.FileSize;
                                DownloadCurrentFileSizeGz = workerFile.FileSizeGz;
                                var localFile = Path.Combine(DownloadPath, workerFile.Source);
                                if (!Directory.Exists(Path.GetDirectoryName(localFile) ?? String.Empty))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(localFile) ?? String.Empty);
                                }

                                Log(LogTypeEnum.Info, $"Download {workerFile.Source} ({workerFile.FileDateTime:dd.MM.yy HH:mm:ss})");
                                var baseDownloadSize = DownloadSizeCompleted;
                                NotifyProgressChanged(DownloadSizeCompleted, DownloadSizeTotal, currentFile + currentFileDiff, filteredWorkerFilesAll.Count, workerFile.FileName);
                                using (var sw = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    using (var view = new SectionStream(responseStream, workerFile.FileSizeGz))
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
                                                NotifyProgressChanged(DownloadSizeCompleted, DownloadSizeTotal);
                                            }
                                        }

                                        DownloadSizeCompleted = baseDownloadSize + workerFile.FileSizeGz;
                                    }
                                }
                                File.SetLastWriteTime(localFile, workerFile.FileDateTime);
                                currentFile++;
                                DownloadFilesCompleted++;
                            }
                            responseStream?.Close();
                        }
                        response.Close();

                        NotifyProgressChanged(DownloadSizeTotal, DownloadSizeTotal, filteredWorkerFilesAll.Count, filteredWorkerFilesAll.Count, string.Empty);
                    }
                }
                // Write Worker Config
                WorkerConfig.AppPath = ApplicationPath;
                WorkerConfig.TempPath = DownloadPath;
                WorkerConfig.ApplicationFile = SkipRestartApplication ? null : System.Reflection.Assembly.GetEntryAssembly().Location;
                WorkerConfig.CommandLineArgs = Tools.GetCommandLineArgs();
                WorkerConfig.WaitForProcess = SkipWaitForProcess ? 0 : Process.GetCurrentProcess().Id;
                WorkerConfig.StartProcess = ObsoleteRegistryKeysUsed() ? WorkerProcess.CreateFromProcessStartInfo(new ProcessStartInfo(Path.Combine(ApplicationPath, "Update.Registry.exe"))) : null;
                WorkerConfig.CheckProcesses = CheckProcesses;
                WorkerConfig.KillProcesses = KillProcesses;
                WorkerConfig.LogFile = LogFile;

                var xmlSer = new XmlSerializer(typeof(WorkerConfig));
                using (var writer = new StreamWriter(Path.Combine(DownloadPath, UPDATE_WORKER_XML_FILE)))
                {
                    xmlSer.Serialize(writer, WorkerConfig);
                }
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(LogTypeEnum.Error, ex.ToString());
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
                Log(LogTypeEnum.Error, ex.ToString());
                return null;
            }
        }

        public enum LogTypeEnum { Info, Warning, Error }
        public void Log(LogTypeEnum logType, string message)
        {
            File.AppendAllText(LogFile, $"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {logType.ToString().PadRight(7)}: {"UpdateClient"}: {message.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", " ")}" + Environment.NewLine);
        }

        public bool UploadLogfile()
        {
            try
            {
                if (!File.Exists(LogFile)) return true;
                if (!CheckConnectivity()) return true;

                var baseUri = new Uri("http://" + UpdateUrl);
                var uri = new Uri(baseUri, UPLOAD_LOG_METHOD);

                var nvc = new NameValueCollection
                {
                    {"updateChannel", UpdateChannel.ToString()},
                    {"userName", (UserNames.Length>0 && !String.IsNullOrWhiteSpace(UserNames[0])) ? UserNames[0] : $"{Environment.UserName}@{Environment.MachineName}" },
                    {"logFileDateTime", $"{File.GetLastWriteTime(LogFile):dd.MM.yyyy HH:mm:ss}" },
                    {"logFileContent", CompressStringGz(File.ReadAllText(LogFile))}
                };
                _webClient.UploadValues(uri.AbsoluteUri, "POST", nvc);
                File.Delete(LogFile);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Log(LogTypeEnum.Error, ex.ToString());
                return false;
            }
        }

        private static bool ObsoleteRegistryKeysUsed()
        {
            var keysToCheck = new[]
                              {
                                  Registry.LocalMachine,
                                  Registry.CurrentUser
                              };

            foreach (var key in keysToCheck)
            {
                var obsoleteKey = key.OpenSubKey(REGISTRY_GROUP_NAME_OBSOLETE);
                var currentKey = key.OpenSubKey(REGISTRY_GROUP_NAME);

                if (obsoleteKey != null && (currentKey == null || currentKey.SubKeyCount == 0))
                {
                    if (obsoleteKey.GetValue("KeysMigrated") == null)
                        return true;
                }
            }

            return false;
        }

        private void DownloadFile(string fileName, string localFile, DateTime dateTime, long fileSize, long fileSizeGz)
        {
            if (!Directory.Exists(Path.GetDirectoryName(localFile) ?? String.Empty)) Directory.CreateDirectory(Path.GetDirectoryName(localFile) ?? String.Empty);
            var baseUri = new Uri("http://" + UpdateUrl);
            var uri = new Uri(baseUri, DOWNLOAD_FILE_METHOD);
            var url = uri + "?updateChannel=" + UpdateChannel + "&fileName=" + Uri.EscapeDataString(fileName);
            using (var sr = _webClient.OpenRead(url))
            {
                if (sr == null) throw new Exception("Fehler beim öffnen der URL: " + url);
                using (var sw = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var gz = new GZipStream(sr, CompressionMode.Decompress))
                    {
                        var buffer = new byte[STREAM_BUFFER_SIZE];
                        int read;
                        var total = 0;
                        while ((read = gz.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            sw.Write(buffer, 0, read);
                            total += read;
                            NotifyProgressChanged(DownloadSizeCompleted + (long)Math.Round((double)fileSizeGz / fileSize * total), DownloadSizeTotal);
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
            var updateFile = UpdateConf.UpdateFiles.FirstOrDefault(f => f.FilePath == String.Empty && f.FileName.Equals(UPDATE_WORKER_BIN_FILE, StringComparison.CurrentCultureIgnoreCase));
            if (updateFile != null)
            {
                var fileInfo = new FileInfo(binFileInTemp);
                if (!fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, updateFile.FileDateTime) || fileInfo.Length != updateFile.FileSize)
                {
                    Log(LogTypeEnum.Info, $"Download UpdateWorker {binFileInTemp} ({updateFile.FileDateTime:dd.MM.yy HH:mm:ss})");
                    DownloadFilesTotal++;
                    DownloadSizeTotal += updateFile.FileSizeGz;
                    DownloadCurrentFilename = updateFile.FileName;
                    DownloadCurrentFileSize = updateFile.FileSize;
                    DownloadCurrentFileSizeGz = updateFile.FileSizeGz;
                    NotifyProgressChanged(0, DownloadSizeTotal, 0, DownloadFilesTotal, updateFile.FileName);
                    DownloadFile(UPDATE_WORKER_BIN_FILE, binFileInTemp, updateFile.FileDateTime, updateFile.FileSize, updateFile.FileSizeGz);
                    DownloadSizeCompleted += updateFile.FileSizeGz;
                    DownloadFilesCompleted++;
                    NotifyProgressChanged(updateFile.FileSizeGz, DownloadSizeTotal, 1, DownloadFilesTotal, string.Empty);
                }
            }
        }

        private bool CompareFiles(bool autoDelete)
        {
            WorkerConfig.WorkerFiles.Clear();
            // AppDir
            CompareDirectory("AppDir", ApplicationPath, UpdateConf.UpdateFiles, autoDelete);
            // Support
            CompareDirectory("Support", Path.Combine(ApplicationPath, "Support"), UpdateConf.UpdateFiles, autoDelete);
            // Plugins
            CompareDirectory("Plugins", Path.Combine(ApplicationPath, "Plugins"), UpdateConf.UpdateFiles, autoDelete);
            // AppDir Deletes
            CompareDeletes(ApplicationPath, UpdateConf.UpdateDeletes.Where(w => w.FilePath == "AppDir"));
            // Support Deletes
            CompareDeletes(Path.Combine(ApplicationPath, "Support"), UpdateConf.UpdateDeletes.Where(w => w.FilePath == "Support"));
            // Plugin Deletes
            CompareDeletes(Path.Combine(ApplicationPath, "Plugins"), UpdateConf.UpdateDeletes.Where(w => w.FilePath == "Plugins"));

            return WorkerConfig.WorkerFiles.Any() || WorkerConfig.WorkerDeletes.Any();
        }
        private void CompareDirectory(string filePath, string path, IEnumerable<UpdateConfig.UpdateFile> updateFiles, bool autoDelete)
        {
            foreach (var updateFile in updateFiles.Where(f => f.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase)))
            {
                var localFile = Path.Combine(path, updateFile.FileName);
                CompareFile(updateFile, localFile);
            }

            if (!autoDelete)
            {
                return;
            }

            //HACK Temporäre Implementierung, diesen Code nicht übernehmen wenn das Update neu geschrieben wird!
            if (filePath.Equals("Plugins", StringComparison.InvariantCultureIgnoreCase))
            {
                // Sonderbehandlung für FilePath=Plugins: Ausschließlich Pluginverzeichnisse von Plugins aufräumen, die auch ausgeliefert werden
                var pluginPaths = updateFiles.Where(f => f.FileName.Length == 17 && f.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase) && f.FileName.EndsWith("plugin.xml", StringComparison.InvariantCultureIgnoreCase))
                                             .Select(f => f.FileName.Substring(0, 6)).ToList();

                foreach (var pluginPath in pluginPaths.Where(p => Directory.Exists(Path.Combine(path, p))))
                {
                    foreach (var absoluteFile in Directory.EnumerateFiles(Path.Combine(path, pluginPath), "*.*", SearchOption.TopDirectoryOnly))
                    {
                        var filename = absoluteFile.Substring(path.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        if (updateFiles.Any(f => f.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase) && f.FileName.Equals(filename, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            continue;
                        }

                        WorkerConfig.WorkerDeletes.Add(new WorkerConfig.WorkerDelete { Path = absoluteFile });
                    }
                }
            }
            else if (Directory.Exists(path) && filePath.Equals("AppDir", StringComparison.InvariantCultureIgnoreCase))
            {
                var addFolder = new[]
                                {
                                    "Demo\\Web\\bin",
                                    "Demo\\Web\\Images",
                                    "Demo\\Web\\Scripts",
                                    "Demo\\Web\\Views\\Shared",
                                    "Demo\\Web\\Views\\Vp",
                                    "Demo\\Web\\Views\\Vp\\ChangeForm"
                                };
                var filesToCheck = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);

                foreach (var folderPath in addFolder)
                {
                    var concatPath = Path.Combine(path, folderPath);
                    if (Directory.Exists(concatPath))
                    {
                        filesToCheck = filesToCheck.Concat(Directory.EnumerateFiles(concatPath, "*.*", SearchOption.TopDirectoryOnly));
                    }
                }

                foreach (var absoluteFile in filesToCheck)
                {
                    var filename = absoluteFile.Substring(path.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (((filename.Equals("branding.xml", StringComparison.InvariantCultureIgnoreCase) || filename.Equals("updateworker.exe", StringComparison.InvariantCultureIgnoreCase) || filename.Equals("updateservice.exe", StringComparison.InvariantCultureIgnoreCase)))
                        || (updateFiles.Any(f => f.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase) && f.FileName.Equals(filename, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        continue;
                    }

                    WorkerConfig.WorkerDeletes.Add(new WorkerConfig.WorkerDelete { Path = absoluteFile });
                }
            }
        }
        private void CompareFile(UpdateConfig.UpdateFile updateFile, string localFile)
        {
            // Wenn Branding.xml lokal schon vorhanden - auf keinen fall überschreiben!!!
            if (updateFile.FilePath.Equals("AppDir", StringComparison.CurrentCultureIgnoreCase) && updateFile.FileName.Equals("Branding.xml", StringComparison.CurrentCultureIgnoreCase) && File.Exists(localFile)) return;

            var fileInfo = new FileInfo(localFile);
            if (!fileInfo.Exists || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, updateFile.FileDateTime) || fileInfo.Length != updateFile.FileSize)
            {
                //Log(LogTypeEnum.Info, updateFile.FileName + " " + updateFile.FileSize);
                WorkerConfig.WorkerFiles.Add(new WorkerConfig.WorkerFile
                {
                    Source = Path.Combine(updateFile.FilePath, updateFile.FileName),
                    Destination = localFile,
                    FileDateTime = updateFile.FileDateTime,
                    FileSize = updateFile.FileSize,
                    FileSizeGz = updateFile.FileSizeGz,
                    FileName = updateFile.FileName
                });
                //if (updateFile.FileName.EndsWith(ResetFileTag, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    _workerConfig.WorkerFiles.Add(new WorkerConfig.WorkerFile
                //    {
                //        Source = Path.Combine(updateFile.FilePath, updateFile.FileName),
                //        Destination = localFile.Substring(0, localFile.Length - ResetFileTag.Length),
                //        FileDateTime = updateFile.FileDateTime,
                //        FileSize = updateFile.FileSize,
                //        FileSizeGz = updateFile.FileSizeGz,
                //        FileName = updateFile.FileName
                //    });
                //}
            }
        }
        private void CompareDeletes(string path, IEnumerable<UpdateConfig.UpdateDelete> updateDeletes)
        {
            var deletePaths = new HashSet<string>(WorkerConfig.WorkerDeletes.Select(c => c.Path), StringComparer.InvariantCultureIgnoreCase);

            foreach (var updateDelete in updateDeletes)
            {
                var localPath = Path.Combine(path, updateDelete.FileName);
                if (File.Exists(localPath) || Directory.Exists(localPath))
                {
                    deletePaths.Add(localPath);
                }
            }

            WorkerConfig.WorkerDeletes.Clear();
            WorkerConfig.WorkerDeletes.AddRange(deletePaths.Select(p => new WorkerConfig.WorkerDelete { Path = p }));
        }

        private void NotifyProgressChanged(long position, long total)
        {
            ProgressChanged?.Invoke(this, new FractionalProgressChangedEventArgs(position, total));
        }

        private void NotifyProgressChanged(long position, long total, long currentFile, long totalFiles, string fileName)
        {
            ProgressChanged?.Invoke(this, new FractionalProgressChangedEventArgs(position, total, currentFile, totalFiles, fileName));
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
