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
                DoCheck(RegistryKeys.Inst);

                // Startmenu
                DoCheck(StartMenu.Inst);

                // Desktop links
                DoCheck(DesktopLinks.Inst);

                // Update Service
                //DoCheck(UpdateService.Inst);

                // Branding
                DoCheck(BrandingEntries.Inst);

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

        private static void DoCheck(IFix fix)
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
                    _numDefect++;
                    Console.Write(result == null ? "NICHT FESTSTELLBAR" : "DEFEKT");
                    if(_fix)
                    {
                        Console.Write(" -> KORREKTUR: ");
                        Console.Out.Flush();
                        try
                        {
                            fix.Apply();
                            _numFix++;
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