// =====================================================================
//  Message.cs  (project Communication, shared by the server and the client)
//
//  Role   : all the messages that travel on the network.
//           Same idea as Expr / Result in the professor's skeleton.
//
//  Message      interface implemented by every message
//  LoginMsg     client -> server   first message (username)
//  ChatMsg      both directions    public message in a channel
//  PrivateMsg   both directions    private message (/msg)
//  CommandMsg   client -> server   /name /users /join /quit
//  InfoMsg      server -> client   information or error
//  UserListMsg  server -> client   list of the connected users
//
//  Course : [COURS 2 - Classification] interface + polymorphism
//           [COURS 6 - Serialization]  [Serializable] : can be sent with BinaryFormatter
//           [COURS 1 - Basics]         private attributes + read-only properties
// =====================================================================

using System;

namespace Communication
{
    // every message sent on the network implements Message   [COURS 2 - Classification]
    // the receiver finds the real type with "is" and a cast
    public interface Message
    {
        string ToString();
    }

    // client -> server : first message after the connection
    [Serializable]
    public class LoginMsg : Message
    {
        private string username;

        public LoginMsg(string username)
        {
            this.username = username;
        }

        public string Username
        {
            get { return username; }
        }

        public override string ToString()
        {
            return "login " + username;
        }
    }

    // both directions : public message in a channel (general or finance)
    [Serializable]
    public class ChatMsg : Message
    {
        private string sender;
        private string channel;
        private string text;

        public ChatMsg(string sender, string channel, string text)
        {
            this.sender = sender;
            this.channel = channel;
            this.text = text;
        }

        public string Sender
        {
            get { return sender; }
        }

        public string Channel
        {
            get { return channel; }
        }

        public string Text
        {
            get { return text; }
        }

        public override string ToString()
        {
            return "[#" + channel + "] " + sender + " : " + text;
        }
    }

    // both directions : private message to one user
    [Serializable]
    public class PrivateMsg : Message
    {
        private string sender;
        private string target;
        private string text;

        public PrivateMsg(string sender, string target, string text)
        {
            this.sender = sender;
            this.target = target;
            this.text = text;
        }

        public string Sender
        {
            get { return sender; }
        }

        public string Target
        {
            get { return target; }
        }

        public string Text
        {
            get { return text; }
        }

        public override string ToString()
        {
            return "[private] " + sender + " -> " + target + " : " + text;
        }
    }

    // client -> server : /name, /users, /join, /quit
    [Serializable]
    public class CommandMsg : Message
    {
        private string command;
        private string argument;

        public CommandMsg(string command, string argument)
        {
            this.command = command;
            this.argument = argument;
        }

        public string Command
        {
            get { return command; }
        }

        public string Argument
        {
            get { return argument; }
        }

        public override string ToString()
        {
            return "/" + command + " " + argument;
        }
    }

    // server -> client : information or error ("bob joined the chat"...)
    [Serializable]
    public class InfoMsg : Message
    {
        private string text;

        public InfoMsg(string text)
        {
            this.text = text;
        }

        public string Text
        {
            get { return text; }
        }

        public override string ToString()
        {
            return "[info] " + text;
        }
    }

    // server -> client : list of the connected users (an array is serializable)
    [Serializable]
    public class UserListMsg : Message
    {
        private string[] users;

        public UserListMsg(string[] users)
        {
            this.users = users;
        }

        public string[] Users
        {
            get { return users; }
        }

        public override string ToString()
        {
            return "connected users : " + string.Join(", ", users);
        }
    }
}
