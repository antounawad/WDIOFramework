//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Data.SqlClient;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.ServiceProcess;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using Admin.Helpers;
//using Admin.Models;
//using Eulg.Web.Service.Models;
//using Microsoft.SqlServer.Management.Common;
//using Microsoft.SqlServer.Management.Smo;
////using Microsoft.SqlServer.Management.Common;
////using Microsoft.SqlServer.Management.Smo;

//namespace Eulg.Web.Service.Controllers
//{
//    [Authorize(Roles = "Cockpit")]
//    public class AdminController : Controller
//    {
//        public ActionResult Index()
//        {
//            if(!AdminConfig.IsAdminService) return HttpNotFound("AdminConfig.xml fehlt - dies ist kein Admin-Service! Bitte admin.xbav-berater.de verwenden!");
//            return View();
//        }

//        #region New
//        public ActionResult IndexNew()
//        {
//            if(!AdminConfig.IsAdminService) return HttpNotFound("AdminConfig.xml fehlt - dies ist kein Admin-Service! Bitte admin.xbav-berater.de verwenden!");
//            return View();
//        }
//        public ActionResult ConfigNew()
//        {
//            if(AdminConfig.Current == null) return HttpNotFound("AdminConfig.xml fehlt - dies ist kein Admin-Service! Bitte admin.xbav-berater.de verwenden!");
//            return View();
//        }
//        public JsonResult GetAreas()
//        {
//            return Json(AdminConfig.Current.Areas.OrderBy(_ => _.SortOrder).ThenBy(_ => _.IsRelease ? 0 : 1).ThenBy(_ => _.Name));
//        }
//        #endregion

//        public ActionResult Config()
//        {
//            if(AdminConfig.Current == null) return HttpNotFound("AdminConfig.xml fehlt - dies ist kein Admin-Service! Bitte admin.xbav-berater.de verwenden!");
//            return View();
//        }

//        [HttpPost]
//        public JsonResult SiteAction(string siteName, string action)
//        {
//            var ajaxResponse = new AjaxResponse();
//            try
//            {
//                var site = AdminConfig.Current.GetSiteType(siteName);
//                var offlineFile = Path.Combine(site.Path, "app_offline.htm");
//                var maintenanceFile = Path.Combine(site.Path, "App_Data", "app_maintenance.tag");
//                switch(action.ToLowerInvariant().Trim())
//                {
//                    case "maintenance":
//                        if(!System.IO.File.Exists(maintenanceFile))
//                        {
//                            System.IO.File.WriteAllText(maintenanceFile, "maintenance");
//                            ajaxResponse.Success = true;
//                        }
//                        else
//                        {
//                            ajaxResponse.Message = "Seite ist bereits im Maintenance Modus";
//                        }
//                        break;
//                    case "offline":
//                        if(!System.IO.File.Exists(offlineFile))
//                        {
//                            if(!string.IsNullOrEmpty(AdminConfig.Current.CommonConfig.AppOfflineTemplateFile) && System.IO.File.Exists(AdminConfig.Current.CommonConfig.AppOfflineTemplateFile))
//                                System.IO.File.Copy(AdminConfig.Current.CommonConfig.AppOfflineTemplateFile, offlineFile, true);
//                            else
//                                System.IO.File.WriteAllText(offlineFile, "offline");

//                            ajaxResponse.Success = true;
//                        }
//                        else
//                        {
//                            ajaxResponse.Message = "Seite ist bereits im Maintenance Modus";
//                        }
//                        break;
//                    case "online":
//                        if(System.IO.File.Exists(offlineFile)) { System.IO.File.Delete(offlineFile); ajaxResponse.Success = true; }
//                        if(System.IO.File.Exists(maintenanceFile)) { System.IO.File.Delete(maintenanceFile); ajaxResponse.Success = true; }
//                        if(!ajaxResponse.Success.GetValueOrDefault(false)) ajaxResponse.Message = "Seite ist bereits online!";
//                        break;
//                    case "backup":
//                        var zipFile = Path.Combine(AdminConfig.Current.CommonConfig.Backup, "Sites", AdminConfig.SanitizeFileName($"{siteName}_{DateTime.Now:yyyyMMddHHmm}.zip", '_'));
//                        ajaxResponse.Success = AdminConfig.BackupDirectory(zipFile, site.Path, new[] { @"App_Data\", @"app_offline.htm" }, (site is AdminConfig.BeratungSiteType) ? new[] { @"App_Data\Plugins" } : null);
//                        AdminConfig.Current.LogRotateSiteBackup(site);
//                        break;
//                    default:
//                        throw new Exception("Unbekannte Aktion: " + action);
//                }
//            }
//            catch(Exception exception)
//            {
//                ajaxResponse.Message = exception.GetMessagesTree();
//            }
//            return Json(ajaxResponse);
//        }

