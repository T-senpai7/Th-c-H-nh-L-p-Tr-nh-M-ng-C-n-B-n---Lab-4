using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bai07
{
    public partial class LoginForm : Form
    {
        private TabControl tabControl = null!;
        private TabPage tabLogin = null!;
        private TabPage tabRegister = null!;
        
        // Login controls
        private TextBox txtLoginUsername = null!;
        private TextBox txtLoginPassword = null!;
        private Button btnLogin = null!;
        private Label lblLoginStatus = null!;

        // Register controls
        private TextBox txtRegisterUsername = null!;
        private TextBox txtRegisterPassword = null!;
        private TextBox txtRegisterEmail = null!;
        private TextBox txtRegisterFirstName = null!;
        private TextBox txtRegisterLastName = null!;
        private ComboBox cmbRegisterSex = null!;
        private DateTimePicker dtpRegisterBirthday = null!;
        private TextBox txtRegisterLanguage = null!;
        private TextBox txtRegisterPhone = null!;
        private Button btnRegister = null!;
        private Label lblRegisterStatus = null!;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Đăng nhập - Hôm nay ăn gì?";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // Tab Control
            tabControl = new TabControl();
            tabControl.Location = new Point(10, 10);
            tabControl.Size = new Size(470, 500);
            tabControl.Font = new Font("Segoe UI", 10F);

            // Login Tab
            tabLogin = new TabPage("Đăng nhập");
            tabLogin.BackColor = Color.White;
            tabLogin.Padding = new Padding(20);

            // Tính toán vị trí căn giữa dựa trên chiều rộng tab control trừ padding
            int tabPageWidth = tabControl.Width - 40; // Trừ padding (20 mỗi bên)
            int centerX = tabPageWidth / 2;
            int fieldWidth = 300;
            int startY = 40;

            Label lblLoginTitle = new Label();
            lblLoginTitle.Text = "ĐĂNG NHẬP";
            lblLoginTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblLoginTitle.Size = new Size(250, 40);
            lblLoginTitle.Location = new Point(centerX - lblLoginTitle.Width / 2, startY);
            lblLoginTitle.TextAlign = ContentAlignment.MiddleCenter;

            int yPos = startY + 60;

            Label lblLoginUser = new Label();
            lblLoginUser.Text = "Username:";
            lblLoginUser.Size = new Size(100, 25);
            lblLoginUser.Location = new Point(centerX - fieldWidth / 2, yPos);
            lblLoginUser.Font = new Font("Segoe UI", 9F);

            txtLoginUsername = new TextBox();
            txtLoginUsername.Location = new Point(centerX - fieldWidth / 2, yPos + 25);
            txtLoginUsername.Size = new Size(fieldWidth, 32);
            txtLoginUsername.Text = "phatpt";
            txtLoginUsername.Font = new Font("Segoe UI", 10F);

            yPos += 70;

            Label lblLoginPass = new Label();
            lblLoginPass.Text = "Password:";
            lblLoginPass.Size = new Size(100, 25);
            lblLoginPass.Location = new Point(centerX - fieldWidth / 2, yPos);
            lblLoginPass.Font = new Font("Segoe UI", 9F);

            txtLoginPassword = new TextBox();
            txtLoginPassword.Location = new Point(centerX - fieldWidth / 2, yPos + 25);
            txtLoginPassword.Size = new Size(fieldWidth, 32);
            txtLoginPassword.PasswordChar = '*';
            txtLoginPassword.Font = new Font("Segoe UI", 10F);

            yPos += 80;

            btnLogin = new Button();
            btnLogin.Text = "Đăng nhập";
            btnLogin.Location = new Point(centerX - fieldWidth / 2, yPos);
            btnLogin.Size = new Size(fieldWidth, 45);
            btnLogin.BackColor = Color.FromArgb(0, 122, 204);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.Click += BtnLogin_Click;

            lblLoginStatus = new Label();
            lblLoginStatus.Text = "";
            lblLoginStatus.Location = new Point(centerX - fieldWidth / 2, yPos + 60);
            lblLoginStatus.Size = new Size(fieldWidth, 50);
            lblLoginStatus.ForeColor = Color.Red;
            lblLoginStatus.TextAlign = ContentAlignment.MiddleCenter;

            tabLogin.Controls.AddRange(new Control[] {
                lblLoginTitle, lblLoginUser, txtLoginUsername,
                lblLoginPass, txtLoginPassword, btnLogin, lblLoginStatus
            });

            // Register Tab
            tabRegister = new TabPage("Đăng ký");
            tabRegister.BackColor = Color.White;
            tabRegister.Padding = new Padding(20);
            tabRegister.AutoScroll = true;

            // Tính toán vị trí căn giữa dựa trên chiều rộng tab control trừ padding
            int regTabPageWidth = tabControl.Width - 40; // Trừ padding (20 mỗi bên)
            int regCenterX = regTabPageWidth / 2;
            int regFieldWidth = 300;
            int regStartY = 20;

            Label lblRegisterTitle = new Label();
            lblRegisterTitle.Text = "ĐĂNG KÝ TÀI KHOẢN";
            lblRegisterTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblRegisterTitle.Size = new Size(320, 35);
            lblRegisterTitle.Location = new Point(regCenterX - lblRegisterTitle.Width / 2, regStartY);
            lblRegisterTitle.TextAlign = ContentAlignment.MiddleCenter;

            int regYPos = regStartY + 45;
            int spacing = 30;

            // Username (required)
            Label lblRegUser = new Label();
            lblRegUser.Text = "Username *:";
            lblRegUser.Size = new Size(120, 25);
            lblRegUser.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegUser.Font = new Font("Segoe UI", 9F);

            txtRegisterUsername = new TextBox();
            txtRegisterUsername.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            txtRegisterUsername.Size = new Size(regFieldWidth, 28);
            txtRegisterUsername.Font = new Font("Segoe UI", 10F);
            regYPos += spacing + 28;

            // Password (required)
            Label lblRegPass = new Label();
            lblRegPass.Text = "Password *:";
            lblRegPass.Size = new Size(120, 25);
            lblRegPass.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegPass.Font = new Font("Segoe UI", 9F);

            txtRegisterPassword = new TextBox();
            txtRegisterPassword.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            txtRegisterPassword.Size = new Size(regFieldWidth, 28);
            txtRegisterPassword.PasswordChar = '*';
            txtRegisterPassword.Font = new Font("Segoe UI", 10F);
            regYPos += spacing + 28;

            // Email (optional)
            Label lblRegEmail = new Label();
            lblRegEmail.Text = "Email:";
            lblRegEmail.Size = new Size(120, 25);
            lblRegEmail.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegEmail.Font = new Font("Segoe UI", 9F);

            txtRegisterEmail = new TextBox();
            txtRegisterEmail.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            txtRegisterEmail.Size = new Size(regFieldWidth, 28);
            txtRegisterEmail.Font = new Font("Segoe UI", 10F);
            regYPos += spacing + 28;

            // First Name (optional)
            Label lblRegFirstName = new Label();
            lblRegFirstName.Text = "Họ:";
            lblRegFirstName.Size = new Size(120, 25);
            lblRegFirstName.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegFirstName.Font = new Font("Segoe UI", 9F);

            txtRegisterFirstName = new TextBox();
            txtRegisterFirstName.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            txtRegisterFirstName.Size = new Size(regFieldWidth, 28);
            txtRegisterFirstName.Font = new Font("Segoe UI", 10F);
            regYPos += spacing + 28;

            // Last Name (optional)
            Label lblRegLastName = new Label();
            lblRegLastName.Text = "Tên:";
            lblRegLastName.Size = new Size(120, 25);
            lblRegLastName.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegLastName.Font = new Font("Segoe UI", 9F);

            txtRegisterLastName = new TextBox();
            txtRegisterLastName.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            txtRegisterLastName.Size = new Size(regFieldWidth, 28);
            txtRegisterLastName.Font = new Font("Segoe UI", 10F);
            regYPos += spacing + 28;

            // Sex (optional)
            Label lblRegSex = new Label();
            lblRegSex.Text = "Giới tính:";
            lblRegSex.Size = new Size(120, 25);
            lblRegSex.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegSex.Font = new Font("Segoe UI", 9F);

            cmbRegisterSex = new ComboBox();
            cmbRegisterSex.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            cmbRegisterSex.Size = new Size(regFieldWidth, 28);
            cmbRegisterSex.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRegisterSex.Font = new Font("Segoe UI", 10F);
            cmbRegisterSex.Items.AddRange(new object[] { "Không chọn", "Nam (0)", "Nữ (1)", "Khác (2)" });
            cmbRegisterSex.SelectedIndex = 0;
            regYPos += spacing + 28;

            // Birthday (optional)
            Label lblRegBirthday = new Label();
            lblRegBirthday.Text = "Ngày sinh:";
            lblRegBirthday.Size = new Size(120, 25);
            lblRegBirthday.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegBirthday.Font = new Font("Segoe UI", 9F);

            dtpRegisterBirthday = new DateTimePicker();
            dtpRegisterBirthday.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            dtpRegisterBirthday.Size = new Size(regFieldWidth, 28);
            dtpRegisterBirthday.Format = DateTimePickerFormat.Short;
            dtpRegisterBirthday.ShowCheckBox = true;
            dtpRegisterBirthday.Checked = false;
            dtpRegisterBirthday.Font = new Font("Segoe UI", 10F);
            regYPos += spacing + 28;

            // Language (optional)
            Label lblRegLanguage = new Label();
            lblRegLanguage.Text = "Ngôn ngữ:";
            lblRegLanguage.Size = new Size(120, 25);
            lblRegLanguage.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegLanguage.Font = new Font("Segoe UI", 9F);

            txtRegisterLanguage = new TextBox();
            txtRegisterLanguage.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            txtRegisterLanguage.Size = new Size(regFieldWidth, 28);
            txtRegisterLanguage.Text = "vi"; // Default Vietnamese
            txtRegisterLanguage.Font = new Font("Segoe UI", 10F);
            regYPos += spacing + 28;

            // Phone (optional)
            Label lblRegPhone = new Label();
            lblRegPhone.Text = "Số điện thoại:";
            lblRegPhone.Size = new Size(120, 25);
            lblRegPhone.Location = new Point(regCenterX - regFieldWidth / 2, regYPos);
            lblRegPhone.Font = new Font("Segoe UI", 9F);

            txtRegisterPhone = new TextBox();
            txtRegisterPhone.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 25);
            txtRegisterPhone.Size = new Size(regFieldWidth, 28);
            txtRegisterPhone.Font = new Font("Segoe UI", 10F);
            regYPos += spacing + 28;

            btnRegister = new Button();
            btnRegister.Text = "Đăng ký";
            btnRegister.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 10);
            btnRegister.Size = new Size(regFieldWidth, 45);
            btnRegister.BackColor = Color.FromArgb(40, 167, 69);
            btnRegister.ForeColor = Color.White;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnRegister.Click += BtnRegister_Click;

            lblRegisterStatus = new Label();
            lblRegisterStatus.Text = "";
            lblRegisterStatus.Location = new Point(regCenterX - regFieldWidth / 2, regYPos + 65);
            lblRegisterStatus.Size = new Size(regFieldWidth, 40);
            lblRegisterStatus.ForeColor = Color.Red;
            lblRegisterStatus.AutoEllipsis = true;
            lblRegisterStatus.TextAlign = ContentAlignment.MiddleCenter;

            tabRegister.Controls.AddRange(new Control[] {
                lblRegisterTitle, 
                lblRegUser, txtRegisterUsername,
                lblRegPass, txtRegisterPassword, 
                lblRegEmail, txtRegisterEmail,
                lblRegFirstName, txtRegisterFirstName,
                lblRegLastName, txtRegisterLastName,
                lblRegSex, cmbRegisterSex,
                lblRegBirthday, dtpRegisterBirthday,
                lblRegLanguage, txtRegisterLanguage,
                lblRegPhone, txtRegisterPhone,
                btnRegister, lblRegisterStatus
            });

            tabControl.TabPages.Add(tabLogin);
            tabControl.TabPages.Add(tabRegister);

            this.Controls.Add(tabControl);
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            lblLoginStatus.Text = "Đang xử lý...";
            lblLoginStatus.ForeColor = Color.Blue;

            string username = txtLoginUsername.Text.Trim();
            string password = txtLoginPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblLoginStatus.Text = "Vui lòng nhập đầy đủ thông tin!";
                lblLoginStatus.ForeColor = Color.Red;
                btnLogin.Enabled = true;
                return;
            }

            var (success, message, response) = await ApiHelper.LoginAsync(username, password);

            if (success && response != null)
            {
                lblLoginStatus.Text = message;
                lblLoginStatus.ForeColor = Color.Green;
                
                // Mở MainForm
                this.Hide();
                var mainForm = new MainForm();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            else
            {
                lblLoginStatus.Text = message;
                lblLoginStatus.ForeColor = Color.Red;
                btnLogin.Enabled = true;
            }
        }

        private async void BtnRegister_Click(object? sender, EventArgs e)
        {
            btnRegister.Enabled = false;
            lblRegisterStatus.Text = "Đang xử lý...";
            lblRegisterStatus.ForeColor = Color.Blue;

            string username = txtRegisterUsername.Text.Trim();
            string password = txtRegisterPassword.Text;
            string email = txtRegisterEmail.Text.Trim();
            string firstName = txtRegisterFirstName.Text.Trim();
            string lastName = txtRegisterLastName.Text.Trim();
            string language = txtRegisterLanguage.Text.Trim();
            string phone = txtRegisterPhone.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(username))
            {
                lblRegisterStatus.Text = "Vui lòng nhập Username!";
                lblRegisterStatus.ForeColor = Color.Red;
                btnRegister.Enabled = true;
                txtRegisterUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                lblRegisterStatus.Text = "Vui lòng nhập Password!";
                lblRegisterStatus.ForeColor = Color.Red;
                btnRegister.Enabled = true;
                txtRegisterPassword.Focus();
                return;
            }

            if (password.Length < 6)
            {
                lblRegisterStatus.Text = "Password phải có ít nhất 6 ký tự!";
                lblRegisterStatus.ForeColor = Color.Red;
                btnRegister.Enabled = true;
                txtRegisterPassword.Focus();
                return;
            }

            // Validate phone format (nếu có)
            if (!string.IsNullOrEmpty(phone))
            {
                // Phone chỉ chứa số và các ký tự +, -, (, )
                var phoneRegex = new System.Text.RegularExpressions.Regex(@"^[\d\+\-\(\)\s]+$");
                if (!phoneRegex.IsMatch(phone))
                {
                    lblRegisterStatus.Text = "Số điện thoại chỉ được chứa số và ký tự +, -, (, )";
                    lblRegisterStatus.ForeColor = Color.Red;
                    btnRegister.Enabled = true;
                    txtRegisterPhone.Focus();
                    return;
                }
            }

            // Parse sex
            int? sex = null;
            if (cmbRegisterSex.SelectedIndex > 0)
            {
                sex = cmbRegisterSex.SelectedIndex - 1; // 0=Nam, 1=Nữ, 2=Khác
            }

            // Parse birthday
            string? birthday = null;
            if (dtpRegisterBirthday.Checked)
            {
                birthday = dtpRegisterBirthday.Value.ToString("yyyy-MM-dd");
            }

            // Gọi API Register
            var (success, message, user) = await ApiHelper.RegisterAsync(
                username: username,
                password: password,
                email: string.IsNullOrEmpty(email) ? null : email,
                firstName: string.IsNullOrEmpty(firstName) ? null : firstName,
                lastName: string.IsNullOrEmpty(lastName) ? null : lastName,
                sex: sex,
                birthday: birthday,
                language: string.IsNullOrEmpty(language) ? null : language,
                phone: string.IsNullOrEmpty(phone) ? null : phone);

            if (success && user != null)
            {
                lblRegisterStatus.Text = "Đăng ký thành công! Đang chuyển đến trang chính...";
                lblRegisterStatus.ForeColor = Color.Green;
                
                // Clear password field để bảo mật
                txtRegisterPassword.Text = "";
                
                // Chuyển sang tab đăng nhập và điền username
                tabControl.SelectedTab = tabLogin;
                txtLoginUsername.Text = username;
                txtLoginPassword.Text = "";
                
                // Mở MainForm sau 1.5 giây
                await Task.Delay(1500);
                
                this.Hide();
                var mainForm = new MainForm();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            else
            {
                // Hiển thị lỗi chi tiết
                lblRegisterStatus.Text = !string.IsNullOrEmpty(message) ? message : "Đăng ký thất bại. Vui lòng thử lại!";
                lblRegisterStatus.ForeColor = Color.Red;
                btnRegister.Enabled = true;
            }
        }
    }
}

