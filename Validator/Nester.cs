#define SaveProgramming


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trees;

namespace Validator
{
    public class Nester
    {
        public Nester(int dimension)
        {
            hyperCube = new HyperCubeNodes[1 << dimension]; //1 << dimension ~ 2 ^ dimension
            NumOfNodes = (1 << dimension);
            int binaryTreeArrayLenght = NumOfNodes * 2 + 1;
            parents = new int[binaryTreeArrayLenght];
            rightSons = new int[binaryTreeArrayLenght];
            currMatching = new int[binaryTreeArrayLenght];
            nextNeighbors = new int[binaryTreeArrayLenght];
            newIndexes = new int[binaryTreeArrayLenght];
            firstUnvalidNeighbor = dimension;

            neighborsInHyperCube = new int[dimension];
            neighborsInHyperCube[0] = 1;
            for (int i = 1; i < dimension; i++)
                neighborsInHyperCube[i] = neighborsInHyperCube[i - 1] << 1;
        }

        private enum HyperCubeNodes { empty, full };

        //data
        #region Data
        //not changing during one iteration
        private Tree.Nodes[] currTree;
        private int NumOfNodes { get; }
        private int firstUnvalidNeighbor;
        private int[] parents;

        //temp - not used by main algorithm
        private int[] newIndexes;
        private int[] rightSons;

        //dynamic variables
        private HyperCubeNodes[] hyperCube;
        private int[] neighborsInHyperCube;
        private int[] currMatching;
        private int[] nextNeighbors;
        int currNode;
        #endregion

        //constants
        #region Constants
        private const int unvalidValue = -1;
        private const int defaultNeighbor = 0;
        #endregion

        //test methods
        #region TestMethods

        public int[] GetYourMaching() => currMatching;
        public void TestEnterence()
        {
            currTree = IntsToNodes(new int[] { 1, 1, 0, 0, 1, 1, 0, 0, 0 });
        }
        private Tree.Nodes[] IntsToNodes(int[] tree)
        {
            Tree.Nodes[] newArr = new Tree.Nodes[tree.Length];

            for (int i = 0; i < tree.Length; i++)
                if (tree[i] == 1)
                    newArr[i] = Tree.Nodes.valid;

            return newArr;
        }
        #endregion

        //before actual run methods
        #region Before Every Tree

        /// <summary>
        /// Prepares arrays "rightSons" and "parents"
        /// </summary>
        /// <param name="tree">Arrays will be created for this tree</param>
        private void PrepareArraysFor(Tree.Nodes[] tree)
        {
            currTree = tree;
            FillRightSons();
            FillParents();
            ClearCurrMatching();
            ClearNextNeigbors();
            ShrinkParrrents();
        }

        private void RecycleArraysFor(Tree.Nodes[] tree)
        {
            currTree = tree;
            FillRightSons();
            FillParents();
            ShrinkParrrents();
        }

        #region Shrinking

        private void ShrinkParrrents()
        {
            int numOfUnvalid = 0;
            int index = 1;

            while (index < parents.Length)
            {
                if (currTree[index] == Tree.Nodes.valid)
                {
                    parents[index - numOfUnvalid] = newIndexes[parents[index]];
                    newIndexes[index] = index - numOfUnvalid;
                }
                else
                    numOfUnvalid++;

                index++;
            }

#if SaveProgramming
            int numOfNodes = currTree.Length / 2;

            for (int i = numOfNodes; i < parents.Length; i++)
                parents[i] = unvalidValue;
#endif
        }

        #endregion


        #region Clearing
        /// <summary>
        /// Prepares hyperCube to reuse
        /// </summary>
        private void ClearHyperCube()
        {
            for (int i = 0; i < hyperCube.Length; i++)
                hyperCube[i] = HyperCubeNodes.empty;
        }

        private void ClearNextNeigbors()
        {
            for (int i = 0; i < nextNeighbors.Length; i++)
                nextNeighbors[i] = defaultNeighbor;
        }

        private void ClearCurrMatching()
        {
            for (int i = 0; i < currMatching.Length; i++)
                currMatching[i] = unvalidValue;
        }
        #endregion

        #region Filling Children and Parents
        /// <summary>
        /// Fills array rightSons
        /// </summary>
        private void FillRightSons() => GoThru_FindYourRightSon_ReturnMyRightSon(0);

        private int GoThru_FindYourRightSon_ReturnMyRightSon(int myIndex)
        {
            if (currTree[myIndex] == 0)
            {
#if SaveProgramming
                rightSons[myIndex] = unvalidValue; //in case someone want use this as index
#endif
                return myIndex + 1;
            }

            int myRightSon = GoThru_FindYourRightSon_ReturnMyRightSon(myIndex + 1);
            rightSons[myIndex] = myRightSon;

            return GoThru_FindYourRightSon_ReturnMyRightSon(myRightSon);
        }

        /// <summary>
        /// Fills array parents, !! array of rights sons has to be finished !!
        /// </summary>
        private void FillParents()
        {
            GoThru_NoteParent(0, 0);
            parents[0] = unvalidValue;
        }

