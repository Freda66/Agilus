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
            //if (MyRobot.IsGripperOpen()) MyRobot.CloseGripper();
            //else MyRobot.OpenGripper();

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
                // Met le thread principale (ici) en attente d'une millisecond pour autoriser le thread secondaire à faire quelque chose
                Thread.Sleep(1);
                // Convertion des donnees de la souris pour le robot 
                CartesianPosition CartPositionMouse = new CartesianPosition();
                CartPositionMouse.X = -MyMouse.MoveByVector.Z;
                CartPositionMouse.Y = -MyMouse.MoveByVector.X;
                CartPositionMouse.Z = MyMouse.MoveByVector.Y;
                CartPositionMouse.A = -MyMouse.RotateByVector.Z;
                CartPositionMouse.B = -MyMouse.RotateByVector.X;
                CartPositionMouse.C = MyMouse.RotateByVector.Y;
                Console.WriteLine("cmd robot: X : {0} | Y : {1} | Z : {2} | A : {3} | B : {4} | C : {5}", CartPositionMouse.X, CartPositionMouse.Y, CartPositionMouse.Z, CartPositionMouse.A, CartPositionMouse.B, CartPositionMouse.C);
                // Envoi les commandes de deplacement au robot
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
