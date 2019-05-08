#define CheckingCorrectness

using System;
using System.Collections.Generic;
using System.Text;
using Trees;
using Validator;
using MachingCheck;

namespace Havel_s_hypothesis
{
    class OneCoreHandler
    {
        //"arguments"
        public Tree baseTree;
        public int amountOfChackedTrees;

        //result
        public bool AllTreesWereCorrect;

        public OneCoreHandler(int dimension)
        {
            nester = new Nester(dimension);
#if CheckingCorrectness
            checker = new MachingChecker(dimension);
#endif
        }

        private Nester nester;
#if CheckingCorrectness
        private MachingChecker checker;
#endif

        public void Run()
        {
            AllTreesWereCorrect = true;

            for (int i = 0; i < amountOfChackedTrees; i++)
            {
                if(!nester.TryFitTree(baseTree))
                {
                    AllTreesWereCorrect = false;
                    return;
                }

#if CheckingCorrectness
                checker.IsCorrectMaching(baseTree, nester.GetYourMaching());
#endif

                if (!baseTree.NextBalancedTree())
                    return;
            }
        }



    }
}
