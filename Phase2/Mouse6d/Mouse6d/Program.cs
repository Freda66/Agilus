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
        static Mouse MyMouse;
        static void Main(string[] args)
        {
            Console.WriteLine("Start program...");
            MyMouse = new Mouse();
            
            MyMouse.Init();

            MyMouse.Calibrate();
            
            //MyMouse.Loop();

            Console.WriteLine("End program... Press a key to quit...");
            Console.ReadKey();
        }

    // End class
    }
}
