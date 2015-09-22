using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using Eulg.Update.Shared;

namespace Eulg.Update.Worker
{
    internal class ServiceProxy : IUpdater
    {
        private const string WCF_SERVICE_ADDRESS = "net.pipe://localhost/EULGUpdate/Updater";

        private IUpdater _channel;
        public bool UseService { get; set; }

        public void DirectoryCreate(string path)
        {
            ExecuteCommand(u => u.DirectoryCreate(path));
        }

        public void DirectoryMove(string sourceDirName, string destDirName)
        {
            ExecuteCommand(u => u.DirectoryMove(sourceDirName, destDirName));
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            ExecuteCommand(u => u.DirectoryDelete(path, recursive));
        }

        public void DirectorySetAccessControlWorld(string path)
        {
            ExecuteCommand(u => u.DirectorySetAccessControlWorld(path));
        }

        public void DirectorySetLastWriteTime(string path, DateTime lastWriteTime)
        {
            ExecuteCommand(u => u.DirectorySetLastWriteTime(path, lastWriteTime));
        }

        public void FileCopy(string sourceFileName, string destFileName, bool overwrite)
        {
            ExecuteCommand(u => u.FileCopy(sourceFileName, destFileName, overwrite));
        }

        public void FileMove(string sourceFileName, string destFileName)
        {
            ExecuteCommand(u => u.FileMove(sourceFileName, destFileName));
        }

        public void FileDelete(string path)
        {
            ExecuteCommand(u => u.FileDelete(path));
        }

        public void FileSetAttributes(string path, FileAttributes fileAttributes)
        {
            ExecuteCommand(u => u.FileSetAttributes(path, fileAttributes));
        }

        public void FileSetAccessControlInherit(string path)
        {
            ExecuteCommand(u => u.FileSetAccessControlInherit(path));
        }

        public void FileSetLastWriteTime(string path, DateTime lastWriteTime)
        {
            ExecuteCommand(u => u.FileSetLastWriteTime(path, lastWriteTime));
        }

        public void ProcessStart(WorkerProcess processStartInfo, bool wait)
        {
            ExecuteCommand(u => u.ProcessStart(processStartInfo, wait));
        }

        public Process[] GetProcessesByName(string processName)
        {
            return ExecuteQuery(u => u.GetProcessesByName(processName));
        }

        public Process GetProcessById(int processId)
        {
            return ExecuteQuery(u => u.GetProcessById(processId));
        }

        public void KillProcessesByName(string processName)
        {
            ExecuteCommand(u => u.KillProcessesByName(processName));
        }

        public void KillProcessById(int processId)
        {
            ExecuteCommand(u => u.KillProcessById(processId));
        }

        public void RestartWindows()
        {
            ExecuteCommand(u => u.RestartWindows());
        }

        public void ServiceStart(string serviceName, bool wait)
        {
            ExecuteCommand(u => u.ServiceStart(serviceName, wait));
        }

        public void ServiceStop(string serviceName, bool wait)
        {
            ExecuteCommand(u => u.ServiceStop(serviceName, wait));
        }

        public string[] GetExceptions(bool clear)
        {
            return ExecuteQuery(u => u.GetExceptions(clear));
        }

        private void ExecuteCommand(Action<IUpdater> command)
        {
            OpenChannelIfNecessary();
            try
            {
                command(_channel);
            }
            catch (Exception ex1)
            {
                var method = new StackFrame(2, false).GetMethod().Name;
                if (UseService)
                {
                    Log.Write(ex1, method + ": First attempt failed; re-opening service connection and trying again");

                    // Falls Befehlsausführung Fehler verursacht, Verbindung zum Service neu öffnen und ein zweites Mal versuchen; wenn es dann immer noch fehlschlägt, Ausnahme fliegen lassen
                    OpenChannel();

                    try
                    {
                        command(_channel);
                    }
                    catch (Exception ex2)
                    {
                        Log.Write(ex2, method + ": Second attempt failed; giving up");
                        throw;
                    }
                }
                else
                {
                    Log.Write(ex1, method);
                    throw;
                }
            }
        }

        private T ExecuteQuery<T>(Func<IUpdater, T> query)
        {
            OpenChannelIfNecessary();
            try
            {
                return query(_channel);
            }
            catch (Exception ex1)
            {
                var method = new StackFrame(2, false).GetMethod().Name;
                if (UseService)
                {
                    Log.Write(ex1, method + ": First attempt failed; re-opening service connection and trying again");

                    // Falls Befehlsausführung Fehler verursacht, Verbindung zum Service neu öffnen und ein zweites Mal versuchen; wenn es dann immer noch fehlschlägt, Ausnahme fliegen lassen
                    OpenChannel();

                    try
                    {
                        return query(_channel);
                    }
                    catch (Exception ex2)
                    {
                        Log.Write(ex2, method + ": Second attempt failed; giving up");
                        throw;
                    }
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                    Log.Write(ex1, method);
                    throw;
                }
            }
        }

        private void OpenChannelIfNecessary()
        {
            if (_channel == null)
            {
                OpenChannel();
            }
        }

        private void OpenChannel()
        {
            try
            {
                if (UseService)
                {
                    var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                    var endpoint = new EndpointAddress(WCF_SERVICE_ADDRESS);
                    _channel = ChannelFactory<IUpdater>.CreateChannel(binding, endpoint);
                }
                else
                {
                    _channel = new NoService();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, "Failed to open service connection! :-(");
                throw;
            }
        }
    }
}