//        [HttpPost]
//        public JsonResult ServiceAction(string serviceName, string action)
//        {
//            var ajaxResponse = new AjaxResponse();
//            try
//            {
//                if(!ServiceController.GetServices().Any(a => a.ServiceName.Equals(serviceName))) throw new Exception($"Service nicht gefunden: '{serviceName}'");
//                using(var svc = new ServiceController(serviceName))
//                {
//                    switch(action.ToLowerInvariant().Trim())
//                    {
//                        case "start":
//                            svc.Start();
//                            svc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
//                            ajaxResponse.Success = (svc.Status == ServiceControllerStatus.Running);
//                            break;
//                        case "stop":
//                            svc.Stop();
//                            svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
//                            ajaxResponse.Success = (svc.Status == ServiceControllerStatus.Stopped);
//                            break;
//                        case "backup":
//                            var zipFile = Path.Combine(AdminConfig.Current.CommonConfig.Backup, "Services", AdminConfig.SanitizeFileName($"{serviceName}_{DateTime.Now:yyyyMMddHHmm}.zip", '_'));
//                            ajaxResponse.Success = AdminConfig.BackupDirectory(zipFile, AdminConfig.GetServicePath(serviceName), new[] { @"Logs\", @"sessions\" });
//                            AdminConfig.Current.LogRotateServiceBackup(serviceName);
//                            break;
//                        default:
//                            throw new Exception("Unbekannte Aktion: " + action);
//                    }
//                }
//            }
//            catch(Exception exception)
//            {
//                ajaxResponse.Message = exception.GetMessagesTree();
//            }
//            return Json(ajaxResponse);
//        }

//        public void DatabaseAction(string area, string database, string command)
//        {
//            try
//            {
//                var myArea = AdminConfig.Current.Areas.First(a => a.Name.Equals(area));
//                var myDb = new[] { myArea.MainDB, myArea.PortalDB }.First(w => w.Name.Equals(database));
//                switch(command.ToLowerInvariant().Trim())
//                {
//                    case "backup":
//                        var bakFile = Path.Combine(AdminConfig.Current.CommonConfig.Backup, "Databases", AdminConfig.SanitizeFileName($"{myArea.Name}_{database}_{DateTime.Now:yyyyMMddHHmm}.bak", '_'));
//                        BackupDatabase(myDb.Host, myDb.Name, myDb.Password, myDb.Password, bakFile, AdminConfig.Current.CommonConfig.BackupCompression);
//                        AdminConfig.Current.LogRotateDatabaseBackup(myArea.Name, database);
//                        break;
//                    default:
//                        throw new Exception("Unbekannte Aktion: " + command);
//                }
//            }
//            catch(Exception exception)
//            {
//                Response.Write(exception.GetMessagesTree());
//            }
//        }

//        [HttpPost]
//        public string ExecuteDbScript(string area, string database, string script, bool batch)
//        {
//            var msgs = new List<string>();
//            try
//            {
//                var myArea = AdminConfig.Current.Areas.First(a => a.Name.Equals(area));
//                var myDb = new[] { myArea.MainDB, myArea.PortalDB }.First(w => w.Name.Equals(database));
//                var serverConnection = string.IsNullOrEmpty(myDb.User) ? new ServerConnection(myDb.Host) : new ServerConnection(myDb.Host, myDb.User, myDb.Password);
//                var server = new Microsoft.SqlServer.Management.Smo.Server(serverConnection);
//                var db = new Database(server, database);

//                server.ConnectionContext.StatementExecuted += (sender, args) => { msgs.Add($"<p class='text-muted'>{args.TimeStamp}: {args.SqlStatement}</p>"); };
//                server.ConnectionContext.ServerMessage += (sender, args) => { msgs.Add($"<p class='text-danger'>{args.Error.LineNumber}: {args.Error.Message}</p>"); };
//                server.ConnectionContext.InfoMessage += (sender, args) => { if(batch) msgs.Add($"<p class='text-info'>{args.Message}</p>"); };
//                server.ConnectionContext.StateChange += (sender, args) => { if(batch) msgs.Add($"<p class='text-warning'>{args.CurrentState.ToString()}</p>"); };
//                if(batch)
//                {
//                    db.ExecuteNonQuery(script);
//                }
//                else
//                {
//                    DataSet res = db.ExecuteWithResults(script);
//                    foreach(DataTable table in res.Tables)
//                    {
//                        msgs.Add("<table class='table table-bordered table-condensed table-striped'>");
//                        msgs.Add("<tr>");
//                        foreach(DataColumn col in table.Columns)
//                        {
//                            msgs.Add($"<th>{col.Caption}</th>");
//                        }
//                        msgs.Add("</tr>");
//                        for(var r = 0; r < table.Rows.Count; r++)
//                        {
//                            msgs.Add("<tr>");
//                            for(var c = 0; c < table.Columns.Count; c++)
//                            {
//                                msgs.Add($"<td>{HttpUtility.HtmlEncode(table.Rows[r][c])}</td>");
//                            }
//                            msgs.Add("</tr>");
//                        }
//                        msgs.Add("</tr>");
//                        msgs.Add("</table>");
//                    }
//                }
//            }
//            catch(Exception exception)
//            {
//                msgs.Add($"<p class='text-danger'>{exception.GetMessagesTree()}</p>");
//            }

