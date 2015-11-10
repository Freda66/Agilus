using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DaemonMouse
{
    class Program
    {
        static TDx.TDxInput.Device device;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting mouse6D daemon");

            device = new TDx.TDxInput.Device();
            var connect = false;
            try
            {
                device.Connect();
                connect = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Mouse connection failed !: ", ex.Data);
            }
            var oldX = 0.0;
            var oldY = 0.0;
            var oldZ = 0.0;
            var oldRX = 0.0;
            var oldRY = 0.0;
            var oldRZ = 0.0;
            var oldAngle = 0.0;
            while (connect)
            {
                var translation = device.Sensor.Translation;
                var rotation = device.Sensor.Rotation;

                try
                {
                    if (oldX != translation.X || oldY != translation.Y || oldZ != translation.Z || oldRX != rotation.X || oldRY != rotation.Y || oldRZ != rotation.Z || oldAngle != rotation.Angle)
                    {
                        ServiceMouse.MouseSoapClient service = new ServiceMouse.MouseSoapClient();

                        Console.WriteLine("Translation : {0}\r\nRotation : {1}",
                            string.Format("X={0} Y={1} Z={2}", translation.X, translation.Y, translation.Z),
                            string.Format("X={0} Y={1} Z={2} Angle={3}", rotation.X, rotation.Y, rotation.Z, rotation.Angle));

                        service.SendMousePosition(translation.X, translation.Y, translation.Z, rotation.X, rotation.Y, rotation.Z, rotation.Angle);
                    }
                }
                catch (Exception e) { Console.WriteLine(e.Data); }

                oldX = translation.X;
                oldY = translation.Y;
                oldZ = translation.Z;
                oldRX = rotation.X;
                oldRY = rotation.Y;
                oldRZ = rotation.Z;
                oldAngle = rotation.Angle;

                Thread.Sleep(150);
            }

            Console.WriteLine("Daemon mouse closed !");
            Console.ReadKey();
        }
    }
}
