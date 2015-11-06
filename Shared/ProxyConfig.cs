using System;
using System.Net;
using Microsoft.Win32;

namespace Eulg.Shared
{
    public static class ProxyConfig
    {
        public enum EProxyType { Default = 0, None = 1, Manual = 2 }
        public static EProxyType ProxyType { get; set; }
        public static string Address { get; set; }
        public static ushort? HttpPort { get; set; }
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
                    try
                    {
                        proxy = new WebProxy(Address, HttpPort.GetValueOrDefault(3128));
                        if (!string.IsNullOrWhiteSpace(Username))
                        {
                            proxy.Credentials = new NetworkCredential(Username, Password, Domain);
                        }
                    }
                    catch
                    {
                        proxy = WebRequest.GetSystemWebProxy();
                        proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
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
            try
            {
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
            }
            catch
            {
                // ignored
            }
            if (Address != null && Address.Equals("*")) Address = null;
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
            if (ProxyType == EProxyType.Manual)
            {
                if (string.IsNullOrWhiteSpace(Address) || Address.Equals("*") || HttpPort == null || HttpPort < 1)
                    ProxyType = EProxyType.Default;
            }

            // ReSharper disable once TooWideLocalVariableScope
            RegistryKey keyLmSoftware;
            RegistryKey keyLmParent = null;
            try
            {
                keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true);
                keyLmParent = keyLmSoftware?.OpenSubKey(REG_KEY_PARENT, true) ?? keyLmSoftware?.CreateSubKey(REG_KEY_PARENT);
            }
            catch
            {
                // ignored: nur mir Admin-Rechten in HKEY_LOCAL_MACHINE
            }

            var keyCuSoftware = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true);
            var keyCuParent = keyCuSoftware?.OpenSubKey(REG_KEY_PARENT, true) ?? keyCuSoftware?.CreateSubKey(REG_KEY_PARENT);

            switch (ProxyType)
            {
                case EProxyType.Default:
                    Address = "*";
                    HttpPort = null;
                    Username = null;
                    Password = null;
                    Domain = null;
                    break;

                case EProxyType.None:
                    Address = "";
                    HttpPort = null;
                    Username = null;
                    Password = null;
                    Domain = null;
                    break;

                case EProxyType.Manual:
                    break;
            }

            foreach (var key in new[] { keyCuParent, keyLmParent })
            {
                if (key != null)
                {
                    if (Address != null) key.SetValue("ProxyAddress", Address, RegistryValueKind.String); else key.DeleteValue("ProxyAddress", false);
                    if (HttpPort != null) key.SetValue("ProxyHttpPort", HttpPort, RegistryValueKind.DWord); else key.DeleteValue("ProxyHttpPort", false);
                    if (Username != null) key.SetValue("ProxyUsername", Username, RegistryValueKind.String); else key.DeleteValue("ProxyUsername", false);
                    if (Password != null) key.SetValue("ProxyPassword", Password, RegistryValueKind.String); else key.DeleteValue("ProxyPassword", false);
                    if (Domain != null) key.SetValue("ProxyDomain", Domain, RegistryValueKind.String); else key.DeleteValue("ProxyDomain", false);
                    key.DeleteValue("ProxySocks5Port", false);
                }
            }

        }
    }
}
