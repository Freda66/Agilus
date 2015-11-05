﻿#region Using
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

        #region Attributs
        public TDx.TDxInput.Device Mouse6d;

        public TDx.TDxInput.Vector3D MoveByVector;
        public TDx.TDxInput.Vector3D RotateByVector;

        public double MaxTransX = 1.0;
        public double MaxTransY = 1.0;
        public double MaxTransZ = 1.0;

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
            #region Mouse connection
            Mouse6d = new TDx.TDxInput.Device();
            if (Mouse6d != null)
            {
                Mouse6d.Connect();
            }
            #endregion

            MoveByVector = new TDx.TDxInput.Vector3D() { X = 0.0, Y = 0.0, Z = 0.0 };
            RotateByVector = new TDx.TDxInput.Vector3D() { X = 0.0, Y = 0.0, Z = 0.0 };
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
            while (Mouse6d.IsConnected && !_calibrationEnd)
            {
                Translation = Mouse6d.Sensor.Translation;
                NativeKeyboard Keyboard = new NativeKeyboard();

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

                //#region break while
                //if (Keyboard.IsKeyDown(NativeKeyboard.KeyCode.Enter))
                //{
                //    _calibrationEnd = true;
                //}
                //#endregion
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
            while (Mouse6d.IsConnected || !_shouldStop)
            {
                Translation = Mouse6d.Sensor.Translation;
                Rotation = Mouse6d.Sensor.Rotation;

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
                MoveByVector.X = VectorNorm.X * Vitesse;
                MoveByVector.Y = VectorNorm.Y * Vitesse;
                MoveByVector.Z = VectorNorm.Z * Vitesse;
                #endregion

                #region Rotation vector send to the robot
                RotateByVector.X = Rotation.X;
                RotateByVector.Y = Rotation.Y;
                RotateByVector.Z = Rotation.Z;
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
        public MouseInfos GetMouseInfos()
        {
            return new MouseInfos()
            {
                TranslationX = MoveByVector.X,
                TranslationY = MoveByVector.Y,
                TranslationZ = MoveByVector.Z,
                RotationX = RotateByVector.X,
                RotationY = RotateByVector.Y,
                RotationZ = RotateByVector.Z,
                IsCalibrated = _isCalibrated,
                IsConnected = Mouse6d.IsConnected
            };
        }

    }
}
