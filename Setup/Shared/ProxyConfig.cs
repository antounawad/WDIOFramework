using System.Net;
using Microsoft.Win32;

namespace Eulg.Setup.Shared
{
    public class ProxyConfig
    {
        public string Address { get; set; }
        public ushort HttpPort { get; set; }
        public ushort Socks5Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        public static readonly ProxyConfig Instance = new ProxyConfig();

        private const string REG_KEY_SOFTWARE = "Software";
        private const string REG_KEY_PARENT = "EULG Software GmbH";

        public static void Init()
        {
            Instance.ReadFromRegistry();
            Instance.SetDefault();
        }

        public void SetDefault()
        {
            if (Instance.Address != null)
            {
                if (!string.IsNullOrWhiteSpace(Instance.Address))
                {
                    WebRequest.DefaultWebProxy = new WebProxy(Address, HttpPort);
                    if (!string.IsNullOrWhiteSpace(Username))
                    {
                        WebRequest.DefaultWebProxy.Credentials = new NetworkCredential(Username, Password, Domain);
                    }
                }
                else
                {
                    WebRequest.DefaultWebProxy = null;
                }
            }
            else
            {
                WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
                WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
        }

        public IWebProxy GetWebProxy()
        {
            return WebRequest.DefaultWebProxy;
        }

        public void ReadFromRegistry()
        {
            Username = Password = Domain = Address = null;
            HttpPort = Socks5Port = 0;

            var keyCuSoftware = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, false);
            var keyCuKs = keyCuSoftware?.OpenSubKey(REG_KEY_PARENT, false);
            if (keyCuKs != null)
            {
                Address = keyCuKs.GetValue("ProxyAddress", null) as string;
                HttpPort = (ushort)((keyCuKs.GetValue("ProxyHttpPort", null) as int? ?? 0) & 0xffff);
                Socks5Port = (ushort)((keyCuKs.GetValue("ProxySocks5Port", null) as int? ?? 0) & 0xffff);
                Username = keyCuKs.GetValue("ProxyUsername", null) as string;
                Password = keyCuKs.GetValue("ProxyPassword", null) as string;
                Domain = keyCuKs.GetValue("ProxyDomain", null) as string;
            }
            if (Address == null && Username == null)
            {
                var keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, false);
                var keyLmKs = keyLmSoftware?.OpenSubKey(REG_KEY_PARENT, false);
                if (keyLmKs != null)
                {
                    Address = keyLmKs.GetValue("ProxyAddress", null) as string;
                    HttpPort = (ushort)((keyLmKs.GetValue("ProxyHttpPort", null) as int? ?? 0) & 0xffff);
                    Socks5Port = (ushort)((keyLmKs.GetValue("ProxySocks5Port", null) as int? ?? 0) & 0xffff);
                    Username = keyLmKs.GetValue("ProxyUsername", null) as string;
                    Password = keyLmKs.GetValue("ProxyPassword", null) as string;
                    Domain = keyLmKs.GetValue("ProxyDomain", null) as string;
                }
            }
        }

        public void WriteToRegistry()
        {
            var keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true);
            var keyLmKs = keyLmSoftware.OpenSubKey(REG_KEY_PARENT, true) ?? keyLmSoftware.CreateSubKey(REG_KEY_PARENT);
            var keyCuSoftware = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true);
            var keyCuKs = keyCuSoftware.OpenSubKey(REG_KEY_PARENT, true) ?? keyCuSoftware.CreateSubKey(REG_KEY_PARENT);
            if (Address != null)
            {
                keyLmKs.SetValue("ProxyAddress", Address, RegistryValueKind.String);
                keyLmKs.SetValue("ProxyHttpPort", HttpPort, RegistryValueKind.DWord);
                keyLmKs.SetValue("ProxySocks5Port", Socks5Port, RegistryValueKind.DWord);
                keyLmKs.SetValue("ProxyUsername", Username, RegistryValueKind.String);
                keyLmKs.SetValue("ProxyPassword", Password, RegistryValueKind.String);
                keyLmKs.SetValue("ProxyDomain", Domain, RegistryValueKind.String);

                keyCuKs.SetValue("ProxyAddress", Address, RegistryValueKind.String);
                keyCuKs.SetValue("ProxyHttpPort", HttpPort, RegistryValueKind.DWord);
                keyCuKs.SetValue("ProxySocks5Port", Socks5Port, RegistryValueKind.DWord);
                keyCuKs.SetValue("ProxyUsername", Username, RegistryValueKind.String);
                keyCuKs.SetValue("ProxyPassword", Password, RegistryValueKind.String);
                keyCuKs.SetValue("ProxyDomain", Domain, RegistryValueKind.String);
            }
            else
            {
                if (keyLmKs != null)
                {
                    keyLmKs.DeleteValue("ProxyAddress", false);
                    keyLmKs.DeleteValue("ProxyHttpPort", false);
                    keyLmKs.DeleteValue("ProxySocks5Port", false);
                    keyLmKs.DeleteValue("ProxyUsername", false);
                    keyLmKs.DeleteValue("ProxyPassword", false);
                    keyLmKs.DeleteValue("ProxyDomain", false);
                }

                if (keyCuKs != null)
                {
                    keyCuKs.DeleteValue("ProxyAddress", false);
                    keyCuKs.DeleteValue("ProxyHttpPort", false);
                    keyCuKs.DeleteValue("ProxySocks5Port", false);
                    keyCuKs.DeleteValue("ProxyUsername", false);
                    keyCuKs.DeleteValue("ProxyPassword", false);
                    keyCuKs.DeleteValue("ProxyDomain", false);
                }
            }
        }
    }
}
