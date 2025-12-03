using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Bai6
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnGetUserInfo_Click(object sender, EventArgs e)
        {
            // Disable button during request
            btnGetUserInfo.Enabled = false;
            txtResult.Clear();
            txtResult.Text = "Đang xử lý...\r\n";

            try
            {
                string url = txtUrl.Text.Trim();
                string tokenType = txtTokenType.Text.Trim();
                string accessToken = txtAccessToken.Text.Trim();

                // Validation
                if (string.IsNullOrEmpty(url))
                {
                    MessageBox.Show("Vui lòng nhập URL!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnGetUserInfo.Enabled = true;
                    return;
                }

                if (string.IsNullOrEmpty(tokenType))
                {
                    MessageBox.Show("Vui lòng nhập Token Type!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnGetUserInfo.Enabled = true;
                    return;
                }

                if (string.IsNullOrEmpty(accessToken))
                {
                    MessageBox.Show("Vui lòng nhập Access Token!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnGetUserInfo.Enabled = true;
                    return;
                }

                // Perform HTTP GET request
                await GetUserInfoAsync(url, tokenType, accessToken);
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
                btnGetUserInfo.Enabled = true;
            }
        }

        private async Task GetUserInfoAsync(string url, string tokenType, string accessToken)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // Set Authorization header với JWT token
                    client.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue(tokenType, accessToken);

                    // Send GET request
                    var response = await client.GetAsync(url);
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Check if request was successful
                    if (!response.IsSuccessStatusCode)
                    {
                        // Parse error response
                        try
                        {
                            var errorObject = JObject.Parse(responseString);
                            var detail = errorObject["detail"]?.ToString() ?? responseString;
                            txtResult.Text = $"Detail: {detail}\r\n";
                            txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
                        }
                        catch
                        {
                            // If not JSON, show raw response
                            txtResult.Text = $"Lỗi: {responseString}\r\n";
                            txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
                        }
                        return;
                    }

                    // Parse JSON response
                    try
                    {
                        var userObject = JObject.Parse(responseString);
                        
                        // Format và hiển thị thông tin user
                        txtResult.Text = "THÔNG TIN NGƯỜI DÙNG:\r\n";
                        txtResult.Text += "========================\r\n\r\n";

                        // Hiển thị các trường thông tin cơ bản
                        if (userObject["id"] != null)
                            txtResult.Text += $"ID: {userObject["id"]}\r\n";
                        
                        if (userObject["username"] != null)
                            txtResult.Text += $"Username: {userObject["username"]}\r\n";
                        
                        if (userObject["email"] != null)
                            txtResult.Text += $"Email: {userObject["email"]}\r\n";
                        
                        if (userObject["full_name"] != null)
                            txtResult.Text += $"Họ và tên: {userObject["full_name"]}\r\n";
                        
                        if (userObject["phone"] != null)
                            txtResult.Text += $"Số điện thoại: {userObject["phone"]}\r\n";
                        
                        if (userObject["address"] != null)
                            txtResult.Text += $"Địa chỉ: {userObject["address"]}\r\n";
                        
                        if (userObject["is_active"] != null)
                        {
                            var isActiveStr = userObject["is_active"]?.ToString();
                            var isActive = !string.IsNullOrEmpty(isActiveStr) && isActiveStr.ToLower() == "true";
                            txtResult.Text += $"Trạng thái: {(isActive ? "Hoạt động" : "Không hoạt động")}\r\n";
                        }

                        // Hiển thị JSON đầy đủ ở cuối
                        txtResult.Text += "\r\n========================\r\n";
                        txtResult.Text += "JSON RESPONSE (ĐẦY ĐỦ):\r\n";
                        txtResult.Text += "========================\r\n";
                        var jsonString = userObject.ToString(Newtonsoft.Json.Formatting.Indented);
                        if (jsonString != null)
                            txtResult.Text += jsonString;
                    }
                    catch (Exception parseEx)
                    {
                        // Nếu không parse được JSON, hiển thị raw response
                        txtResult.Text = "RESPONSE (RAW):\r\n";
                        txtResult.Text += "========================\r\n";
                        txtResult.Text += responseString;
                        txtResult.Text += $"\r\n\r\nLưu ý: Không thể parse JSON. Lỗi: {parseEx.Message}";
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

