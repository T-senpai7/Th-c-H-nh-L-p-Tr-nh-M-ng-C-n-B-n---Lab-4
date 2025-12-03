using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Bai05
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // Disable button during request
            btnLogin.Enabled = false;
            txtResult.Clear();
            txtResult.Text = "Đang xử lý...\r\n";

            try
            {
                string url = txtUrl.Text.Trim();
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(url))
                {
                    MessageBox.Show("Vui lòng nhập URL!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnLogin.Enabled = true;
                    return;
                }

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Vui lòng nhập Username!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnLogin.Enabled = true;
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Vui lòng nhập Password!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnLogin.Enabled = true;
                    return;
                }

                // Perform HTTP POST request
                await LoginAsync(url, username, password);
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Lỗi: {ex.Message}\r\n";
                if (ex.InnerException != null)
                {
                    txtResult.Text += $"Chi tiết: {ex.InnerException.Message}\r\n";
                }
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private async Task LoginAsync(string url, string username, string password)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // Create form-data content
                    var content = new MultipartFormDataContent
                    {
                        { new StringContent(username), "username" },
                        { new StringContent(password), "password" }
                    };

                    // Send POST request
                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Parse JSON response
                    var responseObject = JObject.Parse(responseString);

                    // Check if login was successful
                    if (!response.IsSuccessStatusCode)
                    {
                        // Login failed - show detail
                        var detail = responseObject["detail"]?.ToString() ?? "Không có thông tin chi tiết";
                        txtResult.Text = $"Detail: {detail}\r\n";
                        txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
                    }
                    else
                    {
                        // Login successful
                        var tokenType = responseObject["token_type"]?.ToString() ?? "";
                        var accessToken = responseObject["access_token"]?.ToString() ?? "";

                        txtResult.Text = "Bearer\r\n";
                        txtResult.Text += $"{accessToken}\r\n";
                        txtResult.Text += "\r\n";
                        txtResult.Text += "Đăng nhập thành công\r\n";
                    }
                }
                catch (HttpRequestException ex)
                {
                    txtResult.Text = $"Lỗi kết nối: {ex.Message}\r\n";
                    if (ex.InnerException != null)
                    {
                        txtResult.Text += $"Chi tiết: {ex.InnerException.Message}\r\n";
                    }
                }
                catch (Exception ex)
                {
                    txtResult.Text = $"Lỗi: {ex.Message}\r\n";
                }
            }
        }
    }
}

