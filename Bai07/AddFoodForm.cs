using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bai07
{
    public partial class AddFoodForm : Form
    {
        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private TextBox txtAddress = null!;
        private TextBox txtImageUrl = null!;
        private NumericUpDown numPrice = null!;
        private Button btnAdd = null!;
        private Button btnCancel = null!;
        private Label lblStatus = null!;

        public AddFoodForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Thêm món ăn mới";
            this.Size = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);

            Label lblTitle = new Label();
            lblTitle.Text = "THÊM MÓN ĂN MỚI";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(60, 15);
            lblTitle.Size = new Size(380, 40);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            Label lblName = new Label();
            lblName.Text = "Tên món ăn *:";
            lblName.Location = new Point(20, 80);
            lblName.Size = new Size(150, 25);
            lblName.Font = new Font("Segoe UI", 10F);

            txtName = new TextBox();
            txtName.Location = new Point(20, 105);
            txtName.Size = new Size(460, 30);
            txtName.Font = new Font("Segoe UI", 10F);

            Label lblPrice = new Label();
            lblPrice.Text = "Giá (VNĐ):";
            lblPrice.Location = new Point(20, 145);
            lblPrice.Size = new Size(150, 25);
            lblPrice.Font = new Font("Segoe UI", 10F);

            numPrice = new NumericUpDown();
            numPrice.Location = new Point(20, 170);
            numPrice.Size = new Size(200, 30);
            numPrice.Font = new Font("Segoe UI", 10F);
            numPrice.Maximum = 1_000_000_000;
            numPrice.Minimum = 0;
            numPrice.Increment = 1000;
            numPrice.ThousandsSeparator = true;

            Label lblAddress = new Label();
            lblAddress.Text = "Địa chỉ (tùy chọn):";
            lblAddress.Location = new Point(240, 145);
            lblAddress.Size = new Size(240, 25);
            lblAddress.Font = new Font("Segoe UI", 10F);

            txtAddress = new TextBox();
            txtAddress.Location = new Point(240, 170);
            txtAddress.Size = new Size(240, 30);
            txtAddress.Font = new Font("Segoe UI", 10F);

            Label lblDescription = new Label();
            lblDescription.Text = "Mô tả:";
            lblDescription.Location = new Point(20, 210);
            lblDescription.Size = new Size(150, 25);
            lblDescription.Font = new Font("Segoe UI", 10F);

            txtDescription = new TextBox();
            txtDescription.Location = new Point(20, 235);
            txtDescription.Size = new Size(460, 120);
            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Font = new Font("Segoe UI", 10F);

            Label lblImageUrl = new Label();
            lblImageUrl.Text = "URL hình ảnh (tùy chọn):";
            lblImageUrl.Location = new Point(20, 365);
            lblImageUrl.Size = new Size(200, 25);
            lblImageUrl.Font = new Font("Segoe UI", 10F);

            txtImageUrl = new TextBox();
            txtImageUrl.Location = new Point(20, 390);
            txtImageUrl.Size = new Size(460, 30);
            txtImageUrl.Font = new Font("Segoe UI", 10F);
            // Note: PlaceholderText requires .NET 6+ or custom implementation

            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Location = new Point(20, 430);
            lblStatus.Size = new Size(460, 17);
            lblStatus.ForeColor = Color.Red;
            lblStatus.AutoEllipsis = true;

            btnAdd = new Button();
            btnAdd.Text = "Thêm món ăn";
            btnAdd.Location = new Point(200, 445);
            btnAdd.Size = new Size(130, 40);
            btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAdd.Click += BtnAdd_Click;

            btnCancel = new Button();
            btnCancel.Text = "Hủy";
            btnCancel.Location = new Point(350, 445);
            btnCancel.Size = new Size(130, 40);
            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 10F);
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblTitle, lblName, txtName,
                lblPrice, numPrice,
                lblAddress, txtAddress,
                lblDescription, txtDescription,
                lblImageUrl, txtImageUrl,
                lblStatus, btnAdd, btnCancel
            });
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string description = txtDescription.Text.Trim();
            string address = txtAddress.Text.Trim();
            string imageUrl = txtImageUrl.Text.Trim();
            decimal price = numPrice.Value;

            if (string.IsNullOrEmpty(name))
            {
                lblStatus.Text = "Vui lòng nhập tên món ăn!";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            btnAdd.Enabled = false;
            lblStatus.Text = "Đang thêm món ăn...";
            lblStatus.ForeColor = Color.Blue;

            var (success, food, message) = await ApiHelper.AddFoodAsync(
                tenMonAn: name,
                gia: price,
                moTa: string.IsNullOrEmpty(description) ? null : description,
                hinhAnh: string.IsNullOrEmpty(imageUrl) ? null : imageUrl,
                diaChi: string.IsNullOrEmpty(address) ? null : address
            );

            if (success && food != null)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = Color.Green;
                await Task.Delay(1000);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = Color.Red;
                btnAdd.Enabled = true;
            }
        }
    }
}

