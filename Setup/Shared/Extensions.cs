using System;
using System.ComponentModel;

namespace Eulg.Setup.Shared
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
    }
}
