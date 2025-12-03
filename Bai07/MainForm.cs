using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bai07
{
    public partial class MainForm : Form
    {
        private TabControl tabControl = null!;
        private TabPage tabAllFoods = null!;
        private TabPage tabMyFoods = null!;
        
        // Panels cho mỗi tab
        private Panel scrollPanelAll = null!;
        private Panel scrollPanelMy = null!;
        private FlowLayoutPanel flowPanelAll = null!;
        private FlowLayoutPanel flowPanelMy = null!;
        
        // Buttons
        private Button btnAddFood = null!;
        private Button btnDeleteFood = null!;
        private Button btnRandomAll = null!;
        private Button btnRandomMy = null!;
        private Label lblUserInfo = null!;
        private Button btnLogout = null!;
        
        // Pagination controls
        private Label lblPageInfo = null!;
        private Button btnPrevPage = null!;
        private Button btnNextPage = null!;
        private NumericUpDown numPage = null!;
        private NumericUpDown numPageSize = null!;
        
        // Current state
        private int currentPage = 1;
        private int pageSize = 10;
        private bool showingAllFoods = true;
        private User? currentUser = null;
        private List<MonAn> currentFoods = new List<MonAn>();
        private MonAn? selectedFood = null;

        public MainForm()
        {
            InitializeComponent();
            LoadUserInfo();
            LoadFoods();
        }

        private void InitializeComponent()
        {
            this.Text = "Hôm nay ăn gì? - Quản lý món ăn";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // User info panel
            Panel panelUser = new Panel();
            panelUser.Location = new Point(10, 10);
            panelUser.Size = new Size(980, 50);
            panelUser.BackColor = Color.White;
            panelUser.BorderStyle = BorderStyle.FixedSingle;

            lblUserInfo = new Label();
            lblUserInfo.Text = "Đang tải thông tin...";
            lblUserInfo.Location = new Point(10, 15);
            lblUserInfo.Size = new Size(700, 20);
            lblUserInfo.Font = new Font("Segoe UI", 10F);

            btnLogout = new Button();
            btnLogout.Text = "Đăng xuất";
            btnLogout.Location = new Point(880, 10);
            btnLogout.Size = new Size(90, 30);
            btnLogout.BackColor = Color.FromArgb(220, 53, 69);
            btnLogout.ForeColor = Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Click += BtnLogout_Click;

            panelUser.Controls.Add(lblUserInfo);
            panelUser.Controls.Add(btnLogout);

            // Tab Control
            tabControl = new TabControl();
            tabControl.Location = new Point(10, 70);
            tabControl.Size = new Size(980, 500);
            tabControl.Font = new Font("Segoe UI", 10F);
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            // ========== Tab: Tất cả món ăn ==========
            tabAllFoods = new TabPage("Tất cả món ăn");
            tabAllFoods.BackColor = Color.White;
            tabAllFoods.Padding = new Padding(10);

            // Scroll panel cho tab All
            scrollPanelAll = new Panel();
            scrollPanelAll.Location = new Point(10, 10);
            scrollPanelAll.Size = new Size(960, 400);
            scrollPanelAll.AutoScroll = true;
            scrollPanelAll.BackColor = Color.FromArgb(248, 249, 250);
            scrollPanelAll.BorderStyle = BorderStyle.FixedSingle;

            flowPanelAll = new FlowLayoutPanel();
            flowPanelAll.Dock = DockStyle.Top;
            flowPanelAll.AutoSize = true;
            flowPanelAll.FlowDirection = FlowDirection.LeftToRight;
            flowPanelAll.WrapContents = true;
            flowPanelAll.Padding = new Padding(10);
            flowPanelAll.BackColor = Color.Transparent;

            scrollPanelAll.Controls.Add(flowPanelAll);

            // Buttons panel cho tab All
            Panel panelButtonsAll = CreateButtonsPanel();
            panelButtonsAll.Location = new Point(10, 420);

            tabAllFoods.Controls.Add(scrollPanelAll);
            tabAllFoods.Controls.Add(panelButtonsAll);

            // ========== Tab: Món ăn của tôi ==========
            tabMyFoods = new TabPage("Món ăn của tôi");
            tabMyFoods.BackColor = Color.White;
            tabMyFoods.Padding = new Padding(10);

            // Scroll panel cho tab My
            scrollPanelMy = new Panel();
            scrollPanelMy.Location = new Point(10, 10);
            scrollPanelMy.Size = new Size(960, 400);
            scrollPanelMy.AutoScroll = true;
            scrollPanelMy.BackColor = Color.FromArgb(248, 249, 250);
            scrollPanelMy.BorderStyle = BorderStyle.FixedSingle;

            flowPanelMy = new FlowLayoutPanel();
            flowPanelMy.Dock = DockStyle.Top;
            flowPanelMy.AutoSize = true;
            flowPanelMy.FlowDirection = FlowDirection.LeftToRight;
            flowPanelMy.WrapContents = true;
            flowPanelMy.Padding = new Padding(10);
            flowPanelMy.BackColor = Color.Transparent;

            scrollPanelMy.Controls.Add(flowPanelMy);

            // Buttons panel cho tab My
            Panel panelButtonsMy = CreateButtonsPanel();
            panelButtonsMy.Location = new Point(10, 420);

            tabMyFoods.Controls.Add(scrollPanelMy);
            tabMyFoods.Controls.Add(panelButtonsMy);

            // Add tabs
            tabControl.TabPages.Add(tabAllFoods);
            tabControl.TabPages.Add(tabMyFoods);

            // Pagination panel
            Panel panelPagination = new Panel();
            panelPagination.Location = new Point(10, 580);
            panelPagination.Size = new Size(980, 70);
            panelPagination.BackColor = Color.White;
            panelPagination.BorderStyle = BorderStyle.FixedSingle;

            Label lblPageSize = new Label();
            lblPageSize.Text = "Số món/trang:";
            lblPageSize.Location = new Point(10, 25);
            lblPageSize.Size = new Size(100, 20);

            numPageSize = new NumericUpDown();
            numPageSize.Location = new Point(120, 23);
            numPageSize.Size = new Size(60, 25);
            numPageSize.Minimum = 5;
            numPageSize.Maximum = 50;
            numPageSize.Value = 10;
            numPageSize.ValueChanged += NumPageSize_ValueChanged;

            btnPrevPage = new Button();
            btnPrevPage.Text = "◀ Trước";
            btnPrevPage.Location = new Point(200, 20);
            btnPrevPage.Size = new Size(100, 30);
            btnPrevPage.Click += BtnPrevPage_Click;

            lblPageInfo = new Label();
            lblPageInfo.Text = "Trang 1 / 1";
            lblPageInfo.Location = new Point(310, 25);
            lblPageInfo.Size = new Size(150, 20);
            lblPageInfo.TextAlign = ContentAlignment.MiddleCenter;

            numPage = new NumericUpDown();
            numPage.Location = new Point(470, 23);
            numPage.Size = new Size(60, 25);
            numPage.Minimum = 1;
            numPage.Maximum = 1;
            numPage.Value = 1;
            numPage.ValueChanged += NumPage_ValueChanged;

            btnNextPage = new Button();
            btnNextPage.Text = "Sau ▶";
            btnNextPage.Location = new Point(540, 20);
            btnNextPage.Size = new Size(100, 30);
            btnNextPage.Click += BtnNextPage_Click;

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.Location = new Point(650, 20);
            btnRefresh.Size = new Size(120, 30);
            btnRefresh.BackColor = Color.FromArgb(108, 117, 125);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += BtnRefresh_Click;

            panelPagination.Controls.AddRange(new Control[] {
                lblPageSize, numPageSize, btnPrevPage, lblPageInfo,
                numPage, btnNextPage, btnRefresh
            });

            this.Controls.Add(panelUser);
            this.Controls.Add(tabControl);
            this.Controls.Add(panelPagination);
        }

        private Panel CreateButtonsPanel()
        {
            Panel panelButtons = new Panel();
            panelButtons.Size = new Size(960, 50);
            panelButtons.BackColor = Color.Transparent;

            Button btnAdd = new Button();
            btnAdd.Text = "➕ Thêm món ăn";
            btnAdd.Location = new Point(10, 10);
            btnAdd.Size = new Size(150, 35);
            btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAdd.Click += BtnAddFood_Click;

            Button btnDelete = new Button();
            btnDelete.Text = "🗑️ Xóa món ăn";
            btnDelete.Location = new Point(170, 10);
            btnDelete.Size = new Size(150, 35);
            btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDelete.Click += BtnDeleteFood_Click;

            Button btnRandAll = new Button();
            btnRandAll.Text = "🎲 Ngẫu nhiên (Cộng đồng)";
            btnRandAll.Location = new Point(330, 10);
            btnRandAll.Size = new Size(200, 35);
            btnRandAll.BackColor = Color.FromArgb(255, 193, 7);
            btnRandAll.ForeColor = Color.Black;
            btnRandAll.FlatStyle = FlatStyle.Flat;
            btnRandAll.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRandAll.Click += BtnRandomAll_Click;

            Button btnRandMy = new Button();
            btnRandMy.Text = "🎲 Ngẫu nhiên (Của tôi)";
            btnRandMy.Location = new Point(540, 10);
            btnRandMy.Size = new Size(200, 35);
            btnRandMy.BackColor = Color.FromArgb(23, 162, 184);
            btnRandMy.ForeColor = Color.White;
            btnRandMy.FlatStyle = FlatStyle.Flat;
            btnRandMy.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRandMy.Click += BtnRandomMy_Click;

            panelButtons.Controls.AddRange(new Control[] {
                btnAdd, btnDelete, btnRandAll, btnRandMy
            });

            return panelButtons;
        }

        private FlowLayoutPanel GetCurrentFlowPanel()
        {
            return showingAllFoods ? flowPanelAll : flowPanelMy;
        }

        private async void LoadUserInfo()
        {
            var (success, user, message) = await ApiHelper.GetCurrentUserAsync();
            if (success && user != null)
            {
                currentUser = user;
                lblUserInfo.Text = $"Xin chào, {user.FullName ?? user.Username} ({user.Username})";
                lblUserInfo.ForeColor = Color.Green;
            }
            else
            {
                lblUserInfo.Text = $"Lỗi: {message}";
                lblUserInfo.ForeColor = Color.Red;
            }
        }

        private async void LoadFoods()
        {
            var flowPanel = GetCurrentFlowPanel();
            flowPanel.Controls.Clear();
            flowPanel.Enabled = false;
            currentFoods.Clear();
            selectedFood = null;

            MonAnListResponse? response;
            string message;
            bool success = false;

            if (showingAllFoods)
            {
                var result = await ApiHelper.GetAllFoodsAsync(currentPage, pageSize);
                success = result.Success;
                response = result.Response;
                message = result.Message;
            }
            else
            {
                var result = await ApiHelper.GetMyFoodsAsync(currentPage, pageSize);
                success = result.Success;
                response = result.Response;
                message = result.Message;
            }

            if (success && response != null)
            {
                var foods = response.Data ?? new List<MonAn>();
                currentFoods = foods;
                
                System.Diagnostics.Debug.WriteLine($"LoadFoods ({(showingAllFoods ? "All" : "My")}): Loaded {foods.Count} foods");
                
                if (foods.Count > 0)
                {
                    foreach (var food in foods)
                    {
                        var foodCard = CreateFoodCard(food);
                        flowPanel.Controls.Add(foodCard);
                    }
                }
                else
                {
                    Label lblNoFood = new Label();
                    lblNoFood.Text = showingAllFoods 
                        ? "Không có món ăn nào trong cộng đồng" 
                        : "Bạn chưa có món ăn nào";
                    lblNoFood.Size = new Size(300, 30);
                    lblNoFood.Font = new Font("Segoe UI", 12F);
                    lblNoFood.ForeColor = Color.Gray;
                    lblNoFood.TextAlign = ContentAlignment.MiddleCenter;
                    flowPanel.Controls.Add(lblNoFood);
                }

                // Update pagination
                int currentPageNum = response.Pagination?.Current ?? 1;
                int pageSizeNum = response.Pagination?.PageSize ?? pageSize;
                int totalItems = response.Pagination?.Total ?? 0;
                int totalPages = pageSizeNum > 0 ? (int)Math.Ceiling((double)totalItems / pageSizeNum) : 1;
                if (totalPages < 1) totalPages = 1;
                
                numPage.Maximum = totalPages;
                numPage.Value = currentPageNum > 0 ? Math.Min(currentPageNum, totalPages) : 1;
                currentPage = (int)numPage.Value;
                lblPageInfo.Text = $"Trang {currentPageNum} / {totalPages} (Tổng: {totalItems} món)";

                btnPrevPage.Enabled = currentPageNum > 1;
                btnNextPage.Enabled = currentPageNum < totalPages;
            }
            else
            {
                Label lblError = new Label();
                lblError.Text = $"Lỗi: {message}";
                lblError.Size = new Size(400, 50);
                lblError.Font = new Font("Segoe UI", 12F);
                lblError.ForeColor = Color.Red;
                lblError.TextAlign = ContentAlignment.MiddleCenter;
                flowPanel.Controls.Add(lblError);
                
                MessageBox.Show($"Lỗi khi tải danh sách món ăn: {message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            flowPanel.Enabled = true;
        }

        private Panel CreateFoodCard(MonAn food)
        {
            Panel card = new Panel();
            card.Size = new Size(280, 350);
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Margin = new Padding(10);
            card.Cursor = Cursors.Hand;
            card.Tag = food;

            card.Click += (s, e) => SelectFoodCard(card, food);

            // PictureBox
            PictureBox picFood = new PictureBox();
            picFood.Location = new Point(10, 10);
            picFood.Size = new Size(260, 180);
            picFood.SizeMode = PictureBoxSizeMode.Zoom;
            picFood.BackColor = Color.FromArgb(240, 240, 240);
            picFood.BorderStyle = BorderStyle.FixedSingle;
            picFood.Click += (s, e) => SelectFoodCard(card, food);
            
            string? imageUrl = food.HinhAnh;
            if (!string.IsNullOrEmpty(imageUrl) && Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            {
                try
                {
                    picFood.LoadAsync(imageUrl);
                    picFood.LoadCompleted += (s, e) => {
                        if (e.Error != null) ShowNoImagePlaceholder(picFood);
                    };
                }
                catch { ShowNoImagePlaceholder(picFood); }
            }
            else
            {
                ShowNoImagePlaceholder(picFood);
            }

            // Tên món ăn
            Label lblName = new Label();
            lblName.Text = food.TenMonAn ?? "N/A";
            lblName.Location = new Point(10, 200);
            lblName.Size = new Size(260, 30);
            lblName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(33, 37, 41);
            lblName.AutoEllipsis = true;
            lblName.Click += (s, e) => SelectFoodCard(card, food);

            // Giá
            Label lblPrice = new Label();
            lblPrice.Text = food.Gia > 0 ? $"💰 {food.Gia:N0} đ" : "💰 Giá: Liên hệ";
            lblPrice.Location = new Point(10, 235);
            lblPrice.Size = new Size(260, 20);
            lblPrice.Font = new Font("Segoe UI", 10F);
            lblPrice.ForeColor = Color.FromArgb(220, 53, 69);
            lblPrice.Click += (s, e) => SelectFoodCard(card, food);

            // Địa chỉ
            Label lblAddress = new Label();
            lblAddress.Text = !string.IsNullOrEmpty(food.DiaChi) ? $"📍 {food.DiaChi}" : "📍 Địa chỉ: N/A";
            lblAddress.Location = new Point(10, 260);
            lblAddress.Size = new Size(260, 20);
            lblAddress.Font = new Font("Segoe UI", 9F);
            lblAddress.ForeColor = Color.FromArgb(108, 117, 125);
            lblAddress.AutoEllipsis = true;
            lblAddress.Click += (s, e) => SelectFoodCard(card, food);

            // Người đóng góp
            Label lblContributor = new Label();
            lblContributor.Text = $"👤 {(!string.IsNullOrEmpty(food.NguoiDongGop) ? food.NguoiDongGop : "N/A")}";
            lblContributor.Location = new Point(10, 285);
            lblContributor.Size = new Size(260, 20);
            lblContributor.Font = new Font("Segoe UI", 9F);
            lblContributor.ForeColor = Color.FromArgb(108, 117, 125);
            lblContributor.AutoEllipsis = true;
            lblContributor.Click += (s, e) => SelectFoodCard(card, food);

            // Mô tả
            if (!string.IsNullOrEmpty(food.MoTa))
            {
                Label lblDesc = new Label();
                lblDesc.Text = food.MoTa.Length > 50 ? food.MoTa.Substring(0, 50) + "..." : food.MoTa;
                lblDesc.Location = new Point(10, 310);
                lblDesc.Size = new Size(260, 30);
                lblDesc.Font = new Font("Segoe UI", 8F);
                lblDesc.ForeColor = Color.FromArgb(73, 80, 87);
                lblDesc.AutoEllipsis = true;
                lblDesc.Click += (s, e) => SelectFoodCard(card, food);
                card.Controls.Add(lblDesc);
            }

            card.Controls.AddRange(new Control[] {
                picFood, lblName, lblPrice, lblAddress, lblContributor
            });

            return card;
        }

        private void SelectFoodCard(Panel card, MonAn food)
        {
            var flowPanel = GetCurrentFlowPanel();
            
            foreach (Control ctrl in flowPanel.Controls)
            {
                if (ctrl is Panel p && p != card)
                {
                    p.BackColor = Color.White;
                    p.BorderStyle = BorderStyle.FixedSingle;
                }
            }
            
            card.BackColor = Color.FromArgb(230, 240, 255);
            card.BorderStyle = BorderStyle.Fixed3D;
            selectedFood = food;
        }

        private void ShowNoImagePlaceholder(PictureBox picFood)
        {
            picFood.Image = null;
            picFood.Controls.Clear();
            
            Label lblNoImage = new Label();
            lblNoImage.Text = "📷\nKhông có hình";
            lblNoImage.Dock = DockStyle.Fill;
            lblNoImage.TextAlign = ContentAlignment.MiddleCenter;
            lblNoImage.Font = new Font("Segoe UI", 10F);
            lblNoImage.ForeColor = Color.Gray;
            picFood.Controls.Add(lblNoImage);
        }

        private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            showingAllFoods = tabControl.SelectedIndex == 0;
            currentPage = 1;
            numPage.Value = 1;
            LoadFoods();
        }

        private void BtnAddFood_Click(object? sender, EventArgs e)
        {
            var addForm = new AddFoodForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadFoods();
            }
        }

        private async void BtnDeleteFood_Click(object? sender, EventArgs e)
        {
            if (selectedFood == null)
            {
                MessageBox.Show("Vui lòng chọn món ăn cần xóa (click vào card món ăn)!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa món ăn \"{selectedFood.TenMonAn}\"?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var (success, message) = await ApiHelper.DeleteFoodAsync(selectedFood.Id);
                
                if (success)
                {
                    MessageBox.Show(message, "Thành công", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectedFood = null;
                    LoadFoods();
                }
                else
                {
                    MessageBox.Show(message, "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnRandomAll_Click(object? sender, EventArgs e)
        {
            var (success, food, message) = await ApiHelper.GetRandomFoodAsync();
            
            if (success && food != null)
            {
                ShowRandomFoodDialog(food, "Món ăn ngẫu nhiên từ cộng đồng");
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnRandomMy_Click(object? sender, EventArgs e)
        {
            var (success, food, message) = await ApiHelper.GetRandomMyFoodAsync();
            
            if (success && food != null)
            {
                ShowRandomFoodDialog(food, "Món ăn ngẫu nhiên của bạn");
            }
            else
            {
                MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowRandomFoodDialog(MonAn food, string title)
        {
            Form dialog = new Form();
            dialog.Text = title;
            dialog.Size = new Size(500, 450);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.BackColor = Color.White;

            Label lblTitle = new Label();
            lblTitle.Text = $"🎲 {food.TenMonAn}";
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(440, 40);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.ForeColor = Color.FromArgb(33, 37, 41);

            Label lblPrice = new Label();
            lblPrice.Text = food.Gia > 0 ? $"💰 Giá: {food.Gia:N0} đ" : "💰 Giá: Liên hệ";
            lblPrice.Font = new Font("Segoe UI", 12F);
            lblPrice.Location = new Point(20, 70);
            lblPrice.Size = new Size(440, 25);
            lblPrice.ForeColor = Color.FromArgb(220, 53, 69);

            Label lblAddress = new Label();
            lblAddress.Text = $"📍 Địa chỉ: {food.DiaChi ?? "N/A"}";
            lblAddress.Font = new Font("Segoe UI", 10F);
            lblAddress.Location = new Point(20, 100);
            lblAddress.Size = new Size(440, 25);
            lblAddress.ForeColor = Color.FromArgb(108, 117, 125);

            Label lblDescTitle = new Label();
            lblDescTitle.Text = "📝 Mô tả:";
            lblDescTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDescTitle.Location = new Point(20, 135);
            lblDescTitle.Size = new Size(440, 20);

            TextBox txtDescription = new TextBox();
            txtDescription.Text = food.MoTa ?? "Không có mô tả";
            txtDescription.Location = new Point(20, 160);
            txtDescription.Size = new Size(440, 150);
            txtDescription.Multiline = true;
            txtDescription.ReadOnly = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.BackColor = Color.FromArgb(248, 249, 250);

            Label lblContributor = new Label();
            lblContributor.Text = $"👤 Người đóng góp: {food.NguoiDongGop ?? "N/A"}";
            lblContributor.Font = new Font("Segoe UI", 10F);
            lblContributor.Location = new Point(20, 320);
            lblContributor.Size = new Size(440, 25);
            lblContributor.ForeColor = Color.FromArgb(108, 117, 125);

            Button btnClose = new Button();
            btnClose.Text = "Đóng";
            btnClose.Location = new Point(200, 360);
            btnClose.Size = new Size(100, 35);
            btnClose.BackColor = Color.FromArgb(108, 117, 125);
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Click += (s, e) => dialog.Close();

            dialog.Controls.AddRange(new Control[] {
                lblTitle, lblPrice, lblAddress, lblDescTitle, 
                txtDescription, lblContributor, btnClose
            });

            dialog.ShowDialog();
        }

        private void BtnPrevPage_Click(object? sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                numPage.Value = currentPage;
                LoadFoods();
            }
        }

        private void BtnNextPage_Click(object? sender, EventArgs e)
        {
            currentPage++;
            numPage.Value = currentPage;
            LoadFoods();
        }

        private void NumPage_ValueChanged(object? sender, EventArgs e)
        {
            if (numPage.Value != currentPage)
            {
                currentPage = (int)numPage.Value;
                LoadFoods();
            }
        }

        private void NumPageSize_ValueChanged(object? sender, EventArgs e)
        {
            pageSize = (int)numPageSize.Value;
            currentPage = 1;
            numPage.Value = 1;
            LoadFoods();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadFoods();
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            ApiHelper.ClearAccessToken();
            this.Hide();
            var loginForm = new LoginForm();
            loginForm.FormClosed += (s, args) => this.Close();
            loginForm.Show();
        }
    }
}