using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    public class StartMenu: LinksBase
    {
        private const string NEW_START_MENU_FOLDER = "xbAV-Berater";
        private const string SYNC_CLIENT = "EULG_sync_Client.exe";
        private const string SUPPORT = "Support\\EulgSupport.exe";
        private const string REMOTE = "Support\\EulgFernwartung.exe";

        private static readonly Dictionary<string, string> _availableBuildTags = new Dictionary<string, string>()
                                                                {
                                                                    { "EULG" , ""},
                                                                    { "EULG-Test" , " (Test)"},
                                                                    { "EulgDeTest" , " (test.eulg.de)"},
                                                                };

        private static readonly Dictionary<string, string> _availableStartMenuFolders = new Dictionary<string, string>()
                                                                { 
                                                                    { "EULG" , "EULG"},
                                                                    { "EULG-Test" , "EULG (Test)"},
                                                                    { "EulgDeTest" , "EULG (test.eulg.de)"},
                                                                };

        private static string _legacyStartMenuFolder => _availableStartMenuFolders[BASEDIRECTORYNAME];
        private static string _buildTag => _availableBuildTags[BASEDIRECTORYNAME];

        public static bool Check()
        {
            var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), _legacyStartMenuFolder);
            var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), _legacyStartMenuFolder);

            return !(Directory.Exists(clientStartMenuMain) || Directory.Exists(clientStartMenuPrograms));
        }

        public static void Fix()
        {
            var clientStartMenuLegacy = string.Empty;
            var clientStartMenuNew = string.Empty;
            var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), _legacyStartMenuFolder);
            var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), _legacyStartMenuFolder);

            if (Directory.Exists(clientStartMenuMain))
            {
                clientStartMenuLegacy = clientStartMenuMain;
                clientStartMenuNew = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), NEW_START_MENU_FOLDER);
            }
            else if (Directory.Exists(clientStartMenuPrograms))
            {
                clientStartMenuLegacy = clientStartMenuPrograms;
                clientStartMenuNew = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), NEW_START_MENU_FOLDER);
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

                var clientLink = $"xbAV-Berater{_buildTag}.lnk";
                var syncClientLink = $"Sync-Client{_buildTag}.lnk";
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
