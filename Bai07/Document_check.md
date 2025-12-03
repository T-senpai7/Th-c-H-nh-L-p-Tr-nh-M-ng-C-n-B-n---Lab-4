# BÀI 7: HÔM NAY ĂN GÌ? - ỨNG DỤNG QUẢN LÝ MÓN ĂN

## Ý TƯỞNG CHÍNH

Ứng dụng quản lý món ăn cho phép người dùng đăng ký, đăng nhập, và quản lý danh sách món ăn của mình cũng như xem món ăn từ cộng đồng. Hệ thống sử dụng:

- **RESTful API**: Giao tiếp với server qua HTTP requests (GET, POST, DELETE)
- **JWT Authentication**: Sử dụng JWT token để xác thực người dùng
- **Phân trang**: Hiển thị danh sách món ăn theo trang với khả năng điều hướng
- **Windows Forms UI**: Giao diện desktop với các form riêng biệt cho từng chức năng
- **Async/Await Pattern**: Xử lý bất đồng bộ để không block UI thread

### Kiến trúc hệ thống:

```
┌─────────────┐         HTTP Requests         ┌─────────────┐
│   Client    │ ────────────────────────────▶ │   Server    │
│ (Windows    │  POST /auth/token             │  (API)      │
│  Forms)     │  POST /api/v1/user/signup     │             │
│             │  POST /api/v1/monan/all       │             │
│             │  POST /api/v1/monan/my-dishes │             │
│             │  POST /api/v1/monan/add       │             │
│             │  DELETE /api/v1/monan/{id}    │             │
│             │                                │             │
│             │ ◀───────────────────────────── │             │
│             │      JSON Response             │             │
│             │      (data + pagination)       │             │
└─────────────┘                                └─────────────┘
```

### Luồng xử lý chính:

1. **Đăng ký/Đăng nhập**: Người dùng tạo tài khoản hoặc đăng nhập để lấy JWT token
2. **Xem danh sách món ăn**: Load danh sách từ API với phân trang
3. **Thêm món ăn**: Tạo món ăn mới và gửi lên server
4. **Xóa món ăn**: Xóa món ăn đã chọn
5. **Chọn ngẫu nhiên**: Lấy ngẫu nhiên một món ăn từ danh sách

### Tính năng chính:

1. **Đăng ký/Đăng nhập**: Tạo tài khoản mới hoặc đăng nhập với username/password
2. **Xem danh sách**: Xem tất cả món ăn từ cộng đồng hoặc chỉ món ăn của mình
3. **Phân trang**: Điều hướng giữa các trang, thay đổi số món ăn/trang
4. **Thêm món ăn**: Thêm món ăn mới với tên, giá, mô tả, hình ảnh, địa chỉ
5. **Xóa món ăn**: Xóa món ăn đã tạo (chỉ món ăn của mình)
6. **Chọn ngẫu nhiên**: Chọn ngẫu nhiên từ cộng đồng hoặc từ món ăn của mình

---

## CÁC BƯỚC THỰC HIỆN

### 1. Nhận sự kiện người dùng

Hệ thống nhận và xử lý các sự kiện từ người dùng thông qua giao diện Windows Forms:

#### 1.1. Sự kiện đăng nhập

**Trong LoginForm.cs:**
```csharp
private async void BtnLogin_Click(object? sender, EventArgs e)
{
    btnLogin.Enabled = false;
    lblLoginStatus.Text = "Đang xử lý...";
    lblLoginStatus.ForeColor = Color.Blue;

    string username = txtLoginUsername.Text.Trim();
    string password = txtLoginPassword.Text;

    // Validation
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        lblLoginStatus.Text = "Vui lòng nhập đầy đủ thông tin!";
        lblLoginStatus.ForeColor = Color.Red;
        btnLogin.Enabled = true;
        return;
    }

    // Gọi API đăng nhập
    var (success, message, response) = await ApiHelper.LoginAsync(username, password);

    if (success && response != null)
    {
        // Mở MainForm
        this.Hide();
        var mainForm = new MainForm();
        mainForm.Show();
    }
    else
    {
        lblLoginStatus.Text = message;
        lblLoginStatus.ForeColor = Color.Red;
        btnLogin.Enabled = true;
    }
}
```

**Các hành động khi click đăng nhập:**
- Disable nút đăng nhập để tránh click nhiều lần
- Hiển thị "Đang xử lý..." để người dùng biết hệ thống đang làm việc
- Lấy username và password từ TextBox
- Trim khoảng trắng thừa
- Validation dữ liệu đầu vào
- Gọi `ApiHelper.LoginAsync()` để xử lý đăng nhập
- Nếu thành công → Mở MainForm
- Nếu thất bại → Hiển thị thông báo lỗi

#### 1.2. Sự kiện đăng ký