//            return string.Join(Environment.NewLine, msgs);
//        }

//        [HttpPost]
//        public string DbProvision(string area, string database, EProvisioningTask mode, string schema)
//        {
//            throw new NotImplementedException();
//            //try
//            //{
//            //    var myArea = AdminConfig.Current.Areas.First(a => a.Name.Equals(area));
//            //    var myDb = new[] { myArea.MainDB, myArea.PortalDB }.First(w => w.Name.Equals(database));

//            //    var csb = new SqlConnectionStringBuilder
//            //    {
//            //        InitialCatalog = myDb.Name,
//            //        DataSource = myDb.Host,
//            //        UserID = myDb.User,
//            //        Password = myDb.Password
//            //    };

//            //    using(var conn = new SqlConnection(csb.ConnectionString))
//            //    {
//            //        conn.Open();
//            //        int dbVersion;
//            //        using(var cmd = new SqlCommand("SELECT [value] FROM DbConfigMenge WHERE [sysname]='DbVersion'", conn))
//            //        {
//            //            dbVersion = Convert.ToInt32(cmd.ExecuteScalar());
//            //        }

//            //        var model = DataModel.Load().GetSchemaForVersion(dbVersion);
//            //        var scriptWriter = new ProvisioningScriptWriter(model);

//            //        using(var script = new StringWriter())
//            //        {
//            //            script.WriteLine("USE {0};", myDb.Name);
//            //            script.WriteLine("DELETE FROM DbConfigMenge WHERE [sysname]='SyncProvisionTimestamp';");
//            //            script.WriteLine("DELETE FROM DbConfigMenge WHERE [sysname]='SyncProvisionSchemaVersion';");

//            //            switch(mode)
//            //            {
//            //                case EProvisioningTask.Create:
//            //                    scriptWriter.WriteCreateNewScript(script);
//            //                    script.WriteLine($"INSERT INTO DbConfigMenge ([sysname],[value]) VALUES ('SyncProvisionTimestamp',N'{DateTime.Now:yyyyMMddHHmmss}');");
//            //                    script.WriteLine($"INSERT INTO DbConfigMenge ([sysname],[value]) VALUES ('SyncProvisionSchemaVersion',N'{schema}');");
//            //                    break;
//            //                case EProvisioningTask.Delete:
//            //                    scriptWriter.WriteDeleteScript(conn, script);
//            //                    break;
//            //                case EProvisioningTask.Update:
//            //                case EProvisioningTask.Verify:
//            //                    return "'UPDATE' and 'VERIFY' tasks are not implemented at the moment";
//            //                case EProvisioningTask.Rebuild:
//            //                    scriptWriter.WriteDeleteScript(conn, script);
//            //                    scriptWriter.WriteCreateNewScript(script);
//            //                    script.WriteLine($"INSERT INTO DbConfigMenge ([sysname],[value]) VALUES ('SyncProvisionTimestamp',N'{DateTime.Now:yyyyMMddHHmmss}');");
//            //                    script.WriteLine($"INSERT INTO DbConfigMenge ([sysname],[value]) VALUES ('SyncProvisionSchemaVersion',N'{schema}');");
//            //                    break;
//            //                default:
//            //                    throw new InvalidEnumArgumentException();
//            //            }

//            //            return script.ToString();
//            //        }
//            //    }
//            //}
//            //catch(Exception exception)
//            //{
//            //    return exception.GetMessagesTree();
//            //}
//        }

//        public FileResult DownloadDatabaseBackup(string fileName)
//        {
//            var f = Path.Combine(AdminConfig.Current.CommonConfig.Backup, "Databases", fileName);
//            if(!System.IO.File.Exists(f))
//            {
//                throw new Exception("Datei nicht gefunden: " + fileName);
//            }
//            return File(f, "application/octet-stream", fileName);
//        }

