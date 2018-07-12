using System;
using System.IO.Compression;
using System.Text;
using System.Web.Mvc;
using Admin.Filters;
using Admin.Models;
using Newtonsoft.Json;

namespace Eulg.Web.Service.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class MyControllerBase : Controller
    {
        private const CompressionLevel DEFAULT_COMPRESSION_LEVEL = CompressionLevel.Fastest;

        #region JsonNet
        protected internal JsonNetResult JsonNet(object content)
        {
            return JsonNet(content, null, null);
        }
        protected internal JsonNetResult JsonNet(object content, JsonSerializerSettings serializerSettings)
        {
            return JsonNet(content, serializerSettings, null);
        }
        protected internal JsonNetResult JsonNet(object content, JsonSerializerSettings serializerSettings, Encoding encoding)
        {
            CompressFilter.ApplyCompression(HttpContext, DEFAULT_COMPRESSION_LEVEL);
            return new JsonNetResult { Data = content, ContentEncoding = encoding }; ;
        }
        #endregion

        #region Json

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            //Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //Response.Cache.SetNoStore();
            CompressFilter.ApplyCompression(HttpContext, DEFAULT_COMPRESSION_LEVEL);
            return new JsonResult { Data = data, JsonRequestBehavior = behavior, MaxJsonLength = Int32.MaxValue };
        }
        #endregion

    }
}