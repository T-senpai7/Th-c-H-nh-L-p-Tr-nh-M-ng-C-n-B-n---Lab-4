# BÀI 5: HTTP POST - ĐĂNG NHẬP QUA API

## Ý TƯỞNG CHÍNH

Ứng dụng đăng nhập qua HTTP POST API cho phép người dùng xác thực với server thông qua giao thức HTTP. Hệ thống sử dụng:

- **HTTP POST Request**: Gửi thông tin đăng nhập (username và password) dưới dạng form-data
- **JSON Response**: Nhận và parse JSON response từ server để lấy access token
- **Windows Forms UI**: Giao diện desktop đơn giản, dễ sử dụng
- **Async/Await Pattern**: Xử lý bất đồng bộ để không block UI thread

### Kiến trúc hệ thống:

```
┌─────────────┐         HTTP POST         ┌─────────────┐
│   Client    │ ───────────────────────▶ │   Server    │
│ (Windows    │  multipart/form-data      │  (API)      │
│  Forms)     │                            │             │
│             │ ◀───────────────────────── │             │
│             │      JSON Response         │             │
└─────────────┘      (access_token)       └─────────────┘
```

### Luồng xử lý:

1. **Người dùng nhập thông tin**: URL, Username, Password
2. **Validation**: Kiểm tra dữ liệu đầu vào
3. **Gửi HTTP POST**: Tạo multipart/form-data và gửi đến API
4. **Nhận Response**: Parse JSON để lấy access token hoặc thông báo lỗi
5. **Hiển thị kết quả**: Token hoặc thông báo lỗi trên UI

### Tính năng chính:

1. **Đăng nhập qua API**: Xác thực với server thông qua HTTP POST
2. **Validation đầu vào**: Kiểm tra URL, Username, Password không được rỗng
3. **Xử lý lỗi**: Hiển thị thông báo lỗi rõ ràng khi đăng nhập thất bại hoặc lỗi kết nối
4. **Async processing**: Không block UI khi đang xử lý request
5. **Format output**: Hiển thị token theo format chuẩn (Bearer token)

---

## CÁC BƯỚC THỰC HIỆN

### 1. Nhận sự kiện người dùng

Hệ thống nhận và xử lý các sự kiện từ người dùng thông qua giao diện Windows Forms:

#### 1.1. Sự kiện click nút LOGIN

**Trong Form1.cs:**
```csharp
private async void btnLogin_Click(object sender, EventArgs e)
{
    // Disable button để tránh click nhiều lần
    btnLogin.Enabled = false;
    txtResult.Clear();
    txtResult.Text = "Đang xử lý...\r\n";
    
    try
    {
        // Lấy dữ liệu từ các TextBox
        string url = txtUrl.Text.Trim();
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;
        
        // Gọi hàm xử lý đăng nhập
        await LoginAsync(url, username, password);
    }
    catch (Exception ex)
    {
        // Xử lý lỗi nếu có
        txtResult.Text = $"Lỗi: {ex.Message}\r\n";
    }
    finally
    {
        // Enable lại button sau khi xong
        btnLogin.Enabled = true;
    }
}
```

**Các hành động khi click LOGIN:**
- Disable nút LOGIN để tránh click nhiều lần
- Clear vùng kết quả
- Hiển thị "Đang xử lý..." để người dùng biết hệ thống đang làm việc
- Lấy dữ liệu từ các TextBox (URL, Username, Password)
- Trim khoảng trắng thừa ở đầu/cuối
- Gọi hàm `LoginAsync()` để xử lý đăng nhập

#### 1.2. Sự kiện nhập liệu

**Validation khi nhập:**
- Người dùng có thể nhập URL, Username, Password vào các TextBox
- Password được ẩn bằng ký tự `*` (PasswordChar = '*')
- Hệ thống không validate real-time, chỉ validate khi click LOGIN

