using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using Eulg.Shared;
using Update.Fix.Fixes;

namespace Update.Fix
{
    internal class Program
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();

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
            var showMsgbox = args.Any(a => a.Equals("UI", StringComparison.OrdinalIgnoreCase));

            try
            {
                var program = new Program(args);

                var showConsole = args.Any(a => a.Equals("TERM", StringComparison.OrdinalIgnoreCase));
                if (showConsole || !showMsgbox) // Default to console
                {
                    AllocConsole();
                }

                var defectsCount = 0;
                var repairedCount = 0;

                // Registry Keys
                program.DoCheck(RegistryKeys.Inst, ref defectsCount, ref repairedCount);

                // Startmenu
                program.DoCheck(StartMenu.Inst, ref defectsCount, ref repairedCount);

                // Desktop links
                program.DoCheck(DesktopLinks.Inst, ref defectsCount, ref repairedCount);

                // Update Service
                program.DoCheck(UpdateService.Inst, ref defectsCount, ref repairedCount);

                // Branding (Sonderverhalten für Migration von 1.x nach 2.0; nicht allgemeingültig)
                if (!program.DoCheck(BrandingEntries.Inst, ref defectsCount, ref repairedCount) && showMsgbox)
                {
                    ShowUpdateResultMessage(false);
                    return 9;
                }

                if (showMsgbox)
                {
                    // Sonderverhalten für Migration von 1.x nach 2.0; solange Aktualisierung der Branding OK ist der Rest egal.
                    ShowUpdateResultMessage(true);
                }

                if(defectsCount > 0) return 2;
                if (repairedCount > 0) return 1;
                return 0;
            }
            catch (Exception exception)
            {
                if (showMsgbox)
                {
                    ShowUpdateResultMessage(false, exception);
                }

                Console.WriteLine(exception.GetMessagesTree());
                return 9;
            }
        }

        private static void ShowUpdateResultMessage(bool success, Exception exception = null)
        {
            var message = success
                ? "Die Aktualisierung wurde erfolgreich durchgeführt. Sie können nun den „xbAV-Berater“ über die Desktop-Verknüpfung starten."
                : "Die Aktualisierung des xbAV-Beraters war leider nicht erfolgreich. Bitte wenden Sie sich an unseren Support unter 0681 2107380.";

            if (!success && exception != null)
            {
                message = $"{message}\r\n\r\nSystemmeldung: {exception.Message}";
            }

            MessageBox.Show(message, "xbAV-Berater Update", MessageBoxButtons.OK, success ? MessageBoxIcon.Asterisk : MessageBoxIcon.Exclamation);
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

        private bool DoCheck(IFix fix, ref int defectsCount, ref int repairedCount)
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
                    return false;
                }

                if(result == true)
                {
                    Console.WriteLine("OK");
                    return true;
                }

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
                        return true;
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

                return false;
            }

            return true;
        }

        private static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
