using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLX.Robot.Kuka.Controller;

namespace ConsoleApplication1
{
    class Plateau
    {
        private const int NBLIGNESPLATEAU = 4;      // Nombre de lignes du plateau
        private const int NBCOLONNESPLATEAU = 4;    // Nombre de colonnes du plateau
        private const double XD = 935.37;           // Dernière position
        private const double YD = -267.96;          // 
        private const double XB = 678.63;           // Origine
        private const double YB = -289.64;          //
        private const double XA = 775.02;           //
        private const double YA = -405.31;          //
        private const double XC = 839.75;           //
        private const double YC = -154.96;          //

        /*  D   O   O   A
         *  O   O   O   O
         *  O   O   O   O
         *  C   O   O   B
         *              
         *              KUKA
         * */
        private double THETA = Math.Acos((XC - XB) / Math.Sqrt(Math.Pow(XC - XB, 2) + Math.Pow(YC - YB, 2)));   // Angle pour compenser l'orientation du robot par rapport au plateau

        private double PASX = Math.Sqrt(Math.Pow(XC - XB, 2) + Math.Pow(YC - YB, 2)) / (NBCOLONNESPLATEAU - 1); // Espacement entre les trous sur les X
        private double PASY = Math.Sqrt(Math.Pow(XA - XB, 2) + Math.Pow(YA - YB, 2)) / (NBLIGNESPLATEAU - 1);   // Espacement entre les trous sur les Y 
        private CartesianPosition point = new CartesianPosition();  // Point situé au dessus du plateau (avant la descente)

        // Plateau qui contient tous les emplacements
        public List<Robot.RobotActions.Emplacement> plateau;


        // Fonction qui renvoi l'intégralité du plateau
        public List<Robot.RobotActions.Emplacement> GetPlateau()
        {
            return this.plateau;
        }


        // Constructeur 
        public Plateau()
        {
            // Initialisation du plateau
            plateau = new List<Robot.RobotActions.Emplacement>();

            // Initialisation des emplacements
            this.init();
        }

        private void init()
        {
            // Parcours du plateau
            for (int i = 0; i < NBLIGNESPLATEAU; i++)
                {
                    for (int j = 0; j < NBCOLONNESPLATEAU; j++)
                    {

                        // Création des coordonées X et Y
                        double posx = XB + j * PASX * Math.Cos(THETA) + i * PASY * Math.Sin(THETA);
                        double posy = YB - i * PASY * Math.Cos(THETA) + j * PASX * Math.Sin(THETA);

                        // Création d'un nouveau point en conservant le Z et les angles pour avoir une approche identique du plateau
                        point = new NLX.Robot.Kuka.Controller.CartesianPosition();
                        point.X = posx;
                        point.Y = posy;
                        point.Z = 144.49;//391.337982;
                        point.A = 54;
                        point.B = -0.54;
                        point.C = 91.58;

                        // On marque l'emplacement comme étant libre
                        bool busy = false;
                        Robot.RobotActions.Emplacement temp = new Robot.RobotActions.Emplacement();
                        temp.point = point;
                        temp.isBusy = busy;

                        // Ajout de l'emplacement 
                        this.plateau.Add(temp);

                        Console.WriteLine("Emplacement(" + i + "," + j + ") => X: " + posx + "  Y: " + posy);
                    }
                }
        }
    }
}
