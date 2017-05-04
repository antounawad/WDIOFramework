using System.Data.SQLite;

namespace Eulg.Tools.QueryPad
{
    public class Database
    {
        public string Filename { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(GetConnectionString());
        }
        public string GetConnectionString()
        {
            var csb = new SQLiteConnectionStringBuilder
                      {
                          DataSource = Filename,
                          FailIfMissing = true,
                          JournalMode = SQLiteJournalModeEnum.Default,
                          ForeignKeys = true,
                          BinaryGUID = false,
                          SyncMode = SynchronizationModes.Normal
                      };
            if (Password != null)
            {
                csb.Password = Password;
            }
            return csb.ConnectionString;
        }

    }
}
