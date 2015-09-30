using System;
using System.Net;
using Microsoft.Win32;

namespace Eulg.Setup.Shared
{
    public static class ProxyConfig
    {
        public enum EProxyType { Default = 0, None = 1, Manual = 2 }
        public static EProxyType ProxyType { get; set; }
        public static string Address { get; set; }
        public static ushort HttpPort { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string Domain { get; set; }

        private const string REG_KEY_SOFTWARE = "Software";
        private const string REG_KEY_PARENT = "EULG Software GmbH";

        public static void Init()
        {
            ReadFromRegistry();
            SetDefault();
        }
        public static void SetDefault()
        {
            WebRequest.DefaultWebProxy = GetWebProxy();
        }

        public static IWebProxy GetWebProxy()
        {
            IWebProxy proxy;
            switch (ProxyType)
            {
                case EProxyType.Default:
                    proxy = WebRequest.GetSystemWebProxy();
                    proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    break;
                case EProxyType.None:
                    proxy = null;
                    break;
                case EProxyType.Manual:
                    proxy = new WebProxy(Address, HttpPort);
                    if (!string.IsNullOrWhiteSpace(Username))
                    {
                        proxy.Credentials = new NetworkCredential(Username, Password, Domain);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return proxy;
        }

        public static void ReadFromRegistry()
        {
            Address = Username = Password = Domain = null;
            HttpPort = 0;
            var keyCuParent = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, false)?.OpenSubKey(REG_KEY_PARENT, false);
            if (keyCuParent != null)
            {
                Address = keyCuParent.GetValue("ProxyAddress", null) as string;
                HttpPort = (ushort)((keyCuParent.GetValue("ProxyHttpPort", null) as int? ?? 0) & 0xffff);
                Username = keyCuParent.GetValue("ProxyUsername", null) as string;
                Password = keyCuParent.GetValue("ProxyPassword", null) as string;
                Domain = keyCuParent.GetValue("ProxyDomain", null) as string;
            }
            if (Address == null)
            {
                var keyLmParent = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, false)?.OpenSubKey(REG_KEY_PARENT, false);
                if (keyLmParent != null)
                {
                    Address = keyLmParent.GetValue("ProxyAddress", null) as string;
                    HttpPort = (ushort)((keyLmParent.GetValue("ProxyHttpPort", null) as int? ?? 0) & 0xffff);
                    Username = keyLmParent.GetValue("ProxyUsername", null) as string;
                    Password = keyLmParent.GetValue("ProxyPassword", null) as string;
                    Domain = keyLmParent.GetValue("ProxyDomain", null) as string;
                }
            }
            if (Address == null)
                ProxyType = EProxyType.Default;
            else if (Address.Equals(string.Empty, StringComparison.InvariantCultureIgnoreCase))
                ProxyType = EProxyType.None;
            else if (HttpPort < 1)
                ProxyType = EProxyType.Default;
            else
                ProxyType = EProxyType.Manual;
        }
        public static void WriteToRegistry()
        {
            var keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true);
            var keyLmParent = keyLmSoftware?.OpenSubKey(REG_KEY_PARENT, true) ?? keyLmSoftware?.CreateSubKey(REG_KEY_PARENT);

            var keyCuSoftware = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true);
            var keyCuParent = keyCuSoftware?.OpenSubKey(REG_KEY_PARENT, true) ?? keyCuSoftware?.CreateSubKey(REG_KEY_PARENT);

            switch (ProxyType)
            {
                case EProxyType.Default:
                    if (keyLmParent != null)
                    {
                        keyLmParent.DeleteValue("ProxyAddress", false);
                        keyLmParent.DeleteValue("ProxyHttpPort", false);
                        keyLmParent.DeleteValue("ProxySocks5Port", false);
                        keyLmParent.DeleteValue("ProxyUsername", false);
                        keyLmParent.DeleteValue("ProxyPassword", false);
                        keyLmParent.DeleteValue("ProxyDomain", false);
                    }
                    if (keyCuParent != null)
                    {
                        keyCuParent.DeleteValue("ProxyAddress", false);
                        keyCuParent.DeleteValue("ProxyHttpPort", false);
                        keyCuParent.DeleteValue("ProxySocks5Port", false);
                        keyCuParent.DeleteValue("ProxyUsername", false);
                        keyCuParent.DeleteValue("ProxyPassword", false);
                        keyCuParent.DeleteValue("ProxyDomain", false);
                    }
                    break;

                case EProxyType.None:
                    keyLmParent.SetValue("ProxyAddress", string.Empty, RegistryValueKind.String);
                    keyLmParent.SetValue("ProxyHttpPort", 0, RegistryValueKind.DWord);
                    keyLmParent.SetValue("ProxyUsername", string.Empty, RegistryValueKind.String);
                    keyLmParent.SetValue("ProxyPassword", string.Empty, RegistryValueKind.String);
                    keyLmParent.SetValue("ProxyDomain", string.Empty, RegistryValueKind.String);

                    keyCuParent.SetValue("ProxyAddress", string.Empty, RegistryValueKind.String);
                    keyCuParent.SetValue("ProxyHttpPort", 0, RegistryValueKind.DWord);
                    keyCuParent.SetValue("ProxyUsername", string.Empty, RegistryValueKind.String);
                    keyCuParent.SetValue("ProxyPassword", string.Empty, RegistryValueKind.String);
                    keyCuParent.SetValue("ProxyDomain", string.Empty, RegistryValueKind.String);
                    break;

                case EProxyType.Manual:
                    keyLmParent.SetValue("ProxyAddress", Address, RegistryValueKind.String);
                    keyLmParent.SetValue("ProxyHttpPort", HttpPort, RegistryValueKind.DWord);
                    keyLmParent.SetValue("ProxyUsername", Username, RegistryValueKind.String);
                    keyLmParent.SetValue("ProxyPassword", Password, RegistryValueKind.String);
                    keyLmParent.SetValue("ProxyDomain", Domain, RegistryValueKind.String);

                    keyCuParent.SetValue("ProxyAddress", Address, RegistryValueKind.String);
                    keyCuParent.SetValue("ProxyHttpPort", HttpPort, RegistryValueKind.DWord);
                    keyCuParent.SetValue("ProxyUsername", Username, RegistryValueKind.String);
                    keyCuParent.SetValue("ProxyPassword", Password, RegistryValueKind.String);
                    keyCuParent.SetValue("ProxyDomain", Domain, RegistryValueKind.String);
                    break;
            }
        }
    }
}
