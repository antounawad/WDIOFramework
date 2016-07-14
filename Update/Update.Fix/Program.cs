using System;
using System.Linq;
using System.Security.Principal;
using Eulg.Shared;
using Update.Fix.Fixes;

namespace Update.Fix
{
    internal class Program
    {
        private static bool _fix;
        private static bool _all;
        private static string[] _args;
        private static int _numDefect;
        private static int _numFix;

        private delegate bool Check();

        /* 
            Exit-Codes: 0 - alles I.O.
                        1 - mind. 1 Check negativ
                        2 - mind. 1 Fix ausgeführt
                        9 - mind. 1 Check/Fix fehlerhaft
        */
        private static int Main(string[] args)
        {
            try
            {
                _fix = args.Any(a => a.Equals("FIX", StringComparison.InvariantCultureIgnoreCase));
                _all = args.Any(a => a.Equals("ALL", StringComparison.InvariantCultureIgnoreCase));
                _args = args;
                if (_fix && !IsAdministrator())
                {
                    throw new Exception("FIX nur als Administrator möglich!");
                }
                if (args.Length < 1) _all = true;

                // Registry Keys
                DoCheck("RegistryKeys", RegistryKeys.Check, RegistryKeys.Fix);

                // Startmenu
                DoCheck("StartMenu", StartMenu.Check, StartMenu.Fix);

                // Desktop links
                DoCheck("DesktopLinks", DesktopLinks.Check, DesktopLinks.Fix);

                // Update Service
                DoCheck("UpdateService", UpdateService.Check, UpdateService.Fix);

                // Branding
                DoCheck("BrandingEntries", BrandingEntries.Check, BrandingEntries.Fix);

                if (_numFix > 0) return 2;
                if (_numDefect > 0) return 1;
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.GetMessagesTree());
                return 9;
            }
        }

        private static void DoCheck(string fixName, Check check, Action fix)
        {
            if (_all || _args.Any(a => a.Equals(fixName, StringComparison.InvariantCultureIgnoreCase)))
            {
                Console.Write("Prüfe {0}: ", fixName);
                Console.Out.Flush();
                var checkResult = check();
                if (checkResult)
                {
                    Console.Write("OK" + Environment.NewLine);
                }
                else
                {
                    _numDefect++;
                    Console.Write("DEFEKT");
                    if (_fix)
                    {
                        Console.Write(" -> KORREKTUR: ");
                        Console.Out.Flush();
                        try
                        {
                            fix();
                            _numFix++;
                            Console.Write("ERFOLGREICH." + Environment.NewLine);
                        }
                        catch (Exception exception)
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