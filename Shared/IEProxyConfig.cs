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

        /// <summary>
        /// Execute proxy detection in the same process instead of querying a system service.
        /// </summary>
        private const int WINHTTP_AUTOPROXY_RUN_INPROCESS = 0x10000;

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
        public bool IsAutoDetect { get; private set; }

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
            proxyUrls = proxyList?.Split(';').Select(_ => _.Trim()).Where(_ => !string.IsNullOrEmpty(_)).ToArray() ?? new string[0];
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

        private string GetProxyForUrl(string url, string pac)
        {
            var session = Open("Proxy Discovery", 0, IntPtr.Zero, IntPtr.Zero, 0);
            if(session == null) throw GetLastWin32ErrorException();

            try
            {
                var options = new AutoProxyOptions();
                var info = new ProxyInfo();

                options.dwFlags = (pac == null ? WINHTTP_AUTOPROXY_AUTO_DETECT : WINHTTP_AUTOPROXY_CONFIG_URL) | WINHTTP_AUTOPROXY_RUN_INPROCESS;
                options.dwAutoDetectFlags = pac == null ? (WINHTTP_AUTO_DETECT_TYPE_DHCP | WINHTTP_AUTO_DETECT_TYPE_DNS_A) : 0;
                options.lpszAutoConfigUrl = pac;
                options.fAutoLoginIfChallenged = true;

                if(GetProxyForUrl(session, url, ref options, ref info))
                {
                    return info.lpszProxy;
                }

                var errno = Marshal.GetLastWin32Error();
                if(errno == (int)EWinHttpErrors.AutodetectionFailed || errno == (int)EWinHttpErrors.UnableToDownloadScript || errno == 1168)
                {
                    IsAutoDetect = false;
                    return "";
                }

                throw new Exception($"Proxy-Konfiguration konnte nicht automatisch ermittelt werden (Fehler [{(EWinHttpErrors)errno}]). Bitte automatische Konfiguration deaktivieren und erneut versuchen.", GetLastWin32ErrorException());
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
            return Enum.IsDefined(typeof(EWinHttpErrors), error)
                ? new Win32Exception(error, ((EWinHttpErrors)error).ToString())
                : new Win32Exception(error);
        }

        private const int WIN_HTTP_ERROR_BASE = 12000;

        private enum EWinHttpErrors
        {
            AccessDenied = 5,
            OutOfHandles = WIN_HTTP_ERROR_BASE + 1,
            Timeout = WIN_HTTP_ERROR_BASE + 2,
            InternalError = WIN_HTTP_ERROR_BASE + 4,
            InvalidUrl = WIN_HTTP_ERROR_BASE + 5,
            UnrecognizedScheme = WIN_HTTP_ERROR_BASE + 6,
            NameNotResolved = WIN_HTTP_ERROR_BASE + 7,
            InvalidOption = WIN_HTTP_ERROR_BASE + 9,
            OptionNotSettable = WIN_HTTP_ERROR_BASE + 11,
            Shutdown = WIN_HTTP_ERROR_BASE + 12,
            LoginFailure = WIN_HTTP_ERROR_BASE + 15,
            OperationCancelled = WIN_HTTP_ERROR_BASE + 17,
            IncorrectHandleType = WIN_HTTP_ERROR_BASE + 18,
            IncorrectHandleState = WIN_HTTP_ERROR_BASE + 19,
            CannotConnect = WIN_HTTP_ERROR_BASE + 29,
            ConnectionError = WIN_HTTP_ERROR_BASE + 30,
            ResendRequest = WIN_HTTP_ERROR_BASE + 32,
            ClientAuthCertNeeded = WIN_HTTP_ERROR_BASE + 44,
            CannotCallBeforeOpen = WIN_HTTP_ERROR_BASE + 100,
            CannotCallBeforeSend = WIN_HTTP_ERROR_BASE + 101,
            CannotCallAfterSend = WIN_HTTP_ERROR_BASE + 102,
            CannotCallAfterOpen = WIN_HTTP_ERROR_BASE + 103,
            HeaderNotFound = WIN_HTTP_ERROR_BASE + 150,
            InvalidServerResponse = WIN_HTTP_ERROR_BASE + 152,
            InvalidHeader = WIN_HTTP_ERROR_BASE + 153,
            InvalidQueryRequest = WIN_HTTP_ERROR_BASE + 154,
            HeaderAlreadyExists = WIN_HTTP_ERROR_BASE + 155,
            RedirectFailed = WIN_HTTP_ERROR_BASE + 156,
            AutoProxyServiceError = WIN_HTTP_ERROR_BASE + 178,
            BadAutoProxyScript = WIN_HTTP_ERROR_BASE + 166,
            UnableToDownloadScript = WIN_HTTP_ERROR_BASE + 167,
            NotInitialized = WIN_HTTP_ERROR_BASE + 172,
            SecureFailure = WIN_HTTP_ERROR_BASE + 175,
            SecureCertDateInvalid = WIN_HTTP_ERROR_BASE + 37,
            SecureCertCnInvalid = WIN_HTTP_ERROR_BASE + 38,
            SecureInvalidCa = WIN_HTTP_ERROR_BASE + 45,
            SecureCertRevFailed = WIN_HTTP_ERROR_BASE + 57,
            SecureChannelError = WIN_HTTP_ERROR_BASE + 157,
            SecureInvalidCert = WIN_HTTP_ERROR_BASE + 169,
            SecureCertRevoked = WIN_HTTP_ERROR_BASE + 170,
            SecureCertWrongUsage = WIN_HTTP_ERROR_BASE + 179,
            AutodetectionFailed = WIN_HTTP_ERROR_BASE + 180,
            HeaderCountExceeded = WIN_HTTP_ERROR_BASE + 181,
            HeaderSizeOverflow = WIN_HTTP_ERROR_BASE + 182,
            ChunkedEncodingHeaderSizeOverflow = WIN_HTTP_ERROR_BASE + 183,
            ResponseDrainOverflow = WIN_HTTP_ERROR_BASE + 184,
            ClientCertNoPrivateKey = WIN_HTTP_ERROR_BASE + 185,
            ClientCertNoAccessPrivateKey = WIN_HTTP_ERROR_BASE + 186
        }

        #endregion
    }
}
