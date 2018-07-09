using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Eulg.Common.Graph;
using Npgsql;
using NpgsqlTypes;

namespace DbMigrate
{
    internal static class Migration
    {
        internal static string GetMsSqlConnectionString()
        {
            return new SqlConnectionStringBuilder
            {
                ApplicationIntent = ApplicationIntent.ReadOnly,
                DataSource = "192.168.0.5",
                UserID = "eulgweb",
                Password = "eulgweb",
                InitialCatalog = "test_hotfix"
            }.ConnectionString;
        }
        internal static string GetPostgresConnectionString()
        {
            return new NpgsqlConnectionStringBuilder
            {
                Host = "192.168.0.5",
                Username = "postgres",
                Password = "Fs234fds",
                Database = "test_hotfix",

            }.ConnectionString;
        }

        internal static void DoIt()
        {
            using (var connMsSql = new SqlConnection(GetMsSqlConnectionString()))
            {
                connMsSql.Open();

                // Get Source Tables:
                var tables = new List<string>();
                var cmdTables = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='dbo' ORDER BY TABLE_NAME", connMsSql);
                var rdrTables = cmdTables.ExecuteReader();
                while (rdrTables.Read())
                {
                    tables.Add(rdrTables.GetString(0));
                }
                rdrTables.Close();

                using (var connPg = new NpgsqlConnection(GetPostgresConnectionString()))
                {
                    connPg.Open();
                    connPg.Notification += (o, e) => System.Diagnostics.Debug.WriteLine($"{e.PID}: {e.Condition}: {e.AdditionalInformation}");

                    // Get Destination Tables:
                    var dest_tables = new List<string>();
                    var dest_cmdTables = new NpgsqlCommand("SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname != 'pg_catalog' AND schemaname != 'information_schema' ORDER BY tablename", connPg);
                    var dest_rdrTables = dest_cmdTables.ExecuteReader();
                    while (dest_rdrTables.Read())
                    {
                        dest_tables.Add(dest_rdrTables.GetString(0));
                    }
                    dest_rdrTables.Close();

                    foreach (var table in tables.Where(_ => !_.StartsWith("_", StringComparison.Ordinal)))
                    {
                        if (!dest_tables.Contains(table))
                        {
                            System.Diagnostics.Debug.WriteLine("Missing Table is Postgres: " + table);
                            dest_tables.Remove(table);
                        }
                    }
                    foreach (var table in dest_tables.Where(_ => !_.StartsWith("_", StringComparison.Ordinal)))
                    {
                        if (!tables.Contains(table))
                        {
                            System.Diagnostics.Debug.WriteLine("Missing Table in MsSql: " + table);
                            tables.Remove(table);
                        }
                    }

                    var reihenfolgeLoeschen = GetDependencyTree(connMsSql, tables, ETopSortOrder.SourceToSink).Where(_ => !_.StartsWith("_", StringComparison.Ordinal)).ToArray();
                    foreach (var table in reihenfolgeLoeschen)
                    {
                        System.Diagnostics.Debug.WriteLine($"Truncate: {table}");

                        new NpgsqlCommand($"TRUNCATE TABLE public.\"{table}\" RESTART IDENTITY CASCADE", connPg).ExecuteNonQuery();

                    }

                    var reihenfolgeInsert = GetDependencyTree(connMsSql, tables, ETopSortOrder.SinkToSource).Where(_ => !_.StartsWith("_", StringComparison.Ordinal)).ToArray();
                    foreach (var table in reihenfolgeInsert)
                    {
                        CopyTable(connMsSql, connPg, table);
                    }
                }
            }
        }

        private static void CopyTable(SqlConnection connSource, NpgsqlConnection connDest, string table)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Insert: {table}");
                //new NpgsqlCommand($"ALTER TABLE public.\"{table}\" DISABLE TRIGGER ALL", connDest).ExecuteNonQuery();
                //new NpgsqlCommand($"TRUNCATE TABLE public.\"{table}\" RESTART IDENTITY CASCADE", connDest).ExecuteNonQuery();

