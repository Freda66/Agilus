using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using NLX.Robot.Kuka.Controller;
using System.Threading;
using ConsoleApplication1;

//angles en degrès
//unité : milimètre

namespace Robot
{
    class RobotActions
    {
        

        #region Membres robot
        private TDx.TDxInput.Device device;
        private TDx.TDxInput.Vector3D vecteur;
        private RobotController robot;
        private CartesianPosition point_relatif;
        #endregion

        #region Structures
        private struct Pince
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
        private List<CartesianPosition> liste_aller_plateau;
        private List<CartesianPosition> liste_placer_piece;
        private List<CartesianPosition> liste_retour_magasin;

        
        private List<object> liste_commandes = new List<object>();
        private List<Emplacement> plateau;

        private CartesianPosition pts;
        #endregion

        #region threads
        private System.Threading.Tasks.Task mouseTask;
        #endregion



        public RobotActions()
        {
            liste_temp = new List<CartesianPosition>();

            /*init*/

            vecteur = new TDx.TDxInput.Vector3D();
            point_relatif = new CartesianPosition();
            liste_aller_magasin = new List<CartesianPosition>();
            liste_aller_plateau = new List<CartesianPosition>();
            liste_placer_piece = new List<CartesianPosition>();
            liste_retour_magasin = new List<CartesianPosition>();
            plateau = new Plateau().GetPlateau();

        }

        private void AllerAuPlateau()
        {

            NLX.Robot.Kuka.Controller.CartesianPosition pts4 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // lève la pièce
            pts4.X = 515.147888;
            pts4.Y = 237.083374;
            pts4.Z = 297.377289;
            pts4.A = 72.1285477;
            pts4.B = 85.9736862;
            pts4.C = -15.78833;

            liste_aller_plateau.Add(pts4);
            Console.WriteLine("pts 4");

            NLX.Robot.Kuka.Controller.CartesianPosition pts5 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // recul pour retrait piece
            pts5.X = 515.147888;
            pts5.Y = 75.3249283;
            pts5.Z = 297.377289;
            pts5.A = 72.1285477;
            pts5.B = 85.9736862;
            pts5.C = -15.78833;
            liste_aller_plateau.Add(pts5);
            Console.WriteLine("pts 5");

            NLX.Robot.Kuka.Controller.CartesianPosition pts6 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // Rotation vers plateau
            pts6.X = 660.406555;
            pts6.Y = -143.749481;
            pts6.Z = 431.472351;
            pts6.A = -107.49276;
            pts6.B = 2.0946629;
            pts6.C = -90.567405;
            liste_aller_plateau.Add(pts6);

            robot.PlayTrajectory(liste_aller_plateau);
        }

        private void AllerAuMagasin()
        {
            NLX.Robot.Kuka.Controller.CartesianPosition pts2 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // Saisie de la pièce
            pts2.X = 515.147888;
            pts2.Y = 184.597168;
            pts2.Z = 218.120453;
            pts2.A = 72.1285477;
            pts2.B = 85.9736862;
            pts2.C = -15.78833;

            liste_aller_magasin.Add(pts2);
            Console.WriteLine("pts 2");

            NLX.Robot.Kuka.Controller.CartesianPosition pts3 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // Pièce saisie 
            pts3.X = 515.147888;
            pts3.Y = 237.083374;
            pts3.Z = 219.083771;
            pts3.A = 72.1285477;
            pts3.B = 85.9736862;
            pts3.C = -15.78833;

            liste_aller_magasin.Add(pts3);

            robot.PlayTrajectory(liste_aller_magasin);
            
        }

