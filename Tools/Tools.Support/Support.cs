﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Eulg.Shared;
using Eulg.Update.Common;
using Eulg.Update.Shared;

namespace Eulg.Client.SupportTool
{
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }));
        }
    }

    public class LogEntry : PropertyChangedBase
    {
        public DateTime Time { get; set; }
        public string Severity { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string LogFilename { get; set; }
        public Support.ELogType LogType { get; set; }
        public string Raw { get; set; }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }

    public class Support
    {
        public enum ELogType
        {
            Client,
            Sync
        }

        public const string UPDATE_SERVICE_NAME = "EulgUpdate";
        public const string FERNWARTUNG_EXECUTABLE_NAME = "EulgFernwartung.exe";
        private const int STREAM_BUFFER_SIZE = 81920;
        private static string _ildasmPath;
        private long _sizeProcessed;
        private long _filesProcessed;
        private string _oldMessage;
        private int _oldPercent;

        public static Branding CurrentBranding { get; set; }

        public string DataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CurrentBranding.FileSystem.DataDir);

        public event ProgressChangedEventHandler ProgressChanged;

        public static void Init()
        {
            // Read Branding
            var path = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            if (!File.Exists(Path.Combine(path, "Branding.xml")))
            {
                path = Path.GetFullPath(Path.Combine(path, ".."));
            }
            if (!File.Exists(Path.Combine(path, "Branding.xml")))
            {
                throw new Exception("Branding nicht gefunden!");
            }
            Branding.Current = CurrentBranding = Branding.Read(Path.Combine(path, "Branding.xml"));
        }

        public static void RunFernwartung()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FERNWARTUNG_EXECUTABLE_NAME);
            var p = new Process { StartInfo = { FileName = file } };
            p.Start();
        }

        private static IEnumerable<string> WriteSafeReadAllLines(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        yield return sr.ReadLine();
                    }
                }
            }
        }

        #region Update

        public void DoUpdateCheck()
        {
            // ReSharper disable once RedundantAssignment
            var appPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
#if DEBUG
            appPath = @"C:\Program Files (x86)\EulgDeTest";
#endif
            ProxyConfig.SetDefault();
            var updateClient = new UpdateClient
            {
                UpdateUrl = CurrentBranding.Urls.Update,
                UseHttps = CurrentBranding.Update.UseHttps,
                UpdateChannel = CurrentBranding.Update.Channel,
                ApplicationPath = appPath,
                DownloadPath = Path.Combine(Path.GetTempPath(), "EulgSupportUpdate"),
                LogFile = Path.Combine(Path.GetTempPath(), "EulgSupportUpdate.log"),
                UserNames = new[] { "" },
                Passwords = new[] { "" },
                CheckProcesses = string.Empty, // AppBinary bei SupportTool auch nach KILL, siehe EULG-6189
                KillProcesses = Path.GetFileNameWithoutExtension(CurrentBranding.FileSystem.AppBinary) + ";" + Path.GetFileNameWithoutExtension(CurrentBranding.FileSystem.SyncBinary) + ";server", // "server.exe" is Allianz-RK background process
                SkipWaitForProcess = true,
                SkipRestartApplication = true,
            };
            updateClient.ProgressChanged += (sender, args) =>
            {
                NotifyProgressChanged((int)Math.Floor(args.Progress * 100), (args.CurrentItem ?? string.Empty));
            };
            NotifyProgressChanged(-1, "Update-Katalog abrufen...");
            switch (updateClient.FetchManifest(FingerPrint.ClientId))
            {
                case UpdateClient.EUpdateCheckResult.UpdatesAvailable:
                    break;

                case UpdateClient.EUpdateCheckResult.NoConnection:
                    MessageBox.Show("Es konnte keine Verbindung zum Update-Server hergestellt werden." + Environment.NewLine + updateClient.LastError, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                case UpdateClient.EUpdateCheckResult.UpToDate:
                    MessageBox.Show("Update-Konfiguration enthält keine Dateien." + Environment.NewLine + updateClient.LastError, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    // Result empty or CreateCache im Gange
                    return;
                case UpdateClient.EUpdateCheckResult.AuthFail:
                    MessageBox.Show("Der Update-Server hat die Zugangsdaten abgelehnt." + Environment.NewLine + updateClient.LastError, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                case UpdateClient.EUpdateCheckResult.ClientIdFail:
                    MessageBox.Show("Der Update-Server hat die Zugangsdaten abgelehnt." + Environment.NewLine + updateClient.LastError, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                case UpdateClient.EUpdateCheckResult.NoService:
                    MessageBox.Show("Der Update-Server-Dienst ist z. Zt. nicht aktiviert. Bitte versuchen Sie es später nochmal." + Environment.NewLine + updateClient.LastError, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                case UpdateClient.EUpdateCheckResult.Error:
                    MessageBox.Show("Fehler beim Abrufen der Update-Konfiguration: " + updateClient.LastError, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
            }

            if (!updateClient.UpdateConf.UpdateFiles.Any() && updateClient.UpdateConf.UpdateDeletes.Any())
            {
                MessageBox.Show("Update-Konfiguration enthält keine Dateien.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            _ildasmPath = Path.Combine(Path.GetTempPath(), "ildasm.exe");
            File.WriteAllBytes(_ildasmPath, Disassembler.Ildasm);

            CompareDirectory(updateClient.ApplicationPath, "AppDir", updateClient.UpdateConf.UpdateFiles.Where(w => w.FilePath == "AppDir").ToArray(), updateClient);
            CompareDirectory(Path.Combine(updateClient.ApplicationPath, "Support"), "Support", updateClient.UpdateConf.UpdateFiles.Where(w => w.FilePath == "Support").ToArray(), updateClient);
            if (updateClient.WorkerConfig.WorkerFiles.Any() || updateClient.WorkerConfig.WorkerDeletes.Any())
            {
                var updateSupport = (updateClient.WorkerConfig.WorkerFiles.Any(a => a.Source.StartsWith(@"Support\", StringComparison.InvariantCultureIgnoreCase)));
                if (updateSupport)
                {
                    updateClient.SkipWaitForProcess = false;
                    updateClient.SkipRestartApplication = false;
                }
                if (!Directory.Exists(updateClient.DownloadPath))
                {
                    Directory.CreateDirectory(updateClient.DownloadPath);
                }
                NotifyProgressChanged(-1, "Programmdateien herunterladen...");
                if (!updateClient.DownloadUpdatesStream())
                {
                    MessageBox.Show("Fehler beim Download der Programmdateien!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    NotifyProgressChanged(-1, "Protokoll übertragen...");
                    updateClient.UploadLogfile();
                    return;
                }
                var workerProcess = updateClient.RunUpdateWorker(!CheckUpdateService());
                if (updateSupport)
                {
                    Environment.Exit(0);
                }
                else
                {
                    workerProcess?.WaitForExit();
                    MessageBox.Show("Programmdateien wurden aktualisiert!", "Prüfung", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Alle Programmdateien sind aktuell!", "Prüfung", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            NotifyProgressChanged(-1, "Protokoll übertragen...");
            updateClient.UploadLogfile();
        }

        private void CompareDirectory(string path, string filePath, UpdateConfig.UpdateFile[] updateFiles, UpdateClient updateClient)
        {
            // Delete Extra Files
            NotifyProgressChanged(-1, "Verzeichnis durchsuchen..");
            var localFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            var filesTotal = localFiles.LongLength;
            if (filesTotal == 0)
                filesTotal = 1;
            foreach (var file in localFiles)
            {
                System.Threading.Interlocked.Add(ref _filesProcessed, 1);
                NotifyProgressChanged(Convert.ToInt32((Convert.ToDecimal(_filesProcessed) / Convert.ToDecimal(filesTotal)) * 100), file);
                var fileName = Path.GetFileName(file) ?? string.Empty;
                var relPath = file.Substring(path.Length + 1);
                if (filePath.Equals("AppDir", StringComparison.InvariantCultureIgnoreCase)
                    && (relPath.StartsWith("Setup\\", StringComparison.InvariantCultureIgnoreCase)
                        || relPath.StartsWith("Support\\", StringComparison.InvariantCultureIgnoreCase)
                        || relPath.StartsWith("Plugins\\", StringComparison.InvariantCultureIgnoreCase)
                        || fileName.Equals("Branding.xml", StringComparison.InvariantCultureIgnoreCase)
                        || fileName.Equals("UpdateWorker.exe", StringComparison.InvariantCultureIgnoreCase)
                       ))
                {
                    continue;
                }
                if (!updateFiles.Any(_ => _.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase)
                                          && _.FileName.Equals(relPath, StringComparison.InvariantCultureIgnoreCase)))
                {
                    updateClient.WorkerConfig.WorkerDeletes.Add(new WorkerConfig.WorkerDelete { Path = file });
                    updateClient.Log(UpdateClient.LogTypeEnum.Info, "Delete Extra File: " + relPath);
                }
            }

            // Delete Extra Directories?!?
            var dirs = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
            foreach (var dir in dirs)
            {
                var relPath = dir.Substring(path.Length + 1);

                if (relPath.Equals(@"Setup", StringComparison.InvariantCultureIgnoreCase)
                    || relPath.Equals(@"Support", StringComparison.InvariantCultureIgnoreCase)
                    || relPath.Equals(@"Plugins", StringComparison.InvariantCultureIgnoreCase)
                    || relPath.StartsWith(@"Plugins\", StringComparison.InvariantCultureIgnoreCase)
                    || relPath.Equals(@"Demo\Web\App_Data", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (!updateFiles.Any(a => a.FileName.StartsWith(relPath)))
                {
                    updateClient.WorkerConfig.WorkerDeletes.Add(new WorkerConfig.WorkerDelete { Path = dir });
                    updateClient.Log(UpdateClient.LogTypeEnum.Info, "Delete Extra Dir: " + relPath);
                }
            }

            // check Files
            var fixFiles = new ConcurrentBag<WorkerConfig.WorkerFile>();

            var updateFilesToCheck = updateFiles.Where(_ => _.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            if (filePath.Equals("AppDir", StringComparison.InvariantCultureIgnoreCase))
            {
                updateFilesToCheck = updateFilesToCheck.Where(_ => !_.FileName.Equals("Branding.xml", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            }
            var totalSize = updateFilesToCheck.Sum(_ => _.FileSize);
            Parallel.ForEach(updateFilesToCheck, updateFile =>
            {
                System.Threading.Interlocked.Add(ref _sizeProcessed, updateFile.FileSize);
                NotifyProgressChanged(Convert.ToInt32((Convert.ToDecimal(_sizeProcessed) / Convert.ToDecimal(totalSize)) * 100), $"{updateFile.FilePath}\\{updateFile.FileName}");
                var localFile = Path.Combine(path, updateFile.FileName);

                var fileInfo = new FileInfo(localFile);

                if (!fileInfo.Exists
                    || !Tools.CompareLazyFileDateTime(fileInfo.LastWriteTime, updateFile.FileDateTime)
                    || fileInfo.Length != updateFile.FileSize
                    || !GetHash(fileInfo, updateFile).Equals(updateFile.CheckSum, StringComparison.InvariantCultureIgnoreCase))
                {
                    var fixFile = new WorkerConfig.WorkerFile
                    {
                        Source = Path.Combine(updateFile.FilePath, updateFile.FileName),
                        Destination = localFile,
                        FileDateTime = updateFile.FileDateTime,
                        FileSize = updateFile.FileSize,
                        FileSizeGz = updateFile.FileSizeGz,
                        FileName = updateFile.FileName,
                        NewFile = !fileInfo.Exists
                    };
                    fixFiles.Add(fixFile);
                }
            });
            updateClient.WorkerConfig.WorkerFiles.AddRange(fixFiles);
        }

        private static string GetHash(FileSystemInfo file, UpdateConfig.UpdateFile updateFile)
        {
            if (updateFile.FilePath.Equals("AppDir", StringComparison.InvariantCultureIgnoreCase)
                && Path.GetFileName(file.FullName).StartsWith("Eulg", StringComparison.InvariantCultureIgnoreCase)
                && (Path.GetExtension(file.FullName).Equals(".exe", StringComparison.InvariantCultureIgnoreCase) || Path.GetExtension(file.FullName).Equals(".dll", StringComparison.InvariantCultureIgnoreCase)))
            {
                bool disasmUsed;
                updateFile.CheckSum = Tools.CalculateAssemblyHash(file.FullName, _ildasmPath, out disasmUsed);
            }
            else
            {
                updateFile.CheckSum = Tools.CalculateMd5Hash(file.FullName);
            }
            return updateFile.CheckSum;
        }

        #endregion

        #region Upload

        public bool Upload(bool log, bool queue, bool cache)
        {
            ProxyConfig.SetDefault();

            var archiveFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".zip");
            if (File.Exists(archiveFileName))
            {
                File.Delete(archiveFileName);
            }
            using (var archive = ZipFile.Open(archiveFileName, ZipArchiveMode.Create))
            {
                if (log)
                {
                    var di = new DirectoryInfo(DataDir);
                    foreach (var fileInfo in di.GetFiles("EULG_client-*.log", SearchOption.TopDirectoryOnly).OrderByDescending(_ => _.LastWriteTime).Take(2).ToArray())
                    {
                        NotifyProgressChanged(-1, fileInfo.Name);
                        TryAddFile(archive, fileInfo.FullName, fileInfo.Name);
                    }
                    foreach (var fileInfo in di.GetFiles("Eulg_sync_client-*.log", SearchOption.TopDirectoryOnly).OrderByDescending(_ => _.LastWriteTime).Take(2).ToArray())
                    {
                        NotifyProgressChanged(-1, fileInfo.Name);
                        TryAddFile(archive, fileInfo.FullName, fileInfo.Name);
                    }
                }
                if (queue || cache)
                {
                    var diData = new DirectoryInfo(Path.Combine(DataDir, "Data"));
                    if (queue)
                    {
                        foreach (var fileInfo in diData.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                                       .Where(_ => _.Name.StartsWith("queue", StringComparison.InvariantCultureIgnoreCase) && (_.Extension.Equals(".db", StringComparison.InvariantCultureIgnoreCase) || _.Extension.Equals(".db-journal", StringComparison.InvariantCultureIgnoreCase))).ToArray())
                        {
                            NotifyProgressChanged(-1, fileInfo.Name);
                            TryAddFile(archive, fileInfo.FullName, Path.Combine("Queue", fileInfo.Name));
                        }
                    }
                    if (cache)
                    {
                        foreach (var fileInfo in diData.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                                       .Where(_ => !_.Name.StartsWith("queue", StringComparison.InvariantCultureIgnoreCase) && (_.Extension.Equals(".db", StringComparison.InvariantCultureIgnoreCase) || _.Extension.Equals(".db-journal", StringComparison.InvariantCultureIgnoreCase))).ToArray())
                        {
                            NotifyProgressChanged(-1, fileInfo.Name);
                            TryAddFile(archive, fileInfo.FullName, Path.Combine("Data", fileInfo.Name));
                        }
                    }
                }
            }
            var uri = new Uri((CurrentBranding.Update.UseHttps ? "https://" : "http://") + CurrentBranding.Urls.Update.TrimEnd('/') + "/UploadSupportFile");
            var request = WebRequest.CreateHttp(uri);
            request.Method = "POST";
            request.Headers.Add("UpdateChannel", CurrentBranding.Update.Channel.ToString());
            request.Headers.Add("UserName", $"{Environment.UserName}@{Environment.MachineName}");
            using (var reqStream = request.GetRequestStream())
            {
                using (var archiveStream = File.OpenRead(archiveFileName))
                {
                    var buffer = new byte[STREAM_BUFFER_SIZE];
                    int read;
                    long s = 0;
                    var archSize = archiveStream.Length;
                    if (archSize < 1)
                    {
                        archSize = 1;
                    }
                    while ((read = archiveStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        NotifyProgressChanged(Convert.ToInt32((Convert.ToDecimal(s) / Convert.ToDecimal(archSize)) * 100), "Upload...");
                        reqStream.Write(buffer, 0, read);
                        s += read;
                    }
                }
            }
            NotifyProgressChanged(-1, "Upload...");
            var ok = false;
            var result = request.GetResponse();
            var resStream = result.GetResponseStream();
            if (resStream != null)
            {
                using (var x = new StreamReader(resStream))
                {
                    var answer = x.ReadToEnd();
                    if (answer.Equals("OK"))
                    {
                        ok = true;
                    }
                }
            }
            if (File.Exists(archiveFileName))
            {
                File.Delete(archiveFileName);
            }
            return ok;
        }

        private void TryAddFile(ZipArchive archive, string sourceFileName, string entryName)
        {
            try
            {
                // Hier nicht
                // archive.CreateEntryFromFile(...);
                // benutzen! Wichtig ist unten das FileShare.ReadWrite, da sonst die aktuelle Datei im Upload fehlt, wenn der Client offen ist!

                using (var stream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var zipArchiveEntry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    var dateTime = File.GetLastWriteTime(sourceFileName);
                    if (dateTime.Year < 1980 || dateTime.Year > 2107) dateTime = new DateTime(1980, 1, 1, 0, 0, 0);
                    zipArchiveEntry.LastWriteTime = dateTime;
                    using (var destination1 = zipArchiveEntry.Open())
                    {
                        stream.CopyTo(destination1);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        private void NotifyProgressChanged(int percent = -1, string message = null)
        {
            try
            {
                if (percent < -1)
                {
                    percent = -1;
                }
                if (percent > 100)
                {
                    percent = 100;
                }
                if (percent == _oldPercent && message == _oldMessage)
                {
                    return;
                }
                if (string.IsNullOrEmpty(message))
                    message = _oldMessage;
                _oldMessage = message;
                _oldPercent = percent;
                ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percent, message));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region LogViewer

        public string GetLogEntries(ELogType logType, int number = 2)
        {
            var di = new DirectoryInfo(DataDir);
            var pattern = string.Empty;
            switch (logType)
            {
                case ELogType.Client:
                    pattern = "EULG_client-*.log";
                    break;
                case ELogType.Sync:
                    pattern = "Eulg_sync_client-*.log";
                    break;
            }
            var q = di.GetFiles(pattern, SearchOption.TopDirectoryOnly).OrderByDescending(_ => _.LastWriteTime);
            q = number > 0 ? q.Take(number).OrderBy(_ => _.LastWriteTime) : q.OrderBy(_ => _.LastWriteTime);
            var sb = new StringBuilder();
            foreach (var fileInfo in q.ToArray())
            {
                foreach (var line in WriteSafeReadAllLines(fileInfo.FullName))
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        #endregion

        #region UpdateService

        public static bool CheckUpdateService()
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
            {
                return false;
            }
            using (var svc = new ServiceController(UPDATE_SERVICE_NAME))
            {
                if (svc.Status != ServiceControllerStatus.Running)
                {
                    try
                    {
                        svc.Start();
                        svc.WaitForStatus(ServiceControllerStatus.Running);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static bool InstallUpdateService()
        {
            try
            {
                if (ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
                {
                    RemoveUpdateService();
                }
                var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH", "UpdateService", "UpdateService.exe");
                var p = new Process
                {
                    StartInfo =
                            {
                                FileName = exePath,
                                Arguments = "install",
                                Verb = "runas"
                            }
                };
                p.Start();
                p.WaitForExit();
                return true;
            }
            catch
            {
                //LogException(exception);
            }
            return false;
        }

        public static bool RemoveUpdateService()
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
            {
                return true;
            }
            var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH", "UpdateService", "UpdateService.exe");
            if (!File.Exists(exePath))
            {
                return true;
            }
            try
            {
                StopService(true);
                var p = new Process
                {
                    StartInfo =
                            {
                                FileName = exePath,
                                Arguments = "uninstall",
                                Verb = "runas"
                            }
                };
                p.Start();
                p.WaitForExit();
                return true;
            }
            catch
            {
                //LogException(exception);
            }
            return false;
        }

        public static void StopService(bool wait)
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
            {
                return;
            }
            using (var svc = new ServiceController(UPDATE_SERVICE_NAME))
            {
                if (svc.Status != ServiceControllerStatus.Stopped)
                {
                    svc.Stop();
                }
                if (wait)
                {
                    svc.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
        }

        #endregion

        public static bool ExitClient()
        {
#if DEBUG
            const string PROCESS = "EULG_client.vshost";
#else
            const string PROCESS = "EULG_client";
#endif

            if (ProcessHelper.IsProcessRunning(PROCESS))
            {
                if (MessageBox.Show("Um die gewünschte Funktion auszuführen, muss der Beratungsclient geschlossen werden.\n\n Wollen Sie den Beratungslient jetzt schließen?",
                                    "Beratungsclient schließen", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    ProcessHelper.CloseProcess(PROCESS);
                }
            }

            return !ProcessHelper.IsProcessRunning(PROCESS);
        }
    }
}