        private void GoThru_NoteParent(int myIndex, int parent)
        {

            parents[myIndex] = parent;

            if (currTree[myIndex] == Tree.Nodes.nil)
                return;

            GoThru_NoteParent(myIndex + 1, myIndex);
            GoThru_NoteParent(rightSons[myIndex], myIndex);
        }
        #endregion
        #endregion

        //hyperCubeNeighbors methods
        #region HyperCube Neighbors

        /// <summary>
        /// Get neighbor of hyperNode in hyperCube
        /// </summary>
        /// <param name="hyperNode">Node of hyper cube</param>
        /// <param name="bitToChange">index of bit to change</param>
        /// <returns></returns>
        private int GetNeighbor(int hyperNode, int bitToChange)
            => hyperNode ^ neighborsInHyperCube[bitToChange];

        /// <summary>
        /// Calculates next hyper node, based on parent of node on "treeNode"
        /// </summary>
        /// <param name="treeNode">node identificator</param>
        /// <param name="bitToChange"></param>
        /// <returns>Next hyper node</returns>
        private int GetNextHyperNode(int treeNode, int bitToChange)
            => currMatching[parents[treeNode]] ^ neighborsInHyperCube[bitToChange];

        private bool HyperNodeIsFull(int hyperNode) => hyperCube[hyperNode] == HyperCubeNodes.full;

        private bool HasValidNeigbor(int treeNode) => nextNeighbors[treeNode] < firstUnvalidNeighbor;

#if SaveProgramming
        private void AllocateHyperNode(int hyperNode)
        {
            if (hyperCube[hyperNode] == HyperCubeNodes.full)
                throw new MulitipleAllocationOrDeallocationException();

            hyperCube[hyperNode] = HyperCubeNodes.full;
        }
        private void DeallocateHyperNode(int hyperNode)
        {
            if (hyperCube[hyperNode] == HyperCubeNodes.empty)
                throw new MulitipleAllocationOrDeallocationException();

            hyperCube[hyperNode] = HyperCubeNodes.empty;
        }
#else
        private void AllocateHyperNode(int hyperNode) => hyperCube[hyperNode] = HyperCubeNodes.full;
        private void DeallocateHyperNode(int hyperNode) => hyperCube[hyperNode] = HyperCubeNodes.empty;
#endif
        #endregion

        //main alghorithm
        #region Main Alghoritm

        public bool TryFitTree(Tree tree)
        {
            PrepareNewWorkSpaceFor(tree);

            try
            {
                FitTree();
            }
            catch(IndexOutOfRangeException)
            {
                if (currNode <= 0)
                    return false;
                else
                    throw;
            }


            return true;
        }

        private void FitTree()
        {
            while (currNode < NumOfNodes)
            {
                if (!HasValidNeigbor(currNode))
                {
                    RollBack();
                    continue;
                }

                do
                {
                    int nextHyperNode = GetNextHyperNode(currNode, nextNeighbors[currNode]);

                    if (HyperNodeIsFull(nextHyperNode))
                        nextNeighbors[currNode]++;
                    else
                    {
                        TakeHyperNode(nextHyperNode);
                        break;
                    }
                }
                while (HasValidNeigbor(currNode));
            }
        }

        private void PrepareNewWorkSpaceFor(Tree tree)
        {
            if (tree.FirstChangedOne <= 1) //initial tree
            {
                PrepareFirstRun(tree.binaryRepr);
                return;
            }

            //recycling old maching 

            RecycleArraysFor(tree.binaryRepr);
            nextNeighbors[1] = 0; //important - preventing cumulation of "unused heuristics"
            currNode--; //from last succesful searching currNode is out of range

            while (currNode >= tree.FirstChangedOne)
                RollBack();
        }

        private void PrepareFirstRun(Tree.Nodes[] tree)
        {
            PrepareArraysFor(tree);
            ClearHyperCube();

            //allocating first node
            AllocateHyperNode(0);
            currMatching[0] = 0;
            currNode = 1;
        }

        private void TakeHyperNode(int hyperNode)
        {
            if (currMatching[currNode] != unvalidValue)
                DeallocateHyperNode(currMatching[currNode]);

            AllocateHyperNode(hyperNode);
            nextNeighbors[currNode]++;
            currMatching[currNode] = hyperNode;
            currNode++;
        }

        /// <summary>
        /// Moves currNode to "indexOfNode" and restarts every nodes after node on "indexOfNode"
        /// </summary>
        private void RollBack()
        {
            if (currMatching[currNode] != unvalidValue) //it is possible that first node will have hyperNode
            {                                           //than dealocate that hyper node
                DeallocateHyperNode(currMatching[currNode]);
                currMatching[currNode] = unvalidValue;
            }

            nextNeighbors[currNode] = defaultNeighbor;
            currNode--;

            DeallocateHyperNode(currMatching[currNode]);
            currMatching[currNode] = unvalidValue;
        }

        #endregion
    }

    class MulitipleAllocationOrDeallocationException : Exception { }
}
