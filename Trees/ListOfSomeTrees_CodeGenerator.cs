using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace Trees
{
    public class ListOfSomeTrees_CodeGenerator
    {
        public static void GenerateCode(TextWriter output, int numOfNodes, int interval_powerOfTwo)
        {
            Tree t = new Tree(numOfNodes);

            int numOfTreesInOneGo = 1 << interval_powerOfTwo;
            int moduloMask = numOfTreesInOneGo - 1;

            output.WriteLine(
@"using System.Collections.Generic;
using System.Collections;

namespace Trees
{
    public class TreeList : IEnumerable<string>
    {
        public int intervalSize = " + numOfTreesInOneGo + @";
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {");
            //body

            long Count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            do
            {
                if ((Count & moduloMask) == 0)
                {
                    output.WriteLine(
$"            yield return \"{t}\"; //numOfCluster: {Count / numOfTreesInOneGo}, treeCount: {Count}");
                    Console.WriteLine($"Trees done: {Count}, Elapsed: {sw.Elapsed}, unbal: {t.UnbalancedTrees}, all: {t.UnbalancedTrees + Count}, ClusterSize: {numOfTreesInOneGo}");
                }

                Count++;
            }
            while (t.NextBalancedTree());
            Console.WriteLine("==== DONE ====");
            Console.WriteLine($"Trees done: {Count}, Elapsed: {sw.Elapsed}, unbal: {t.UnbalancedTrees}, all: {t.UnbalancedTrees + Count}");

            //end body
            output.WriteLine(@"
        }
    }
}");




        }
    }
}
