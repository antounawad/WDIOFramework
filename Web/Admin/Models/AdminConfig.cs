//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data.SqlClient;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.ServiceProcess;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Routing;
//using System.Xml;
//using System.Xml.Serialization;
//using Microsoft.Win32;
//using Newtonsoft.Json;

//namespace Eulg.Web.Service.Models
//{
//    public class AdminConfig
//    {
//        private const string LABEL_COLOR_SUCCESS = "label-success";
//        private const string LABEL_COLOR_ERROR = "label-danger";
//        private const string LABEL_COLOR_WARNING = "label-warning";
//        private const string LABEL_COLOR_DEFAULT = "label-default";


//        private static AdminConfig _current;
//        public static AdminConfig Current
//        {
//            get
//            {
//                var configFile = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "AdminConfig.xml");
//                if(_current?.ConfigDateTime == null || File.GetLastWriteTime(configFile) > _current?.ConfigDateTime) _current = Read(configFile);
//                return _current;
//            }
//        }

//        public static bool IsAdminService { get; set; }

//        public DateTime ConfigDateTime { get; set; }

//        public List<Area> Areas { get; set; } = new List<Area>();

//        [XmlElement]
//        public CommonConfigType CommonConfig { get; set; }

//        public class CommonConfigType
//        {
//            [XmlAttribute]
//            public string Backup { get; set; }

//            [XmlAttribute]
//            public bool BackupCompression { get; set; }

//            [XmlAttribute]
//            public string AppOfflineTemplateFile { get; set; }

//            [XmlAttribute]
//            public string CommonDeployPath { get; set; }
//        }

//        [JsonObject(MemberSerialization.OptIn)]
//        public class Area
//        {
//            [XmlAttribute, JsonProperty]
//            public string Name { get; set; }

//            [XmlAttribute, JsonProperty]
//            public bool IsRelease { get; set; }

//            [XmlAttribute, JsonProperty]
//            public string Theme { get; set; }

//            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
//            public EClientTheme? RealTheme => ThemeHelperBase.GetTheme(Theme);

//            [XmlAttribute]
//            public int SortOrder { get; set; } = 9999;

//            [XmlElement]
//            public DatabaseType MainDB { get; set; }

//            [XmlElement]
//            public DatabaseType PortalDB { get; set; }

//            //[XmlElement]
//            //public DatabaseType TrainingDB { get; set; }
//            //[XmlElement]
//            //public DatabaseType DemoDB { get; set; }

//            //[XmlElement]
//            //public ServiceType SyncService { get; set; }
//            [XmlElement]
//            public ServiceType TaskSchedulerService { get; set; }

//            [XmlElement]
//            public PortalSiteType PortalSite { get; set; }

//            [XmlElement]
//            public VerwaltungSiteType VerwaltungSite { get; set; }

//            [XmlElement]
//            public BeratungSiteType BeratungSite { get; set; }

//            [XmlElement]
//            public SchnellrechnerSiteType SchnellrechnerSite { get; set; }

//            [XmlElement]
//            public ServiceSiteType ServiceSite { get; set; }

//            [XmlElement]
//            public OfflineClientSiteType OfflineClientSite { get; set; }

//            [XmlIgnore, JsonProperty]
//            public List<string> ReadErrors { get; } = new List<string>();
//        }

//        public class DatabaseType
//        {
//            [XmlAttribute]
//            public string Host { get; set; }

//            [XmlAttribute]
//            public string Name { get; set; }

//            [XmlAttribute]
//            public string User { get; set; }

//            [XmlAttribute]
//            public string Password { get; set; }

//            [XmlAttribute]
//            public bool HasSync { get; set; }
//        }

//        public class ServiceType : IConfig
//        {
//            [XmlAttribute]
//            public string ServiceName { get; set; }

//            [XmlAttribute]
//            public string DeployPath { get; set; }

//            [XmlAttribute]
//            public string JenkinsDeployPath { get; set; }

//            [XmlIgnore]
//            public string Path
//            {
//                get
//                {
//                    if(_path == null)
//                    {
//                        try
//                        {
//                            _path = string.Empty;
//                            if(!string.IsNullOrWhiteSpace(ServiceName))
//                            {
//                                _path = GetServicePath(ServiceName);
//                            }
//                        }
//                        catch { }
//                    }
//                    return _path;
//                }
//            }
//            private string _path;

