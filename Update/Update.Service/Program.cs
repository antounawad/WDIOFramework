using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace Eulg.Update.Service
{
    public static class Program
    {
        public static int Main()
        {
            if (Environment.UserInteractive)
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine("EULG Update Service Version {0}", version);
                Console.WriteLine();
                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    var exePath = Assembly.GetExecutingAssembly().Location;
                    if (args[1].Equals("install", StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            Console.WriteLine("Install: {0}", exePath);
                            ManagedInstallerClass.InstallHelper(new[] { exePath });
                            Console.WriteLine("Erfolgreich.");
                            return 0;
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("FEHLER: {0}", exception);
                        }
                        return 1;
                    }
                    if (args[1].Equals("uninstall", StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            Console.WriteLine("Uninstall: {0}", exePath);
                            ManagedInstallerClass.InstallHelper(new[] { "/u", exePath });
                            Console.WriteLine("Erfolgreich.");
                            return 0;
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("FEHLER: {0}", exception);
                        }
                        return 1;
                    }
                    Console.WriteLine("Paramter: install | uninstall");
                }
            }

            ServiceBase[] servicesToRun =
            {
                new EulgUpdateService()
            };
            ServiceBase.Run(servicesToRun);
            return 0;
        }
    }
}
