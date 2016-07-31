using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    internal class StartMenu : LinksBase, IFix
    {
        private const string NEW_START_MENU_FOLDER = "xbAV-Berater";
        private const string SYNC_CLIENT = "Eulg_sync_client.exe";
        private const string SUPPORT = "Support\\Support.exe";
        private const string REMOTE = "Support\\Fernwartung.exe";

        private static readonly Dictionary<string, string> _availableStartMenuFolders = new Dictionary<string, string>
        {
            { "", "EULG" },
            { "Test", "EULG (Test)" },
            { "Beta", "EULG (Beta)" },
            { "PreRel", "EULG (Pre)" },
            { "EulgDeTest", "EULG (test.eulg.de)" }
        };

        private StartMenu() { }

        public static IFix Inst { get; } = new StartMenu();

        public string Name => nameof(StartMenu);

        public bool? Check()
        {
            var folderName = GetLegacyStartMenuFolder();
            if (folderName == null) return null;

            var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), folderName);
            var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), folderName);

            return !(Directory.Exists(clientStartMenuMain) && Directory.Exists(clientStartMenuPrograms));
        }

        public void Apply()
        {
            var folderName = GetLegacyStartMenuFolder();
            if(folderName == null) throw new InvalidOperationException("Kann Order im Startmenü nicht bestimmen");

            var clientStartMenuLegacy = string.Empty;
            var clientStartMenuNew = string.Empty;
            var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), folderName);
            var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), folderName);

            var buildTag = GetBuildTag();
            var newFolderName = string.IsNullOrEmpty(buildTag) ? NEW_START_MENU_FOLDER : $"{NEW_START_MENU_FOLDER} ({buildTag})";

            if (Directory.Exists(clientStartMenuMain))
            {
                clientStartMenuLegacy = clientStartMenuMain;
                clientStartMenuNew = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), newFolderName);
            }
            else if (Directory.Exists(clientStartMenuPrograms))
            {
                clientStartMenuLegacy = clientStartMenuPrograms;
                clientStartMenuNew = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), newFolderName);
            }

            if (!string.IsNullOrEmpty(clientStartMenuNew))
            {
                var baseDirectory = GetInstallDir();

                string[] startMenuGroupNew = new string[0];
                if (Directory.Exists(clientStartMenuNew))
                {
                    startMenuGroupNew = Directory.GetFiles(clientStartMenuNew, "*.lnk");
                }
                else
                {
                    Directory.CreateDirectory(clientStartMenuNew);
                }

                var clientName = string.IsNullOrEmpty(buildTag) ? "xbAV-Berater" : $"xbAV-Berater ({buildTag})";
                var syncName = string.IsNullOrEmpty(buildTag) ? "Sync-Client" : $"Sync-Client ({buildTag})";

                var clientLink = $"{clientName}.lnk";
                var syncClientLink = $"{syncName}.lnk";
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

        private static string GetLegacyStartMenuFolder()
        {
            var buildTag = GetBuildTag();
            if (buildTag == null)
            {
                return null;
            }

            string folderName;
            return _availableStartMenuFolders.TryGetValue(buildTag, out folderName) ? folderName : $"EULG ({buildTag})";
        }
    }
}
