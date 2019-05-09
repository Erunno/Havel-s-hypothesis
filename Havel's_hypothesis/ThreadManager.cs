using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Trees;
using Validator;
using System.Diagnostics;
using System.Net.Mail;

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

        private EmailLogger _emailLogger;
        public EmailLogger emailLogger
        {
            get => _emailLogger;
            set
            {
                _emailLogger = value;
                _emailLogger.manager = this;
            }
        }


        int sleepInterval = 10 * 1000; // 10 sec
        Stopwatch sw;

        public void RunAllTest()
        {
            sw.Restart();
            InitThreads();
            _emailLogger?.InitLog();

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

        long treesDone = 0;
        long balTreesDone = 0;

        long AllTrees { get; }
        const int lenghtOfLoadingBar = 50;

        private void Log(Tree tree)
        {
            balTreesDone += trees.intervalSize;
            treesDone += trees.intervalSize + tree.UnbalancedTrees;

            Console.Clear();

            Console.WriteLine(CreateConsoleMessage());

            _emailLogger?.Log();
        }

        public void ShowOddTree()
        {
            Console.WriteLine("Odd tree found !!!!!!!!!");
            Console.WriteLine(oddTree);

            _emailLogger?.OddTreeLog(oddTree);
        }

        public void ShowEnd()
        {
            Console.WriteLine("**DONE**");
            _emailLogger?.SuccessfulRunLog();
        }


        private string CreateConsoleMessage()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Statistics:");
            sb.AppendLine();

            sb.AppendLine($"TreesDone: {treesDone:N0}, (balanced: {balTreesDone:N0}), AllTrees: {AllTrees:N0}");

            //loading bar
            double percentage = GetPercentage();

            int doneBars = (int)((lenghtOfLoadingBar * treesDone) / AllTrees);
            doneBars = doneBars > lenghtOfLoadingBar ? lenghtOfLoadingBar : doneBars;

            var timeToEnd = (sw.Elapsed / (percentage / 100)) - sw.Elapsed;
            sb.AppendLine($"Elapsed: {sw.Elapsed}, Estimated time to end: {timeToEnd} (DateTime: {DateTime.Now + timeToEnd})");

            sb.AppendLine();
            sb.Append("[");
            sb.Append((new StringBuilder().Append('=', doneBars)).ToString());
            sb.Append((new StringBuilder().Append(' ', lenghtOfLoadingBar - doneBars)).ToString());
            sb.AppendLine($"] {percentage} %");

            return sb.ToString();
        }

        private double GetPercentage()
        {
            double percentage = (treesDone * 100) / (double)AllTrees;
            percentage = percentage > 100 ? 100 : percentage;
            return percentage;
        }

        public class EmailLogger
        {
            MailMessage mail;
            SmtpClient SmtpServer;
            public ThreadManager manager;
            private int intervalOfSendingMail_percentage;
            private int nextMail_percentage;

            public EmailLogger(int iterval_percentage)
            {
                intervalOfSendingMail_percentage = iterval_percentage;
                nextMail_percentage = intervalOfSendingMail_percentage;

                mail = new MailMessage();

                SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress("c.sharp.myapps@gmail.com");
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("c.sharp.myapps@gmail.com", "Kakanec001");
                SmtpServer.EnableSsl = true;
            }


            public void Log()
            {
                if (nextMail_percentage < manager.GetPercentage())
                {
                    nextMail_percentage += intervalOfSendingMail_percentage;
                    SendProgressMail();
                }
            }

            private void SendProgressMail()
            {
                mail.Subject = $"{Environment.MachineName} - progress: {manager.GetPercentage():N2} %";
                mail.Body = manager.CreateConsoleMessage();

                try
                {
                    SmtpServer.Send(mail);
                    Console.WriteLine("Email sent");
                }
                catch
                {
                    Console.WriteLine("Unable to send email");
                }
            }

            public void AddReceiver(string receiver) => mail.To.Add(receiver);

            public void InitLog()
            {
                mail.Subject = $"{Environment.MachineName} - Start";
                mail.Body = $"Program has started\nDimension: {manager.dimension}";

                try
                {
                    SmtpServer.Send(mail);
                    Console.WriteLine("Email sent");
                }
                catch
                {
                    Console.WriteLine("Unable to send email");
                }
            }

            public void SuccessfulRunLog()
            {
                mail.Subject = $"{Environment.MachineName} - Start";
                mail.Body = manager.CreateConsoleMessage();
                mail.Body += $"\nHypothesis has been verified for dimension {manager.dimension}";

                try
                {
                    SmtpServer.Send(mail);
                    Console.WriteLine("Email sent");
                }
                catch
                {
                    Console.WriteLine("Unable to send email");
                }
            }

            public void OddTreeLog(Tree tree)
            {
                mail.Subject = $"{Environment.MachineName} - Odd tree";
                mail.Body = $"Odd tree has been found:\n{tree}";

                try
                {
                    SmtpServer.Send(mail);
                    Console.WriteLine("Email sent");
                }
                catch
                {
                    Console.WriteLine("Unable to send email");
                }
            }
        }

    }
}
