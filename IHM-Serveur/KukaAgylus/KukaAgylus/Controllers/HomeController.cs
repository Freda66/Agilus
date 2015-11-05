using KukaAgylus.Models;
using Mouse6d;
using NLX.Robot.Kuka.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace KukaAgylus.Controllers
{
    public class HomeController : Controller
    {
        private LogManager Logs = MvcApplication.Logs;

        private MouseInfos MouseInfos = MvcApplication.MouseInfos;
        private Mouse MyMouse = MvcApplication.MyMouse;

        private RobotController MyRobot = MvcApplication.MyRobot;

        private bool _learningLoopRunning = false;

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

        public ActionResult Console()
        {
            return View();
        }

        public ActionResult Infos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetLogs()
        {
            return Json(Logs.GetDisplayableLogs() , JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMouseInfos()
        {
            return Json(MouseInfos.GetHtmlString(), JsonRequestBehavior.AllowGet);
            //return Json(MyMouse.GetMouseInfos().GetHtmlString(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetRobotInfos()
        {
            return Json(MvcApplication.RobotInfos.GetHtmlString(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SwitchMouseCalibration(bool start)
        {
            if (start)
            {
                //Démarrage de la calibration
                Logs.AddLog("info", "Starting mouse calibration ...");
                Thread calibThread = new Thread(MyMouse.Calibrate);
                calibThread.Start();
            }
            else
            {
                //Arrêt de la calibration
                Logs.AddLog("info", "Stop mouse calibration");
                MyMouse.CalibrationStop();
            }

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SwitchRobotConnection(bool connect, string ip)
        {
            bool success = false;
            if (connect)
            {
                //Connexion du robot
                Logs.AddLog("info", string.Format("Starting robot connection on {0} ...", ip));
                MvcApplication.RobotInfos.IsConnected = true;
                try
                {
                    MyRobot.Connect(ip);
                    MvcApplication.RobotInfos.IsConnected = true;
                    success = true;
                    Logs.AddLog("info", "Robot connected");
                }
                catch (Exception e)
                {
                    Logs.AddLog("Error", string.Format("Error in robot connection: {0} ...", e.Data));
                    MvcApplication.RobotInfos.IsConnected = false;
                }
            }
            else
            {
                //Deconnexion du robot
                Logs.AddLog("info", "Robot disconnection not implemented");
                //MvcApplication.RobotInfos.IsConnected = false;
                success = true;
            }

            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult ApplyRobotSettings(string mode, double? velocity)
        {
            bool success = (mode == "Learning" || mode == "Processing") && MvcApplication.RobotInfos.IsConnected;
            if (success && velocity != null)
            {
                MvcApplication.RobotInfos.Velocity = velocity.Value;
                MyMouse.Vitesse = velocity.Value;
            }
            if (success)
            {
                MvcApplication.RobotInfos.Mode = mode;
                Logs.AddLog("info", string.Format("Change robot settings: Mode \"{0}\", Velocity \"{1}\" ...", mode, MvcApplication.RobotInfos.Velocity));

                if (mode == "Learning")
                {
                    StopLearningLoop();
                    StartLearningLoop();
                }
                else StopLearningLoop();
            }
            else
            {
                Logs.AddLog("error", "Invalid operation while settings change");
            }

            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetProcessList()
        {
            var fakeList = new List<string>() { "toto", "emile" };

            return Json(fakeList, JsonRequestBehavior.AllowGet);
        }

        private void StartLearningLoop()
        {
            Thread learningLoop = new Thread(LearningLoop);
            learningLoop.Start();
        }

        private void LearningLoop()
        {
            // Envoi la commande au robot
            MyRobot.StartRelativeMovement();

            // Créer un objet thread de l'objet Mouse, fonction Loop
            Thread MouseThread = new Thread(MyMouse.Loop);

            // Demarre le thread.
            MouseThread.Start();
            Logs.AddLog("INFO", "Starting mouse thread...");
            //Console.WriteLine("main thread: Starting mouse thread...");

            // Attend que le thread soit lancé et activé
            while (!MouseThread.IsAlive) ;
            //Console.WriteLine("main thread: Mouse alive");
            Logs.AddLog("INFO", "Thread mouse alive");

            // Boucle tant qu'on utilise la souris 
            _learningLoopRunning = true;
            while (_learningLoopRunning)
            {
                // Met le thread principale (ici) en attente d'une millisecond pour autoriser le thread secondaire à faire quelque chose
                Thread.Sleep(1);

                // Envoi les commandes de deplacement au robot
                MyRobot.SetRelativeMovement(MyMouse.CartPosition);
            }

            // Demande l'arret du thread de la souris
            MyMouse.RequestStop();

            // Bloque le thread principale tant que le thread Mouse n'est pas terminé
            MouseThread.Join();

            // Arret le mouvement
            MyRobot.StopRelativeMovement();
            Logs.AddLog("INFO", "End learning loop");
        }

        private void StopLearningLoop()
        {
            _learningLoopRunning = false;
        }

        [HttpGet]
        public void SendMousePosition(double tx, double ty, double tz, double rx, double ry, double rz)
        {
            MouseInfos.TranslationX = tx;
            MouseInfos.TranslationY = ty;
            MouseInfos.TranslationZ = tz;

            MouseInfos.RotationX = rx;
            MouseInfos.RotationY = ry;
            MouseInfos.RotationZ = rz;
        }

    }
}