using System;
using System.Threading;

namespace Lab4
{
    // exercice 1
    class Race
    {
        private int i = 5;
        private Semaphore sem = new Semaphore(1, 1);
        private int loops = 1000;

        // 1) no protection : i++ and i-- are not atomic (read, modify, write),
        // so the final value is not always 5.
        public void Inc()
        {
            for (int k = 0; k < loops; k++) i++;
        }

        public void Dec()
        {
            for (int k = 0; k < loops; k++) i--;
        }

        // 2) with a register and a sleep, the interleaving is forced :
        // T1 reads i=5 into Reg, T2 reads i=5 into Reg too, then both write back.
        // if T1 writes last -> 6, if T2 writes last -> 4. the second write
        // erases the first one, so one of the two updates is lost.
        public void IncReg()
        {
            int Reg = i;
            Thread.Sleep(100);
            Reg++;
            i = Reg;
        }

        public void DecReg()
        {
            int Reg = i;
            Thread.Sleep(100);
            Reg--;
            i = Reg;
        }

        // 3) mutual exclusion : the critical section is protected by a semaphore
        // initialized to 1, so only one thread can read/modify/write i at a time.
        public void IncSafe()
        {
            for (int k = 0; k < loops; k++)
            {
                sem.WaitOne();
                int Reg = i;
                Reg++;
                i = Reg;
                sem.Release();
            }
        }

        public void DecSafe()
        {
            for (int k = 0; k < loops; k++)
            {
                sem.WaitOne();
                int Reg = i;
                Reg--;
                i = Reg;
                sem.Release();
            }
        }

        public int Value
        {
            get { return i; }
        }

        public void Run(ThreadStart f1, ThreadStart f2)
        {
            i = 5;
            Thread t1 = new Thread(f1);
            Thread t2 = new Thread(f2);
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Console.WriteLine("i = " + i);
        }
    }

    // exercice 2 : (a+b)*(c-d)*(e+f)
    class Calc
    {
        private double[] table = new double[3];
        private int next = 0;
        private Semaphore spot = new Semaphore(1, 1);   // protects the table
        private Semaphore done = new Semaphore(0, 3);   // counts finished tasks

        private double a = 2, b = 3, c = 10, d = 4, e = 1, f = 5;

        private void Store(double v)
        {
            spot.WaitOne();
            table[next] = v;
            next++;
            spot.Release();
            done.Release();
        }

        public void T1() { Store(a + b); }
        public void T2() { Store(c - d); }
        public void T3() { Store(e + f); }

        public void T4()
        {
            // wait for two tasks
            done.WaitOne();
            done.WaitOne();

            spot.WaitOne();
            double partial = table[0] * table[1];
            spot.Release();
            Console.WriteLine("partial = " + partial);

            // wait for the last one
            done.WaitOne();

            spot.WaitOne();
            double result = partial * table[2];
            spot.Release();
            Console.WriteLine("result = " + result);
        }

        public void Run()
        {
            Thread t4 = new Thread(new ThreadStart(T4));
            t4.Start();

            new Thread(new ThreadStart(T1)).Start();
            new Thread(new ThreadStart(T2)).Start();
            new Thread(new ThreadStart(T3)).Start();

            t4.Join();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Race r = new Race();

            Console.WriteLine("1) without synchronization :");
            r.Run(new ThreadStart(r.Inc), new ThreadStart(r.Dec));

            Console.WriteLine("2) with a register and a sleep :");
            r.Run(new ThreadStart(r.IncReg), new ThreadStart(r.DecReg));

            Console.WriteLine("3) with a semaphore :");
            r.Run(new ThreadStart(r.IncSafe), new ThreadStart(r.DecSafe));

            Console.WriteLine("exercice 2 :");
            new Calc().Run();
        }
    }
}