**Trong LoginForm.cs:**
```csharp
private async void BtnRegister_Click(object? sender, EventArgs e)
{
    btnRegister.Enabled = false;
    lblRegisterStatus.Text = "Đang xử lý...";
    lblRegisterStatus.ForeColor = Color.Blue;

    // Lấy dữ liệu từ form
    string username = txtRegisterUsername.Text.Trim();
    string password = txtRegisterPassword.Text;
    string email = txtRegisterEmail.Text.Trim();
    string firstName = txtRegisterFirstName.Text.Trim();
    string lastName = txtRegisterLastName.Text.Trim();
    // ... các trường khác

    // Validation
    if (string.IsNullOrEmpty(username))
    {
        lblRegisterStatus.Text = "Vui lòng nhập Username!";
        return;
    }

    if (string.IsNullOrEmpty(password) || password.Length < 6)
    {
        lblRegisterStatus.Text = "Password phải có ít nhất 6 ký tự!";
        return;
    }

    // Gọi API đăng ký
    var (success, message, user) = await ApiHelper.RegisterAsync(
        username, password, email, firstName, lastName, ...);

    if (success && user != null)
    {
        // Chuyển sang tab đăng nhập và tự động đăng nhập
        tabControl.SelectedTab = tabLogin;
        txtLoginUsername.Text = username;
        // Mở MainForm sau 1.5 giây
        await Task.Delay(1500);
        this.Hide();
        var mainForm = new MainForm();
        mainForm.Show();
    }
}
```

**Validation khi đăng ký:**
- Username: Bắt buộc, không được rỗng
- Password: Bắt buộc, tối thiểu 6 ký tự
- Email: Tùy chọn
- Phone: Tùy chọn, nếu có phải đúng format (chỉ số và ký tự +, -, (, ))

#### 1.3. Sự kiện thêm món ăn

**Trong MainForm.cs:**
```csharp
private void BtnAddFood_Click(object? sender, EventArgs e)
{
    var addForm = new AddFoodForm();
    if (addForm.ShowDialog() == DialogResult.OK)
    {
        LoadFoods(); // Reload danh sách sau khi thêm
    }
}
```

**Trong AddFoodForm.cs:**
```csharp
private async void BtnAdd_Click(object? sender, EventArgs e)
{
    string name = txtName.Text.Trim();
    string description = txtDescription.Text.Trim();
    string address = txtAddress.Text.Trim();
    string imageUrl = txtImageUrl.Text.Trim();
    decimal price = numPrice.Value;

    // Validation
    if (string.IsNullOrEmpty(name))
    {
        lblStatus.Text = "Vui lòng nhập tên món ăn!";
        return;
    }

    // Gọi API thêm món ăn
    var (success, food, message) = await ApiHelper.AddFoodAsync(
        tenMonAn: name,
        gia: price,
        moTa: string.IsNullOrEmpty(description) ? null : description,
        hinhAnh: string.IsNullOrEmpty(imageUrl) ? null : imageUrl,
        diaChi: string.IsNullOrEmpty(address) ? null : address
    );

    if (success && food != null)
    {
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
```

#### 1.4. Sự kiện xóa món ăn

**Trong MainForm.cs:**
```csharp
private async void BtnDeleteFood_Click(object? sender, EventArgs e)
{
    if (selectedFood == null)
    {
        MessageBox.Show("Vui lòng chọn món ăn cần xóa!");
        return;
    }

    // Xác nhận xóa
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
            MessageBox.Show(message, "Thành công");
            selectedFood = null;
            LoadFoods(); // Reload danh sách
        }
        else
        {
            MessageBox.Show(message, "Lỗi");
        }
    }
}
```

#### 1.5. Sự kiện chọn món ăn

**Trong MainForm.cs:**
```csharp
private void SelectFoodCard(Panel card, MonAn food)
{
    var flowPanel = GetCurrentFlowPanel();
    
    // Bỏ chọn tất cả cards khác
    foreach (Control ctrl in flowPanel.Controls)
    {
        if (ctrl is Panel p && p != card)
        {
            p.BackColor = Color.White;
            p.BorderStyle = BorderStyle.FixedSingle;
        }
    }
    
    // Chọn card hiện tại
    card.BackColor = Color.FromArgb(230, 240, 255);
    card.BorderStyle = BorderStyle.Fixed3D;
    selectedFood = food;
}
```

**Khi click vào food card:**
- Card được highlight (đổi màu nền và border)
- Các cards khác trở về trạng thái bình thường
- `selectedFood` được cập nhật để dùng cho xóa

#### 1.6. Sự kiện phân trang

**Trong MainForm.cs:**
```csharp
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
```

**Các sự kiện phân trang:**
- **Trước**: Giảm `currentPage` và load lại danh sách
- **Sau**: Tăng `currentPage` và load lại danh sách
- **Nhập số trang**: Cập nhật `currentPage` và load lại
- **Thay đổi số món/trang**: Reset về trang 1 và load lại với `pageSize` mới

#### 1.7. Sự kiện chọn ngẫu nhiên

