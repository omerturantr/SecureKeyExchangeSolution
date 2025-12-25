namespace Client1App
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            btnConnectToCA = new Button();
            btnSendKsToClient2 = new Button();
            btnTestSessionKey = new Button();
            groupEndpoints = new GroupBox();
            lblCaIp = new Label();
            lblCaPort = new Label();
            lblClient2Ip = new Label();
            lblClient2Port = new Label();
            txtCaIp = new TextBox();
            txtCaPort = new TextBox();
            txtClient2Ip = new TextBox();
            txtClient2Port = new TextBox();
            groupEndpoints.SuspendLayout();
            SuspendLayout();
            // 
            // btnConnectToCA
            // 
            btnConnectToCA.Location = new Point(158, 64);
            btnConnectToCA.Name = "btnConnectToCA";
            btnConnectToCA.Size = new Size(219, 92);
            btnConnectToCA.TabIndex = 0;
            btnConnectToCA.Text = "Connect to CA";
            btnConnectToCA.UseVisualStyleBackColor = true;
            btnConnectToCA.Click += btnConnectToCA_Click;
            // 
            // btnSendKsToClient2
            // 
            btnSendKsToClient2.Location = new Point(454, 64);
            btnSendKsToClient2.Name = "btnSendKsToClient2";
            btnSendKsToClient2.Size = new Size(216, 92);
            btnSendKsToClient2.TabIndex = 1;
            btnSendKsToClient2.Text = "Send Ks to Client2";
            btnSendKsToClient2.UseVisualStyleBackColor = true;
            btnSendKsToClient2.Click += btnSendKsToClient2_Click;
            // 
            // btnTestSessionKey
            // 
            btnTestSessionKey.Location = new Point(722, 64);
            btnTestSessionKey.Name = "btnTestSessionKey";
            btnTestSessionKey.Size = new Size(237, 92);
            btnTestSessionKey.TabIndex = 2;
            btnTestSessionKey.Text = "Test Ks";
            btnTestSessionKey.UseVisualStyleBackColor = true;
            btnTestSessionKey.Click += btnTestSessionKey_Click;
            // 
            // groupEndpoints
            // 
            groupEndpoints.Controls.Add(txtClient2Port);
            groupEndpoints.Controls.Add(txtClient2Ip);
            groupEndpoints.Controls.Add(txtCaPort);
            groupEndpoints.Controls.Add(txtCaIp);
            groupEndpoints.Controls.Add(lblClient2Port);
            groupEndpoints.Controls.Add(lblClient2Ip);
            groupEndpoints.Controls.Add(lblCaPort);
            groupEndpoints.Controls.Add(lblCaIp);
            groupEndpoints.Location = new Point(158, 196);
            groupEndpoints.Name = "groupEndpoints";
            groupEndpoints.Size = new Size(801, 166);
            groupEndpoints.TabIndex = 3;
            groupEndpoints.TabStop = false;
            groupEndpoints.Text = "Endpoints (IPv4 / Port)";
            // 
            // lblCaIp
            // 
            lblCaIp.AutoSize = true;
            lblCaIp.Location = new Point(24, 42);
            lblCaIp.Name = "lblCaIp";
            lblCaIp.Size = new Size(45, 20);
            lblCaIp.TabIndex = 0;
            lblCaIp.Text = "CA IP";
            // 
            // lblCaPort
            // 
            lblCaPort.AutoSize = true;
            lblCaPort.Location = new Point(410, 42);
            lblCaPort.Name = "lblCaPort";
            lblCaPort.Size = new Size(58, 20);
            lblCaPort.TabIndex = 1;
            lblCaPort.Text = "CA Port";
            // 
            // lblClient2Ip
            // 
            lblClient2Ip.AutoSize = true;
            lblClient2Ip.Location = new Point(24, 100);
            lblClient2Ip.Name = "lblClient2Ip";
            lblClient2Ip.Size = new Size(69, 20);
            lblClient2Ip.TabIndex = 2;
            lblClient2Ip.Text = "Client2 IP";
            // 
            // lblClient2Port
            // 
            lblClient2Port.AutoSize = true;
            lblClient2Port.Location = new Point(410, 100);
            lblClient2Port.Name = "lblClient2Port";
            lblClient2Port.Size = new Size(82, 20);
            lblClient2Port.TabIndex = 3;
            lblClient2Port.Text = "Client2 Port";
            // 
            // txtCaIp
            // 
            txtCaIp.Location = new Point(112, 39);
            txtCaIp.Name = "txtCaIp";
            txtCaIp.Size = new Size(240, 27);
            txtCaIp.TabIndex = 4;
            // 
            // txtCaPort
            // 
            txtCaPort.Location = new Point(510, 39);
            txtCaPort.Name = "txtCaPort";
            txtCaPort.Size = new Size(120, 27);
            txtCaPort.TabIndex = 5;
            // 
            // txtClient2Ip
            // 
            txtClient2Ip.Location = new Point(112, 97);
            txtClient2Ip.Name = "txtClient2Ip";
            txtClient2Ip.Size = new Size(240, 27);
            txtClient2Ip.TabIndex = 6;
            // 
            // txtClient2Port
            // 
            txtClient2Port.Location = new Point(510, 97);
            txtClient2Port.Name = "txtClient2Port";
            txtClient2Port.Size = new Size(120, 27);
            txtClient2Port.TabIndex = 7;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1140, 430);
            Controls.Add(groupEndpoints);
            Controls.Add(btnTestSessionKey);
            Controls.Add(btnSendKsToClient2);
            Controls.Add(btnConnectToCA);
            Name = "Form1";
            Text = "Client1App";
            Load += Form1_Load;
            groupEndpoints.ResumeLayout(false);
            groupEndpoints.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnConnectToCA;
        private Button btnSendKsToClient2;
        private Button btnTestSessionKey;

        private GroupBox groupEndpoints;
        private Label lblCaIp;
        private Label lblCaPort;
        private Label lblClient2Ip;
        private Label lblClient2Port;
        private TextBox txtCaIp;
        private TextBox txtCaPort;
        private TextBox txtClient2Ip;
        private TextBox txtClient2Port;
    }
}
