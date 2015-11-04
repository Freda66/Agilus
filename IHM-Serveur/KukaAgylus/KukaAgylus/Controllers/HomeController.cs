using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KukaAgylus.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult BtnConnect_Click(object sender, EventArgs e)
        {
            MvcApplication.Logs.Add(new Models.Log("info", "Connecting to the device ..."));
            // ADD CONNECTION HERE

            return View("Index");
        }
        
        [HttpGet]
        public ActionResult GetLogs()
        {
            var logsToString = new List<string>();
            foreach (var log in MvcApplication.Logs.OrderByDescending(m => m.Time))
                logsToString.Add(log.ToString());
            return Json(logsToString, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMouseInfos()
        {
            return Json(MvcApplication.MouseInfos.GetHtmlString(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetRobotInfos()
        {
            return Json(MvcApplication.RobotInfos.GetHtmlString(), JsonRequestBehavior.AllowGet);
        }

    }
}