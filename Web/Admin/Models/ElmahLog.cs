using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Admin.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Eulg.Web.Service.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ElmahLog
    {
        [JsonProperty] public Guid ErrorId { get; set; }
        [JsonIgnore] public string Application { get; set; }
        [JsonProperty] public string ApplicationName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public short? Release { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Type { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Source { get; set; }
        [JsonProperty] public string Message { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string Detail { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string User { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public long? StatusCode { get; set; }
        [JsonIgnore] public DateTime TimeUtc { get; set; }
        [JsonProperty] public DateTime TimeLocal => TimeUtc.ToLocalTime();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public long? Sequence { get; set; }
        [JsonIgnore] public string AllXml { get; set; }
        [JsonProperty] public string ReportedToJira { get; set; }
        [JsonProperty] public int ErrorCount { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, Dictionary<string, string>> DetailSections { get; set; }

        public enum EListFilter { App, Release, StartTime, EndTime }

        [JsonIgnore] public List<SelectOption> Apps => null;

        public static GridDataResult<ElmahLog> GetLogEntries(GridDataRequest request)
        {
            using(var connection = GetConnecion())
            {
                var query = @"SELECT
                    MIN(ee.Sequence) AS MinSequence,
                    COUNT(*) AS ErrorCount,
                    ee.Application,
                    ea.Name,
                    ea.Release,
                    ee.ErrorId,
                    ee.Type,
                    ee.Source,
                    ee.Message,
                    ee.User,
                    ee.StatusCode,
                    ee.ReportedToJira,
                    ee.TimeUtc
                    FROM elmah_error AS ee
                    LEFT JOIN elmah_application AS ea ON ee.Application = ea.ApplicationID
                    WHERE 1=1";

                ApplyFilters(request.Filters, ref query);
                if(!string.IsNullOrEmpty(request.Search)) query += $" AND MATCH (ee.Message, ee.Type, ee.Source, ee.User) AGAINST ('{MySqlHelper.EscapeString(request.Search)}' IN BOOLEAN MODE) ";

                query += @" GROUP BY
                    ee.Application,
                    ea.Name,
                    ea.Release,
                    ee.Type,
                    ee.Source,
                    ee.Message,
                    ee.User,
                    ee.StatusCode,
                    DATE_FORMAT(ee.TimeUtc, '%d/%m/%Y')
                    ORDER BY ee.Sequence DESC;";

                var command = new MySqlCommand(query, connection);
                var reader = command.ExecuteReader();
                var list = new List<ElmahLog>();

                while(reader.Read())
                {
                    list.Add(ReadLogEntry(reader));
                }

                return new GridDataResult<ElmahLog>(list.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize), list.Count);
            }
        }

        public static List<SelectOption> GetApps()
        {
            using(var conn = GetConnecion())
            {
                var cmd = new MySqlCommand("SELECT ApplicationID, Name, A.Release FROM elmah_application A ORDER BY A.Order", conn);  //SELECT E.Application, A.Name FROM elmah_error E LEFT JOIN elmah_application A ON E.Application=A.ApplicationID GROUP BY E.Application
                var rdr = cmd.ExecuteReader();
                var list = new List<SelectOption>();
                while(rdr.Read())
                {
                    list.Add(new SelectOption(rdr.GetString("ApplicationID"), rdr.GetString("Name")));
                }
                return list;
            }
        }
        public static DateTime? GetMaxDate()
        {
            using(var conn = GetConnecion())
            {
                var cmdMaxDate = new MySqlCommand("SELECT MAX(TimeUtc) FROM elmah_error", conn);
                return (cmdMaxDate.ExecuteScalar() as DateTime?)?.Date;
            }
        }
        public static ElmahLog GetLogEntry(Guid id)
        {
            using(var conn = GetConnecion())
            {
                var cmd = new MySqlCommand(@"SELECT
                    MIN(ee.Sequence) AS MinSequence,
                    COUNT(*) AS ErrorCount,
                    ee.Application,
                    ea.Name,
                    ea.Release,
                    ee.ErrorId,
                    ee.Type,
                    ee.Source,
                    ee.Message,
                    ee.User,
                    ee.StatusCode,
                    ee.AllXml,
                    ee.ReportedToJira,
                    ee.TimeUtc
                    FROM elmah_error AS ee
                    LEFT JOIN elmah_application AS ea ON ee.Application = ea.ApplicationID
                    WHERE ee.ErrorId = @ErrorId
                    GROUP BY
                    ee.Application,
                    ea.Name,
                    ea.Release,
                    ee.Type,
                    ee.Source,
                    ee.Message,
                    ee.User,
                    ee.StatusCode,
                    DATE_FORMAT(ee.TimeUtc, '%d/%m/%Y')", conn);

                cmd.Parameters.AddWithValue("@ErrorId", id);
                var rdr = cmd.ExecuteReader();
                if(rdr.HasRows)
                {
                    rdr.Read();
                    return ReadLogEntry(rdr, true);
                }
                throw new Exception($"Log-Eintrag {id} nicht gefunden!");
            }
        }
        public static bool ExportLogEntryToJira(Guid id)
        {
            var logEntry = GetLogEntry(id);
            var doc = new XmlDocument();
            doc.LoadXml(logEntry.AllXml);

            var message = new MailMessage(Settings.Current.Mail.Smtp.FromEmail, "jira@xbav-berater.de", GetJiraMailSubjectFromElmahLog(logEntry), GetJiraMailBodyFromElmahLog(logEntry));

            var smtpClient = new SmtpClient(Settings.Current.Mail.Smtp.Host, Settings.Current.Mail.Smtp.Port)
            {
                Credentials = new NetworkCredential(Settings.Current.Mail.Smtp.UserName, Settings.Current.Mail.Smtp.Password),
                EnableSsl = Settings.Current.Mail.Smtp.EnableSsl
            };

            smtpClient.Send(message);
            //Logging.MailLog(message.Subject, (message.To?.Any() ?? false) ? message.To[0].Address : string.Empty);

            SetReportedToJiraDate(id);

            return true;
        }

        private static string GetJiraMailSubjectFromElmahLog(ElmahLog logEntry)
        {
            return $"Elmah: {logEntry.ApplicationName}: {logEntry.Message.Split(new[] { '\r', '\n' }).FirstOrDefault()}";
        }

        private static string GetJiraMailBodyFromElmahLog(ElmahLog logEntry)
        {
            var stringBuilder = new StringBuilder();
            var logEntryAllXmlDocument = XDocument.Parse(logEntry.AllXml);
            var errorAttributes = logEntryAllXmlDocument.Root.Attributes();
            var serverVariables = logEntryAllXmlDocument.Descendants("serverVariables").Elements("item");

            stringBuilder.AppendFormat("*{0}* \n", logEntry.Message);
            stringBuilder.AppendFormat("[+Elmah-Log+|{0}://{1}/Logging/Show/{2}]\n",
                HttpContext.Current.Request.Url.Scheme,
                HttpContext.Current.Request.Url.Authority,
                logEntry.ErrorId);

            stringBuilder.Append("\\\\\n");
            stringBuilder.Append("\\\\\n");

            string errorAttributeDetail = null;
            foreach(var errorAttribute in errorAttributes)
            {
                if(errorAttribute.Name == "application")
                {
                    stringBuilder.AppendFormat("|*{0}*|*{1}* ({2})|\n", errorAttribute.Name, logEntry.ApplicationName, errorAttribute.Value);
                    continue;
                }

                if(errorAttribute.Name == "message")
                {
                    continue;
                }

                if(errorAttribute.Name == "detail")
                {
                    errorAttributeDetail = errorAttribute.Value;
                    continue;
                }

                stringBuilder.AppendFormat("|*{0}*|{1}|\n", errorAttribute.Name, errorAttribute.Value);
            }

            stringBuilder.Append("\\\\\n");

            if(errorAttributeDetail != null)
            {
                stringBuilder.Append("*Stacktrace:*\n");
                stringBuilder.AppendFormat("{{code}}{0}{{code}}\n", errorAttributeDetail);
            }

            stringBuilder.Append("\\\\\n");

            foreach(var serverVariable in serverVariables)
            {
                stringBuilder.AppendFormat("|*{0}*|{1}|\n",
                    serverVariable.Attribute("name").Value,
                    serverVariable.Element("value").Attribute("string").Value);
            }

            return stringBuilder.ToString();
        }

        private static void SetReportedToJiraDate(Guid id)
        {
            using(var conn = GetConnecion())
            {
                var cmd = new MySqlCommand("UPDATE elmah_error SET ReportedToJira = NOW() WHERE ErrorId=@ErrorId", conn);
                cmd.Parameters.AddWithValue("@ErrorId", id);

                if(cmd.ExecuteNonQuery() == 0)
                {
                    throw new Exception($"Log-Eintrag {id} nicht gefunden!");
                }
            }
        }

        private static ElmahLog ReadLogEntry(MySqlDataReader rdr, bool withXml = false)
        {
            var x = new ElmahLog
            {
                ErrorId = rdr.GetGuid("ErrorId"),
                Application = rdr.GetString("Application"),
                ApplicationName = rdr.IsDBNull(rdr.GetOrdinal("Name")) ? rdr.GetString("Application").Split('/').Last() : rdr.GetString("Name"),
                Release = rdr.IsDBNull(rdr.GetOrdinal("Name")) ? (short?)null : rdr.GetInt16("Release"),
                Type = rdr.GetString("Type"),
                Source = rdr.GetString("Source"),
                Message = rdr.GetString("Message"),
                User = rdr.GetString("User"),
                StatusCode = rdr.GetInt64("StatusCode"),
                TimeUtc = DateTime.SpecifyKind(rdr.GetDateTime("TimeUtc"), DateTimeKind.Utc),
                Sequence = withXml ? rdr.GetInt64("MinSequence") : (long?)null,
                AllXml = withXml ? rdr.GetString("AllXml") : null,
                ErrorCount = rdr.GetInt32("ErrorCount"),
                ReportedToJira = rdr.IsDBNull(rdr.GetOrdinal("ReportedToJira")) ? null : DateTime.SpecifyKind(rdr.GetDateTime("ReportedToJira"), DateTimeKind.Utc).ToString("dd.MM.yyyy HH:mm:ss")
            };
            if(string.IsNullOrWhiteSpace(x.Type)) x.Type = null;
            if(string.IsNullOrWhiteSpace(x.Source)) x.Source = null;
            if(string.IsNullOrWhiteSpace(x.User)) x.User = null;
            if(x.StatusCode == 0) x.StatusCode = null;
            if(x.Release == 0) x.Release = null;
            if(withXml)
            {
                var xmlDoc = XDocument.Parse(x.AllXml);
                x.Detail = xmlDoc.Root?.Attributes().FirstOrDefault(f => f.Name.ToString().Equals("detail"))?.Value;
                x.DetailSections = xmlDoc.Root?.Elements().ToDictionary(e => e.Name.ToString(), e => e.Elements().ToDictionary(f => f.FirstAttribute.Value, f => f.Elements().First().FirstAttribute.Value));
            }
            return x;
        }
        private static MySqlConnection GetConnecion()
        {
#if DEBUG
            var csb = new MySqlConnectionStringBuilder
            {
                Server = "eulg.de",
                Database = "elmah",
                UserID = "elmah",
                Password = "elmah",
                AllowBatch = true
            };
            var conn = new MySqlConnection(csb.ConnectionString);
#else
            var c = System.Configuration.ConfigurationManager.ConnectionStrings["elmah-mysql"].ConnectionString;
            var conn = new MySqlConnection(c);
#endif
            conn.Open();
            return conn;
        }
        private static void ApplyFilters(Dictionary<string, string> filters, ref string query)
        {
            foreach(var filter in filters)
            {
                if(Enum.TryParse(filter.Key, out EListFilter key))
                {
                    switch(key)
                    {
                        case EListFilter.App:
                            query += $" AND ee.Application='{MySqlHelper.EscapeString(filter.Value)}'";
                            break;
                        case EListFilter.Release:
                            if(filter.Value.Equals("True", StringComparison.InvariantCultureIgnoreCase))
                            {
                                query += $" AND ea.Release=1";
                            }
                            break;
                        case EListFilter.StartTime:
                            if(DateTime.TryParse(filter.Value, out var dateTimeStart)) query += $" AND DATE(ee.TimeUtc) >= '{dateTimeStart:yyyy-MM-dd}'";
                            break;
                        case EListFilter.EndTime:
                            if(DateTime.TryParse(filter.Value, out var dateTimeEnd)) query += $" AND DATE(ee.TimeUtc) >= '{dateTimeEnd:yyyy-MM-dd}'";
                            break;
                    }
                }
                else throw new Exception("Unbekannter Filter: " + filter.Key);
            }
        }
    }
}