**Trong MainForm.cs:**
```csharp
private async void BtnRandomAll_Click(object? sender, EventArgs e)
{
    var (success, food, message) = await ApiHelper.GetRandomFoodAsync();
    
    if (success && food != null)
    {
        ShowRandomFoodDialog(food, "Món ăn ngẫu nhiên từ cộng đồng");
    }
    else
    {
        MessageBox.Show(message, "Lỗi");
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
        MessageBox.Show(message, "Lỗi");
    }
}
```

#### 1.8. Sự kiện chuyển tab

**Trong MainForm.cs:**
```csharp
private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
{
    showingAllFoods = tabControl.SelectedIndex == 0; // 0 = Tất cả, 1 = Của tôi
    currentPage = 1;
    numPage.Value = 1;
    LoadFoods();
}
```

**Khi chuyển tab:**
- Cập nhật `showingAllFoods` (true = tất cả, false = của tôi)
- Reset về trang 1
- Load lại danh sách món ăn tương ứng

---

### 2. Đọc dữ liệu từ web

Hệ thống đọc dữ liệu từ server thông qua các API endpoints:

#### 2.1. Đăng nhập và lấy token

**Trong ApiHelper.cs:**
```csharp
public static async Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(
    string username, string password)
{
    using var client = CreateClient();
    var url = $"{_baseUrl}/auth/token";

    // Tạo form-data content
    var content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("username", username),
        new KeyValuePair<string, string>("password", password),
        new KeyValuePair<string, string>("grant_type", "password")
    });

    var response = await client.PostAsync(url, content);
    var responseString = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseString);
        if (loginResponse != null)
        {
            _accessToken = loginResponse.AccessToken; // Lưu token
            return (true, "Đăng nhập thành công!", loginResponse);
        }
    }
    
    var error = JsonConvert.DeserializeObject<ApiError>(responseString);
    return (false, error?.Detail ?? "Đăng nhập thất bại", null);
}
```

**Cấu trúc request:**
- **Method**: POST
- **URL**: `https://nt106.uitiot.vn/auth/token`
- **Content-Type**: `application/x-www-form-urlencoded`
- **Body**: `username`, `password`, `grant_type=password`

**Cấu trúc response:**
```json
{
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "token_type": "bearer"
}
```

#### 2.2. Đăng ký tài khoản

**Trong ApiHelper.cs:**
```csharp
public static async Task<(bool Success, string Message, User? Response)> RegisterAsync(
    string username, string password, string? email = null, ...)
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

    var response = await client.PostAsync(url, content);
    var responseString = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        var user = JsonConvert.DeserializeObject<User>(responseString);
        // Sau khi đăng ký thành công, tự động đăng nhập
        var loginResult = await LoginAsync(username, password);
        return (true, "Đăng ký thành công! Đã tự động đăng nhập.", user);
    }
    
    // Xử lý lỗi validation (422)
    if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
    {
        var errorDetail = JsonConvert.DeserializeObject<ApiErrorDetail>(responseString);
        var errorMessages = string.Join("\n", errorDetail.Detail.Select(e => e.Msg ?? ""));
        return (false, errorMessages, null);
    }
}
```

**Cấu trúc request:**
- **Method**: POST
- **URL**: `https://nt106.uitiot.vn/api/v1/user/signup`
- **Content-Type**: `application/json`
- **Body**: JSON với các trường user

#### 2.3. Lấy danh sách tất cả món ăn

**Trong ApiHelper.cs:**
```csharp
public static async Task<(bool Success, MonAnListResponse? Response, string Message)> GetAllFoodsAsync(
    int page = 1, int pageSize = 10)
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

    var response = await client.PostAsync(url, content);
    var responseString = await response.Content.ReadAsStringAsync();

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
```

**Cấu trúc request:**
- **Method**: POST
- **URL**: `https://nt106.uitiot.vn/api/v1/monan/all`
- **Headers**: `Authorization: Bearer <token>`
- **Content-Type**: `application/json`
- **Body**: `{"current": 1, "pageSize": 10}`

**Cấu trúc response:**
```json
{
    "data": [
        {
            "id": 1,
            "ten_mon_an": "Bún Bò Huế",
            "gia": 35000,
            "mo_ta": "...",
            "hinh_anh": "https://...",
            "dia_chi": "123 - ABC",
            "nguoi_dong_gop": "baonv"
        },
        ...
    ],
    "pagination": {
        "current": 1,
        "pageSize": 10,
        "total": 50
    }
}
```

#### 2.4. Lấy danh sách món ăn của tôi

**Trong ApiHelper.cs:**
```csharp
public static async Task<(bool Success, MonAnListResponse? Response, string Message)> GetMyFoodsAsync(
    int page = 1, int pageSize = 10)
{
    using var client = CreateClient();
    var url = $"{_baseUrl}/api/v1/monan/my-dishes";

    // Tương tự GetAllFoodsAsync nhưng endpoint khác
    var requestData = new
    {
        current = page,
        pageSize = pageSize
    };

    var json = JsonConvert.SerializeObject(requestData);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(url, content);
    // ... xử lý tương tự
}
```

