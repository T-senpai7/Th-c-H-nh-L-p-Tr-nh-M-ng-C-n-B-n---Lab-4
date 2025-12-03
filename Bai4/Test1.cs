using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public class Bai5Form : Form
    {
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
        private Button btnImportData;
        private Button btnExportStats;
        private ProgressBar progressBar;
        private ToolTip toolTip;
        
        private const int ROWS = 3; // A, B, C
        private const int COLS = 5; // 1, 2, 3, 4, 5
        
        private Dictionary<string, double> movieBasePrices = new Dictionary<string, double>()
        {
            { "Đào, phở và piano", 45000 },
            { "Mai", 100000 },
            { "Gặp lại chị bầu", 70000 },
            { "Tarot", 90000 }
        };

        private Dictionary<string, List<int>> movieRooms = new Dictionary<string, List<int>>()
        {
            { "Đào, phở và piano", new List<int> { 1, 2, 3 } },
            { "Mai", new List<int> { 2, 3 } },
            { "Gặp lại chị bầu", new List<int> { 1 } },
            { "Tarot", new List<int> { 3 } }
        };
        
        private Dictionary<string, Dictionary<string, bool>> roomSeatStatus = new Dictionary<string, Dictionary<string, bool>>();
        private Dictionary<string, string> seatType = new Dictionary<string, string>();
        private Dictionary<string, double> priceMultiplier = new Dictionary<string, double>();
        private List<string> selectedSeats = new List<string>();
        private string currentSelectedRoom = "";
        
        // Dictionary để lưu thông tin thống kê theo phim
        private Dictionary<string, MovieStats> movieStatistics = new Dictionary<string, MovieStats>();
        
        // Dictionary để lưu thông tin đặt vé chi tiết
        private Dictionary<string, List<BookingInfo>> bookingHistory = new Dictionary<string, List<BookingInfo>>();
        
        // Lớp để lưu thông tin đặt vé
        private class BookingInfo
        {
            public string CustomerName { get; set; }
            public string MovieName { get; set; }
            public string RoomName { get; set; }
            public List<string> Seats { get; set; }
            public DateTime BookingTime { get; set; }
            public double TotalPrice { get; set; }
        }

        // Lớp để lưu thông tin thống kê của một phim
        private class MovieStats
        {
            public string MovieName { get; set; }
            public int TotalSeats { get; set; } 
            public int SoldSeats { get; set; } 
            public double Revenue { get; set; } 
            public int Rank { get; set; } 

            public int RemainingSeats => TotalSeats - SoldSeats; // Số ghế còn lại
            public double SoldRatio => TotalSeats > 0 ? (double)SoldSeats / TotalSeats : 0;
        }

        public Bai5Form()
        {
            InitializeData();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Lab 1 - Bài 5: Hệ Thống Đặt Vé Rạp Phim";
            this.Size = new Size(900, 730);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Font = new Font("Segoe UI", 10F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Padding = new Padding(0);

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
            lblTitle.Location = new Point(0, 0);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            pnlHeader.Controls.Add(lblTitle);

            pnlMain = new Panel();
            pnlMain.Size = new Size(860, 600);
            pnlMain.Location = new Point(20, 110);
            pnlMain.BackColor = Color.Transparent;

            pnlCinema = new Panel();
            pnlCinema.Size = new Size(420, 320);
            pnlCinema.Location = new Point(0, 0);
            pnlCinema.BackColor = Color.White;
            pnlCinema.BorderStyle = BorderStyle.None;
            pnlCinema.Padding = new Padding(15);

            
            pnlCinema.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220), 1), 
                    0, 0, pnlCinema.Width - 1, pnlCinema.Height - 1);
            };

            lblScreen = new Label();
            lblScreen.Text = "MÀN HÌNH";
            lblScreen.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblScreen.ForeColor = Color.White;
            lblScreen.BackColor = Color.FromArgb(52, 58, 64);
            lblScreen.Size = new Size(360, 40);
            lblScreen.Location = new Point(30, 20);
            lblScreen.TextAlign = ContentAlignment.MiddleCenter;

            CreateSeatGrid();

            pnlCustomerInfo = new Panel();
            pnlCustomerInfo.Size = new Size(420, 240);
            pnlCustomerInfo.Location = new Point(440, 0);
            pnlCustomerInfo.BackColor = Color.White;
            pnlCustomerInfo.BorderStyle = BorderStyle.None;
            pnlCustomerInfo.Padding = new Padding(25);

            
            pnlCustomerInfo.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220), 1), 
                    0, 0, pnlCustomerInfo.Width - 1, pnlCustomerInfo.Height - 1);
            };

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
            lblCustomerName.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            lblCustomerName.ForeColor = Color.FromArgb(73, 80, 87);

            txtCustomerName = new TextBox();
            txtCustomerName.Size = new Size(370, 30);
            txtCustomerName.Location = new Point(25, 80);
            txtCustomerName.Font = new Font("Segoe UI", 11F);
            txtCustomerName.BorderStyle = BorderStyle.FixedSingle;

            lblMovies = new Label();
            lblMovies.Text = "Tên phim";
            lblMovies.Size = new Size(370, 22);
            lblMovies.Location = new Point(25, 120);
            lblMovies.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            lblMovies.ForeColor = Color.FromArgb(73, 80, 87);

            cmbMovies = new ComboBox();
            cmbMovies.Size = new Size(370, 30);
            cmbMovies.Location = new Point(25, 145);
            cmbMovies.Font = new Font("Segoe UI", 11F);
            cmbMovies.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMovies.Items.AddRange(movieBasePrices.Keys.ToArray());
            cmbMovies.SelectedIndexChanged += CmbMovies_SelectedIndexChanged;

            lblRoom = new Label();
            lblRoom.Text = "Phòng chiếu";
            lblRoom.Size = new Size(370, 22);
            lblRoom.Location = new Point(25, 185);
            lblRoom.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            lblRoom.ForeColor = Color.FromArgb(73, 80, 87);

            cmbRooms = new ComboBox();
            cmbRooms.Size = new Size(370, 30);
            cmbRooms.Location = new Point(25, 210);
            cmbRooms.Font = new Font("Segoe UI", 11F);
            cmbRooms.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRooms.ForeColor = Color.FromArgb(108, 117, 125);
            cmbRooms.Enabled = false;
            cmbRooms.SelectedIndexChanged += CmbRooms_SelectedIndexChanged;

            pnlSeatInfo = new Panel();
            pnlSeatInfo.Size = new Size(420, 130);
            pnlSeatInfo.Location = new Point(440, 260);
            pnlSeatInfo.BackColor = Color.White;
            pnlSeatInfo.BorderStyle = BorderStyle.None;
            pnlSeatInfo.Padding = new Padding(25);

            // Thêm shadow effect - AI hỗ trợ
            pnlSeatInfo.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220), 1), 
                    0, 0, pnlSeatInfo.Width - 1, pnlSeatInfo.Height - 1);
            };

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
            lblSelectedSeats.ForeColor = Color.FromArgb(73, 80, 87);

            lblTotalPrice = new Label();
            lblTotalPrice.Text = "Tổng tiền: 0 VNĐ";
            lblTotalPrice.Size = new Size(370, 30);
            lblTotalPrice.Location = new Point(25, 85);
            lblTotalPrice.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTotalPrice.ForeColor = Color.FromArgb(220, 53, 69);

            CreateLegend();

            // Tạo panel chứa các nút và căn giữa
            Panel pnlButtons = new Panel();
            pnlButtons.Size = new Size(860, 60);
            pnlButtons.Location = new Point(0, 530);
            pnlButtons.BackColor = Color.Transparent;

            // Tính toán vị trí để căn giữa các nút
            int totalButtonWidth = 140 * 5; // 5 nút, mỗi nút rộng 140px
            int totalSpacing = 20 * 4; // 4 khoảng cách giữa các nút, mỗi khoảng 20px
            int totalWidth = totalButtonWidth + totalSpacing;
            int startX = (pnlButtons.Width - totalWidth) / 2;

            // Thiết lập vị trí cho từng nút
            btnImportData = new Button();
            btnImportData.Text = "Nhập Dữ Liệu";
            btnImportData.Size = new Size(140, 45);
            btnImportData.Location = new Point(startX, 8);
            btnImportData.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnImportData.BackColor = Color.FromArgb(13, 110, 253);
            btnImportData.ForeColor = Color.White;
            btnImportData.FlatStyle = FlatStyle.Flat;
            btnImportData.FlatAppearance.BorderSize = 0;
            btnImportData.Cursor = Cursors.Hand;
            btnImportData.Click += BtnImportData_Click;

            btnBook = new Button();
            btnBook.Text = "Đặt Vé";
            btnBook.Size = new Size(140, 45);
            btnBook.Location = new Point(startX + 140 + 20, 8); // Vị trí nút trước + độ rộng nút + khoảng cách
            btnBook.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnBook.BackColor = Color.FromArgb(40, 167, 69);
            btnBook.ForeColor = Color.White;
            btnBook.FlatStyle = FlatStyle.Flat;
            btnBook.FlatAppearance.BorderSize = 0;
            btnBook.Cursor = Cursors.Hand;
            btnBook.Click += BtnBook_Click;

            btnClear = new Button();
            btnClear.Text = "Xóa";
            btnClear.Size = new Size(140, 45);
            btnClear.Location = new Point(startX + (140 + 20) * 2, 8); // Vị trí nút trước + (độ rộng nút + khoảng cách) * 2
            btnClear.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnClear.BackColor = Color.FromArgb(255, 193, 7);
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Cursor = Cursors.Hand;
            btnClear.Click += BtnClear_Click;

            btnExit = new Button();
            btnExit.Text = "Thoát";
            btnExit.Size = new Size(140, 45);
            btnExit.Location = new Point(startX + (140 + 20) * 3, 8); // Vị trí nút trước + (độ rộng nút + khoảng cách) * 3
            btnExit.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnExit.BackColor = Color.FromArgb(108, 117, 125);
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += BtnExit_Click;

            btnExportStats = new Button();
            btnExportStats.Text = "Xuất Thống Kê";
            btnExportStats.Size = new Size(140, 45);
            btnExportStats.Location = new Point(startX + (140 + 20) * 4, 8); // Vị trí nút trước + (độ rộng nút + khoảng cách) * 4
            btnExportStats.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnExportStats.BackColor = Color.FromArgb(111, 66, 193);
            btnExportStats.ForeColor = Color.White;
            btnExportStats.FlatStyle = FlatStyle.Flat;
            btnExportStats.FlatAppearance.BorderSize = 0;
            btnExportStats.Cursor = Cursors.Hand;
            btnExportStats.Click += BtnExportStats_Click;

            // Thêm ProgressBar
            progressBar = new ProgressBar();
            progressBar.Size = new Size(860, 20);
            progressBar.Location = new Point(0, 500);
            progressBar.Visible = false;
            progressBar.Style = ProgressBarStyle.Continuous;
            
            // Khởi tạo ToolTip
            toolTip = new ToolTip();
            toolTip.IsBalloon = true;
            toolTip.ToolTipTitle = "Thông tin ghế";
            toolTip.ToolTipIcon = ToolTipIcon.Info;

            AddButtonHoverEffects();

            pnlCustomerInfo.Controls.Add(lblCustomerName);
            pnlCustomerInfo.Controls.Add(txtCustomerName);
            pnlCustomerInfo.Controls.Add(lblMovies);
            pnlCustomerInfo.Controls.Add(cmbMovies);
            pnlCustomerInfo.Controls.Add(lblRoom);
            pnlCustomerInfo.Controls.Add(cmbRooms);

            pnlSeatInfo.Controls.Add(lblSelectedSeats);
            pnlSeatInfo.Controls.Add(lblTotalPrice);

            pnlButtons.Controls.Add(btnImportData);
            pnlButtons.Controls.Add(btnBook);
            pnlButtons.Controls.Add(btnClear);
            pnlButtons.Controls.Add(btnExit);
            pnlButtons.Controls.Add(btnExportStats);

            pnlCinema.Controls.Add(lblScreen);

            pnlMain.Controls.Add(pnlCinema);
            pnlMain.Controls.Add(pnlCustomerInfo);
            pnlMain.Controls.Add(pnlSeatInfo);
            pnlMain.Controls.Add(pnlButtons);
            pnlMain.Controls.Add(progressBar);

            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlHeader);

            txtCustomerName.TabIndex = 0;
            cmbMovies.TabIndex = 1;
            cmbRooms.TabIndex = 2;
            btnImportData.TabIndex = 3;
            btnBook.TabIndex = 4;
            btnClear.TabIndex = 5;
            btnExit.TabIndex = 6;
            btnExportStats.TabIndex = 7;

            this.Load += (s, e) => txtCustomerName.Focus();
        }

        private void InitializeData()
        {
            for (int room = 1; room <= 3; room++)
            {
                string roomKey = $"Phòng {room}";
                roomSeatStatus[roomKey] = new Dictionary<string, bool>();
                
                for (int row = 0; row < ROWS; row++)
                {
                    for (int col = 0; col < COLS; col++)
                    {
                        string seatName = GetSeatName(row, col);
                        roomSeatStatus[roomKey][seatName] = false;
                    }
                }
            }

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
            
            // Khởi tạo thống kê cho mỗi phim
            InitializeMovieStatistics();
            
            // Khởi tạo booking history cho mỗi phòng
            for (int room = 1; room <= 3; room++)
            {
                string roomKey = $"Phòng {room}";
                bookingHistory[roomKey] = new List<BookingInfo>();
            }
        }

        private void InitializeMovieStatistics()
        {
            movieStatistics.Clear();
            
            foreach (string movie in movieBasePrices.Keys)
            {
                MovieStats stats = new MovieStats
                {
                    MovieName = movie,
                    TotalSeats = movieRooms[movie].Count * ROWS * COLS, // Mỗi phòng có ROWS*COLS ghế
                    SoldSeats = 0,
                    Revenue = 0
                };
                
                movieStatistics[movie] = stats;
            }
        }

        private void CreateSeatGrid()
        {
            int startY = 75;
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
                        default:
                            btn.BackColor = Color.FromArgb(40, 167, 69);
                            break;
                    }
                    
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Cursor = Cursors.Hand;
                    
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
            pnlLegend.BorderStyle = BorderStyle.None;

            pnlLegend.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220), 1), 
                    0, 0, pnlLegend.Width - 1, pnlLegend.Height - 1);
            };

            Label lblLegendTitle = new Label();
            lblLegendTitle.Text = "CHÚ THÍCH";
            lblLegendTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblLegendTitle.ForeColor = Color.FromArgb(52, 58, 64);
            lblLegendTitle.Size = new Size(370, 25);
            lblLegendTitle.Location = new Point(25, 10);
            pnlLegend.Controls.Add(lblLegendTitle);

            int legendY = 42;
            int legendX = 30;

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
            lblVat.ForeColor = Color.FromArgb(73, 80, 87);
            pnlLegend.Controls.Add(lblVat);

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
            lblVip.ForeColor = Color.FromArgb(73, 80, 87);
            pnlLegend.Controls.Add(lblVip);

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
            lblNormal.ForeColor = Color.FromArgb(73, 80, 87);
            pnlLegend.Controls.Add(lblNormal);

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
            lblSelected.ForeColor = Color.FromArgb(73, 80, 87);
            pnlLegend.Controls.Add(lblSelected);

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
            lblBooked.ForeColor = Color.FromArgb(73, 80, 87);
            pnlLegend.Controls.Add(lblBooked);

            pnlMain.Controls.Add(pnlLegend);
        }

        private void AddButtonHoverEffects()
        {
            btnBook.MouseEnter += (s, e) => {
                btnBook.BackColor = Color.FromArgb(33, 136, 56);
            };
            btnBook.MouseLeave += (s, e) => {
                btnBook.BackColor = Color.FromArgb(40, 167, 69);
            };

            btnClear.MouseEnter += (s, e) => {
                btnClear.BackColor = Color.FromArgb(230, 180, 0);
            };
            btnClear.MouseLeave += (s, e) => {
                btnClear.BackColor = Color.FromArgb(255, 193, 7);
            };

            btnExit.MouseEnter += (s, e) => {
                btnExit.BackColor = Color.FromArgb(90, 98, 104);
            };
            btnExit.MouseLeave += (s, e) => {
                btnExit.BackColor = Color.FromArgb(108, 117, 125);
            };
            
            btnImportData.MouseEnter += (s, e) => {
                btnImportData.BackColor = Color.FromArgb(11, 94, 215);
            };
            btnImportData.MouseLeave += (s, e) => {
                btnImportData.BackColor = Color.FromArgb(13, 110, 253);
            };

            btnExportStats.MouseEnter += (s, e) => {
                btnExportStats.BackColor = Color.FromArgb(95, 55, 165);
            };
            btnExportStats.MouseLeave += (s, e) => {
                btnExportStats.BackColor = Color.FromArgb(111, 66, 193);
            };
        }

        private string GetSeatName(int row, int col)
        {
            char rowChar = (char)('A' + row);
            return $"{rowChar}{col + 1}";
        }

        private void CmbMovies_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbRooms.Items.Clear();
            
            if (cmbMovies.SelectedIndex != -1)
            {
                string selectedMovie = cmbMovies.SelectedItem.ToString();
                
                if (movieRooms.ContainsKey(selectedMovie))
                {
                    foreach (int roomNumber in movieRooms[selectedMovie])
                    {
                        cmbRooms.Items.Add($"Phòng {roomNumber}");
                    }
                    
                    cmbRooms.Enabled = true;
                    cmbRooms.ForeColor = Color.Black;
                    
                    if (cmbRooms.Items.Count > 0)
                    {
                        cmbRooms.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                cmbRooms.Enabled = false;
                cmbRooms.ForeColor = Color.FromArgb(108, 117, 125);
                ClearCurrentRoomSeats();
            }
            
            UpdateSeatInfo();
        }

        private void CmbRooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearCurrentRoomSeats();
            
            if (cmbRooms.SelectedIndex != -1)
            {
                currentSelectedRoom = cmbRooms.SelectedItem.ToString();
                UpdateSeatDisplay();
            }
            
            UpdateSeatInfo();
        }

        private void ClearCurrentRoomSeats()
        {
            selectedSeats.Clear();
            
            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    string seatName = GetSeatName(row, col);
                    RestoreSeatColor(seatButtons[row, col], seatName);
                }
            }
        }

        private void UpdateSeatDisplay()
        {
            if (string.IsNullOrEmpty(currentSelectedRoom))
                return;

            for (int row = 0; row < ROWS; row++)
            {
                for (int col = 0; col < COLS; col++)
                {
                    string seatName = GetSeatName(row, col);
                    Button btn = seatButtons[row, col];
                    
                    if (roomSeatStatus.ContainsKey(currentSelectedRoom) && 
                        roomSeatStatus[currentSelectedRoom].ContainsKey(seatName) &&
                        roomSeatStatus[currentSelectedRoom][seatName])
                    {
                        btn.BackColor = Color.FromArgb(169, 169, 169);
                        btn.Enabled = false;
                        
                        // Tìm thông tin người đặt ghế này
                        string customerInfo = GetCustomerInfoForSeat(seatName);
                        btn.Text = seatName;
                        toolTip.SetToolTip(btn, customerInfo);
                    }
                    else
                    {
                        btn.Enabled = true;
                        RestoreSeatColor(btn, seatName);
                        toolTip.SetToolTip(btn, "");
                    }
                }
            }
        }
        
        private string GetCustomerInfoForSeat(string seatName)
        {
            if (bookingHistory.ContainsKey(currentSelectedRoom))
            {
                foreach (BookingInfo booking in bookingHistory[currentSelectedRoom])
                {
                    if (booking.Seats.Contains(seatName))
                    {
                        return $"Đã đặt bởi: {booking.CustomerName}\nPhim: {booking.MovieName}\nThời gian: {booking.BookingTime:dd/MM/yyyy HH:mm}";
                    }
                }
            }
            return "";
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string seatName = btn.Tag.ToString();
            
            if (string.IsNullOrEmpty(currentSelectedRoom))
            {
                MessageBox.Show("Vui lòng chọn phòng chiếu trước!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (roomSeatStatus[currentSelectedRoom][seatName])
            {
                MessageBox.Show($"Ghế {seatName} đã được đặt trong {currentSelectedRoom}!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void RestoreSeatColor(Button btn, string seatName)
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
                default:
                    btn.BackColor = Color.FromArgb(40, 167, 69);
                    break;
            }
        }

        private void UpdateSeatInfo()
        {
            lblSelectedSeats.Text = $"Ghế đã chọn: {string.Join(", ", selectedSeats)}";

            double total = 0;
            if (cmbMovies.SelectedIndex != -1 && selectedSeats.Count > 0)
            {
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
            }

            lblTotalPrice.Text = $"Tổng tiền: {total:N0} VNĐ";
        }

        private void BtnBook_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerName.Focus();
                return;
            }

            if (cmbMovies.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn phim!", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbMovies.Focus();
                return;
            }

            if (cmbRooms.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn phòng chiếu!", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbRooms.Focus();
                return;
            }

            if (selectedSeats.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 ghế!", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedMovie = cmbMovies.SelectedItem.ToString();
            double basePrice = movieBasePrices[selectedMovie];
            double total = 0;
            
            foreach (string seat in selectedSeats)
            {
                string ticketType = seatType[seat];
                total += basePrice * priceMultiplier[ticketType];
            }

            string message = $"THÔNG TIN KHÁCH HÀNG\n\n";
            message += $"Họ và tên: {txtCustomerName.Text}\n";
            message += $"Tên phim: {cmbMovies.SelectedItem}\n";
            message += $"Phòng chiếu: {cmbRooms.SelectedItem}\n";
            message += $"Ghế đã chọn: {string.Join(", ", selectedSeats)}\n\n";
            
            message += $"CHI TIẾT THANH TOÁN:\n";
            message += $"Giá vé chuẩn: {basePrice:N0} VNĐ\n\n";
            foreach (string seat in selectedSeats)
            {
                if (seatType.ContainsKey(seat))
                {
                    string ticketType = seatType[seat];
                    double price = basePrice * priceMultiplier[ticketType];
                    message += $"- {seat} ({ticketType}): {price:N0} VNĐ\n";
                }
            }
            message += $"\nTỔNG CỘNG: {total:N0} VNĐ";

            MessageBox.Show(message, "Hóa Đơn Thanh Toán", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            foreach (string seat in selectedSeats)
            {
                roomSeatStatus[currentSelectedRoom][seat] = true;
            }
            
            // Lưu thông tin đặt vé vào booking history
            BookingInfo booking = new BookingInfo
            {
                CustomerName = txtCustomerName.Text,
                MovieName = selectedMovie,
                RoomName = currentSelectedRoom,
                Seats = new List<string>(selectedSeats),
                BookingTime = DateTime.Now,
                TotalPrice = total
            };
            
            bookingHistory[currentSelectedRoom].Add(booking);
            
            // Cập nhật thống kê
            if (movieStatistics.ContainsKey(selectedMovie))
            {
                MovieStats stats = movieStatistics[selectedMovie];
                stats.SoldSeats += selectedSeats.Count;
                stats.Revenue += total;
            }

            BtnClear_Click(sender, e);
            
            UpdateSeatDisplay();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtCustomerName.Clear();
            cmbMovies.SelectedIndex = -1;
            cmbRooms.Items.Clear();
            cmbRooms.Enabled = false;
            cmbRooms.ForeColor = Color.FromArgb(108, 117, 125);
            currentSelectedRoom = "";

            ClearCurrentRoomSeats();

            lblSelectedSeats.Text = "Ghế đã chọn: ";
            lblTotalPrice.Text = "Tổng tiền: 0 VNĐ";

            txtCustomerName.Focus();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void BtnImportData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                Title = "Chọn file dữ liệu đặt vé",
                FileName = "input5.txt"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ReadBookingDataFromFile(openFileDialog.FileName);
                    MessageBox.Show("Đã nhập dữ liệu đặt vé thành công!", "Thông báo", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đọc file: {ex.Message}", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ReadBookingDataFromFile(string filePath)
        {
            // Đọc file với cấu trúc: tên người đặt - Tên phim - phòng chiếu - ghế đặt
            string[] lines = File.ReadAllLines(filePath);
            
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                
                // Phân tích dòng dữ liệu: "Tên người - Tên phim - Phòng X - Ghế1,Ghế2,..."
                string[] parts = line.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length >= 4)
                {
                    string customerName = parts[0].Trim();
                    string movieName = parts[1].Trim();
                    string roomName = parts[2].Trim();
                    string seatsStr = parts[3].Trim();
                    
                    // Phân tích danh sách ghế
                    string[] seatArray = seatsStr.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> seats = new List<string>();
                    
                    foreach (string seat in seatArray)
                    {
                        string cleanSeat = seat.Trim();
                        if (!string.IsNullOrEmpty(cleanSeat))
                        {
                            seats.Add(cleanSeat);
                        }
                    }
                    
                    // Kiểm tra phòng có tồn tại không
                    if (roomSeatStatus.ContainsKey(roomName))
                    {
                        // Loại bỏ ghế trùng và chỉ giữ ghế chưa bị đặt
                        HashSet<string> uniqueSeats = new HashSet<string>(seats, StringComparer.OrdinalIgnoreCase);
                        List<string> bookableSeats = new List<string>();
                        foreach (string seat in uniqueSeats)
                        {
                            if (roomSeatStatus[roomName].ContainsKey(seat) && !roomSeatStatus[roomName][seat])
                            {
                                bookableSeats.Add(seat);
                            }
                        }

                        if (bookableSeats.Count == 0)
                        {
                            // Không có ghế hợp lệ để đặt, bỏ qua dòng này
                            continue;
                        }

                        // Đánh dấu các ghế vừa đặt
                        foreach (string seat in bookableSeats)
                        {
                            roomSeatStatus[roomName][seat] = true;
                        }
                        
                        // Tính tổng tiền cho ghế hợp lệ
                        double totalPrice = 0;
                        if (movieBasePrices.ContainsKey(movieName))
                        {
                            double basePrice = movieBasePrices[movieName];
                            foreach (string seat in bookableSeats)
                            {
                                if (seatType.ContainsKey(seat))
                                {
                                    string ticketType = seatType[seat];
                                    totalPrice += basePrice * priceMultiplier[ticketType];
                                }
                            }
                        }
                        
                        // Lưu thông tin đặt vé (chỉ ghế hợp lệ)
                        BookingInfo booking = new BookingInfo
                        {
                            CustomerName = customerName,
                            MovieName = movieName,
                            RoomName = roomName,
                            Seats = bookableSeats,
                            BookingTime = DateTime.Now,
                            TotalPrice = totalPrice
                        };
                        
                        bookingHistory[roomName].Add(booking);
                        
                        // Cập nhật thống kê phim
                        if (movieStatistics.ContainsKey(movieName))
                        {
                            MovieStats stats = movieStatistics[movieName];
                            stats.SoldSeats += bookableSeats.Count;
                            stats.Revenue += totalPrice;
                        }
                    }
                }
            }
            
            // Cập nhật hiển thị ghế nếu đang chọn phòng
            if (!string.IsNullOrEmpty(currentSelectedRoom))
            {
                UpdateSeatDisplay();
            }
        }
        
        private async void BtnExportStats_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                Title = "Lưu file dữ liệu đặt vé",
                FileName = "output5.txt"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                progressBar.Visible = true;
                progressBar.Value = 0;
                
                try
                {
                    await Task.Run(() => ExportBookingDataToFile(saveFileDialog.FileName));
                    MessageBox.Show("Đã xuất dữ liệu đặt vé thành công!", "Thông báo", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    progressBar.Visible = false;
                }
            }
        }

        private void ExportBookingDataToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("DỮ LIỆU ĐẶT VÉ RẠP PHIM");
                writer.WriteLine("=======================");
                writer.WriteLine();
                
                int totalBookings = 0;
                int currentBooking = 0;
                
                // Đếm tổng số booking để tính progress
                foreach (var room in bookingHistory)
                {
                    totalBookings += room.Value.Count;
                }
                
                // Xuất dữ liệu theo từng phòng
                foreach (var room in bookingHistory)
                {
                    string roomName = room.Key;
                    List<BookingInfo> bookings = room.Value;
                    
                    if (bookings.Count > 0)
                    {
                        writer.WriteLine($"PHÒNG: {roomName}");
                        writer.WriteLine("----------------------------");
                        
                        foreach (BookingInfo booking in bookings)
                        {
                            writer.WriteLine($"Tên khách hàng: {booking.CustomerName}");
                            writer.WriteLine($"Tên phim: {booking.MovieName}");
                            writer.WriteLine($"Phòng chiếu: {booking.RoomName}");
                            writer.WriteLine($"Ghế đã đặt: {string.Join(", ", booking.Seats)}");
                            writer.WriteLine($"Thời gian đặt: {booking.BookingTime:dd/MM/yyyy HH:mm:ss}");
                            writer.WriteLine($"Tổng tiền: {booking.TotalPrice:N0} VNĐ");
                            writer.WriteLine();
                            
                            // Cập nhật ProgressBar
                            currentBooking++;
                            int progressValue = totalBookings > 0 ? (int)((double)currentBooking / totalBookings * 100) : 100;
                            
                            // Cập nhật UI từ thread khác
                            this.Invoke(new Action(() => {
                                progressBar.Value = progressValue;
                            }));
                            
                            // Giả lập quá trình xuất file chậm để thấy ProgressBar
                            Thread.Sleep(200);
                        }
                        
                        writer.WriteLine("================================");
                        writer.WriteLine();
                    }
                }
                
                // Xuất thống kê tổng quan
                writer.WriteLine("THỐNG KÊ TỔNG QUAN");
                writer.WriteLine("==================");
                writer.WriteLine($"Tổng số lượt đặt vé: {totalBookings}");
                writer.WriteLine($"Tổng doanh thu: {movieStatistics.Values.Sum(s => s.Revenue):N0} VNĐ");
                writer.WriteLine();
                
                // Xuất thống kê theo phim
                writer.WriteLine("THỐNG KÊ THEO PHIM");
                writer.WriteLine("==================");
                CalculateRevenueRanking();
                
                foreach (var movie in movieStatistics.OrderBy(m => m.Value.Rank))
                {
                    string movieName = movie.Key;
                    MovieStats stats = movie.Value;
                    
                    writer.WriteLine($"Tên phim: {movieName}");
                    writer.WriteLine($"Số lượng vé bán ra: {stats.SoldSeats}");
                    writer.WriteLine($"Số lượng vé tồn: {stats.RemainingSeats}");
                    writer.WriteLine($"Tỉ lệ vé bán ra: {stats.SoldRatio:P2}");
                    writer.WriteLine($"Doanh thu: {stats.Revenue:N0} VNĐ");
                    writer.WriteLine($"Xếp hạng doanh thu: {stats.Rank}");
                    writer.WriteLine();
                }
            }
        }

        private void CalculateRevenueRanking()
        {
            // Sắp xếp phim theo doanh thu từ cao đến thấp
            var sortedMovies = movieStatistics.OrderByDescending(m => m.Value.Revenue).ToList();
            
            // Gán xếp hạng
            int rank = 1;
            foreach (var movie in sortedMovies)
            {
                movie.Value.Rank = rank++;
            }
        }
    }
}
