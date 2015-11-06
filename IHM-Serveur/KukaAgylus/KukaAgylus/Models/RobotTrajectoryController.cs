using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLX.Robot.Kuka.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace KukaAgylus.Models
{
    public class RobotTrajectoryController
    {
        public LogManager Logs { get; set; }

        #region constantes calcul
        //CONSTANTES POUR POSITIONS PLATEAU
        private const int NBLIGNESPLATEAU = 4;
        private const int NBCOLONNESPLATEAU = 4;
        private const double XD = 935.37;  //Xb -> derniere position
        private const double YD = -267.96; //Yb
        private const double XB = 676.32;  //Xd -> origine
        private const double YB = -281.79; //Yd
        private const double XA = 777.05;  //Xc
        private const double YA = -396.82; //Yc
        private const double XC = 840.74;
        private const double YC = -148.34;
        private double THETA = Math.Acos((XC - XB) / Math.Sqrt(Math.Pow(XC - XB, 2) + Math.Pow(YC - YB, 2)));

        private double PASX = Math.Sqrt(Math.Pow(XC - XB, 2) + Math.Pow(YC - YB, 2)) / NBCOLONNESPLATEAU - 1;
        private double PASY = Math.Sqrt(Math.Pow(XA - XB, 2) + Math.Pow(YA - YB, 2)) / NBLIGNESPLATEAU - 1;
        #endregion

        #region membres - robot
        private TDx.TDxInput.Device device;
        private TDx.TDxInput.Vector3D vecteur;
        public RobotController robot;
        private CartesianPosition point_relatif;
        private int movementCount = 0;
        private int actionOpenCount = 0;
        private int actionCloseCount = 0;
        #endregion

        #region structs
        public struct Pince
        {
            public bool isOpen;
        }

        public struct Emplacement
        {
            public bool isBusy;
            public CartesianPosition point;
        }
        #endregion

        #region listes et points
        private List<CartesianPosition> liste_temp;
        private List<CartesianPosition> liste_aller_magasin;
        private List<CartesianPosition> liste_aller_magasin_to_plateau;
        private List<CartesianPosition> liste_placer_piece;
        private List<CartesianPosition> liste_retour_magasin;

        private CartesianPosition ptsPlateau = new CartesianPosition();
        public List<dynamic> liste_commandes = new List<dynamic>();
        private List<Emplacement> plateau;

        private CartesianPosition pts;
        #endregion

        #region threads
        private System.Threading.Tasks.Task mouseTask;
        #endregion

        public RobotTrajectoryController()
        {
            liste_temp = new List<CartesianPosition>();

            /*init*/

            vecteur = new TDx.TDxInput.Vector3D();
            point_relatif = new CartesianPosition();
            //liste_points = new List<CartesianPosition>();
            liste_aller_magasin = new List<CartesianPosition>();
            liste_aller_magasin_to_plateau = new List<CartesianPosition>();
            liste_placer_piece = new List<CartesianPosition>();
            liste_retour_magasin = new List<CartesianPosition>();
            plateau = new List<Emplacement>();
        }


        public void ConnectionAuRobot(String adresseIP)
        {
            robot = new RobotController();
            robot.Connect(adresseIP);
            mouseTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                //Console.WriteLine("start program");
                device = new TDx.TDxInput.Device();
                device.Connect();
                try
                {
                    robot.GetCurrentPosition();
                    //Console.WriteLine("Connection success ! ");
                }
                catch (Exception exc)
                {
                    //Console.WriteLine("Connection failed ! => " + exc.Message);
                }
            });
        }

        //retourne la postion robot : X, Y, Z, A, B, C
        public List<double> RecupererPositionRobot()
        {
            List<double> list = new List<double>();
            list.Add(robot.GetCurrentPosition().X);
            list.Add(robot.GetCurrentPosition().Y);
            list.Add(robot.GetCurrentPosition().Z);
            list.Add(robot.GetCurrentPosition().A);
            list.Add(robot.GetCurrentPosition().B);
            list.Add(robot.GetCurrentPosition().C);

            return list;
        }

        public void EnregistrerPositionRobot()
        {
            CartesianPosition point = new CartesianPosition();
            point = robot.GetCurrentPosition();

            Console.WriteLine("EnregistrerPositionRobot()  => "
                + " X: " + point.X
                + " Y: " + point.Y
                + " Z: " + point.Z
                + " A: " + point.A
                + " B: " + point.B
                + " C: " + point.C);
            liste_temp.Add(point);
            foreach (CartesianPosition pt in liste_temp)
            {
                Console.WriteLine("EnregistrerPositionRobot()  => liste_temp " + liste_temp.IndexOf(pt) + " : "
               + " X: " + pt.X
               + " Y: " + pt.Y
               + " Z: " + pt.Z
               + " A: " + pt.A
               + " B: " + pt.B
               + " C: " + pt.C);
            }
        }

        public bool RecupererEtatCapteurProximite()
        {
            return robot.ReadSensor();
        }

        public void OuvrirPince()
        {
            liste_commandes.Add(new { id = string.Format("Movement {0}", movementCount), list = liste_temp });
            movementCount++;
            liste_temp = new List<CartesianPosition>();

            Pince maPince = new Pince();
            maPince.isOpen = true;

            liste_commandes.Add(new { id = string.Format("Open Gripper {0}", actionOpenCount), list = maPince });
            actionOpenCount++;

            Console.WriteLine("OuvrirPince  =>  Open Gripper");
            robot.OpenGripper();
        }

        public void FermerPince()
        {
            liste_commandes.Add(new { id = string.Format("Movement {0}", movementCount), list = liste_temp });
            movementCount++;
            liste_temp = new List<CartesianPosition>();

            Pince maPince = new Pince();
            maPince.isOpen = false;

            liste_commandes.Add(new { id = string.Format("Close Gripper {0}", actionCloseCount), list = maPince });
            actionCloseCount++;

            Console.WriteLine("FermerPince()  =>  Close Gripper");
            robot.CloseGripper();
        }

        //return true si l'action a bien ete effectuee
        public bool ExecuterTrajectoireEnregistree()
        {
            //Console.WriteLine("Lancement trajectoire");

            foreach (var action in liste_commandes)
            {
                var actionList = action.list;

                Console.WriteLine("ExecuterTrajectoireEnregistree()  =>  type:" + actionList.GetType());

                //if (actionList.GetType().ToString().Contains("CartesianPosition"))
                if(actionList.Type == JTokenType.Array)
                {
                    List<CartesianPosition> trajectory = new List<CartesianPosition>();
                    foreach (var point in actionList)
                    {
                        trajectory.Add(new CartesianPosition()
                        {
                            X = point["X"],
                            Y = point["Y"],
                            Z = point["Z"],
                            A = point["A"],
                            B = point["B"],
                            C = point["C"]
                        });
                    }

                    Console.WriteLine("ExecuterTrajectoireEnregistree()  =>  nb pt trajectory : " + trajectory.Count);
                    robot.PlayTrajectory(trajectory);
                }
                else 
                {
                    if (actionList["isOpen"].Value)
                    {
                        robot.OpenGripper();
                    }
                    else
                    {
                        robot.CloseGripper();
                    }
                }
            }
            return true;
        }

        private bool LoadTrajectoryFromJsonFile(string filename)
        {
            if (Directory.Exists(Environment.CurrentDirectory + @"\trajectories") && File.Exists(Environment.CurrentDirectory + @"\trajectories\" + filename + ".json"))
            {
                string json = File.ReadAllText(Environment.CurrentDirectory + @"\trajectories\" + filename + ".json");
                liste_commandes = JsonConvert.DeserializeObject<List<dynamic>>(json);
                return true;
            }
            return false;
        }

        private string TrajectoryToJSON()
        {
            string json = "";
            json = JsonConvert.SerializeObject(liste_commandes);
            return json;
        }

        public void SaveTrajectory(string filename)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + @"\trajectories"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\trajectories");
            File.WriteAllText(Environment.CurrentDirectory + @"\trajectories\" + filename + ".json", this.TrajectoryToJSON(), Encoding.UTF8);
        }

        public List<string> GetProcessList()
        {
            string exportFolderAll = Environment.CurrentDirectory + @"\trajectories";
            var tempList = new List<string>();
            if(Directory.Exists(Environment.CurrentDirectory + @"\trajectories"))
                foreach (string path in Directory.GetFiles(exportFolderAll).ToList<string>())
                {
                    tempList.Add(path.Split('\\').Last().Replace(".json", ""));
                }
            return tempList;
        }

        public List<dynamic> GetProcess(string processName)
        {
            LoadTrajectoryFromJsonFile(processName);
            return liste_commandes;
        }
    }

}