**Cấu trúc request:**
- **Method**: POST
- **URL**: `https://nt106.uitiot.vn/api/v1/monan/my-dishes`
- **Headers**: `Authorization: Bearer <token>`
- **Body**: `{"current": 1, "pageSize": 10}`

#### 2.5. Lấy thông tin user hiện tại

**Trong ApiHelper.cs:**
```csharp
public static async Task<(bool Success, User? Response, string Message)> GetCurrentUserAsync()
{
    using var client = CreateClient();
    var url = $"{_baseUrl}/api/v1/user/me";

    var response = await client.GetAsync(url);
    var responseString = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        var user = JsonConvert.DeserializeObject<User>(responseString);
        return (true, user, "Thành công");
    }
    
    var error = JsonConvert.DeserializeObject<ApiError>(responseString);
    return (false, null, error?.Detail ?? "Lỗi khi lấy thông tin user");
}
```

**Cấu trúc request:**
- **Method**: GET
- **URL**: `https://nt106.uitiot.vn/api/v1/user/me`
- **Headers**: `Authorization: Bearer <token>`

#### 2.6. Thiết lập Authorization Header

**Trong ApiHelper.cs:**
```csharp
private static HttpClient CreateClient()
{
    var client = new HttpClient();
    client.Timeout = TimeSpan.FromSeconds(30);
    
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    
    // Thêm Authorization header nếu có token
    if (!string.IsNullOrEmpty(_accessToken))
    {
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }
    
    return client;
}
```

**Mỗi request đều tự động thêm Authorization header nếu có token.**

---

### 3. Xác thực dữ liệu

Hệ thống xác thực dữ liệu ở cả phía Client và Server:

#### 3.1. Validation đăng nhập

**Trong LoginForm.cs:**
```csharp
if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
{
    lblLoginStatus.Text = "Vui lòng nhập đầy đủ thông tin!";
    lblLoginStatus.ForeColor = Color.Red;
    btnLogin.Enabled = true;
    return;
}
```

**Kiểm tra:**
- Username không được rỗng
- Password không được rỗng

#### 3.2. Validation đăng ký

**Trong LoginForm.cs:**
```csharp
// Username bắt buộc
if (string.IsNullOrEmpty(username))
{
    lblRegisterStatus.Text = "Vui lòng nhập Username!";
    return;
}

// Password bắt buộc và tối thiểu 6 ký tự
if (string.IsNullOrEmpty(password))
{
    lblRegisterStatus.Text = "Vui lòng nhập Password!";
    return;
}

if (password.Length < 6)
{
    lblRegisterStatus.Text = "Password phải có ít nhất 6 ký tự!";
    return;
}

// Phone format validation (nếu có)
if (!string.IsNullOrEmpty(phone))
{
    var phoneRegex = new System.Text.RegularExpressions.Regex(@"^[\d\+\-\(\)\s]+$");
    if (!phoneRegex.IsMatch(phone))
    {
        lblRegisterStatus.Text = "Số điện thoại chỉ được chứa số và ký tự +, -, (, )";
        return;
    }
}
```

**Kiểm tra:**
- Username: Bắt buộc, không được rỗng
- Password: Bắt buộc, tối thiểu 6 ký tự
- Phone: Tùy chọn, nếu có phải đúng format (chỉ số và ký tự +, -, (, ))

#### 3.3. Validation thêm món ăn

**Trong AddFoodForm.cs:**
```csharp
string name = txtName.Text.Trim();

if (string.IsNullOrEmpty(name))
{
    lblStatus.Text = "Vui lòng nhập tên món ăn!";
    lblStatus.ForeColor = Color.Red;
    return;
}
```

**Kiểm tra:**
- Tên món ăn: Bắt buộc, không được rỗng
- Giá: Tùy chọn (mặc định 0)
- Mô tả: Tùy chọn
- URL hình ảnh: Tùy chọn
- Địa chỉ: Tùy chọn

#### 3.4. Validation từ Server

**Xử lý lỗi 422 (Validation Error):**
```csharp
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
```

**Cấu trúc lỗi validation từ server:**
```json
{
    "detail": [
        {
            "type": "value_error",
            "loc": ["body", "username"],
            "msg": "Username đã tồn tại",
            "input": "phatpt"
        },
        ...
    ]
}
```

#### 3.5. Xử lý lỗi 401 (Unauthorized)

**Trong ApiHelper.cs:**
```csharp
if (response.StatusCode == HttpStatusCode.Unauthorized)
{
    return (false, null, "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
}
```

**Các trường hợp:**
- Token hết hạn
- Token không hợp lệ
- Chưa đăng nhập

#### 3.6. Xử lý lỗi 403 (Forbidden)

**Trong ApiHelper.cs:**
```csharp
if (response.StatusCode == HttpStatusCode.Forbidden)
{
    return (false, null, "Bạn không có quyền xóa món ăn này!");
}
```

