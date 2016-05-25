using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;

namespace Tools.TestDataScrambler
{
    public partial class MainWindow : Window
    {
        public class DbField
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public List<object> List = new List<object>();
            public int ListIndex { get; set; }
        }

        public class DbTable
        {
            public string Name { get; set; }
            public List<DbField> Fields = new List<DbField>();
            public List<string> EmptyFields = new List<string>();
            public string ExcludeDemoAgencyField { get; set; }
            public string ExcludeDemoVnField { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            TextBoxConnectionString.Text = @"Data Source=(LocalDB)\MSSqlLocalDB;Initial Catalog=eulgtest;Integrated Security=True;Connect Timeout=1200";
#else
            TextBoxConnectionString.Text = @"Data Source=localhost;Initial Catalog=eulgtest;User Id=eulgweb;Password=eulgweb;Connect Timeout=1200";
#endif
        }

        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            var connectionString = TextBoxConnectionString.Text;

            using (var conn = new SqlConnection(connectionString))
            {
                var t1 = $"Datenbank {conn.Database} auf Server {conn.DataSource} scramblen?";
                if (MessageBox.Show(t1, "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
            }

            var t = new Thread(() => { DoIt(connectionString); });
            t.Start();
        }

        private void DoIt(string connectionString)
        {
            #region Init

            var tables = new List<DbTable>
                         {
                             new DbTable
                             {
                                 Name = "ContactMenge",
                                 ExcludeDemoAgencyField = "Address_ID",
                                 ExcludeDemoVnField = "Address_ID",
                                 Fields = new List<DbField>
                                          {
                                              new DbField {Name = "degree", Type = typeof(string)},
                                              new DbField {Name = "name", Type = typeof(string)},
                                              new DbField {Name = "surname", Type = typeof(string)},
                                              new DbField {Name = "birthdate", Type = typeof(DateTime)},
                                              //new DbField {Name = "email", Type = typeof(string)},
                                              new DbField {Name = "telephone", Type = typeof(string)},
                                              new DbField {Name = "fax", Type = typeof(string)},
                                              new DbField {Name = "mobilephone", Type = typeof(string)},
                                              new DbField {Name = "company", Type = typeof(string)},
                                          }
                             },
                             new DbTable
                             {
                                 Name = "AddressMenge",
                                 ExcludeDemoAgencyField = "ID",
                                 ExcludeDemoVnField = "ID",
                                 Fields = new List<DbField>
                                          {
                                              new DbField {Name = "street", Type = typeof(string)},
                                          },
                                 //EmptyFields = {"signature"}
                             }
                         };

            #endregion

            Dispatcher.Invoke(() =>
            {
                LabelStatus.Content = "Datenbank öffnen";
                ProgressBar.IsIndeterminate = true;
            });
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                //new SqlCommand("BEGIN TRAN;", conn).ExecuteNonQuery();

                #region Demo-Agenturen

                var demoAgencies = new List<Guid>();
                using (var rdr = (new SqlCommand("SELECT Address_ID FROM AgencyMenge WHERE AgencyCustomerType=3", conn)).ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        demoAgencies.Add(rdr.GetFieldValue<Guid>(0));
                    }
                }
                var demoVns = new List<Guid>();
                using (var rdr = (new SqlCommand("SELECT v.ID FROM VNMenge v JOIN AgencyMenge a ON a.Address_ID=v.Agency_ID WHERE a.AgencyCustomerType=3", conn)).ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        demoVns.Add(rdr.GetFieldValue<Guid>(0));
                    }
                }

                #endregion

