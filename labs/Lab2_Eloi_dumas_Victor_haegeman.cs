using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Lab2
{
    // exercice 1
    public class PrimeEventArgs : System.EventArgs
    {
        private long _prime;

        public long Prime
        {
            get { return _prime; }
        }

        public PrimeEventArgs(long p) : base()
        {
            _prime = p;
        }
    }

    public delegate void PrimeEventHandler(object sender, PrimeEventArgs pe);

    public class PrimeSender
    {
        public event PrimeEventHandler PrimeFound;

        protected virtual void OnPrimeFound(long p)
        {
            if (PrimeFound != null) PrimeFound(this, new PrimeEventArgs(p));
        }

        public void CalculatePrimes(long limit)
        {
            for (long n = 2; n <= limit; n++)
            {
                bool prime = true;
                for (long d = 2; d * d <= n; d++)
                {
                    if (n % d == 0) { prime = false; break; }
                }
                if (prime) OnPrimeFound(n);
            }
        }
    }

    public class PrimeReceiver
    {
        public PrimeReceiver(PrimeSender s)
        {
            s.PrimeFound += new PrimeEventHandler(action);
        }

        public void action(object sender, PrimeEventArgs pe)
        {
            Console.WriteLine("prime found : " + pe.Prime);
        }
    }

    // exercice 2
    // writing directly in the textBox from a thread throws InvalidOperationException :
    // a control can only be modified by the thread that created it.
    // solution : Invoke + a delegate, so the write is done by the UI thread.
    public class MainForm : Form
    {
        private TextBox _box;
        private Button _start;
        private List<Thread> _threads = new List<Thread>();

        private delegate void WriteDelegate(char c);

        public MainForm()
        {
            _box = new TextBox();
            _box.Multiline = true;
            _box.Dock = DockStyle.Fill;

            _start = new Button();
            _start.Text = "start";
            _start.Dock = DockStyle.Bottom;
            _start.Click += new EventHandler(Start_Click);

            Controls.Add(_box);
            Controls.Add(_start);
        }

        private void Write(char c)
        {
            if (_box.InvokeRequired)
                _box.Invoke(new WriteDelegate(Write), new object[] { c });
            else
                _box.AppendText(c.ToString());
        }

        private void Loop(object letter)
        {
            char c = (char)letter;
            while (true)
            {
                Write(c);
                Thread.Sleep(500);
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(Loop));
                t.IsBackground = true;
                t.Start((char)('A' + i));
                _threads.Add(t);
            }
        }
    }

    // exercice 3
    public class systemResource
    {
        private string _name;
        private object _lock = new object();

        public systemResource(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool Take(string who)
        {
            if (!Monitor.TryEnter(_lock))
            {
                Console.WriteLine(who + " : " + _name + " not available, waiting");
                Monitor.Enter(_lock);
            }
            Console.WriteLine(who + " takes " + _name);
            return true;
        }

        public void Release(string who)
        {
            Console.WriteLine(who + " releases " + _name);
            Monitor.Exit(_lock);
        }
    }

    public class askResource
    {
        private string _name;
        private List<systemResource> _resources = new List<systemResource>();
        private List<int> _times = new List<int>();

        public askResource(string name)
        {
            _name = name;
        }

        public void ask(systemResource r, int seconds)
        {
            _resources.Add(r);
            _times.Add(seconds);
        }

        public void run()
        {
            for (int i = 0; i < _resources.Count; i++)
            {
                systemResource r = _resources[i];
                r.Take(_name);
                Thread.Sleep(_times[i] * 1000);
                r.Release(_name);
            }
            Console.WriteLine(_name + " finished");
        }
    }

    class Program
    {
        static void Ex1()
        {
            PrimeSender s = new PrimeSender();
            new PrimeReceiver(s);
            s.CalculatePrimes(100);
        }

        static void Ex2()
        {
            Application.Run(new MainForm());
        }

        static void Ex3()
        {
            systemResource disk = new systemResource("disk");
            systemResource ram = new systemResource("ram");
            systemResource printer = new systemResource("printer");

            askResource p1 = new askResource("P1");
            p1.ask(disk, 5);
            p1.ask(ram, 10);
            p1.ask(printer, 3);

            askResource p2 = new askResource("P2");
            p2.ask(ram, 4);
            p2.ask(disk, 2);

            askResource p3 = new askResource("P3");
            p3.ask(printer, 6);
            p3.ask(ram, 2);

            Thread t1 = new Thread(new ThreadStart(p1.run));
            Thread t2 = new Thread(new ThreadStart(p2.run));
            Thread t3 = new Thread(new ThreadStart(p3.run));

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();
        }

        [STAThread]
        static void Main(string[] args)
        {
            Ex1();
            Ex3();
            Ex2();
        }
    }
}
