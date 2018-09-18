using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Test_Administration
{

    public class PRC
    {
        public int PID { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
    }

    public class Init
    {
        public string rootPath;
        public string testSuite;
        public string channel;
        public string test;
        public string testConfig;
        public string domaene;
        public string appCall;
        public int port;
        public string confFile;
        public string confPath;
        public string portFile;
        public bool singleCall;
        public string driver;

        public Init(string[] args)
        {
            rootPath = args[0];
            testSuite = args[1];
            channel = args[2];
            test = args[3];
            testConfig = args[4];
            domaene = args[5];
            if (args.Length >= 7)
            {
                driver = args[6];
            }
            if (args.Length <= 8)
            {
                appCall = args[7];
            }
            port = GetAvailablePort(4000);
            confPath = rootPath + "\\" + testSuite + "\\" + channel + "\\" + test + "\\" + testConfig;
            portFile = confPath + "\\" + "UsedPort.txt";
            confFile = confPath + "\\wdio.conf.js";
            singleCall = string.IsNullOrEmpty(appCall);
        }

        private int GetAvailablePort(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;

        }
    }


    class Program
    {
        private const string OnlySelenium = "OnlySelenium";
        private const string OnlyStart = "OnlyStart";

        public static void Main(string[] args)
        {
            Init init = null;
            try
            {
                // Lesen der Argumente
                init = new Init(args);

                // Falls nur Selenium gestartet werden soll oder alles auf einmal gemacht werden soll (zu Testzwecken auf lokalem PC...)
                // darf die PortDatei nicht existieren.
                if ((init.singleCall || init.appCall == OnlySelenium) && File.Exists(init.portFile))
                {
                    throw new Exception("UsedPort File exist.");
                }
                if (init.appCall == OnlyStart && !File.Exists(init.portFile))
                {
                    throw new Exception("Only Start and UsedPort File not exist.");
                }

                string app = "";
                string appParams = "";
                ProcessStartInfo startInfo = null;

                // Schreiben der wdio conf, nur bei SelenimStart oder SingleCall
                if (init.appCall == OnlySelenium || init.singleCall)
                {
                    var wdioConfLines = File.ReadAllLines(init.confFile).ToList();
                    var lineIndex = wdioConfLines.FindIndex(p => !p.StartsWith("//") && p.Contains("\tport:"));
                    if (lineIndex >= 0)
                    {
                        wdioConfLines[lineIndex] = "\tport:" + init.port.ToString() + ",";
                    }
                    File.WriteAllLines(init.confFile, wdioConfLines);

                    // Portfile schreiben
                    string[] usedPort = new string[] { init.port.ToString() };
                    File.WriteAllLines(init.portFile, usedPort);

                    // Selenim Server auf dem freien Port starten
                    if (init.driver == "firefox")
                    {
                        app = init.rootPath + "\\selenium\\seleniumStart.bat";
                    }
                    else if(init.driver == "iexplorer")
                    {
                        app = init.rootPath + "\\selenium\\iedriverstart.bat";
                    }
                    appParams = init.port + " " + init.rootPath + "\\Driver";

                    startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = app;
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                    startInfo.Arguments = appParams;


                    using (Process exeProcessSel = Process.Start(startInfo))
                    {
                        if (init.appCall == OnlyStart)
                        {
                            exeProcessSel.Close();
                        }
                    }


                    // Falls nur Selenium gestartet werden soll, wird die Anwendung beendet;
                    if (init.appCall == OnlySelenium)
                    {
                        return;
                    }

                }
                else if (init.appCall == OnlyStart)
                {
                    // Andernfalls wurde der Port bereits vergeben (JenkinsCall OnlySelenium und muss aus portfile herausgelesen werden...
                    init.port = Convert.ToInt32(File.ReadAllText(init.portFile));
                    Console.WriteLine("Used Port: " + init.port);
                }

                startInfo = new ProcessStartInfo();
                // Diese Unterscheidung musste ich leider machen wegen Jenkins. Ansonsten konnte ich den StandardOutput nicht umleiten und hatte im Jenkins somit keine Logausgabe.
                if (init.appCall == OnlyStart)
                {
                    startInfo.RedirectStandardOutput = true;
                    startInfo.UseShellExecute = false;
                    app = "start.bat";
                    appParams = init.channel + " " + init.test + " " + init.testConfig + " " + init.domaene;
                }
                else
                {
                    startInfo.UseShellExecute = false;
                    app = init.rootPath + "\\node_modules\\.bin\\wdio";
                    appParams = init.confFile + " --" + init.channel + ":" + init.test + ":" + init.testConfig + " --" + init.domaene;
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                }

                startInfo.FileName = app;
                startInfo.Arguments = appParams;
                startInfo.CreateNoWindow = false;

                using (Process exeProcess = Process.Start(startInfo))
                {
                    if (init.appCall == OnlyStart)
                    {
                        Console.WriteLine(exeProcess.StandardOutput.ReadToEnd());
                    }
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (init?.appCall != OnlySelenium && init?.port >= 4000)
                {
                    var processes = GetAllProcesses(init.port.ToString());
                    if (processes.Any(p => p.Port == init.port))
                        try
                        {
                            Process.GetProcessById(processes.First(p => p.Port == init.port).PID).Kill();
                            Console.WriteLine("Process: " + init.port + " is killed...");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            if (File.Exists(init.portFile))
                            {
                                File.Delete(init.portFile);
                            }

                        }
                    else
                    {
                        Console.WriteLine("No process to kill!");
                    }

                    if (File.Exists(init.portFile))
                    {
                        File.Delete(init.portFile);
                    }
                }

            }
            return;
        }


        private static List<PRC> GetAllProcesses(string port)
        {
            var pStartInfo = new ProcessStartInfo();
            pStartInfo.FileName = "netstat.exe";
            pStartInfo.Arguments = "-a -n -o";
            pStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            pStartInfo.UseShellExecute = false;
            pStartInfo.RedirectStandardInput = true;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;

            var process = new Process()
            {
                StartInfo = pStartInfo
            };
            process.Start();

            var soStream = process.StandardOutput;

            var output = soStream.ReadToEnd();
            if (process.ExitCode != 0)
                throw new Exception("somethign broke");

            var result = new List<PRC>();

            var lines = Regex.Split(output, "\r\n").Where(p => p.Contains(":" + port) && p.Contains("ABHÖREN"));
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("Proto"))
                    continue;

                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var len = parts.Length;
                if (len > 1)
                {
                    result.Add(new PRC
                    {
                        Protocol = parts[0],
                        Port = int.Parse(parts[1].Split(':').Last()),
                        PID = int.Parse(parts[len - 1])
                    });
                }
            }
            return result;
        }
    }
}

