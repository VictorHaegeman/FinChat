// =====================================================================
//  FinanceNews.cs  (project ChatServer)
//
//  Role   : thread of the server that gets the latest finance article
//           from the Finnhub API and sends it in #finance, every hour.
//           The article goes through the SAME broadcast() as the users'
//           messages : FinanceBot is just another sender.
//
//  Course : [COURS 4 - Threads]   own thread, Thread.Sleep, Abort
//           [COURS 5 - IO]        the API key is read from a text file
//           [COURS 2 - Exceptions] try / catch if the API does not answer
//           [HORS COURS]          HTTP request (WebClient) and JSON reading
//                                 (DataContractJsonSerializer) : only here
//
//  Setup  : create a file finnhub_key.txt containing your key (one line)
//           in the folder where you start the server (FinChat).
//           This file is in .gitignore : the key never goes on GitHub.
// =====================================================================

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Communication;

namespace ChatServer
{
    // one article of the Finnhub API (only the fields we use)
    // [HORS COURS] [DataContract] / [DataMember] : same idea as [Serializable]
    // in the course, but to read JSON. The fields are filled by the
    // DataContractJsonSerializer (= null only to tell the compiler it is normal)
    [DataContract]
    public class Article
    {
        [DataMember(Name = "headline")]
        private string headline = null;

        [DataMember(Name = "source")]
        private string source = null;

        [DataMember(Name = "url")]
        private string url = null;

        public string Headline
        {
            get { return headline; }
        }

        public string Source
        {
            get { return source; }
        }

        public string Url
        {
            get { return url; }
        }
    }

    public class FinanceNews
    {
        private const string KEY_FILE = "finnhub_key.txt";
        private const string URL = "https://finnhub.io/api/v1/news?category=general&token=";

        // 3 600 000 ms = 1 hour (put 60000 = 1 minute for the demo)
        private const int INTERVAL = 3600000;

        private Server server;
        private Thread thread;
        private string apiKey = null;
        private string lastUrl = "";   // to avoid sending the same article twice

        public FinanceNews(Server s)
        {
            server = s;
        }

        public void start()
        {
            // [COURS 4 - Threads]
            thread = new Thread(new ThreadStart(run));
            thread.Start();
        }

        public void stop()
        {
            thread.Abort();   // [COURS 4 - Threads] "Destroying a thread"
        }

        // method run by the thread
        public void run()
        {
            try
            {
                apiKey = readKey();
            }
            catch (Exception)
            {
                Logger.log("finance : no API key, create the file " + KEY_FILE + " (#finance disabled)");
                return;   // the thread ends, the chat works without #finance
            }

            while (true)
            {
                if (server.Running)
                {
                    sendLastArticle();
                }
                Thread.Sleep(INTERVAL);
            }
        }

        // [COURS 5 - IO] reads the first line of finnhub_key.txt
        private string readKey()
        {
            using (StreamReader sr = new StreamReader(KEY_FILE))
            {
                return sr.ReadLine().Trim();
            }
        }

        private void sendLastArticle()
        {
            try   // [COURS 2 - Exceptions]
            {
                Article a = getLastArticle();
                if (a != null && a.Url != lastUrl)
                {
                    lastUrl = a.Url;
                    string text = a.Headline + " (" + a.Source + ") " + a.Url;

                    ChatMsg news = new ChatMsg("FinanceBot", "finance", text);
                    server.LastNews = news;
                    server.broadcast(news);   // same event as the users' messages
                    Logger.log("finance article sent : " + a.Headline);
                }
            }
            catch (ThreadAbortException)
            {
                throw;   // stop() was called
            }
            catch (Exception e)
            {
                // no internet, wrong key, API down...
                Logger.log("finance API error : " + e.Message);
            }
        }

        // [HORS COURS] HTTP request + reading of the JSON answer (a list of articles)
        private Article getLastArticle()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;   // HTTPS

            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            string json = web.DownloadString(URL + apiKey);

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Article[]));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            Article[] articles = (Article[])ser.ReadObject(ms);
            ms.Close();

            if (articles.Length == 0)
            {
                return null;
            }
            return articles[0];   // the most recent article
        }
    }
}
