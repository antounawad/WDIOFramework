using System;
using System.ComponentModel;
using System.IO;

namespace Eulg.Shared
{
    public static class Extensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static string GetMessagesTree(this Exception exception)
        {
            var tmp = exception.Message;
            var ex = exception;
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                tmp += Environment.NewLine + ex.Message;
            }
            tmp += Environment.NewLine + exception.StackTrace;
            return tmp;
        }

        public static string EnsureTrailing(this string input, string suffix, StringComparison stringComparison = StringComparison.Ordinal)
        {
            return !input.EndsWith(suffix, stringComparison) ? input + suffix : input;
        }

        public static byte[] Read(this Stream stream, int count)
        {
            var buffer = new byte[count];
            var offset = 0;

            while(offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if(read == 0)
                {
                    throw new EndOfStreamException();
                }

                offset += read;
            }

            return buffer;
        }

        public static void Copy(this Stream stream, Stream dest, int count)
        {
            var buffer = new byte[8192];
            var offset = 0;

            while(offset < count)
            {
                var read = stream.Read(buffer, 0, Math.Min(buffer.Length, count - offset));
                if(read == 0)
                {
                    throw new EndOfStreamException();
                }

                dest.Write(buffer, 0, read);
                offset += read;
            }
        }
    }
}
