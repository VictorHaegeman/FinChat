using System;
using System.Net.Sockets;
using Communication;

namespace ChatServer
{
    // one Receiver per connected client, running in its own thread
    // (same idea as the Receiver class of the skeleton)
    public class Receiver
    {
        private TcpClient comm;
        private Server server;
        private string username = null;     // null until the login
        private string channel = "general";

        // several threads can send to this client at the same time
        // (a message from another user + an article) : one lock per client
        private object send_grant = new object();
        private bool closed = false;

        public Receiver(TcpClient s, Server server)
        {
            comm = s;
            this.server = server;
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        // ==================== reception loop ====================

        public void doOperation()
        {
            try
            {
                while (!closed)
                {
                    Message msg = Net.rcvMsg(comm.GetStream());

                    // check the class with "is", then cast (Classification course)
                    if (msg is LoginMsg)
                    {
                        login((LoginMsg)msg);
                    }
                    else if (username == null)
                    {
                        send(new InfoMsg("please log in first"));
                    }
                    else if (msg is ChatMsg)
                    {
                        publicMessage((ChatMsg)msg);
                    }
                    else if (msg is PrivateMsg)
                    {
                        PrivateMsg pm = (PrivateMsg)msg;
                        // the server writes the real sender name
                        server.sendPrivate(new PrivateMsg(username, pm.Target, pm.Text), this);
                    }
                    else if (msg is CommandMsg)
                    {
                        command((CommandMsg)msg);
                    }
                }
            }
            catch (Exception e)
            {
                // the client closed his window or the connection is lost
                if (!closed)
                {
                    Logger.log("connection lost with " + nameForLog() + " (" + e.GetType().Name + ")");
                }
            }
            close(null);
        }

        // ==================== actions ====================

        private void login(LoginMsg msg)
        {
            if (username != null)
            {
                send(new InfoMsg("you are already logged in"));
                return;
            }

            string name = "";
            if (msg.Username != null)
            {
                name = msg.Username.Trim();
            }
            if (name == "" || name.Contains(" "))
            {
                close("invalid username (no spaces)");
                return;
            }
            if (!server.reserveName(this, name))
            {
                close("the username " + name + " is already taken, choose another one");
                return;
            }

            // subscription to the event (Delegates course : +=)
            server.BroadcastEvent += new MessageDelegate(onBroadcast);

            Logger.log(username + " logged in");
            send(new InfoMsg("welcome " + username + ", you are in #general"));
            server.broadcast(new InfoMsg(username + " joined the chat"));
            server.sendUserList();
        }

        private void publicMessage(ChatMsg msg)
        {
            // the server uses its own information (real name and channel)
            server.broadcast(new ChatMsg(username, channel, msg.Text));
            Logger.log("[#" + channel + "] " + username + " : " + msg.Text);
        }

        private void command(CommandMsg msg)
        {
            string arg = "";
            if (msg.Argument != null)
            {
                arg = msg.Argument.Trim();
            }

            switch (msg.Command)
            {
                case "name":
                    string oldName = username;
                    if (arg == "" || arg.Contains(" "))
                    {
                        send(new InfoMsg("usage : /name newname (no spaces)"));
                    }
                    else if (!server.reserveName(this, arg))
                    {
                        send(new InfoMsg("the username " + arg + " is already taken"));
                    }
                    else
                    {
                        Logger.log(oldName + " is now " + username);
                        server.broadcast(new InfoMsg(oldName + " is now " + username));
                        server.sendUserList();
                    }
                    break;

                case "users":
                    send(new InfoMsg("connected users : " + string.Join(", ", server.getUsers())));
                    break;

                case "join":
                    string newChannel = arg.ToLower();
                    if (newChannel != "general" && newChannel != "finance")
                    {
                        send(new InfoMsg("unknown channel, use general or finance"));
                    }
                    else
                    {
                        channel = newChannel;
                        send(new InfoMsg("you joined #" + channel));
                        Logger.log(username + " joined #" + channel);

                        // the last finance article for the user who arrives in #finance
                        if (channel == "finance" && server.LastNews != null)
                        {
                            send(server.LastNews);
                        }
                    }
                    break;

                case "quit":
                    close("bye " + username);
                    break;

                default:
                    send(new InfoMsg("unknown command /" + msg.Command));
                    break;
            }
        }

        // ==================== event handler ====================

        // called by the event for every broadcast (like PrimeReceiver.action in Lab 2)
        public void onBroadcast(object sender, MessageEventArgs e)
        {
            // public messages are only for the users in the same channel
            if (e.Msg is ChatMsg && ((ChatMsg)e.Msg).Channel != channel)
            {
                return;
            }
            send(e.Msg);
        }

        // ==================== sending / closing ====================

        public void send(Message msg)
        {
            lock (send_grant)
            {
                try
                {
                    Net.sendMsg(comm.GetStream(), msg);
                }
                catch (Exception)
                {
                    // the connection is broken : the reception loop will close it
                }
            }
        }

        public void close(string reason)
        {
            lock (send_grant)
            {
                if (closed)
                {
                    return;   // already closed
                }
                closed = true;
            }

            if (reason != null)
            {
                send(new InfoMsg(reason));
            }

            // unsubscription (Delegates course : -=)
            server.BroadcastEvent -= onBroadcast;
            server.removeClient(this);
            comm.Close();

            if (username != null)
            {
                Logger.log(username + " disconnected");
                server.broadcast(new InfoMsg(username + " left the chat"));
                server.sendUserList();
            }
            else
            {
                Logger.log("connection closed (not logged in)");
            }
        }

        private string nameForLog()
        {
            if (username == null)
            {
                return "an unknown client";
            }
            return username;
        }
    }
}
