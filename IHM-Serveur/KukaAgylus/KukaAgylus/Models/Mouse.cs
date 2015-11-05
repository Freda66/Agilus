﻿#region Using
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

        public double Treshold = 0.1; // Filled by a textField
        public double Vitesse = 100.0;

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
                Logs.AddLog("info", string.Format("MOUSE :  X : {0} | Y : {1} | Z : {2}", Translation.X, Translation.Y, Translation.Z));
            
                #region Get max X for calibration
                if (Translation.X > MaxTransX)
                {
                    MaxTransX = Translation.X;
                    Logs.AddLog("info", string.Format("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ));
                }
                #endregion

                #region Get max Y for calibration
                if (Translation.Y > MaxTransY)
                {
                    MaxTransY = Translation.Y;
                    Logs.AddLog("info", string.Format("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ));
                }
                #endregion

                #region Get max Z for calibration
                if (Translation.Z > MaxTransZ)
                {
                    MaxTransZ = Translation.Z;
                    Logs.AddLog("info", string.Format("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ));
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
                    //Console.WriteLine("Error, vector > 1");
                    Logs.AddLog("Error", "Error in mouse loop(): vector > 1");
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
            return rotation;
        }
        #endregion

    }
}