                foreach (var table in tables)
                {
                    foreach (var field in table.Fields)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LabelStatus.Content = $"Lese {field.Name} in Tabelle {table.Name}";
                            ProgressBar.IsIndeterminate = true;
                        });
                        using (var rdr = (new SqlCommand(string.Format("SELECT [{1}] FROM [{0}] WHERE [{1}]<>'' AND [{1}] IS NOT NULL GROUP BY [{1}]", table.Name, field.Name), conn)).ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                //if (!String.IsNullOrEmpty(table.ExcludeDemoAgencyField) && demoAgencies.Contains(rdr.GetFieldValue<Guid>(rdr.GetOrdinal(table.ExcludeDemoVnField)))) continue;
                                //if (!String.IsNullOrEmpty(table.ExcludeDemoVnField) && demoAgencies.Contains(rdr.GetFieldValue<Guid>(rdr.GetOrdinal(table.ExcludeDemoVnField)))) continue;
                                field.List.Add(rdr[0]);
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            LabelStatus.Content = $"Sortiere {field.Name} in Tabelle {table.Name}  ";
                            ProgressBar.IsIndeterminate = true;
                        });
                        Shuffle(field.List);
                    }
                }

                #region E-Mail-Adressen
                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = "E-Mail-Adressen...";
                    ProgressBar.IsIndeterminate = true;
                });
                var daMail = new SqlDataAdapter("SELECT Address_ID, email FROM ContactMenge WHERE email IS NOT NULL AND email<>''", conn);
                new SqlCommandBuilder(daMail);
                var dtMail = new DataTable();
                daMail.Fill(dtMail);
                var daUser = new SqlDataAdapter("SELECT ID, username FROM UserMenge", conn);
                new SqlCommandBuilder(daUser);
                var dtUser = new DataTable();
                daUser.Fill(dtUser);
                foreach (DataRow row in dtMail.Rows)
                {
                    var m = row.Field<string>("email");
                    if(m.EndsWith("eulg.de", StringComparison.InvariantCultureIgnoreCase)
                        || m.EndsWith("xbav.de", StringComparison.InvariantCultureIgnoreCase)
                        || m.EndsWith("entgeltumwandler.de", StringComparison.InvariantCultureIgnoreCase)
                        || m.EndsWith("ks-software.de", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                    var prefix = string.Empty;
                    for (var i = 0; i < 8; i++)
                    {
                        prefix += (char)_rng.Next('a', 'z');
                    }
                    var n = $"{prefix}@service.eulg.de";
                    row.SetField("email", n);
                    foreach (DataRow rowUser in dtUser.Rows)
                    {
                        if (rowUser.Field<string>("username").Equals(m, StringComparison.InvariantCultureIgnoreCase))
                        {
                            rowUser.SetField("username", n);
                            break;
                        }
                    }
                }
                daMail.Update(dtMail);
                daUser.Update(dtUser);
                #endregion

                foreach (var table in tables)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LabelStatus.Content = $"Ändere Tabelle {table.Name}";
                        ProgressBar.IsIndeterminate = true;
                    });

                    var da = new SqlDataAdapter($"SELECT * FROM [{table.Name}]", conn);
                    new SqlCommandBuilder(da);
                    var dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(table.ExcludeDemoAgencyField) && demoAgencies.Contains(row.Field<Guid>(table.ExcludeDemoAgencyField))) continue;
                        if (!string.IsNullOrEmpty(table.ExcludeDemoVnField) && demoAgencies.Contains(row.Field<Guid>(table.ExcludeDemoVnField))) continue;
                        foreach (var field in table.Fields)
                        {
                            if (row.IsNull(field.Name))
                            {
                                continue;
                            }
                            if (field.Type == typeof(string))
                            {
                                if (string.IsNullOrEmpty(row.Field<string>(field.Name)))
                                {
                                    continue;
                                }
                            }
                            row.SetField(field.Name, field.List[field.ListIndex]);
                            field.ListIndex++;
                            if (field.ListIndex >= field.List.Count)
                            {
                                field.ListIndex = 0;
                            }
                        }
                        foreach (var emptyField in table.EmptyFields)
                        {
                            row[emptyField] = null;
                        }
                    }
                    Dispatcher.Invoke(() =>
                    {
                        LabelStatus.Content = $"Speichere Tabelle {table.Name}";
                        ProgressBar.IsIndeterminate = true;
                    });
                    da.Update(dt);
                }

                #region Dokumente Consultation

                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = "Lösche Consultation-Dokumente";
                    ProgressBar.IsIndeterminate = true;
                });

                var daDoc = new SqlDataAdapter("SELECT d.ID FROM DocumentMenge d JOIN ConsultationMenge c ON c.AdviceData_ID = d.Consultation_ID JOIN AgencyMenge a ON a.Address_ID = c.Agency_ID WHERE a.AgencyCustomerType<>3", conn);
                new SqlCommandBuilder(daDoc);
                var dtDoc = new DataTable();
                daDoc.Fill(dtDoc);

                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = $"Lösche Consultation-Dokumente ({dtDoc.Rows.Count:n})";
                    ProgressBar.IsIndeterminate = false;
                });
                for (var i = 0; i < dtDoc.Rows.Count; i++)
                {
                    Dispatcher.Invoke(() => { ProgressBar.Value = 100d * i / dtDoc.Rows.Count; });
                    new SqlCommand($"UPDATE DocumentMenge SET [data]='', notice=null WHERE ID='{dtDoc.Rows[i][0]}'", conn).ExecuteNonQuery();
                    //dtDoc.Rows[i]["data"] = null;
                    //dtDoc.Rows[i]["notice"] = null;
                }
                Dispatcher.Invoke(() => { ProgressBar.IsIndeterminate = true; });

                #endregion

                #region Documente VN

                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = "Lösche VN-Dokumente";
                    ProgressBar.IsIndeterminate = true;
                });

                daDoc = new SqlDataAdapter("SELECT d.ID FROM DocumentMenge d JOIN VnMenge v ON v.ID = d.VN_ID JOIN AgencyMenge a ON a.Address_ID = v.Agency_ID WHERE a.AgencyCustomerType<>3", conn);
                new SqlCommandBuilder(daDoc);
                dtDoc = new DataTable();
                daDoc.Fill(dtDoc);

                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = $"Lösche VN-Dokumente ({dtDoc.Rows.Count:n})";
                    ProgressBar.IsIndeterminate = false;
                });
                for (var i = 0; i < dtDoc.Rows.Count; i++)
                {
                    Dispatcher.Invoke(() => { ProgressBar.Value = 100d * i / dtDoc.Rows.Count; });
                    new SqlCommand($"UPDATE DocumentMenge SET [data]='', notice=null WHERE ID='{dtDoc.Rows[i][0]}'", conn).ExecuteNonQuery();
                }
                Dispatcher.Invoke(() => { ProgressBar.IsIndeterminate = true; });

                #endregion

                #region Documente VN

                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = "Lösche VP-Dokumente";
                    ProgressBar.IsIndeterminate = true;
                });

                daDoc = new SqlDataAdapter("SELECT d.ID FROM DocumentMenge d JOIN VpMenge v ON v.Address_ID = d.VP_ID JOIN AgencyMenge a ON a.Address_ID = v.Agency_ID WHERE a.AgencyCustomerType<>3", conn);
                new SqlCommandBuilder(daDoc);
                dtDoc = new DataTable();
                daDoc.Fill(dtDoc);

                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = $"Lösche VP-Dokumente ({dtDoc.Rows.Count:n})";
                    ProgressBar.IsIndeterminate = false;
                });
                for (var i = 0; i < dtDoc.Rows.Count; i++)
                {
                    Dispatcher.Invoke(() => { ProgressBar.Value = 100d * i / dtDoc.Rows.Count; });
                    new SqlCommand($"UPDATE DocumentMenge SET [data]='', notice=null WHERE ID='{dtDoc.Rows[i][0]}'", conn).ExecuteNonQuery();
                }
                Dispatcher.Invoke(() => { ProgressBar.IsIndeterminate = true; });

                #endregion

                //Dispatcher.Invoke(() => { LabelStatus.Content = "Transaktion übernehmen.."; });
                //new SqlCommand("COMMIT TRAN;", conn).ExecuteNonQuery();

                var t = string.Format("ALTER DATABASE {0} SET RECOVERY SIMPLE WITH NO_WAIT;" + Environment.NewLine
                    + "DBCC SHRINKDATABASE(N'{0}', 0);" + Environment.NewLine
                    + "DBCC SHRINKDATABASE(N'{0}', TRUNCATEONLY);" + Environment.NewLine
                    + "ALTER DATABASE {0} SET RECOVERY FULL WITH NO_WAIT;" + Environment.NewLine
                    + "GO", conn.Database);



                conn.Close();
                Dispatcher.Invoke(() =>
            {
                Clipboard.Clear(); Clipboard.SetText(t);

                LabelStatus.Content = "Habe fertig.";
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = 100;
                MessageBox.Show("Bitte anschliessend folgens Script in Toad ausführen: " + Environment.NewLine + t + Environment.NewLine + "(ist in der Zwischenablage)", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            }

        }

        #region Shuffle

        //public static void Shuffle<T>(IList<T> list)
        //{
        //    RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        //    int n = list.Count;
        //    while (n > 1)
        //    {
        //        byte[] box = new byte[1];
        //        do provider.GetBytes(box);
        //        while (!(box[0] < n * (Byte.MaxValue / n)));
        //        int k = (box[0] % n);
        //        n--;
        //        T value = list[k];
        //        list[k] = list[n];
        //        list[n] = value;
        //    }
        //}
        private static readonly Random _rng = new Random();
        public static void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = _rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion

    }
}
