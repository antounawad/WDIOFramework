using System;
using System.Net;
using System.Xml.Serialization;
using Eulg.Shared;

namespace Eulg.Setup
{
    public class BrandingApi
    {
        [XmlRoot]
        private class Parameters
        {
            [XmlAttribute]
            public string Channel { get; set; }

            [XmlAttribute]
            public string Username { get; set; }

            [XmlAttribute]
            public string Password { get; set; }
        }

        private readonly string _serviceUrl;
        private readonly Branding.EUpdateChannel _channel;

        public BrandingApi(string serviceUrl, Branding.EUpdateChannel channel)
        {
            _serviceUrl = serviceUrl;
            _channel = channel;
        }

        public BrandingInfo GetBranding()
        {
            var parameters = new Parameters
            {
                Channel = _channel.ToString(),
                Username = null,
                Password = null
            };

            return Invoke(parameters);
        }

        public BrandingInfo GetProfile(string username, string password)
        {
            var parameters = new Parameters
            {
                Channel = _channel.ToString(),
                Username = username,
                Password = password
            };

            return Invoke(parameters);
        }

        private BrandingInfo Invoke(Parameters parameters)
        {
            var apiUri = SetupHelper.GetUpdateApi(_serviceUrl, "GetBrandingProfileInfo");

            var request = (HttpWebRequest)WebRequest.Create(apiUri);
            request.Method = WebRequestMethods.Http.Post;

            using (var stream = request.GetRequestStream())
            {
                new XmlSerializer(typeof(Parameters)).Serialize(stream, parameters);
            }

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    return (BrandingInfo)new XmlSerializer(typeof(BrandingInfo)).Deserialize(stream);
                }
            }
        }
    }
}
