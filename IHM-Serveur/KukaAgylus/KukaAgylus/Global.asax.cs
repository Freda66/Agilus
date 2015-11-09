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
    public class MvcApplication : HttpApplication
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

        private void TestLoadProcess()
        {
            var proc = new RobotProcess("testProcess");
        }

        private void TestCreationProcess()
        {
            var pos1 = new RobotPosition()
            {
                Position = new CartesianPosition()
                {
                    X = 0.1,
                    Y = 0.2,
                    Z = 0.3,
                    A = 0.4,
                    B = 0.5,
                    C = 0.6

                }
            };
            var pos2 = new RobotPosition()
            {
                Position = new CartesianPosition()
                {
                    X = 1.1,
                    Y = 1.2,
                    Z = 1.3,
                    A = 1.4,
                    B = 1.5,
                    C = 1.6

                }
            };
            var pos3 = new RobotPosition()
            {
                Position = new CartesianPosition()
                {
                    X = 0.1,
                    Y = 0.2,
                    Z = 0.3,
                    A = 0.4,
                    B = 0.5,
                    C = 0.6

                }
            };
            var pos4 = new RobotPosition()
            {
                Position = new CartesianPosition()
                {
                    X = 1.1,
                    Y = 1.2,
                    Z = 1.3,
                    A = 1.4,
                    B = 1.5,
                    C = 1.6

                }
            };
            var listPos1 = new List<RobotPosition>();
            listPos1.Add(pos1);
            listPos1.Add(pos2);

            var listPos2 = new List<RobotPosition>();
            listPos2.Add(pos3);
            listPos2.Add(pos4);

            var gripperAction1 = new GripperAction()
            {
                Name = "Close gripper",
                Command = GripperAction.Action.Close
            };
            var movement1 = new Movement()
            {
                Name = "Mouvement 1",
                Positions = listPos1
            };
            var gripperAction2 = new GripperAction()
            {
                Name = "Open gripper",
                Command = GripperAction.Action.Open
            };
            var movement2 = new Movement()
            {
                Name = "Mouvement 2",
                Positions = listPos2
            };

            RobotProcess proc = new RobotProcess("testProcess2");
            proc.Commands.Add(movement1);
            proc.Commands.Add(gripperAction1);
            proc.Commands.Add(movement2);
            proc.Commands.Add(gripperAction2);

            proc.SaveProcess();
        }
    }
}
