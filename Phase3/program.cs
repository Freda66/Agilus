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

        private void AllerAuMagasin()
        {
            NLX.Robot.Kuka.Controller.CartesianPosition pts2 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // Saisie de la pièce
            pts2.X = 503.16;
            pts2.Y = 174.39;
            pts2.Z = 219.45;
            pts2.A = 80.60;
            pts2.B = -83.91;
            pts2.C = -168.19;

            liste_aller_magasin.Add(pts2);
            Console.WriteLine("pts 2");

            robot.PlayTrajectory(liste_aller_magasin);

            
            
        }

        private void PrendrePiece()
        {
            NLX.Robot.Kuka.Controller.CartesianPosition pts3 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // Pièce saisie
            // Coordonnées à affiner surtout sur A, B et C
            pts3.X = 503.16;
            pts3.Y = 232.13;
            pts3.Z = 216.00;
            pts3.A = 73.61;
            pts3.B = -86.54;
            pts3.C = -158.52;

            //A rotation autour de Z
            //B rotation autour de Y
            //C rotation autour de X
            liste_aller_plateau.Add(pts3);

            robot.PlayTrajectory(liste_aller_plateau);// On saisie et on prend la pièce

            liste_aller_plateau.Clear();// On a fais le mouvement on peut vider la liste

            robot.CloseGripper();
            NLX.Robot.Kuka.Controller.CartesianPosition pts4 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // lève la pièce
            // Coordonnées à revoir surtout pour les A, B et C
            pts4.X = 503.16;
            pts4.Y = 232.13;
            pts4.Z = 316.00;
            pts4.A = 73.61;
            pts4.B = -86.54;
            pts4.C = -158.52;

            liste_aller_plateau.Add(pts4);
            Console.WriteLine("pts 4");

            NLX.Robot.Kuka.Controller.CartesianPosition pts5 = new NLX.Robot.Kuka.Controller.CartesianPosition();

            // recul pour retrait piece
            // Coordonnées à revoir surtout pour les A, B et C
            pts5.X = 503.16;
            pts5.Y = 150.46;
            pts5.Z = 316.00;
            pts5.A = 73.61;
            pts5.B = -86.54;
            pts5.C = -158.52;
            liste_aller_plateau.Add(pts5);
            Console.WriteLine("pts 5");

            robot.PlayTrajectory(liste_aller_plateau);
        }

       
        private void goToIJPosition(int i, int j)
        {
            // Création d'une liste temporaire pour tester les emplacements
            List<CartesianPosition> temp = new List<CartesianPosition>();


            Emplacement emplacement = new Emplacement();

            emplacement = plateau[i * 4 + j];

            if (!emplacement.isBusy)
            {
                AllerAuMagasin();
                PrendrePiece();


                temp.Clear();
                // Permet de jouer point par point sur le plateau
                temp.Add(emplacement.point);

                robot.PlayTrajectory(temp);
                Thread.Sleep(5000);

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

                // Relève la pince. Modification
                robot.StartRelativeMovement();
                Thread.Sleep(1000);
                monPoint.Z = 530.0;
                robot.SetRelativeMovement(monPoint);
                robot.StopRelativeMovement();

            }
        }


        private void TestPlateau()
        {
            

            for (int i = 0; i < plateau.Count;i++)
            {
                for (int j = 0; j < plateau.Count; j++)
                {
                    AllerAuMagasin();
                    PrendrePiece();
                    goToIJPosition(i, j);
                }

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
