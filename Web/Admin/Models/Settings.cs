using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Admin.Models
{
    public class Settings
    {
        public static Settings Current { get; set; }

        [XmlElement]
        public ConfigType Config { get; set; }
        public class ConfigType
        {
            [XmlAttribute]
            public bool TestServer { get; set; }
            [XmlAttribute]
            public string PluginDirectory { get; set; }
            [XmlAttribute]
            public string Theme { get; set; }
            [XmlAttribute]
            public string SsoCookieDomain { get; set; }
            [XmlAttribute]
            public string SsoCookieName { get; set; }
            [XmlAttribute]
            public bool CheckMultiLogin { get; set; }
            [XmlAttribute]
            public string Temp { get; set; }
            [XmlAttribute]
            public int PasswordStrength { get; set; } = 8;
            [XmlAttribute]
            public bool DisableContentDispositionInline { get; set; }
            [XmlAttribute]
            public bool ValidateAntiForgeryToken { get; set; }
        }

        [XmlElement]
        public DatabaseType Database { get; set; }
        public class DatabaseType
        {
            [XmlIgnore]
            public bool DbReadOnly { get; } = false;
            [XmlAttribute]
            public string ConnectionString { get; set; }
            [XmlAttribute]
            public string ConnectionStringLocalDbForDebug { get; set; }
            [XmlAttribute]
            public string ConnectionStringSqliteForDebug { get; set; }
            [XmlAttribute]
            public string ConnectionStringDwh { get; set; }
        }

        [XmlElement]
        public LoggingType Logging { get; set; }
        public class LoggingType
        {
            [XmlAttribute]
            public string MainLogFile { get; set; }
            [XmlAttribute]
            public string MailLogFile { get; set; }
            [XmlAttribute]
            public string LoginLogFile { get; set; }
            [XmlAttribute]
            public string BeratungLogPath { get; set; }
        }

        [XmlElement]
        public UrlsType Urls { get; set; }
        public class UrlsType
        {
            [XmlAttribute]
            public string UrlPortal { get; set; }
            [XmlAttribute]
            public string UrlVerwaltung { get; set; }
            [XmlAttribute]
            public string UrlService { get; set; }
            [XmlAttribute]
            public string UrlBeratung { get; set; }
            [XmlAttribute]
            public string UrlSchnellrechner { get; set; }
            [XmlAttribute]
            public bool RedirectToHttps { get; set; }
        }

        [XmlElement]
        public MailType Mail { get; set; }
        public class MailType
        {
            [XmlAttribute]
            public bool AllowExternalMails { get; set; }
            [XmlAttribute]
            public string RewriteAddress { get; set; }
            [XmlAttribute]
            public bool UseMailQueue { get; set; }
            [XmlAttribute]
            public string PdfParserFallbackAddress { get; set; }
            [XmlAttribute]
            public string SendMailsBccAddress { get; set; }

            [XmlElement]
            public SmtpType Smtp { get; set; }
            public class SmtpType
            {
                [XmlAttribute]
                public string Host { get; set; }
                [XmlAttribute]
                public int Port { get; set; }
                [XmlAttribute]
                public bool EnableSsl { get; set; }
                [XmlAttribute]
                public string UserName { get; set; }
                [XmlAttribute]
                public string Password { get; set; }
                [XmlAttribute]
                public string FromName { get; set; }
                [XmlAttribute]
                public string FromEmail { get; set; }
            }

            [XmlElement]
            public Pop3Type Pop3 { get; set; }
            public class Pop3Type
            {
                [XmlIgnore]
                public string UserName { get; set; }
                [XmlIgnore]
                public string Password { get; set; }
                [XmlAttribute]
                public string Host { get; set; }
                [XmlAttribute]
                public int Port { get; set; }
                [XmlAttribute]
                public bool UseSsl { get; set; }
            }
        }

        [XmlElement]
        public AgencyBackupPath AgencyBackup { get; set; }
        public class AgencyBackupPath
        {
            [XmlAttribute]
            public string Path { get; set; }
        }

        [XmlArray("LoginFilter")]
        [XmlArrayItem(typeof(string), ElementName = "Allowed")]
        public List<string> LoginFilterAllowed { get; set; }

        #region Read/Write
        public static Settings Read(string filename)
        {
            using(var fileStream = File.OpenRead(filename))
            {
                var xmlSerializer = new XmlSerializer(typeof(Settings));
                return xmlSerializer.Deserialize(fileStream) as Settings;
            }
        }

        public void Write(string filename)
        {
            using(var fileStream = File.Open(filename, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(typeof(Settings));
                xmlSerializer.Serialize(fileStream, this);
            }
        }
        #endregion

        #region Defaults

        public static Settings GetDefault()
        {
            var settings = new Settings
            {
                Config = new ConfigType
                {
                    TestServer = true,
                },
                Database = new DatabaseType
                {
                    ConnectionString = @"Data Source=staging.xbAV-Berater.de;Initial Catalog=test_staging;User Id=eulgweb;Password=eulgweb;Connect Timeout=1200",
                },
                Logging = new LoggingType
                {
                    MainLogFile = Path.Combine(Path.GetTempPath(), "Eulg.log"),
                    MailLogFile = Path.Combine(Path.GetTempPath(), "EulgMail.log"),
                    LoginLogFile = Path.Combine(Path.GetTempPath(), "EulgLogin.log"),
                },
                Urls = new UrlsType
                {
                    UrlPortal = @"http://portal.eulg.de",
                    UrlVerwaltung = @"https://vermittlerbereich.xbav-berater.de",
                    RedirectToHttps = false,
                },
                Mail = new MailType
                {
                    AllowExternalMails = false,
                    UseMailQueue = false,
                    PdfParserFallbackAddress = "hans-peter.bremer@xbav.de",
                    RewriteAddress = "hans-peter.bremer@xbav.de",
                    SendMailsBccAddress = String.Empty,
                    Smtp = new MailType.SmtpType
                    {
                        Host = "mail.eulg.de",
                        Port = 25,
                        UserName = "noreply@eulg.de",
                        Password = "6KW7Bggt",
                        FromEmail = "noreply@eulg.de",
                        FromName = "xbAV-Berater",
                        EnableSsl = false
                    }
                }
            };
            return settings;
        }

        #endregion

    }
}
