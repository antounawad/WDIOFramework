using System;

namespace Eulg.Update.Shared
{
    internal static class Extensions
    {
        public static string GetMessagesTree(this Exception exception)
        {
            var tmp = exception.Message;
            var ex = exception;
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                tmp += Environment.NewLine + ex.Message;
            }
            return tmp;
        }
    }
}