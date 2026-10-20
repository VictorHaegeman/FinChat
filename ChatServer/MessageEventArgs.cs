// =====================================================================
//  MessageEventArgs.cs  (project ChatServer)
//
//  Role   : the two pieces needed by the broadcast event of the Server
//           1. the delegate MessageDelegate = "shape" of the methods that
//              can react to a broadcast (Receiver.onBroadcast)
//           2. MessageEventArgs = the information carried by the event
//
//  Course : [COURS 3 - Delegates] same pattern as PrimeDelegate /
//           PrimeEventArgs in Lab 2 exercise 1
// =====================================================================

using System;
using Communication;

namespace ChatServer
{
    // [COURS 3 - Delegates] the delegate : (object sender, XxxEventArgs e)
    public delegate void MessageDelegate(object sender, MessageEventArgs e);

    // [COURS 3 - Delegates] the "nature" of the event : the message to broadcast
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
