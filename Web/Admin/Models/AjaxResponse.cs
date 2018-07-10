using System;
using System.Linq;
using Newtonsoft.Json;

namespace Admin.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AjaxResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RedirectUrl { get; set; }
    }
}