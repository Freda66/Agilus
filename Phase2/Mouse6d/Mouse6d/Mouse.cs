#region Using
using System;
using System.Threading;
#endregion

namespace Mouse6d
{
    class Mouse
    {

        #region Attributs
        public TDx.TDxInput.Device Mouse6d;

        public TDx.TDxInput.Vector3D MoveByVector;
        public TDx.TDxInput.Vector3D RotateByVector;

        public double MaxTransX = 1.0;
        public double MaxTransY = 1.0;
        public double MaxTransZ = 1.0;

        public double Treshold = 0.1; // Filled by a textField
        public double Vitesse = 100.0;
        
        private volatile bool _shouldStop; // Attribut qui permet d'arreter le thread et accessible par d'autre thread (volatile)
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

            MoveByVector = new TDx.TDxInput.Vector3D(0.0, 0.0, 0.0);
            RotateByVector = new TDx.TDxInput.Vector3D(0.0, 0.0, 0.0);
        }
        #endregion

        #region Fonctions
        /// <summary>
        /// 
        /// </summary>
        public void Calibrate()
        {
            #region Variables
            bool End = false;

            TDx.TDxInput.Vector3D Translation;
            #endregion

            #region Loop for the calibration
            while (Mouse6d.IsConnected && !End)
            {
                Translation = Mouse6d.Sensor.Translation;
                NativeKeyboard Keyboard = new NativeKeyboard();

                #region Get max X for calibration
                if (Translation.X > MaxTransX)
                {
                    MaxTransX = Translation.X;
                    Console.WriteLine("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ);
                }
                #endregion

                #region Get max Y for calibration
                if (Translation.Y > MaxTransY)
                {
                    MaxTransY = Translation.Y;
                    Console.WriteLine("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ);
                }
                #endregion

                #region Get max Z for calibration
                if (Translation.Z > MaxTransZ)
                {
                    MaxTransZ = Translation.Z;
                    Console.WriteLine("X : {0} | Y : {1} | Z : {2}", MaxTransX, MaxTransY, MaxTransZ);
                }
                #endregion

                #region break while
                if (Keyboard.IsKeyDown(NativeKeyboard.KeyCode.Enter))
                {
                    End = true;
                }
                #endregion
            }
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
                    Console.WriteLine("Error, vector > 1");
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
            }
            #endregion
        }

        /// <summary>
        /// Fonction qui demande d'arreter le thread en cours d'execution
        /// </summary>
        public void RequestStop()
        {
            _shouldStop = true;
        }
        #endregion 

    }
}