**Các trường hợp:**
- Cố gắng xóa món ăn của người khác
- Không có quyền thực hiện thao tác

---

### 4. Xử lý dữ liệu

Sau khi xác thực và nhận response, hệ thống xử lý dữ liệu để hiển thị:

#### 4.1. Xử lý response đăng nhập

**Trong LoginForm.cs:**
```csharp
var (success, message, response) = await ApiHelper.LoginAsync(username, password);

if (success && response != null)
{
    // Token đã được lưu trong ApiHelper
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
```

**Các bước:**
1. Gọi API đăng nhập
2. Nếu thành công → Token được lưu trong `ApiHelper._accessToken`
3. Ẩn LoginForm và mở MainForm
4. Nếu thất bại → Hiển thị thông báo lỗi

#### 4.2. Xử lý response danh sách món ăn

**Trong MainForm.cs:**
```csharp
private async void LoadFoods()
{
    var flowPanel = GetCurrentFlowPanel();
    flowPanel.Controls.Clear();
    currentFoods.Clear();
    selectedFood = null;

    MonAnListResponse? response;
    bool success = false;

    if (showingAllFoods)
    {
        var result = await ApiHelper.GetAllFoodsAsync(currentPage, pageSize);
        success = result.Success;
        response = result.Response;
    }
    else
    {
        var result = await ApiHelper.GetMyFoodsAsync(currentPage, pageSize);
        success = result.Success;
        response = result.Response;
    }

    if (success && response != null)
    {
        var foods = response.Data ?? new List<MonAn>();
        currentFoods = foods;
        
        // Tạo food cards
        foreach (var food in foods)
        {
            var foodCard = CreateFoodCard(food);
            flowPanel.Controls.Add(foodCard);
        }

        // Cập nhật pagination
        int currentPageNum = response.Pagination?.Current ?? 1;
        int pageSizeNum = response.Pagination?.PageSize ?? pageSize;
        int totalItems = response.Pagination?.Total ?? 0;
        int totalPages = pageSizeNum > 0 ? (int)Math.Ceiling((double)totalItems / pageSizeNum) : 1;
        
        numPage.Maximum = totalPages;
        numPage.Value = currentPageNum > 0 ? Math.Min(currentPageNum, totalPages) : 1;
        currentPage = (int)numPage.Value;
        lblPageInfo.Text = $"Trang {currentPageNum} / {totalPages} (Tổng: {totalItems} món)";

        btnPrevPage.Enabled = currentPageNum > 1;
        btnNextPage.Enabled = currentPageNum < totalPages;
    }
}
```

**Các bước:**
1. Clear danh sách cũ
2. Gọi API lấy danh sách (tất cả hoặc của tôi)
3. Parse response để lấy `data` và `pagination`
4. Tạo food cards cho mỗi món ăn
5. Cập nhật thông tin phân trang

#### 4.3. Tạo Food Card

**Trong MainForm.cs:**
```csharp
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

    // PictureBox cho hình ảnh
    PictureBox picFood = new PictureBox();
    picFood.Size = new Size(260, 180);
    picFood.SizeMode = PictureBoxSizeMode.Zoom;
    
    string? imageUrl = food.HinhAnh;
    if (!string.IsNullOrEmpty(imageUrl) && Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
    {
        try
        {
            picFood.LoadAsync(imageUrl);
        }
        catch { ShowNoImagePlaceholder(picFood); }
    }
    else
    {
        ShowNoImagePlaceholder(picFood);
    }

    // Label tên món ăn
    Label lblName = new Label();
    lblName.Text = food.TenMonAn ?? "N/A";
    lblName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

    // Label giá
    Label lblPrice = new Label();
    lblPrice.Text = food.Gia > 0 ? $"💰 {food.Gia:N0} đ" : "💰 Giá: Liên hệ";
    lblPrice.ForeColor = Color.FromArgb(220, 53, 69);

    // Label địa chỉ
    Label lblAddress = new Label();
    lblAddress.Text = !string.IsNullOrEmpty(food.DiaChi) ? $"📍 {food.DiaChi}" : "📍 Địa chỉ: N/A";

    // Label người đóng góp
    Label lblContributor = new Label();
    lblContributor.Text = $"👤 {food.NguoiDongGop ?? "N/A"}";

    card.Controls.AddRange(new Control[] {
        picFood, lblName, lblPrice, lblAddress, lblContributor
    });

    return card;
}
```

**Cấu trúc Food Card:**
- **PictureBox**: Hiển thị hình ảnh món ăn (hoặc placeholder nếu không có)
- **Label tên**: Tên món ăn (bold)
- **Label giá**: Giá món ăn (màu đỏ)
- **Label địa chỉ**: Địa chỉ (màu xám)
- **Label người đóng góp**: Username người tạo (màu xám)

#### 4.4. Xử lý response thêm món ăn

**Trong AddFoodForm.cs:**
```csharp
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
```

