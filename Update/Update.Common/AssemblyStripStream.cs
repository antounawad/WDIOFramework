using System;
using System.IO;
using System.Text;

namespace Eulg.Update.Common
{
    /// <summary>
    /// A stream which overwrites certain bytes with zeros upon reading, namely the linker timestamp, the random module ID
    /// and 16 more mysterious bytes which appear to be a random identifier possibly for a resource section.
    /// </summary>
    public class AssemblyStripStream : Stream
    {
        private readonly Stream _inner;
        private readonly byte[] _linkerTimestampBytes;
        private readonly int _linkerTimestampOffset1;
        private readonly int _linkerTimestampOffset2;
        private readonly byte[] _moduleIdBytes;
        private readonly int _moduleIdOffset;
        private readonly byte[] _moduleAsciiIdBytes;
        private readonly int _moduleAsciiIdOffset;
        private readonly byte[] _mysteriousRsdsBytes;
        private readonly int _mysteriousRsdsOffset;

        public AssemblyStripStream(Stream inner, Guid mvId)
        {
            if(!inner.CanSeek)
                throw new ArgumentException("Stream must be seekable");

            _inner = inner;
            _linkerTimestampBytes = GetLinkerTimestampBytes(out _linkerTimestampOffset1);
            _linkerTimestampOffset2 = FindByteSequenceBackwards(_linkerTimestampBytes);
            _moduleIdBytes = mvId.ToByteArray();
            _moduleIdOffset = FindByteSequenceBackwards(_moduleIdBytes);
            _moduleAsciiIdBytes = Encoding.ASCII.GetBytes(mvId.ToString().ToUpperInvariant());
            _moduleAsciiIdOffset = FindByteSequenceBackwards(_moduleAsciiIdBytes);
            _mysteriousRsdsBytes = GetMysteriousRsdsBytes(_linkerTimestampOffset2, out _mysteriousRsdsOffset);
        }

        public override bool CanRead
        {
            get { return _inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _inner.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _inner.CanWrite; }
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override long Length
        {
            get { return _inner.Length; }
        }

        public override long Position
        {
            get { return _inner.Position; }
            set { _inner.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var position = _inner.Position;
            var result = _inner.Read(buffer, offset, count);
            for(var n = _linkerTimestampOffset1; _linkerTimestampOffset1 >= 0 && n < _linkerTimestampOffset1 + _linkerTimestampBytes.Length; ++n)
            {
                if(n - position >= 0 && n - position < count)
                {
                    buffer[n - position + offset] = 0;
                }
            }
            for(var n = _linkerTimestampOffset2; _linkerTimestampOffset2 >= 0 && n < _linkerTimestampOffset2 + _linkerTimestampBytes.Length; ++n)
            {
                if(n - position >= 0 && n - position < count)
                {
                    buffer[n - position + offset] = 0;
                }
            }
            for(var n = _moduleIdOffset; _moduleIdOffset >= 0 && n < _moduleIdOffset + _moduleIdBytes.Length; ++n)
            {
                if(n - position >= 0 && n - position < count)
                {
                    buffer[n - position + offset] = 0;
                }
            }
            for(var n = _moduleAsciiIdOffset; _moduleAsciiIdOffset >= 0 && n < _moduleAsciiIdOffset + _moduleAsciiIdBytes.Length; ++n)
            {
                if(n - position >= 0 && n - position < count)
                {
                    buffer[n - position + offset] = 0;
                }
            }
            for(var n = _mysteriousRsdsOffset; _mysteriousRsdsOffset >= 0 && n < _mysteriousRsdsOffset + _mysteriousRsdsBytes.Length; ++n)
            {
                if(n - position >= 0 && n - position < count)
                {
                    buffer[n - position + offset] = 0;
                }
            }
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
        }

        private byte[] GetLinkerTimestampBytes(out int offset)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;

            var oldPosition = Position;
            var buffer32Bit = new byte[4];

            try
            {
                _inner.Position = peHeaderOffset;
                _inner.Read(buffer32Bit, 0, 4);
                offset = BitConverter.ToInt32(buffer32Bit, 0) + linkerTimestampOffset;

                _inner.Position = offset;
                _inner.Read(buffer32Bit, 0, 4);
                return buffer32Bit;
            }
            finally
            {
                _inner.Position = oldPosition;
            }
        }

        private byte[] GetMysteriousRsdsBytes(int secondLinkerTimestampOffset, out int offset)
        {
            offset = -1;
            if(secondLinkerTimestampOffset < 0)
            {
                return null;
            }

            var oldPosition = Position;
            var signature = new byte[4];
            var mysteriousBytes = new byte[16];

            try
            {
                _inner.Position = secondLinkerTimestampOffset + 24;

                _inner.Read(signature, 0, 4);
                if(Encoding.ASCII.GetString(signature) == "RSDS")
                {
                    offset = (int)_inner.Position;
                    _inner.Read(mysteriousBytes, 0, 16);
                    return mysteriousBytes;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                _inner.Position = oldPosition;
            }
        }

        private int FindByteSequenceBackwards(byte[] sequence)
        {
            var buffer = new byte[8192];
            var oldPosition = Position;

            try
            {
                _inner.Position = _inner.Length;
                while(true)
                {
                    _inner.Position = Math.Max(0, _inner.Position - buffer.Length);
                    var count = _inner.Read(buffer, 0, buffer.Length);
                    if(count < sequence.Length)
                    {
                        throw new InvalidOperationException(":-(");
                    }

                    for(var n = count - sequence.Length - 1; n >= 0; --n)
                    {
                        int m;
                        for(m = 0; m < sequence.Length; ++m)
                        {
                            if(buffer[n + m] != sequence[m])
                                break;
                        }

                        if(m == sequence.Length)
                        {
                            return (int)(_inner.Position - count + n);
                        }
                    }

                    _inner.Position -= count;
                    if(_inner.Position == 0)
                    {
                        return -1;
                    }
                }
            }
            finally
            {
                _inner.Position = oldPosition;
            }
        }
    }
}
