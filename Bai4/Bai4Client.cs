using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bai4
{
    public partial class Bai4Client : Form
    {
        // UI Components
        private Panel pnlHeader;
        private Label lblTitle;
        private Panel pnlMain;
        private Panel pnlCinema;
        private Label lblScreen;
        private Button[,] seatButtons;
        private Panel pnlCustomerInfo;
        private TextBox txtCustomerName;
        private ComboBox cmbMovies;
        private ComboBox cmbRooms;
        private Label lblCustomerName;
        private Label lblMovies;
        private Label lblRoom;
        private Panel pnlSeatInfo;
        private Label lblSelectedSeats;
        private Label lblTotalPrice;
        private Button btnBook;
        private Button btnClear;
        private Button btnExit;
        private Button btnConnect;
        private TextBox txtServerIP;
        private TextBox txtServerPort;
        private Label lblServerIP;
        private Label lblServerPort;
        private ToolTip toolTip;

        // Data
        private const int ROWS = 3;
        private const int COLS = 5;
        private Dictionary<string, double> movieBasePrices = new Dictionary<string, double>();
        private Dictionary<string, string> seatType = new Dictionary<string, string>();
        private Dictionary<string, double> priceMultiplier = new Dictionary<string, double>();
        private Dictionary<string, bool> seatBookedStatus = new Dictionary<string, bool>();
        private List<string> selectedSeats = new List<string>();
        private string currentSelectedRoom = "";
        private string currentSelectedMovie = "";

        // Network
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private Thread receiveThread;
        private bool isConnected = false;

        public Bai4Client()
        {
            InitializeData();
            InitializeComponent();
        }

        private void InitializeData()
        {
            // Initialize seat types
            string[] vatSeats = { "A1", "A5", "C1", "C5" };
            string[] normalSeats = { "A2", "A3", "A4", "C2", "C3", "C4" };
            string[] vipSeats = { "B1", "B2", "B3", "B4", "B5" };

            foreach (string seat in vatSeats)
            {
                seatType[seat] = "Vé vớt";
            }
            foreach (string seat in normalSeats)
            {
                seatType[seat] = "Vé thường";
            }
            foreach (string seat in vipSeats)
            {
                seatType[seat] = "Vé VIP";
            }

            priceMultiplier["Vé vớt"] = 0.25;
            priceMultiplier["Vé thường"] = 1.0;
            priceMultiplier["Vé VIP"] = 2.0;

            // Initialize seat status
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    string seatName = GetSeatName(row, col);
                    seatBookedStatus[seatName] = false;
                }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Bai4 - TCP Client (Đặt vé rạp phim)";
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Font = new Font("Segoe UI", 10F);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            // Header
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.BackColor = Color.FromArgb(220, 53, 69);
            pnlHeader.Height = 90;
            pnlHeader.Padding = new Padding(0);

            lblTitle = new Label();
            lblTitle.Text = "HỆ THỐNG ĐẶT VÉ RẠP PHIM";
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = false;
            lblTitle.Size = new Size(900, 90);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblTitle);

            // Connection panel
            Panel pnlConnection = new Panel();
            pnlConnection.Size = new Size(860, 50);
            pnlConnection.Location = new Point(20, 100);
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
            txtServerPort.Text = "8080";
            txtServerPort.Location = new Point(340, 12);
            txtServerPort.Size = new Size(80, 30);

            btnConnect = new Button();
            btnConnect.Text = "Kết nối";
            btnConnect.Location = new Point(440, 10);
            btnConnect.Size = new Size(100, 30);
            btnConnect.BackColor = Color.FromArgb(40, 167, 69);
            btnConnect.ForeColor = Color.White;
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnConnect.Click += BtnConnect_Click;

            pnlConnection.Controls.Add(lblServerIP);
            pnlConnection.Controls.Add(txtServerIP);
            pnlConnection.Controls.Add(lblServerPort);
            pnlConnection.Controls.Add(txtServerPort);
            pnlConnection.Controls.Add(btnConnect);

            // Main panel
            pnlMain = new Panel();
            pnlMain.Size = new Size(860, 600);
            pnlMain.Location = new Point(20, 160);
            pnlMain.BackColor = Color.Transparent;

            // Cinema panel
            pnlCinema = new Panel();
            pnlCinema.Size = new Size(420, 320);
            pnlCinema.Location = new Point(0, 0);
            pnlCinema.BackColor = Color.White;
            pnlCinema.BorderStyle = BorderStyle.FixedSingle;

            lblScreen = new Label();
            lblScreen.Text = "MÀN HÌNH";
            lblScreen.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblScreen.ForeColor = Color.White;
            lblScreen.BackColor = Color.FromArgb(52, 58, 64);
            lblScreen.Size = new Size(360, 40);
            lblScreen.Location = new Point(30, 20);
            lblScreen.TextAlign = ContentAlignment.MiddleCenter;
            pnlCinema.Controls.Add(lblScreen);

            CreateSeatGrid();

            // Customer info panel
            pnlCustomerInfo = new Panel();
            pnlCustomerInfo.Size = new Size(420, 240);
            pnlCustomerInfo.Location = new Point(440, 0);
            pnlCustomerInfo.BackColor = Color.White;
            pnlCustomerInfo.BorderStyle = BorderStyle.FixedSingle;

            Label lblInfoTitle = new Label();
            lblInfoTitle.Text = "THÔNG TIN KHÁCH HÀNG";
            lblInfoTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblInfoTitle.ForeColor = Color.FromArgb(52, 58, 64);
            lblInfoTitle.Size = new Size(370, 30);
            lblInfoTitle.Location = new Point(25, 15);
            pnlCustomerInfo.Controls.Add(lblInfoTitle);

            lblCustomerName = new Label();
            lblCustomerName.Text = "Họ và tên";
            lblCustomerName.Size = new Size(370, 22);
            lblCustomerName.Location = new Point(25, 55);
            lblCustomerName.Font = new Font("Segoe UI", 10F);

            txtCustomerName = new TextBox();
            txtCustomerName.Size = new Size(370, 30);
            txtCustomerName.Location = new Point(25, 80);
            txtCustomerName.Font = new Font("Segoe UI", 11F);
            txtCustomerName.BorderStyle = BorderStyle.FixedSingle;
            txtCustomerName.TextChanged += txtCustomerName_TextChanged;

            lblMovies = new Label();
            lblMovies.Text = "Tên phim";
            lblMovies.Size = new Size(370, 22);
            lblMovies.Location = new Point(25, 120);
            lblMovies.Font = new Font("Segoe UI", 10F);

            cmbMovies = new ComboBox();
            cmbMovies.Size = new Size(370, 30);
            cmbMovies.Location = new Point(25, 145);
            cmbMovies.Font = new Font("Segoe UI", 11F);
            cmbMovies.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMovies.Enabled = false;
            cmbMovies.SelectedIndexChanged += CmbMovies_SelectedIndexChanged;

            lblRoom = new Label();
            lblRoom.Text = "Phòng chiếu";
            lblRoom.Size = new Size(370, 22);
            lblRoom.Location = new Point(25, 185);
            lblRoom.Font = new Font("Segoe UI", 10F);

            cmbRooms = new ComboBox();
            cmbRooms.Size = new Size(370, 30);
            cmbRooms.Location = new Point(25, 210);
            cmbRooms.Font = new Font("Segoe UI", 11F);
            cmbRooms.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRooms.Enabled = false;
            cmbRooms.SelectedIndexChanged += CmbRooms_SelectedIndexChanged;

            pnlCustomerInfo.Controls.Add(lblCustomerName);
            pnlCustomerInfo.Controls.Add(txtCustomerName);
            pnlCustomerInfo.Controls.Add(lblMovies);
            pnlCustomerInfo.Controls.Add(cmbMovies);
            pnlCustomerInfo.Controls.Add(lblRoom);
            pnlCustomerInfo.Controls.Add(cmbRooms);

            // Seat info panel
            pnlSeatInfo = new Panel();
            pnlSeatInfo.Size = new Size(420, 130);
            pnlSeatInfo.Location = new Point(440, 260);
            pnlSeatInfo.BackColor = Color.White;
            pnlSeatInfo.BorderStyle = BorderStyle.FixedSingle;

            Label lblSeatTitle = new Label();
            lblSeatTitle.Text = "THÔNG TIN VÉ";
            lblSeatTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSeatTitle.ForeColor = Color.FromArgb(52, 58, 64);
            lblSeatTitle.Size = new Size(370, 30);
            lblSeatTitle.Location = new Point(25, 15);
            pnlSeatInfo.Controls.Add(lblSeatTitle);

            lblSelectedSeats = new Label();
            lblSelectedSeats.Text = "Ghế đã chọn: ";
            lblSelectedSeats.Size = new Size(370, 25);
            lblSelectedSeats.Location = new Point(25, 55);
            lblSelectedSeats.Font = new Font("Segoe UI", 10F);

            lblTotalPrice = new Label();
            lblTotalPrice.Text = "Tổng tiền: 0 VNĐ";
            lblTotalPrice.Size = new Size(370, 30);
            lblTotalPrice.Location = new Point(25, 85);
            lblTotalPrice.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTotalPrice.ForeColor = Color.FromArgb(220, 53, 69);

            pnlSeatInfo.Controls.Add(lblSelectedSeats);
            pnlSeatInfo.Controls.Add(lblTotalPrice);

            CreateLegend();

            // Buttons panel
            Panel pnlButtons = new Panel();
            pnlButtons.Size = new Size(860, 60);
            pnlButtons.Location = new Point(0, 490); // Di chuyển panel lên trên (từ 530 xuống 510)
            pnlButtons.BackColor = Color.Transparent;

            // Tính toán vị trí để căn giữa các button
            int buttonWidth = 140;
            int buttonHeight = 45;
            int buttonSpacing = 20; // Khoảng cách giữa các button
            int totalButtonWidth = buttonWidth * 3; // 3 button
            int totalSpacing = buttonSpacing * 2; // 2 khoảng cách giữa 3 button
            int totalWidth = totalButtonWidth + totalSpacing;
            int startX = (pnlButtons.Width - totalWidth) / 2; // Căn giữa
            int startY = 5; // Đặt button gần phía trên của panel để hiển thị đầy đủ

            btnBook = new Button();
            btnBook.Text = "Đặt Vé";
            btnBook.Size = new Size(buttonWidth, buttonHeight);
            btnBook.Location = new Point(startX, startY);
            btnBook.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnBook.BackColor = Color.FromArgb(40, 167, 69);
            btnBook.ForeColor = Color.White;
            btnBook.FlatStyle = FlatStyle.Flat;
            btnBook.FlatAppearance.BorderSize = 0;
            btnBook.Cursor = Cursors.Hand;
            btnBook.Enabled = false;
            btnBook.Click += BtnBook_Click;

            btnClear = new Button();
            btnClear.Text = "Xóa";
            btnClear.Size = new Size(buttonWidth, buttonHeight);
            btnClear.Location = new Point(startX + buttonWidth + buttonSpacing, startY);
            btnClear.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnClear.BackColor = Color.FromArgb(255, 193, 7);
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Cursor = Cursors.Hand;
            btnClear.Click += BtnClear_Click;

            btnExit = new Button();
            btnExit.Text = "Thoát";
            btnExit.Size = new Size(buttonWidth, buttonHeight);
            btnExit.Location = new Point(startX + (buttonWidth + buttonSpacing) * 2, startY);
            btnExit.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnExit.BackColor = Color.FromArgb(108, 117, 125);
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += BtnExit_Click;

            pnlButtons.Controls.Add(btnBook);
            pnlButtons.Controls.Add(btnClear);
            pnlButtons.Controls.Add(btnExit);

            pnlMain.Controls.Add(pnlCinema);
            pnlMain.Controls.Add(pnlCustomerInfo);
            pnlMain.Controls.Add(pnlSeatInfo);
            pnlMain.Controls.Add(pnlButtons);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlConnection);
            this.Controls.Add(pnlMain);

            toolTip = new ToolTip();
        }

        private void CreateSeatGrid()
        {
            int startY = 75; // Di chuyển các nút ghế lên trên nhiều hơn
            int startX = 50;
            int buttonSize = 50;
            int spacing = 12;

            seatButtons = new Button[ROWS, COLS];

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    Button btn = new Button();
                    btn.Size = new Size(buttonSize, buttonSize);
                    btn.Location = new Point(startX + col * (buttonSize + spacing),
                                            startY + row * (buttonSize + spacing));

                    string seatName = GetSeatName(row, col);
                    btn.Text = seatName;
                    btn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                    btn.Tag = seatName;
                    btn.Enabled = false;

                    RestoreSeatColor(btn, seatName);

                    btn.Click += SeatButton_Click;
                    seatButtons[row, col] = btn;
                    pnlCinema.Controls.Add(btn);
                }
            }
        }

        private void CreateLegend()
        {
            Panel pnlLegend = new Panel();
            pnlLegend.Size = new Size(420, 120);
            pnlLegend.Location = new Point(0, 340);
            pnlLegend.BackColor = Color.White;
            pnlLegend.BorderStyle = BorderStyle.FixedSingle;

            Label lblLegendTitle = new Label();
            lblLegendTitle.Text = "CHÚ THÍCH";
            lblLegendTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblLegendTitle.ForeColor = Color.FromArgb(52, 58, 64);
            lblLegendTitle.Size = new Size(370, 25);
            lblLegendTitle.Location = new Point(25, 10);
            pnlLegend.Controls.Add(lblLegendTitle);

            int legendY = 42;
            int legendX = 30;

            // Vé vớt
            Panel vatPanel = new Panel();
            vatPanel.BackColor = Color.FromArgb(255, 193, 7);
            vatPanel.Location = new Point(legendX, legendY);
            vatPanel.Size = new Size(30, 20);
            pnlLegend.Controls.Add(vatPanel);

            Label lblVat = new Label();
            lblVat.Text = "Vé vớt (1/4 giá)";
            lblVat.Location = new Point(legendX + 38, legendY - 2);
            lblVat.Size = new Size(160, 25);
            lblVat.Font = new Font("Segoe UI", 9.5F);
            pnlLegend.Controls.Add(lblVat);

            // Vé VIP
            Panel vipPanel = new Panel();
            vipPanel.BackColor = Color.FromArgb(220, 53, 69);
            vipPanel.Location = new Point(legendX + 200, legendY);
            vipPanel.Size = new Size(30, 20);
            pnlLegend.Controls.Add(vipPanel);

            Label lblVip = new Label();
            lblVip.Text = "Vé VIP (2x giá)";
            lblVip.Location = new Point(legendX + 238, legendY - 2);
            lblVip.Size = new Size(160, 25);
            lblVip.Font = new Font("Segoe UI", 9.5F);
            pnlLegend.Controls.Add(lblVip);

            // Vé thường
            Panel normalPanel = new Panel();
            normalPanel.BackColor = Color.FromArgb(40, 167, 69);
            normalPanel.Location = new Point(legendX, legendY + 28);
            normalPanel.Size = new Size(30, 20);
            pnlLegend.Controls.Add(normalPanel);

            Label lblNormal = new Label();
            lblNormal.Text = "Vé thường (1x)";
            lblNormal.Location = new Point(legendX + 38, legendY + 26);
            lblNormal.Size = new Size(160, 25);
            lblNormal.Font = new Font("Segoe UI", 9.5F);
            pnlLegend.Controls.Add(lblNormal);

            // Đã chọn
            Panel selectedPanel = new Panel();
            selectedPanel.BackColor = Color.FromArgb(108, 117, 125);
            selectedPanel.Location = new Point(legendX + 200, legendY + 28);
            selectedPanel.Size = new Size(30, 20);
            pnlLegend.Controls.Add(selectedPanel);

            Label lblSelected = new Label();
            lblSelected.Text = "Đã chọn";
            lblSelected.Location = new Point(legendX + 238, legendY + 26);
            lblSelected.Size = new Size(160, 25);
            lblSelected.Font = new Font("Segoe UI", 9.5F);
            pnlLegend.Controls.Add(lblSelected);

            // Đã đặt
            Panel bookedPanel = new Panel();
            bookedPanel.BackColor = Color.FromArgb(169, 169, 169);
            bookedPanel.Location = new Point(legendX, legendY + 56);
            bookedPanel.Size = new Size(30, 20);
            pnlLegend.Controls.Add(bookedPanel);

            Label lblBooked = new Label();
            lblBooked.Text = "Đã đặt";
            lblBooked.Location = new Point(legendX + 38, legendY + 54);
            lblBooked.Size = new Size(160, 25);
            lblBooked.Font = new Font("Segoe UI", 9.5F);
            pnlLegend.Controls.Add(lblBooked);

            pnlMain.Controls.Add(pnlLegend);
        }

        private string GetSeatName(int row, int col)
        {
            char rowChar = (char)('A' + row);
            return $"{rowChar}{col + 1}";
        }

        private void RestoreSeatColor(Button btn, string seatName)
        {
            if (seatBookedStatus.ContainsKey(seatName) && seatBookedStatus[seatName])
            {
                btn.BackColor = Color.FromArgb(169, 169, 169);
                btn.Enabled = false;
                btn.ForeColor = Color.White;
            }
            else
            {
                string ticketType = seatType.ContainsKey(seatName) ? seatType[seatName] : "Vé thường";
                switch (ticketType)
                {
                    case "Vé vớt":
                        btn.BackColor = Color.FromArgb(255, 193, 7);
                        break;
                    case "Vé thường":
                        btn.BackColor = Color.FromArgb(40, 167, 69);
                        break;
                    case "Vé VIP":
                        btn.BackColor = Color.FromArgb(220, 53, 69);
                        break;
                }
                btn.ForeColor = Color.White;
            }
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
                
                // Kiểm tra IP hợp lệ
                if (string.IsNullOrEmpty(ipAddress))
                {
                    MessageBox.Show("Vui lòng nhập địa chỉ IP server!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra Port hợp lệ
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

                // Đảm bảo chỉ dùng IPv4
                IPAddress ip;
                if (ipAddress == "127.0.0.1" || ipAddress == "localhost")
                {
                    // Sử dụng Loopback để đảm bảo IPv4
                    ip = IPAddress.Loopback; // Tương đương 127.0.0.1 IPv4
                }
                else
                {
                    ip = IPAddress.Parse(ipAddress);
                    // Kiểm tra đảm bảo là IPv4
                    if (ip.AddressFamily != AddressFamily.InterNetwork)
                    {
                        throw new Exception($"Địa chỉ IP '{ipAddress}' không phải là IPv4. Vui lòng sử dụng IPv4 address (ví dụ: 127.0.0.1)");
                    }
                }
                
                // Tạo TcpClient với IPv4 chỉ định
                tcpClient = new TcpClient(AddressFamily.InterNetwork);
                
                // Kết nối với timeout sử dụng Task
                Exception? connectException = null;
                var connectTask = Task.Run(() =>
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
                
                // Đợi kết nối với timeout 5 giây
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
                    tcpClient = null!;
                    
                    if (connectException != null)
                    {
                        throw new Exception($"Không thể kết nối đến server: {connectException.Message}\n\nVui lòng kiểm tra:\n1. Server đã được khởi động và click 'Listen' chưa?\n2. IP address có đúng không? ({ipAddress})\n3. Port {port} có bị chặn bởi Firewall không?");
                    }
                    else
                    {
                        throw new Exception($"Timeout: Không thể kết nối đến server trong 5 giây.\n\nVui lòng kiểm tra:\n1. Server đã được khởi động và click 'Listen' chưa?\n2. IP address có đúng không? ({ipAddress})\n3. Port {port} có bị chặn bởi Firewall không?");
                    }
                }
                
                // Kiểm tra kết nối thành công
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
                    tcpClient = null!;
                    throw new Exception("Kết nối thất bại! Vui lòng kiểm tra server đã sẵn sàng chưa.");
                }
                
                // Set timeout cho stream - tăng timeout để tránh mất kết nối không cần thiết
                networkStream = tcpClient.GetStream();
                networkStream.ReadTimeout = 30000; // 30 giây thay vì 5 giây
                networkStream.WriteTimeout = 5000;

                isConnected = true;
                btnConnect.Text = "Ngắt kết nối";
                btnConnect.BackColor = Color.FromArgb(220, 53, 69);
                txtServerIP.Enabled = false;
                txtServerPort.Enabled = false;
                cmbMovies.Enabled = true;

                // Start receive thread
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                // Request movies
                SendMessage("GET_MOVIES|");
            }
            catch (Exception ex)
            {
                string portText = txtServerPort.Text.Trim();
                int port = 8080;
                int.TryParse(portText, out port);
                MessageBox.Show($"Lỗi kết nối: {ex.Message}\n\nĐảm bảo server đã được khởi động và đang lắng nghe trên port {port}.", 
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Cleanup nếu kết nối thất bại
                try
                {
                    if (tcpClient != null)
                    {
                        tcpClient.Close();
                        tcpClient = null!;
                    }
                    if (networkStream != null)
                    {
                        networkStream.Close();
                        networkStream = null!;
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
            cmbMovies.Enabled = false;
            cmbRooms.Enabled = false;
            btnBook.Enabled = false;
            cmbMovies.Items.Clear();
            cmbRooms.Items.Clear();
        }

        private void SendMessage(string message)
        {
            try
            {
                if (isConnected && networkStream != null)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                    networkStream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi tin nhắn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
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
                    // Kiểm tra nếu là timeout - không nên break khi timeout
                    if (ioEx.InnerException is SocketException socketEx)
                    {
                        // SocketException thường xảy ra khi mất kết nối thực sự
                        break;
                    }
                    // Nếu là timeout (ReadTimeout), tiếp tục vòng lặp
                    if (ioEx.Message.Contains("timed out") || ioEx.Message.Contains("timeout"))
                    {
                        // Kiểm tra kết nối còn sống không
                        if (tcpClient == null || !tcpClient.Connected)
                        {
                            break;
                        }
                        // Nếu vẫn còn kết nối, tiếp tục đợi
                        continue;
                    }
                    // Các IOException khác - có thể là mất kết nối
                    break;
                }
                catch (SocketException)
                {
                    // SocketException thường xảy ra khi mất kết nối
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // Stream hoặc client đã bị dispose
                    break;
                }
                catch (Exception)
                {
                    // Các exception khác - break để kiểm tra kết nối
                    break;
                }
            }

            // Chỉ hiển thị thông báo mất kết nối nếu thực sự đang kết nối
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (isConnected)
                    {
                        MessageBox.Show("Mất kết nối với server", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            string[] parts = message.Split('|');
            if (parts.Length == 0) return;

            string command = parts[0];

            switch (command)
            {
                case "MOVIES":
                    if (parts.Length > 1)
                    {
                        LoadMovies(parts[1]);
                    }
                    break;

                case "ROOMS":
                    if (parts.Length > 1)
                    {
                        LoadRooms(parts[1]);
                    }
                    break;

                case "SEATS":
                    if (parts.Length > 1)
                    {
                        LoadSeats(parts[1]);
                    }
                    break;

                case "BOOK_SUCCESS":
                    MessageBox.Show("Đặt vé thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BtnClear_Click(null, null);
                    // Refresh seats
                    if (!string.IsNullOrEmpty(currentSelectedMovie) && !string.IsNullOrEmpty(currentSelectedRoom))
                    {
                        SendMessage($"GET_SEATS|{currentSelectedMovie}|{currentSelectedRoom}");
                    }
                    break;

                case "BOOK_ERROR":
                    if (parts.Length > 1)
                    {
                        MessageBox.Show($"Lỗi đặt vé: {parts[1]}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;

                case "UPDATE_SEATS":
                    if (parts.Length > 2)
                    {
                        UpdateSeats(parts[1], parts[2]);
                    }
                    break;

                case "ERROR":
                    if (parts.Length > 1)
                    {
                        MessageBox.Show($"Lỗi: {parts[1]}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
            }
        }

        private void LoadMovies(string data)
        {
            cmbMovies.Items.Clear();
            movieBasePrices.Clear();

            string[] movies = data.Split(';');
            foreach (string movie in movies)
            {
                if (string.IsNullOrEmpty(movie)) continue;
                string[] parts = movie.Split(':');
                if (parts.Length == 2)
                {
                    string name = parts[0];
                    double price = double.Parse(parts[1]);
                    cmbMovies.Items.Add(name);
                    movieBasePrices[name] = price;
                }
            }
        }

        private void LoadRooms(string data)
        {
            cmbRooms.Items.Clear();
            string[] rooms = data.Split(';');
            foreach (string room in rooms)
            {
                if (!string.IsNullOrEmpty(room))
                {
                    cmbRooms.Items.Add(room);
                }
            }
            cmbRooms.Enabled = true;
            if (cmbRooms.Items.Count > 0)
            {
                cmbRooms.SelectedIndex = 0;
            }
        }

        private void LoadSeats(string data)
        {
            // Reset seat status
            foreach (string seatName in seatBookedStatus.Keys.ToList())
            {
                seatBookedStatus[seatName] = false;
            }

            string[] seats = data.Split(';');
            foreach (string seat in seats)
            {
                if (string.IsNullOrEmpty(seat)) continue;
                string[] parts = seat.Split(':');
                if (parts.Length == 3)
                {
                    string seatName = parts[0];
                    bool isBooked = parts[2] == "1";
                    if (seatBookedStatus.ContainsKey(seatName))
                    {
                        seatBookedStatus[seatName] = isBooked;
                    }
                }
            }

            UpdateSeatDisplay();
        }

        private void UpdateSeats(string roomName, string seatNamesStr)
        {
            if (roomName != currentSelectedRoom) return;

            string[] seatNames = seatNamesStr.Split(',');
            foreach (string seatName in seatNames)
            {
                if (seatBookedStatus.ContainsKey(seatName))
                {
                    seatBookedStatus[seatName] = true;
                }
            }

            UpdateSeatDisplay();
        }

        private void UpdateSeatDisplay()
        {
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    string seatName = GetSeatName(row, col);
                    Button btn = seatButtons[row, col];

                    if (seatBookedStatus.ContainsKey(seatName) && seatBookedStatus[seatName])
                    {
                        btn.BackColor = Color.FromArgb(169, 169, 169);
                        btn.Enabled = false;
                    }
                    else if (!selectedSeats.Contains(seatName))
                    {
                        btn.Enabled = isConnected && !string.IsNullOrEmpty(currentSelectedRoom);
                        RestoreSeatColor(btn, seatName);
                    }
                }
            }
        }

        private void CmbMovies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMovies.SelectedIndex == -1) return;

            currentSelectedMovie = cmbMovies.SelectedItem.ToString();
            cmbRooms.Items.Clear();
            cmbRooms.Enabled = false;
            ClearCurrentRoomSeats();
            SendMessage($"GET_ROOMS|{currentSelectedMovie}");
        }

        private void CmbRooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRooms.SelectedIndex == -1) return;

            currentSelectedRoom = cmbRooms.SelectedItem.ToString();
            ClearCurrentRoomSeats();
            SendMessage($"GET_SEATS|{currentSelectedMovie}|{currentSelectedRoom}");
        }

        private void ClearCurrentRoomSeats()
        {
            selectedSeats.Clear();
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    string seatName = GetSeatName(row, col);
                    Button btn = seatButtons[row, col];
                    if (!seatBookedStatus[seatName])
                    {
                        RestoreSeatColor(btn, seatName);
                    }
                }
            }
            UpdateSeatInfo();
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            if (!isConnected || string.IsNullOrEmpty(currentSelectedRoom))
            {
                MessageBox.Show("Vui lòng kết nối và chọn phòng trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Button btn = sender as Button;
            string seatName = btn.Tag.ToString();

            if (seatBookedStatus.ContainsKey(seatName) && seatBookedStatus[seatName])
            {
                MessageBox.Show($"Ghế {seatName} đã được đặt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedSeats.Contains(seatName))
            {
                selectedSeats.Remove(seatName);
                RestoreSeatColor(btn, seatName);
            }
            else
            {
                selectedSeats.Add(seatName);
                btn.BackColor = Color.FromArgb(108, 117, 125);
            }

            UpdateSeatInfo();
        }

        private void UpdateSeatInfo()
        {
            lblSelectedSeats.Text = $"Ghế đã chọn: {string.Join(", ", selectedSeats)}";

            double total = 0;
            if (cmbMovies.SelectedIndex != -1 && selectedSeats.Count > 0)
            {
                string selectedMovie = cmbMovies.SelectedItem.ToString();
                if (movieBasePrices.ContainsKey(selectedMovie))
                {
                    double basePrice = movieBasePrices[selectedMovie];
                    foreach (string seat in selectedSeats)
                    {
                        if (seatType.ContainsKey(seat))
                        {
                            string ticketType = seatType[seat];
                            total += basePrice * priceMultiplier[ticketType];
                        }
                    }
                }
            }

            lblTotalPrice.Text = $"Tổng tiền: {total:N0} VNĐ";
            btnBook.Enabled = isConnected && selectedSeats.Count > 0 && !string.IsNullOrWhiteSpace(txtCustomerName.Text);
        }

        private void txtCustomerName_TextChanged(object sender, EventArgs e)
        {
            UpdateSeatInfo();
        }

        private void BtnBook_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbMovies.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn phim!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbRooms.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn phòng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedSeats.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Calculate total price
            double total = 0;
            string selectedMovie = cmbMovies.SelectedItem.ToString();
            double basePrice = movieBasePrices[selectedMovie];
            foreach (string seat in selectedSeats)
            {
                if (seatType.ContainsKey(seat))
                {
                    string ticketType = seatType[seat];
                    total += basePrice * priceMultiplier[ticketType];
                }
            }

            // Send booking request
            string seatNamesStr = string.Join(",", selectedSeats);
            SendMessage($"BOOK_SEATS|{txtCustomerName.Text}|{currentSelectedMovie}|{currentSelectedRoom}|{seatNamesStr}|{total}");
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtCustomerName.Clear();
            cmbMovies.SelectedIndex = -1;
            cmbRooms.Items.Clear();
            cmbRooms.Enabled = false;
            currentSelectedRoom = "";
            currentSelectedMovie = "";
            ClearCurrentRoomSeats();
            lblSelectedSeats.Text = "Ghế đã chọn: ";
            lblTotalPrice.Text = "Tổng tiền: 0 VNĐ";
            btnBook.Enabled = false;
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                Disconnect();
            }
            this.Close();
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
                    e.Cancel = true; // Hủy đóng form
                    return;
                }
            }
            base.OnFormClosing(e);
        }
    }
}

