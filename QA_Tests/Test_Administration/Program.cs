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
        public int port;
        public string confpath;

        public Init(string[] args)
        {
            rootPath = args[0];
            testSuite = args[1];
            channel = args[2];
            test = args[3];
            testConfig = args[4];
            domaene = args[5];
            port = GetAvailablePort(4000);
            confpath = rootPath + "\\" + testSuite + "\\" + channel + "\\" + test + "\\" + testConfig + "\\wdio.conf.js";
        }

        private int GetAvailablePort(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            //getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            //getting active udp listeners
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
        public static void Main(string[] args)
        {
            Init init = null;
            try
            {

                init = new Init(args);

                var wdioConfLines = File.ReadAllLines(init.confpath).ToList();
                var lineIndex = wdioConfLines.FindIndex(p => !p.StartsWith("//") && p.Contains("\tport:"));
                if (lineIndex >= 0)
                {
                    wdioConfLines[lineIndex] = "\tport:" + init.port.ToString() + ",";
                }
                File.WriteAllLines(init.confpath, wdioConfLines);

                string app = init.rootPath + "\\selenium\\seleniumStart.bat";
                string appParams = init.port + " " + init.rootPath + "\\Driver";

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = true;
                startInfo.FileName = app;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.Arguments = appParams;

                // Start the process with the info we specified.
                // Call WaitForExit and then the using-statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    //exeProcess.WaitForExit();
                    exeProcess.Start();
                }

                app = init.rootPath + "\\node_modules\\.bin\\wdio";
                appParams = init.confpath + " --" + init.channel + ":" + init.test + ":" + init.testConfig + " --" + init.domaene;

                startInfo = new ProcessStartInfo();
                startInfo.FileName = app;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.Arguments = appParams;
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = true;


                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (init?.port >= 4000)
                {
                    var processes = GetAllProcesses(init.port.ToString());
                    if (processes.Any(p => p.Port == init.port))
                        try
                        {
                            Process.GetProcessById(processes.First(p => p.Port == init.port).PID).Kill();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    else
                    {
                        Console.WriteLine("No process to kill!");
                    }
                }

            }
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
                if (len > 2)
                    result.Add(new PRC
                    {
                        Protocol = parts[0],
                        Port = int.Parse(parts[1].Split(':').Last()),
                        PID = int.Parse(parts[len - 1])
                    });


            }
            return result;
        }
    }
}

