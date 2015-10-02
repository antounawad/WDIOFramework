using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Eulg.Client.SupportTool
{
    public enum EProcessState
    {
        NotFound,
        AlreadyRunning,
        Started,
        Closed,
        Error
    }

    public class ProcessHelper
    {
        public static EProcessState StartProcess(string applicationPath, string processName, bool runas = false, string arguments = "")
        {
            if (string.IsNullOrWhiteSpace(applicationPath) || !File.Exists(applicationPath))
            {
                return EProcessState.NotFound;
            }

            if (IsProcessRunning(processName))
            {
                return EProcessState.AlreadyRunning;
            }

            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = applicationPath,
                        Arguments = arguments,
                        Verb = runas ? "runas" : String.Empty,
                    }
                };

                process.Start();
            }
            catch
            {
                //Logging.Log.Instance.Here().Warning(ex, "Fehler beim Starten des Prozesses \"{0}\"", processName);
                return EProcessState.Error;
            }


            return EProcessState.Started;
        }

        public static EProcessState CloseProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            if (!processes.Any())
            {
                return EProcessState.NotFound;
            }

            try
            {
                processes.First().CloseMainWindow();

                Thread.Sleep(200);

                if (!processes.First().HasExited)
                {
                    processes.First().Kill();
                }
            }
            catch
            {
                //Logging.Log.Instance.Here().Warning(ex, "Fehler beim Beenden des Prozesses \"{0}\"", processName);
                return EProcessState.Error;
            }

            processes = Process.GetProcessesByName(processName);

            return processes.Any() ? EProcessState.Error : EProcessState.Closed;
        }

        public static bool IsProcessRunning(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            return processes.Any();
        }
    }
}
