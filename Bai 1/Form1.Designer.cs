namespace Bai_1
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.Link = new System.Windows.Forms.TextBox();
            this.btn_get = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.richTextBox1.Location = new System.Drawing.Point(32, 163);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(854, 584);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // Link
            // 
            this.Link.BackColor = System.Drawing.Color.White;
            this.Link.Location = new System.Drawing.Point(32, 77);
            this.Link.Multiline = true;
            this.Link.Name = "Link";
            this.Link.Size = new System.Drawing.Size(656, 52);
            this.Link.TabIndex = 1;
            this.Link.TextChanged += new System.EventHandler(this.Link_TextChanged);
            // 
            // btn_get
            // 
            this.btn_get.BackColor = System.Drawing.Color.Transparent;
            this.btn_get.Location = new System.Drawing.Point(732, 77);
            this.btn_get.Name = "btn_get";
            this.btn_get.Size = new System.Drawing.Size(154, 52);
            this.btn_get.TabIndex = 2;
            this.btn_get.Text = "GET";
            this.btn_get.UseVisualStyleBackColor = false;
            this.btn_get.Click += new System.EventHandler(this.btn_get_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "Nhập Link: ";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(915, 778);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_get);
            this.Controls.Add(this.Link);
            this.Controls.Add(this.richTextBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox Link;
        private System.Windows.Forms.Button btn_get;
        private System.Windows.Forms.Label label1;
    }
}

