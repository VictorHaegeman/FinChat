using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Communication
{
    // same class as in the DistantCalculator skeleton :
    // a Message object is sent / received through the TCP stream with a BinaryFormatter
    public class Net
    {
        public static void sendMsg(Stream s, Message msg)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(s, msg);
        }

        public static Message rcvMsg(Stream s)
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (Message)bf.Deserialize(s);
        }
    }
}