**Các bước:**
1. Gọi API thêm món ăn
2. Nếu thành công → Hiển thị thông báo thành công, đợi 1 giây, đóng form
3. Nếu thất bại → Hiển thị thông báo lỗi, enable lại button

#### 4.5. Xử lý response xóa món ăn

**Trong MainForm.cs:**
```csharp
var (success, message) = await ApiHelper.DeleteFoodAsync(selectedFood.Id);

if (success)
{
    MessageBox.Show(message, "Thành công");
    selectedFood = null;
    LoadFoods(); // Reload danh sách
}
else
{
    MessageBox.Show(message, "Lỗi");
}
```

**Các bước:**
1. Gọi API xóa món ăn
2. Nếu thành công → Hiển thị thông báo, clear selection, reload danh sách
3. Nếu thất bại → Hiển thị thông báo lỗi

#### 4.6. Xử lý chọn ngẫu nhiên

**Trong ApiHelper.cs:**
```csharp
public static async Task<(bool Success, MonAn? Response, string Message)> GetRandomFoodAsync()
{
    // Lấy tất cả món ăn (100 món đầu tiên)
    var result = await GetAllFoodsAsync(1, 100);
    if (result.Success && result.Response?.Data != null && result.Response.Data.Count > 0)
    {
        var random = new Random();
        var randomFood = result.Response.Data[random.Next(result.Response.Data.Count)];
        return (true, randomFood, "Thành công");
    }
    
    return (false, null, "Không có món ăn nào");
}
```

**Các bước:**
1. Lấy danh sách món ăn (100 món đầu tiên)
2. Chọn ngẫu nhiên một món từ danh sách
3. Trả về món ăn được chọn

**Trong MainForm.cs:**
```csharp
private void ShowRandomFoodDialog(MonAn food, string title)
{
    Form dialog = new Form();
    dialog.Text = title;
    dialog.Size = new Size(500, 450);
    
    // Hiển thị thông tin món ăn
    Label lblTitle = new Label();
    lblTitle.Text = $"🎲 {food.TenMonAn}";
    lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
    
    Label lblPrice = new Label();
    lblPrice.Text = food.Gia > 0 ? $"💰 Giá: {food.Gia:N0} đ" : "💰 Giá: Liên hệ";
    
    // ... các label khác
    
    dialog.ShowDialog();
}
```

#### 4.7. Xử lý phân trang

**Cập nhật pagination info:**
```csharp
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
```

**Các bước:**
1. Lấy thông tin pagination từ response
2. Tính tổng số trang: `totalPages = ceil(totalItems / pageSize)`
3. Cập nhật NumericUpDown (min=1, max=totalPages, value=currentPage)
4. Cập nhật label hiển thị: "Trang X / Y (Tổng: Z món)"
5. Enable/disable nút Trước/Sau dựa trên vị trí trang hiện tại

---

### 5. Hiển thị kết quả

Hệ thống hiển thị kết quả cho người dùng thông qua giao diện:

#### 5.1. Hiển thị trạng thái đăng nhập

**Trong LoginForm.cs:**
```csharp
// Khi bắt đầu xử lý
btnLogin.Enabled = false;
lblLoginStatus.Text = "Đang xử lý...";
lblLoginStatus.ForeColor = Color.Blue;

// Khi thành công
lblLoginStatus.Text = message;
lblLoginStatus.ForeColor = Color.Green;

// Khi thất bại
lblLoginStatus.Text = message;
lblLoginStatus.ForeColor = Color.Red;
btnLogin.Enabled = true;
```

**Màu sắc:**
- **Xanh dương**: Đang xử lý
- **Xanh lá**: Thành công
- **Đỏ**: Thất bại

#### 5.2. Hiển thị danh sách món ăn

**Trong MainForm.cs:**
```csharp
// Tạo food cards và thêm vào FlowLayoutPanel
foreach (var food in foods)
{
    var foodCard = CreateFoodCard(food);
    flowPanel.Controls.Add(foodCard);
}

// Nếu không có món ăn
if (foods.Count == 0)
{
    Label lblNoFood = new Label();
    lblNoFood.Text = showingAllFoods 
        ? "Không có món ăn nào trong cộng đồng" 
        : "Bạn chưa có món ăn nào";
    lblNoFood.Font = new Font("Segoe UI", 12F);
    lblNoFood.ForeColor = Color.Gray;
    flowPanel.Controls.Add(lblNoFood);
}
```

**Layout:**
- **FlowLayoutPanel**: Tự động sắp xếp các cards theo dòng
- **WrapContents**: Tự động xuống dòng khi hết chỗ
- **ScrollPanel**: Có thanh cuộn nếu nội dung dài

#### 5.3. Hiển thị thông tin user

**Trong MainForm.cs:**
```csharp
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
```

**Format:**
- "Xin chào, [FullName] ([Username])" nếu có FullName
- "Xin chào, [Username] ([Username])" nếu không có FullName

#### 5.4. Hiển thị thông tin phân trang