//            [XmlIgnore]
//            public string Name { get => ServiceName; }
//        }

//        public abstract class SiteType : IConfig
//        {
//            [XmlAttribute]
//            public string SiteName { get; set; }

//            [XmlAttribute]
//            public string Path { get; set; }

//            [XmlAttribute]
//            public string DeployPath { get; set; }

//            [XmlAttribute]
//            public string JenkinsDeployPath { get; set; }

//            [XmlIgnore]
//            public bool Available => !string.IsNullOrWhiteSpace(SiteName) && !string.IsNullOrWhiteSpace(Path) && Directory.Exists(Path);

//            [XmlIgnore]
//            public string Name { get => SiteName; }
//        }

//        public abstract class WebSiteType : SiteType
//        {
//            [XmlAttribute]
//            public string Url { get; set; }

//            [XmlAttribute]
//            public string DeployUrl { get; set; }

//            [XmlAttribute]
//            public string JenkinsDeployUrl { get; set; }


//            [XmlIgnore]
//            public bool HasMaintenanceMode { get; set; }

//            [XmlIgnore]
//            public bool HasRk { get; set; }

//            [XmlIgnore]
//            public string UrlSessions
//            {
//                get
//                {
//                    if(this is BeratungSiteType) return $"{Url}/Tools/Sessions";
//                    if(this is SchnellrechnerSiteType) return $"{Url}/Tools/Sessions";
//                    if(this is VerwaltungSiteType) return $"{Url}/Management/Sessions";
//                    if(this is ServiceSiteType) return $"{Url}/Admin/Sessions";
//                    return null;
//                }
//            }
//        }

//        public class PortalSiteType : WebSiteType { public PortalSiteType() { HasMaintenanceMode = true; } }
//        public class BeratungSiteType : WebSiteType { public BeratungSiteType() { HasRk = true; } }
//        public class SchnellrechnerSiteType : WebSiteType { public SchnellrechnerSiteType() { HasRk = true; } }
//        public class VerwaltungSiteType : WebSiteType { }
//        public class ServiceSiteType : WebSiteType { }
//        public class OfflineClientSiteType : SiteType { }

//        #region Read/Write/Demo

//        public static AdminConfig Read(string filename)
//        {
//            try
//            {
//                if(!File.Exists(filename)) return null;
//                using(var fileStream = File.OpenRead(filename))
//                {
//                    var xmlSerializer = new XmlSerializer(typeof(AdminConfig));
//                    var adminConfig = xmlSerializer.Deserialize(fileStream) as AdminConfig ?? new AdminConfig();
//                    foreach(var area in adminConfig.Areas)
//                    {
//                        //area.DemoDB.HasSync = true;
//                        area.MainDB.HasSync = true;
//                        //area.TrainingDB.HasSync = true;
//                        area.PortalDB.HasSync = false;
//                    }
//                    adminConfig.ConfigDateTime = File.GetLastWriteTime(filename);
//                    return adminConfig;
//                }
//            }
//            catch(Exception)
//            {
//                return null;
//            }
//        }
//        public void Write(string filename)
//        {
//            using(var fileStream = File.Open(filename, FileMode.Create))
//            {
//                var xmlSerializer = new XmlSerializer(typeof(AdminConfig));
//                xmlSerializer.Serialize(fileStream, this);
//            }
//        }
//#if DEBUG
//        public static AdminConfig GetDemo()
//        {
//            var c = new AdminConfig
//            {
//                CommonConfig = new CommonConfigType
//                {
//                    Backup = @"C:\backup"
//                }
//            };

