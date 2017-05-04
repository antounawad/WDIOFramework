using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using Eulg.Shared;
using Eulg.Update.Shared;

namespace Eulg.Update.Service
{
    public class Updater : IUpdater
    {
        private const int FileOperationTimeout = 5000;
        private const int FileOperationWaitTime = 250;

        public void DirectoryCreate(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void DirectoryMove(string sourceDirName, string destDirName)
        {
            try
            {
                Directory.Move(sourceDirName, destDirName);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void DirectorySetAccessControlWorld(string path)
        {
            try
            {
                var directorySecurity = Directory.GetAccessControl(path);
                directorySecurity.SetAccessRuleProtection(true, false);
                var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                directorySecurity.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(path, directorySecurity);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void DirectorySetLastWriteTime(string path, DateTime lastWriteTime)
        {
            try
            {
                Directory.SetLastWriteTime(path, lastWriteTime);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void FileCopy(string sourceFileName, string destFileName, bool overwrite)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var ex = new Exception();
                while (stopwatch.ElapsedMilliseconds < FileOperationTimeout)
                {
                    try
                    {
                        File.Copy(sourceFileName, destFileName, overwrite);
                        return;
                    }
                    catch (Exception exception)
                    {
                        ex = exception;
                    }
                    Thread.Sleep(FileOperationWaitTime);
                }
                throw ex;
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void FileMove(string sourceFileName, string destFileName)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var ex = new Exception();
                while (stopwatch.ElapsedMilliseconds < FileOperationTimeout)
                {
                    try
                    {
                        File.Move(sourceFileName, destFileName);
                        return;
                    }
                    catch (Exception exception)
                    {
                        ex = exception;
                    }
                    Thread.Sleep(FileOperationWaitTime);
                }
                throw ex;
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void FileDelete(string path)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var ex = new Exception();
                while (stopwatch.ElapsedMilliseconds < FileOperationTimeout)
                {
                    try
                    {
                        File.Delete(path);
                        return;
                    }
                    catch (Exception exception)
                    {
                        ex = exception;
                    }
                    Thread.Sleep(FileOperationWaitTime);
                }
                throw ex;
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void FileSetAttributes(string path, FileAttributes fileAttributes)
        {
            try
            {
                File.SetAttributes(path, fileAttributes);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void FileSetAccessControlInherit(string path)
        {
            try
            {
                FileSecurity fileSecurity = File.GetAccessControl(path);
                fileSecurity.SetAccessRuleProtection(false, false);
                File.SetAccessControl(path, fileSecurity);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void FileSetLastWriteTime(string path, DateTime lastWriteTime)
        {
            try
            {
                File.SetLastWriteTime(path, lastWriteTime);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void ProcessStart(WorkerProcess processStartInfo, bool wait)
        {
            try
            {
                var process = new Process { StartInfo = processStartInfo.ToProcessStartInfo() };
                process.Start();
                if (wait)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public Process[] GetProcessesByName(string processName)
        {
            try
            {
                return Process.GetProcessesByName(processName);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
            return null;
        }

        public Process GetProcessById(int processId)
        {
            try
            {
                return Process.GetProcessById(processId);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
            return null;
        }

        public void KillProcessesByName(string processName)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    process.Kill();
                }
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void KillProcessById(int processId)
        {
            try
            {
                Process.GetProcessById(processId).Kill();
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        [DllImport("user32.dll")]
        private static extern int ExitWindowsEx(int operationFlag, int operationReason);

        public void RestartWindows()
        {
            try
            {
                ExitWindowsEx(2, 0);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void ServiceStart(string serviceName, bool wait)
        {
            try
            {
                var svc = new ServiceController(serviceName);
                svc.Start();
                if (wait) svc.WaitForStatus(ServiceControllerStatus.Running);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public void ServiceStop(string serviceName, bool wait)
        {
            try
            {
                var svc = new ServiceController(serviceName);
                svc.Stop();
                if (wait) svc.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            catch (Exception exception)
            {
                StoreException(exception);
            }
        }

        public string[] GetExceptions(bool clear)
        {
            var x = _exceptions.ToArray();
            if (clear) _exceptions.Clear();
            return x;
        }

        private static readonly List<string> _exceptions = new List<string>();

        private static void StoreException(Exception exception)
        {
            _exceptions.Add(exception.GetMessagesTree());
        }
    }
}