**Trong MainForm.cs:**
```csharp
lblPageInfo.Text = $"Trang {currentPageNum} / {totalPages} (Tổng: {totalItems} món)";
```

**Format:**
- "Trang X / Y (Tổng: Z món)"
- Ví dụ: "Trang 1 / 5 (Tổng: 50 món)"

#### 5.5. Hiển thị dialog chọn ngẫu nhiên

**Trong MainForm.cs:**
```csharp
private void ShowRandomFoodDialog(MonAn food, string title)
{
    Form dialog = new Form();
    dialog.Text = title;
    dialog.Size = new Size(500, 450);
    dialog.StartPosition = FormStartPosition.CenterParent;

    Label lblTitle = new Label();
    lblTitle.Text = $"🎲 {food.TenMonAn}";
    lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
    lblTitle.TextAlign = ContentAlignment.MiddleCenter;

    Label lblPrice = new Label();
    lblPrice.Text = food.Gia > 0 ? $"💰 Giá: {food.Gia:N0} đ" : "💰 Giá: Liên hệ";
    lblPrice.ForeColor = Color.FromArgb(220, 53, 69);

    Label lblAddress = new Label();
    lblAddress.Text = $"📍 Địa chỉ: {food.DiaChi ?? "N/A"}";

    TextBox txtDescription = new TextBox();
    txtDescription.Text = food.MoTa ?? "Không có mô tả";
    txtDescription.Multiline = true;
    txtDescription.ReadOnly = true;
    txtDescription.ScrollBars = ScrollBars.Vertical;

    Label lblContributor = new Label();
    lblContributor.Text = $"👤 Người đóng góp: {food.NguoiDongGop ?? "N/A"}";

    Button btnClose = new Button();
    btnClose.Text = "Đóng";
    btnClose.DialogResult = DialogResult.OK;

    dialog.Controls.AddRange(new Control[] {
        lblTitle, lblPrice, lblAddress, txtDescription, lblContributor, btnClose
    });

    dialog.ShowDialog();
}
```

**Cấu trúc dialog:**
- **Title**: "🎲 [Tên món ăn]" (bold, lớn)
- **Giá**: "💰 Giá: [giá] đ" (màu đỏ)
- **Địa chỉ**: "📍 Địa chỉ: [địa chỉ]"
- **Mô tả**: TextBox multiline, readonly, có scrollbar
- **Người đóng góp**: "👤 Người đóng góp: [username]"
- **Nút Đóng**: Đóng dialog

#### 5.6. Hiển thị lỗi

**MessageBox cho lỗi:**
```csharp
MessageBox.Show($"Lỗi khi tải danh sách món ăn: {message}", "Lỗi", 
    MessageBoxButtons.OK, MessageBoxIcon.Error);
```

**Label cho lỗi:**
```csharp
Label lblError = new Label();
lblError.Text = $"Lỗi: {message}";
lblError.ForeColor = Color.Red;
lblError.TextAlign = ContentAlignment.MiddleCenter;
flowPanel.Controls.Add(lblError);
```

**Các loại hiển thị lỗi:**
- **MessageBox**: Cho lỗi quan trọng (xóa, thêm, load danh sách)
- **Label trong form**: Cho lỗi không nghiêm trọng
- **Status label**: Cho lỗi validation (màu đỏ)

#### 5.7. Hiển thị hình ảnh

**Trong MainForm.cs:**
```csharp
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
```

**Xử lý:**
- Nếu có URL hợp lệ → Load ảnh bất đồng bộ
- Nếu load lỗi hoặc không có URL → Hiển thị placeholder "📷\nKhông có hình"

---

## TÓM TẮT LUỒNG XỬ LÝ

```
1. Người dùng mở ứng dụng → LoginForm hiển thị
   ↓
2. Người dùng đăng nhập/đăng ký
   ↓
3. Validation dữ liệu đầu vào
   ↓
4. Gửi request đến API
   ↓
5. Nhận response và lưu token (nếu đăng nhập thành công)
   ↓
6. Mở MainForm
   ↓
7. Load thông tin user và danh sách món ăn
   ↓
8. Hiển thị danh sách món ăn dưới dạng cards
   ↓
9. Người dùng có thể:
   - Xem danh sách (tất cả hoặc của tôi)
   - Thêm món ăn mới
   - Xóa món ăn (click card → click xóa)
   - Chọn ngẫu nhiên
   - Điều hướng phân trang
   ↓
10. Mỗi thao tác:
    - Validation dữ liệu
    - Gửi request đến API
    - Nhận response
    - Cập nhật UI
```

---

## CẤU TRÚC FILE

```
Bai07/
├── Bai07.csproj          # File cấu hình project
├── Program.cs             # Entry point
├── Models.cs              # Model classes (đã chuyển sang ApiHelper.cs)
├── ApiHelper.cs           # API helper class - xử lý tất cả API calls
├── LoginForm.cs           # Form đăng nhập/đăng ký
├── MainForm.cs            # Form chính với các chức năng
├── AddFoodForm.cs         # Form thêm món ăn
├── App.config             # Configuration
└── README.md              # Hướng dẫn sử dụng
```