        /// <summary>
        /// A SUPPRIMER
        /// </summary>
        public void TestPlateau()
        {
            // Création d'une liste temporaire pour tester les emplacements
            List<CartesianPosition> temp = new List<CartesianPosition>();

            
            foreach (Emplacement emplacement in plateau)
            {
                /*AllerAuMagasin();
                robot.CloseGripper();
                AllerAuPlateau();

                */
                temp.Clear();
                temp.Add(emplacement.point);

                robot.PlayTrajectory(temp);
                Thread.Sleep(5000);
                /*
                robot.StartRelativeMovement();
                Thread.Sleep(2000);

                CartesianPosition monPoint = new CartesianPosition();
                monPoint.X = 0.0;
                monPoint.Y = 0.0;
                monPoint.Z = -20.0;
                robot.SetRelativeMovement(monPoint);
                robot.StopRelativeMovement();
                Thread.Sleep(500);
                robot.OpenGripper();

                robot.StartRelativeMovement();
                Thread.Sleep(1000);
                monPoint.Z = 230.0;
                robot.SetRelativeMovement(monPoint);
                robot.StopRelativeMovement();

                AllerAuMagasin();*/
                
                
                
                
            }
        }


        // Fonction de connexion  au robot
        public void ConnectionAuRobot(String adresseIP)
        {
            robot = new RobotController();
            robot.Connect(adresseIP);

            // Création d'un thread pour éviter que le garbage collector tue le processus à n'importe quel moment
            mouseTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                device = new TDx.TDxInput.Device();
                device.Connect();
                try
                {
                    robot.GetCurrentPosition();
                }
                catch (Exception exc)
                {
                   //  //  Console.WriteLine("Connection failed ! => " + exc.Message);
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

        // Fonction de sauvegarde de la position du robot
        public void EnregistrerPositionRobot()
        {
            CartesianPosition point = new CartesianPosition();
            point = robot.GetCurrentPosition();

           
            liste_temp.Add(point);
            
        }

        public bool RecupererEtatCapteurProxomite()
        {
            return robot.ReadSensor();
        }

        public void OuvrirPince()
        {
            // On ajoute les valeurs déjà présentes dans la liste des positions
            liste_commandes.Add(liste_temp);
            liste_temp = new List<CartesianPosition>();

            // Ajout de l'objet pince à la liste des commandes
            Pince maPince = new Pince();
            maPince.isOpen = true;

            liste_commandes.Add(maPince);

           
            robot.OpenGripper();
        }

        public void FermerPince()
        {
            // On ajoute les valeurs déjà présentes dans la liste des positions
            liste_commandes.Add(liste_temp);
            liste_temp = new List<CartesianPosition>();

            Pince maPince = new Pince();
            maPince.isOpen = false;
            // Ajout de l'objet pince à la liste des commandes
            liste_commandes.Add(maPince);

           
            robot.CloseGripper();
        }

        //return true si l'action a bien ete effectuee
        private bool ExecuterTrajectoireEnregistree()
        {

            foreach (object action in liste_commandes)
            {
                // On vérifie le type de l'objet
                if (action.GetType().ToString().Contains("CartesianPosition"))
                {
                    // Création d'une liste pour effectuer une trajectoire
                    List<CartesianPosition> trajectory = new List<CartesianPosition>();
                    trajectory = (List<CartesianPosition>)action;

                    robot.PlayTrajectory(trajectory);
                }
                else if (action.GetType().ToString().Contains("Pince"))
                {
                    if (((Pince)action).isOpen)
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


        static void Main(string[] args)
        {
            RobotActions robot = new RobotActions();

           
            bool flag = false;
            while (!flag)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                if(key.Key == ConsoleKey.C)
                {
                    robot.ConnectionAuRobot("192.168.1.1");
                }

                //quitte le programme
                if(key.Key == ConsoleKey.Escape)
                {
                    flag = true;
                }

                //joue la trajectoire enregistree
                if(key.Key == ConsoleKey.Enter)
                {
                    //TODO : voir si necessite d'une valeur de retour pour attendre que le robot est fini son action avant de quitter. 
                    bool trajOk = robot.ExecuterTrajectoireEnregistree();
                }

                //enregistre la position
                if (key.Key == ConsoleKey.Spacebar)
                {
                    List<double> ld = robot.RecupererPositionRobot();
                    robot.EnregistrerPositionRobot();
                }

                if (key.Key == ConsoleKey.T)
                {
                    robot.TestPlateau();
                }
                //ouvre la pince
                if (key.Key == ConsoleKey.A)
                {
                    robot.OuvrirPince();
                }

                //ferme la pince
                if (key.Key == ConsoleKey.Z)
                {
                    robot.FermerPince();
                }
            }

            Console.ReadKey();
        }
    }
}