//            c.Areas.Add(new Area
//            {
//                Name = "Release",
//                IsRelease = true,
//                Theme = "Eulg",
//                MainDB = new DatabaseType { Name = "eulg", Host = "localhost", User = "eulgweb", Password = "eulgweb" },
//                PortalDB = new DatabaseType { Name = "eulgportal", Host = "localhost", User = "eulgweb", Password = "eulgweb" },
//                TaskSchedulerService = new ServiceType { ServiceName = "EulgTaskScheduler", DeployPath = @"C:\Release\TaskScheduler.Deploy" /*Path = @"C:\Release\TaskScheduler"*/ },
//                PortalSite = new PortalSiteType { SiteName = "Portal", Path = @"C:\Sites\EulgPortal", DeployPath = @"C:\Sites\Deploy\Portal", Url = @"http://www.eulg.de" },
//                VerwaltungSite = new VerwaltungSiteType { SiteName = "Vermittlerbereich", Path = @"C:\Sites\Verwaltung", DeployPath = @"C:\Sites\Deploy\Verwaltung", Url = @"http://verwaltung.eulg.de" },
//                BeratungSite = new BeratungSiteType { SiteName = "Beratung", Path = @"C:\Sites\Beratung", DeployPath = @"C:\Sites\Deploy\Verwaltung", Url = @"http://verwaltung.eulg.de" },
//                SchnellrechnerSite = new SchnellrechnerSiteType() { SiteName = "Schnellrechner", Path = @"C:\Sites\Schnellrechner", DeployPath = @"C:\Sites\Deploy\Schnellrechner", Url = @"http://verwaltung.eulg.de" },
//                ServiceSite = new ServiceSiteType { SiteName = "Service", Path = @"C:\Sites\Service", DeployPath = @"C:\Sites\Deploy\Service", Url = @"http://service.eulg.de" }
//            });

//            c.Areas.Add(new Area
//            {
//                Name = "SI",
//                IsRelease = true,
//                Theme = "Si",
//                MainDB = new DatabaseType { Name = "eulg", Host = "localhost", User = "eulgweb", Password = "eulgweb" },
//                PortalDB = new DatabaseType { Name = "eulgportal", Host = "localhost", User = "eulgweb", Password = "eulgweb" },
//                TaskSchedulerService = new ServiceType { ServiceName = "EulgTaskScheduler", DeployPath = @"C:\Release\TaskScheduler.Deploy" /*Path = @"C:\Release\TaskScheduler"*/ },
//                PortalSite = new PortalSiteType { SiteName = "Portal", Path = @"C:\Sites\EulgPortal", DeployPath = @"C:\Sites\Deploy\Portal", Url = @"http://www.eulg.de" },
//                VerwaltungSite = new VerwaltungSiteType { SiteName = "Vermittlerbereich", Path = @"C:\Sites\Verwaltung", DeployPath = @"C:\Sites\Deploy\Verwaltung", Url = @"http://verwaltung.eulg.de" },
//                BeratungSite = new BeratungSiteType { SiteName = "Beratung", Path = @"C:\Sites\Beratung", DeployPath = @"C:\Sites\Deploy\Verwaltung", Url = @"http://verwaltung.eulg.de" },
//                SchnellrechnerSite = new SchnellrechnerSiteType() { SiteName = "Schnellrechner", Path = @"C:\Sites\Schnellrechner", DeployPath = @"C:\Sites\Deploy\Schnellrechner", Url = @"http://verwaltung.eulg.de" },
//                ServiceSite = new ServiceSiteType { SiteName = "Service", Path = @"C:\Sites\Service", DeployPath = @"C:\Sites\Deploy\Service", Url = @"http://service.eulg.de" }
//            });

