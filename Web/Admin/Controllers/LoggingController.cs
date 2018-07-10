using System;
using System.Web.Mvc;
using Admin.Models;
using Eulg.Web.Service.Models;

namespace Eulg.Web.Service.Controllers
{
    //[Authorize]
    public class LoggingController : Controller
    {
        public ActionResult Index() => View();
        public ActionResult Show(Guid id) => View(ElmahLog.GetLogEntry(id));

        public JsonNetResult GetLogEntries(GridDataRequest request) => new JsonNetResult(ElmahLog.GetLogEntries(request));
        public JsonNetResult GetLogEntry(Guid id) => new JsonNetResult(ElmahLog.GetLogEntry(id));
        public JsonNetResult GetApps() => new JsonNetResult(ElmahLog.GetApps());
        public JsonNetResult ExportLogEntryToJira(Guid id) => new JsonNetResult(ElmahLog.ExportLogEntryToJira(id));
    }
}