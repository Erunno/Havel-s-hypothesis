using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Trees;
using Validator;
using System.Diagnostics;

namespace Havel_s_hypothesis
{
    class ThreadManager
    {
        public ThreadManager(int dimension)
        {
            allThreadsHandlers = new HandlerThread[Environment.ProcessorCount];
            for (int i = 0; i < allThreadsHandlers.Length; i++)
                allThreadsHandlers[i] = new HandlerThread();

            this.dimension = dimension;
            trees = new TreeList();
            treeEtor = trees.GetEnumerator();

            sw = new Stopwatch();
            AllTrees = TreeCounter.NumOfTrees(1 << dimension);
        }

        class HandlerThread
        {
            public OneCoreHandler handler;
            public Thread thread;
        }

        int dimension;
        HandlerThread[] allThreadsHandlers;

        TreeList trees;
        IEnumerator<string> treeEtor;

        int sleepInterval = 10 * 1000; // 10 sec
        Stopwatch sw;

        public void RunAllTest()
        {
            sw.Restart();

            InitThreads();

            do
            {
                Thread.Sleep(sleepInterval);

                for (int i = 0; i < allThreadsHandlers.Length; i++)
                    CheckThread_IfItIsDoneRunNewOne(i);

                if (oddTreeHasBeenFound)
                {
                    ShowOddTree();
                    return;
                }
            }
            while (!AllTreesDone || SomeoneIsRunning());

            ShowEnd();
            sw.Stop();
        }

        private bool SomeoneIsRunning()
        {
            for (int i = 0; i < allThreadsHandlers.Length; i++)
                if (allThreadsHandlers[i].thread.ThreadState == System.Threading.ThreadState.Running)
                    return true;

            return false;
        }

        private void InitThreads()
        {
            for (int i = 0; i < allThreadsHandlers.Length; i++)
                CreateAndRunNextThread(i);
        }

        private void CheckThread_IfItIsDoneRunNewOne(int index)
        {
            if (allThreadsHandlers[index].thread.ThreadState == System.Threading.ThreadState.Running) //thread is not done with its job
                return;

            if (!allThreadsHandlers[index].handler.AllTreesWereCorrect)
            {
                oddTreeHasBeenFound = true;
                oddTree = allThreadsHandlers[index].handler.baseTree;
                return;
            }

            Log(allThreadsHandlers[index].handler.baseTree);

            CreateAndRunNextThread(index);
        }

        bool AllTreesDone = false;
        bool oddTreeHasBeenFound = false;
        Tree oddTree = null;

        private void CreateAndRunNextThread(int indexInArray)
        {
            if (AllTreesDone = !treeEtor.MoveNext()) //done
                return;

            OneCoreHandler handler = new OneCoreHandler(dimension);
            handler.amountOfChackedTrees = trees.intervalSize; //+1 just for sure
            handler.baseTree = new Tree(treeEtor.Current);

            allThreadsHandlers[indexInArray].thread = new Thread(handler.Run);
            allThreadsHandlers[indexInArray].handler = handler;

            allThreadsHandlers[indexInArray].thread.Start();
        }

        long treesDone = 40;
        long balTreesDone = 0;

        long AllTrees { get; }
        const int lenghtOfLoadingBar = 70;

        private void Log(Tree tree)
        {
            balTreesDone += trees.intervalSize;
            treesDone += trees.intervalSize + tree.UnbalancedTrees;

            Console.Clear();

            Console.WriteLine("Statistics:");
            Console.WriteLine($"TreesDone: {treesDone}, (balanced: {balTreesDone}), AllTrees: {AllTrees}, Elapsed: {sw.Elapsed}");
            ShowLoadingBar();
        }

        private void ShowLoadingBar()
        {
            double percentage = (lenghtOfLoadingBar * treesDone) / (double)AllTrees;
            int doneBars = (int) ((lenghtOfLoadingBar * treesDone) / AllTrees);
            doneBars = doneBars > lenghtOfLoadingBar ? lenghtOfLoadingBar : doneBars;

            Console.Write("[");
            Console.Write((new StringBuilder().Append('=', doneBars)).ToString());
            Console.Write((new StringBuilder().Append(' ', lenghtOfLoadingBar - doneBars)).ToString());
            Console.WriteLine($"] {percentage} %");
        }

        public void ShowOddTree()
        {
            Console.WriteLine("Odd tree found !!!!!!!!!");
            Console.WriteLine(oddTree);
        }

        public void ShowEnd()
        {
            Console.WriteLine("**DONE**");
        }
    }
}