//            c.Areas.Add(new Area
//            {
//                Name = "Hotfix",
//                IsRelease = false,
//                MainDB = new DatabaseType { Name = "eulg", Host = "localhost", User = "eulgweb", Password = "eulgweb" },
//                PortalDB = new DatabaseType { Name = "eulgportal", Host = "localhost", User = "eulgweb", Password = "eulgweb" },
//                TaskSchedulerService = new ServiceType { ServiceName = "EulgTaskScheduler", DeployPath = @"C:\Release\TaskScheduler.Deploy" /*Path = @"C:\Release\TaskScheduler"*/ },
//                PortalSite = new PortalSiteType { SiteName = "Portal", Path = @"C:\Sites\EulgPortal", DeployPath = @"C:\Sites\Deploy\Portal", Url = @"http://www.eulg.de" },
//                VerwaltungSite = new VerwaltungSiteType { SiteName = "Vermittlerbereich", Path = @"C:\Sites\Verwaltung", DeployPath = @"C:\Sites\Deploy\Verwaltung", Url = @"http://verwaltung.eulg.de" },
//                BeratungSite = new BeratungSiteType { SiteName = "Beratung", Path = @"C:\Sites\Beratung", DeployPath = @"C:\Sites\Deploy\Verwaltung", Url = @"http://verwaltung.eulg.de" },
//                SchnellrechnerSite = new SchnellrechnerSiteType() { SiteName = "Schnellrechner", Path = @"C:\Sites\Schnellrechner", DeployPath = @"C:\Sites\Deploy\Schnellrechner", Url = @"http://verwaltung.eulg.de" },
//                ServiceSite = new ServiceSiteType { SiteName = "Service", Path = @"C:\Sites\Service", DeployPath = @"C:\Sites\Deploy\Service", Url = @"http://service.eulg.de" }
//            });


//            return c;
//        }
//#endif
//        #endregion

//        #region Site Tools

//        public enum ESiteState
//        {
//            [Description("läuft")]
//            Running,
//            [Description("offline")]
//            Offline,
//            [Description("Wartungsmodus")]
//            Maintenance,
//            [Description("Seite fehlt")]
//            Missing,
//            [Description("unbekannt")]
//            Unknown,
//            [Description("Fehler")]
//            Error
//        }

//        public static ESiteState GetSiteState(SiteType site)
//        {
//            try
//            {
//                if(!Directory.Exists(site.Path)) return ESiteState.Missing;
//                if(File.Exists(Path.Combine(site.Path, "app_offline.htm"))) return ESiteState.Offline;
//                if(File.Exists(Path.Combine(site.Path, "App_Data", "app_maintenance.tag"))) return ESiteState.Maintenance;
//                return ESiteState.Running;
//                //return ESiteState.Unknown;
//            }
//            catch
//            {
//                return ESiteState.Error;
//            }
//        }

//        public static string GetSiteStateClass(ESiteState state)
//        {
//            switch(state)
//            {
//                case ESiteState.Running:
//                    return LABEL_COLOR_SUCCESS;
//                case ESiteState.Maintenance:
//                    return LABEL_COLOR_WARNING;
//                case ESiteState.Offline:
//                    return LABEL_COLOR_ERROR;
//                default:
//                    return LABEL_COLOR_DEFAULT;
//            }
//        }

//        public WebSiteType GetSiteType(string siteName)
//        {
//            foreach(var area in Areas)
//            {
//                if(area.PortalSite.SiteName != null && area.PortalSite.SiteName.Equals(siteName, StringComparison.InvariantCultureIgnoreCase)) return area.PortalSite;
//                if(area.VerwaltungSite.SiteName != null && area.VerwaltungSite.SiteName.Equals(siteName, StringComparison.InvariantCultureIgnoreCase)) return area.VerwaltungSite;
//                if(area.ServiceSite.SiteName != null && area.ServiceSite.SiteName.Equals(siteName, StringComparison.InvariantCultureIgnoreCase)) return area.ServiceSite;
//                if(area.BeratungSite.SiteName != null && area.BeratungSite.SiteName.Equals(siteName, StringComparison.InvariantCultureIgnoreCase)) return area.BeratungSite;
//                if(area.SchnellrechnerSite.SiteName != null && area.SchnellrechnerSite.SiteName.Equals(siteName, StringComparison.InvariantCultureIgnoreCase)) return area.SchnellrechnerSite;
//            }
//            return null;
//        }

//        public static IHtmlString GetSiteLastBackup(SiteType site)
//        {
//            var backupPath = Path.Combine(Current.CommonConfig.Backup, "Sites");
//            var filter = SanitizeFileName($"{site.SiteName}", '_') + "_????????????.zip";
//            if(Directory.Exists(backupPath))
//            {
//                var file = (new DirectoryInfo(backupPath)).GetFiles(filter, SearchOption.TopDirectoryOnly).OrderByDescending(o => o.LastAccessTime).FirstOrDefault();
//                if(file != null)
//                {
//                    return new HtmlString($"<span class='{((DateTime.Now - file.LastWriteTime).TotalHours <= 8 ? "build-uptodate" : "build-outdated")}'>{file.LastWriteTime:dd.MM.yyyy HH:mm} ({HumanSize(file.Length)})</span>");
//                }
//            }
//            return new HtmlString("-");
//        }

