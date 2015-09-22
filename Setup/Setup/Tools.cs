using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Windows.Input;

namespace Eulg.Setup
{
    public class Tools
    {
        public static string CalculateMd5Hash(string fileName)
        {
            var md5 = MD5.Create();
            using (var si = File.OpenRead(fileName))
            {
                var hash = md5.ComputeHash(si);
                var sb = new StringBuilder();
                foreach (byte t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static bool CompareLazyFileDateTime(DateTime file1, DateTime file2) // True wenn Dateien identisch!
        {
            var t = (file1 - file2);
            if (t.Days != 0)
            {
                return false;
            }
            if (t.Minutes != 0)
            {
                return false;
            }
            if (Math.Abs(t.Hours) > 2)
            {
                return false; // 1 Stunde für UTC/MEZ und 1 Stunde für Sommerzeit
            }
            if (Math.Abs(t.Seconds) > 2)
            {
                return false; // 2 Sekunden kulanz für FAT
            }
            return true;
        }

        public static void SetDirectoryAccessControl(string path)
        {
            var dirSec = Directory.GetAccessControl(path);
            dirSec.SetAccessRuleProtection(true, false);
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            dirSec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.SetAccessControl(path, dirSec);
        }

        public static string GetCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            var a = String.Empty;
            for (var i = 1; i < args.Length; i++)
            {
                var s = args[i];
                a += s + " ";
            }
            return a.Trim();
        }

        public class WaitCursor : IDisposable
        {
            private readonly Cursor _previousCursor;

            public WaitCursor()
            {
                _previousCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;
            }

            public void Dispose()
            {
                Mouse.OverrideCursor = _previousCursor;
            }
        }
    }
}
