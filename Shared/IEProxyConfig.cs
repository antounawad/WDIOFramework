using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Eulg.Shared
{
    public class IEProxyConfig : IWebProxy
    {
        #region AutoProxy Constants

        /// <summary>
        /// Attempt to automatically discover the URL of the PAC file using both DHCP and DNS queries to the local network (WPAD).
        /// </summary>
        private const int WINHTTP_AUTOPROXY_AUTO_DETECT = 0x1;

        /// <summary>
        /// Download the PAC file from the URL in the WINHTTP_AUTOPROXY_OPTIONS structure.
        /// </summary>
        private const int WINHTTP_AUTOPROXY_CONFIG_URL = 0x2;

        private const int WINHTTP_AUTOPROXY_NO_CACHE_CLIENT = 0x00080000;
        private const int WINHTTP_AUTOPROXY_NO_CACHE_SVC = 0x00100000;

        /// <summary>
        /// Use DHCP to locate the proxy auto-configuration file.
        /// </summary>
        private const int WINHTTP_AUTO_DETECT_TYPE_DHCP = 0x1;

        /// <summary>
        /// Use DNS to attempt to locate the proxy auto-configuration file at a well-known location on the domain of the local computer
        /// </summary>
        private const int WINHTTP_AUTO_DETECT_TYPE_DNS_A = 0x2;

        #endregion

        #region Proxy Structures

        /// <summary>
        /// The structure is used to indicate to the WinHttpGetProxyForURL function whether to specify the URL of the Proxy Auto-Configuration (PAC) file or to automatically locate the URL with DHCP or DNS queries to the network.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct AutoProxyOptions
        {
            /// <summary>
            /// Mechanisms should be used to obtain the PAC file
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwFlags;

            /// <summary>
            /// In WPAD mode, specifies the strategies (DHCP, DNS) to use to discover the proxy settings.
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int dwAutoDetectFlags;

            /// <summary>
            /// If dwflags includes the WINHTTP_AUTOPROXY_CONFIG_URL flag, the URL of the proxy auto-configuration (PAC) file, otherwise NULL.
            /// </summary>
            public string lpszAutoConfigUrl;

            private readonly IntPtr lpvReserved;

            [MarshalAs(UnmanagedType.U4)]
            private readonly int dwReserved;

            /// <summary>
            /// Specifies whether the client's domain credentials should be automatically sent in response to an NTLM or Negotiate Authentication challenge when WinHTTP requests the PAC file.
            /// </summary>
            public bool fAutoLoginIfChallenged;

        }

        /// <summary>
        /// The structure contains the session or default proxy configuration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ProxyInfo
        {
            /// <summary>
            /// Unsigned long integer value that contains the access type
            /// </summary>   
            [MarshalAs(UnmanagedType.U4)]
            private readonly int dwAccessType;

            /// <summary>
            /// Pointer to a string value that contains the proxy server list
            /// </summary>
            public readonly string lpszProxy;

            /// <summary>
            /// Pointer to a string value that contains the proxy bypass list
            /// </summary>
            private readonly string lpszProxyBypass;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ProxyConfig
        {
            [MarshalAs(UnmanagedType.Bool)]
            public readonly bool AutoDetect;
            public readonly string AutoConfigUrl;
            public readonly string Proxy;
            public readonly string ProxyBypass;
        }

        #endregion

        #region WinHttp

        [DllImport("winhttp.dll", EntryPoint = "WinHttpGetProxyForUrl", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetProxyForUrl(IntPtr hSession, string lpcwszUrl, ref AutoProxyOptions pAutoProxyOptions, ref ProxyInfo pProxyInfo);

        [DllImport("winhttp.dll", EntryPoint = "WinHttpOpen", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr Open(string pwszUserAgent, int dwAccessType, IntPtr pwszProxyName, IntPtr pwszProxyBypass, int dwFlags);

        [DllImport("winhttp.dll", EntryPoint = "WinHttpCloseHandle", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool Close(IntPtr hInternet);

        [DllImport("winhttp.dll", EntryPoint = "WinHttpGetIEProxyConfigForCurrentUser", SetLastError = true)]
        private static extern bool GetIEProxyConfig(ref ProxyConfig pProxyConfig);

        #endregion

        /// <summary>
        /// Erzeugt eine neue Instanz mit den Proxyeinstellungen des aktuell angemeldeten Benutzers.
        /// </summary>
        public IEProxyConfig()
        {
            var config = new ProxyConfig();
            if(!GetIEProxyConfig(ref config))
            {
                throw GetLastWin32ErrorException();
            }

            IsAutoDetect = config.AutoDetect;
            PacScript = string.IsNullOrEmpty(config.AutoConfigUrl) ? null : new Uri(config.AutoConfigUrl);
            ProxyList = string.IsNullOrEmpty(config.Proxy) ? Enumerable.Empty<KeyValuePair<string, string>>() : ParseProxyList(config.Proxy).ToList();

            var bypassList = new List<string>();
            if(!string.IsNullOrEmpty(config.ProxyBypass))
            {
                bypassList.AddRange(config.ProxyBypass.Split(';').Select(_ => _.Trim()));
                BypassLocal = bypassList.RemoveAll(_ => _.Equals("<local>", StringComparison.OrdinalIgnoreCase)) > 0;
            }

            ProxyBypassList = bypassList;
            NeverUseProxy = !IsAutoDetect && PacScript == null && !ProxyList.Any();
        }

        /// <summary>
        /// Erzeugt eine Instanz, die Proxyeinstellungen aus einem explizit angegebenen PAC-Skript ermittelt.
        /// </summary>
        public IEProxyConfig(Uri pacScript)
        {
            IsAutoDetect = false;
            NeverUseProxy = false;
            PacScript = pacScript;
            ProxyList = Enumerable.Empty<KeyValuePair<string, string>>();
            ProxyBypassList = Enumerable.Empty<string>();
            BypassLocal = false;
        }

        /// <summary>
        /// Proxyeinstellungen werden über WPAD ermittelt.
        /// </summary>
        public bool IsAutoDetect { get; }

        /// <summary>
        /// Niemals Proxy benutzen.
        /// </summary>
        public bool NeverUseProxy { get; }

        /// <summary>
        /// Proxyserver nicht für lokale Adressen benutzen.
        /// </summary>
        public bool BypassLocal { get; }

        /// <summary>
        /// Proxyeinstellungen sind durch das PAC-Skript festgelegt.
        /// </summary>
        public Uri PacScript { get; }

        /// <summary>
        /// Liste manuell konfigurierter Proxyserver (wenn kein PAC-Skript vorliegt).
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ProxyList { get; }

        /// <summary>
        /// Liste von Hosts zu denen direkt verbunden werden soll (wenn kein PAC-Skript vorliegt).
        /// </summary>
        public IEnumerable<string> ProxyBypassList { get; }

        /// <summary>
        /// Ermittelt den zu verwendenden Proxyserver für die angegebene URL. Der Rückgabewert gibt an ob die Verbindung über einen Proxy (<c>true</c>) oder direkt (<c>false</c>) hergestellt werden soll.
        /// </summary>
        public bool GetProxiesForUrl(Uri url, out string[] proxyUrls)
        {
            if(NeverUseProxy)
            {
                proxyUrls = new string[0];
                return false;
            }

            if(!IsAutoDetect && PacScript == null)
            {
                if(ProxyBypassList.Any(bp => bp.Equals(url.Host, StringComparison.OrdinalIgnoreCase)))
                {
                    proxyUrls = new string[0];
                    return false;
                }

                proxyUrls = ProxyList.Where(p => p.Key.Equals(url.Scheme, StringComparison.OrdinalIgnoreCase))
                    .Concat(ProxyList.Where(p => p.Key == null))
                    .Concat(ProxyList.Where(p => p.Key.Equals("http", StringComparison.OrdinalIgnoreCase)))
                    .Select(p => p.Value)
                    .Take(1)
                    .ToArray();

                return proxyUrls.Length > 0;
            }

            var proxyList = GetProxyForUrl(url.OriginalString, IsAutoDetect ? null : PacScript.OriginalString);
            proxyUrls = proxyList.Split(';').Select(_ => _.Trim()).Where(_ => !string.IsNullOrEmpty(_)).ToArray();
            return proxyUrls.Length > 0;
        }

        public override string ToString()
        {
            if(NeverUseProxy)
            {
                return "none";
            }

            if(!IsAutoDetect && PacScript == null)
            {
                var manual = $"manual {string.Join(", ", ProxyList.Select(p => p.Key == null ? p.Value : $"{p.Key}={p.Value}"))}";
                var bypass = string.Join(" and ", new[] { BypassLocal ? "local" : null, string.Join(", ", ProxyBypassList) }.Where(_ => _ != null));

                return string.IsNullOrEmpty(bypass) ? manual : $"{manual}; bypass for {bypass}";
            }

            return IsAutoDetect ? "wpad" : $"auto-config {PacScript.OriginalString}";
        }

        private static string GetProxyForUrl(string url, string pac)
        {
            var session = Open("Proxy Discovery", 0, IntPtr.Zero, IntPtr.Zero, 0);
            if(session == null) throw GetLastWin32ErrorException();

            try
            {
                var options = new AutoProxyOptions();
                var info = new ProxyInfo();

                options.dwFlags = pac == null ? WINHTTP_AUTOPROXY_AUTO_DETECT : WINHTTP_AUTOPROXY_CONFIG_URL;
                options.dwFlags |= WINHTTP_AUTOPROXY_NO_CACHE_CLIENT | WINHTTP_AUTOPROXY_NO_CACHE_SVC;
                options.dwAutoDetectFlags = pac == null ? (WINHTTP_AUTO_DETECT_TYPE_DHCP | WINHTTP_AUTO_DETECT_TYPE_DNS_A) : 0;
                options.lpszAutoConfigUrl = pac;
                options.fAutoLoginIfChallenged = true;

                if(GetProxyForUrl(session, url, ref options, ref info))
                {
                    return info.lpszProxy;
                }

                throw GetLastWin32ErrorException();
            }
            finally
            {
                Close(session);
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> ParseProxyList(string proxyList)
        {
            var entries = proxyList.Split(';');

            return from entry in entries
                   where !string.IsNullOrWhiteSpace(entry)
                   let schemeSeparator = entry.IndexOf('=')
                   select schemeSeparator < 0
                       ? new KeyValuePair<string, string>(null, entry)
                       : new KeyValuePair<string, string>(entry.Substring(0, schemeSeparator), entry.Substring(schemeSeparator + 1));
        }

        #region IWebProxy

        private static readonly Regex _uriSchemeRegex = new Regex(@"^([a-z]+):\/\/", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        Uri IWebProxy.GetProxy(Uri destination)
        {
            string[] proxyUrl;
            if(GetProxiesForUrl(destination, out proxyUrl))
            {
                if(!_uriSchemeRegex.IsMatch(proxyUrl[0]))
                {
                    // Uri scheme is necessary, and we assume HTTP if none is given since we only support HTTP proxies anyway.
                    proxyUrl[0] = $"http://{proxyUrl[0]}";
                }

                return new Uri(proxyUrl[0]);
            }

            return destination;
        }

        bool IWebProxy.IsBypassed(Uri host)
        {
            string[] proxyUrl;
            return !GetProxiesForUrl(host, out proxyUrl);
        }

        ICredentials IWebProxy.Credentials { get; set; }

        #endregion

        #region Errors

        private static Exception GetLastWin32ErrorException()
        {
            var error = Marshal.GetLastWin32Error();
            var winHttpError = (EWinHttpErrors)(error - 12000);
            return Enum.IsDefined(typeof(EWinHttpErrors), winHttpError)
                ? new Win32Exception(error, winHttpError.ToString())
                : new Win32Exception(error);
        }

        private enum EWinHttpErrors
        {
            OutOfHandles = 1,
            Timeout = 2,
            InternalError = 4,
            InvalidUrl = 5,
            UnrecognizedScheme = 6,
            NameNotResolved = 7,
            InvalidOption = 9,
            OptionNotSettable = 11,
            Shutdown = 12,
            LoginFailure = 15,
            OperationCancelled = 17,
            IncorrectHandleType = 18,
            IncorrectHandleState = 19,
            CannotConnect = 29,
            ConnectionError = 30,
            ResendRequest = 32,
            ClientAuthCertNeeded = 44,
            CannotCallBeforeOpen = 100,
            CannotCallBeforeSend = 101,
            CannotCallAfterSend = 102,
            CannotCallAfterOpen = 103,
            HeaderNotFound = 150,
            InvalidServerResponse = 152,
            InvalidHeader = 153,
            InvalidQueryRequest = 154,
            HeaderAlreadyExists = 155,
            RedirectFailed = 156,
            AutoProxyServiceError = 178,
            BadAutoProxyScript = 166,
            UnableToDownloadScript = 167,
            NotInitialized = 172,
            SecureFailure = 175,
            SecureCertDateInvalid = 37,
            SecureCertCnInvalid = 38,
            SecureInvalidCa = 45,
            SecureCertRevFailed = 57,
            SecureChannelError = 157,
            SecureInvalidCert = 169,
            SecureCertRevoked = 170,
            SecureCertWrongUsage = 179,
            AutodetectionFailed = 180,
            HeaderCountExceeded = 181,
            HeaderSizeOverflow = 182,
            ChunkedEncodingHeaderSizeOverflow = 183,
            ResponseDrainOverflow = 184,
            ClientCertNoPrivateKey = 185,
            ClientCertNoAccessPrivateKey = 186
        }

        #endregion
    }
}