//        public void LogRotateSiteBackup(SiteType site)
//        {
//            var files = GetBackupFiles("Sites", SanitizeFileName($"{site.SiteName}", '_') + "_????????????.zip").ToArray();
//            for(var i = 4; i < files.Length; i++)
//            {
//                File.Delete(Path.Combine(CommonConfig.Backup, "Sites", files[i].Name));
//            }
//        }


//        public class BuildInfo
//        {
//            public BuildInfo(string path)
//            {
//                SiteInfo = "<span>?</span>";

//                var file = Path.Combine(path ?? "", "BuildInfo.xml");
//                if(File.Exists(file))
//                {
//                    var xml = new XmlDocument();
//                    xml.Load(file);

//                    Date = xml.SelectSingleNode("/BuildInfo/BuildDate")?.InnerText ?? String.Empty;
//                    Branch = xml.SelectSingleNode("/BuildInfo/Branch")?.InnerText ?? String.Empty;
//                    Commit = xml.SelectSingleNode("/BuildInfo/Commit")?.InnerText ?? String.Empty;
//                    Machine = xml.SelectSingleNode("/BuildInfo/BuildMachineName")?.InnerText ?? String.Empty;

//                    var username = xml.SelectSingleNode("/BuildInfo/UserName")?.InnerText ?? String.Empty;
//                    if(!String.IsNullOrEmpty(username) && username.Contains("\\")) // Backslash aus KSSOFTWARE-Domäne
//                    {
//                        var backslashIndex = username.IndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
//                        username = username.Substring(backslashIndex + 1, username.Length - backslashIndex - 1);
//                    }

//                    Username = username;

//                    OwnerInfo = !string.IsNullOrEmpty(Username) ? Username : Machine;

//                    DateTime dt;
//                    if(DateTime.TryParse(Date, out dt)) Date = $"{dt:dd.MM.yy HH:mm}";
//                    SiteInfo = $"<strong>{Branch}</strong>&emsp;{Date}&emsp;<small>{Commit}&emsp;{OwnerInfo}</small>";
//                }

//                RkInfo = string.Empty;
//                var rkDll = Path.Combine(path ?? "", "App_Data", "Plugins", "RK9999", "Eulg.Plugins.VorfuehrVR.dll");
//                if(File.Exists(rkDll))
//                {
//                    RkInfo = $"RK: <strong>{File.GetLastWriteTime(rkDll):dd.MM.yy HH:mm}</strong>";
//                }
//            }

//            public string Date { get; set; } = String.Empty;
//            public string Branch { get; set; } = String.Empty;
//            public string Commit { get; set; } = String.Empty;
//            public string Machine { get; set; } = String.Empty;
//            public string Username { get; set; } = String.Empty;
//            public string OwnerInfo { get; set; } = String.Empty;

//            public string SiteInfo { get; }
//            public string RkInfo { get; }
//            public IHtmlString GetBuildInfo() => new HtmlString(SiteInfo + "<br />" + RkInfo);
//            public IHtmlString GetSiteInfo() => new HtmlString(SiteInfo);
//            public IHtmlString GetRkInfo() => new HtmlString(RkInfo);
//        }

//        public static IHtmlString GetBuildInfo(string path) => new BuildInfo(path).GetBuildInfo();

//        #endregion

//        #region Service Tools