//        #region Database Tools

//        private void BackupDatabase(string instance, string database, string username, string password, string backupFile, bool compression)
//        {
//            Response.Cache.SetNoStore();
//            Response.BufferOutput = false;
//            Response.Buffer = false;
//            Response.Write("<ul style='list-style-type: none;padding: 0;font-family: monospace;'>");

//            var pathToBackup = Path.GetDirectoryName(backupFile) ?? "";
//            if(!Directory.Exists(pathToBackup)) Directory.CreateDirectory(pathToBackup);
//            if(System.IO.File.Exists(backupFile)) System.IO.File.Delete(backupFile);

//            var serverConnection = string.IsNullOrEmpty(username) ? new ServerConnection(instance) : new ServerConnection(instance, username, password);
//            var server = new Microsoft.SqlServer.Management.Smo.Server(serverConnection);
//            var backup = new Backup
//            {
//                Action = BackupActionType.Database,
//                Database = database,
//                CopyOnly = true,
//                Incremental = false,
//                BackupSetName = "Sicherung",
//                BackupSetDescription = "Sicherung xxxx",
//                CompressionOption = compression ? BackupCompressionOptions.On : BackupCompressionOptions.Off,
//                PercentCompleteNotification = 5
//            };

//            //var device = new BackupDeviceItem("backup.bak", DeviceType.File);
//            //backup.Devices.Add(device);
//            backup.Devices.AddDevice(backupFile, DeviceType.File);

//            backup.PercentComplete += (sender, args) =>
//            {
//                Response.Write($"<li>{args.Percent}%</li>");
//                Response.Flush();
//            };
//            backup.Information += (sender, args) =>
//            {
//                Response.Write($"<li>{args.Error.Message}</li>");
//                Response.Flush();
//            };
//            backup.Complete += (sender, args) =>
//            {
//                Response.Write($"<li>Backup abgeschlossen. {args.Error.Message}</li>");
//                Response.Flush();
//            };

//            backup.SqlBackup(server);

//            Response.Write("</ul>");
//            serverConnection.Disconnect();
//        }

//        #endregion

//        #region Deploy

//        public void DeployService(string service)
//        {
//            var s = AdminConfig.Current.GetServiceType(service);
//            DeployDir(s.DeployPath, AdminConfig.GetServicePath(service));
//        }

//        public void DeploySite(string site)
//        {
//            var s = AdminConfig.Current.GetSiteType(site);
//            var directories = GetDeployDirectories(s, s is AdminConfig.BeratungSiteType || s is AdminConfig.SchnellrechnerSiteType);
//            Deployment(s, directories);
//        }

//        public void DeploySiteOnly(string site, bool jenkins = false)
//        {
//            var s = AdminConfig.Current.GetSiteType(site);
//            var directories = GetDeployDirectories(s, false);
//            Deployment(s, directories);
//        }

//        public void DeployRk(string site)
//        {
//            var s = AdminConfig.Current.GetSiteType(site);
//            if(!(s is AdminConfig.BeratungSiteType)) throw new Exception("DeployRk nur bei Beratung!");

//            var pluginDirectory = GetPluginTuple(s);
//            Deployment(s, new[] { pluginDirectory });
//        }


//        private void Deployment(AdminConfig.SiteType s, IEnumerable<Tuple<string, string>> directories)
//        {
//            var setOnlineOffline = false;
//            var offlineFile = Path.Combine(s.Path, "app_offline.htm");
//            if(AdminConfig.GetSiteState(s) == AdminConfig.ESiteState.Running)
//            {
//                setOnlineOffline = true;
//                if(!string.IsNullOrEmpty(AdminConfig.Current.CommonConfig.AppOfflineTemplateFile) && System.IO.File.Exists(AdminConfig.Current.CommonConfig.AppOfflineTemplateFile))
//                    System.IO.File.Copy(AdminConfig.Current.CommonConfig.AppOfflineTemplateFile, offlineFile, true);
//                else
//                    System.IO.File.WriteAllText(offlineFile, "offline");

//                Thread.Sleep(500);
//            }

//            DeployDir(directories);

//            if(setOnlineOffline && System.IO.File.Exists(offlineFile))
//            {
//                System.IO.File.Delete(offlineFile);
//            }
//        }

//        private IEnumerable<Tuple<string, string>> GetDeployDirectories(AdminConfig.WebSiteType s, bool includePlugins)
//        {
//            var list = new List<Tuple<string, string>>
//                {

//                    new Tuple<string, string>(s.DeployPath, s.Path)
//                };

//            if(includePlugins)
//            {
//                var pluginDirectories = GetPluginTuple(s);
//                list.Add(pluginDirectories);
//            }

