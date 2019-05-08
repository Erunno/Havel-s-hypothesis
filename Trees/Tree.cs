using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Trees
{
    public class Tree
    {
        public Nodes[] binaryRepr;
        Context[] contexts;
        public long UnbalancedTrees = 0;
        public int NumOfNodes { get; }
        public int FirstChangedOne { get; private set; } = 0;
        private int indexOfFirstChangedOne;

        public Tree(int numOfNodes)
        {
            binaryRepr = new Nodes[2 * numOfNodes + 1];
            contexts = new Context[2 * numOfNodes + 1];

            for (int i = 0; i < contexts.Length; i++)
            {
                contexts[i] = new Context(i)
                {
                    rightSon = 2 + i,
                    numOfNodes = i % 2 == 0 ? numOfNodes - i / 2 : 0,
                };
                binaryRepr[i] = (Nodes)(i % 2 == 0 ? 1 : 0);
            }

            binaryRepr[binaryRepr.Length - 1] = Nodes.nil;

            NumOfNodes = numOfNodes;
        }

        #region Tree from String

        public Tree(string stringRepr)
        {
            binaryRepr = new Nodes[stringRepr.Length];
            contexts = new Context[stringRepr.Length];
            NumOfNodes = (stringRepr.Length - 1) / 2;

            for (int i = 0; i < stringRepr.Length; i++)
                if (stringRepr[i] == '1')
                    binaryRepr[i] = Nodes.valid;

            FillContext_ResturnLastIndex(0); 
        }

        public int FillContext_ResturnLastIndex(int myIndex)
        {
            Context myContext = contexts[myIndex] = new Context(myIndex);

            if (binaryRepr[myIndex] == Nodes.nil)
                return myIndex + 1;

            myContext.rightSon = FillContext_ResturnLastIndex(myIndex + 1);
            int myEnd = FillContext_ResturnLastIndex(myContext.rightSon);

            myContext.numOfNodes = contexts[myContext.rightSon].numOfNodes + contexts[myIndex + 1].numOfNodes + 1;

            return myEnd;
        }

        #endregion

        #region Generating next tree
        public bool NextBalancedTree()
        {
            Result res;
            indexOfFirstChangedOne = int.MaxValue; //restarting min index
            
            do
            {
                UnbalancedTrees++;
                res = NextTree(0);
            }
            while (res == Result.notYet && !IsBalanced());

            UnbalancedTrees--; //last was balanced

            UpdateFirstChangedOne();

            return res == Result.notYet;
        }

        public enum Nodes { nil = 0, valid = 1 };

        private enum Result { allTreesGenerated, notYet }

        class Context
        {
            public int originIndex { get; }

            public Context(int originIndex) => this.originIndex = originIndex;

            public int rightSon;
            public void ShiftRightSon() => rightSon += 2;

            public int numOfNodes;
            public int LeftSonNumOfNodes => (rightSon - originIndex - 2) / 2;
            public int RightSonNumOfNodes => numOfNodes - LeftSonNumOfNodes - 1;


            public override string ToString() => $"i: {originIndex}, rSon: {rightSon}, numN: {numOfNodes}, lnumN: {LeftSonNumOfNodes}, rnumN: {RightSonNumOfNodes}";
        }

        private Result NextTree(int index)
        {
            Context myContext = contexts[index];

            if (myContext.numOfNodes == 0)
                return Result.allTreesGenerated;

            if (NextTree(myContext.rightSon) == Result.notYet)
                return Result.notYet;

            if (NextTree(index + 1) == Result.notYet) //my left son
            {
                GenerateBaseTreeTo(myContext.rightSon, myContext.RightSonNumOfNodes); //restart of right son
                return Result.notYet;
            }

            if (myContext.LeftSonNumOfNodes == (myContext.numOfNodes - 1) / 2) //i dont have to generate symetric trees
                return Result.allTreesGenerated;

            myContext.ShiftRightSon();

            GenerateBaseTreeTo(myContext.rightSon, myContext.RightSonNumOfNodes); 
            GenerateBaseTreeTo(index + 1, myContext.LeftSonNumOfNodes);                 

            return Result.notYet;
        }

        private void GenerateBaseTreeTo(int index, int numOfNodes)
        {
            if(index < indexOfFirstChangedOne)
                indexOfFirstChangedOne = index;

            for (int i = 0; i < numOfNodes * 2; i += 2)
            {
                contexts[i + index].numOfNodes = numOfNodes - i / 2;
                contexts[i + index + 1].numOfNodes = 0;

                contexts[i + index].rightSon = index + i + 2;

                binaryRepr[i + index] = Nodes.valid;
                binaryRepr[i + index + 1] = Nodes.nil;
            }

            contexts[numOfNodes * 2 + index].numOfNodes = 0;
            binaryRepr[numOfNodes * 2 + index] = 0;
        }

        private void UpdateFirstChangedOne()
        {
            FirstChangedOne = NumOfNodes - 2;

            for (int i = indexOfFirstChangedOne + 1; i < binaryRepr.Length; i++)
                if (binaryRepr[i] == Nodes.valid)
                    FirstChangedOne--;
        }
        #endregion

        #region Balancing
        private struct NumOfColors
        {
            public int numOfBlack, numOfWhite;
        }

        public bool IsBalanced()
        {
            NumOfColors colors = new NumOfColors();
            CountColors(treeIndex: 0, ref colors, imWhite: true);
            return colors.numOfWhite == colors.numOfBlack;
        }

        /// <summary>
        /// Counts num of nodes in every level
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="colors"></param>
        /// <param name="imWhite"></param>
        /// <returns>Index behind tree</returns>
        private int CountColors(int treeIndex, ref NumOfColors colors, bool imWhite)
        {
            if (binaryRepr[treeIndex] == Nodes.nil)
                return treeIndex + 1;

            if (imWhite)
                colors.numOfWhite++;
            else
                colors.numOfBlack++;

            int rightNeighbor = CountColors(treeIndex + 1, ref colors, imWhite ^ true); //left tree 
            return CountColors(rightNeighbor, ref colors, imWhite ^ true);              //right tree
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < binaryRepr.Length; i++)
                sb.Append((int)binaryRepr[i]);

            return sb.ToString();
        }
    }
}
