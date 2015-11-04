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
            ((List<Models.Log>)Session["LOGS"]).Add(new Models.Log("infos", "Connecting to the device ..."));
            // ADD CONNECTION HERE

            return View("Index");
        }
    }
}