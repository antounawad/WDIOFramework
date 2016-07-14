using System;
using System.IO;
using System.Linq;
using Eulg.Shared;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    public class StartMenu
    {
        private const string BRANDING_FILE_NAME = "Branding.xml";

        private const string LEGACY_STARTMENUFOLDER = "EULG";

        private const string CLIENT = "EULG_client.exe";
        private const string SYNC_CLIENT = "EULG_sync_Client.exe";
        private const string SUPPORT = "Support\\Support.exe";
        private const string REMOTE = "Support\\Fernwartung.exe";

        private static Branding _branding;
        private static Branding Branding
        {
            get
            {
                if(_branding == null)
                {
                    var brandingXmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BRANDING_FILE_NAME);
                    if (!File.Exists(brandingXmlFile)) throw new Exception("Datei " + brandingXmlFile + " nicht gefunden.");

                    _branding = Branding.Read(brandingXmlFile);
                }

                return _branding;
            }
        }

        internal static bool Check()
        {
            if(Branding != null)
            {
                var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Branding.ShellIcons.StartMenuFolder);
                var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Branding.ShellIcons.StartMenuFolder);

                return Directory.Exists(clientStartMenuMain) || Directory.Exists(clientStartMenuPrograms);
            }

            return true;
        }

        internal static void Fix()
        {
            if (Branding != null)
            {
                var clientStartMenuLegacy = string.Empty;
                var clientStartMenuNew = string.Empty;
                var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), LEGACY_STARTMENUFOLDER);
                var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), LEGACY_STARTMENUFOLDER);

                if (Directory.Exists(clientStartMenuMain))
                {
                    clientStartMenuLegacy = clientStartMenuMain;
                    clientStartMenuNew = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Branding.ShellIcons.StartMenuFolder);
                }
                else if (Directory.Exists(clientStartMenuPrograms))
                {
                    clientStartMenuLegacy = clientStartMenuPrograms;
                    clientStartMenuNew = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Branding.ShellIcons.StartMenuFolder);
                }

                if (!string.IsNullOrEmpty(clientStartMenuNew))
                {
                    var buildTag = Branding.Info.BuildTag;
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    string[] startMenuGroupNew = new string[0];
                    if (Directory.Exists(clientStartMenuNew))
                    {
                        startMenuGroupNew = Directory.GetFiles(clientStartMenuNew, "*.lnk");
                    }
                    else
                    {
                        Directory.CreateDirectory(clientStartMenuNew);
                    }

                    var clientLink = !string.IsNullOrEmpty(Branding.ShellIcons.StartMenuApp)
                                         ? $"{Branding.ShellIcons.StartMenuApp}.lnk"
                                         : $"xbAV-Berater{(!string.IsNullOrEmpty(buildTag) ? " (" + buildTag + ")" : string.Empty)}.lnk";
                    var syncClientLink = !string.IsNullOrEmpty(Branding.ShellIcons.StartMenuSync)
                                             ? $"{Branding.ShellIcons.StartMenuSync}.lnk"
                                             : $"Sync-Client{(!string.IsNullOrEmpty(buildTag) ? " (" + buildTag + ")" : string.Empty)}.lnk";
                    var supportLink = !string.IsNullOrEmpty(Branding.ShellIcons.StartMenuSupportTool) ? $"{Branding.ShellIcons.StartMenuSupportTool}.lnk" : "Support.lnk";
                    var remoteLink = !string.IsNullOrEmpty(Branding.ShellIcons.StartMenuFernwartung) ? $"{Branding.ShellIcons.StartMenuFernwartung}.lnk" : "Fernwartung.lnk";

                    if (!startMenuGroupNew.Contains(Path.Combine(clientStartMenuNew, clientLink)))
                    {
                        SetLink(Path.Combine(clientStartMenuNew, clientLink), Path.Combine(baseDirectory, CLIENT));
                    }

                    if (!startMenuGroupNew.Contains(Path.Combine(clientStartMenuNew, supportLink)))
                    {
                        SetLink(Path.Combine(clientStartMenuNew, supportLink), Path.Combine(baseDirectory, SUPPORT));
                    }

                    if (!startMenuGroupNew.Contains(Path.Combine(clientStartMenuNew, remoteLink)))
                    {
                        SetLink(Path.Combine(clientStartMenuNew, remoteLink), Path.Combine(baseDirectory, REMOTE));
                    }

                    if (!startMenuGroupNew.Contains(Path.Combine(clientStartMenuNew, syncClientLink)))
                    {
                        SetLink(Path.Combine(clientStartMenuNew, syncClientLink), Path.Combine(baseDirectory, SYNC_CLIENT));
                    }
                }

                if (!string.IsNullOrEmpty(clientStartMenuLegacy))
                {
                    var startMenuGroupLegacy = Directory.GetFiles(clientStartMenuLegacy, "*.lnk");

                    foreach (var file in startMenuGroupLegacy)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception e)
                        {
                            // ignore
                        }
                    }

                    try
                    {
                        Directory.Delete(clientStartMenuLegacy);
                    }
                    catch (Exception e)
                    {
                        // ignore
                    }
                }
            }
        }

        private static void SetLink(string link, string destination)
        {
            var shell = new WshShellClass();

            try
            {
                var shortcut = (IWshShortcut)shell.CreateShortcut(link);
                shortcut.TargetPath = destination;
                shortcut.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