//            return list;
//        }

//        private IEnumerable<Tuple<string, string>> GetJenkinsDeployDirectories(AdminConfig.WebSiteType s, bool includePlugins = true)
//        {
//            var list = new List<Tuple<string, string>>
//                {

//                    new Tuple<string, string>(s.JenkinsDeployPath, s.Path)
//                };

//            if(includePlugins)
//            {
//                var pluginDirectories = GetPluginTupleJenkins(s);
//                if(pluginDirectories != null)
//                {
//                    list.Add(pluginDirectories);
//                }
//            }

//            return list;
//        }



//        private Tuple<string, string> GetPluginTuple(AdminConfig.WebSiteType s)
//        {
//            return new Tuple<string, string>(Path.Combine(s.DeployPath, "App_Data", "Plugins"), Path.Combine(s.Path, "App_Data", "Plugins"));
//        }

//        private Tuple<string, string> GetPluginTupleJenkins(AdminConfig.WebSiteType s)
//        {
//            if(Directory.Exists(Path.Combine(s.JenkinsDeployPath, "App_Data", "Plugins")))
//            {
//                return new Tuple<string, string>(Path.Combine(s.JenkinsDeployPath, "App_Data", "Plugins"), Path.Combine(s.Path, "App_Data", "Plugins"));
//            }
//            return null;
//        }


//        private void DeployDir(string source, string destination)
//        {
//            DeployDir(new[] { new Tuple<string, string>(source, destination) });
//        }

//        private void DeployDir(IEnumerable<Tuple<string, string>> dirs)
//        {
//            Response.Cache.SetNoStore();
//            Response.BufferOutput = false;
//            Response.Buffer = false;
//            Response.Write("<pre>");

//            var exitCode = 0;

//            foreach(var dir in dirs)
//            {
//                var source = Path.GetFullPath(dir.Item1);
//                if(!Directory.Exists(source)) throw new DirectoryNotFoundException("Quellverzeichnis nicht gefunden: " + source);
//                var destination = Path.GetFullPath(dir.Item2);
//                if(!Directory.Exists(destination)) throw new DirectoryNotFoundException("Zielverzeichnis nicht gefunden: " + destination);

//                Response.Write($"<h3>{source} -> {destination}</h3>");

//                var p = new Process
//                {
//                    StartInfo = new ProcessStartInfo
//                    {
//                        CreateNoWindow = true,
//                        UseShellExecute = false,
//                        FileName = "ROBOCOPY.EXE",
//                        Arguments = $"\"{source}\" \"{destination}\" /MIR /XJ /W:1 /R:3 /NP /NJH /NJS /XF app_offline.htm /XD App_Data /XF Config.xml /XF sitemap.xml /XF robots.txt",
//                        RedirectStandardOutput = true,
//                        RedirectStandardError = true,
//                        StandardOutputEncoding = Encoding.GetEncoding(850),
//                        StandardErrorEncoding = Encoding.GetEncoding(850)
//                    }
//                };
//                p.OutputDataReceived += (sender, args) =>
//                {
//                    Response.Write($"{args.Data}" + Environment.NewLine);
//                };
//                p.ErrorDataReceived += (sender, args) =>
//                {
//                    Response.Write($"<strong style='color: red'>{args.Data}</strong>" + Environment.NewLine);
//                };
//                p.Start();
//                p.BeginOutputReadLine();
//                p.BeginErrorReadLine();
//                p.WaitForExit();
//                if(exitCode < p.ExitCode) exitCode = p.ExitCode;
//                if(p.ExitCode >= 8) break;
//            }

//            Response.Write(exitCode < 8 ? $"<strong id='ExitCodeSuccess' style='color: green'>ERFOLG: {exitCode}</strong>" : $"<strong id='ExitCodeError' style='color: green'>FEHLER: {exitCode}</strong>");
//            Response.Write("</pre>");
//            Response.Write("<script>parent.document.getElementById('modalLog').classList.add('" + ((exitCode < 8) ? "modal-success" : "modal-error") + "');</script>");
//            Response.Flush();
//        }

//        #endregion

//        public ActionResult Sessions()
//        {
//            var login = Session.GetLogin();
//            if(login == null || login.Disabled) return new HttpUnauthorizedResult();
//            if(login.CockpitType == ECockpitType.None) return RedirectToAction("Index", "Home");

//            var sessions = MvcApplication.SessionStates.ToDictionary(x => x.Key, y => y.Value);
//            return View(sessions);
//        }

//        public ActionResult KillSession(string sessionID)
//        {
//            var login = Session.GetLogin();
//            if(login == null || login.Disabled) return new HttpUnauthorizedResult();
//            if(login.CockpitType == ECockpitType.None) return RedirectToAction("Index", "Home");

