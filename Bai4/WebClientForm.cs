using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Bai4
{
    public class WebClientForm : Form
    {
        // UI Components
        private Panel pnlHeader;
        private Label lblTitle;
        private Panel pnlConnection;
        private TextBox txtServerIP;
        private TextBox txtServerPort;
        private Label lblServerIP;
        private Label lblServerPort;
        private Button btnConnect;
        private Label lblStatus;
        private TextBox txtLog;
        private Button btnClearLog;
        private Button btnOpenWeb;
        private WebBrowser? webBrowser;
        private TabControl tabControl;
        private TabPage tabLog;
        private TabPage tabWeb;
        private const int HTTP_PORT = 8888; // HTTP server port

        // Network
        private TcpClient? tcpClient;
        private NetworkStream? networkStream;
        private Thread? receiveThread;
        private bool isConnected = false;
        private const int DEFAULT_PORT = 8889; // Different port from HTTP server

        public WebClientForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Web Client - TCP Connection & Booking";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Font = new Font("Segoe UI", 10F);

            // Header
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.BackColor = Color.FromArgb(13, 110, 253);
            pnlHeader.Height = 60;
            pnlHeader.Padding = new Padding(0);

            lblTitle = new Label();
            lblTitle.Text = "WEB CLIENT - TCP CONNECTION";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = false;
            lblTitle.Size = new Size(700, 60);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblTitle);

            // Connection panel
            pnlConnection = new Panel();
            pnlConnection.Size = new Size(660, 80);
            pnlConnection.Location = new Point(20, 80);
            pnlConnection.BackColor = Color.White;
            pnlConnection.BorderStyle = BorderStyle.FixedSingle;

            lblServerIP = new Label();
            lblServerIP.Text = "Server IP:";
            lblServerIP.Location = new Point(20, 15);
            lblServerIP.Size = new Size(80, 25);

            txtServerIP = new TextBox();
            txtServerIP.Text = "127.0.0.1";
            txtServerIP.Location = new Point(110, 12);
            txtServerIP.Size = new Size(150, 30);

            lblServerPort = new Label();
            lblServerPort.Text = "Port:";
            lblServerPort.Location = new Point(280, 15);
            lblServerPort.Size = new Size(50, 25);

            txtServerPort = new TextBox();
            txtServerPort.Text = DEFAULT_PORT.ToString();
            txtServerPort.Location = new Point(340, 12);
            txtServerPort.Size = new Size(80, 30);

            btnConnect = new Button();
            btnConnect.Text = "Kết nối";
            btnConnect.Location = new Point(440, 10);
            btnConnect.Size = new Size(120, 35);
            btnConnect.BackColor = Color.FromArgb(40, 167, 69);
            btnConnect.ForeColor = Color.White;
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnConnect.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConnect.Click += BtnConnect_Click;

            lblStatus = new Label();
            lblStatus.Text = "Trạng thái: Chưa kết nối";
            lblStatus.Location = new Point(20, 50);
            lblStatus.Size = new Size(540, 25);
            lblStatus.Font = new Font("Segoe UI", 10F);
            lblStatus.ForeColor = Color.FromArgb(220, 53, 69);

            // Open Web button
            btnOpenWeb = new Button();
            btnOpenWeb.Text = "Mở trang đặt vé";
            btnOpenWeb.Location = new Point(570, 10);
            btnOpenWeb.Size = new Size(120, 35);
            btnOpenWeb.BackColor = Color.FromArgb(255, 193, 7);
            btnOpenWeb.ForeColor = Color.Black;
            btnOpenWeb.FlatStyle = FlatStyle.Flat;
            btnOpenWeb.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOpenWeb.Enabled = false;
            btnOpenWeb.Click += BtnOpenWeb_Click;

            pnlConnection.Controls.Add(lblServerIP);
            pnlConnection.Controls.Add(txtServerIP);
            pnlConnection.Controls.Add(lblServerPort);
            pnlConnection.Controls.Add(txtServerPort);
            pnlConnection.Controls.Add(btnConnect);
            pnlConnection.Controls.Add(btnOpenWeb);
            pnlConnection.Controls.Add(lblStatus);

            // Tab Control
            tabControl = new TabControl();
            tabControl.Location = new Point(20, 170);
            tabControl.Size = new Size(960, 490);
            tabControl.Font = new Font("Segoe UI", 10F);

            // Tab 1: Log
            tabLog = new TabPage("Log kết nối");
            tabLog.BackColor = Color.White;

            Label lblLog = new Label();
            lblLog.Text = "Log kết nối:";
            lblLog.Location = new Point(10, 10);
            lblLog.Size = new Size(200, 25);
            lblLog.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnClearLog = new Button();
            btnClearLog.Text = "Xóa log";
            btnClearLog.Location = new Point(850, 8);
            btnClearLog.Size = new Size(90, 28);
            btnClearLog.BackColor = Color.FromArgb(108, 117, 125);
            btnClearLog.ForeColor = Color.White;
            btnClearLog.FlatStyle = FlatStyle.Flat;
            btnClearLog.Font = new Font("Segoe UI", 9F);
            btnClearLog.Click += BtnClearLog_Click;

            txtLog = new TextBox();
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Location = new Point(10, 40);
            txtLog.Size = new Size(940, 440);
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.BackColor = Color.FromArgb(30, 30, 30);
            txtLog.ForeColor = Color.LightGreen;
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            tabLog.Controls.Add(lblLog);
            tabLog.Controls.Add(btnClearLog);
            tabLog.Controls.Add(txtLog);

            // Tab 2: Web Browser
            tabWeb = new TabPage("Trang đặt vé");
            tabWeb.BackColor = Color.White;

            webBrowser = new WebBrowser();
            webBrowser.Dock = DockStyle.Fill;
            webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.IsWebBrowserContextMenuEnabled = true;
            webBrowser.WebBrowserShortcutsEnabled = true;

            tabWeb.Controls.Add(webBrowser);

            tabControl.TabPages.Add(tabLog);
            tabControl.TabPages.Add(tabWeb);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlConnection);
            this.Controls.Add(tabControl);

            AddLog("Web Client đã sẵn sàng. Nhập IP và Port server để kết nối.");
            AddLog("Sau khi kết nối, click 'Mở trang đặt vé' để truy cập trang booking.");
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                Disconnect();
                return;
            }

            try
            {
                string ipAddress = txtServerIP.Text.Trim();
                string portText = txtServerPort.Text.Trim();

                if (string.IsNullOrEmpty(ipAddress))
                {
                    MessageBox.Show("Vui lòng nhập địa chỉ IP server!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(portText))
                {
                    MessageBox.Show("Vui lòng nhập port server!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(portText, out int port) || port < 1 || port > 65535)
                {
                    MessageBox.Show("Port không hợp lệ! Port phải là số từ 1 đến 65535.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                IPAddress ip;
                if (ipAddress == "127.0.0.1" || ipAddress == "localhost")
                {
                    ip = IPAddress.Loopback;
                }
                else
                {
                    ip = IPAddress.Parse(ipAddress);
                    if (ip.AddressFamily != AddressFamily.InterNetwork)
                    {
                        throw new Exception($"Địa chỉ IP '{ipAddress}' không phải là IPv4.");
                    }
                }

                tcpClient = new TcpClient(AddressFamily.InterNetwork);

                Exception? connectException = null;
                var connectTask = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        tcpClient.Connect(ip, port);
                    }
                    catch (Exception ex)
                    {
                        connectException = ex;
                    }
                });

                bool completed = connectTask.Wait(TimeSpan.FromSeconds(5));

                if (!completed || connectException != null)
                {
                    try
                    {
                        if (tcpClient != null && tcpClient.Connected)
                        {
                            tcpClient.Close();
                        }
                    }
                    catch { }
                    tcpClient = null;

                    if (connectException != null)
                    {
                        throw new Exception($"Không thể kết nối đến server: {connectException.Message}");
                    }
                    else
                    {
                        throw new Exception("Timeout: Không thể kết nối đến server trong 5 giây.");
                    }
                }

                if (tcpClient == null || !tcpClient.Connected)
                {
                    try
                    {
                        if (tcpClient != null)
                        {
                            tcpClient.Close();
                        }
                    }
                    catch { }
                    tcpClient = null;
                    throw new Exception("Kết nối thất bại!");
                }

                networkStream = tcpClient.GetStream();
                networkStream.ReadTimeout = 30000;
                networkStream.WriteTimeout = 5000;

                isConnected = true;
                btnConnect.Text = "Ngắt kết nối";
                btnConnect.BackColor = Color.FromArgb(220, 53, 69);
                txtServerIP.Enabled = false;
                txtServerPort.Enabled = false;
                btnOpenWeb.Enabled = true;
                lblStatus.Text = $"Trạng thái: Đã kết nối đến {ipAddress}:{port}";
                lblStatus.ForeColor = Color.FromArgb(40, 167, 69);

                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                AddLog($"✓ Đã kết nối đến server {ipAddress}:{port}");
                
                // Test connection by sending a message
                SendMessage("GET_MOVIES|");

                // Tự động mở trang web khi kết nối thành công
                LoadBookingPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}\n\nĐảm bảo Web Server đã được khởi động và TCP server đang chạy.", 
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLog($"✗ Lỗi kết nối: {ex.Message}");

                try
                {
                    if (tcpClient != null)
                    {
                        tcpClient.Close();
                        tcpClient = null;
                    }
                    if (networkStream != null)
                    {
                        networkStream.Close();
                        networkStream = null;
                    }
                }
                catch { }
            }
        }

        private void Disconnect()
        {
            isConnected = false;
            try
            {
                if (networkStream != null)
                {
                    networkStream.Close();
                }
                if (tcpClient != null)
                {
                    tcpClient.Close();
                }
            }
            catch { }

            btnConnect.Text = "Kết nối";
            btnConnect.BackColor = Color.FromArgb(40, 167, 69);
            txtServerIP.Enabled = true;
            txtServerPort.Enabled = true;
            btnOpenWeb.Enabled = false;
            lblStatus.Text = "Trạng thái: Chưa kết nối";
            lblStatus.ForeColor = Color.FromArgb(220, 53, 69);
            AddLog("Đã ngắt kết nối");
        }

        private void SendMessage(string message)
        {
            try
            {
                if (isConnected && networkStream != null)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    networkStream.Write(data, 0, data.Length);
                    AddLog($"[Gửi] {message}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"[Lỗi gửi] {ex.Message}");
                Disconnect();
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[4096];
            StringBuilder messageBuilder = new StringBuilder();

            while (isConnected && tcpClient != null && tcpClient.Connected)
            {
                try
                {
                    int bytesRead = networkStream!.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(data);

                    while (messageBuilder.ToString().Contains("\n"))
                    {
                        int newlineIndex = messageBuilder.ToString().IndexOf("\n");
                        string message = messageBuilder.ToString().Substring(0, newlineIndex);
                        messageBuilder.Remove(0, newlineIndex + 1);

                        ProcessServerMessage(message);
                    }
                }
                catch (System.IO.IOException ioEx)
                {
                    if (ioEx.InnerException is SocketException)
                    {
                        break;
                    }
                    if (ioEx.Message.Contains("timed out") || ioEx.Message.Contains("timeout"))
                    {
                        if (tcpClient == null || !tcpClient.Connected)
                        {
                            break;
                        }
                        continue;
                    }
                    break;
                }
                catch (SocketException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AddLog($"[Lỗi nhận] {ex.Message}");
                    break;
                }
            }

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (isConnected)
                    {
                        AddLog("Mất kết nối với server");
                        Disconnect();
                    }
                }));
            }
        }

        private void ProcessServerMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(ProcessServerMessage), message);
                return;
            }

            AddLog($"[Nhận] {message}");

            string[] parts = message.Split('|');
            if (parts.Length == 0) return;

            string command = parts[0];

            switch (command)
            {
                case "MOVIES":
                    if (parts.Length > 1)
                    {
                        AddLog($"✓ Nhận danh sách phim: {parts[1]}");
                    }
                    break;

                case "ROOMS":
                    if (parts.Length > 1)
                    {
                        AddLog($"✓ Nhận danh sách phòng: {parts[1]}");
                    }
                    break;

                case "SEATS":
                    if (parts.Length > 1)
                    {
                        AddLog($"✓ Nhận trạng thái ghế: {parts[1]}");
                    }
                    break;

                case "BOOK_SUCCESS":
                    AddLog("✓ Đặt vé thành công!");
                    break;

                case "BOOK_ERROR":
                    if (parts.Length > 1)
                    {
                        AddLog($"✗ Lỗi đặt vé: {parts[1]}");
                    }
                    break;

                case "UPDATE_SEATS":
                    if (parts.Length > 2)
                    {
                        AddLog($"🔄 Cập nhật ghế: Phòng {parts[1]}, Ghế {parts[2]}");
                    }
                    break;

                case "ERROR":
                    if (parts.Length > 1)
                    {
                        AddLog($"✗ Lỗi: {parts[1]}");
                    }
                    break;
            }
        }

        private void AddLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(AddLog), message);
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void BtnOpenWeb_Click(object sender, EventArgs e)
        {
            LoadBookingPage();
            // Chuyển sang tab web
            tabControl.SelectedTab = tabWeb;
        }

        private void LoadBookingPage()
        {
            try
            {
                string serverIP = txtServerIP.Text.Trim();
                if (serverIP == "localhost" || serverIP == "127.0.0.1")
                {
                    serverIP = "127.0.0.1";
                }

                string url = $"http://{serverIP}:{HTTP_PORT}/Viewing.html";
                
                if (webBrowser != null)
                {
                    webBrowser.Navigate(url);
                    AddLog($"Đang tải trang đặt vé: {url}");
                }
                else
                {
                    // Fallback: mở browser ngoài
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                    AddLog($"Đã mở browser: {url}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi khi mở trang web: {ex.Message}");
                MessageBox.Show($"Lỗi khi mở trang web: {ex.Message}\n\nĐảm bảo HTTP Server đã được khởi động trên port {HTTP_PORT}.", 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isConnected)
            {
                DialogResult result = MessageBox.Show(
                    "Bạn đang kết nối đến server. Bạn có muốn ngắt kết nối và đóng cửa sổ?",
                    "Xác nhận đóng Client",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Disconnect();
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnFormClosing(e);
        }
    }
}