//        public enum EServiceState
//        {
//            [Description("läuft")]
//            Running,
//            [Description("angehalten")]
//            Stopped,
//            [Description("Dienst fehlt")]
//            Missing,
//            [Description("unbekannt")]
//            Unknown,
//            [Description("Fehler")]
//            Error
//        }
//        public static EServiceState GetServiceState(ServiceType service)
//        {
//            try
//            {
//                if(!ServiceController.GetServices().Any(a => a.ServiceName.Equals(service.ServiceName))) return EServiceState.Missing;
//                using(var svc = new ServiceController(service.ServiceName))
//                {
//                    switch(svc.Status)
//                    {
//                        case ServiceControllerStatus.Running:
//                            return EServiceState.Running;
//                        case ServiceControllerStatus.Stopped:
//                            return EServiceState.Stopped;
//                    }
//                }
//                return EServiceState.Unknown;
//            }
//            catch
//            {
//                return EServiceState.Error;
//            }
//        }
//        public static string GetServiceStateClass(EServiceState state)
//        {
//            switch(state)
//            {
//                case EServiceState.Running:
//                    return LABEL_COLOR_SUCCESS;
//                case EServiceState.Stopped:
//                    return LABEL_COLOR_ERROR;
//                default:
//                    return LABEL_COLOR_DEFAULT;
//            }
//        }
//        public ServiceType GetServiceType(string serviceName)
//        {
//            foreach(var area in Areas)
//            {
//                //if (area.SyncService.ServiceName.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase)) return area.SyncService;
//                if(area.TaskSchedulerService.ServiceName.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase)) return area.TaskSchedulerService;
//            }
//            return null;
//        }
//        public static string GetServiceVersionString(string serviceName)
//        {
//            if(!ServiceController.GetServices().Any(a => a.ServiceName.Equals(serviceName))) return string.Empty;
//            var image = GetServiceImagePath(serviceName);
//            if(!string.IsNullOrEmpty(image) && File.Exists(image))
//            {
//                var fi = new FileInfo(image);
//                return $"{fi.LastWriteTime:dd.MM.yyyy HH:mm}";
//            }
//            return "?";
//        }
//        public static string GetServiceDeployVersionString(ServiceType service)
//        {
//            if(!ServiceController.GetServices().Any(a => a.ServiceName.Equals(service.ServiceName))) return string.Empty;
//            var image = GetServiceImagePath(service.ServiceName);
//            var deployImage = Path.Combine(service.DeployPath ?? "", Path.GetFileName(image));
//            if(!string.IsNullOrEmpty(service.DeployPath) && Directory.Exists(service.DeployPath) && File.Exists(deployImage))
//            {
//                var fi = new FileInfo(deployImage);
//                return $"{fi.LastWriteTime:dd.MM.yyyy HH:mm}";
//            }
//            return "?";
//        }
//        public static IHtmlString GetServiceLastBackup(ServiceType service)
//        {
//            var backupPath = Path.Combine(Current.CommonConfig.Backup, "Services");
//            var filter = SanitizeFileName($"{service.ServiceName}", '_') + "_????????????.zip";
//            if(Directory.Exists(backupPath))
//            {
//                var file = (new DirectoryInfo(backupPath)).GetFiles(filter, SearchOption.TopDirectoryOnly).OrderByDescending(o => o.LastAccessTime).FirstOrDefault();
//                if(file != null)
//                {
//                    return new HtmlString($"<span class='{((DateTime.Now - file.LastWriteTime).TotalHours <= 8 ? "build-uptodate" : "build-outdated")}'>{file.LastWriteTime:dd.MM.yyyy HH:mm} ({HumanSize(file.Length)})</span>");
//                }
//            }
//            return new HtmlString("-");
//        }
//        public void LogRotateServiceBackup(string serviceName)
//        {
//            var files = GetBackupFiles("Services", SanitizeFileName($"{serviceName}", '_') + "_????????????.zip").ToArray();
//            for(var i = 4; i < files.Length; i++)
//            {
//                File.Delete(Path.Combine(CommonConfig.Backup, "Sites", files[i].Name));
//            }

//        }
//        public static string GetServiceImagePath(string serviceName)
//        {
//            var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName);
//            return key?.GetValue("ImagePath").ToString().Trim(' ', '"');
//        }
//        public static string GetServicePath(string serviceName)
//        {
//            return Path.GetDirectoryName(GetServiceImagePath(serviceName));
//        }

//        #endregion

//        #region Database Tools

