namespace Client2App
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
            btnTestDecryptKs = new Button();
            btnStartListener = new Button();
            btnConnectToCA = new Button();
            txtLog = new TextBox();
            groupEndpoints = new GroupBox();
            txtListenPort = new TextBox();
            txtCaPort = new TextBox();
            txtCaIp = new TextBox();
            lblListenPort = new Label();
            lblCaPort = new Label();
            lblCaIp = new Label();
            groupEndpoints.SuspendLayout();
            SuspendLayout();
            // 
            // btnTestDecryptKs
            // 
            btnTestDecryptKs.Location = new Point(715, 142);
            btnTestDecryptKs.Name = "btnTestDecryptKs";
            btnTestDecryptKs.Size = new Size(203, 92);
            btnTestDecryptKs.TabIndex = 0;
            btnTestDecryptKs.Text = "Test Decrypt Ks";
            btnTestDecryptKs.UseVisualStyleBackColor = true;
            btnTestDecryptKs.Click += btnTestDecryptKs_Click;
            // 
            // btnStartListener
            // 
            btnStartListener.Location = new Point(395, 142);
            btnStartListener.Name = "btnStartListener";
            btnStartListener.Size = new Size(296, 92);
            btnStartListener.TabIndex = 1;
            btnStartListener.Text = "Start Listener";
            btnStartListener.UseVisualStyleBackColor = true;
            btnStartListener.Click += btnStartListener_Click;
            // 
            // btnConnectToCA
            // 
            btnConnectToCA.Location = new Point(126, 142);
            btnConnectToCA.Name = "btnConnectToCA";
            btnConnectToCA.Size = new Size(246, 92);
            btnConnectToCA.TabIndex = 2;
            btnConnectToCA.Text = "Connect to CA";
            btnConnectToCA.UseVisualStyleBackColor = true;
            btnConnectToCA.Click += btnConnectToCA_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(126, 369);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(1008, 250);
            txtLog.TabIndex = 3;
            // 
            // groupEndpoints
            // 
            groupEndpoints.Controls.Add(txtListenPort);
            groupEndpoints.Controls.Add(txtCaPort);
            groupEndpoints.Controls.Add(txtCaIp);
            groupEndpoints.Controls.Add(lblListenPort);
            groupEndpoints.Controls.Add(lblCaPort);
            groupEndpoints.Controls.Add(lblCaIp);
            groupEndpoints.Location = new Point(126, 28);
            groupEndpoints.Name = "groupEndpoints";
            groupEndpoints.Size = new Size(1008, 86);
            groupEndpoints.TabIndex = 4;
            groupEndpoints.TabStop = false;
            groupEndpoints.Text = "Endpoints (IPv4 / Port)";
            // 
            // txtListenPort
            // 
            txtListenPort.Location = new Point(730, 35);
            txtListenPort.Name = "txtListenPort";
            txtListenPort.Size = new Size(140, 27);
            txtListenPort.TabIndex = 5;
            // 
            // txtCaPort
            // 
            txtCaPort.Location = new Point(430, 35);
            txtCaPort.Name = "txtCaPort";
            txtCaPort.Size = new Size(140, 27);
            txtCaPort.TabIndex = 4;
            // 
            // txtCaIp
            // 
            txtCaIp.Location = new Point(80, 35);
            txtCaIp.Name = "txtCaIp";
            txtCaIp.Size = new Size(240, 27);
            txtCaIp.TabIndex = 3;
            // 
            // lblListenPort
            // 
            lblListenPort.AutoSize = true;
            lblListenPort.Location = new Point(640, 38);
            lblListenPort.Name = "lblListenPort";
            lblListenPort.Size = new Size(77, 20);
            lblListenPort.TabIndex = 2;
            lblListenPort.Text = "Listen Port";
            // 
            // lblCaPort
            // 
            lblCaPort.AutoSize = true;
            lblCaPort.Location = new Point(360, 38);
            lblCaPort.Name = "lblCaPort";
            lblCaPort.Size = new Size(58, 20);
            lblCaPort.TabIndex = 1;
            lblCaPort.Text = "CA Port";
            // 
            // lblCaIp
            // 
            lblCaIp.AutoSize = true;
            lblCaIp.Location = new Point(20, 38);
            lblCaIp.Name = "lblCaIp";
            lblCaIp.Size = new Size(44, 20);
            lblCaIp.TabIndex = 0;
            lblCaIp.Text = "CA IP";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1350, 743);
            Controls.Add(groupEndpoints);
            Controls.Add(txtLog);
            Controls.Add(btnConnectToCA);
            Controls.Add(btnStartListener);
            Controls.Add(btnTestDecryptKs);
            Name = "Form1";
            Text = "Client2App";
            groupEndpoints.ResumeLayout(false);
            groupEndpoints.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnTestDecryptKs;
        private Button btnStartListener;
        private Button btnConnectToCA;
        private TextBox txtLog;

        private GroupBox groupEndpoints;
        private Label lblCaIp;
        private Label lblCaPort;
        private Label lblListenPort;
        private TextBox txtCaIp;
        private TextBox txtCaPort;
        private TextBox txtListenPort;
    }
}
