namespace Bai6
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
            this.lblUrl = new System.Windows.Forms.Label();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.lblTokenType = new System.Windows.Forms.Label();
            this.txtTokenType = new System.Windows.Forms.TextBox();
            this.lblAccessToken = new System.Windows.Forms.Label();
            this.txtAccessToken = new System.Windows.Forms.TextBox();
            this.btnGetUserInfo = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // lblUrl
            // 
            this.lblUrl.AutoSize = true;
            this.lblUrl.Location = new System.Drawing.Point(20, 20);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(42, 20);
            this.lblUrl.TabIndex = 0;
            this.lblUrl.Text = "URL:";
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(20, 45);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(500, 27);
            this.txtUrl.TabIndex = 1;
            this.txtUrl.Text = "https://nt106.uitiot.vn/api/v1/user/me";
            // 
            // lblTokenType
            // 
            this.lblTokenType.AutoSize = true;
            this.lblTokenType.Location = new System.Drawing.Point(20, 90);
            this.lblTokenType.Name = "lblTokenType";
            this.lblTokenType.Size = new System.Drawing.Size(90, 20);
            this.lblTokenType.TabIndex = 2;
            this.lblTokenType.Text = "Token Type:";
            // 
            // txtTokenType
            // 
            this.txtTokenType.Location = new System.Drawing.Point(20, 115);
            this.txtTokenType.Name = "txtTokenType";
            this.txtTokenType.Size = new System.Drawing.Size(200, 27);
            this.txtTokenType.TabIndex = 3;
            this.txtTokenType.Text = "Bearer";
            // 
            // lblAccessToken
            // 
            this.lblAccessToken.AutoSize = true;
            this.lblAccessToken.Location = new System.Drawing.Point(20, 160);
            this.lblAccessToken.Name = "lblAccessToken";
            this.lblAccessToken.Size = new System.Drawing.Size(106, 20);
            this.lblAccessToken.TabIndex = 4;
            this.lblAccessToken.Text = "Access Token:";
            // 
            // txtAccessToken
            // 
            this.txtAccessToken.Location = new System.Drawing.Point(20, 185);
            this.txtAccessToken.Multiline = true;
            this.txtAccessToken.Name = "txtAccessToken";
            this.txtAccessToken.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAccessToken.Size = new System.Drawing.Size(500, 80);
            this.txtAccessToken.TabIndex = 5;
            // 
            // btnGetUserInfo
            // 
            this.btnGetUserInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnGetUserInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGetUserInfo.ForeColor = System.Drawing.Color.White;
            this.btnGetUserInfo.Location = new System.Drawing.Point(240, 115);
            this.btnGetUserInfo.Name = "btnGetUserInfo";
            this.btnGetUserInfo.Size = new System.Drawing.Size(280, 150);
            this.btnGetUserInfo.TabIndex = 6;
            this.btnGetUserInfo.Text = "GET USER INFO";
            this.btnGetUserInfo.UseVisualStyleBackColor = false;
            this.btnGetUserInfo.Click += new System.EventHandler(this.btnGetUserInfo_Click);
            // 
            // txtResult
            // 
            this.txtResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.txtResult.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtResult.Location = new System.Drawing.Point(20, 280);
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.Size = new System.Drawing.Size(500, 300);
            this.txtResult.TabIndex = 7;
            this.txtResult.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 600);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnGetUserInfo);
            this.Controls.Add(this.txtAccessToken);
            this.Controls.Add(this.lblAccessToken);
            this.Controls.Add(this.txtTokenType);
            this.Controls.Add(this.lblTokenType);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.lblUrl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bai6";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label lblTokenType;
        private System.Windows.Forms.TextBox txtTokenType;
        private System.Windows.Forms.Label lblAccessToken;
        private System.Windows.Forms.TextBox txtAccessToken;
        private System.Windows.Forms.Button btnGetUserInfo;
        private System.Windows.Forms.RichTextBox txtResult;
    }
}

