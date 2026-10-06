using System;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server serv = new Server(8976);
            FinanceNews news = new FinanceNews(serv);

            Console.WriteLine("=== FinChat server ===");
            Console.WriteLine("commands : start, stop, exit");

            serv.start();
            news.start();

            // the main thread reads the commands typed in the console
            while (true)
            {
                string cmd = Console.ReadLine();
                if (cmd == null)
                {
                    break;
                }
                cmd = cmd.Trim().ToLower();

                if (cmd == "start")
                {
                    serv.start();
                }
                else if (cmd == "stop")
                {
                    serv.stop();
                }
                else if (cmd == "exit")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("unknown command (start, stop, exit)");
                }
            }

            serv.stop();
            news.stop();
        }
    }
}
