using System;
using System.Threading;
using NLX.Robot.Kuka.Controller;

namespace Mouse6d
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start program...");
            Mouse MyMouse = new Mouse();
            RobotController MyRobot = new RobotController();

            // Calibre la souris, valider la calibration avec la touche 4
            MyMouse.Calibrate();

            // Connexion au robot
            MyRobot.Connect("192.168.1.1");

            // Ouvre ou ferme la pince
            if (MyRobot.IsGripperOpen()) MyRobot.CloseGripper();
            else MyRobot.OpenGripper();

            // Envoi la commande au robot
            MyRobot.StartRelativeMovement();

            // Créer un objet thread de l'objet Mouse, fonction Loop
            Thread MouseThread = new Thread(MyMouse.Loop);

            // Demarre le thread.
            MouseThread.Start();
            Console.WriteLine("main thread: Starting mouse thread...");

            // Attend que le thread soit lancé et activé
            while (!MouseThread.IsAlive);
            Console.WriteLine("main thread: Mouse alive");

            // Boucle tant qu'on utilise la souris 
            bool endMouse = true;
            while (endMouse)
            {
                Console.WriteLine("main thread: endMouse = True");
                // Met le thread principale (ici) en attente d'une millisecond pour autoriser le thread secondaire à faire quelque chose
                Thread.Sleep(1);
                // Convertion des donnees de la souris pour le robot 
                CartesianPosition CartPositionMouse = new CartesianPosition();
                CartPositionMouse.A = 0.0; CartPositionMouse.B = 0.0; CartPositionMouse.C = 0.0;
                CartPositionMouse.X = MyMouse.MoveByVector.X;
                CartPositionMouse.Y = MyMouse.MoveByVector.Y;
                CartPositionMouse.Z = MyMouse.MoveByVector.Z;
                CartPositionMouse.A = MyMouse.RotateByVector.X;
                CartPositionMouse.B = MyMouse.RotateByVector.Y;
                CartPositionMouse.C = MyMouse.RotateByVector.Z;
                Console.WriteLine("main thread: X : {0} | Y : {1} | Z : {2}", CartPositionMouse.X, CartPositionMouse.Y, CartPositionMouse.Z);
                // Envoi les commande de deplacement au robot
                MyRobot.SetRelativeMovement(CartPositionMouse);
            }

            // Demande l'arret du thread de la souris
            MyMouse.RequestStop();
            
            // Bloque le thread principale tant que le thread Mouse n'est pas terminé
            MouseThread.Join();

            // Arret le mouvement
            MyRobot.StopRelativeMovement();

            Console.WriteLine("main thread: mouse thread has terminated.");
            
            Console.WriteLine("End program... Press a key to quit...");
            Console.ReadKey();
        }

    // End class
    }
}
