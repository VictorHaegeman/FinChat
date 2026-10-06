using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Communication;
// System.Windows.Forms also has a "Message" class : we say which one we use
using Message = Communication.Message;

namespace ChatClient
{
    // delegates used to come back on the UI thread with Invoke (Lab 2)
    public delegate void displayDelegate(Message msg);
    public delegate void disconnectedDelegate();

    public partial class Form1 : Form
    {
        private TcpClient comm;
        private Thread receiveThread;
        private bool connected = false;
        private string channel = "general";

        public Form1()
        {
            InitializeComponent();
            comboBoxChannel.SelectedIndex = 0;
            setConnected(false);
        }

        // ==================== buttons ====================

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            string name = textBoxName.Text.Trim();
            if (name == "")
            {
                addLine("[info] choose a username first");
                return;
            }

            try
            {
                // connection to the server (like the skeleton)
                comm = new TcpClient(textBoxHost.Text.Trim(), int.Parse(textBoxPort.Text.Trim()));
            }
            catch (Exception ex)
            {
                addLine("[error] cannot connect to the server : " + ex.Message);
                return;
            }

            channel = "general";
            comboBoxChannel.SelectedIndex = 0;   // before connected = true : no /join sent
            connected = true;
            setConnected(true);
            listBoxUsers.Items.Clear();

            // thread that receives the messages of the server
            receiveThread = new Thread(new ThreadStart(receive));
            receiveThread.Start();

            send(new LoginMsg(name));
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            disconnect();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string text = textBoxInput.Text.Trim();
            if (text == "" || !connected)
            {
                return;
            }
            textBoxInput.Clear();

            if (text.StartsWith("/"))
            {
                sendCommand(text);
            }
            else
            {
                // public message in the current channel (the server writes the sender name)
                send(new ChatMsg(null, channel, text));
            }
        }

        private void comboBoxChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newChannel = (string)comboBoxChannel.SelectedItem;
            if (!connected || newChannel == channel)
            {
                return;
            }
            channel = newChannel;
            send(new CommandMsg("join", channel));
        }

        // double click on a user : prepares a private message
        private void listBoxUsers_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxUsers.SelectedItem != null)
            {
                textBoxInput.Text = "/msg " + listBoxUsers.SelectedItem + " ";
                textBoxInput.Focus();
                textBoxInput.SelectionStart = textBoxInput.Text.Length;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            disconnect();
        }

        // ==================== commands ====================

        // "/msg bob hello" -> command "msg", argument "bob hello"
        private void sendCommand(string text)
        {
            string[] parts = text.Substring(1).Split(new char[] { ' ' }, 2);
            string cmd = parts[0].ToLower();
            string arg = "";
            if (parts.Length > 1)
            {
                arg = parts[1].Trim();
            }

            if (cmd == "msg")
            {
                string[] p = arg.Split(new char[] { ' ' }, 2);
                if (p.Length < 2)
                {
                    addLine("[info] usage : /msg user message");
                    return;
                }
                send(new PrivateMsg(null, p[0], p[1]));
            }
            else if (cmd == "quit")
            {
                disconnect();
            }
            else if (cmd == "join" && comboBoxChannel.Items.Contains(arg.ToLower()))
            {
                // changing the combo box sends the /join (comboBoxChannel_SelectedIndexChanged)
                comboBoxChannel.SelectedItem = arg.ToLower();
            }
            else
            {
                // /name, /users, or an unknown command (the server answers)
                send(new CommandMsg(cmd, arg));
            }
        }

        // ==================== network ====================

        private void send(Message msg)
        {
            try
            {
                Net.sendMsg(comm.GetStream(), msg);
            }
            catch (Exception ex)
            {
                addLine("[error] " + ex.Message);
            }
        }

        // runs in receiveThread
        private void receive()
        {
            try
            {
                while (true)
                {
                    Message msg = Net.rcvMsg(comm.GetStream());
                    display(msg);
                }
            }
            catch (Exception)
            {
                // the connection is closed
            }
            disconnected();
        }

        private void disconnect()
        {
            if (!connected)
            {
                return;
            }
            send(new CommandMsg("quit", ""));
            connected = false;
            comm.Close();   // receive() stops with an exception
            addLine("[info] you left the chat");
            setConnected(false);
            listBoxUsers.Items.Clear();
        }

        // ==================== display (thread-safe, Lab 2) ====================

        private void display(Message msg)
        {
            if (InvokeRequired)
            {
                // we are in receiveThread : ask the UI thread to run display()
                Invoke(new displayDelegate(display), new object[] { msg });
                return;
            }

            if (msg is UserListMsg)
            {
                listBoxUsers.Items.Clear();
                foreach (string user in ((UserListMsg)msg).Users)
                {
                    listBoxUsers.Items.Add(user);
                }
            }
            else
            {
                addLine(msg.ToString());
            }
        }

        // called at the end of receiveThread (server stopped, login refused...)
        private void disconnected()
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new disconnectedDelegate(disconnected));
                }
                catch (Exception)
                {
                    // the window is already closed
                }
                return;
            }

            if (connected)
            {
                connected = false;
                comm.Close();
                addLine("[info] disconnected from the server");
                setConnected(false);
                listBoxUsers.Items.Clear();
            }
        }

        private void addLine(string line)
        {
            textBoxMessages.AppendText(line + Environment.NewLine);
        }

        private void setConnected(bool c)
        {
            buttonConnect.Enabled = !c;
            buttonDisconnect.Enabled = c;
            buttonSend.Enabled = c;
            textBoxHost.Enabled = !c;
            textBoxPort.Enabled = !c;
            textBoxName.Enabled = !c;

            if (c)
            {
                labelStatus.Text = "connected";
                Text = "FinChat - " + textBoxName.Text.Trim();
            }
            else
            {
                labelStatus.Text = "not connected";
                Text = "FinChat";
            }
        }
    }
}
