using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace KukaAgylus
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static List<Models.Log> Logs = new List<Models.Log>();
        public static Models.RobotInfos RobotInfos = new Models.RobotInfos();
        public static Models.Mouse6DInfos MouseInfos = new Models.Mouse6DInfos();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Logs.Add(new Models.Log("info", "New user connected"));
        }
    }
}
