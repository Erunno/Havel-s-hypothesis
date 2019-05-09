using System;
using Trees;
using Validator;
using System.Diagnostics;
using MachingCheck;
using System.Threading;
using System.IO;

namespace Havel_s_hypothesis
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadManager tm = new ThreadManager(5);

            tm.emailLogger = new ThreadManager.EmailLogger(iterval_percentage: 5);
            tm.emailLogger.AddReceiver("matyas.brabec@seznam.cz");

            tm.RunAllTest();

            string line;
            Console.WriteLine();
            Console.WriteLine("Type \"exit\" to exit app");

            while ((line = Console.ReadLine()).ToUpper() != "EXIT")
                Console.WriteLine("Type \"exit\" to exit app");
        }
    }
}
