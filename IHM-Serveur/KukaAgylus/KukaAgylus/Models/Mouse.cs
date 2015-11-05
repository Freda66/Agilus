#region Using
using System;
using KukaAgylus.Models;
using System.Threading;
using System.Collections.Generic;
using NLX.Robot.Kuka.Controller;
#endregion

namespace Mouse6d
{
    public class Mouse
    {
        public List<Log> LogsList { get; set; }
        public MouseInfos MouseInfos { get; set; }

        #region Attributs

        public TDx.TDxInput.Vector3D MoveByVector;
        public TDx.TDxInput.Vector3D RotateByVector;

        public double MaxTransX = 1.0;
        public double MaxTransY = 1.0;
        public double MaxTransZ = 1.0;

        public double Treshold = 0.1; // Filled by a textField
        public double Vitesse = 100.0;

        private volatile bool _calibrationEnd = false;

        private volatile bool _shouldStop; // Attribut qui permet d'arreter le thread et accessible par d'autre thread (volatile)
        private bool _isCalibrated = false;

        public CartesianPosition CartPosition = new CartesianPosition();

        #endregion


        #region Constructeur
        /// <summary>
        /// Fonction qui initialise la classe
        /// </summary>
        public Mouse()
        {
            MoveByVector = new TDx.TDxInput.Vector3D();
            RotateByVector = new TDx.TDxInput.Vector3D();
        }
        #endregion

        #region Fonctions
        /// <summary>
        /// 
        /// </summary>
        public void Calibrate()
        {
            _isCalibrated = false;
            #region Variables

            TDx.TDxInput.Vector3D Translation;
            #endregion
            
            #region Loop for the calibration
            while (!_calibrationEnd)
            {
                Thread.Sleep(100);
                Translation = GetTranslationVector();
                LogsList.Add(new Log("info", string.Format("MOUSE :  X : {0} | Y : {1} | Z : {2}", Translation.X, Translation.Y, Translation.Z)));
            
                #region Get max X for calibration
                if (Translation.X > MaxTransX)
                {
                    MaxTransX = Translation.X;
                    LogsList.Add(new Log("info", string.Format("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ)));
                }
                #endregion

                #region Get max Y for calibration
                if (Translation.Y > MaxTransY)
                {
                    MaxTransY = Translation.Y;
                    LogsList.Add(new Log("info", string.Format("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ)));
                }
                #endregion

                #region Get max Z for calibration
                if (Translation.Z > MaxTransZ)
                {
                    MaxTransZ = Translation.Z;
                    LogsList.Add(new Log("info", string.Format("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ)));
                }
                #endregion
            }
            LogsList.Add(new Log("info", "End Calibration mouse"));
            _isCalibrated = true;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        public void Loop()
        {
            #region Variables
            TDx.TDxInput.Vector3D VectorNorm = new TDx.TDxInput.Vector3D();
            TDx.TDxInput.Vector3D Translation;
            TDx.TDxInput.AngleAxis Rotation;

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
                    //Console.WriteLine("Error, vector > 1");
                    LogsList.Add(new Log("Error", "Error in mouse loop(): vector > 1"));
                    _shouldStop = true;
                }
                #endregion
                
                #region Movement vector send to the robot
                // Translation alone
                if (VectorNorm.X > Treshold || VectorNorm.Y > Treshold || VectorNorm.Z > Treshold)
                {
                    MoveByVector.X = VectorNorm.X * Vitesse;
                    MoveByVector.Y = VectorNorm.Y * Vitesse;
                    MoveByVector.Z = VectorNorm.Z * Vitesse;
                    RotateByVector.X = 0.0;
                    RotateByVector.Y = 0.0;
                    RotateByVector.Z = 0.0;
                }
                // Rotation alone
                else if (Rotation.X > Treshold || Rotation.Y > Treshold || Rotation.Z > Treshold)
                {
                    MoveByVector.X = 0.0;
                    MoveByVector.Y = 0.0;
                    MoveByVector.Z = 0.0;
                    RotateByVector.X = Rotation.X;
                    RotateByVector.Y = Rotation.Y;
                    RotateByVector.Z = Rotation.Z;
                }
                #endregion

                CartPosition = new CartesianPosition()
                {
                    X = -MoveByVector.Z,
                    Y = -MoveByVector.X,
                    Z = MoveByVector.Y,
                    A = RotateByVector.X,
                    B = RotateByVector.Y,
                    C = RotateByVector.Z
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
        #endregion
        
        public TDx.TDxInput.Vector3D GetTranslationVector()
        {
            TDx.TDxInput.Vector3D translation = new TDx.TDxInput.Vector3D();
            translation.X = MouseInfos.TranslationX;
            translation.Y = MouseInfos.TranslationY;
            translation.Z = MouseInfos.TranslationZ;
            return translation;
        }

        public TDx.TDxInput.AngleAxis GetRotationAxis()
        {
            TDx.TDxInput.AngleAxis rotation = new TDx.TDxInput.AngleAxis();
            rotation.X = MouseInfos.RotationX;
            rotation.Y = MouseInfos.RotationY;
            rotation.Z = MouseInfos.RotationZ;
            return rotation;
        }
    }
}
