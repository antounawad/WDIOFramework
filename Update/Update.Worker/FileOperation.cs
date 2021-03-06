﻿using System;
using System.IO;
using System.Linq;
using Eulg.Update.Shared;

namespace Eulg.Update.Worker
{
    public abstract class FileOp
    {
        private static readonly Random _random = new Random(DateTime.Now.Millisecond);

        private enum EState
        {
            None,
            Prepared,
            Committed
        }

        private EState _state = EState.None;

        protected abstract void DoPrepare();
        protected abstract void DoCommit();
        protected abstract void DoRollback();

        protected static string GetRandomBackupSuffix(char opType)
        {
            var randomString = new string(Enumerable.Range(0, 4).Select(_ => (char)('a' - 1 + _random.Next(27))).ToArray());
            return $"{char.ToUpperInvariant(opType)}${randomString}";
        }

        public string TargetFile { get; private set; }

        public void Prepare()
        {
            if (_state != EState.None)
            {
                throw new InvalidOperationException();
            }

            try
            {
                DoPrepare();
                _state = EState.Prepared;
            }
            catch
            {
                DoRollback();
                throw;
            }
        }

        public void Commit()
        {
            if (_state == EState.None)
            {
                throw new InvalidOperationException();
            }
            if (_state == EState.Committed)
            {
                return;
            }

            DoCommit();
            _state = EState.Committed;
        }

        public void Rollback()
        {
            if (_state == EState.Committed)
            {
                throw new InvalidOperationException();
            }
            if (_state == EState.None)
            {
                return;
            }

            try
            {
                DoRollback();
            }
            catch (Exception ex)
            {
                // Rollback must not throw
                Log.Write(ex, "Error while reverting {0}", GetType().Name);
            }
            finally
            {
                _state = EState.None;
            }
        }

        protected IUpdater Service { get; private set; }

        public static FileOp Copy(IUpdater service, string source, string dest, bool resetFile)
        {
            if (resetFile)
            {
                return new FileCopyReset(source, dest)
                {
                    Service = service,
                    TargetFile = dest
                };
            }

            return new FileCopy(source, dest)
            {
                Service = service,
                TargetFile = dest
            };
        }

        public static FileOp Delete(IUpdater service, string target)
        {
            if (!File.Exists(target) && !Directory.Exists(target))
            {
                return null;
            }

            return new FileDelete(target)
            {
                Service = service,
                TargetFile = target
            };
        }

        #region Copy

        private class FileCopy : FileOp
        {
            private readonly string _source;
            private readonly string _dest;
            private readonly string _backup;

            private bool _haveBackup;

            public FileCopy(string source, string dest)
            {
                _source = source;
                _dest = dest;
                _backup = dest + $".{GetRandomBackupSuffix('U')}";
            }

            protected override void DoPrepare()
            {
                _haveBackup = false;

                if (!Directory.Exists(Path.GetDirectoryName(_dest) ?? ""))
                {
                    Service.DirectoryCreate(Path.GetDirectoryName(_dest) ?? "");
                    Service.DirectorySetAccessControlWorld(Path.GetDirectoryName(_dest) ?? "");
                }

                if (File.Exists(_dest))
                {
                    if (File.Exists(_backup))
                    {
                        Service.FileDelete(_backup);
                    }

                    Service.FileMove(_dest, _backup);
                    _haveBackup = true;
                }

                Service.FileMove(_source, _dest);
                if (File.Exists(_source))
                {
                    throw new IOException($"Moving '{_source}' to '{_dest}' failed (source still exists)");
                }

                Service.FileSetAccessControlInherit(_dest);
            }

            protected override void DoCommit()
            {
                if (!_haveBackup)
                {
                    return;
                }

                try
                {
                    Service.FileDelete(_backup);
                }
                catch(Exception ex)
                {
                    Log.Write(UpdateWorker.LogTypeEnum.Warning, "Failed to remove backup; non-critical but annoying [{0}: {1}]", ex.GetType().Name, ex.Message);
                }
            }