**Xử lý khoảng trắng:**
```csharp
string url = txtUrl.Text.Trim();      // Loại bỏ khoảng trắng đầu/cuối
string username = txtUsername.Text.Trim();
string password = txtPassword.Text;   // Password không trim (có thể có khoảng trắng hợp lệ)
```

#### 1.3. Sự kiện thay đổi input trong khi xử lý

**Xử lý:**
- Khi người dùng click LOGIN, dữ liệu được lấy ngay lập tức
- Nếu người dùng thay đổi input trong khi request đang chạy, request vẫn sử dụng dữ liệu cũ
- Điều này đảm bảo tính nhất quán của dữ liệu

---

### 2. Đọc dữ liệu từ web

Hệ thống đọc dữ liệu từ server thông qua HTTP POST request:

#### 2.1. Tạo HTTP POST request

**Tạo MultipartFormDataContent (`Form1.cs`):**
```csharp
private async Task LoginAsync(string url, string username, string password)
{
    using (var client = new HttpClient())
    {
        // Tạo form-data content
        var content = new MultipartFormDataContent
        {
            { new StringContent(username), "username" },
            { new StringContent(password), "password" }
        };
        
        // Gửi POST request
        var response = await client.PostAsync(url, content);
    }
}
```

**Cấu trúc request:**
- **Method**: POST
- **Content-Type**: `multipart/form-data`
- **Body**: 
  - `username`: Giá trị từ TextBox Username
  - `password`: Giá trị từ TextBox Password

**Ví dụ request:**
```
POST https://nt106.uitiot.vn/auth/token HTTP/1.1
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary...

------WebKitFormBoundary...
Content-Disposition: form-data; name="username"

phatpt
------WebKitFormBoundary...
Content-Disposition: form-data; name="password"

123456
------WebKitFormBoundary...--
```

#### 2.2. Gửi request và nhận response

**Gửi request:**
```csharp
// Gửi POST request
var response = await client.PostAsync(url, content);

// Đọc response dưới dạng string
var responseString = await response.Content.ReadAsStringAsync();
```

**Thông tin response:**
- **Status Code**: 200 (thành công), 401 (unauthorized), 422 (validation error), etc.
- **Content-Type**: `application/json`
- **Body**: JSON string chứa token hoặc thông báo lỗi

#### 2.3. Parse JSON response

**Parse JSON (`Form1.cs`):**
```csharp
// Parse JSON response
var responseObject = JObject.Parse(responseString);

// Lấy các trường từ JSON
var tokenType = responseObject["token_type"]?.ToString() ?? "";
var accessToken = responseObject["access_token"]?.ToString() ?? "";
var detail = responseObject["detail"]?.ToString() ?? "Không có thông tin chi tiết";
```

**Cấu trúc JSON response:**

**Thành công (200 OK):**
```json
{
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "token_type": "bearer"
}
```

**Thất bại (401 Unauthorized):**
```json
{
    "detail": "Incorrect username or password"
}
```

**Thất bại (422 Unprocessable Entity):**
```json
{
    "detail": "Field required"
}
```

#### 2.4. Xử lý lỗi kết nối

**Catch HttpRequestException:**
```csharp
catch (HttpRequestException ex)
{
    txtResult.Text = $"Lỗi kết nối: {ex.Message}\r\n";
    if (ex.InnerException != null)
    {
        txtResult.Text += $"Chi tiết: {ex.InnerException.Message}\r\n";
    }
}
```

**Các loại lỗi kết nối:**
- **DNS Resolution Error**: Không thể resolve domain
- **Connection Timeout**: Server không phản hồi trong thời gian quy định
- **Network Error**: Mất kết nối Internet
- **SSL/TLS Error**: Lỗi chứng chỉ SSL

---

### 3. Xác thực dữ liệu

Hệ thống xác thực dữ liệu ở phía Client trước khi gửi request:

#### 3.1. Validation URL

**Kiểm tra URL không rỗng:**
```csharp
if (string.IsNullOrEmpty(url))
{
    MessageBox.Show("Vui lòng nhập URL!", "Lỗi", 
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
    btnLogin.Enabled = true;
    return;
}
```

