using System;
using System.Web.Mvc;
using Admin.Models;
using Eulg.Web.Service.Models;

namespace Eulg.Web.Service.Controllers
{
    //[Authorize]
    public class LoggingController : MyControllerBase
    {
        public ActionResult Index() => View();
        public ActionResult Show(Guid id) => View(ElmahLog.GetLogEntry(id));

        public ActionResult GetLogEntries(GridDataRequest request)
        {
            var x = ElmahLog.GetLogEntries(request);
            //return Json(x);
            return new JsonNetResult(x);
        }

        public JsonNetResult GetLogEntry(Guid id) => new JsonNetResult(ElmahLog.GetLogEntry(id));
        public JsonNetResult GetApps() => new JsonNetResult(ElmahLog.GetApps());
        public JsonNetResult ExportLogEntryToJira(Guid id) => new JsonNetResult(ElmahLog.ExportLogEntryToJira(id));
    }
}