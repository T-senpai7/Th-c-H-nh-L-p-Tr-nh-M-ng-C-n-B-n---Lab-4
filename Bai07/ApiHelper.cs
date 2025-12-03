using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace Bai07
{
    public class ApiHelper
    {
        private static string? _accessToken;
        private static string _baseUrl = "https://nt106.uitiot.vn";

        public static void SetAccessToken(string token)
        {
            _accessToken = token;
        }

        public static void ClearAccessToken()
        {
            _accessToken = null;
        }

        public static string? GetAccessToken()
        {
            return _accessToken;
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (!client.DefaultRequestHeaders.UserAgent.Any())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Bai07-Client/1.0");
            }
            
            if (!string.IsNullOrEmpty(_accessToken))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            
            return client;
        }

        private static void LogApiCall(string method, string url, HttpStatusCode? statusCode, 
            string? contentType, string? responseBody, Exception? ex = null)
        {
            var log = new StringBuilder();
            log.AppendLine($"=== API Call: {method} ===");
            log.AppendLine($"URL: {url}");
            log.AppendLine($"Has Token: {!string.IsNullOrEmpty(_accessToken)}");
            
            if (statusCode.HasValue)
                log.AppendLine($"Status Code: {(int)statusCode.Value} ({statusCode.Value})");
            
            if (!string.IsNullOrEmpty(contentType))
                log.AppendLine($"Content-Type: {contentType}");
            
            if (ex != null)
                log.AppendLine($"Exception: {ex.GetType().Name} - {ex.Message}");
            
            if (!string.IsNullOrEmpty(responseBody))
            {
                var preview = responseBody.Length > 500 
                    ? responseBody.Substring(0, 500) + "..." 
                    : responseBody;
                log.AppendLine($"Response Preview:\n{preview}");
            }
            
            Debug.WriteLine(log.ToString());
        }

        // ==================== AUTHENTICATION ====================

        /// <summary>
        /// Đăng ký tài khoản mới
        /// POST /api/v1/user/signup
        /// Request: {
        ///   "username": "string",
        ///   "email": "user@example.com",
        ///   "password": "stringst",
        ///   "first_name": "string",
        ///   "last_name": "string",
        ///   "sex": 0,
        ///   "birthday": "2025-12-03",
        ///   "language": "string",
        ///   "phone": "string"
        /// }
        /// Response: User object
        /// </summary>
        public static async Task<(bool Success, string Message, User? Response)> RegisterAsync(
            string username, string password, string? email = null, 
            string? firstName = null, string? lastName = null,
            int? sex = null, string? birthday = null, string? language = null, string? phone = null)
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/user/signup";

                var data = new
                {
                    username = username,
                    password = password,
                    email = email,
                    first_name = firstName,
                    last_name = lastName,
                    sex = sex,
                    birthday = birthday,
                    language = language,
                    phone = phone
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                LogApiCall("POST", url, null, "application/json", json);

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("POST", url, response.StatusCode, 
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    var user = JsonConvert.DeserializeObject<User>(responseString);
                    if (user != null)
                    {
                        // Sau khi đăng ký thành công, cần đăng nhập để lấy token
                        var loginResult = await LoginAsync(username, password);
                        if (loginResult.Success && loginResult.Response != null)
                        {
                            return (true, "Đăng ký thành công! Đã tự động đăng nhập.", user);
                        }
                        return (true, "Đăng ký thành công! Vui lòng đăng nhập.", user);
                    }
                }
                
                // Xử lý lỗi validation (422)
                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    try
                    {
                        var errorDetail = JsonConvert.DeserializeObject<ApiErrorDetail>(responseString);
                        if (errorDetail != null && errorDetail.Detail != null && errorDetail.Detail.Count > 0)
                        {
                            var errorMessages = string.Join("\n", errorDetail.Detail.Select(e => e.Msg ?? ""));
                            return (false, errorMessages, null);
                        }
                    }
                    catch { }
                }
                
                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, error?.Detail ?? "Đăng ký thất bại", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Đăng nhập
        /// POST /auth/token
        /// </summary>
        public static async Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(
            string username, string password)
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/auth/token";

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("grant_type", "password")
                });

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("POST", url, response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseString);
                    if (loginResponse != null)
                    {
                        _accessToken = loginResponse.AccessToken;
                        return (true, "Đăng nhập thành công!", loginResponse);
                    }
                }
                
                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, error?.Detail ?? "Đăng nhập thất bại", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Refresh token
        /// POST /auth/refresh
        /// </summary>
        public static async Task<(bool Success, string Message, LoginResponse? Response)> RefreshTokenAsync()
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/auth/refresh";

                var response = await client.PostAsync(url, null);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("POST", url, response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseString);
                    if (loginResponse != null)
                    {
                        _accessToken = loginResponse.AccessToken;
                        return (true, "Refresh token thành công!", loginResponse);
                    }
                }
                
                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, error?.Detail ?? "Refresh token thất bại", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        // ==================== USER ====================

        /// <summary>
        /// Lấy thông tin user hiện tại
        /// GET /api/v1/user/me
        /// </summary>
        public static async Task<(bool Success, User? Response, string Message)> GetCurrentUserAsync()
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/user/me";

                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("GET", url, response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    var user = JsonConvert.DeserializeObject<User>(responseString);
                    return (true, user, "Thành công");
                }
                
                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, null, error?.Detail ?? "Lỗi khi lấy thông tin user");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả users (chỉ superuser)
        /// GET /api/v1/user/all
        /// </summary>
        public static async Task<(bool Success, List<User>? Response, string Message)> GetAllUsersAsync()
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/user/all";

                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("GET", url, response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    var users = JsonConvert.DeserializeObject<List<User>>(responseString);
                    return (true, users, "Thành công");
                }
                
                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, null, error?.Detail ?? "Lỗi khi lấy danh sách users");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        // ==================== MÓN ĂN (MONAN) ====================
        // QUAN TRỌNG: 
        // - Endpoint là /api/v1/monan/
        // - /all và /my-dishes là POST với body {"current": 1, "pageSize": 5}
        // - Response có "data" và "pagination"

        /// <summary>
        /// Lấy danh sách tất cả món ăn (cộng đồng)
        /// POST /api/v1/monan/all
        /// Request: {"current": 1, "pageSize": 5}
        /// Response: {"data": [...], "pagination": {...}}
        /// </summary>
        public static async Task<(bool Success, MonAnListResponse? Response, string Message)> GetAllFoodsAsync(
            int page = 1, int pageSize = 10)
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/monan/all";

                // QUAN TRỌNG: API yêu cầu POST với body JSON
                var requestData = new
                {
                    current = page,
                    pageSize = pageSize
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                LogApiCall("POST", url, null, "application/json", json);

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "";

                LogApiCall("POST", url, response.StatusCode, contentType, responseString);

                // Kiểm tra HTML response
                if (responseString.TrimStart().StartsWith("<") || contentType.Contains("text/html"))
                {
                    return (false, null, $"Server trả về HTML thay vì JSON. Status: {response.StatusCode}");
                }

                if (response.IsSuccessStatusCode)
                {
                    var foodResponse = JsonConvert.DeserializeObject<MonAnListResponse>(responseString);
                    return (true, foodResponse, "Thành công");
                }
                
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, null, "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
                }

                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, null, error?.Detail ?? $"Lỗi: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                LogApiCall("POST", $"{_baseUrl}/api/v1/monan/all", null, null, null, ex);
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách món ăn của bản thân
        /// POST /api/v1/monan/my-dishes
        /// Request: {"current": 1, "pageSize": 5}
        /// Response: {"data": [...], "pagination": {...}}
        /// </summary>
        public static async Task<(bool Success, MonAnListResponse? Response, string Message)> GetMyFoodsAsync(
            int page = 1, int pageSize = 10)
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/monan/my-dishes";

                // QUAN TRỌNG: API yêu cầu POST với body JSON
                var requestData = new
                {
                    current = page,
                    pageSize = pageSize
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                LogApiCall("POST", url, null, "application/json", json);

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "";

                LogApiCall("POST", url, response.StatusCode, contentType, responseString);

                if (responseString.TrimStart().StartsWith("<") || contentType.Contains("text/html"))
                {
                    return (false, null, $"Server trả về HTML thay vì JSON. Status: {response.StatusCode}");
                }

                if (response.IsSuccessStatusCode)
                {
                    var foodResponse = JsonConvert.DeserializeObject<MonAnListResponse>(responseString);
                    return (true, foodResponse, "Thành công");
                }
                
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, null, "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
                }

                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, null, error?.Detail ?? $"Lỗi: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                LogApiCall("POST", $"{_baseUrl}/api/v1/monan/my-dishes", null, null, null, ex);
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Thêm món ăn mới
        /// POST /api/v1/monan/add
        /// </summary>
        public static async Task<(bool Success, MonAn? Response, string Message)> AddFoodAsync(
            string tenMonAn, decimal gia = 0, string? moTa = null, string? hinhAnh = null, string? diaChi = null)
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/monan/add";

                var data = new
                {
                    ten_mon_an = tenMonAn,
                    gia = gia,
                    mo_ta = moTa,
                    hinh_anh = hinhAnh,
                    dia_chi = diaChi
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                LogApiCall("POST", url, null, "application/json", json);

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("POST", url, response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    var food = JsonConvert.DeserializeObject<MonAn>(responseString);
                    return (true, food, "Thêm món ăn thành công!");
                }
                
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, null, "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
                }

                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, null, error?.Detail ?? "Thêm món ăn thất bại");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Overload để tương thích với code cũ
        /// </summary>
        public static async Task<(bool Success, MonAn? Response, string Message)> AddFoodAsync(
            string name, string? description = null, string? imageUrl = null)
        {
            return await AddFoodAsync(
                tenMonAn: name,
                gia: 0,
                moTa: description,
                hinhAnh: imageUrl,
                diaChi: null
            );
        }

        /// <summary>
        /// Lấy thông tin món ăn theo ID
        /// GET /api/v1/monan/{id}
        /// </summary>
        public static async Task<(bool Success, MonAn? Response, string Message)> GetFoodByIdAsync(int foodId)
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/monan/{foodId}";

                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("GET", url, response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    var food = JsonConvert.DeserializeObject<MonAn>(responseString);
                    return (true, food, "Thành công");
                }
                
                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, null, error?.Detail ?? "Lỗi khi lấy thông tin món ăn");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật món ăn theo ID
        /// PUT /api/v1/monan/{id}
        /// </summary>
        public static async Task<(bool Success, MonAn? Response, string Message)> UpdateFoodAsync(
            int foodId, string? tenMonAn = null, decimal? gia = null, string? moTa = null, 
            string? hinhAnh = null, string? diaChi = null)
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/monan/{foodId}";

                var data = new
                {
                    ten_mon_an = tenMonAn,
                    gia = gia,
                    mo_ta = moTa,
                    hinh_anh = hinhAnh,
                    dia_chi = diaChi
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("PUT", url, response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    var food = JsonConvert.DeserializeObject<MonAn>(responseString);
                    return (true, food, "Cập nhật món ăn thành công!");
                }
                
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, null, "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return (false, null, "Bạn không có quyền sửa món ăn này!");
                }

                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, null, error?.Detail ?? "Cập nhật món ăn thất bại");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa món ăn theo ID
        /// DELETE /api/v1/monan/{id}
        /// </summary>
        public static async Task<(bool Success, string Message)> DeleteFoodAsync(int foodId)
        {
            try
            {
                using var client = CreateClient();
                var url = $"{_baseUrl}/api/v1/monan/{foodId}";

                var response = await client.DeleteAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                LogApiCall("DELETE", url, response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType, responseString);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Xóa món ăn thành công!");
                }
                
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return (false, "Bạn không có quyền xóa món ăn này!");
                }

                var error = JsonConvert.DeserializeObject<ApiError>(responseString);
                return (false, error?.Detail ?? "Xóa món ăn thất bại");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Chọn ngẫu nhiên món ăn từ cộng đồng
        /// </summary>
        public static async Task<(bool Success, MonAn? Response, string Message)> GetRandomFoodAsync()
        {
            try
            {
                // Lấy tất cả món ăn và chọn ngẫu nhiên
                var result = await GetAllFoodsAsync(1, 100);
                if (result.Success && result.Response?.Data != null && result.Response.Data.Count > 0)
                {
                    var random = new Random();
                    var randomFood = result.Response.Data[random.Next(result.Response.Data.Count)];
                    return (true, randomFood, "Thành công");
                }
                
                return (false, null, "Không có món ăn nào");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Chọn ngẫu nhiên món ăn từ danh sách của bản thân
        /// </summary>
        public static async Task<(bool Success, MonAn? Response, string Message)> GetRandomMyFoodAsync()
        {
            try
            {
                var result = await GetMyFoodsAsync(1, 100);
                if (result.Success && result.Response?.Data != null && result.Response.Data.Count > 0)
                {
                    var random = new Random();
                    var randomFood = result.Response.Data[random.Next(result.Response.Data.Count)];
                    return (true, randomFood, "Thành công");
                }
                
                return (false, null, "Bạn chưa có món ăn nào");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        // ==================== HELPER METHODS ====================

        public static async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                
                var response = await client.GetAsync($"{_baseUrl}/docs");
                
                return response.IsSuccessStatusCode 
                    ? (true, "Kết nối API thành công!")
                    : (false, $"Server trả về status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (false, $"Không thể kết nối đến server: {ex.Message}");
            }
        }

        public static async Task<bool> IsTokenValidAsync()
        {
            if (string.IsNullOrEmpty(_accessToken))
                return false;

            var result = await GetCurrentUserAsync();
            return result.Success;
        }

        public static void Logout()
        {
            ClearAccessToken();
        }
    }

    // ==================== MODELS ====================

    public class LoginResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = "bearer";
    }

    public class ApiError
    {
        [JsonProperty("detail")]
        public string? Detail { get; set; }
    }

    public class ApiErrorDetail
    {
        [JsonProperty("detail")]
        public List<ValidationError>? Detail { get; set; }
    }

    public class ValidationError
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("loc")]
        public List<object>? Loc { get; set; }

        [JsonProperty("msg")]
        public string? Msg { get; set; }

        [JsonProperty("input")]
        public object? Input { get; set; }
    }

    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("full_name")]
        public string? FullName { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }

        [JsonProperty("is_superuser")]
        public bool IsSuperuser { get; set; }

        [JsonProperty("birthday")]
        public string? Birthday { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Model cho Món Ăn - theo đúng API response
    /// Response format:
    /// {
    ///   "id": 1,
    ///   "ten_mon_an": "Bún Bò Huế",
    ///   "gia": 35000,
    ///   "mo_ta": "...",
    ///   "hinh_anh": "https://...",
    ///   "dia_chi": "123 - ABC",
    ///   "nguoi_dong_gop": "baonv"
    /// }
    /// </summary>
    public class MonAn
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("ten_mon_an")]
        public string TenMonAn { get; set; } = string.Empty;

        [JsonProperty("gia")]
        public decimal Gia { get; set; }

        [JsonProperty("mo_ta")]
        public string? MoTa { get; set; }

        [JsonProperty("hinh_anh")]
        public string? HinhAnh { get; set; }

        [JsonProperty("dia_chi")]
        public string? DiaChi { get; set; }

        [JsonProperty("nguoi_dong_gop")]
        public string? NguoiDongGop { get; set; }

        // Alias properties để tương thích với code cũ
        [JsonIgnore]
        public string Name => TenMonAn;
        
        [JsonIgnore]
        public string? Description => MoTa;
        
        [JsonIgnore]
        public string? ImageUrl => HinhAnh;
        
        [JsonIgnore]
        public decimal Price => Gia;
        
        [JsonIgnore]
        public string? Address => DiaChi;

        [JsonIgnore]
        public string? Owner => NguoiDongGop;
    }

    /// <summary>
    /// Alias cho MonAn để tương thích với code cũ
    /// </summary>
    public class Food : MonAn { }

    /// <summary>
    /// Response chứa danh sách món ăn với pagination
    /// Response format:
    /// {
    ///   "data": [...],
    ///   "pagination": {
    ///     "current": 1,
    ///     "pageSize": 5,
    ///     "total": 10
    ///   }
    /// }
    /// </summary>
    public class MonAnListResponse
    {
        [JsonProperty("data")]
        public List<MonAn> Data { get; set; } = new List<MonAn>();

        [JsonProperty("pagination")]
        public PaginationInfo? Pagination { get; set; }

        // Alias để tương thích code cũ
        [JsonIgnore]
        public List<MonAn> Items => Data;
    }

    /// <summary>
    /// Alias cho MonAnListResponse để tương thích với code cũ
    /// </summary>
    public class FoodListResponse : MonAnListResponse { }

    /// <summary>
    /// Thông tin phân trang theo đúng API response
    /// </summary>
    public class PaginationInfo
    {
        [JsonProperty("current")]
        public int Current { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        // Alias để tương thích code cũ
        [JsonIgnore]
        public int CurrentPage => Current;

        [JsonIgnore]
        public int TotalItems => Total;

        [JsonIgnore]
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
    }
}