using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


//angles en degrès
//unité : milimètre

namespace Mouse
{
    public partial class Form1 : Form
    {
        static TDx.TDxInput.Device device;
        static TDx.TDxInput.Vector3D vecteur;
        public NLX.Robot.Kuka.Controller.RobotController robot;
        public NLX.Robot.Kuka.Controller.CartesianPosition point_relatif;

        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_points;
        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_aller_magasin;
        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_aller_magasin_to_plateau;
        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_placer_piece;

        struct Emplacement
        {
            public bool isBusy;
            public NLX.Robot.Kuka.Controller.CartesianPosition point;

        }
        public List<Emplacement> plateau;


        public NLX.Robot.Kuka.Controller.CartesianPosition pts;
        public Form1()
        {
            InitializeComponent();
            vecteur = new TDx.TDxInput.Vector3D();
            point_relatif = new NLX.Robot.Kuka.Controller.CartesianPosition();
            liste_points = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            liste_aller_magasin = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            liste_aller_magasin_to_plateau = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            liste_placer_piece = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            plateau = new List<Emplacement>();

            pts = new NLX.Robot.Kuka.Controller.CartesianPosition();
            // Devant magasin
            pts.X = 504.34;
            pts.Y = 188.64;
            pts.Z = 221.57;
            pts.A = 46.43;
            pts.B = 89.34;
            pts.C = -49.89;

            liste_aller_magasin.Add(pts);

            // Saisie de la pièce
            pts.X = 504.34;
            pts.Y = 231.91;
            pts.Z = 221.63;
            pts.A = 46.43;
            pts.B = 89.34;
            pts.C = -49.89;

            liste_aller_magasin.Add(pts);

            // Fermeture pince

            // Pièce saisie 
            pts.X = 504.34;
            pts.Y = 231.50;
            pts.Z = 279.35;
            pts.A = 46.18;
            pts.B = 89.34;
            pts.C = -50.14;
            
            liste_aller_magasin_to_plateau.Add(pts);

            // Retrait
            pts.X = 504.34;
            pts.Y = 144.68;
            pts.Z = 279.35;
            pts.A = 46.18;
            pts.B = 89.34;
            pts.C = -50.14;

            liste_aller_magasin_to_plateau.Add(pts);

            // Au dessus origine plateau
            pts.X = 932.88;
            pts.Y = -267.05;
            pts.Z = 347.88;
            pts.A = 31.97;
            pts.B = 1.26;
            pts.C = 92.69;
            liste_aller_magasin_to_plateau.Add(pts);


            

        }

        System.Threading.Tasks.Task mouseTask;

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }


        private void buttonConnect_Click(object sender, EventArgs e)
        {
            robot = new NLX.Robot.Kuka.Controller.RobotController();
            robot.Connect("192.168.1.1");
            mouseTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                Console.WriteLine("start program");
                device = new TDx.TDxInput.Device();
                device.Connect();

                Console.WriteLine("Mouse connected");
               /* while (true)
                {
                    var translation = device.Sensor.Translation;
                    var rotation = device.Sensor.Rotation;


                    //Console.WriteLine("Translation: X:" + (translation.X / norme).ToString() + "  Y:" + (translation.Y / norme).ToString());
                    //Console.WriteLine("Rotation: X:" + (rotation.X / norme).ToString() + "  Y:" + (rotation.Y / norme).ToString());

                    // si besoin mettre timer
                    System.Threading.Thread.Sleep(50);
                }*/
            });

            Console.WriteLine("press key to quit");
           // Console.ReadKey();
        }

        private void buttonGetPosition_Click(object sender, EventArgs e)
        {
            labelX.Text = robot.GetCurrentPosition().X.ToString();
            labelY.Text = robot.GetCurrentPosition().Y.ToString();
            labelZ.Text = robot.GetCurrentPosition().Z.ToString();
        }

       

        private void buttonReadSensor_Click(object sender, EventArgs e)
        {
            labelReadSensor.Text = robot.ReadSensor().ToString();
        }

        private void buttonSavePosition_Click(object sender, EventArgs e)
        {
            NLX.Robot.Kuka.Controller.CartesianPosition point= new NLX.Robot.Kuka.Controller.CartesianPosition();

            point.X = robot.GetCurrentPosition().X;
            point.Y = robot.GetCurrentPosition().Y;
            point.Z = robot.GetCurrentPosition().Z;
            point.A = robot.GetCurrentPosition().A;
            point.B = robot.GetCurrentPosition().B;
            point.C = robot.GetCurrentPosition().C;

            liste_points.Add(point);
            listBoxSavePosition.Items.Add("X"+robot.GetCurrentPosition().X.ToString() + " Y:" + robot.GetCurrentPosition().Y.ToString() + " Z:" + robot.GetCurrentPosition().Z.ToString());
        }

        private void buttonOpenGripper_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Open Gripper");
            robot.OpenGripper();
        }

        private void buttonCloseGripper_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Close Gripper");
            robot.CloseGripper();

           
        }

        private void buttonHaut_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Haut");
            point_relatif.Z = 1;
            robot.StartRelativeMovement();
            robot.SetRelativeMovement(point_relatif);
            robot.StopRelativeMovement();
        }

        private void buttonBas_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Bas");
            point_relatif.Z = -1;
            robot.StartRelativeMovement();
            robot.SetRelativeMovement(point_relatif);
            robot.StopRelativeMovement();
        }

        private void buttonDroite_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Droite");
            point_relatif.Y = -1;
            robot.StartRelativeMovement();
            robot.SetRelativeMovement(point_relatif);
            robot.StopRelativeMovement();
        }

        private void buttonGauche_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Gauche");
            point_relatif.Y = 1;
            robot.StartRelativeMovement();
            robot.SetRelativeMovement(point_relatif);
            robot.StopRelativeMovement();
        }

        private void buttonTrajectory_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Lancement trajectoire");
            robot.PlayTrajectory(liste_points);
        }

        private void buttonAllerAuMagasin_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Aller au magasin");

            robot.PlayTrajectory(liste_aller_magasin);
            robot.CloseGripper();
            
        }

        private void buttonAllerPlateau_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Aller origine plateau");
            robot.PlayTrajectory(liste_aller_magasin_to_plateau);
        }

        private void buttonPlacerPiece_Click(object sender, EventArgs e)
        {
            var position_libre = -1;
            for (int i=0; i<plateau.Count;i++)
            {
                if (!plateau.ElementAt(i).isBusy)
                {
                    position_libre=i;
                    break;
                }
            }


            if (position_libre >= 0)
            {
                pts.X = plateau.ElementAt(position_libre).point.X;
                pts.Y = plateau.ElementAt(position_libre).point.Y;
                pts.Z = plateau.ElementAt(position_libre).point.Z;
                pts.A = plateau.ElementAt(position_libre).point.A;
                pts.B = plateau.ElementAt(position_libre).point.B;
                pts.C = plateau.ElementAt(position_libre).point.C;

                liste_placer_piece.Add(pts);
            }
        }
    }
}
