using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using Telerik.Windows.Controls;

namespace Eulg.Tools.QueryPad
{
    public static class EulgHelper
    {
        public static void ReadEulgDatabasesFromRegistry(RadTreeView treeView)
        {
            var treeItemEulgRoot = new RadTreeViewItem { Header = "EULG-Datenbanken", IsExpanded = true };
            treeView.Items.Add(treeItemEulgRoot);

            using (var regKeyEulgRoot = Registry.CurrentUser.OpenSubKey(@"Software\EULG Software GmbH", false))
            {
                if (regKeyEulgRoot != null)
                {
                    foreach (var buildTag in regKeyEulgRoot.GetSubKeyNames())
                    {
                        var buildTagPath = buildTag.Replace("EULG_", "EULG-");

                        var treeItemBuildTag = new RadTreeViewItem { Header = buildTag, IsExpanded = true };
                        var queueItem = new RadTreeViewItem { Header = "QueueDB", IsExpanded = true};
                        treeItemBuildTag.Items.Add(queueItem);

                        DbHelper.Databases.Add(new Database
                        {
                            Name = String.Format("{0}: Queue", buildTag),
                            Filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), buildTagPath, "Data", String.Format("{0}.db", "Queue")),
                            Password = null
                        });

                        treeItemEulgRoot.Items.Add(treeItemBuildTag);
                        var regKeyBuildTag = regKeyEulgRoot.OpenSubKey(buildTag, false);
                        if (regKeyBuildTag != null)
                        {
                            var regKeyAccounts = regKeyBuildTag.OpenSubKey("Account", false);
                            if (regKeyAccounts != null)
                            {
                                foreach (var accountName in regKeyAccounts.GetSubKeyNames())
                                {
                                    var accountItem = new RadTreeViewItem { Header = accountName };
                                    treeItemBuildTag.Items.Add(accountItem);
                                    var regKeyAccount = regKeyAccounts.OpenSubKey(accountName, false);
                                    if (regKeyAccount != null)
                                    {
                                        foreach (var dbName in regKeyAccount.GetSubKeyNames())
                                        {
                                            var regKeyDb = regKeyAccount.OpenSubKey(dbName, false);
                                            if (regKeyDb != null)
                                            {
                                                var dbType = regKeyDb.GetValue("Type", "Unknown");
                                                var dbPassword = regKeyDb.GetValue("DbPasswordDebug") as string;
                                                if (String.IsNullOrEmpty(dbPassword))
                                                {
                                                    var b = regKeyDb.GetValue("DbPassword") as byte[];
                                                    if (b != null)
                                                    {
                                                        dbPassword = DecryptPassword(b);
                                                    }
                                                }
                                                
                                                DbHelper.Databases.Add(new Database
                                                                       {
                                                                           Name = String.Format("{0}: {1} {2}", buildTag, accountName, dbType),
                                                                           Filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), buildTagPath, "Data", String.Format("{0}.db", dbName)),
                                                                           Password = dbPassword
                                                                       });
                                                var treeItemDatabase = new RadTreeViewItem { Header = dbType };
                                                accountItem.Items.Add(treeItemDatabase);
                                                
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string DecryptPassword(byte[] bytes)
        {
            var keyData = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(keyData);
        }
    }
}
