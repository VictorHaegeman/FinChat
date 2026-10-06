using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Communication;

namespace ChatServer
{
    public class Server
    {
        private int port;
        private TcpListener listener;
        private bool running = false;

        // list of the connected clients, shared by all the threads
        private List<Receiver> clients = new List<Receiver>();
        private static object access_grant = new object();

        // last article sent in #finance (given to the users who join #finance)
        private ChatMsg lastNews = null;

        // the event : every logged-in Receiver subscribes to it
        public event MessageDelegate BroadcastEvent;

        public Server(int port)
        {
            this.port = port;
        }

        public bool Running
        {
            get { return running; }
        }

        public ChatMsg LastNews
        {
            get { return lastNews; }
            set { lastNews = value; }
        }

        // ==================== start / stop ====================

        public void start()
        {
            if (running)
            {
                Logger.log("server already started");
                return;
            }

            // 127.0.0.1 like in the skeleton (only this computer)
            // use IPAddress.Any to accept clients from other computers
            listener = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
            listener.Start();
            running = true;

            new Thread(new ThreadStart(listen)).Start();
            Logger.log("server started on port " + port);
        }

        public void stop()
        {
            if (!running)
            {
                return;
            }
            running = false;
            listener.Stop();   // AcceptTcpClient() stops with a SocketException

            // disconnect all the clients (copy of the list : close() removes them)
            Receiver[] copy;
            lock (access_grant)
            {
                copy = clients.ToArray();
            }
            foreach (Receiver r in copy)
            {
                r.close("the server has been stopped");
            }

            Logger.log("server stopped");
        }

        // thread that waits for the connections (like start() in the skeleton)
        private void listen()
        {
            try
            {
                while (running)
                {
                    TcpClient comm = listener.AcceptTcpClient();   // blocking
                    Logger.log("connection established @" + comm.Client.RemoteEndPoint);

                    Receiver r = new Receiver(comm, this);
                    addClient(r);
                    new Thread(new ThreadStart(r.doOperation)).Start();   // 1 thread per client
                }
            }
            catch (Exception)
            {
                // normal when stop() is called : listener.Stop() unblocks AcceptTcpClient()
            }
        }

        // ==================== list of the clients (lock) ====================

        public void addClient(Receiver r)
        {
            lock (access_grant)
            {
                clients.Add(r);
            }
        }

        public void removeClient(Receiver r)
        {
            lock (access_grant)
            {
                clients.Remove(r);
            }
        }

        // gives the name to r if nobody else has it (check + change in the same lock)
        public bool reserveName(Receiver r, string name)
        {
            if (name.ToLower() == "financebot")
            {
                return false;
            }

            lock (access_grant)
            {
                foreach (Receiver other in clients)
                {
                    if (other != r && other.Username != null && other.Username.ToLower() == name.ToLower())
                    {
                        return false;
                    }
                }
                r.Username = name;
                return true;
            }
        }

        public string[] getUsers()
        {
            List<string> names = new List<string>();
            lock (access_grant)
            {
                foreach (Receiver r in clients)
                {
                    if (r.Username != null)
                    {
                        names.Add(r.Username);
                    }
                }
            }
            return names.ToArray();
        }

        public Receiver findClient(string name)
        {
            lock (access_grant)
            {
                foreach (Receiver r in clients)
                {
                    if (r.Username != null && r.Username.ToLower() == name.ToLower())
                    {
                        return r;
                    }
                }
            }
            return null;
        }

        // ==================== sending ====================

        // protected virtual method raising the event (Delegates course / Lab 2)
        protected virtual void onBroadcast(object sender, MessageEventArgs e)
        {
            MessageDelegate subscribers = BroadcastEvent;   // copy : a client may unsubscribe at the same time
            if (subscribers != null)                        // check for subscribers
            {
                subscribers(sender, e);                     // every Receiver.onBroadcast is called
            }
        }

        // message for everybody (each Receiver filters by channel)
        public void broadcast(Message msg)
        {
            onBroadcast(this, new MessageEventArgs(msg));
        }

        public void sendUserList()
        {
            broadcast(new UserListMsg(getUsers()));
        }

        // message for one user only
        public void sendPrivate(PrivateMsg msg, Receiver from)
        {
            Receiver target = findClient(msg.Target);
            if (target == null)
            {
                from.send(new InfoMsg("user " + msg.Target + " not found"));
                return;
            }

            target.send(msg);
            if (target != from)
            {
                from.send(msg);   // copy for the sender, to see it in his window
            }
            Logger.log("private message " + msg.Sender + " -> " + msg.Target);
        }
    }
}
