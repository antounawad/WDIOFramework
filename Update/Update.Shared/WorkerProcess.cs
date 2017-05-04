using System.Diagnostics;

namespace Eulg.Update.Shared
{
    public class WorkerProcess
    {
        public string FileName { get; set; }

        public string Arguments { get; set; }

        public bool CreateNoWindow { get; set; }

        public bool UseShellExecute { get; set; }

        public static WorkerProcess CreateFromProcessStartInfo(ProcessStartInfo info)
        {
            var proc = new WorkerProcess
            {
                FileName = info.FileName,
                Arguments = info.Arguments,
                CreateNoWindow = info.CreateNoWindow,
                UseShellExecute = info.UseShellExecute
            };

            return proc;
        }

        public ProcessStartInfo ToProcessStartInfo()
        {
            var proc = new ProcessStartInfo
            {
                FileName = FileName,
                Arguments = Arguments,
                CreateNoWindow = CreateNoWindow,
                UseShellExecute = UseShellExecute
            };

            return proc;
        }
    }
}