**Xử lý:**
- Kiểm tra URL có giá trị không
- Nếu rỗng → Hiển thị MessageBox cảnh báo
- Không gửi request
- Enable lại nút LOGIN

**Lưu ý:**
- Hệ thống không validate format URL (có thể là URL không hợp lệ)
- Validation format URL sẽ được xử lý bởi HttpClient khi gửi request

#### 3.2. Validation Username

**Kiểm tra Username không rỗng:**
```csharp
if (string.IsNullOrEmpty(username))
{
    MessageBox.Show("Vui lòng nhập Username!", "Lỗi", 
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
    btnLogin.Enabled = true;
    return;
}
```

**Xử lý:**
- Trim khoảng trắng đầu/cuối trước khi kiểm tra
- Nếu rỗng sau khi trim → Hiển thị MessageBox
- Không gửi request nếu thiếu Username

#### 3.3. Validation Password

**Kiểm tra Password không rỗng:**
```csharp
if (string.IsNullOrEmpty(password))
{
    MessageBox.Show("Vui lòng nhập Password!", "Lỗi", 
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
    btnLogin.Enabled = true;
    return;
}
```

**Xử lý:**
- Password không được trim (có thể có khoảng trắng hợp lệ)
- Nếu rỗng → Hiển thị MessageBox
- Không gửi request nếu thiếu Password

#### 3.4. Validation từ Server

**Server validation:**
- Nếu Username/Password sai → Server trả về 401 Unauthorized
- Nếu thiếu field → Server trả về 422 Unprocessable Entity
- Nếu format không đúng → Server trả về lỗi tương ứng

**Xử lý response từ server:**
```csharp
if (!response.IsSuccessStatusCode)
{
    // Login failed - show detail
    var detail = responseObject["detail"]?.ToString() ?? "Không có thông tin chi tiết";
    txtResult.Text = $"Detail: {detail}\r\n";
    txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
}
```

#### 3.5. Xử lý lỗi parse JSON

**Nếu response không phải JSON hợp lệ:**
```csharp
catch (Exception ex)
{
    txtResult.Text = $"Lỗi: {ex.Message}\r\n";
    if (ex.InnerException != null)
    {
        txtResult.Text += $"Chi tiết: {ex.InnerException.Message}\r\n";
    }
}
```

**Các trường hợp:**
- Response không phải JSON → Exception khi parse
- Response rỗng → Exception khi parse
- Response format sai → Exception khi parse

---

### 4. Xử lý dữ liệu

Sau khi xác thực và nhận response, hệ thống xử lý dữ liệu để hiển thị kết quả:

#### 4.1. Xử lý response thành công

**Kiểm tra status code:**
```csharp
if (response.IsSuccessStatusCode)  // 200-299
{
    // Login successful
    var tokenType = responseObject["token_type"]?.ToString() ?? "";
    var accessToken = responseObject["access_token"]?.ToString() ?? "";
    
    // Format output
    txtResult.Text = "Bearer\r\n";
    txtResult.Text += $"{accessToken}\r\n";
    txtResult.Text += "\r\n";
    txtResult.Text += "Đăng nhập thành công\r\n";
}
```

**Các bước xử lý:**
1. Kiểm tra `response.IsSuccessStatusCode` (200-299)
2. Lấy `token_type` từ JSON (thường là "bearer")
3. Lấy `access_token` từ JSON (JWT token)
4. Format output theo yêu cầu:
   - Dòng 1: "Bearer"
   - Dòng 2: Access token
   - Dòng 3: Dòng trống
   - Dòng 4: "Đăng nhập thành công"

#### 4.2. Xử lý response thất bại

**Kiểm tra status code lỗi:**
```csharp
if (!response.IsSuccessStatusCode)
{
    // Login failed - show detail
    var detail = responseObject["detail"]?.ToString() ?? "Không có thông tin chi tiết";
    txtResult.Text = $"Detail: {detail}\r\n";
    txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
}
```

