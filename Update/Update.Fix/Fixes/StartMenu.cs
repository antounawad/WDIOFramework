using System;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    public class StartMenu: LinksBase
    {
        private const string LEGACY_STARTMENUFOLDER = "EULG";

        private const string SYNC_CLIENT = "EULG_sync_Client.exe";
        private const string SUPPORT = "Support\\EulgSupport.exe";
        private const string REMOTE = "Support\\EulgFernwartung.exe";

        public static bool Check()
        {
            if(Branding != null)
            {
                var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Branding.ShellIcons.StartMenuFolder);
                var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Branding.ShellIcons.StartMenuFolder);

                return Directory.Exists(clientStartMenuMain) || Directory.Exists(clientStartMenuPrograms);
            }

            return true;
        }

        public static void Fix()
        {
            if (Branding != null)
            {
                var buildTag = Branding.Info.BuildTag;

                var clientStartMenuLegacy = string.Empty;
                var clientStartMenuNew = string.Empty;
                var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), $"{LEGACY_STARTMENUFOLDER}{(!string.IsNullOrEmpty(buildTag) ? " (" + buildTag + ")" : string.Empty)}");
                var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), $"{LEGACY_STARTMENUFOLDER}{(!string.IsNullOrEmpty(buildTag) ? " (" + buildTag + ")" : string.Empty)}");

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
                    var baseDirectory = BASEDIRECTORY;

                    string[] startMenuGroupNew = new string[0];
                    if (Directory.Exists(clientStartMenuNew))
                    {
                        startMenuGroupNew = Directory.GetFiles(clientStartMenuNew, "*.lnk");
                    }
                    else
                    {
                        Directory.CreateDirectory(clientStartMenuNew);
                    }

                    var clientLink = $"xbAV-Berater{(!string.IsNullOrEmpty(buildTag) ? " (" + buildTag + ")" : string.Empty)}.lnk";
                    var syncClientLink = $"Sync-Client{(!string.IsNullOrEmpty(buildTag) ? " (" + buildTag + ")" : string.Empty)}.lnk";
                    var supportLink = "Support.lnk";
                    var remoteLink = "Fernwartung.lnk";

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
    }
}
