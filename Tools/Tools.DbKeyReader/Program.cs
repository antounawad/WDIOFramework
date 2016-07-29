using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Tools.DbKeyReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var tempPath = Path.GetTempPath();
            var outputName = Path.Combine(tempPath, Path.ChangeExtension(Path.GetRandomFileName(), ".txt"));
            var companies = new[] { "KS Software GmbH", "EULG Software GmbH", "xbAV Beratungssoftware GmbH" };

            using(var output = File.Open(outputName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(output))
                {
                    foreach (var company in companies)
                    {
                        using (var softwareKey = Registry.CurrentUser.OpenSubKey($@"Software\{company}"))
                        {
                            if (softwareKey == null)
                            {
                                continue;
                            }

                            writer.WriteLine(company);
                            writer.WriteLine(string.Empty.PadLeft(company.Length, '='));
                            writer.WriteLine();

                            foreach (var installName in softwareKey.GetSubKeyNames())
                            {
                                using (var installKey = softwareKey.OpenSubKey(installName))
                                {
                                    if (!installKey.GetSubKeyNames().Contains("Account"))
                                        continue;

                                    using (var accountKey = installKey.OpenSubKey("Account"))
                                    {
                                        foreach (var userName in accountKey.GetSubKeyNames())
                                        {
                                            using (var userKey = accountKey.OpenSubKey(userName))
                                            {
                                                writer.WriteLine("{0} // {1}", installName, userName);

                                                foreach (var databaseName in userKey.GetSubKeyNames())
                                                {
                                                    try
                                                    {
                                                        using (var databaseKey = userKey.OpenSubKey(databaseName))
                                                        {
                                                            var cipher = (byte[]) databaseKey.GetValue("DbPassword");
                                                            var type = (string) databaseKey.GetValue("Type");
                                                            var password = cipher == null
                                                                ? "(unset)"
                                                                : Encoding.UTF8.GetString(ProtectedData.Unprotect(
                                                                    cipher, null, DataProtectionScope.CurrentUser));
                                                            writer.WriteLine("{0} {1,9} {2}", databaseName, type,
                                                                password);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        writer.WriteLine("{0} {1,9}: {2}", databaseName,
                                                            ex.GetType().Name, ex.Message);
                                                    }
                                                }

                                                writer.WriteLine();
                                            }
                                        }
                                    }
                                }
                            }

                            writer.WriteLine();
                            writer.WriteLine();
                        }
                    }
                }
            }

            try
            {
                Console.WriteLine("{0}", outputName);
                using (var shellExec = Process.Start(outputName))
                {
                    shellExec.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine();
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
            finally
            {
                try
                {
                    Thread.Sleep(100);
                    File.Delete(outputName);
                }
                catch { }
            }
        }
    }
}
