using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eulg.Shared;
using Eulg.Update.Shared;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Eulg.Update.Common.Helper
{
    public class StartMenuHelper
    {
        private const string CLIENT = "EULG_client.exe";
        private const string SYNC_CLIENT = "EULG_sync_Client.exe";
        private const string SUPPORT = "Support\\EulgSupport.exe";
        private const string REMOTE = "Support\\EulgFernwartung.exe";

        private readonly List<Link> _links = new List<Link>();
        private readonly Branding _branding;

        private class Link
        {
            public Link(string tempLink, string startMenuLink, string linksTo)
            {
                TempLink = tempLink;
                StartMenuLink = startMenuLink;
                LinksTo = linksTo;
            }

            public string TempLink { get; private set; }
            public string StartMenuLink { get; private set; }
            public string LinksTo { get; private set; }
        }

        public StartMenuHelper()
        {
            var branding = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Branding.xml");

            if(File.Exists(branding))
            {
                _branding = Branding.Read(branding);
            }
        }

        public StartMenuHelper(Branding branding)
        {
            _branding = branding;
        }

        public bool CheckStartMenuGroup(string temp)
        {
            _links.Clear();

            if (_branding != null)
            {
                var clientStartMenu = string.Empty;
                var clientStartMenuMain = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), _branding.ShellIcons.StartMenuFolder);
                var clientStartMenuPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), _branding.ShellIcons.StartMenuFolder);

                if (Directory.Exists(clientStartMenuMain))
                {
                    clientStartMenu = clientStartMenuMain;
                }
                else if (Directory.Exists(clientStartMenuPrograms))
                {
                    clientStartMenu = clientStartMenuPrograms;
                }

                if (!string.IsNullOrEmpty(clientStartMenu))
                {
                    var updateSyncLink = false;
                    var buildTag = _branding.Info.BuildTag;
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var startMenuGroup = Directory.GetFiles(clientStartMenu, "*.lnk");

                    var clientLink = !string.IsNullOrEmpty(_branding.ShellIcons.StartMenuApp) ? $"{_branding.ShellIcons.StartMenuApp}.lnk" : $"xbAV-Berater{(!string.IsNullOrEmpty(buildTag) ? " (" + buildTag + ")" : string.Empty)}.lnk";
                    var syncClientLink = !string.IsNullOrEmpty(_branding.ShellIcons.StartMenuSync) ? $"{_branding.ShellIcons.StartMenuSync}.lnk" : $"Sync-Client{(!string.IsNullOrEmpty(buildTag) ? " (" + buildTag + ")" : string.Empty)}.lnk";
                    var supportLink = _branding.ShellIcons.StartMenuSupportTool ?? "Support.lnk";
                    var remoteLink = _branding.ShellIcons.StartMenuFernwartung ?? "Fernwartung.lnk";

                    if (!startMenuGroup.Contains(Path.Combine(clientStartMenu, clientLink)))
                    {
                        _links.Add(new Link(Path.Combine(temp, clientLink), Path.Combine(clientStartMenu, clientLink), Path.Combine(baseDirectory, CLIENT)));
                    }

                    if (!startMenuGroup.Contains(Path.Combine(clientStartMenu, supportLink)))
                    {
                        updateSyncLink = true;
                        _links.Add(new Link(Path.Combine(temp, supportLink), Path.Combine(clientStartMenu, supportLink), Path.Combine(baseDirectory, SUPPORT)));
                    }

                    if (!startMenuGroup.Contains(Path.Combine(clientStartMenu, remoteLink)))
                    {
                        updateSyncLink = true;
                        _links.Add(new Link(Path.Combine(temp, remoteLink), Path.Combine(clientStartMenu, remoteLink), Path.Combine(baseDirectory, REMOTE)));
                    }

                    if (!startMenuGroup.Contains(Path.Combine(clientStartMenu, syncClientLink)) || updateSyncLink)
                    {
                        _links.Add(new Link(Path.Combine(temp, syncClientLink), Path.Combine(clientStartMenu, syncClientLink), Path.Combine(baseDirectory, SYNC_CLIENT)));
                    }

                    if (_links.Any())
                    {
                        GenerateMissingLinks();
                    }
                }
            }

            return _links.Any();
        }

        private void GenerateMissingLinks()
        {
            foreach (var link in _links)
            {
                SetLink(link.TempLink, link.LinksTo);
            }
        }

        public void AddLinks(List<WorkerConfig.WorkerFile> workerFiles)
        {
            foreach (var link in _links)
            {
                if (File.Exists(link.TempLink))
                {
                    workerFiles.Insert(0, new WorkerConfig.WorkerFile()
                    {
                        Destination = link.StartMenuLink,
                        Source = link.TempLink,
                        FileDateTime = DateTime.Now,
                    });
                }
            }
        }

        private static void SetLink(string link, string destination)
        {
            var shell = new WshShellClass();

            try
            {
                var shortcut = (IWshShortcut) shell.CreateShortcut(link);
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