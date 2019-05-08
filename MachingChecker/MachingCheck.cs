using System;
using Trees;
using System.Linq;

namespace MachingCheck
{
    class TreeFormatConvertor
    {
        public int[] Maching;

        private const int unvalid = -1;

        public TreeFormatConvertor(int dimension)
        {
            Maching = new int[2 * (1 << dimension) + 1];
        }

        public void ConvertFromShrinkedFormat(Tree.Nodes[] binRepr, int[] shrinkedMaching)
        {
            if (binRepr.Length != Maching.Length)
                throw new ArgumentException("Tree has diffent dimension");

            int currIndexInShrinked = 0;

            for (int i = 0; i < Maching.Length; i++)
            {
                if (binRepr[i] == Tree.Nodes.valid)
                    Maching[i] = shrinkedMaching[currIndexInShrinked++];
                else
                    Maching[i] = unvalid;
            }
        }
    }

    public class MachingChecker
    {
        private TreeFormatConvertor convertor;
        private int[] oneBitNumbers;
        private bool[] UsedNodes;
        private int[] Maching;


        public MachingChecker(int dimension)
        {
            convertor = new TreeFormatConvertor(dimension);
            oneBitNumbers = new int[dimension];
            UsedNodes = new bool[1 << dimension];
            Maching = convertor.Maching;

            oneBitNumbers[0] = 1;
            for (int i = 1; i < dimension; i++)
                oneBitNumbers[i] = oneBitNumbers[i - 1] << 1;
        }

        /// <summary>
        /// Throws exception in case that maching was incorrect
        /// </summary>
        public void IsCorrectMaching(Tree tree, int[] maching)
        {
            for (int i = 0; i < UsedNodes.Length; i++)
                UsedNodes[i] = false;

            binReprTree = tree.binaryRepr;

            convertor.ConvertFromShrinkedFormat(binReprTree, maching);

            //checking
            UsedNodes[maching[0]] = true;
            int rightTree = CheckSubtree_ReturnYourEndIndex(maching[0], 1);
            CheckSubtree_ReturnYourEndIndex(maching[0], rightTree);
        }

        private Tree.Nodes[] binReprTree;

        private int CheckSubtree_ReturnYourEndIndex(int parentsHyperNode, int myIndex)
        {
            if (binReprTree[myIndex] == Tree.Nodes.nil)
                return myIndex + 1;

            if (!CorrectNeigborhood(parentsHyperNode, Maching[myIndex]))
                throw new IncorrectMachingEx();


            TakeHyperNode(Maching[myIndex]);

            int rightSon = CheckSubtree_ReturnYourEndIndex(Maching[myIndex], myIndex + 1);
            return CheckSubtree_ReturnYourEndIndex(Maching[myIndex], rightSon);
        }

        private bool CorrectNeigborhood(int node1, int node2) => oneBitNumbers.Contains(node1 ^ node2);
        private void TakeHyperNode(int hyperNode) => UsedNodes[hyperNode] = UsedNodes[hyperNode] == false ? true : throw new IncorrectMachingEx();

        class IncorrectMachingEx : Exception { }
    }
}
