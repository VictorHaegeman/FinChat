// =====================================================================
//  Logger.cs  (project ChatServer)
//
//  Role   : server activity (connections, disconnections, messages,
//           errors) displayed in the console AND written in server_log.txt
//           (project : "Log server activity")
//
//  Course : [COURS 5 - IO]     StreamWriter
//           [LAB 4 - lock]     several threads can log at the same time
//           [COURS 1 - Basics] static method and static attribute
// =====================================================================

using System;
using System.IO;

namespace ChatServer
{
    public class Logger
    {
        private const string fileName = "server_log.txt";

        // [LAB 4 - lock] static : the SAME lock object for all the threads
        private static object access_grant = new object();

        public static void log(string text)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + text;

            lock (access_grant)
            {
                Console.WriteLine(line);

                try
                {
                    // [COURS 5 - IO] true = append at the end of the file
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
