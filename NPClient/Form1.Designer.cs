using System.Drawing;
using System.Windows.Forms;

namespace NPClient
{
    partial class Tool
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Tool));
            TextIP = new TextBox();
            TextPort = new TextBox();
            LabelIP = new Label();
            LabelPort = new Label();
            ConnectServer = new Button();
            DisconnectServer = new Button();
            Console = new RichTextBox();
            label1 = new Label();
            label2 = new Label();
            lable3 = new Label();
            TextPayload = new TextBox();
            SendData = new Button();
            comboFlags = new ComboBox();
            comboCmd = new ComboBox();
            SuspendLayout();
            // 
            // TextIP
            // 
            TextIP.AccessibleRole = AccessibleRole.None;
            TextIP.Location = new Point(75, 29);
            TextIP.Name = "TextIP";
            TextIP.Size = new Size(188, 27);
            TextIP.TabIndex = 0;
            TextIP.Text = "192.168.1.2";
            // 
            // TextPort
            // 
            TextPort.Location = new Point(75, 73);
            TextPort.Name = "TextPort";
            TextPort.Size = new Size(188, 27);
            TextPort.TabIndex = 1;
            TextPort.Text = "65000";
            // 
            // LabelIP
            // 
            LabelIP.AutoSize = true;
            LabelIP.Font = new Font("Arial Black", 9F);
            LabelIP.Location = new Point(12, 29);
            LabelIP.Name = "LabelIP";
            LabelIP.Size = new Size(27, 22);
            LabelIP.TabIndex = 2;
            LabelIP.Text = "IP";
            // 
            // LabelPort
            // 
            LabelPort.AutoSize = true;
            LabelPort.Font = new Font("Arial Black", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LabelPort.Location = new Point(12, 73);
            LabelPort.Name = "LabelPort";
            LabelPort.Size = new Size(57, 22);
            LabelPort.TabIndex = 3;
            LabelPort.Text = "PORT";
            // 
            // ConnectServer
            // 
            ConnectServer.Font = new Font("Arial Black", 9F);
            ConnectServer.ForeColor = SystemColors.ActiveCaptionText;
            ConnectServer.Location = new Point(75, 115);
            ConnectServer.Name = "ConnectServer";
            ConnectServer.Size = new Size(91, 29);
            ConnectServer.TabIndex = 4;
            ConnectServer.Text = "Connect";
            ConnectServer.UseVisualStyleBackColor = true;
            ConnectServer.Click += Connect_Click;
            // 
            // DisconnectServer
            // 
            DisconnectServer.Font = new Font("Arial Black", 9F);
            DisconnectServer.ForeColor = SystemColors.ActiveCaptionText;
            DisconnectServer.Location = new Point(172, 115);
            DisconnectServer.Name = "DisconnectServer";
            DisconnectServer.Size = new Size(91, 29);
            DisconnectServer.TabIndex = 5;
            DisconnectServer.Text = "Disconet";
            DisconnectServer.UseVisualStyleBackColor = true;
            DisconnectServer.Click += Disconnect_Click;
            // 
            // Console
            // 
            Console.BackColor = SystemColors.MenuText;
            Console.Font = new Font("Arial Black", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Console.ForeColor = SystemColors.InactiveBorder;
            Console.Location = new Point(12, 166);
            Console.Name = "Console";
            Console.Size = new Size(1038, 395);
            Console.TabIndex = 6;
            Console.Text = "";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial Black", 9F);
            label1.Location = new Point(534, 73);
            label1.Name = "label1";
            label1.Size = new Size(48, 22);
            label1.TabIndex = 8;
            label1.Text = "CMD";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Arial Black", 9F);
            label2.Location = new Point(302, 73);
            label2.Name = "label2";
            label2.Size = new Size(55, 22);
            label2.TabIndex = 9;
            label2.Text = "FLAG";
            // 
            // lable3
            // 
            lable3.AutoSize = true;
            lable3.Font = new Font("Arial Black", 9F);
            lable3.Location = new Point(301, 34);
            lable3.Name = "lable3";
            lable3.Size = new Size(54, 22);
            lable3.TabIndex = 11;
            lable3.Text = "DATA";
            // 
            // TextPayload
            // 
            TextPayload.Location = new Point(362, 29);
            TextPayload.Name = "TextPayload";
            TextPayload.Size = new Size(382, 27);
            TextPayload.TabIndex = 12;
            // 
            // SendData
            // 
            SendData.Font = new Font("Arial Black", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            SendData.ForeColor = SystemColors.ActiveCaptionText;
            SendData.Location = new Point(362, 115);
            SendData.Name = "SendData";
            SendData.Size = new Size(107, 27);
            SendData.TabIndex = 13;
            SendData.Text = "Send Data";
            SendData.UseVisualStyleBackColor = true;
            SendData.Click += SendData_Click;
            // 
            // comboFlags
            // 
            comboFlags.FormattingEnabled = true;
            comboFlags.Location = new Point(362, 67);
            comboFlags.Name = "comboFlags";
            comboFlags.Size = new Size(156, 28);
            comboFlags.TabIndex = 14;
            // 
            // comboCmd
            // 
            comboCmd.FormattingEnabled = true;
            comboCmd.Location = new Point(588, 67);
            comboCmd.Name = "comboCmd";
            comboCmd.Size = new Size(156, 28);
            comboCmd.TabIndex = 15;
            // 
            // Tool
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Desktop;
            ClientSize = new Size(1062, 573);
            Controls.Add(comboCmd);
            Controls.Add(comboFlags);
            Controls.Add(SendData);
            Controls.Add(TextPayload);
            Controls.Add(lable3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(Console);
            Controls.Add(DisconnectServer);
            Controls.Add(ConnectServer);
            Controls.Add(LabelPort);
            Controls.Add(LabelIP);
            Controls.Add(TextPort);
            Controls.Add(TextIP);
            ForeColor = SystemColors.Control;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Tool";
            Text = "Tool";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TextIP;
        private TextBox TextPort;
        private Label LabelIP;
        private Label LabelPort;
        private Button ConnectServer;
        private Button DisconnectServer;
        private RichTextBox Console;
        private Label label1;
        private Label label2;
        private Label lable3;
        private TextBox TextPayload;
        private Button SendData;
        private ComboBox comboFlags;
        private ComboBox comboCmd;
    }
}
