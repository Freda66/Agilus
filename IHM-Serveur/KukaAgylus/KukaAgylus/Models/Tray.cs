using Newtonsoft.Json;
using NLX.Robot.Kuka.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace KukaAgylus.Models
{

    /*  D   O   O   A
     *  O   O   O   O
     *  O   O   O   O
     *  C   O   O   B
     *              
     *              KUKA
     * */

    public class Tray
    {
        private class Location
        {
            public int RowIndex { get; set; }
            public int ColIndex { get; set; }
            public bool IsBusy { get; set; } = false;
        }

        private List<Location> Locations { get; set; } = new List<Location>();

        public CartesianPosition PointA { get; set; } = new CartesianPosition();
        public CartesianPosition PointB { get; set; } = new CartesianPosition();
        public CartesianPosition PointC { get; set; } = new CartesianPosition();

        public int RowsCount { get; set; }
        public int ColsCount { get; set; }

        /// <summary>
        /// Angle pour compenser l'orientation du robot par rapport au plateau
        /// </summary>
        private double Theta
        {
            get { return Math.Acos((PointC.X - PointB.X) / Math.Sqrt(Math.Pow(PointC.X - PointB.X, 2) + Math.Pow(PointC.Y - PointB.Y, 2))); }
        }

        /// <summary>
        /// Espacement entre les trous sur les X
        /// </summary>
        private double PasX
        {
            get { return Math.Sqrt(Math.Pow(PointC.X - PointB.X, 2) + Math.Pow(PointC.Y - PointB.Y, 2)) / (ColsCount - 1); }
        }

        /// <summary>
        /// Espacement entre les trous sur les Y 
        /// </summary>
        private double PasY
        {
            get { return Math.Sqrt(Math.Pow(PointA.X - PointB.X, 2) + Math.Pow(PointA.Y - PointB.Y, 2)) / (RowsCount - 1); }
        }

        /// <summary>
        /// Constructeur de classe
        /// </summary>
        public Tray()
        {
            LoadTray();
            ResetLocations();
        }

        /// <summary>
        /// Initialise la liste des Locations du plateau
        /// </summary>
        public void ResetLocations()
        {
            for (int i = 0; i < RowsCount; i++)
            {
                for (int y = 0; y < ColsCount; y++)
                {
                    Locations.Add(new Location()
                    {
                        RowIndex = i,
                        ColIndex = y
                    });
                }
            }
        }

        private CartesianPosition GetPositionByLocation(Location location)
        {
            return GetPositionByIndex(location.RowIndex, location.ColIndex);
        }

        private CartesianPosition GetPositionByIndex(int rowId, int colId)
        {
            double posx = PointB.X + colId * PasX * Math.Cos(Theta) + rowId * PasY * Math.Sin(Theta);
            double posy = PointB.Y - rowId * PasY * Math.Cos(Theta) + colId * PasX * Math.Sin(Theta);

            return new CartesianPosition()
            {
                X = posx,
                Y = posy,
                Z = PointA.Z,
                A = PointA.A,
                B = PointA.C,
                C = PointC.C
            };
        }

        private Location GetFirstBusyLocation()
        {
            var locationsBusy = from loc in Locations
                                where loc.IsBusy
                                select loc;
            if (locationsBusy.Count() > 0) return locationsBusy.First();
            else return null;
        }

        private Location GetFirstEmptyLocation()
        {
            var locationsEmpty = from loc in Locations
                                 where !loc.IsBusy
                                 select loc;
            if (locationsEmpty.Count() > 0) return locationsEmpty.First();
            else return null;
        }

        private List<CartesianPosition> GetApproachPositions(bool isWithdrawAction)
        {
            var approachPoints = new List<CartesianPosition>();
            var loc = isWithdrawAction ? GetFirstBusyLocation() : GetFirstEmptyLocation();
            if (loc != null)
            {
                //Création du point d'approche superieur
                var upperPoint = GetPositionByLocation(loc);
                upperPoint.Z += 200;
                //Création du point de depose/retrait
                var insidePoint = GetPositionByLocation(loc);
                insidePoint.Z -= 70;

                approachPoints.Add(upperPoint);
                approachPoints.Add(insidePoint);
            }
            return approachPoints;
        }

        public List<IRobotCommand> GetRobotCommand(bool isWithdrawAction)
        {
            // Liste de commande à renvoyer
            List<IRobotCommand> commands = new List<IRobotCommand>();

            // Liste des points d'approches
            var approachPoints = GetApproachPositions(isWithdrawAction);

            // Création du mouvement d'approche
            var movementApproach = new Movement()
            {
                Name = "Move to tray",
                Positions = new List<RobotPosition>()
            };
            foreach (var point in approachPoints)
            {
                movementApproach.Positions.Add(new RobotPosition()
                {
                    Position = point
                });
            }

            commands.Add(movementApproach);

            // Ajout de la commande de la pince
            commands.Add(new GripperAction()
            {
                Name = isWithdrawAction ? "Get item on tray" : "Depose item on tray",
                Command = isWithdrawAction ? GripperAction.Action.Close : GripperAction.Action.Open
            });

            // Création du mouvement de recul
            var movementWithdraw = new Movement()
            {
                Name = "Movement back from tray",
                Positions = new List<RobotPosition>()
            };
            approachPoints.Reverse();
            foreach (var point in approachPoints)
            {
                movementWithdraw.Positions.Add(new RobotPosition()
                {
                    Position = point
                });
            }
            commands.Add(movementWithdraw);

            return commands;
        }

        public void LoadTray()
        {
            if (Directory.Exists(Environment.CurrentDirectory + @"\tray")
                && File.Exists(Environment.CurrentDirectory + @"\tray\tray_calib.json"))
            {
                // Si le process existe on le charge
                string json = File.ReadAllText(Environment.CurrentDirectory + @"\tray\tray_calib.json");
                var calibPositions = JsonConvert.DeserializeObject<List<CartesianPosition>>(json);
                if (calibPositions.Count() == 3)
                {
                    this.PointA = calibPositions[0];
                    this.PointB = calibPositions[1];
                    this.PointC = calibPositions[2];
                }
            }
        }
        public void SaveTrayCalibration()
        {
            // Création du répertoire tray s'il n'existe pas
            if (!Directory.Exists(Environment.CurrentDirectory + @"\tray"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\tray");
            var calibPositions = new List<CartesianPosition>()
            {
                PointA,
                PointB,
                PointC
            };
            // Sauvegarde du process
            File.WriteAllText(Environment.CurrentDirectory + @"\tray\tray_calib.json", JsonConvert.SerializeObject(calibPositions), Encoding.UTF8);
        }
    }
}