**Các loại lỗi:**
- **401 Unauthorized**: Username/Password sai
  - Detail: "Incorrect username or password"
- **422 Unprocessable Entity**: Validation error từ server
  - Detail: Thông báo lỗi cụ thể (ví dụ: "Field required")
- **500 Internal Server Error**: Lỗi server
  - Detail: Thông báo lỗi từ server

#### 4.3. Xử lý lỗi kết nối

**Catch HttpRequestException:**
```csharp
catch (HttpRequestException ex)
{
    txtResult.Text = $"Lỗi kết nối: {ex.Message}\r\n";
    if (ex.InnerException != null)
    {
        txtResult.Text += $"Chi tiết: {ex.InnerException.Message}\r\n";
    }
}
```

**Các loại lỗi kết nối:**
- **DNS Resolution Error**: Không thể resolve domain
  - Message: "The remote name could not be resolved"
- **Connection Timeout**: Server không phản hồi
  - Message: "The operation timed out"
- **Network Error**: Mất kết nối
  - Message: "An error occurred while sending the request"

#### 4.4. Xử lý exception chung

**Catch tất cả exception:**
```csharp
catch (Exception ex)
{
    txtResult.Text = $"Lỗi: {ex.Message}\r\n";
    if (ex.InnerException != null)
    {
        txtResult.Text += $"Chi tiết: {ex.InnerException.Message}\r\n";
    }
}
```

**Các exception có thể xảy ra:**
- **JsonException**: Lỗi parse JSON
- **HttpRequestException**: Lỗi HTTP request
- **TaskCanceledException**: Request bị cancel
- **Exception**: Các lỗi khác

#### 4.5. Async/Await Pattern

**Sử dụng async/await để không block UI:**
```csharp
private async void btnLogin_Click(object sender, EventArgs e)
{
    // ...
    await LoginAsync(url, username, password);
    // ...
}

private async Task LoginAsync(string url, string username, string password)
{
    // ...
    var response = await client.PostAsync(url, content);
    var responseString = await response.Content.ReadAsStringAsync();
    // ...
}
```

**Lợi ích:**
- UI không bị đơ khi đang xử lý request
- Người dùng có thể thấy "Đang xử lý..." trong khi chờ
- Nút LOGIN được disable để tránh click nhiều lần

---

### 5. Hiển thị kết quả

Hệ thống hiển thị kết quả cho người dùng thông qua RichTextBox:

#### 5.1. Hiển thị trạng thái xử lý

**Khi bắt đầu xử lý:**
```csharp
btnLogin.Enabled = false;  // Disable button
txtResult.Clear();         // Clear kết quả cũ
txtResult.Text = "Đang xử lý...\r\n";  // Hiển thị trạng thái
```

**Mục đích:**
- Thông báo cho người dùng biết hệ thống đang xử lý
- Tránh click nhiều lần (button bị disable)
- Clear kết quả cũ để hiển thị kết quả mới

#### 5.2. Hiển thị kết quả thành công

**Format output khi đăng nhập thành công:**
```csharp
txtResult.Text = "Bearer\r\n";
txtResult.Text += $"{accessToken}\r\n";
txtResult.Text += "\r\n";
txtResult.Text += "Đăng nhập thành công\r\n";
```

**Ví dụ output:**
```
Bearer
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InBoYXRwdCIsImV4cCI6MTcxMzYyMTA0N30.re7JotDf35TM83qpLxVlbiAZIBv1esy_92Ye-xXXgDY

Đăng nhập thành công
```

**Cấu trúc:**
- **Dòng 1**: "Bearer" (token type)
- **Dòng 2**: Access token (JWT token)
- **Dòng 3**: Dòng trống
- **Dòng 4**: "Đăng nhập thành công"

#### 5.3. Hiển thị kết quả thất bại

