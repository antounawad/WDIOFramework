using System;
using System.Linq;

namespace Admin.Helpers
{
    public static class ExceptionExtensions
    {
        public static string GetMessagesTree(this Exception exception)
        {
            var tmp = exception.Message;
            while(exception.InnerException != null)
            {
                exception = exception.InnerException;
                tmp += Environment.NewLine + exception.Message;
            }
            return tmp;
        }
    }
}