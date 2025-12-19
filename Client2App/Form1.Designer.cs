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
            txtLog = new TextBox();
            btnConnectToCA = new Button();
            SuspendLayout();
            // 
            // btnTestDecryptKs
            // 
            btnTestDecryptKs.Location = new Point(126, 142);
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
            // txtLog
            // 
            txtLog.Location = new Point(126, 369);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(1008, 250);
            txtLog.TabIndex = 2;
            // 
            // btnConnectToCA
            // 
            btnConnectToCA.Location = new Point(756, 142);
            btnConnectToCA.Name = "btnConnectToCA";
            btnConnectToCA.Size = new Size(246, 92);
            btnConnectToCA.TabIndex = 3;
            btnConnectToCA.Text = "Connect to CA";
            btnConnectToCA.UseVisualStyleBackColor = true;
            btnConnectToCA.Click += btnConnectToCA_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1350, 743);
            Controls.Add(btnConnectToCA);
            Controls.Add(txtLog);
            Controls.Add(btnStartListener);
            Controls.Add(btnTestDecryptKs);
            Name = "Form1";
            Text = "Client2App";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnTestDecryptKs;
        private Button btnStartListener;
        private TextBox txtLog;
        private Button btnConnectToCA;
    }
}
