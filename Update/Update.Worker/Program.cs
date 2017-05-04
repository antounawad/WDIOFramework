using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Eulg.Update.Worker
{
    internal static class Program
    {
        internal static FrmProgress MainWindow;
        public static Thread Worker;

        [STAThread]
        internal static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1].Equals("/UpdateService", StringComparison.CurrentCultureIgnoreCase))
            {
                UpdateService();
                Application.Exit();
                return;
            }

            MainWindow = new FrmProgress();
            MainWindow.Show();

            Worker = new Thread(RunWorker) { IsBackground = false };
            Worker.Start();

            Application.Run(MainWindow);
        }

        private static void RunWorker()
        {
            var worker = new UpdateWorker { UseService = !IsAdministrator() };
            try
            {
                try
                {
                    var startServiceTask = Task.Run(delegate
                    {
                        if (worker.UseService)
                        {
                            worker.StartService(true);
                        }
                    });

                    worker.CreateChannel();
                    worker.ReadConfig();

                    while (!worker.WaitForProcess())
                    {
                        var result = MessageBox.Show("Die Aktualisierung kann nicht durchgeführt werden, da der Beratungsclient noch läuft.", "EULG Update", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
                        if (result != DialogResult.Retry)
                        {
                            Log.Write(UpdateWorker.LogTypeEnum.Info, "Client is still running and the user chose to cancel");
                            return;
                        }

                        Log.Write(UpdateWorker.LogTypeEnum.Info, "Client was still running; retrying as user's request");
                    }

                    var runningProcs = worker.CheckProcesses();
                    while (runningProcs.Any(p => !p.HasExited))
                    {
                        var tmp = runningProcs.Where(p => !p.HasExited).Select(p => Environment.NewLine + "– " + p.ProcessName);
                        var result = MessageBox.Show("Die Aktualisierung kann nicht durchgeführt werden, da noch noch abhängige Anwendungen laufen. " +
                                                     "Laufende Prozesse: " + string.Join("", tmp), "EULG Update", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
                        if (result != DialogResult.Retry)
                        {
                            Log.Write(UpdateWorker.LogTypeEnum.Info, "There were running processes left and the user chose to cancel");
                            return;
                        }

                        Log.Write(UpdateWorker.LogTypeEnum.Info, "There were running processes left; retrying as user's request");
                    }

                    foreach (var p in runningProcs)
                    {
                        p.Dispose();
                    }

                    if (worker.UseService)
                    {
                        startServiceTask.Wait();
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(UpdateWorker.LogTypeEnum.Error, ex.ToString());
                    var msg = ex is AggregateException ? string.Join(Environment.NewLine, ((AggregateException) ex).Flatten().InnerExceptions.Where(f => f is ServiceActivationException).Select(s => s.Message)) : ex.Message;
                    if (MessageBox.Show("Die Aktualisierung konnte leider nicht durchgeführt werden. Bei der Vorbereitung des Updates trat ein unvorhergesehener Fehler auf. Es wird die letzte Version des Clients gestartet." +
                                        Environment.NewLine + Environment.NewLine + string.Format("Fehler: {1} ({0})", ex.GetType(), msg), "EULG Update: Fehler", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    {
                        worker.StartApplication(false);
                    }
                    return;
                }

                try
                {
                    worker.ExecuteUpdate(MainWindow);
                }
                catch (Exception ex)
                {
                    Log.Write(UpdateWorker.LogTypeEnum.Error, ex.ToString());
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        if (MessageBox.Show(MainWindow, "Die Aktualisierung konnte leider nicht durchgeführt werden. Bei der Anwendung des Updates trat ein unvorhergesehener Fehler auf. Es wird die letzte Version des Clients gestartet." +
                                                        Environment.NewLine + Environment.NewLine + string.Format("Fehler: {1} ({0})", ex.GetType(), ex.Message), "EULG Update: Fehler", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                        {
                            worker.StartApplication(false);
                        }
                    });
                    return;
                }
                finally
                {
                    if (!worker.UseService)
                    {
                        UpdateWorker.StopService(false);
                    }
                }

                worker.CleanUp();

                if (MainWindow != null)
                {
                    Action refreshWindow = MainWindow.Refresh;
                    Action closeWindow = MainWindow.Close;

                    MainWindow.Invoke(refreshWindow);
                    Thread.Sleep(250);
                    MainWindow.Invoke(closeWindow);
                }

                worker.ExecuteCustomProcess();

                worker.StartApplication(true);
            }
            catch (Exception ex)
            {
                Log.Write(ex, "Error during cleanup... not notifying the user about this");
            }
            finally
            {
                Log.Close();
                Application.Exit();
            }
        }

        private static void UpdateService()
        {
            var updateExeNew = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "UpdateServiceNew.exe");
            if (File.Exists(updateExeNew))
            {
                throw new Exception(updateExeNew + " nicht gefunden!");
            }

            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UpdateWorker.UpdateServiceName)))
            {
                UpdateWorker.StopService(true);
            }

            var updateExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "xbAV Beratungssoftware GmbH", "UpdateService", "UpdateService.exe");
            if (!Directory.Exists(Path.GetDirectoryName(updateExe)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(updateExe));
            }
            if (!File.Exists(updateExe))
            {
                File.Copy(updateExeNew, updateExe, true);
            }

            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UpdateWorker.UpdateServiceName)))
            {
                UpdateWorker.RemoveUpdateService();
            }

            File.Copy(updateExeNew, updateExe, true);

            UpdateWorker.InstallUpdateService();
        }

        public static bool IsAdministrator()
        {
            //#if DEBUG
            //            return true;
            //#else
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            //#endif
        }
    }
}