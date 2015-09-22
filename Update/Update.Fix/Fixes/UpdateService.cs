using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using Microsoft.Win32;

namespace Update.Fix.Fixes
{
    internal static class UpdateService
    {
        private const string UPDATE_SERVICE_NAME = "EulgUpdate";
        private const string UPDATE_SERVICE_PARENT_PATH = "EULG Software GmbH";
        private const string UPDATE_SERVICE_PARENT_PATH_OBSOLETE = "KS Software GmbH";
        private const string UPDATE_SERVICE_PATH = "UpdateService";
        private const string UPDATE_SERVICE_BINARY = "UpdateService.exe";
        private static readonly DateTime _updateServiceDateTimeFixed = new DateTime(2015, 09, 10);
        private static readonly TimeSpan _serviceTimeout = new TimeSpan(0, 0, 0, 30);

        internal static bool Check()
        {
            try
            {
                // Service überhaupt installiert?
                if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME))) return false;

                // Service im richtigen Pfad (oder evtl noch KS...)
                var pathShould = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), UPDATE_SERVICE_PARENT_PATH, UPDATE_SERVICE_PATH, UPDATE_SERVICE_BINARY);
                var pathIs = GetServiceImagePath(UPDATE_SERVICE_NAME);
                if (!pathIs.Equals(pathShould, StringComparison.InvariantCultureIgnoreCase)) return false;

                // Service richtige Version?
                var fi = new FileInfo(pathIs);
                if (fi.LastWriteTime < _updateServiceDateTimeFixed) return false;

                // Service lässt sich starten?
                using (var svc = new ServiceController(UPDATE_SERVICE_NAME))
                {
                    if (svc.Status != ServiceControllerStatus.Running)
                    {
                        svc.Start();
                        svc.WaitForStatus(ServiceControllerStatus.Running, _serviceTimeout);
                    }
                    var ok = (svc.Status != ServiceControllerStatus.Running);
                    if (svc.Status != ServiceControllerStatus.Stopped)
                    {
                        svc.Stop();
                    }
                    return ok;
                }
            }
            catch
            {
                return false;
            }
        }

        internal static void Fix()
        {
            var newImageFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UPDATE_SERVICE_BINARY);
            if (!File.Exists(newImageFile)) throw new Exception("Datei " + newImageFile + " nicht gefunden.");

            var pathShould = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), UPDATE_SERVICE_PARENT_PATH, UPDATE_SERVICE_PATH, UPDATE_SERVICE_BINARY);

            // Wenn Service bereits installiert -> zuerst entfernen...
            if (ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
            {
                var pathIs = GetServiceImagePath(UPDATE_SERVICE_NAME);

                // Stopp
                using (var svc = new ServiceController(UPDATE_SERVICE_NAME))
                {
                    if (svc.Status != ServiceControllerStatus.Stopped)
                    {
                        svc.Stop();
                        svc.WaitForStatus(ServiceControllerStatus.Stopped, _serviceTimeout);
                    }
                }

                // Uninstall
                var p = new Process { StartInfo = { FileName = pathIs, Arguments = "uninstall", RedirectStandardOutput = false, RedirectStandardError = false, CreateNoWindow = false, UseShellExecute = false } };
                p.Start();
                p.WaitForExit();

                // Delete
                if (pathIs.Equals(pathShould, StringComparison.InvariantCultureIgnoreCase))
                {
                    var pathCurrent = Path.GetDirectoryName(pathIs) ?? string.Empty;
                    var pathObsolete = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), UPDATE_SERVICE_PARENT_PATH_OBSOLETE, UPDATE_SERVICE_PATH);
                    if (pathCurrent.Equals(pathObsolete, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var pathToDelete = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), UPDATE_SERVICE_PARENT_PATH_OBSOLETE);
                        DeleteDirectory(pathToDelete);
                    }
                }
            }

            // Copy new Image
            var destDir = Path.GetDirectoryName(pathShould) ?? string.Empty;
            Directory.CreateDirectory(destDir);
            File.Copy(newImageFile, pathShould, true);
            SetDirectoryAccessControl(destDir);

            // Install new Service
            var pNew = new Process { StartInfo = { FileName = pathShould, Arguments = "install", RedirectStandardOutput = false, RedirectStandardError = false, CreateNoWindow = false, UseShellExecute = false } };
            pNew.Start();
            pNew.WaitForExit();
        }

        private static void SetDirectoryAccessControl(string path)
        {
            var dirSec = Directory.GetAccessControl(path);
            dirSec.SetAccessRuleProtection(true, false);
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            dirSec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.SetAccessControl(path, dirSec);
        }

        private static string GetServiceImagePath(string serviceName)
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName);
            return key?.GetValue("ImagePath").ToString();
        }

        private static void DeleteDirectory(string directory)
        {
            var files = Directory.GetFiles(directory);
            var dirs = Directory.GetDirectories(directory);
            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            foreach (var dir in dirs)
            {
                DeleteDirectory(dir);
            }
            Directory.Delete(directory, false);
        }
    }
}