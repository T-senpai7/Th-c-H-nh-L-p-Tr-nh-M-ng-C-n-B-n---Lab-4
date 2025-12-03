using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Bai4
{
    internal static class Program
    {
        private static List<Form> openForms = new List<Form>();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            // Menu để chọn Server hoặc Client
            Form menu = new Form();
            menu.Text = "Bai4 - Quản lý phòng vé - Menu";
            menu.Size = new Size(500, 400);
            menu.StartPosition = FormStartPosition.CenterScreen;
            menu.FormBorderStyle = FormBorderStyle.FixedDialog;
            menu.MaximizeBox = false;
            menu.MinimizeBox = true;
            menu.BackColor = Color.FromArgb(245, 245, 245);

            Label lblTitle = new Label();
            lblTitle.Text = "CHỌN CHỨC NĂNG";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Size = new Size(450, 50);
            lblTitle.Location = new Point(25, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.ForeColor = Color.FromArgb(52, 58, 64);

            Button btnServer = new Button();
            btnServer.Text = "TCP Server";
            btnServer.Size = new Size(180, 70);
            btnServer.Location = new Point(50, 90);
            btnServer.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnServer.BackColor = Color.FromArgb(13, 110, 253);
            btnServer.ForeColor = Color.White;
            btnServer.FlatStyle = FlatStyle.Flat;
            btnServer.FlatAppearance.BorderSize = 0;
            btnServer.Cursor = Cursors.Hand;
            btnServer.Click += (s, e) => {
                try
                {
                    Bai4Server serverForm = new Bai4Server();
                    serverForm.Show();
                    openForms.Add(serverForm);
                    
                    // Xử lý khi đóng form
                    serverForm.FormClosing += (sender, args) => {
                        openForms.Remove(serverForm);
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở Server: {ex.Message}", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnClient = new Button();
            btnClient.Text = "TCP Client";
            btnClient.Size = new Size(180, 70);
            btnClient.Location = new Point(270, 90);
            btnClient.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnClient.BackColor = Color.FromArgb(40, 167, 69);
            btnClient.ForeColor = Color.White;
            btnClient.FlatStyle = FlatStyle.Flat;
            btnClient.FlatAppearance.BorderSize = 0;
            btnClient.Cursor = Cursors.Hand;
            btnClient.Click += (s, e) => {
                try
                {
                    Bai4Client clientForm = new Bai4Client();
                    clientForm.Show();
                    openForms.Add(clientForm);
                    
                    // Xử lý khi đóng form
                    clientForm.FormClosing += (sender, args) => {
                        openForms.Remove(clientForm);
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở Client: {ex.Message}", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnWebServer = new Button();
            btnWebServer.Text = "Web Server";
            btnWebServer.Size = new Size(180, 70);
            btnWebServer.Location = new Point(50, 170);
            btnWebServer.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnWebServer.BackColor = Color.FromArgb(255, 193, 7);
            btnWebServer.ForeColor = Color.Black;
            btnWebServer.FlatStyle = FlatStyle.Flat;
            btnWebServer.FlatAppearance.BorderSize = 0;
            btnWebServer.Cursor = Cursors.Hand;
            btnWebServer.Click += (s, e) => {
                try
                {
                    WebServerForm webServerForm = new WebServerForm();
                    webServerForm.Show();
                    openForms.Add(webServerForm);
                    
                    // Xử lý khi đóng form
                    webServerForm.FormClosing += (sender, args) => {
                        openForms.Remove(webServerForm);
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở Web Server: {ex.Message}", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnWebClient = new Button();
            btnWebClient.Text = "Web Client";
            btnWebClient.Size = new Size(180, 70);
            btnWebClient.Location = new Point(270, 170);
            btnWebClient.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnWebClient.BackColor = Color.FromArgb(23, 162, 184);
            btnWebClient.ForeColor = Color.White;
            btnWebClient.FlatStyle = FlatStyle.Flat;
            btnWebClient.FlatAppearance.BorderSize = 0;
            btnWebClient.Cursor = Cursors.Hand;
            btnWebClient.Click += (s, e) => {
                try
                {
                    WebClientForm webClientForm = new WebClientForm();
                    webClientForm.Show();
                    openForms.Add(webClientForm);
                    
                    // Xử lý khi đóng form
                    webClientForm.FormClosing += (sender, args) => {
                        openForms.Remove(webClientForm);
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở Web Client: {ex.Message}", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnExit = new Button();
            btnExit.Text = "Thoát ứng dụng";
            btnExit.Size = new Size(150, 40);
            btnExit.Location = new Point(175, 280);
            btnExit.Font = new Font("Segoe UI", 10F);
            btnExit.BackColor = Color.FromArgb(220, 53, 69);
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += (s, e) => {
                DialogResult result = MessageBox.Show(
                    "Bạn có chắc muốn thoát ứng dụng?\nTất cả các cửa sổ Server/Client đang mở sẽ được đóng.",
                    "Xác nhận thoát",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };

            Label lblNote = new Label();
            lblNote.Text = "💡 Bạn có thể mở nhiều Client cùng lúc để test đồng bộ vé";
            lblNote.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblNote.ForeColor = Color.FromArgb(108, 117, 125);
            lblNote.Size = new Size(450, 20);
            lblNote.Location = new Point(25, 340);
            lblNote.TextAlign = ContentAlignment.MiddleCenter;

            menu.Controls.Add(lblTitle);
            menu.Controls.Add(btnServer);
            menu.Controls.Add(btnClient);
            menu.Controls.Add(btnWebServer);
            menu.Controls.Add(btnWebClient);
            menu.Controls.Add(btnExit);
            menu.Controls.Add(lblNote);

            // Khi đóng menu form, chỉ hỏi xác nhận nếu còn form khác đang mở
            menu.FormClosing += (s, e) => {
                if (openForms.Count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        $"Còn {openForms.Count} cửa sổ đang mở.\nBạn có chắc muốn đóng menu?\n(Các cửa sổ Server/Client vẫn sẽ hoạt động)",
                        "Đóng menu",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                    // Nếu Yes, chỉ đóng menu, không đóng các form khác
                }
            };

            // Đảm bảo menu có thể minimize
            menu.WindowState = FormWindowState.Normal;

            Application.Run(menu);
        }
    }
}