//        public static MvcHtmlString GetDatabaseLastBackup(string areaName, string databaseName, RequestContext context)
//        {
//            var file = GetBackupFiles("Databases", SanitizeFileName($"{areaName}_{databaseName}", '_') + "_????????????.bak").FirstOrDefault();
//            if(file != null)
//            {
//                var u = new UrlHelper(context);
//                var url = u.Action("DownloadDatabaseBackup", "Admin", new { fileName = Path.GetFileName(file.Name) });
//                return new MvcHtmlString($"<a href='{url}' title='Datenbank-Backup downloaden' class='link {((DateTime.Now - file.LastWriteTime).TotalHours <= 8 ? "build-uptodate" : "build-outdated")}'>{file.LastWriteTime:dd.MM.yyyy HH:mm} ({HumanSize(file.Length)})</a>");
//            }
//            return new MvcHtmlString("-");
//        }
//        public void LogRotateDatabaseBackup(string areaName, string databaseName)
//        {
//            var files = GetBackupFiles("Databases", SanitizeFileName($"{areaName}_{databaseName}", '_') + "_????????????.bak").ToArray();
//            for(var i = 2; i < files.Length; i++)
//            {
//                File.Delete(Path.Combine(CommonConfig.Backup, "Databases", files[i].Name));
//            }
//        }
//        public struct DbSchemaVersion
//        {
//            public string SchemaVersion { get; set; }
//            public DateTime? SyncProvisionTimestamp { get; set; }
//            public string SyncProvisionSchemaVersion { get; set; }
//            public string Err { get; set; }
//        }
//        public static DbSchemaVersion GetDbSchemaVersion(string host, string database, string user, string password)
//        {
//            DbSchemaVersion ver = new DbSchemaVersion();
//            try
//            {
//                var task = Task<DbSchemaVersion>.Factory.StartNew(() =>
//                {
//                    try
//                    {
//                        var csb = new SqlConnectionStringBuilder
//                        {
//                            InitialCatalog = database,
//                            DataSource = host,
//                            UserID = user,
//                            Password = password,
//                            IntegratedSecurity = String.IsNullOrEmpty(user) && String.IsNullOrEmpty(password)
//                        };
//                        using(var conn = new SqlConnection(csb.ConnectionString))
//                        {
//                            conn.Open();
//                            ver.SchemaVersion = conn.ExecuteScalar("SELECT [Value] FROM DBConfigMenge WHERE [sysname]='SchemaVersion';") as string ?? "?";
//                            ver.SyncProvisionSchemaVersion = conn.ExecuteScalar("SELECT [Value] FROM DBConfigMenge WHERE [sysname]='SyncProvisionSchemaVersion';") as string ?? "?";
//                            var t = conn.ExecuteScalar("SELECT [Value] FROM DBConfigMenge WHERE [sysname]='SyncProvisionTimestamp';") as string ?? "?";
//                            if(t.Length >= 14)
//                                ver.SyncProvisionTimestamp = new DateTime(Convert.ToInt32(t.Substring(0, 4)),
//                                    Convert.ToInt32(t.Substring(4, 2)),
//                                    Convert.ToInt32(t.Substring(6, 2)),
//                                    Convert.ToInt32(t.Substring(8, 2)),
//                                    Convert.ToInt32(t.Substring(10, 2)),
//                                    Convert.ToInt32(t.Substring(12, 2)));
//                        }
//                    }
//                    catch
//                    {
//                    }
//                    return ver;
//                });
//                if(!task.Wait(1000))
//                {
//                    ver.Err = "?";
//                }
//            }
//            catch
//            {
//            }
//            return ver;
//        }

//        #endregion

//        public static bool BackupDirectory(string archiveFile, string sourcePath, string[] exclude, string[] includes = null)
//        {
//            var pathToArchive = Path.GetDirectoryName(archiveFile);
//            if(!Directory.Exists(pathToArchive)) Directory.CreateDirectory(pathToArchive);
//            if(File.Exists(archiveFile)) File.Delete(archiveFile);
//            sourcePath = Path.GetFullPath(sourcePath);
//            using(var zipArchive = ZipFile.Open(archiveFile, ZipArchiveMode.Create))
//            {
//                var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
//                foreach(var file in files)
//                {
//                    var relPath = file.Substring(sourcePath.Length + 1);
//                    if(exclude.Any(a => relPath.StartsWith(a, StringComparison.CurrentCultureIgnoreCase))) continue;
//                    zipArchive.CreateEntryFromFile(file, relPath, CompressionLevel.Optimal);
//                }
//                if(includes != null)
//                {
//                    foreach(var include in includes)
//                    {
//                        var path = Path.Combine(sourcePath, include);
//                        files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
//                        foreach(var file in files)
//                        {
//                            var relPath = file.Substring(sourcePath.Length + 1);
//                            var exclRelPath = file.Substring(path.Length + 1);
//                            if(exclude.Any(a => exclRelPath.StartsWith(a, StringComparison.CurrentCultureIgnoreCase))) continue;
//                            zipArchive.CreateEntryFromFile(file, relPath, CompressionLevel.Optimal);
//                        }

