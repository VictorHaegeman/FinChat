// =====================================================================
//  Receiver.cs  (project ChatServer)
//
//  Role   : ONE object per connected client, running in its own thread
//           (same idea as the Receiver class of the skeleton).
//           - reads the messages of its client in a loop
//           - sorts them by type and does the action
//           - subscribes to the broadcast event of the Server
//           - sends messages to its client
//
//  Course : [COURS 2 - Classification] "is" + cast to know the message type
//           [COURS 3 - Delegates]      += / -= on the event, onBroadcast
//           [COURS 4 - Threads]        doOperation() runs in a thread
//           [COURS 2 - Exceptions]     try / catch when the client disappears
//           [LAB 4 - lock]             one lock per client for sending
//
//  Map of the file :
//      #region Attributes
//      #region Reception loop        doOperation()
//      #region Actions               login(), publicMessage(), command()
//      #region Event handler         onBroadcast()
//      #region Send / close          send(), close()
// =====================================================================

using System;
using System.Net.Sockets;
using Communication;

namespace ChatServer
{
    public class Receiver
    {
        #region Attributes

        private TcpClient comm;
        private Server server;
        private string username = null;     // null until the login
        private string channel = "general";

        // [LAB 4 - lock] several threads can send to this client at the same time
        // (a message from another user + a finance article) : one lock per client
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

        #endregion

        #region Reception loop

        // [COURS 4 - Threads] method run by the thread of this client
        public void doOperation()
        {
            try   // [COURS 2 - Exceptions]
            {
                while (!closed)
                {
                    // [COURS 6 - Serialization] waits for the next message
                    Message msg = Net.rcvMsg(comm.GetStream());

                    // [COURS 2 - Classification] check the class with "is", then cast
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

        #endregion

        #region Actions (login, public message, commands)

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

            // [COURS 3 - Delegates] subscription to the event (+=)
            server.BroadcastEvent += new MessageDelegate(onBroadcast);

            Logger.log(username + " logged in");
            send(new InfoMsg("welcome " + username + ", you are in #general (type /help for the commands)"));
            server.broadcast(new InfoMsg(username + " joined the chat"));
            server.sendUserList();
        }

        private void publicMessage(ChatMsg msg)
        {
            // the server uses its own information (real name and channel)
            server.broadcast(new ChatMsg(username, channel, msg.Text));
            Logger.log("[#" + channel + "] " + username + " : " + msg.Text);
        }

        // /name, /users, /join, /quit
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

        #endregion

        #region Event handler

        // [COURS 3 - Delegates] called by the event for EVERY broadcast
        // (like receiver.action in the course and PrimeReceiver.action in Lab 2)
        public void onBroadcast(object sender, MessageEventArgs e)
        {
            // public messages are only for the users in the same channel
            if (e.Msg is ChatMsg && ((ChatMsg)e.Msg).Channel != channel)
            {
                return;
            }
            send(e.Msg);
        }

        #endregion

        #region Send / close

        public void send(Message msg)
        {
            lock (send_grant)   // [LAB 4 - lock] only one thread writes in this socket at a time
            {
                try
                {
                    Net.sendMsg(comm.GetStream(), msg);   // [COURS 6 - Serialization]
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

            // [COURS 3 - Delegates] unsubscription (-=)
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

        #endregion
    }
}
