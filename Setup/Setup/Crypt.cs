using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Eulg.Setup
{
    public static class Crypt
    {
        public const string REG_VAL_NAME_USRNAME = "usr";
        public const string REG_VAL_NAME_PASS = "sep";

        private static readonly SecureString Password;
        private const string SALT_USERNAME = @" mdnfklutz2q89x4§V$&5v0 hds ,cv,.";
        private const string SALT_PASSWORD = @" mdynsui4z234@²6498 rts ä";

        static Crypt()
        {
            Password = new SecureString();
            Password.AppendChar('d');
            Password.AppendChar('s');
            Password.AppendChar('m');
            Password.AppendChar('n');
            Password.AppendChar('0');
            Password.AppendChar('2');
            Password.AppendChar('_');
            Password.AppendChar('8');
            Password.AppendChar('d');
            Password.AppendChar('R');
            Password.AppendChar('e');
            Password.AppendChar('ü');
            Password.AppendChar('4');
            Password.AppendChar('?');
            Password.AppendChar('^');
            Password.AppendChar('#');
        }

        public static string EncryptUsername(string username)
        {
            return Encrypt(username, Password, SALT_USERNAME);
        }

        public static string EncryptPassword(string password)
        {
            return Encrypt(password, Password, SALT_PASSWORD);
        }

        private static string Encrypt(string value, SecureString password, string salt)
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(ConvertToUnsecureString(password), Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new AesManaged();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIv);

            using (MemoryStream buffer = new MemoryStream())
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(value);
                    }
                }

                return Convert.ToBase64String(buffer.ToArray());
            }
        }

        private static string ConvertToUnsecureString(SecureString secureString)
        {
            if (secureString == null)
            {
                throw new ArgumentNullException("secureString");
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
