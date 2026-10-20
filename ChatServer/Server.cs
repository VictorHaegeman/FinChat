// =====================================================================
//  Server.cs  (project ChatServer)
//
//  Role   : - start / stop the TCP server
//           - accept the clients (1 Receiver + 1 thread per client)
//           - keep the list of the connected clients
//           - BROADCAST the messages with an event (delegates)
//           - send the private messages
//
//  Course : [COURS 4 - Network]     TcpListener, AcceptTcpClient
//           [COURS 4 - Threads]     listening thread, 1 thread per client
//           [COURS 3 - Delegates]   event BroadcastEvent + onBroadcast
//           [COURS 8 - Collections] List<Receiver>
//           [LAB 4 - lock]          the list is shared by all the threads
//
//  Map of the file :
//      #region Attributes
//      #region Start / stop          start(), stop(), listen()
//      #region List of the clients   addClient, removeClient, reserveName, getUsers, findClient
//      #region Broadcast (event)     onBroadcast(), broadcast(), sendUserList()
//      #region Private messages      sendPrivate()
// =====================================================================

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
        #region Attributes

        private int port;
        private TcpListener listener;
        private bool running = false;

        // [COURS 8 - Collections] the connected clients, shared by all the threads
        private List<Receiver> clients = new List<Receiver>();

        // [LAB 4 - lock] static object used to lock the list
        private static object access_grant = new object();

        // last article sent in #finance (given to the users who join #finance)
        private ChatMsg lastNews = null;

        // [COURS 3 - Delegates] the event : every logged-in Receiver subscribes to it (+=)
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

        #endregion

        #region Start / stop

        public void start()
        {
            if (running)
            {
                Logger.log("server already started");
                return;
            }

            // [COURS 4 - Network] 127.0.0.1 like in the course and the skeleton (this computer only)
            // use IPAddress.Any to accept clients from other computers
            listener = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
            listener.Start();
            running = true;

            // [COURS 4 - Threads] the waiting for clients runs in its own thread
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
            listener.Stop();   // AcceptTcpClient() stops with an exception

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

        // listening thread : waits for the connections (like start() in the skeleton)
        private void listen()
        {
            try
            {
                while (running)
                {
                    // [COURS 4 - Network] blocking until a client connects
                    TcpClient comm = listener.AcceptTcpClient();
                    Logger.log("connection established @" + comm.Client.RemoteEndPoint);

                    Receiver r = new Receiver(comm, this);
                    addClient(r);

                    // [COURS 4 - Threads] 1 thread per client ("multi client server" slide)
                    new Thread(new ThreadStart(r.doOperation)).Start();
                }
            }
            catch (Exception)
            {
                // normal when stop() is called : listener.Stop() unblocks AcceptTcpClient()
            }
        }

        #endregion

        #region List of the clients (protected by lock)

        public void addClient(Receiver r)
        {
            lock (access_grant)   // [LAB 4 - lock]
            {
                clients.Add(r);
            }
        }

        public void removeClient(Receiver r)
        {
            lock (access_grant)   // [LAB 4 - lock]
            {
                clients.Remove(r);
            }
        }

        // gives the name to r if nobody else has it
        // (check + change in the SAME lock : two clients cannot take the same name)
        public bool reserveName(Receiver r, string name)
        {
            if (name.ToLower() == "financebot")
            {
                return false;
            }

            lock (access_grant)   // [LAB 4 - lock]
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
            lock (access_grant)   // [LAB 4 - lock]
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
            lock (access_grant)   // [LAB 4 - lock]
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

        #endregion

        #region Broadcast with the event (delegates)

        // [COURS 3 - Delegates] protected virtual method raising the event
        // (like onMyDel in the course and onPrimeFound in Lab 2)
        protected virtual void onBroadcast(object sender, MessageEventArgs e)
        {
            MessageDelegate subscribers = BroadcastEvent;   // copy : a client may unsubscribe at the same time
            if (subscribers != null)                        // check for subscribers
            {
                subscribers(sender, e);                     // calls Receiver.onBroadcast of EVERY subscriber
            }
        }

        // message for everybody (each Receiver keeps it or not, depending on its channel)
        public void broadcast(Message msg)
        {
            onBroadcast(this, new MessageEventArgs(msg));
        }

        public void sendUserList()
        {
            broadcast(new UserListMsg(getUsers()));
        }

        #endregion

        #region Private messages

        // message for one user only (no event : we know the receiver)
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

        #endregion
    }
}
