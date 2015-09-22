using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Eulg.Tools.SqliteDbPassDecryptor
{
    public class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2 || args[0] == "-h" || args[0] == "--help")
            {
                PrintUsageInfo();
                return;
            }

            DecryptPasswords(args[0], args[1]);
            Console.WriteLine();
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }

        private static void PrintUsageInfo()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine();
            Console.WriteLine("     decrypt UserSettingsKey username");
            Console.WriteLine();
            Console.WriteLine("     where:");
            Console.WriteLine();
            Console.WriteLine("     UserSettingsKey - UserSettingsKey value from Branding.xml");
            Console.WriteLine("     username - eulg agency username");
            Console.WriteLine();
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }

        private static void DecryptPasswords(string buildTag, string user)
        {
            Console.WriteLine("database id                           password");
            Dictionary<string, byte[]> dbs = LoadPasswordsFromRegistry(buildTag, user);
            if (dbs == null)
            {
                Console.WriteLine("No database keys found in registry");
                return;
            }
            foreach (KeyValuePair<string, byte[]> db in dbs)
            {
                byte[] keyData = ProtectedData.Unprotect(db.Value, null, DataProtectionScope.CurrentUser);
                Console.WriteLine("{0}  {1}", db.Key, Encoding.UTF8.GetString(keyData));
            }
        }

        private static Dictionary<string, byte[]> LoadPasswordsFromRegistry(string buildTag, string user)
        {
            using (RegistryKey regRoot = Registry.CurrentUser.OpenSubKey(@"Software\EULG Software GmbH", false))
            {
                if (regRoot == null)
                {
                    return null;
                }

                using (RegistryKey regKeyBuildTag = regRoot.OpenSubKey(buildTag, false))
                {
                    if (regKeyBuildTag == null)
                    {
                        return null;
                    }
                    using (RegistryKey regKeyUser = regKeyBuildTag.OpenSubKey(@"Account\" + user))
                    {
                        if (regKeyUser == null)
                        {
                            return null;
                        }

                        string[] subKeyNames = regKeyUser.GetSubKeyNames();
                        Dictionary<string, byte[]> dbsDict = new Dictionary<string, byte[]>(2);
                        foreach (string subKeyName in subKeyNames)
                        {
                            if (IsGuid(subKeyName))
                            {
                                using (RegistryKey dbKey = regKeyUser.OpenSubKey(subKeyName, false))
                                {
                                    byte[] pass = dbKey.GetValue("DbPassword") as byte[];
                                    dbsDict.Add(subKeyName, pass);
                                }
                            }
                        }
                        return dbsDict;
                    }
                }
            }
        }

        public static bool IsGuid(string expression)
        {
            if (expression == null || expression.Length < 36)
            {
                return false;
            }
            const string REGEXP = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$";
            Regex guidRegEx = new Regex(REGEXP);

            return guidRegEx.IsMatch(expression);
        }
    }
}