            protected override void DoRollback()
            {
                if (!_haveBackup || !File.Exists(_backup))
                {
                    return;
                }

                if (File.Exists(_dest))
                {
                    Service.FileDelete(_dest);
                }

                Service.FileMove(_backup, _dest);
            }
        }

        #endregion

        #region Copy (Reset file)

        private class FileCopyReset : FileCopy
        {
            private readonly string _source;
            private readonly string _dest;
            private readonly string _backup;

            private bool _haveBackup;

            public FileCopyReset(string source, string dest)
                : base(source, dest)
            {
                _source = dest;
                _dest = dest.Replace(UpdateWorker.ResetFileTag, "");
                _backup = _dest + $".{GetRandomBackupSuffix('R')}";
            }

            protected override void DoPrepare()
            {
                base.DoPrepare();

                if (File.Exists(_dest))
                {
                    if (File.Exists(_backup))
                    {
                        Service.FileDelete(_backup);
                    }

                    Service.FileMove(_dest, _backup);
                    _haveBackup = true;
                }
            }

            protected override void DoCommit()
            {
                base.DoCommit();

                Service.FileCopy(_source, _dest, true);
                if (!_haveBackup)
                {
                    return;
                }

                try
                {
                    Service.FileDelete(_backup);
                }
                catch(Exception ex)
                {
                    Log.Write(UpdateWorker.LogTypeEnum.Warning, "Failed to remove backup; non-critical but annoying [{0}: {1}]", ex.GetType().Name, ex.Message);
                }
            }

            protected override void DoRollback()
            {
                base.DoRollback();

                if (!_haveBackup || !File.Exists(_backup))
                {
                    return;
                }

                Service.FileMove(_backup, _dest);
            }
        }

        #endregion

        #region Delete

        private class FileDelete : FileOp
        {
            private readonly string _target;
            private readonly string _backup;
            private readonly bool _targetIsDirectory;

            private bool _haveBackup;

            public FileDelete(string target)
            {
                _target = target;
                _backup = target + $".{GetRandomBackupSuffix('D')}";

                _targetIsDirectory = Directory.Exists(_target);
            }

            protected override void DoPrepare()
            {
                _haveBackup = false;
                if (_targetIsDirectory)
                {
                    if (Directory.Exists(_backup))
                    {
                        DelTree(_backup);
                    }
                    if (Directory.Exists(_target))
                    {
                        Service.DirectoryMove(_target, _backup);
                        _haveBackup = true;
                    }
                }
                else
                {
                    if (File.Exists(_backup))
                    {
                        Service.FileDelete(_backup);
                    }
                    if (File.Exists(_target))
                    {
                        Service.FileMove(_target, _backup);
                        _haveBackup = true;
                    }
                }
            }

            protected override void DoCommit()
            {
                try
                {
                    if (_targetIsDirectory && Directory.Exists(_backup))
                    {
                        DelTree(_backup);
                    }
                    else if (!_targetIsDirectory && File.Exists(_backup))
                    {
                        Service.FileDelete(_backup);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(UpdateWorker.LogTypeEnum.Warning, "Failed to remove backup; non-critical but annoying [{0}: {1}]", ex.GetType().Name, ex.Message);
                }
            }

            protected override void DoRollback()
            {
                if (!_haveBackup)
                {
                    return;
                }
                if (_targetIsDirectory)
                {
                    if (!Directory.Exists(_backup))
                    {
                        return;
                    }
                    if (Directory.Exists(_target))
                    {
                        return;
                    }
                    Service.DirectoryMove(_backup, _target);
                }
                else
                {
                    if (!File.Exists(_backup))
                    {
                        return;
                    }
                    if (File.Exists(_target))
                    {
                        return;
                    }
                    Service.FileMove(_backup, _target);
                }
            }

            private void DelTree(string path)
            {
                foreach (var f in Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    Service.FileDelete(f);
                }

                foreach (var d in Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    DelTree(d);
                }
                Service.DirectoryDelete(path, false);
            }
        }

        #endregion
    }
}