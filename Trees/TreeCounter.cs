using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Trees
{
    public class TreeCounter
    {
        private static long[] results = new long[50];

        public static long NumOfTrees(int nodes)
        {
            if (nodes <= 1)
                return 1;

            if (results[nodes] != 0)
                return results[nodes];

            long sum = 0;
            int limit = (nodes - 1) / 2;
            for (int i = 0; i <= limit; i++)
                sum += NumOfTrees(i) * NumOfTrees(nodes - i - 1);

            return results[nodes] = sum;
        }
    }
}
