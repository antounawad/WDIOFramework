﻿using System;
using System.IO;

namespace Eulg.Update.Common
{
    internal class SectionStream : Stream
    {
        private static readonly byte[] _sinkBuffer = new byte[4096];
        private readonly Stream _underlying;
        private readonly long _length;
        private long _position;

        public SectionStream(Stream underlying, long length)
        {
            _underlying = underlying;
            _length = length;
            _position = 0;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return _position; }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = _length - _position;
            if (remaining == 0)
            {
                return 0;
            }

            var n = _underlying.Read(buffer, offset, (int)Math.Min(count, remaining));
            if (_position == 0 && n >= 2 && buffer[0] == 66 && buffer[1] == 67)
            {
                buffer[0] = 31;
                buffer[1] = 139;
            }
            _position += n;
            return n;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            AdvanceToEnd();
        }

        public void AdvanceToEnd()
        {
            while (_position != _length)
            {
                Read(_sinkBuffer, 0, _sinkBuffer.Length);
            }
        }
    }
}
