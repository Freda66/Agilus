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

            MyMouse.Calibrate();

            // Connexion au robot
            MyRobot.Connect("192.168.1.1");
            
            // Créer un objet thread de l'objet Mouse, fonction Loop
            Thread MouseThread = new Thread(MyMouse.Loop);

            // Demarre le thread.
            MouseThread.Start();
            Console.WriteLine("main thread: Starting mouse thread...");

            // Attend que le thread soit lancé et activé
            while (!MouseThread.IsAlive);

            // Boucle tant qu'on utilise la souris 
            bool endMouse = true;
            while (!endMouse)
            {
                // Met le thread principale (ici) en attente d'une millisecond pour autoriser le thread secondaire à faire quelque chose
                Thread.Sleep(1);
                // Envoi la commande au robot
                MyRobot.StartRelativeMovement();
                // Convertion des donnees de la souris pour le robot 
                CartesianPosition CartPositionMouse = new CartesianPosition();
                CartPositionMouse.A = 0.0; CartPositionMouse.B = 0.0; CartPositionMouse.C = 0.0;
                CartPositionMouse.X = MyMouse.MoveByVector.X;
                CartPositionMouse.Y = MyMouse.MoveByVector.Y;
                CartPositionMouse.Z = MyMouse.MoveByVector.Z;
                // Envoi les commande de deplacement au robot
                MyRobot.SetRelativeMovement(CartPositionMouse);
                // Arret le mouvement
                MyRobot.StopRelativeMovement();
            }

            // Demande l'arret du thread de la souris
            MyMouse.RequestStop();
            
            // Bloque le thread principale tant que le thread Mouse n'est pas terminé
            MouseThread.Join(); 

            Console.WriteLine("main thread: mouse thread has terminated.");
            
            Console.WriteLine("End program... Press a key to quit...");
            Console.ReadKey();
        }

    // End class
    }
}