//            MvcApplication.SessionStates.TryGetValue(sessionID, out var session);
//            session?.Session.Clear();
//            session?.Session.Abandon();

//            return RedirectToAction("Sessions");
//        }

//        public ActionResult BlockSyncSession(string sessionID)
//        {
//            var login = Session.GetLogin();
//            if(login == null || login.Disabled) return new HttpUnauthorizedResult();
//            if(login.CockpitType == ECockpitType.None) return RedirectToAction("Index", "Home");

//            MvcApplication.SessionStates.TryGetValue(sessionID, out var session);

//            return RedirectToAction("Sessions");
//        }

//        #region Upload Plugins

//        [HttpPost]
//        [AllowAnonymous]
//        public ActionResult UploadPluginPackage(HttpPostedFileBase file, string area, string type = "Beratung")
//        {
//            if(!Request.IsAuthenticated)
//            {
//                var auth = (Request.Headers.Get("Authorization") ?? "").Split(':');
//                if((auth.Length != 2 || Eulg.Server.Common.Login.GetValidatedUser(auth[0], auth[1])?.CockpitType != ECockpitType.CockpitFullAccess) && !Request.IsAuthenticated)
//                {
//                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
//                }
//            }
//            if(file == null || file.ContentLength == 0) return new HttpStatusCodeResult(System.Net.HttpStatusCode.ExpectationFailed, "Datei fehlt!");



//            var a = AdminConfig.Current.Areas.Single(s => s.Name.Equals(area, StringComparison.InvariantCultureIgnoreCase));
//            var sitePath = type == "Beratung" ? a.BeratungSite.Path : a.SchnellrechnerSite.Path;

//            var offlineFile = Path.Combine(sitePath, "app_offline.htm");

//            // Zip-Datei in App_Data/Upload speichern...
//            var filesPath = Server.MapPath("~/App_Data/Upload");
//            if(!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);
//            var fileName = Path.Combine(filesPath, file.FileName);
//            file.SaveAs(fileName);

//            // Seite offline schalten...
//            bool alreadyOffline = System.IO.File.Exists(offlineFile);
//            if(!alreadyOffline) System.IO.File.WriteAllText(offlineFile, "offline");
//            // warten...
//            Thread.Sleep(2000);

//            // alter Order-Inhalt löschen...
//            var pluginPath = Path.Combine(sitePath, "App_Data", "Plugins");
//            if(Directory.Exists(pluginPath)) Directory.Delete(pluginPath, true);
//            if(!Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);

//            // Zip-Datei entpacken...
//            System.IO.Compression.ZipFile.ExtractToDirectory(fileName, pluginPath);

//            // Seite wieder online schalten...
//            if(!alreadyOffline && System.IO.File.Exists(offlineFile)) System.IO.File.Delete(offlineFile);

//            // Zip-Datei löschen
//            if(System.IO.File.Exists(fileName)) System.IO.File.Delete(fileName);

//            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
//        }

//        #endregion

//        #region Jenkins Backup
//        [AllowAnonymous]
//        public ActionResult BackupSite(string siteName)
//        {
//            try
//            {
//                var auth = (Request.Headers.Get("Authorization") ?? "").Split(':');
//                if(auth.Length != 2 || Eulg.Server.Common.Login.GetValidatedUser(auth[0], auth[1])?.CockpitType != ECockpitType.CockpitFullAccess)
//                {
//                    return Content("AUTHFAIL");
//                }

//                //Todo HaPe vorläufige Redundanz wegen Backup Prüfung
//                //Wird nach Abstimmung mit Holger vereinheitlicht
//                var site = AdminConfig.Current.GetSiteType(siteName);
//                var zipFile = Path.Combine(AdminConfig.Current.CommonConfig.Backup, "Sites", AdminConfig.SanitizeFileName($"{siteName}_{DateTime.Now:yyyyMMddHHmm}.zip", '_'));
//                bool result = AdminConfig.BackupDirectory(zipFile, site.Path, new[] { @"App_Data\", @"app_offline.htm" }, (site is AdminConfig.BeratungSiteType) ? new[] { @"App_Data\Plugins" } : null);
//                AdminConfig.Current.LogRotateSiteBackup(site);
//                //SiteAction(siteName, "Backup");

//                if(result)
//                {
//                    return Content("OK");
//                }

//                throw new Exception("Fehler beim Backup von: " + siteName);

//            }
//            catch(Exception ex)
//            {
//                return Content("FAIL" + ex.Message);
//            }
//        }





