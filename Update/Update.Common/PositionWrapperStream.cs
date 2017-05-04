using System;
using System.IO;

namespace Eulg.Update.Common
{
    // Wrapper für Stream um ZipArchive an Response.OutputStream zu hängen
    // http://stackoverflow.com/questions/16585488/writing-to-ziparchive-using-the-httpcontext-outputstream
    // https://connect.microsoft.com/VisualStudio/feedback/details/816411/ziparchive-shouldnt-read-the-position-of-non-seekable-streams
    public class PositionWrapperStream : Stream
    {
        private readonly Stream wrapped;
        private int pos;
        public PositionWrapperStream(Stream wrapped)
        {
            this.wrapped = wrapped;
        }
        public override bool CanRead { get { throw new NotImplementedException(); } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { throw new NotImplementedException(); } }
        public override long Position { get { return pos; } set { throw new NotSupportedException(); } }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            pos += count;
            wrapped.Write(buffer, offset, count);
        }
        public override void Flush()
        {
            wrapped.Flush();
        }
        protected override void Dispose(bool disposing)
        {
            wrapped.Dispose();
            base.Dispose(disposing);
        }
    }
}
