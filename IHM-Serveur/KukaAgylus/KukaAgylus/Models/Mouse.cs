#region Using
using System;
using KukaAgylus.Models;
using System.Threading;
using System.Collections.Generic;
using NLX.Robot.Kuka.Controller;
using TDx.TDxInput;
#endregion

namespace Mouse6d
{
    public class Mouse
    {
        public LogManager Logs { get; set; }
        public MouseInfos MouseInfos { get; set; }

        #region Attributs
        public Vector3D MoveByVector;
        public Vector3D RotateByVector;

        public double MaxTransX = 1.0;
        public double MaxTransY = 1.0;
        public double MaxTransZ = 1.0;

        public double Treshold
        {
            // Filled by a textField
            get { return MouseInfos.Treshold; }
            set { MouseInfos.Treshold = value; }
        }
        public double VitesseTranslation
        {
            get { return MouseInfos.TranslationVelocity; }
            set { MouseInfos.TranslationVelocity = value; }
        }
        public double VitesseRotation
        {
            get { return MouseInfos.RotationVelocity; }
            set { MouseInfos.RotationVelocity = value; }
        }

        private volatile bool _calibrationEnd = false;
        private volatile bool _shouldStop; // Attribut qui permet d'arreter le thread et accessible par d'autre thread (volatile)

        public CartesianPosition CartPosition = new CartesianPosition();
        #endregion

        #region Constructeur
        /// <summary>
        /// Fonction qui initialise la classe
        /// </summary>
        public Mouse()
        {
            MoveByVector = new Vector3D();
            RotateByVector = new Vector3D();

        }
        #endregion