//        [AllowAnonymous]
//        public ActionResult JenkinsAction(string methodName, string name, string toDo, string database = "", string scriptName = "", string pathInfo = "")
//        {
//            try
//            {
//                var auth = (Request.Headers.Get("Authorization") ?? "").Split(':');
//                if(auth.Length != 2 || Eulg.Server.Common.Login.GetValidatedUser(auth[0], auth[1])?.CockpitType != ECockpitType.CockpitFullAccess)
//                {
//                    return Content("AUTHFAIL");
//                }

//                if(methodName == "siteAction")
//                {
//                    SiteAction(name, toDo);
//                }

//                if(methodName == "serviceAction")
//                {
//                    ServiceAction(name, toDo);
//                }

//                if(methodName == "deploySite")
//                {
//                    DeployJenkinsSite(name);
//                }

//                if(methodName == "checkOnline")
//                {
//                    var s = AdminConfig.Current.GetSiteType(name);
//                    if(AdminConfig.GetSiteState(s) != AdminConfig.ESiteState.Running)
//                    {
//                        return Content("Site is offline");
//                    }
//                }

//                if(methodName == "updateTaskScheduler")
//                {
//                    if(pathInfo == "")
//                    {
//                        return Content("Error : TaskScheduler DeployPath fehlt");
//                    }

//                    var filesPath = Server.MapPath("~/App_Data/Upload");
//                    var zipFile = Path.Combine(filesPath, "TaskScheduler.zip");

//                    if(!System.IO.File.Exists(zipFile))
//                    {
//                        return Content("Error : TaskScheduler.zip nicht gefunden");
//                    }

//                    if(pathInfo == "delete")
//                    {
//                        System.IO.File.Delete(zipFile);
//                        return Content("OK : TaskScheduler.zip Datei wurde gelöscht");
//                    }

//                    if(pathInfo.ToLower().Contains(".deploy"))
//                    {
//                        foreach(DirectoryInfo subDir in new DirectoryInfo(pathInfo).GetDirectories())
//                            subDir.Delete(true);

//                        foreach(System.IO.FileInfo file in new DirectoryInfo(pathInfo).GetFiles())
//                            file.Delete();
//                    }

//                    System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, pathInfo);

//                    DeployService(name);
//                }


//                if(methodName == "ExecuteDBScript" && database != "" && scriptName != "")
//                {

//                    var filesPath = Server.MapPath("~/App_Data/UploadDBScript");

//                    if(!Directory.Exists(filesPath))
//                        return Content("Error: FilePath not found");


//                    FileInfo info = new FileInfo(Path.Combine(filesPath, scriptName));
//                    if(!info.Exists)
//                        return Content("Error: FileInfo");

//                    var script = System.IO.File.ReadAllText(info.FullName);


//                    var result = ExecuteDbScript(name, database, script, true);
//                    return Content(result);
//                }

//                if(methodName == "MoveDBScript" && scriptName != "")
//                {
//                    //return Content(string.Format("deploySite: {0} {1} {2}", methodName, name, toDo));
//                    var filesPath = Server.MapPath("~/App_Data/UploadDBScript");
//                    var donePath = Server.MapPath("~/App_Data/UploadDBScript/done");

//                    if(!Directory.Exists(filesPath))
//                        return Content("Error: FilePath not found");


//                    FileInfo info = new FileInfo(Path.Combine(filesPath, scriptName));
//                    if(!info.Exists)
//                        return Content($"File not found {info.Name}");

//                    string dt1 = DateTime.Now.ToLongDateString().Replace('.', '_').Replace(':', '_');
//                    string dt2 = DateTime.Now.ToLongTimeString().Replace('.', '_').Replace(':', '_');


//                    Directory.Move(info.FullName, Path.Combine(donePath, scriptName + "_done_" + dt1 + "__" + dt2));

//                    return Content("OK");
//                }

//                return Content("OK");

//            }
//            catch(Exception ex)
//            {
//                return Content("FAIL" + ex.Message);
//            }
//        }


//        [AllowAnonymous]
//        public ActionResult BackupDatabase(string area, string database, string command)
//        {
//            try
//            {
//                var auth = (Request.Headers.Get("Authorization") ?? "").Split(':');
//                if(auth.Length != 2 || Eulg.Server.Common.Login.GetValidatedUser(auth[0], auth[1])?.CockpitType != ECockpitType.CockpitFullAccess)
//                {
//                    return Content("AUTHFAIL");
//                }
//                DatabaseAction(area, database, command);
//                return Content("OK");
//            }
//            catch(Exception e)
//            {
//                return Content("FAIL" + e.Message);
//            }
//        }


