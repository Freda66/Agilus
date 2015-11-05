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

            while (connect)
            {
                var translation = device.Sensor.Translation;
                var rotation = device.Sensor.Rotation;

                try
                {
                    ServiceMouse.MouseSoapClient service = new ServiceMouse.MouseSoapClient();

                    Console.WriteLine("Translation : {0}\r\nRotation : {1}",
                        string.Format("X={0} Y={1} Z={2}", translation.X, translation.Y, translation.Z),
                        string.Format("X={0} Y={1} Z={2} Angle={3}", rotation.X, rotation.Y, rotation.Z, rotation.Angle));

                    service.SendMousePosition(translation.X, translation.Y, translation.Z, rotation.X, rotation.Y, rotation.Z, rotation.Angle);
                }
                catch (Exception e) { }

                Thread.Sleep(100);
            }

            Console.WriteLine("Daemon mouse closed !");
            Console.ReadKey();
        }
    }
}