**Format output khi đăng nhập thất bại:**
```csharp
var detail = responseObject["detail"]?.ToString() ?? "Không có thông tin chi tiết";
txtResult.Text = $"Detail: {detail}\r\n";
txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
```

**Ví dụ output (401 Unauthorized):**
```
Detail: Incorrect username or password
Status Code: 401 Unauthorized
```

**Ví dụ output (422 Unprocessable Entity):**
```
Detail: Field required
Status Code: 422 Unprocessable Entity
```

**Cấu trúc:**
- **Dòng 1**: "Detail: [thông báo lỗi từ server]"
- **Dòng 2**: "Status Code: [mã lỗi] [tên lỗi]"

#### 5.4. Hiển thị lỗi kết nối

**Format output khi lỗi kết nối:**
```csharp
txtResult.Text = $"Lỗi kết nối: {ex.Message}\r\n";
if (ex.InnerException != null)
{
    txtResult.Text += $"Chi tiết: {ex.InnerException.Message}\r\n";
}
```

**Ví dụ output (DNS Error):**
```
Lỗi kết nối: The remote name could not be resolved
Chi tiết: [chi tiết lỗi nếu có]
```

**Ví dụ output (Timeout):**
```
Lỗi kết nối: The operation timed out
Chi tiết: [chi tiết lỗi nếu có]
```

**Cấu trúc:**
- **Dòng 1**: "Lỗi kết nối: [thông báo lỗi]"
- **Dòng 2** (nếu có): "Chi tiết: [chi tiết lỗi]"

#### 5.5. Hiển thị lỗi chung

**Format output khi có exception:**
```csharp
txtResult.Text = $"Lỗi: {ex.Message}\r\n";
if (ex.InnerException != null)
{
    txtResult.Text += $"Chi tiết: {ex.InnerException.Message}\r\n";
}
```

**Ví dụ output (JSON Parse Error):**
```
Lỗi: Unexpected character encountered while parsing value
Chi tiết: [chi tiết lỗi nếu có]
```

#### 5.6. Cập nhật trạng thái button

**Sau khi xử lý xong:**
```csharp
finally
{
    btnLogin.Enabled = true;  // Enable lại button
}
```

**Mục đích:**
- Cho phép người dùng thử lại nếu có lỗi
- Đảm bảo button luôn được enable sau khi xử lý xong (dù thành công hay thất bại)

#### 5.7. Format và hiển thị

**RichTextBox properties:**
- **ReadOnly**: `true` - Người dùng không thể chỉnh sửa kết quả
- **Multiline**: `true` - Hiển thị nhiều dòng
- **ScrollBars**: `Vertical` - Có thanh cuộn nếu nội dung dài

**Cách hiển thị:**
- Sử dụng `\r\n` để xuống dòng (Windows line ending)
- Mỗi thông báo trên một dòng riêng
- Format rõ ràng, dễ đọc

---

## TÓM TẮT LUỒNG XỬ LÝ

```
1. Người dùng nhập URL, Username, Password
   ↓
2. Người dùng click nút "LOGIN"
   ↓
3. Disable button, hiển thị "Đang xử lý..."
   ↓
4. Validation dữ liệu đầu vào (URL, Username, Password)
   ↓
5. Nếu validation fail → Hiển thị MessageBox, enable lại button
   ↓
6. Nếu validation pass → Tạo MultipartFormDataContent
   ↓
7. Gửi HTTP POST request đến API
   ↓
8. Nhận response từ server
   ↓
9. Parse JSON response
   ↓
10. Kiểm tra status code
    ↓
11. Nếu thành công (200-299):
    - Lấy access_token và token_type
    - Format output: "Bearer\n[token]\n\nĐăng nhập thành công"
    ↓
12. Nếu thất bại:
    - Lấy detail từ JSON
    - Format output: "Detail: [detail]\nStatus Code: [code]"
    ↓
13. Nếu lỗi kết nối:
    - Format output: "Lỗi kết nối: [message]\nChi tiết: [detail]"
    ↓
14. Enable lại button
    ↓
15. Hiển thị kết quả trong RichTextBox
```