//        [HttpPost]
//        [AllowAnonymous]
//        public ActionResult UploadFile(HttpPostedFileBase file, string uploadPath)
//        {
//            try
//            {
//                if(!Request.IsAuthenticated)
//                {
//                    var auth = (Request.Headers.Get("Authorization") ?? "").Split(':');
//                    if((auth.Length != 2 || Eulg.Server.Common.Login.GetValidatedUser(auth[0], auth[1])?.CockpitType != ECockpitType.CockpitFullAccess) && !Request.IsAuthenticated)
//                    {
//                        return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
//                    }
//                }
//                if(file == null || file.ContentLength == 0) return new HttpStatusCodeResult(System.Net.HttpStatusCode.ExpectationFailed, "Datei fehlt!");

//                var filesPath = Server.MapPath("~/App_Data/" + uploadPath);

//                if(!Directory.Exists(filesPath))
//                    Directory.CreateDirectory(filesPath);

//                var fileName = Path.Combine(filesPath, file.FileName);

//                file.SaveAs(fileName);

//                return Content("OK");
//            }
//            catch(Exception e)
//            {
//                Console.WriteLine(e);
//                return Content("Error:" + e.Message);
//            }
//        }


//        public void DeployJenkinsSite(string site)
//        {
//            try
//            {
//                var s = AdminConfig.Current.GetSiteType(site);
//                var directories = GetJenkinsDeployDirectories(s);
//                Deployment(s, directories);
//            }
//            catch(Exception e)
//            {
//                throw new Exception(e.Message);
//            }

//        }


//        #endregion

//        /// <summary>
//        /// Ruft eine Info über die aktive Benutzer-Sessions pro Site ab (um zu entscheiden, ob das Release veröffentlicht werden kann).
//        /// </summary>
//        /// <param name="sites"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public JsonResult RefreshSessionInfo(string[] sites)
//        {
//            // bei folgenden Projekten dürfen keine aktiven Session existieren, wenn eine neue Version deployed wird (da sonst der Benutzer rausfliegt).
//            // ACHTUNG: diese Projekte müssen im ToolsController die Action "GetActiveSessions" implementieren
//            var sitesRespectingSessions = new AdminConfig.WebSiteType[]
//            {
//                new AdminConfig.BeratungSiteType(),
//                new AdminConfig.VerwaltungSiteType(),
//                new AdminConfig.SchnellrechnerSiteType()
//            };

//            var relevantSiteTypes = new List<AdminConfig.WebSiteType>();

//            foreach(var siteName in sites) // filtere die übergebenen Sites nach session-relevanten Projekten
//            {
//                var s = AdminConfig.Current.GetSiteType(siteName);
//                if(sitesRespectingSessions.Any(x => x.GetType() == s.GetType()))
//                {
//                    relevantSiteTypes.Add(s);
//                }
//            }

//            var result = new ConcurrentBag<Tuple<string, int?>>();
//            var taskArray = new Task[relevantSiteTypes.Count];

//            for(int i = 0; i < relevantSiteTypes.Count; i++)
//            {
//                var i1 = i;
//                taskArray[i] = Task.Factory.StartNew(() =>
//                {
//                    using(var client = new HttpClient())
//                    {
//                        client.Timeout = TimeSpan.FromSeconds(5);
//                        var s = relevantSiteTypes[i1];
//                        Tuple<string, int?> tuple;
//                        try
//                        {
//                            var url = GetActiveSessionsUrl(s);
//                            var activeSessions = client.GetStringAsync(url).Result; // jedes Webprojekt hat eine entsprechende GetActiveSessions-Action im ToolsController
//                            tuple = new Tuple<string, int?>(s.SiteName, Int16.Parse(activeSessions));
//                        }
//                        catch(Exception e)
//                        {
//                            Console.WriteLine(e);
//                            tuple = new Tuple<string, int?>(s.SiteName, null);
//                        }

//                        result.Add(tuple);
//                    }
//                });
//            }

//            Task.WaitAll(taskArray);

//            return Json(result);
//        }

//        private string GetActiveSessionsUrl(AdminConfig.WebSiteType site)
//        {
//            var divider = site.Url.EndsWith("/") ? String.Empty : "/";
//            return site.Url + divider + "Tools/GetActiveSessions";
//        }

//        [AllowAnonymous]
//        public ActionResult HealthCheck()
//        {
//            return Json(AdminConfig.IsAdminService 
//                ? Eulg.Server.Common.HealthCheck.GetChecks(new HealthCheck.DatabaseHealthCheck(), new HealthCheck.DriveHealthCheck(), new HealthCheck.RamHealthCheck()) 
//                : Eulg.Server.Common.HealthCheck.GetChecks(new HealthCheck.DatabaseHealthCheck()));
//        }
//    }
//}