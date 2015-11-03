using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mouse6d
{
    class Program
    {
        static TDx.TDxInput.Device mouse;

        static double maxTransX = 1.0;
        static double maxTransY = 1.0;
        static double maxTransZ = 1.0;

        static double vitesse = 1.0;

        static void Main(string[] args)
        {
            Init();
            
            Calibrate();
            //Loop();
            //TestKeyboard();
        }

        public static void Init()
        {
            Console.WriteLine("Start program...");

            mouse = new TDx.TDxInput.Device();
            if (mouse != null)
            {
                mouse.Connect();
            }

            Console.WriteLine("End Init");
        }

        public static void Calibrate()
        {
            bool end = false;

            TDx.TDxInput.Vector3D translation;

            while (mouse.IsConnected && !end)
            {

                translation = mouse.Sensor.Translation;
                NativeKeyboard keyboard = new NativeKeyboard();

                #region getMaxX
                if (translation.X > maxTransX)
                {
                    maxTransX = translation.X;
                }
                #endregion

                #region getMaxY
                if (translation.Y > maxTransY)
                {
                    maxTransY = translation.Y;
                }
                #endregion

                #region getMaxZ
                if (translation.Z > maxTransZ)
                {
                    maxTransZ = translation.Z;
                }
                #endregion

                #region break while
                if (keyboard.IsKeyDown(NativeKeyboard.KeyCode.Enter)) 
                {
                    end = true;
                }
                #endregion
            }
        }

        public static void TestKeyboard()
        {
            NativeKeyboard _NativeKeyboard = new NativeKeyboard();
            var keyboard = mouse.Keyboard;
 
            while (true)
            //while (!keyboard.IsKeyDown((int) NativeKeyboard.KeyCode.Enter))
            {
                if (_NativeKeyboard.IsKeyDown(NativeKeyboard.KeyCode.Enter))
                {
                    Console.WriteLine("Enter");
                    Console.WriteLine("End Test");
                    Thread.Sleep(50);
                }
            }
            
        }

        public static void Loop()
        {
            bool end = false;
            TDx.TDxInput.Vector3D vectorNorm = new TDx.TDxInput.Vector3D();
            TDx.TDxInput.Vector3D translation;

            var norm = Math.Sqrt(Math.Pow(maxTransX, 2) + Math.Pow(maxTransY, 2) + Math.Pow(maxTransZ, 2));

            while (mouse.IsConnected && !end)
            {
                translation = mouse.Sensor.Translation;

                vectorNorm.X = translation.X / norm * vitesse;
                vectorNorm.Y = translation.Y / norm * vitesse;
                vectorNorm.Z = translation.Z / norm * vitesse;

                #region wrong calibration
                if (vectorNorm.X > 1.0 || vectorNorm.Y > 1.0 || vectorNorm.Z > 1.0)
                {
                    Console.WriteLine("Error, vector > 1");
                    end = true;
                }
                #endregion
            }
        }

    // End class
    }
}
