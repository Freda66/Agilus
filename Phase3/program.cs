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
        private List<CartesianPosition> liste_aller_magasin_to_plateau;
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
            liste_aller_magasin_to_plateau = new List<CartesianPosition>();
            liste_placer_piece = new List<CartesianPosition>();
            liste_retour_magasin = new List<CartesianPosition>();
            plateau = new Plateau().GetPlateau();
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
                temp.Clear();
                temp.Add(emplacement.point);

                robot.PlayTrajectory(temp);
                Thread.Sleep(5000);
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
