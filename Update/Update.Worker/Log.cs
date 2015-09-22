using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Eulg.Update.Worker
{
    internal static class Log
    {
        private static readonly IList<string> _backLog = new List<string>();

        private static StreamWriter _writer;

        public static void TieToFile(string path)
        {
            if (_writer != null)
            {
                throw new InvalidOperationException();
            }

            _writer = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8, 2048, false);

            foreach (var line in _backLog)
            {
                _writer.WriteLine(line);
            }

            _backLog.Clear();
        }

        private static void Write(string line)
        {
            if (_writer != null)
            {
                _writer.WriteLine(line);
            }
            else
            {
                _backLog.Add(line);
            }
        }

        public static void Write(UpdateWorker.LogTypeEnum type, string format, params object[] args)
        {
            var message = args.Length > 0 ? string.Format(format, args) : format;
            var text = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {type.ToString().PadRight(7)}: {"UpdateWorker"}: {message.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", " ")}";
            Write(text);
        }

        public static void Write(Exception ex, string format, params object[] args)
        {
            var message = args.Length > 0 ? string.Format(format, args) : format;
            Write(UpdateWorker.LogTypeEnum.Error, $"[{ex.GetType().Name} \"{ex.Message}\"] {message}");
        }

        public static void Close()
        {
            if (_writer == null)
            {
                if (_backLog.Count == 0)
                {
                    return;
                }

                TieToFile(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? string.Empty, "UpdateWorker.log"));
            }
            else
            {
                _writer.Flush();
                _writer.Close();
            }
            _writer = null;
        }
    }
}
