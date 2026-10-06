using System;
using Communication;

namespace ChatServer
{
    // the delegate : "shape" of the methods that react to a broadcast
    // (same pattern as PrimeDelegate in Lab 2)
    public delegate void MessageDelegate(object sender, MessageEventArgs e);

    // the information carried by the event : the message to broadcast
    public class MessageEventArgs : EventArgs
    {
        private Message msg;

        public Message Msg
        {
            get { return msg; }
        }

        public MessageEventArgs(Message m) : base()
        {
            msg = m;
        }
    }
}
