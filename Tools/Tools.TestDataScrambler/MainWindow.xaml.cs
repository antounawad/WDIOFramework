﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Tools.TestDataScrambler
{
    public partial class MainWindow
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
            public string AddressIdFieldName { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            TextBoxConnectionString.Text = @"Data Source=(LocalDB)\MSSqlLocalDB;Initial Catalog=eulgtest;Integrated Security=True;Connect Timeout=3600";
#else
            TextBoxConnectionString.Text = @"Data Source=localhost;Initial Catalog=eulgtest;User Id=eulgweb;Password=eulgweb;Connect Timeout=3600";
#endif
        }

        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            var connectionString = TextBoxConnectionString.Text;

            using (var conn = new SqlConnection(connectionString))
            {
                var t1 = $"Datenbank '{conn.Database}' auf Server '{conn.DataSource}' scramblen?";
                if (MessageBox.Show(t1, "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
            }

            ButtonStart.IsEnabled = false;
            var t = new Thread(() =>
            {
                try
                {
                    DoIt(connectionString);
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        ButtonStart.IsEnabled = true;
                        ProgressBar.IsIndeterminate = false;
                        ProgressBar.Value = 100;
                    });
                }
            });
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
                                 AddressIdFieldName = "Address_ID",
                                 Fields = new List<DbField>
                                          {
                                              new DbField {Name = "degree", Type = typeof(string)},
                                              new DbField {Name = "name", Type = typeof(string)},
                                              new DbField {Name = "surname", Type = typeof(string)},
                                              new DbField {Name = "birthdate", Type = typeof(DateTime)},
                                              new DbField {Name = "telephone", Type = typeof(string)},
                                              new DbField {Name = "fax", Type = typeof(string)},
                                              new DbField {Name = "mobilephone", Type = typeof(string)},
                                              new DbField {Name = "company", Type = typeof(string)},
                                          }
                             },
                             new DbTable
                             {
                                 Name = "AddressMenge",
                                 AddressIdFieldName = "ID",
                                 Fields = new List<DbField>
                                          {
                                              new DbField {Name = "street", Type = typeof(string)},
                                          },
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

                #region Demo-Agenturen

                var excludeAddressIDs = new List<Guid>(); // Daten von Demo-Agenturen und VRs sollen nicht gescrambelt werden
                var sqlCommand = "SELECT Address_ID" +
                                 "  FROM AgencyMenge" +
                                 " WHERE AgencyCustomerType = 3" +
                                 "    OR Address_ID IN (SELECT Address_ID from VRMenge)";
                using (var rdr = new SqlCommand(sqlCommand, conn).ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        excludeAddressIDs.Add(rdr.GetFieldValue<Guid>(0));
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
                    if (m.EndsWith("eulg.de", StringComparison.InvariantCultureIgnoreCase)
                        || m.EndsWith("xbav.de", StringComparison.InvariantCultureIgnoreCase)
                        || m.EndsWith("xbav-berater.de", StringComparison.InvariantCultureIgnoreCase)
                        || m.EndsWith("entgeltumwandler.de", StringComparison.InvariantCultureIgnoreCase)
                        || m.EndsWith("ks-software.de", StringComparison.InvariantCultureIgnoreCase)
                        || m.EndsWith("example.com", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    var prefix = string.Empty;
                    for (var i = 0; i < 8; i++)
                    {
                        prefix += (char)_rng.Next('a', 'z');
                    }

                    var n = $"{prefix}@example.com";
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
                        if (!string.IsNullOrEmpty(table.AddressIdFieldName) && excludeAddressIDs.Contains(row.Field<Guid>(table.AddressIdFieldName))) continue;

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



                #region Agentur-Namen

                // dadurch dass die ContactMenge angepasst wurde, stimmt der Agenturnamen nicht mehr mit dem zugewiesenen Kontakt überein => anpassen
                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = "Passe Agentur-Namen an";
                    ProgressBar.IsIndeterminate = true;
                });

                sqlCommand =
                    "UPDATE ag SET ag.name = (SELECT COALESCE(c.name + ' ' + c.surname, a.name) FROM ContactMenge c, AgencyMenge a " +
                                               "WHERE c.Address_Id = a.Address_Id AND a.Address_Id = ag.Address_Id)" +
                    "  FROM AgencyMenge ag";
                new SqlCommand(sqlCommand, conn).ExecuteNonQuery();

                #endregion



                #region Dokumente

                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = $"Lösche Dokumente";
                    ProgressBar.IsIndeterminate = true;
                });


                sqlCommand = "TRUNCATE TABLE ElanDocuments";
                new SqlCommand(sqlCommand, conn).ExecuteNonQuery();
                
                sqlCommand = "TRUNCATE TABLE ConsultationCustomDocumentValues";
                new SqlCommand(sqlCommand, conn).ExecuteNonQuery();

                sqlCommand = "TRUNCATE TABLE ChangeFormMenge";
                new SqlCommand(sqlCommand, conn).ExecuteNonQuery();

                sqlCommand = "TRUNCATE TABLE DocumentData";
                new SqlCommand(sqlCommand, conn).ExecuteNonQuery();

                sqlCommand = "DELETE FROM DocumentMenge";
                new SqlCommand(sqlCommand, conn).ExecuteNonQuery();

                #endregion



                Dispatcher.Invoke(() =>
                {
                    LabelStatus.Content = "Lösche sonstige Tabellen";
                    ProgressBar.IsIndeterminate = true;
                });

                sqlCommand = "TRUNCATE TABLE AuditLog";
                new SqlCommand(sqlCommand, conn).ExecuteNonQuery();

				sqlCommand = "TRUNCATE TABLE MailQueue";
                new SqlCommand(sqlCommand, conn).ExecuteNonQuery();



                var t = string.Format("ALTER DATABASE {0} SET RECOVERY SIMPLE WITH NO_WAIT;" + Environment.NewLine
                    + "DBCC SHRINKDATABASE(N'{0}', 0);" + Environment.NewLine
                    + "DBCC SHRINKDATABASE(N'{0}', TRUNCATEONLY);" + Environment.NewLine
                    + "-- ALTER DATABASE {0} SET RECOVERY FULL WITH NO_WAIT; (nicht mehr nötig)" + Environment.NewLine
                    + "GO", conn.Database);

                conn.Close();
                Dispatcher.Invoke(() =>
                {
                    Clipboard.Clear();
                    Clipboard.SetText(t);

                    LabelStatus.Content = "Habe fertig.";
                    MessageBox.Show("Bitte jetzt folgendes Script ausführen: " + Environment.NewLine + t + Environment.NewLine + Environment.NewLine + "(ist in der Zwischenablage)", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
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


        #region Documente konvertieren

        public long DocumentsCompleted;
        public long DocumentsTotal;
        private void ButtonDocConvert_Click(object sender, RoutedEventArgs e)
        {
            var connectionString = TextBoxConnectionString.Text;

            using (var conn = new SqlConnection(connectionString))
            {
                var t1 = $"Dokumente in Datenbank {conn.Database} auf Server {conn.DataSource} konvertieren?";
                if (MessageBox.Show(t1, "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
            }

            ButtonStart.IsEnabled = false;
            ButtonDocConvert.IsEnabled = false;
            var t = new Thread(() =>
            {
                try
                {
                    var documents = GetDocumentList(connectionString);
                    DocumentsTotal = documents.Count;

                    Parallel.ForEach(documents, new ParallelOptions { MaxDegreeOfParallelism = 6 }, doc =>
                    {
                        var id = doc.Item1;
                        var useInsert = doc.Item2;

                        using (var conn = new SqlConnection(connectionString))
                        {
                            conn.Open();

                            byte[] data;
                            using (var cmdRead = new SqlCommand("SELECT [data] FROM [dbo].[DocumentMenge] WHERE ID=@ID", conn))
                            {
                                cmdRead.Parameters.AddWithValue("@ID", id);
                                data = Convert.FromBase64String((string)cmdRead.ExecuteScalar());
                            }

                            var command = useInsert
                                ? "INSERT INTO [dbo].[DocumentData] ([DocumentMenge_ID], [Data]) VALUES (@ID, @Data)"
                                : "UPDATE [dbo].[DocumentData] SET [Data]=@Data WHERE [DocumentMenge_ID]=@ID";
                            using (var cmdWrite = new SqlCommand(command, conn))
                            {
                                cmdWrite.Parameters.AddWithValue("@ID", id);
                                cmdWrite.Parameters.AddWithValue("@Data", data);
                                cmdWrite.ExecuteNonQuery();
                            }

                            Interlocked.Add(ref DocumentsCompleted, 1);

                            Dispatcher.Invoke(() =>
                            {
                                LabelStatus.Content = $"Konvertiere Dokumente {DocumentsCompleted} von {DocumentsTotal}";
                                ProgressBar.IsIndeterminate = false;
                                ProgressBar.Value = 100d * DocumentsCompleted / DocumentsTotal;
                            });
                        }
                    });

                    var maxTimestamp = documents.Max(d => d.Item3);
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (var command = new SqlCommand("DELETE FROM [dbo].[DBConfigMenge] WHERE [sysname]='ts_document_conversion'", conn))
                        {
                            command.ExecuteNonQuery();
                        }
                        using(var command = new SqlCommand("INSERT INTO [dbo].[DBConfigMenge] ([sysname], [value]) VALUES ('ts_document_conversion', @timestamp)", conn))
                        {
                            command.Parameters.Add(new SqlParameter("@timestamp", SqlDbType.VarChar) { Value = maxTimestamp.ToString() });
                            command.ExecuteNonQuery();
                        }
                    }
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        ButtonStart.IsEnabled = true;
                        ButtonDocConvert.IsEnabled = true;
                        ProgressBar.IsIndeterminate = false;
                        ProgressBar.Value = 100;
                    });
                }
            });
            t.Start();
        }

        private static List<Tuple<Guid, bool, ulong>> GetDocumentList(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                ulong timestamp;
                using (var command = new SqlCommand("SELECT [value] FROM [dbo].[DBConfigMenge] WHERE [sysname]='ts_document_conversion'", conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        timestamp = reader.Read() ? ulong.Parse(reader.GetString(0)) : 0;
                    }
                }

                var docIds = new List<Tuple<Guid, bool, ulong>>();
                using (var command = new SqlCommand("SELECT TOP 5000 d.[ID], dd.[DocumentMenge_ID], t.[__LastChange] " +
                                                    "FROM [dbo].[DocumentMenge] d JOIN [sync].[DocumentMenge] t ON d.[ID]=t.[ID] LEFT OUTER JOIN [dbo].[DocumentData] dd ON d.[ID]=dd.[DocumentMenge_ID] " +
                                                    "WHERE d.[deleted]=0 AND d.[data] IS NOT NULL AND LEN(d.[data])>0 AND (dd.[DocumentMenge_ID] IS NULL OR t.[__LastChange]>@timestamp) " +
                                                    "ORDER BY t.[__LastChange] ASC", conn))
                {
                    command.Parameters.Add(new SqlParameter("@timestamp", SqlDbType.Binary) { Value = ToBinary(timestamp) });

                    using (var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            docIds.Add(Tuple.Create(reader.GetGuid(0), reader.IsDBNull(1), ToInteger(reader.GetFieldValue<byte[]>(2))));
                        }
                    }
                }

                return docIds;
            }
        }

        private static byte[] ToBinary(ulong timestamp)
        {
            var result = new byte[8];
            for(var n = 0; n < 8; ++n)
            {
                result[n] = (byte)((timestamp & (0xfful << 8 * (7 - n))) >> 8 * (7 - n));
            }
            return result;
        }

        private static ulong ToInteger(byte[] timestamp)
        {
            if(timestamp == null || timestamp.Length != 8)
            {
                throw new ArgumentException();
            }

            ulong result = 0x0;
            for(var n = 7; n >= 0; --n)
            {
                result |= (ulong)timestamp[7 - n] << n * 8;
            }

            return result;
        }

        #endregion
    }
}
