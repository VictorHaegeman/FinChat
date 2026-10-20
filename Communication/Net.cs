// =====================================================================
//  Net.cs  (project Communication, shared by the server and the client)
//
//  Role   : send / receive ONE Message object on a TCP stream.
//           Taken as is from the professor's skeleton (DistantCalculator).
//
//  Course : [COURS 6 - Serialization] BinaryFormatter
//           [COURS 4 - Network]       the Stream comes from TcpClient.GetStream()
// =====================================================================

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Communication
{
    public class Net
    {
        // object -> bytes -> written in the TCP stream   [COURS 6 - Serialization]
        public static void sendMsg(Stream s, Message msg)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(s, msg);
        }

        // bytes read from the TCP stream -> object   [COURS 6 - Serialization]
        // blocking : waits until a whole message has arrived
        public static Message rcvMsg(Stream s)
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (Message)bf.Deserialize(s);
        }
    }
}
