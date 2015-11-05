using Mouse6d;
using NLX.Robot.Kuka.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using KukaAgylus.Models;

namespace KukaAgylus
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static LogManager Logs = new LogManager();
        
        public static MouseInfos MouseInfos = new MouseInfos();
        public static Mouse MyMouse = new Mouse() { Logs = Logs, MouseInfos = MouseInfos };

        public static RobotController MyRobot = new RobotController();
        public static RobotInfos RobotInfos = new RobotInfos();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Logs.AddLog("info", "New user connected");
        }

    }
}