        #region Fonctions
        /// <summary>
        /// 
        /// </summary>
        public void Calibrate()
        {
            #region Variables
            Vector3D Translation;
            #endregion

            #region Loop for the calibration
            while (!_calibrationEnd)
            {
                Translation = GetTranslationVector();
                //Logs.AddLog("info", string.Format("MOUSE :  X : {0} | Y : {1} | Z : {2}", Translation.X, Translation.Y, Translation.Z));

                #region Get max X for calibration
                if (Translation.X > MaxTransX)
                {
                    MaxTransX = Translation.X;
                    Logs.AddLog("info", string.Format("MOUSE :  X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ));
                }
                #endregion

                #region Get max Y for calibration
                if (Translation.Y > MaxTransY)
                {
                    MaxTransY = Translation.Y;
                    Logs.AddLog("info", string.Format("MOUSE :  X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ));
                }
                #endregion

                #region Get max Z for calibration
                if (Translation.Z > MaxTransZ)
                {
                    MaxTransZ = Translation.Z;
                    Logs.AddLog("info", string.Format("MOUSE :  X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ));
                }
                #endregion
            }
            Logs.AddLog("info", "End Calibration mouse");
            MouseInfos.IsCalibrated = true;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        public void Loop()
        {
            #region Variables
            Vector3D VectorNorm = new Vector3D();
            Vector3D Translation;
            AngleAxis Rotation;

            var Norm = Math.Sqrt(Math.Pow(MaxTransX, 2) + Math.Pow(MaxTransY, 2) + Math.Pow(MaxTransZ, 2));
            #endregion

            #region Loop which get the information from the mouse and convert to the movement of the robot
            while (!_shouldStop)
            {
                Translation = GetTranslationVector();
                Rotation = GetRotationAxis();

                #region Normalization of the vector of the mouse
                VectorNorm.X = Translation.X / Norm;
                VectorNorm.Y = Translation.Y / Norm;
                VectorNorm.Z = Translation.Z / Norm;
                #endregion

                #region Error due to a vector's component upper than 1.0
                if (VectorNorm.X > 1.0 || VectorNorm.Y > 1.0 || VectorNorm.Z > 1.0)
                {
                    Logs.AddLog("Error", "In mouse loop: vector > 1");
                    _shouldStop = true;
                }
                #endregion

                #region Movement vector send to the robot
                // Translation alone
                if (Math.Abs(VectorNorm.X) > Treshold || Math.Abs(VectorNorm.Y) > Treshold || Math.Abs(VectorNorm.Z) > Treshold)
                {
                    MoveByVector.X = VectorNorm.X * VitesseTranslation;
                    MoveByVector.Y = VectorNorm.Y * VitesseTranslation;
                    MoveByVector.Z = VectorNorm.Z * VitesseTranslation;
                    RotateByVector.X = 0.0;
                    RotateByVector.Y = 0.0;
                    RotateByVector.Z = 0.0;
                }
                // Rotation alone
                else if (Math.Abs(Rotation.X) > Treshold || Math.Abs(Rotation.Y) > Treshold || Math.Abs(Rotation.Z) > Treshold)
                {
                    MoveByVector.X = 0.0;
                    MoveByVector.Y = 0.0;
                    MoveByVector.Z = 0.0;

                    #region Rotation X first
                    if (Math.Abs(Rotation.X) > Math.Abs(Rotation.Y) && Math.Abs(Rotation.X) > Math.Abs(Rotation.Z))
                    {
                        if (Rotation.X > 0)
                        {
                            RotateByVector.X = Rotation.Angle * VitesseRotation;
                        }
                        else
                        {
                            RotateByVector.X = -Rotation.Angle * VitesseRotation;
                        }
                        RotateByVector.Y = 0.0;
                        RotateByVector.Z = 0.0;
                    }
                    #endregion

                    #region Rotation Y first
                    if (Math.Abs(Rotation.Y) > Math.Abs(Rotation.X) && Math.Abs(Rotation.Y) > Math.Abs(Rotation.Z))
                    {
                        RotateByVector.X = 0.0;

                        if (Rotation.Y > 0)
                        {
                            RotateByVector.Y = Rotation.Angle * VitesseRotation;
                        }
                        else
                        {
                            RotateByVector.Y = -Rotation.Angle * VitesseRotation;
                        }

                        RotateByVector.Z = 0.0;
                    }
                    #endregion

                    #region Rotation Z first
                    if (Math.Abs(Rotation.Z) > Math.Abs(Rotation.Y) && Math.Abs(Rotation.Z) > Math.Abs(Rotation.X))
                    {
                        RotateByVector.X = 0.0;
                        RotateByVector.Y = 0.0;

                        if (Rotation.Z > 0)
                        {
                            RotateByVector.Z = Rotation.Angle * VitesseRotation;
                        }
                        else
                        {
                            RotateByVector.Z = -Rotation.Angle * VitesseRotation;
                        }
                    }
                    #endregion
                }
                else
                {
                    MoveByVector.X = 0.0;
                    MoveByVector.Y = 0.0;
                    MoveByVector.Z = 0.0;
                    RotateByVector.X = 0.0;
                    RotateByVector.Y = 0.0;
                    RotateByVector.Z = 0.0;
                }
                #endregion
                //Logs.AddLog("info", string.Format("Mouse rotation: X: {0} | Y: {1} | Z: {2}", RotateByVector.X, RotateByVector.Y, RotateByVector.Z));

                CartPosition = new CartesianPosition()
                {
                    X = -MoveByVector.Z,
                    Y = -MoveByVector.X,
                    Z = MoveByVector.Y,
                    A = -RotateByVector.Z,
                    B = -RotateByVector.X,
                    C = RotateByVector.Y
            };
            }
            #endregion
        }


        public void CalibrationStop()
        {
            _calibrationEnd = true;
        }

        /// <summary>
        /// Fonction qui demande d'arreter le thread en cours d'execution
        /// </summary>
        public void RequestStop()
        {
            _shouldStop = true;
        }

        public Vector3D GetTranslationVector()
        {
            Vector3D translation = new Vector3D();
            translation.X = MouseInfos.TranslationX;
            translation.Y = MouseInfos.TranslationY;
            translation.Z = MouseInfos.TranslationZ;
            return translation;
        }

        public AngleAxis GetRotationAxis()
        {
            AngleAxis rotation = new AngleAxis();
            rotation.X = MouseInfos.RotationX;
            rotation.Y = MouseInfos.RotationY;
            rotation.Z = MouseInfos.RotationZ;
            rotation.Angle = MouseInfos.Angle;
            return rotation;
        }
        #endregion

    }
}
