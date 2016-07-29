using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Eulg.Shared
{
    public enum EApiResource : ushort
    {
        /// <summary>
        /// Liefert die URL zum API-Dienst selbst. Wird genutzt um Clients, die den Dienst über alte URLs (Rückwärtskompatibilität) erreichen, die aktuelle URL mitzuteilen.
        /// </summary>
        Self = 0,

        /// <summary>
        /// URL zum Update-Dienst.
        /// </summary>
        UpdateService = 1,

        /// <summary>
        /// URL zum Login-Dienst.
        /// </summary>
        LoginService = 2,

        /// <summary>
        /// Sync-Endpunkt für HTTP-basierten Sync
        /// </summary>
        HttpSync = 4,

        /// <summary>
        /// Portal
        /// </summary>
        WebHome = 100,

        /// <summary>
        /// Verwaltung
        /// </summary>
        WebAdministration = 101,

        /// <summary>
        /// Nutzungsbedingungen (Test)
        /// </summary>
        TrialEula = 102,

        /// <summary>
        /// Nutzungsbedingungen (Normal)
        /// </summary>
        RegularEula = 103,

        /// <summary>
        /// Lizenzpaket kaufen/verlängern
        /// </summary>
        PurchaseLicense = 104,

        /// <summary>
        /// Passwort vergessen/zurücksetzen
        /// </summary>
        RecoverPassword = 105,

        /// <summary>
        /// Change Log
        /// </summary>
        ChangeLog = 106
    }

    public class ApiResourceClient
    {
        //HACK Um externe Abhängigkeiten zu vermeiden. In einer der nächsten Versionen NewtonSoft.Json per ILMerge rein?
        private static readonly Regex _manifestParser = new Regex(@"^\{(""Error"":""(.+)""|""Manifest"":\[(,?\{""Api"":(\d+),""Uri"":""(.+?)""\})*\])\}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly string _serviceUrl;
        private readonly Branding.EUpdateChannel _channel;

        public ApiResourceClient(string serviceUrl, Branding.EUpdateChannel channel)
        {
            _serviceUrl = serviceUrl;
            _channel = channel;
        }

        /// <summary>
        /// URLs für die verschiedenen Dienste und Ressourcen abrufen. Es werden Exceptions geworfen für alles was schiefläuft, unbedingt im try/catch aufrufen.
        /// </summary>
        public IDictionary<EApiResource, Uri> Fetch()
        {
            var request = (HttpWebRequest)WebRequest.Create(_serviceUrl + $"?chan={_channel}");
            request.Proxy = WebRequest.DefaultWebProxy;
            request.AutomaticDecompression = DecompressionMethods.None;
            request.Method = WebRequestMethods.Http.Get;
            request.KeepAlive = false;

            using(var response = (HttpWebResponse)request.GetResponse())
            {
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    Match match;
                    using (var buffer = new MemoryStream())
                    {
                        using(var responseStream = response.GetResponseStream())
                        {
                            responseStream.CopyTo(buffer);
                        }

                        var content = Encoding.UTF8.GetString(buffer.ToArray());
                        match = _manifestParser.Match(content);
                    }

                    if(!match.Success || match.Groups[2].Success)
                    {
                        throw new Exception(match.Success ? match.Groups[2].Value : "Could not parse response to API manifest request");
                    }

                    var apiMap = new Dictionary<EApiResource, Uri>();

                    var count = match.Groups[4].Captures.Count;
                    for (var n = 0; n < count; ++n)
                    {
                        var api = (EApiResource)int.Parse(match.Groups[4].Captures[n].Value);
                        var uri = new Uri(match.Groups[5].Captures[n].Value);
                        apiMap.Add(api, uri);
                    }

                    return apiMap;
                }

                throw new Exception("API manifest request returned status code " + response.StatusCode);
            }
        }
    }
}
