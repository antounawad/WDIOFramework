using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Eulg.Shared;
using Update.Fix.Fixes;

namespace Update.Fix
{
    internal class Program
    {
        private readonly string[] _args;
        private readonly bool _fix;
        private readonly bool _all;

        /* 
            Exit-Codes: 0 - alles I.O.
                        1 - mind. 1 Check negativ
                        2 - mind. 1 Fix ausgeführt
                        9 - mind. 1 Check/Fix fehlerhaft
        */
        private static int Main(string[] args)
        {
            var startAppWhenDone = args.Any(a => a.Equals("STARTAPP", StringComparison.OrdinalIgnoreCase));

            try
            {
                var program = new Program(args);
                var defectsCount = 0;
                var repairedCount = 0;

                // Registry Keys
                program.DoCheck(RegistryKeys.Inst, ref defectsCount, ref repairedCount);

                // Startmenu
                program.DoCheck(StartMenu.Inst, ref defectsCount, ref repairedCount);

                // Desktop links
                program.DoCheck(DesktopLinks.Inst, ref defectsCount, ref repairedCount);

                // Update Service
                //program.DoCheck(UpdateService.Inst, ref defectsCount, ref repairedCount);

                // Branding
                program.DoCheck(BrandingEntries.Inst, ref defectsCount, ref repairedCount);

                if (defectsCount > 0) return 2;
                if (repairedCount > 0) return 1;
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.GetMessagesTree());
                return 9;
            }
            finally
            {
                if (startAppWhenDone)
                {
                    var directory = AppDomain.CurrentDomain.BaseDirectory;
                    var clientExecutable = File.Exists(Path.Combine(directory, "EULG_client.exe"))
                        ? Path.Combine(directory, "EULG_client.exe")
                        : Path.Combine(Path.GetDirectoryName(directory), "EULG_client.exe");

                    using (var process = new Process
                    {
                        StartInfo = new ProcessStartInfo(clientExecutable)
                        {
                            Arguments = "noupdate",
                            Verb = "run"
                        }
                    })
                    {
                        process.Start();
                    }
                }
            }
        }

        private Program(string[] args)
        {
            _args = args;
            _fix = args.Any(a => a.Equals("FIX", StringComparison.OrdinalIgnoreCase));
            _all = args.Any(a => a.Equals("ALL", StringComparison.OrdinalIgnoreCase));

            if(_fix && !IsAdministrator())
            {
                throw new Exception("FIX nur als Administrator möglich!");
            }

            if(args.Length < 1) _all = true;
        }

        private void DoCheck(IFix fix, ref int defectsCount, ref int repairedCount)
        {
            if(_all || _args.Any(a => a.Equals(fix.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Console.Write("Prüfe {0}: ", fix.Name);
                Console.Out.Flush();

                bool? result;
                try
                {
                    result = fix.Check();
                }
                catch (Exception exception)
                {
                    Console.Write("FEHLER!" + Environment.NewLine);
                    Console.WriteLine(exception.GetMessagesTree());
                    return;
                }

                if(result == true)
                {
                    Console.WriteLine("OK");
                }
                else
                {
                    ++defectsCount;
                    Console.Write(result == null ? "NICHT FESTSTELLBAR" : "DEFEKT");
                    if(_fix)
                    {
                        Console.Write(" -> KORREKTUR: ");
                        Console.Out.Flush();
                        try
                        {
                            fix.Apply();
                            ++repairedCount;
                            Console.Write("ERFOLGREICH." + Environment.NewLine);
                        }
                        catch(Exception exception)
                        {
                            Console.Write("FEHLER!" + Environment.NewLine);
                            Console.WriteLine(exception.GetMessagesTree());
                        }
                    }
                    else
                    {
                        Console.Write(Environment.NewLine);
                    }
                }
            }
        }

        private static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
