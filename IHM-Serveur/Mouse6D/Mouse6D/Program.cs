using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Mouse6D
{
    class Program
    {
        static TDx.TDxInput.Device device;

        static void Main(string[] args)
        {
            Console.WriteLine("Daemon souris 6D en cours");

            device = new TDx.TDxInput.Device();
            var connect = false;
            try
            {
                device.Connect();
                connect = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection failed !: ", ex.Data);
            }

            while (connect)
            {
                var translation = device.Sensor.Translation;
                var rotation = device.Sensor.Rotation;

                try
                {
                    Mouse.MouseSoapClient serv = new Mouse.MouseSoapClient();
                
                    Console.WriteLine("Translation : {0}\r\nRotation : {1}",
                        string.Format("X={0} Y={1} Z={2}", translation.X, translation.Y, translation.Z),
                        string.Format("X={0} Y={1} Z={2}", rotation.X, rotation.Y, rotation.Z));

                    serv.SendMousePosition(translation.X, translation.Y, translation.Z, rotation.X, rotation.Y, rotation.Z);
                } catch(Exception e) {}

                Thread.Sleep(100);
            }

            Console.WriteLine("Daemon mouse closed !");
            Console.ReadKey();
        }
    }
}
