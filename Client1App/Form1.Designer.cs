namespace Client1App
{
    partial class Form1
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
            btnConnectToCA = new Button();
            btnTestSessionKey = new Button();
            btnSendKsToClient2 = new Button();
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
            // btnTestSessionKey
            // 
            btnTestSessionKey.Location = new Point(722, 64);
            btnTestSessionKey.Name = "btnTestSessionKey";
            btnTestSessionKey.Size = new Size(237, 92);
            btnTestSessionKey.TabIndex = 1;
            btnTestSessionKey.Text = "Test Ks";
            btnTestSessionKey.UseVisualStyleBackColor = true;
            btnTestSessionKey.Click += btnTestSessionKey_Click;
            // 
            // btnSendKsToClient2
            // 
            btnSendKsToClient2.Location = new Point(454, 64);
            btnSendKsToClient2.Name = "btnSendKsToClient2";
            btnSendKsToClient2.Size = new Size(216, 92);
            btnSendKsToClient2.TabIndex = 2;
            btnSendKsToClient2.Text = "Send Ks to Client2";
            btnSendKsToClient2.UseVisualStyleBackColor = true;
            btnSendKsToClient2.Click += btnSendKsToClient2_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1444, 430);
            Controls.Add(btnSendKsToClient2);
            Controls.Add(btnTestSessionKey);
            Controls.Add(btnConnectToCA);
            Name = "Form1";
            Text = "Client1App";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button btnConnectToCA;
        private Button btnTestSessionKey;
        private Button btnSendKsToClient2;
    }
}