---

## CẤU TRÚC FILE

```
Bai05/
├── Bai05.csproj          # File cấu hình project, khai báo dependencies
├── Program.cs            # Entry point của ứng dụng
├── Form1.cs              # Logic xử lý form và HTTP POST request
├── Form1.Designer.cs     # Thiết kế giao diện (auto-generated)
├── Form1.resx            # Resource file cho form
├── App.config             # File cấu hình ứng dụng
├── README.md              # Hướng dẫn sử dụng tổng quát
├── HUONG_DAN_TEST.md      # Hướng dẫn test chi tiết
├── TEST_CASES.md         # Danh sách test cases
└── Document_check.md     # File này - Tài liệu kiểm tra
```

---

## HƯỚNG DẪN SỬ DỤNG

### Khởi động ứng dụng

1. Chạy: `dotnet run` trong thư mục `Bai05`
2. Hoặc chạy file `.exe` từ thư mục `bin/Debug/net8.0-windows/`

### Sử dụng

1. **Kiểm tra URL**: Mặc định là `https://nt106.uitiot.vn/auth/token`
2. **Nhập Username**: Mặc định là `phatpt` (có thể thay đổi)
3. **Nhập Password**: Nhập password của tài khoản
4. **Click LOGIN**: Chờ kết quả hiển thị

### Kết quả mong đợi

**Thành công:**
```
Bearer
[access_token]
Đăng nhập thành công
```

**Thất bại:**
```
Detail: Incorrect username or password
Status Code: 401 Unauthorized
```

---

## API ENDPOINT

**URL**: `https://nt106.uitiot.vn/auth/token`

**Method**: POST

**Content-Type**: `multipart/form-data`

**Body**:
- `username`: Tên đăng nhập
- `password`: Mật khẩu

**Response thành công (200 OK)**:
```json
{
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "token_type": "bearer"
}
```

**Response thất bại (401 Unauthorized)**:
```json
{
    "detail": "Incorrect username or password"
}
```

**Tài liệu API**: https://nt106.uitiot.vn/docs

---

## XỬ LÝ LỖI

### Lỗi validation

- **URL rỗng**: Hiển thị MessageBox "Vui lòng nhập URL!"
- **Username rỗng**: Hiển thị MessageBox "Vui lòng nhập Username!"
- **Password rỗng**: Hiển thị MessageBox "Vui lòng nhập Password!"

### Lỗi kết nối

- **DNS Error**: "Lỗi kết nối: The remote name could not be resolved"
- **Timeout**: "Lỗi kết nối: The operation timed out"
- **Network Error**: "Lỗi kết nối: [thông báo lỗi]"

### Lỗi từ server

- **401 Unauthorized**: Username/Password sai
- **422 Unprocessable Entity**: Validation error
- **500 Internal Server Error**: Lỗi server

---

## KIỂM TRA VÀ TEST

Xem các file:
- **HUONG_DAN_TEST.md**: Hướng dẫn test từng bước
- **TEST_CASES.md**: Danh sách đầy đủ test cases

### Checklist kiểm tra

- [ ] Validation URL rỗng
- [ ] Validation Username rỗng
- [ ] Validation Password rỗng
- [ ] Đăng nhập thành công với tài khoản hợp lệ
- [ ] Đăng nhập thất bại với password sai
- [ ] Đăng nhập thất bại với username sai
- [ ] Xử lý lỗi kết nối (mất Internet)
- [ ] Xử lý URL không hợp lệ
- [ ] Button disable khi đang xử lý
- [ ] Hiển thị "Đang xử lý..." khi request đang chạy
- [ ] Format output đúng khi thành công
- [ ] Format output đúng khi thất bại

---

**Ngày tạo**: 2024  
**Phiên bản**: 1.0  
**Ứng dụng**: Bai05 - HTTP POST Login

