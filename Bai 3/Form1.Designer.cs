namespace Bai_3
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnR = new System.Windows.Forms.Button();
            this.DownFile = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Location = new System.Drawing.Point(34, 241);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(820, 604);
            this.webView.TabIndex = 0;
            this.webView.ZoomFactor = 1D;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.btnReload);
            this.panel1.Controls.Add(this.btnR);
            this.panel1.Controls.Add(this.DownFile);
            this.panel1.Controls.Add(this.btnLoad);
            this.panel1.Location = new System.Drawing.Point(34, 37);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(820, 170);
            this.panel1.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(17, 36);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(788, 60);
            this.textBox1.TabIndex = 4;
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(664, 114);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(141, 44);
            this.btnReload.TabIndex = 3;
            this.btnReload.Text = "Reload";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnR
            // 
            this.btnR.Location = new System.Drawing.Point(409, 114);
            this.btnR.Name = "btnR";
            this.btnR.Size = new System.Drawing.Size(219, 44);
            this.btnR.TabIndex = 2;
            this.btnR.Text = "Down Resources";
            this.btnR.UseVisualStyleBackColor = true;
            this.btnR.Click += new System.EventHandler(this.button1_Click);
            // 
            // DownFile
            // 
            this.DownFile.Location = new System.Drawing.Point(214, 114);
            this.DownFile.Name = "DownFile";
            this.DownFile.Size = new System.Drawing.Size(145, 44);
            this.DownFile.TabIndex = 1;
            this.DownFile.Text = "Down Files";
            this.DownFile.UseVisualStyleBackColor = true;
            this.DownFile.Click += new System.EventHandler(this.DownFile_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(17, 113);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(145, 45);
            this.btnLoad.TabIndex = 0;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 879);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.webView);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnR;
        private System.Windows.Forms.Button DownFile;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.TextBox textBox1;
    }
}

