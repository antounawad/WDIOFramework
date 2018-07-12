using System.IO.Compression;
using System.Web;
using System.Web.Mvc;

namespace Admin.Filters
{
    public class CompressFilter : ActionFilterAttribute
    {
        public CompressionLevel Level { get; set; }

        public CompressFilter()
        {
            Level = CompressionLevel.Fastest;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext) => ApplyCompression(filterContext.HttpContext, Level);

        public static void ApplyCompression(HttpContextBase httpContext, CompressionLevel compressionLevel)
        {
            var request = httpContext.Request;
            var acceptEncoding = request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(acceptEncoding))
            {
                return;
            }
            acceptEncoding = acceptEncoding.ToUpperInvariant();
            var response = httpContext.Response;
            if (acceptEncoding.Contains("GZIP"))
            {
                response.AppendHeader("Content-encoding", "gzip");
                response.Filter = new GZipStream(response.Filter, compressionLevel);
            }
            else if (acceptEncoding.Contains("DEFLATE"))
            {
                response.AppendHeader("Content-encoding", "deflate");
                response.Filter = new DeflateStream(response.Filter, compressionLevel);
            }
        }

    }
}
