using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Eulg.Update.Shared;

namespace Eulg.Update.Worker
{
    internal class UpdateWorker
    {
        private const string XmlFileName = "UpdateWorker.xml";
        private const int FileOpRetryDelay = 300;

        private readonly string _xmlFile;

        internal WorkerConfig Config { get; set; }
        internal const int WaitForProcessTimeout = 45;
        internal const int CloseProcessTimeout = 10;
        internal const int KillProcessTimeout = 30;
        internal const int ThreadSleepTime = 500;
        internal const string UpdateServiceName = "EulgUpdate";
        internal IUpdater Updater;
        public const string ResetFileTag = ".$EulgReset$";
        internal bool UseService { get; set; }

        internal UpdateWorker()
        {
            _xmlFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, XmlFileName);
        }

        internal void CreateChannel()
        {
            Updater = new ServiceProxy { UseService = UseService };
        }

        internal void ReadConfig()
        {
            if (!File.Exists(_xmlFile))
            {
                throw new Exception("Konfigurationsdatei nicht gefunden!");
            }

            var xmlSer = new XmlSerializer(typeof (WorkerConfig));
            using (var reader = File.OpenRead(_xmlFile))
            {
                Config = (WorkerConfig) xmlSer.Deserialize(reader);
            }

            if (!string.IsNullOrEmpty(Config.LogFile))
            {
                Log.TieToFile(Config.LogFile);
            }
        }

        #region Process Helpers