                var dest_dataAdapter = new NpgsqlDataAdapter($"SELECT * FROM public.\"{table}\"", connDest);
                //dest_dataAdapter.AcceptChangesDuringFill = false;
                //dest_dataAdapter.AcceptChangesDuringUpdate = false;
                //dest_dataAdapter.FillLoadOption = LoadOption.OverwriteChanges;
                //dest_dataAdapter.ContinueUpdateOnError = false;
                //dest_dataAdapter.MissingMappingAction = MissingMappingAction.Ignore;
                //dest_dataAdapter.MissingSchemaAction = MissingSchemaAction.;

                //dest_dataAdapter.UpdateCommand = new NpgsqlCommand();
                var commandBuilder = new NpgsqlCommandBuilder(dest_dataAdapter);
                commandBuilder.ConflictOption = ConflictOption.OverwriteChanges;
                commandBuilder.SetAllValues = true;



                var dest_dataTable = new DataTable();
                dest_dataAdapter.FillSchema(dest_dataTable, SchemaType.Mapped);
                dest_dataTable.PrimaryKey = null;
                dest_dataTable.Constraints.Clear();

                var cmd = commandBuilder.GetInsertCommand(true);
                //if (cmd.Parameters.Count != dest_dataTable.Columns.Count)
                {
                    for (var cc = 0; cc < dest_dataTable.Columns.Count; cc++)
                    {
                        var co = dest_dataTable.Columns[cc];
                        var pa = cmd.Parameters.FirstOrDefault(a => a.SourceColumn.Equals(co.ColumnName));
                        if (pa == null)
                        {
                            var p = cmd.CreateParameter();
                            //p.DbType
                            p.SourceColumn = co.ColumnName;
                            p.ParameterName = $"@{co.ColumnName}";
                            p.SpecificType = co.DataType;
                            cmd.Parameters.Add(p);
                        }
                        else
                        {
                            pa.SpecificType = co.DataType;
                            //pa.DbType = co.DataType
                        }
                    }
                    //System.Diagnostics.Debug.WriteLine($"Blubb");
                }
                //cmd.Prepare();

                using (var src_cmd = new SqlCommand($"SELECT * FROM {table}", connSource))
                {
                    using (var src_rdr = src_cmd.ExecuteReader())
                    {
                        while (src_rdr.Read())
                        {
                            //var r = dest_dataTable.NewRow();
                            for (var col = 0; col < src_rdr.FieldCount; col++)
                            {
                                //var i = dest_dataTable.Columns[src_rdr.GetName(col)];
                                //r.SetField(i, src_rdr.GetValue(col));

                                //cmd.Parameters.Add(src_rdr.GetName(col), NpgsqlDbType.Unknown)
                                cmd.Parameters[src_rdr.GetName(col)].Value = src_rdr.GetValue(col);
                            }

                            //dest_dataTable.Rows.Add(r);
                            //if (dest_dataTable.Rows.Count >= 250)
                            //{
                            //    dest_dataAdapter.Update(dest_dataTable);
                            //    dest_dataTable.Clear();
                            //}
                            var ii = cmd.ExecuteNonQuery();
                        }

                        src_rdr.Close();
                        //dest_dataAdapter.Update(dest_dataTable);
                    }
                }

                //new NpgsqlCommand($"ALTER TABLE public.\"{table}\" ENABLE TRIGGER ALL", connDest).ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{table}: {e}");
            }
        }

        private static IEnumerable<string> GetDependencyTree(SqlConnection connection, IReadOnlyList<string> tables, ETopSortOrder topSortOrder)
        {
            var cmd = new SqlCommand("SELECT KCU1.TABLE_NAME AS FK_TABLE_NAME,KCU2.TABLE_NAME AS REFERENCED_TABLE_NAME FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU1 ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU2 ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION ;", connection);

            var graph = tables.CreateDirectedGraph();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    var x = rdr.GetString(0);
                    var y = rdr.GetString(1);
                    if (x == y)
                        continue;
                    var dep = graph[x][y];
                    if (!dep.Exists) dep.Add();
                }
            }

            return graph.TopologicalSort(topSortOrder);
        }
    }
}
