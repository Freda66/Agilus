using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Timers;

//TODO :  - ROBOT NE VAS PAS A LA BONNE POSITION QUAND ON LANCE LE PLAYTRAJECTORY POUR ALLER DEVANT LE MAGASIN
//        - FORMULES DE CALCUL NE PRENNENT PAS EN COMPTE LA TRANSFORMATION DE REPERE.
//        - RELATIVEMOVEMENT NE FONCTIONNE PAS

//angles en degrès
//unité : milimètre

namespace Mouse
{
    public partial class Form1 : Form
    {
        //CONSTANTES POUR POSITIONS PLATEAU
        public const int NBLIGNESPLATEAU = 4;
        public const int NBCOLONNESPLATEAU = 4;
        public const double XD = 935.37;  //Xb -> derniere position
        public const double YD = -267.96; //Yb
        public const double XB = 677.17;  //Xd -> origine
        public const double YB = -283.23; //Yd
        public const double XA = 771.61;  //Xc
        public const double YA = -394.41; //Yc
        public const double XC = 840.33;
        public const double YC = -150.12;
        public double THETA = Math.Acos((XC - XB) / Math.Sqrt(Math.Pow(XC - XB, 2) + Math.Pow(YC - YB, 2)));
        //public double THETARAD = Math.PI * THETA / 180.0; 
        public  double PASX = Math.Sqrt(Math.Pow(XC - XB, 2) + Math.Pow(YC - YB, 2))/NBCOLONNESPLATEAU-1;
        public double PASY = Math.Sqrt(Math.Pow(XA - XB, 2) + Math.Pow(YA - YB, 2)) / NBLIGNESPLATEAU - 1;


        static TDx.TDxInput.Device device;
        static TDx.TDxInput.Vector3D vecteur;
        public NLX.Robot.Kuka.Controller.RobotController robot;
        public NLX.Robot.Kuka.Controller.CartesianPosition point_relatif;

        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_points;
        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_aller_magasin;
        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_aller_magasin_to_plateau;
        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_placer_piece;
        public List<NLX.Robot.Kuka.Controller.CartesianPosition> liste_retour_magasin;
       
        public NLX.Robot.Kuka.Controller.CartesianPosition ptsPlateau = new NLX.Robot.Kuka.Controller.CartesianPosition();


        public struct Emplacement
        {
            public bool isBusy;
            public NLX.Robot.Kuka.Controller.CartesianPosition point;
        }
        public List<Emplacement> plateau;


        public NLX.Robot.Kuka.Controller.CartesianPosition pts;

