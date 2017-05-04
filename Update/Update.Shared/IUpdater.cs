using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;

namespace Eulg.Update.Shared
{
    [ServiceContract]
    public interface IUpdater
    {
        [OperationContract]
        void DirectoryCreate(string path);

        [OperationContract]
        void DirectoryMove(string sourceDirName, string destDirName);

        [OperationContract]
        void DirectoryDelete(string path, bool recursive);

        [OperationContract]
        void DirectorySetAccessControlWorld(string path);

        [OperationContract]
        void DirectorySetLastWriteTime(string path, DateTime lastWriteTime);

        [OperationContract]
        void FileCopy(string sourceFileName, string destFileName, bool overwrite);

        [OperationContract]
        void FileMove(string sourceFileName, string destFileName);

        [OperationContract]
        void FileDelete(string path);

        [OperationContract]
        void FileSetAttributes(string path, FileAttributes fileAttributes);

        [OperationContract]
        void FileSetAccessControlInherit(string path);

        [OperationContract]
        void FileSetLastWriteTime(string path, DateTime lastWriteTime);

        [OperationContract]
        void ProcessStart(WorkerProcess processStartInfo, bool wait);

        [OperationContract]
        Process[] GetProcessesByName(string processName);

        [OperationContract]
        Process GetProcessById(int processId);

        [OperationContract]
        void KillProcessesByName(string processName);

        [OperationContract]
        void KillProcessById(int processId);

        [OperationContract]
        void RestartWindows();

        [OperationContract]
        void ServiceStart(string serviceName, bool wait);

        [OperationContract]
        void ServiceStop(string serviceName, bool wait);

        [OperationContract]
        string[] GetExceptions(bool clear);
    }
}