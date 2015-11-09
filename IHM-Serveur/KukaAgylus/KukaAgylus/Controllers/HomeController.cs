using KukaAgylus.Models;
using Mouse6d;
using NLX.Robot.Kuka.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        #region Rooted views
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
        #endregion

        #region Request for Logs & Infos
        [HttpGet]
        public ActionResult GetLogs()
        {
            return Json(Logs.GetDisplayableLogs(), JsonRequestBehavior.AllowGet);
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
        #endregion

        #region Settings Robot & Mouse
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
                    MyRobot.GetCurrentPosition();
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
        public ActionResult SetRobotMode(string modeName)
        {
            bool success = (modeName == "Learning" || modeName == "Processing") && MvcApplication.RobotInfos.IsConnected;
            if (success)
            {
                MvcApplication.RobotInfos.Mode = modeName;
                Logs.AddLog("info", string.Format("Change robot mode to \"{0}\" mode", modeName));

                if (modeName == "Learning")
                {
                    StopLearningLoop();
                    StartLearningLoop();
                }
                else StopLearningLoop();
            }
            else
            {
                Logs.AddLog("error", "In change robot mode: Invalid operation");
            }
            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SetMouseTreshold(double treshold)
        {
            Logs.AddLog("info", string.Format("Settings changed: Treshold = {0}", treshold));
            MouseInfos.Treshold = treshold;
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SetTranslationVelocity(double velocity)
        {
            Logs.AddLog("info", string.Format("Settings changed: Translation velocity = {0}", velocity));
            MouseInfos.TranslationVelocity = velocity;
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SetRotationVelocity(double velocity)
        {
            Logs.AddLog("info", string.Format("Settings changed: Rotation velocity = {0}", velocity));
            MouseInfos.RotationVelocity = velocity;
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
        /*
        [HttpGet]
        public ActionResult ApplyRobotSettings(string mode, double? velocity)
        {
            bool success = (mode == "Learning" || mode == "Processing") && MvcApplication.RobotInfos.IsConnected;
            if (success && velocity != null)
            {
                MvcApplication.RobotInfos.Velocity = velocity.Value;
                MouseInfos.TranslationVelocity = velocity.Value;
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
                MyRobot.StopRelativeMovement();
            }

            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }
        */
        #endregion

        #region Process Management
        [HttpGet]
        public ActionResult GetProcessNameList()
        {
            return Json(RobotProcessController.GetProcessNameList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetHtmlProcess(string processName)
        {
            if (!string.IsNullOrEmpty(processName))
            {
                var process = new RobotProcess(processName);

                var htmlFormat = "<li class='list-group-item drag-item'  draggable='true' id='{0}'>{1}<span class='badge'><a class='glyphicon glyphicon-remove' href='javascript:DeleteProcessElement(\"{0}\")'></a></span>{2}</li>";
                var processBuilder = new StringBuilder();

                foreach (var cmd in process.Commands)
                {
                    if (cmd is Movement)
                    {
                        var mvt = cmd as Movement;
                        var htmlInsideFormat = "<li class='list-group-item inside-list-group-item' id='{0}'>{1}<span class='badge'><a class='glyphicon glyphicon-remove' href='javascript:DeleteProcessElement(\"{0}\")'></a></span></li>";
                        var movementBuilder = new StringBuilder();
                        movementBuilder.AppendFormat("<span class='badge'><a class='glyphicon glyphicon-arrow-down' href='javascript:DisplayMovementElement(\"{0}\")'></a></span><ul class='list-group inside-list-group' id='group-{0}'>", cmd.Id);
                        foreach (var pos in mvt.Positions)
                        {
                            movementBuilder.AppendFormat(htmlInsideFormat, pos.Id, string.Concat("Point #", mvt.Positions.IndexOf(pos) + 1));
                        }
                        movementBuilder.Append("</ul>");
                        processBuilder.AppendFormat(htmlFormat, cmd.Id, cmd.Name, movementBuilder.ToString());
                    }
                    else if (cmd is GripperAction)
                    {
                        processBuilder.AppendFormat(htmlFormat, cmd.Id, cmd.Name, string.Empty);
                    }
                }

                return Json(processBuilder.ToString(), JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SwitchCommand(string processName, Guid guidCmd1, Guid guidCmd2)
        {
            
            var success = RobotProcessController.SwitchCommand(processName, guidCmd1, guidCmd2);
            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddCurrentPosition(string processName, Guid guidMovement)
        {
            var success = RobotProcessController.AddCurrentPosition(processName, guidMovement, MyRobot);
            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddGripperAction(string processName, bool open)
        {
            var success = RobotProcessController.AddGripperAction(processName, open ? GripperAction.Action.Open : GripperAction.Action.Close, MyRobot);
            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddMovement(string processName, string movementName)
        {
            var success = RobotProcessController.AddMovement(processName, movementName);
            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult StartProcess(string processName)
        {
            var success = RobotProcessController.ExecuteProcess(MyRobot, processName, MvcApplication.LoadedTray ,Logs);
            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult IsProcessExist(string processName)
        {
            return Json(new { Exist = RobotProcessController.IsExistingProcess(processName) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteCommand(string processName, Guid guidCmd)
        {
            var success = RobotProcessController.DeleteCommand(processName, guidCmd);
            return Json(new { Success = success }, JsonRequestBehavior.AllowGet);
        }
        #endregion



        #region Functions
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
        #endregion

    }
}