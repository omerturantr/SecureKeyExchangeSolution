namespace CAApp
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
            btnGenerateCAKeys = new Button();
            txtCAPublicKey = new TextBox();
            lstLog = new ListBox();
            btnStartCAServer = new Button();
            SuspendLayout();
            // 
            // btnGenerateCAKeys
            // 
            btnGenerateCAKeys.Location = new Point(32, 60);
            btnGenerateCAKeys.Name = "btnGenerateCAKeys";
            btnGenerateCAKeys.Size = new Size(166, 29);
            btnGenerateCAKeys.TabIndex = 0;
            btnGenerateCAKeys.Text = "Generate CA Keys";
            btnGenerateCAKeys.UseVisualStyleBackColor = true;
            btnGenerateCAKeys.Click += button1_Click;
            // 
            // txtCAPublicKey
            // 
            txtCAPublicKey.Location = new Point(713, 216);
            txtCAPublicKey.Multiline = true;
            txtCAPublicKey.Name = "txtCAPublicKey";
            txtCAPublicKey.ReadOnly = true;
            txtCAPublicKey.ScrollBars = ScrollBars.Vertical;
            txtCAPublicKey.Size = new Size(636, 304);
            txtCAPublicKey.TabIndex = 1;
            txtCAPublicKey.TextChanged += txtCAPublicKey_TextChanged;
            // 
            // lstLog
            // 
            lstLog.FormattingEnabled = true;
            lstLog.Location = new Point(32, 216);
            lstLog.Name = "lstLog";
            lstLog.Size = new Size(528, 304);
            lstLog.TabIndex = 2;
            // 
            // btnStartCAServer
            // 
            btnStartCAServer.Location = new Point(713, 60);
            btnStartCAServer.Name = "btnStartCAServer";
            btnStartCAServer.Size = new Size(159, 29);
            btnStartCAServer.TabIndex = 3;
            btnStartCAServer.Text = "Start CA Server";
            btnStartCAServer.UseVisualStyleBackColor = true;
            btnStartCAServer.Click += btnStartCAServer_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1446, 745);
            Controls.Add(btnStartCAServer);
            Controls.Add(lstLog);
            Controls.Add(txtCAPublicKey);
            Controls.Add(btnGenerateCAKeys);
            Name = "Form1";
            Text = "CAApp";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnGenerateCAKeys;
        private TextBox txtCAPublicKey;
        private ListBox lstLog;
        private Button btnStartCAServer;
    }
}
