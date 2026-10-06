using System;
using System.IO;

namespace ChatServer
{
    // server activity : displayed in the console AND written in server_log.txt
    public class Logger
    {
        private const string fileName = "server_log.txt";

        // several threads can log at the same time : static lock object (Lab 4)
        private static object access_grant = new object();

        public static void log(string text)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + text;

            lock (access_grant)
            {
                Console.WriteLine(line);

                try
                {
                    // true = append at the end of the file (IO course : StreamWriter)
                    using (StreamWriter sw = new StreamWriter(fileName, true))
                    {
                        sw.WriteLine(line);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("cannot write the log file : " + e.Message);
                }
            }
        }
    }
}
