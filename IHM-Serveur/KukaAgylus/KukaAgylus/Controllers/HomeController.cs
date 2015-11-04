﻿using System;
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

        [HttpGet]
        public ActionResult SwitchMouseCalibration(bool start)
        {
            if(start)
            {
                //Démarrage de la calibration
                MvcApplication.Logs.Add(new Models.Log("info", "Starting mouse calibration ..."));
                MvcApplication.MouseInfos.IsCalibrated = false;
            }
            else
            {
                //Arrêt de la calibration
                MvcApplication.Logs.Add(new Models.Log("info", "Stop mouse calibration"));
                MvcApplication.MouseInfos.IsCalibrated = true;
            }
            
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SwitchRobotConnection(bool connect, string ip)
        {
            if (connect)
            {
                //Connexion du robot
                MvcApplication.Logs.Add(new Models.Log("info", string.Format("Starting robot connection on {0} ...", ip)));
                MvcApplication.RobotInfos.IsConnected = true;
            }
            else
            {
                //Deconnexion du robot
                MvcApplication.Logs.Add(new Models.Log("info", "Robot disconnected"));
                MvcApplication.RobotInfos.IsConnected = false;
            }

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}