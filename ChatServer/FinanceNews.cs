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
    // [DataContract] / [DataMember] : same idea as [Serializable] in the course,
    // but to read JSON. The fields are filled by the DataContractJsonSerializer
    // (= null only to tell the compiler that it is normal)
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

    // thread of the server that sends a finance article in #finance every hour
    public class FinanceNews
    {
        // free key : create an account on https://finnhub.io
        private const string API_KEY = "PUT_YOUR_FINNHUB_KEY_HERE";
        private const string URL = "https://finnhub.io/api/v1/news?category=general&token=";

        // 3 600 000 ms = 1 hour (put 60000 = 1 minute for the demo)
        private const int INTERVAL = 3600000;

        private Server server;
        private Thread thread;
        private string lastUrl = "";   // to avoid sending the same article twice

        public FinanceNews(Server s)
        {
            server = s;
        }

        public void start()
        {
            thread = new Thread(new ThreadStart(run));
            thread.Start();
        }

        public void stop()
        {
            thread.Abort();   // Threads course : "Destroying a thread"
        }

        public void run()
        {
            while (true)
            {
                if (server.Running)
                {
                    try
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

                Thread.Sleep(INTERVAL);
            }
        }

        // HTTP request + reading of the JSON answer (a list of articles)
        private Article getLastArticle()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;   // HTTPS

            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            string json = web.DownloadString(URL + API_KEY);

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