         /*static void ShowConfig()
         {

            // For read access you do not need to call OpenExeConfiguraton
            foreach(string key in ConfigurationManager.AppSettings)
            {
               string value = ConfigurationManager.AppSettings[key];
               Console.WriteLine("Key: {0}, Value: {1}", key, value);
            }
         }*/
        public Form1()
        {
            InitializeComponent();

            /*ShowConfig();

            // Open App.Config of executable
            System.Configuration.Configuration config =
             ConfigurationManager.OpenExeConfiguration
                        (ConfigurationUserLevel.None);

            // Add an Application Setting.
            config.AppSettings.Settings.Add("ModificationDate",
                           DateTime.Now.ToLongTimeString() + " ");

            // Save the changes in App.config file.
            config.Save(ConfigurationSaveMode.Modified);

            // Force a reload of a changed section.
            ConfigurationManager.RefreshSection("appSettings");
            ShowConfig();*/

            vecteur = new TDx.TDxInput.Vector3D();
            point_relatif = new NLX.Robot.Kuka.Controller.CartesianPosition();
            liste_points = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            liste_aller_magasin = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            liste_aller_magasin_to_plateau = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            liste_placer_piece = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            liste_retour_magasin = new List<NLX.Robot.Kuka.Controller.CartesianPosition>();
            plateau = new List<Emplacement>();

            /*NLX.Robot.Kuka.Controller.CartesianPosition pts = new NLX.Robot.Kuka.Controller.CartesianPosition();
            // Devant magasin
            pts.X = 501.760773;
            pts.Y = -168.319458;
            pts.Z = 290.646942;
            pts.A = -77.6867905;
            pts.B = -5.84098625;
            pts.C = -175.76;

            liste_aller_magasin.Add(pts);*/

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
            Console.WriteLine("pts 3");


            // Fermeture pince

            

            NLX.Robot.Kuka.Controller.CartesianPosition pts4 = new NLX.Robot.Kuka.Controller.CartesianPosition();
            
            // lève la pièce
            pts4.X = 515.147888;
            pts4.Y = 237.083374;
            pts4.Z = 297.377289;
            pts4.A = 72.1285477;
            pts4.B = 85.9736862;
            pts4.C = -15.78833;

            liste_aller_magasin_to_plateau.Add(pts4);
            Console.WriteLine("pts 4");

            NLX.Robot.Kuka.Controller.CartesianPosition pts5 = new NLX.Robot.Kuka.Controller.CartesianPosition();
            
            // recul pour retrait piece
            pts5.X = 515.147888;
            pts5.Y = 75.3249283;
            pts5.Z = 297.377289;
            pts5.A = 72.1285477;
            pts5.B = 85.9736862;
            pts5.C = -15.78833;
            liste_aller_magasin_to_plateau.Add(pts5);
            Console.WriteLine("pts 5");

            NLX.Robot.Kuka.Controller.CartesianPosition pts6 = new NLX.Robot.Kuka.Controller.CartesianPosition();
            
            // Rotation vers plateau
            pts6.X = 660.406555;
            pts6.Y = -143.749481;
            pts6.Z = 431.472351;
            pts6.A = -107.49276;
            pts6.B = 2.0946629;
            pts6.C = -90.567405;
            liste_aller_magasin_to_plateau.Add(pts6);
            Console.WriteLine("pts 6");


            NLX.Robot.Kuka.Controller.CartesianPosition pts7 = new NLX.Robot.Kuka.Controller.CartesianPosition();
            
            // Au dessus origine plateau
            pts7.X = 677.17;
            pts7.Y = -283.23;
            pts7.Z = 391.337982;
            pts7.A = -107.49276;
            pts7.B = 2.03317857;
            pts7.C = -90.54730;
            liste_aller_magasin_to_plateau.Add(pts7);
            Console.WriteLine("pts 7");

            /*NLX.Robot.Kuka.Controller.CartesianPosition pts8 = new NLX.Robot.Kuka.Controller.CartesianPosition();
            
            // Au dessus origine plateau
            pts8.X = 935.379639;
            pts8.Y = -267.96167;
            pts8.Z = 119.363388;
            pts8.A = -107.49276;
            pts8.B = 2.03317857;
            pts8.C = -90.54730; 
            liste_aller_magasin_to_plateau.Add(pts8);*/

            Console.WriteLine("THETA : " + THETA);
            

            for (int i = 0; i < NBLIGNESPLATEAU; i++)
            {
                for (int j = 0; j < NBCOLONNESPLATEAU; j++)
                {

                    double posx = XB + j * PASX * Math.Cos(THETA) + i * PASY * Math.Sin(THETA);
                    double posy = YB - i * PASY * Math.Cos(THETA) + j * PASX * Math.Sin(THETA);


                    ptsPlateau = new NLX.Robot.Kuka.Controller.CartesianPosition();

                    ptsPlateau.X = posx;
                    ptsPlateau.Y = posy;
                    ptsPlateau.Z = 391.337982;
                    ptsPlateau.A = -107.49276;
                    ptsPlateau.B = 2.03317857;
                    ptsPlateau.C = -90.54730;
                    bool busy = false;

                    Emplacement temp = new Emplacement();
                    temp.point = ptsPlateau;
                    temp.isBusy = busy;

                    plateau.Add(temp);

                    Console.WriteLine("Emplacement(" + i + "," + j + ") => X: " + posx + "  Y: " + posy);
                    }
            }

            /*foreach (Emplacement emp in plateau)
            {
                Console.WriteLine("Plateau =>   X: " + emp.point.X + " Y: " + emp.point.Y + " Z: " + emp.point.Z + " A: " + emp.point.A + " B: " + emp.point.B + " C: " + emp.point.C);
            }*/

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
                try
                {
                    robot.GetCurrentPosition();
                    Console.WriteLine("Connection success ! ");
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Connection failed ! => " + exc.Message);
                }

                //Console.WriteLine("Mouse connected");
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

        private void buttonSavePosition_Click(object sender, EventArgs e)
        {
            NLX.Robot.Kuka.Controller.CartesianPosition point = new NLX.Robot.Kuka.Controller.CartesianPosition();

            point.X = robot.GetCurrentPosition().X;
            point.Y = robot.GetCurrentPosition().Y;
            point.Z = robot.GetCurrentPosition().Z;
            point.A = robot.GetCurrentPosition().A;
            point.B = robot.GetCurrentPosition().B;
            point.C = robot.GetCurrentPosition().C;

            liste_points.Add(point);
            listBoxSavePosition.Items.Add("X" + robot.GetCurrentPosition().X.ToString() + " Y:" + robot.GetCurrentPosition().Y.ToString() + " Z:" + robot.GetCurrentPosition().Z.ToString());
        }


        private void buttonReadSensor_Click(object sender, EventArgs e)
        {
            labelReadSensor.Text = robot.ReadSensor().ToString();
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
            robot.OpenGripper();
            robot.PlayTrajectory(liste_aller_magasin);
            robot.CloseGripper();
        }

        private void buttonAllerPlateau_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Aller origine plateau");
            robot.PlayTrajectory(liste_aller_magasin_to_plateau);


            //PlacerPiece();




        }

        
        private void buttonPlacerPiece_Click(object sender, EventArgs e)
        {
            liste_placer_piece.Clear();
            Console.WriteLine("count:"+liste_placer_piece.Count);
            for (int i = 0; i < plateau.Count; i++)
            {
                if (!plateau.ElementAt(i).isBusy)
                {
                    // Indique que l'emplacement est maintenant occupé
                    bool vrai = true;
                    Emplacement empl = new Emplacement();
                    empl.isBusy = true;
                    empl.point = plateau.ElementAt(i).point;
                    plateau.RemoveAt(i);
                    plateau.Insert(i, empl);
                    // Ajoute la position de l'emplacement à la liste
                    liste_placer_piece.Add(plateau.ElementAt(i).point);

                    Console.WriteLine("Point detected : " + i + " => " + " X: " + empl.point.X + " Y: " + empl.point.Y + " Z: " + empl.point.Z + " A: " + empl.point.A + " B: " + empl.point.B + " C: " + empl.point.C);

                    Emplacement empl2 = new Emplacement();
                    empl2.isBusy = true;
                    empl2.point = plateau.ElementAt(i).point;


                    // On descend la pièce
                    empl2.point.Z -= 250;
                    // Ajout de la position à la liste
                    liste_placer_piece.Add(empl2.point);
                    // On effectue la trajectoire
                    robot.PlayTrajectory(liste_placer_piece);
                    // On relache la pièce
                    robot.OpenGripper();


                    Emplacement empl3 = new Emplacement();
                    empl3.isBusy = true;
                    empl3.point = plateau.ElementAt(i).point;


                    // On remonte 
                    empl3.point.Z += 250;

                    //Console.WriteLine("X:" + empl3.point.X + "Y:" + empl3.point.Y + "Z:" + empl3.point.Z + "A:" + empl3.point.A + "B:" + empl3.point.B + "C:" + empl3.point.C);

                    // On ajout à la liste
                    liste_retour_magasin.Add(empl3.point);
                    // On se dégage de la pièce
                    robot.PlayTrajectory(liste_retour_magasin);
                    

                    // On peux arreter une fois qu'on a mis la pièce
                    break;
                }
            }   
        }
    }
}
