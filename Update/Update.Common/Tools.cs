using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace Eulg.Update.Common
{
    public static class Tools
    {
        private static readonly Regex RegexMvid = new Regex("//\\s*MVID\\:\\s*(\\{[a-zA-Z0-9\\-]+\\})", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string CalculateMd5Hash(string fileName)
        {
            var md5 = MD5.Create();
            using (var si = File.OpenRead(fileName))
            {
                var hash = md5.ComputeHash(si);
                var sb = new StringBuilder();
                foreach (var t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static bool CompareLazyFileDateTime(DateTime file1, DateTime file2) // True wenn Dateien identisch!
        {
            var t = (file1 - file2);
            if (t.Days != 0) return false;
            if (t.Minutes != 0) return false;
            if (Math.Abs(t.Hours) > 2) return false;    // 1 Stunde für UTC/MEZ und 1 Stunde für Sommerzeit
            if (Math.Abs(t.Seconds) > 2) return false;  // 2 Sekunden kulanz für FAT
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

        public static string CalculateAssemblyHash(string filename, string ildasmPath, out bool disasmUsed)
        {
            Guid? mvId = null;
            using (var disasm = new Process
            {
                StartInfo =
                {
                    FileName = ildasmPath,
                    Arguments = String.Format("/all /text \"{0}\"", filename),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            })
            {
                disasm.Start();

                string line;
                while (mvId == null && (line = disasm.StandardOutput.ReadLine()) != null)
                {
                    var mvidMatch = RegexMvid.Match(line);
                    if (mvidMatch.Success)
                    {
                        mvId = Guid.Parse(mvidMatch.Groups[1].Value);
                    }
                }

                disasm.StandardOutput.ReadToEnd();
            }
            disasmUsed = false;
            if (mvId == null) return CalculateMd5Hash(filename);
            using (var si = File.OpenRead(filename))
            {
                var maskedStream = new AssemblyStripStream(si, mvId.Value);
                disasmUsed = true;
                return String.Join(string.Empty, MD5.Create().ComputeHash(maskedStream).Select(b => b.ToString("x2")));
            }
        }

        public static bool IsDemo
        {
            get
            {
#if DEMO
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsProof
        {
            get
            {
#if PROOF
                return true;
#else
                return false;
#endif
            }
        }

    }
}
