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
            tm.RunAllTest();
        }
    }
}
