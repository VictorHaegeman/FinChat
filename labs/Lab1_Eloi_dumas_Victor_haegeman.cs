using System;
using System.Threading;

namespace Lab1
{
    // exercice 1
    class X
    {
        private static int _count = 0;
        private int _num;

        public X()
        {
            _count++;
            _num = _count;
        }

        public static int Count
        {
            get { return _count; }
        }

        public override string ToString()
        {
            return "Object from class X (number " + _num + ")";
        }
    }

    // exercice 2 a)
    class Positive
    {
        private long _value = 1;

        public long Value
        {
            get { return _value; }
            set
            {
                if (value > 0) _value = value;
                else Console.WriteLine("value must be strictly positive");
            }
        }
    }

    // exercice 2 b)
    class Range
    {
        private long _min;
        private long _max;

        public Range()
        {
            _min = 0;
            _max = 100;
        }

        public long Min
        {
            get { return _min; }
            set
            {
                if (value <= _max) _min = value;
                else Console.WriteLine("min must be <= max");
            }
        }

        public long Max
        {
            get { return _max; }
            set
            {
                if (value >= _min) _max = value;
                else Console.WriteLine("max must be >= min");
            }
        }
    }

    // exercice 3
    abstract class instrument
    {
        public instrument()
        {
            Console.WriteLine("instrument constructor");
        }

        public abstract void play();
    }

    abstract class brass : instrument
    {
        public brass()
        {
            Console.WriteLine("brass constructor");
        }

        // protected : not callable from Main
        protected virtual void blow()
        {
            Console.WriteLine("brass.blow()");
        }
    }

    class trumpet : brass
    {
        public trumpet()
        {
            Console.WriteLine("trumpet constructor");
        }

        public override void play()
        {
            Console.WriteLine("trumpet.play()");
            blow();
        }

        protected override void blow()
        {
            Console.WriteLine("trumpet.blow()");
        }
    }

    // exercice 4
    class Letter
    {
        private char _c;

        public Letter(char c)
        {
            _c = c;
        }

        public void Run()
        {
            while (true)
            {
                Console.Write(_c);
                Thread.Sleep(500);
            }
        }
    }

    class Program
    {
        static void Ex1()
        {
            X obj1, obj2;

            obj1 = new X();
            obj2 = new X();

            Console.WriteLine(obj1);
            Console.WriteLine(obj2);
        }

        static void Ex2()
        {
            Positive p = new Positive();
            p.Value = 12;
            Console.WriteLine(p.Value);
            p.Value = -3;
            Console.WriteLine(p.Value);

            Range r = new Range();
            r.Min = 50;
            r.Max = 20;
            Console.WriteLine(r.Min + " " + r.Max);
        }

        static void Ex3()
        {
            new trumpet().play();
        }

        static void Ex4()
        {
            for (int i = 0; i < 5; i++)
            {
                Letter l = new Letter((char)('A' + i));
                Thread t = new Thread(new ThreadStart(l.Run));
                t.IsBackground = true;
                t.Start();
            }
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            Ex1();
            Ex2();
            Ex3();
            Ex4();
        }
    }
}