---

## HƯỚNG DẪN SỬ DỤNG

### Khởi động ứng dụng

1. Chạy: `dotnet run` trong thư mục `Bai07`
2. Hoặc chạy file `.exe` từ thư mục `bin/Debug/net8.0-windows/`

### Quy trình sử dụng

1. **Đăng ký/Đăng nhập**
   - Mở ứng dụng → Form đăng nhập hiển thị
   - Tab "Đăng nhập": Nhập username và password
   - Tab "Đăng ký": Tạo tài khoản mới
   - Sau khi đăng nhập thành công, chuyển sang MainForm

2. **Xem danh sách món ăn**
   - Tab "Tất cả món ăn": Xem tất cả món ăn từ cộng đồng
   - Tab "Món ăn của tôi": Xem chỉ món ăn do bản thân tạo
   - Sử dụng phân trang ở dưới để điều hướng

3. **Thêm món ăn mới**
   - Click nút "➕ Thêm món ăn"
   - Nhập tên món ăn (bắt buộc)
   - Nhập giá, địa chỉ, mô tả, URL hình ảnh (tùy chọn)
   - Click "Thêm món ăn"

4. **Xóa món ăn**
   - Click vào card món ăn để chọn
   - Click nút "🗑️ Xóa món ăn"
   - Xác nhận xóa

5. **Chọn ngẫu nhiên**
   - "🎲 Ngẫu nhiên (Cộng đồng)": Chọn ngẫu nhiên từ tất cả món ăn
   - "🎲 Ngẫu nhiên (Của tôi)": Chọn ngẫu nhiên từ món ăn của bản thân
   - Hiển thị dialog với thông tin món ăn được chọn

6. **Phân trang**
   - Chọn số món ăn/trang (5-50)
   - Dùng nút "Trước"/"Sau" để điều hướng
   - Hoặc nhập số trang trực tiếp
   - Click "🔄 Làm mới" để tải lại danh sách

7. **Đăng xuất**
   - Click nút "Đăng xuất" ở góc phải trên
   - Quay lại form đăng nhập

---

## API ENDPOINTS

### Authentication
- `POST /api/v1/user/signup` - Đăng ký
- `POST /auth/token` - Đăng nhập

### Meals (Món ăn)
- `POST /api/v1/monan/all` - Lấy danh sách tất cả món ăn (với pagination)
- `POST /api/v1/monan/my-dishes` - Lấy danh sách món ăn của bản thân (với pagination)
- `POST /api/v1/monan/add` - Thêm món ăn mới
- `DELETE /api/v1/monan/{id}` - Xóa món ăn
- `GET /api/v1/monan/{id}` - Lấy thông tin món ăn theo ID
- `PUT /api/v1/monan/{id}` - Cập nhật món ăn

### User
- `GET /api/v1/user/me` - Lấy thông tin user hiện tại

**Tài liệu API**: https://nt106.uitiot.vn/docs

---

## XỬ LÝ LỖI

### Lỗi validation

- **Username rỗng**: "Vui lòng nhập Username!"
- **Password rỗng**: "Vui lòng nhập Password!"
- **Password < 6 ký tự**: "Password phải có ít nhất 6 ký tự!"
- **Phone format sai**: "Số điện thoại chỉ được chứa số và ký tự +, -, (, )"
- **Tên món ăn rỗng**: "Vui lòng nhập tên món ăn!"

### Lỗi từ server

- **401 Unauthorized**: "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại."
- **403 Forbidden**: "Bạn không có quyền xóa món ăn này!"
- **422 Unprocessable Entity**: Hiển thị chi tiết lỗi validation từ server

### Lỗi kết nối

- **Network Error**: "Lỗi: [thông báo lỗi]"
- **Timeout**: "Lỗi: The operation timed out"

---

## KIỂM TRA VÀ TEST

### Checklist kiểm tra

- [ ] Đăng ký tài khoản mới thành công
- [ ] Đăng nhập thành công
- [ ] Hiển thị thông tin user
- [ ] Load danh sách tất cả món ăn
- [ ] Load danh sách món ăn của tôi
- [ ] Phân trang hoạt động đúng (Trước/Sau/Nhập số trang)
- [ ] Thay đổi số món/trang hoạt động
- [ ] Thêm món ăn mới thành công
- [ ] Xóa món ăn thành công
- [ ] Chọn ngẫu nhiên từ cộng đồng
- [ ] Chọn ngẫu nhiên từ món ăn của tôi
- [ ] Hiển thị hình ảnh món ăn (hoặc placeholder)
- [ ] Đăng xuất thành công
- [ ] Xử lý lỗi khi token hết hạn
- [ ] Validation input đúng

---

**Ngày tạo**: 2024  
**Phiên bản**: 1.0  
**Ứng dụng**: Bai07 - Hôm nay ăn gì?

