// =====================================================================
//  Program.cs  (project ChatClient)
//  Role : entry point of the client, opens the chat window (Form1)
// =====================================================================

using System;
using System.Windows.Forms;

namespace ChatClient
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
