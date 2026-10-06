namespace ChatClient
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.labelHost = new System.Windows.Forms.Label();
            this.textBoxHost = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.textBoxMessages = new System.Windows.Forms.TextBox();
            this.labelChannel = new System.Windows.Forms.Label();
            this.comboBoxChannel = new System.Windows.Forms.ComboBox();
            this.labelUsers = new System.Windows.Forms.Label();
            this.listBoxUsers = new System.Windows.Forms.ListBox();
            this.textBoxInput = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            //
            // panelTop : green bar with the connection settings
            //
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(31, 92, 74);
            this.panelTop.Controls.Add(this.labelHost);
            this.panelTop.Controls.Add(this.textBoxHost);
            this.panelTop.Controls.Add(this.labelPort);
            this.panelTop.Controls.Add(this.textBoxPort);
            this.panelTop.Controls.Add(this.labelName);
            this.panelTop.Controls.Add(this.textBoxName);
            this.panelTop.Controls.Add(this.buttonConnect);
            this.panelTop.Controls.Add(this.buttonDisconnect);
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(784, 48);
            this.panelTop.TabIndex = 0;
            //
            // labelHost
            //
            this.labelHost.AutoSize = true;
            this.labelHost.ForeColor = System.Drawing.Color.White;
            this.labelHost.Location = new System.Drawing.Point(12, 16);
            this.labelHost.Name = "labelHost";
            this.labelHost.Text = "Server";
            //
            // textBoxHost
            //
            this.textBoxHost.Location = new System.Drawing.Point(62, 13);
            this.textBoxHost.Name = "textBoxHost";
            this.textBoxHost.Size = new System.Drawing.Size(100, 20);
            this.textBoxHost.TabIndex = 1;
            this.textBoxHost.Text = "127.0.0.1";
            //
            // labelPort
            //
            this.labelPort.AutoSize = true;
            this.labelPort.ForeColor = System.Drawing.Color.White;
            this.labelPort.Location = new System.Drawing.Point(174, 16);
            this.labelPort.Name = "labelPort";
            this.labelPort.Text = "Port";
            //
            // textBoxPort
            //
            this.textBoxPort.Location = new System.Drawing.Point(206, 13);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(50, 20);
            this.textBoxPort.TabIndex = 2;
            this.textBoxPort.Text = "8976";
            //
            // labelName
            //
            this.labelName.AutoSize = true;
            this.labelName.ForeColor = System.Drawing.Color.White;
            this.labelName.Location = new System.Drawing.Point(270, 16);
            this.labelName.Name = "labelName";
            this.labelName.Text = "Username";
            //
            // textBoxName
            //
            this.textBoxName.Location = new System.Drawing.Point(330, 13);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(110, 20);
            this.textBoxName.TabIndex = 3;
            //
            // buttonConnect
            //
            this.buttonConnect.BackColor = System.Drawing.Color.FromArgb(227, 239, 233);
            this.buttonConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConnect.ForeColor = System.Drawing.Color.FromArgb(31, 92, 74);
            this.buttonConnect.Location = new System.Drawing.Point(456, 10);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(95, 27);
            this.buttonConnect.TabIndex = 4;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = false;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            //
            // buttonDisconnect
            //
            this.buttonDisconnect.BackColor = System.Drawing.Color.FromArgb(227, 239, 233);
            this.buttonDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDisconnect.ForeColor = System.Drawing.Color.FromArgb(31, 92, 74);
            this.buttonDisconnect.Location = new System.Drawing.Point(560, 10);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(95, 27);
            this.buttonDisconnect.TabIndex = 5;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = false;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            //
            // textBoxMessages : all the received messages
            //
            this.textBoxMessages.BackColor = System.Drawing.Color.White;
            this.textBoxMessages.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxMessages.Location = new System.Drawing.Point(12, 60);
            this.textBoxMessages.Multiline = true;
            this.textBoxMessages.Name = "textBoxMessages";
            this.textBoxMessages.ReadOnly = true;
            this.textBoxMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMessages.Size = new System.Drawing.Size(570, 340);
            this.textBoxMessages.TabIndex = 10;
            //
            // labelChannel
            //
            this.labelChannel.AutoSize = true;
            this.labelChannel.ForeColor = System.Drawing.Color.FromArgb(31, 92, 74);
            this.labelChannel.Location = new System.Drawing.Point(596, 60);
            this.labelChannel.Name = "labelChannel";
            this.labelChannel.Text = "Channel";
            //
            // comboBoxChannel
            //
            this.comboBoxChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxChannel.Items.AddRange(new object[] { "general", "finance" });
            this.comboBoxChannel.Location = new System.Drawing.Point(599, 78);
            this.comboBoxChannel.Name = "comboBoxChannel";
            this.comboBoxChannel.Size = new System.Drawing.Size(172, 21);
            this.comboBoxChannel.TabIndex = 11;
            this.comboBoxChannel.SelectedIndexChanged += new System.EventHandler(this.comboBoxChannel_SelectedIndexChanged);
            //
            // labelUsers
            //
            this.labelUsers.AutoSize = true;
            this.labelUsers.ForeColor = System.Drawing.Color.FromArgb(31, 92, 74);
            this.labelUsers.Location = new System.Drawing.Point(596, 112);
            this.labelUsers.Name = "labelUsers";
            this.labelUsers.Text = "Users (double click = /msg)";
            //
            // listBoxUsers
            //
            this.listBoxUsers.BackColor = System.Drawing.Color.FromArgb(227, 239, 233);
            this.listBoxUsers.Location = new System.Drawing.Point(599, 130);
            this.listBoxUsers.Name = "listBoxUsers";
            this.listBoxUsers.Size = new System.Drawing.Size(172, 264);
            this.listBoxUsers.TabIndex = 12;
            this.listBoxUsers.DoubleClick += new System.EventHandler(this.listBoxUsers_DoubleClick);
            //
            // textBoxInput : text or command typed by the user
            //
            this.textBoxInput.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxInput.Location = new System.Drawing.Point(12, 412);
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Size = new System.Drawing.Size(468, 23);
            this.textBoxInput.TabIndex = 6;
            //
            // buttonSend
            //
            this.buttonSend.BackColor = System.Drawing.Color.FromArgb(31, 92, 74);
            this.buttonSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSend.ForeColor = System.Drawing.Color.White;
            this.buttonSend.Location = new System.Drawing.Point(488, 410);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(94, 27);
            this.buttonSend.TabIndex = 7;
            this.buttonSend.Text = "Send";
            this.buttonSend.UseVisualStyleBackColor = false;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            //
            // labelStatus
            //
            this.labelStatus.AutoSize = true;
            this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(91, 104, 98);
            this.labelStatus.Location = new System.Drawing.Point(596, 416);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Text = "not connected";
            //
            // Form1
            //
            this.AcceptButton = this.buttonSend;   // Enter = Send
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(245, 246, 243);
            this.ClientSize = new System.Drawing.Size(784, 450);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.textBoxMessages);
            this.Controls.Add(this.labelChannel);
            this.Controls.Add(this.comboBoxChannel);
            this.Controls.Add(this.labelUsers);
            this.Controls.Add(this.listBoxUsers);
            this.Controls.Add(this.textBoxInput);
            this.Controls.Add(this.buttonSend);
            this.Controls.Add(this.labelStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "FinChat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelHost;
        private System.Windows.Forms.TextBox textBoxHost;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.TextBox textBoxMessages;
        private System.Windows.Forms.Label labelChannel;
        private System.Windows.Forms.ComboBox comboBoxChannel;
        private System.Windows.Forms.Label labelUsers;
        private System.Windows.Forms.ListBox listBoxUsers;
        private System.Windows.Forms.TextBox textBoxInput;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.Label labelStatus;
    }
}
