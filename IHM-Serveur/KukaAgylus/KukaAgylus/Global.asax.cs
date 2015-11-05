using Mouse6d;
using NLX.Robot.Kuka.Controller;
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

        public static Mouse MyMouse = new Mouse() { LogsList = Logs };
        public static RobotController MyRobot = new RobotController();

        public static Models.RobotInfos RobotInfos = new Models.RobotInfos();
        public static Models.MouseInfos MouseInfos = new Models.MouseInfos();

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