        internal bool WaitForProcess()
        {
            if (Config.WaitForProcess == 0)
            {
                return true;
            }

            try
            {
                using (var process = Process.GetProcessById(Config.WaitForProcess))
                {
                    if (!process.WaitForExit(WaitForProcessTimeout * 1000))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (ArgumentException)
            {
                // GetProcessById() throws this if it can't find a process with the given ID.
                return true;
            }
        }

        internal Process[] CheckProcesses()
        {
            var killProcessNames = (Config.KillProcesses ?? string.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var checkProcessNames = (Config.CheckProcesses ?? string.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            var swKill = new Stopwatch();
            swKill.Start();

            var killProcesses = GetRunningProcesses(killProcessNames);
            var killTasks = killProcesses.Select(p => Task.Run(delegate
            {
                try
                {
                    if (!p.CloseMainWindow())
                    {
                        Log.Write(LogTypeEnum.Info, $"Call to CloseMainWindow() failed, attempting Kill() to get rid of {p.MainModule.FileName}");
                        p.Kill();
                    }
                    else if (!p.WaitForExit(CloseProcessTimeout * 1000))
                    {
                        Log.Write(LogTypeEnum.Info, $"{p.MainModule.FileName} did not react to CloseMainWindow(), attempting Kill()");
                        p.Kill();
                    }

                    if (!p.WaitForExit(KillProcessTimeout * 1000))
                    {
                        Log.Write(LogTypeEnum.Warning, $"Tried but could not kill {p.MainModule.FileName}");
                        return p;
                    }

                    Log.Write(LogTypeEnum.Info, $"Killed Process {p.Id}: {p.MainModule.FileName}");
                    p.Dispose();
                    return null;
                }
                catch (Exception ex)
                {
                    Log.Write(ex, $"While ending process '{p.ProcessName}'");
                    return p;
                }
            })).ToArray();

            Task.WaitAll(killTasks);

            var list = new List<Process>();
            list.AddRange(killTasks.Where(t => t.Result != null).Select(t => t.Result));
            list.AddRange(GetRunningProcesses(checkProcessNames));
            return list.ToArray();
        }

        private IEnumerable<Process> GetRunningProcesses(IEnumerable<string> processNames)
        {
            foreach (var p in processNames.SelectMany(Process.GetProcessesByName))
            {
                try
                {
                    if (!p.MainModule.FileName.StartsWith(Config.AppPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex, "While determining if running process belongs to target installation (will kill it regardless)");
                    // Wichtig: im Fehlerfall auf jeden Fall adden wegen Rename-B.u.g. in Windows-API!!!
                }

                yield return p;
            }
        }

        #endregion

        internal void ExecuteUpdate(FrmProgress mainWindow)
        {
            var operations = new List<FileOp>();

            var clientSource = Assembly.GetExecutingAssembly().Location;
            var fi = new FileInfo(clientSource);
            var clientDestination = Path.Combine(Config.AppPath, fi.Name);
            try
            {
                Updater.FileCopy(clientSource, clientDestination, true);
            }
            catch (Exception ex)
            {
                Log.Write(LogTypeEnum.Warning, $"Updating {fi.Name}: {ex.GetType().Name} [{ex.Message}]; this is non-critical");
            }

            foreach (var file in Config.WorkerFiles)
            {
                var sourcePath = Path.Combine(Config.TempPath, file.Source);
                var isResetFile = file.Destination.EndsWith(ResetFileTag, StringComparison.InvariantCultureIgnoreCase);
                operations.Add(FileOp.Copy(Updater, sourcePath, file.Destination, isResetFile));
            }

            operations.AddRange(Config.WorkerDeletes.Select(d => FileOp.Delete(Updater, d.Path)).Where(op => op != null).OrderByDescending(op => op.TargetFile.Length));

            Action initializeProgress = delegate
            {
                mainWindow.UpdateProgress.Value = 0;
                mainWindow.UpdateProgress.Minimum = 0;
                mainWindow.UpdateProgress.Maximum = operations.Count*2;
                mainWindow.UpdateProgress.Style = ProgressBarStyle.Continuous;
            };

            var progress = 0;
            Action setProgress = () => mainWindow.UpdateProgress.Value = progress;

            mainWindow?.Invoke(initializeProgress);

            try
            {
                foreach (var op in operations)
                {
                    var retryCount = 0;
                    while (true)
                    {
                        try
                        {
                            op.Prepare();
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log.Write(LogTypeEnum.Warning, $"{op.GetType().Name} {op.TargetFile}: {ex.GetType().Name} [{ex.Message}]");
                            ++retryCount;

                            if (retryCount < 3)
                            {
                                Thread.Sleep(FileOpRetryDelay);
                                continue;
                            }

                            if (MessageBox.Show("Fehler beim Aktualisieren der Datei: " + op.TargetFile + Environment.NewLine + ex.Message + Environment.NewLine +
                                                Environment.NewLine + "Erneut versuchen?", "EULG Update: Fehler", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) != DialogResult.Retry)
                            {
                                throw;
                            }

                            retryCount = 0;
                        }
                    }

                    ++progress;
                    mainWindow?.Invoke(setProgress);
                }

                var serviceErrors = Updater.GetExceptions(true);
                if (serviceErrors.Any())
                {
                    foreach (var error in serviceErrors)
                    {
                        Log.Write(LogTypeEnum.Error, "Service error during prepare phase: {0}", error);
                    }

                    throw new Exception(string.Join("; ", serviceErrors));
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, "Leaving prepare phase and rolling back");

                operations.Reverse();
                foreach (var op in operations)
                {
                    op.Rollback();
                }

                throw;
            }

            Log.Write(LogTypeEnum.Info, "Prepared {0} operations. Beginning commit", operations.Count);

            foreach (var op in operations)
            {
                op.Commit();

                ++progress;
                mainWindow?.Invoke(setProgress);
            }

            var commitErrors = Updater.GetExceptions(true);
            if (commitErrors.Any())
            {
                foreach (var error in commitErrors)
                {
                    Log.Write(LogTypeEnum.Error, "Service error during commit phase: {0}", error);
                }

                throw new Exception(string.Join("; ", commitErrors));
            }

            Log.Write(LogTypeEnum.Info, "Successfully committed {0} operations", operations.Count);
        }

        internal void CleanUp()
        {
            foreach (var dir in Directory.GetDirectories(Config.TempPath))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {
                    Log.Write(LogTypeEnum.Warning, "Failed to clean up temp folder: [{0}] {1}", ex.GetType().Name, ex.Message);
                }
            }

            if (File.Exists(_xmlFile))
            {
                File.Delete(_xmlFile);
            }
        }

        internal void StartApplication(bool bUpdateSuccess)
        {
            if (string.IsNullOrEmpty(Config.ApplicationFile))
            {
                return;
            }

            try
            {
                using (var p = new Process
                {
                    StartInfo =
                    {
                        FileName = Config.ApplicationFile,
                        Arguments = $"{Config.CommandLineArgs} {(bUpdateSuccess ? "updatesuccess" : "updatefail")}"
                    }
                })
                {
                    p.Start();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, "White trying to start client application");
                MessageBox.Show("Die Aktualisierung wurde durchgeführt, aber der Beratungsclient konnte leider nicht automatisch gestartet werden. Bitte starten Sie den Client manuell.",
                    "EULG Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        internal void ExecuteCustomProcess()
        {
            if (Config.StartProcess != null)
            {
                Updater.ProcessStart(Config.StartProcess, true);
            }
        }

        internal enum LogTypeEnum
        {
            Info,
            Warning,
            Error
        }

        #region Service

        internal void StartService(bool wait)
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UpdateServiceName)))
            {
                throw new ServiceActivationException("Eulg-Update-Service nicht installiert! Bitte starten Sie das EULG Support Tool!");
            }
            using (var svc = new ServiceController(UpdateServiceName))
            {
                if (svc.Status != ServiceControllerStatus.Running)
                {
                    try
                    {
                        svc.Start();
                    }
                    catch (Exception exception)
                    {
                        throw new ServiceActivationException("Eulg-Update-Service kann nicht gestartet werden. Bitte Dienst 'EULG Update' auf Startart 'Manuell' setzen!", exception);
                    }
                }
                if (wait)
                {
                    svc.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
        }

        internal static void StopService(bool wait)
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UpdateServiceName)))
            {
                return;
            }
            using (var svc = new ServiceController(UpdateServiceName))
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

        internal static void InstallUpdateService()
        {
            if (ServiceController.GetServices().Any(a => a.ServiceName.Equals(UpdateServiceName)))
            {
                RemoveUpdateService();
            }
            var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH", "UpdateService", "UpdateService.exe");
            var p = new Process
            {
                StartInfo =
                {
                    FileName = exePath,
                    Arguments = "install"
                }
            };
            p.Start();
            p.WaitForExit();
        }

        internal static void RemoveUpdateService()
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UpdateServiceName)))
            {
                return;
            }
            var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH", "UpdateService", "UpdateService.exe");
            if (!File.Exists(exePath))
            {
                return;
            }
            StopService(true);
            var p = new Process
            {
                StartInfo =
                {
                    FileName = exePath,
                    Arguments = "uninstall"
                }
            };
            p.Start();
            p.WaitForExit();
        }

        #endregion
    }
}