//                    }
//                }
//            }
//            return true;
//        }
//        public static string SanitizeFileName(string name, char replaceChar)
//        {
//            return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, replaceChar));
//        }
//        public static string SanitizePathName(string name, char replaceChar)
//        {
//            return Path.GetInvalidPathChars().Aggregate(name, (current, c) => current.Replace(c, replaceChar));
//        }
//        public static string HumanSize(long size)
//        {
//            string[] sizes = { "B", "KB", "MB", "GB" };
//            var order = 0;
//            while(size >= 1024 && order + 1 < sizes.Length)
//            {
//                order++;
//                size = size / 1024;
//            }
//            return $"{size:0.##} {sizes[order]}";
//        }
//        public static IEnumerable<FileInfo> GetBackupFiles(string folder, string filter)
//        {
//            var backupPath = Path.Combine(Current.CommonConfig.Backup, folder);
//            if(Directory.Exists(backupPath))
//            {
//                return (new DirectoryInfo(backupPath)).GetFiles(filter, SearchOption.TopDirectoryOnly).OrderByDescending(o => o.LastAccessTime).ToArray();
//            }
//            return new FileInfo[] { };
//        }
//        public static IHtmlString GetDbInfoFromConnectionString(string connectionString)
//        {
//            if(string.IsNullOrEmpty(connectionString)) return new HtmlString("?");
//            var csb = new SqlConnectionStringBuilder(connectionString);
//            return new HtmlString($"{csb.InitialCatalog}");
//        }
//        public static IHtmlString GetCheck(bool check) => new HtmlString(check ? "<i class=\"fa fa-check-square-o\"></i>" : "<i class=\"fa fa-square-o\"></i>");

//        public interface IConfig
//        {
//            string Name { get; }
//        }

//        private static readonly Dictionary<string, Tuple<DateTime, Settings>> _settingsCache = new Dictionary<string, Tuple<DateTime, Settings>>();
//        public static Settings GetSettings(string configXmlFilename)
//        {
//            Settings settings = null;
//            if(!_settingsCache.TryGetValue(configXmlFilename, out var settingsFromCache)) settingsFromCache = null;
//            if(!File.Exists(configXmlFilename))
//            {
//                if(settingsFromCache != null) _settingsCache.Remove(configXmlFilename);
//                return null;
//            }
//            var lastWriteTime = File.GetLastWriteTime(configXmlFilename);
//            if(settingsFromCache != null && settingsFromCache.Item1.Equals(lastWriteTime)) return settings;
//            settings = Settings.Read(configXmlFilename);
//            _settingsCache[configXmlFilename] = new Tuple<DateTime, Settings>(lastWriteTime, settings);
//            return settings;
//        }
//        public void ReadConfigs()
//        {
//            foreach(var area in Areas)
//            {
//                foreach(var obj in new IConfig[] { area.BeratungSite, area.SchnellrechnerSite, area.VerwaltungSite, area.ServiceSite, area.PortalSite, area.TaskSchedulerService })
//                {
//                    string configXmlFilename = null;
//                    switch(obj)
//                    {
//                        case WebSiteType webSiteType:
//                            configXmlFilename = Path.Combine(webSiteType.Path, "App_Data", "Config.xml");

//                            break;
//                        case ServiceType serviceType:
//                            configXmlFilename = Path.Combine(serviceType.Path, "Config.xml");
//                            break;
//                        default:
//                            continue;
//                    }
//                    var config = GetSettings(configXmlFilename);
//                }
//            }
//        }
//    }
//}
