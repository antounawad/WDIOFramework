using System;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Admin.Models
{
    public class JsonNetResult : JsonResult
    {
        public JsonSerializerSettings SerializerSettings;

        public JsonNetResult() { }

        public JsonNetResult(object data) : this()
        {
            Data = data;
        }

        public JsonNetResult(object data, JsonSerializerSettings serializerSettings) : this(data)
        {
            Data = data;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if(context == null) throw new ArgumentNullException(nameof(context));
            //if (JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("JSON GET nicht erlaubt!");
            if(ContentEncoding != null) context.HttpContext.Response.ContentEncoding = ContentEncoding;
            var response = context.HttpContext.Response;
            response.ContentType = "application/json";
            var serializer = JsonSerializer.Create(SerializerSettings);
#if DEBUG
            serializer.Formatting = Formatting.Indented;
#else
            serializer.Formatting = Formatting.None;
#endif
            serializer.Serialize(response.Output, Data);
        }
    }

    public class JsonNetResult<T> : JsonNetResult
    {
        //public new T Data { get; set; }
        public JsonNetResult() { }
        public JsonNetResult(T data) : this()
        {
            Data = data;
        }
    }
}