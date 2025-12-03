# BÀI 6: HTTP GET - LẤY THÔNG TIN NGƯỜI DÙNG QUA API

## Ý TƯỞNG CHÍNH

Ứng dụng lấy thông tin người dùng qua HTTP GET API cho phép người dùng xem thông tin cá nhân của mình sau khi đã đăng nhập. Hệ thống sử dụng:

- **HTTP GET Request**: Gửi request đến API endpoint để lấy thông tin user
- **JWT Authentication**: Sử dụng JWT (JSON Web Token) trong Authorization header để xác thực
- **JSON Response**: Nhận và parse JSON response từ server để hiển thị thông tin user
- **Windows Forms UI**: Giao diện desktop đơn giản, dễ sử dụng
- **Async/Await Pattern**: Xử lý bất đồng bộ để không block UI thread

### Kiến trúc hệ thống:

```
┌─────────────┐         HTTP GET          ┌─────────────┐
│   Client    │ ───────────────────────▶ │   Server    │
│ (Windows    │  Authorization: Bearer    │  (API)      │
│  Forms)     │  <access_token>           │             │
│             │                            │             │
│             │ ◀───────────────────────── │             │
│             │      JSON Response         │             │
│             │      (user info)           │             │
└─────────────┘                            └─────────────┘
```

### Luồng xử lý:

1. **Người dùng nhập thông tin**: URL, Token Type, Access Token (từ Bài 5)
2. **Validation**: Kiểm tra dữ liệu đầu vào
3. **Thiết lập Authorization Header**: Tạo header với format `Bearer <access_token>`
4. **Gửi HTTP GET**: Gửi request đến API endpoint
5. **Nhận Response**: Parse JSON để lấy thông tin user hoặc thông báo lỗi
6. **Hiển thị kết quả**: Format và hiển thị thông tin user trên UI

### Tính năng chính:

1. **Lấy thông tin user qua API**: Sử dụng JWT token để xác thực và lấy thông tin
2. **Validation đầu vào**: Kiểm tra URL, Token Type, Access Token không được rỗng
3. **Xử lý lỗi**: Hiển thị thông báo lỗi rõ ràng khi token hết hạn, không hợp lệ hoặc lỗi kết nối
4. **Async processing**: Không block UI khi đang xử lý request
5. **Format output**: Hiển thị thông tin user dễ đọc kèm JSON response đầy đủ

---

## CÁC BƯỚC THỰC HIỆN

### 1. Nhận sự kiện người dùng

Hệ thống nhận và xử lý các sự kiện từ người dùng thông qua giao diện Windows Forms:

#### 1.1. Sự kiện click nút GET USER INFO

**Trong Form1.cs:**
```csharp
private async void btnGetUserInfo_Click(object sender, EventArgs e)
{
    // Disable button để tránh click nhiều lần
    btnGetUserInfo.Enabled = false;
    txtResult.Clear();
    txtResult.Text = "Đang xử lý...\r\n";
    
    try
    {
        // Lấy dữ liệu từ các TextBox
        string url = txtUrl.Text.Trim();
        string tokenType = txtTokenType.Text.Trim();
        string accessToken = txtAccessToken.Text.Trim();
        
        // Gọi hàm xử lý lấy thông tin user
        await GetUserInfoAsync(url, tokenType, accessToken);
    }
    catch (Exception ex)
    {
        // Xử lý lỗi nếu có
        txtResult.Text = $"Lỗi: {ex.Message}\r\n";
    }
    finally
    {
        // Enable lại button sau khi xong
        btnGetUserInfo.Enabled = true;
    }
}
```

**Các hành động khi click GET USER INFO:**
- Disable nút GET USER INFO để tránh click nhiều lần
- Clear vùng kết quả
- Hiển thị "Đang xử lý..." để người dùng biết hệ thống đang làm việc
- Lấy dữ liệu từ các TextBox (URL, Token Type, Access Token)
- Trim khoảng trắng thừa ở đầu/cuối
- Gọi hàm `GetUserInfoAsync()` để xử lý request

#### 1.2. Sự kiện nhập liệu

**Validation khi nhập:**
- Người dùng có thể nhập URL, Token Type, Access Token vào các TextBox
- Token Type mặc định là "Bearer" (thường không cần thay đổi)
- Access Token được lấy từ Bài 5 (HTTP POST Login)
- Hệ thống không validate real-time, chỉ validate khi click GET USER INFO

**Xử lý khoảng trắng:**
```csharp
string url = txtUrl.Text.Trim();              // Loại bỏ khoảng trắng đầu/cuối
string tokenType = txtTokenType.Text.Trim();  // Loại bỏ khoảng trắng đầu/cuối
string accessToken = txtAccessToken.Text.Trim(); // Loại bỏ khoảng trắng đầu/cuối
```

**Lưu ý về Access Token:**
- Token được copy từ Bài 5 sau khi đăng nhập thành công
- Token có thể có khoảng trắng thừa khi copy → Cần trim
- Token có thời gian hết hạn → Cần lấy token mới nếu hết hạn

#### 1.3. Sự kiện thay đổi input trong khi xử lý

**Xử lý:**
- Khi người dùng click GET USER INFO, dữ liệu được lấy ngay lập tức
- Nếu người dùng thay đổi input trong khi request đang chạy, request vẫn sử dụng dữ liệu cũ
- Điều này đảm bảo tính nhất quán của dữ liệu

---

### 2. Đọc dữ liệu từ web

Hệ thống đọc dữ liệu từ server thông qua HTTP GET request với JWT authentication:

#### 2.1. Thiết lập Authorization Header

**Tạo AuthenticationHeaderValue (`Form1.cs`):**
```csharp
private async Task GetUserInfoAsync(string url, string tokenType, string accessToken)
{
    using (var client = new HttpClient())
    {
        // Thiết lập Authorization header với JWT token
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue(tokenType, accessToken);
        
        // Gửi GET request
        var response = await client.GetAsync(url);
    }
}
```

**Cấu trúc Authorization Header:**
- **Format**: `Authorization: <token_type> <access_token>`
- **Ví dụ**: `Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
- **Token Type**: Thường là "Bearer" (có thể là "Basic", "Digest", etc.)
- **Access Token**: JWT token từ Bài 5

**Ví dụ HTTP Request:**
```
GET https://nt106.uitiot.vn/api/v1/user/me HTTP/1.1
Host: nt106.uitiot.vn
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InBoYXRwdCIsImV4cCI6MTcxMzYyMTA0N30.re7JotDf35TM83qpLxVlbiAZIBv1esy_92Ye-xXXgDY
```

#### 2.2. Gửi HTTP GET request

**Gửi request:**
```csharp
// Gửi GET request
var response = await client.GetAsync(url);

// Đọc response dưới dạng string
var responseString = await response.Content.ReadAsStringAsync();
```

**Thông tin response:**
- **Status Code**: 200 (thành công), 401 (unauthorized), 404 (not found), etc.
- **Content-Type**: `application/json`
- **Body**: JSON string chứa thông tin user hoặc thông báo lỗi

#### 2.3. Parse JSON response

**Parse JSON (`Form1.cs`):**
```csharp
// Parse JSON response
var userObject = JObject.Parse(responseString);

// Lấy các trường từ JSON
var id = userObject["id"]?.ToString();
var username = userObject["username"]?.ToString();
var email = userObject["email"]?.ToString();
var fullName = userObject["full_name"]?.ToString();
var phone = userObject["phone"]?.ToString();
var address = userObject["address"]?.ToString();
var isActive = userObject["is_active"]?.ToString();
```

**Cấu trúc JSON response:**

**Thành công (200 OK):**
```json
{
    "id": 1,
    "username": "phatpt",
    "email": "phatpt@example.com",
    "full_name": "Phạm Thành Phát",
    "phone": "0123456789",
    "address": "123 Đường ABC",
    "is_active": true,
    ...
}
```

**Thất bại (401 Unauthorized):**
```json
{
    "detail": "Not authenticated"
}
```

**Thất bại (Token expired):**
```json
{
    "detail": "Token expired"
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
    btnGetUserInfo.Enabled = true;
    return;
}
```

**Xử lý:**
- Kiểm tra URL có giá trị không
- Nếu rỗng → Hiển thị MessageBox cảnh báo
- Không gửi request
- Enable lại nút GET USER INFO

**Lưu ý:**
- Hệ thống không validate format URL (có thể là URL không hợp lệ)
- Validation format URL sẽ được xử lý bởi HttpClient khi gửi request

#### 3.2. Validation Token Type

**Kiểm tra Token Type không rỗng:**
```csharp
if (string.IsNullOrEmpty(tokenType))
{
    MessageBox.Show("Vui lòng nhập Token Type!", "Lỗi", 
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
    btnGetUserInfo.Enabled = true;
    return;
}
```

**Xử lý:**
- Trim khoảng trắng đầu/cuối trước khi kiểm tra
- Nếu rỗng sau khi trim → Hiển thị MessageBox
- Không gửi request nếu thiếu Token Type

**Lưu ý:**
- Token Type thường là "Bearer" (mặc định)
- Có thể là "Basic", "Digest", etc. tùy vào server

#### 3.3. Validation Access Token

**Kiểm tra Access Token không rỗng:**
```csharp
if (string.IsNullOrEmpty(accessToken))
{
    MessageBox.Show("Vui lòng nhập Access Token!", "Lỗi", 
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
    btnGetUserInfo.Enabled = true;
    return;
}
```

**Xử lý:**
- Trim khoảng trắng đầu/cuối (token không nên có khoảng trắng)
- Nếu rỗng → Hiển thị MessageBox
- Không gửi request nếu thiếu Access Token

**Lưu ý:**
- Access Token là JWT token từ Bài 5
- Token có format: `header.payload.signature` (3 phần ngăn cách bởi dấu chấm)
- Token có thời gian hết hạn → Cần lấy token mới nếu hết hạn

#### 3.4. Validation từ Server

**Server validation:**
- Nếu token hết hạn → Server trả về 401 Unauthorized với "Token expired"
- Nếu token không hợp lệ → Server trả về 401 Unauthorized với "Not authenticated"
- Nếu token không đúng format → Server trả về 401 Unauthorized
- Nếu token hợp lệ → Server trả về 200 OK với thông tin user

**Xử lý response từ server:**
```csharp
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
```

**Các trường hợp lỗi:**
- **401 Unauthorized**: Token hết hạn hoặc không hợp lệ
- **404 Not Found**: API endpoint không tồn tại
- **500 Internal Server Error**: Lỗi server

#### 3.5. Xử lý lỗi parse JSON

**Nếu response không phải JSON hợp lệ:**
```csharp
catch (Exception parseEx)
{
    // Nếu không parse được JSON, hiển thị raw response
    txtResult.Text = "RESPONSE (RAW):\r\n";
    txtResult.Text += "========================\r\n";
    txtResult.Text += responseString;
    txtResult.Text += $"\r\n\r\nLưu ý: Không thể parse JSON. Lỗi: {parseEx.Message}";
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
    // Parse JSON response
    var userObject = JObject.Parse(responseString);
    
    // Format và hiển thị thông tin user
    txtResult.Text = "THÔNG TIN NGƯỜI DÙNG:\r\n";
    txtResult.Text += "========================\r\n\r\n";
    
    // Hiển thị các trường thông tin cơ bản
    if (userObject["id"] != null)
        txtResult.Text += $"ID: {userObject["id"]}\r\n";
    
    if (userObject["username"] != null)
        txtResult.Text += $"Username: {userObject["username"]}\r\n";
    
    // ... các trường khác
    
    // Hiển thị JSON đầy đủ ở cuối
    txtResult.Text += "\r\n========================\r\n";
    txtResult.Text += "JSON RESPONSE (ĐẦY ĐỦ):\r\n";
    txtResult.Text += "========================\r\n";
    var jsonString = userObject.ToString(Newtonsoft.Json.Formatting.Indented);
    txtResult.Text += jsonString;
}
```

**Các bước xử lý:**
1. Kiểm tra `response.IsSuccessStatusCode` (200-299)
2. Parse JSON response thành `JObject`
3. Lấy các trường thông tin: id, username, email, full_name, phone, address, is_active
4. Format output theo yêu cầu:
   - Phần 1: "THÔNG TIN NGƯỜI DÙNG" với các trường được format dễ đọc
   - Phần 2: "JSON RESPONSE (ĐẦY ĐỦ)" với JSON đầy đủ được format đẹp

#### 4.2. Xử lý response thất bại

**Kiểm tra status code lỗi:**
```csharp
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
```

**Các loại lỗi:**
- **401 Unauthorized**: Token hết hạn hoặc không hợp lệ
  - Detail: "Not authenticated" hoặc "Token expired"
- **404 Not Found**: API endpoint không tồn tại
  - Detail: "Not found"
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

#### 4.5. Format thông tin user

**Hiển thị các trường thông tin:**
```csharp
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
```

**Xử lý trường is_active:**
- Chuyển đổi từ string/boolean sang text tiếng Việt
- "true" hoặc true → "Hoạt động"
- "false" hoặc false → "Không hoạt động"

#### 4.6. Format JSON response

**Hiển thị JSON đầy đủ:**
```csharp
// Hiển thị JSON đầy đủ ở cuối
txtResult.Text += "\r\n========================\r\n";
txtResult.Text += "JSON RESPONSE (ĐẦY ĐỦ):\r\n";
txtResult.Text += "========================\r\n";
var jsonString = userObject.ToString(Newtonsoft.Json.Formatting.Indented);
if (jsonString != null)
    txtResult.Text += jsonString;
```

**Format JSON:**
- Sử dụng `Formatting.Indented` để format JSON đẹp (có indent)
- Hiển thị tất cả các trường trong JSON (kể cả các trường không được hiển thị ở phần trên)

#### 4.7. Async/Await Pattern

**Sử dụng async/await để không block UI:**
```csharp
private async void btnGetUserInfo_Click(object sender, EventArgs e)
{
    // ...
    await GetUserInfoAsync(url, tokenType, accessToken);
    // ...
}

private async Task GetUserInfoAsync(string url, string tokenType, string accessToken)
{
    // ...
    var response = await client.GetAsync(url);
    var responseString = await response.Content.ReadAsStringAsync();
    // ...
}
```

**Lợi ích:**
- UI không bị đơ khi đang xử lý request
- Người dùng có thể thấy "Đang xử lý..." trong khi chờ
- Nút GET USER INFO được disable để tránh click nhiều lần

---

### 5. Hiển thị kết quả

Hệ thống hiển thị kết quả cho người dùng thông qua RichTextBox:

#### 5.1. Hiển thị trạng thái xử lý

**Khi bắt đầu xử lý:**
```csharp
btnGetUserInfo.Enabled = false;  // Disable button
txtResult.Clear();               // Clear kết quả cũ
txtResult.Text = "Đang xử lý...\r\n";  // Hiển thị trạng thái
```

**Mục đích:**
- Thông báo cho người dùng biết hệ thống đang xử lý
- Tránh click nhiều lần (button bị disable)
- Clear kết quả cũ để hiển thị kết quả mới

#### 5.2. Hiển thị kết quả thành công

**Format output khi lấy thông tin thành công:**
```csharp
txtResult.Text = "THÔNG TIN NGƯỜI DÙNG:\r\n";
txtResult.Text += "========================\r\n\r\n";

// Hiển thị các trường thông tin
txtResult.Text += $"ID: {userObject["id"]}\r\n";
txtResult.Text += $"Username: {userObject["username"]}\r\n";
txtResult.Text += $"Email: {userObject["email"]}\r\n";
// ... các trường khác

// Hiển thị JSON đầy đủ
txtResult.Text += "\r\n========================\r\n";
txtResult.Text += "JSON RESPONSE (ĐẦY ĐỦ):\r\n";
txtResult.Text += "========================\r\n";
txtResult.Text += jsonString;
```

**Ví dụ output:**
```
THÔNG TIN NGƯỜI DÙNG:
========================

ID: 1
Username: phatpt
Email: phatpt@example.com
Họ và tên: Phạm Thành Phát
Số điện thoại: 0123456789
Địa chỉ: 123 Đường ABC
Trạng thái: Hoạt động

========================
JSON RESPONSE (ĐẦY ĐỦ):
========================
{
  "id": 1,
  "username": "phatpt",
  "email": "phatpt@example.com",
  "full_name": "Phạm Thành Phát",
  "phone": "0123456789",
  "address": "123 Đường ABC",
  "is_active": true,
  ...
}
```

**Cấu trúc:**
- **Phần 1**: "THÔNG TIN NGƯỜI DÙNG" với các trường được format dễ đọc
- **Phần 2**: "JSON RESPONSE (ĐẦY ĐỦ)" với JSON đầy đủ được format đẹp

#### 5.3. Hiển thị kết quả thất bại

**Format output khi lấy thông tin thất bại:**
```csharp
var detail = errorObject["detail"]?.ToString() ?? responseString;
txtResult.Text = $"Detail: {detail}\r\n";
txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
```

**Ví dụ output (401 Unauthorized):**
```
Detail: Not authenticated
Status Code: 401 Unauthorized
```

**Ví dụ output (Token expired):**
```
Detail: Token expired
Status Code: 401 Unauthorized
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

#### 5.5. Hiển thị lỗi parse JSON

**Format output khi không parse được JSON:**
```csharp
txtResult.Text = "RESPONSE (RAW):\r\n";
txtResult.Text += "========================\r\n";
txtResult.Text += responseString;
txtResult.Text += $"\r\n\r\nLưu ý: Không thể parse JSON. Lỗi: {parseEx.Message}";
```

**Ví dụ output:**
```
RESPONSE (RAW):
========================
[raw response string]

Lưu ý: Không thể parse JSON. Lỗi: [chi tiết lỗi]
```

#### 5.6. Cập nhật trạng thái button

**Sau khi xử lý xong:**
```csharp
finally
{
    btnGetUserInfo.Enabled = true;  // Enable lại button
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
- Mỗi thông tin trên một dòng riêng
- Format rõ ràng, dễ đọc
- Có separator (dấu =) để phân cách các phần

---

## TÓM TẮT LUỒNG XỬ LÝ

```
1. Người dùng nhập URL, Token Type, Access Token
   ↓
2. Người dùng click nút "GET USER INFO"
   ↓
3. Disable button, hiển thị "Đang xử lý..."
   ↓
4. Validation dữ liệu đầu vào (URL, Token Type, Access Token)
   ↓
5. Nếu validation fail → Hiển thị MessageBox, enable lại button
   ↓
6. Nếu validation pass → Thiết lập Authorization header
   ↓
7. Gửi HTTP GET request đến API với Authorization header
   ↓
8. Nhận response từ server
   ↓
9. Kiểm tra status code
   ↓
10. Nếu thành công (200-299):
    - Parse JSON response
    - Lấy các trường thông tin user
    - Format output: "THÔNG TIN NGƯỜI DÙNG" + "JSON RESPONSE (ĐẦY ĐỦ)"
    ↓
11. Nếu thất bại:
    - Parse error response
    - Format output: "Detail: [detail]\nStatus Code: [code]"
    ↓
12. Nếu lỗi kết nối:
    - Format output: "Lỗi kết nối: [message]\nChi tiết: [detail]"
    ↓
13. Enable lại button
    ↓
14. Hiển thị kết quả trong RichTextBox
```

---

## CẤU TRÚC FILE

```
Bai6/
├── Bai6.csproj          # File cấu hình project, khai báo dependencies
├── Program.cs            # Entry point của ứng dụng
├── Form1.cs              # Logic xử lý form và HTTP GET request
├── Form1.Designer.cs    # Thiết kế giao diện (auto-generated)
├── Form1.resx            # Resource file cho form
├── App.config            # File cấu hình ứng dụng
├── README.md             # Hướng dẫn sử dụng tổng quát
├── HUONG_DAN_TEST.md     # Hướng dẫn test chi tiết
├── TEST_CASES.md         # Danh sách test cases
└── Document_check.md    # File này - Tài liệu kiểm tra
```

---

## HƯỚNG DẪN SỬ DỤNG

### Khởi động ứng dụng

1. Chạy: `dotnet run` trong thư mục `Bai6`
2. Hoặc chạy file `.exe` từ thư mục `bin/Debug/net8.0-windows/`

### Lấy Access Token từ Bài 5

1. Chạy ứng dụng **Bai05** (HTTP POST Login)
2. Đăng nhập với username và password hợp lệ
3. Copy **Access Token** từ kết quả (chuỗi JWT token dài)

### Sử dụng

1. **Kiểm tra URL**: Mặc định là `https://nt106.uitiot.vn/api/v1/user/me`
2. **Kiểm tra Token Type**: Mặc định là `Bearer` (thường không cần thay đổi)
3. **Paste Access Token**: Dán token từ Bài 5 vào trường Access Token
4. **Click GET USER INFO**: Chờ kết quả hiển thị

### Kết quả mong đợi

**Thành công:**
```
THÔNG TIN NGƯỜI DÙNG:
========================
ID: 1
Username: phatpt
Email: phatpt@example.com
...

JSON RESPONSE (ĐẦY ĐỦ):
========================
{...}
```

**Thất bại:**
```
Detail: Not authenticated
Status Code: 401 Unauthorized
```

---

## API ENDPOINT

**URL**: `https://nt106.uitiot.vn/api/v1/user/me`

**Method**: GET

**Headers**:
- `Authorization: Bearer <access_token>`

**Response thành công (200 OK)**:
```json
{
    "id": 1,
    "username": "phatpt",
    "email": "phatpt@example.com",
    "full_name": "Phạm Thành Phát",
    "phone": "0123456789",
    "address": "123 Đường ABC",
    "is_active": true,
    ...
}
```

**Response thất bại (401 Unauthorized)**:
```json
{
    "detail": "Not authenticated"
}
```

**Tài liệu API**: https://nt106.uitiot.vn/docs

---

## JWT AUTHENTICATION

### Cách hoạt động:

1. **Client gửi request** với Authorization header:
   ```
   Authorization: Bearer <access_token>
   ```

2. **Server xác thực token**:
   - Kiểm tra token có hợp lệ không
   - Kiểm tra token có hết hạn không
   - Xác định user từ token

3. **Server trả về thông tin user** nếu token hợp lệ

### Lưu ý về Token:

- Token có thời gian hết hạn (thường 24 giờ hoặc theo cấu hình server)
- Token cần được bảo mật, không chia sẻ công khai
- Nếu token hết hạn, cần đăng nhập lại để lấy token mới
- Token format: `header.payload.signature` (3 phần ngăn cách bởi dấu chấm)

---

## XỬ LÝ LỖI

### Lỗi validation

- **URL rỗng**: Hiển thị MessageBox "Vui lòng nhập URL!"
- **Token Type rỗng**: Hiển thị MessageBox "Vui lòng nhập Token Type!"
- **Access Token rỗng**: Hiển thị MessageBox "Vui lòng nhập Access Token!"

### Lỗi kết nối

- **DNS Error**: "Lỗi kết nối: The remote name could not be resolved"
- **Timeout**: "Lỗi kết nối: The operation timed out"
- **Network Error**: "Lỗi kết nối: [thông báo lỗi]"

### Lỗi từ server

- **401 Unauthorized**: Token hết hạn hoặc không hợp lệ
- **404 Not Found**: API endpoint không tồn tại
- **500 Internal Server Error**: Lỗi server

---

## KIỂM TRA VÀ TEST

Xem các file:
- **HUONG_DAN_TEST.md**: Hướng dẫn test từng bước
- **TEST_CASES.md**: Danh sách đầy đủ test cases

### Checklist kiểm tra

- [ ] Validation URL rỗng
- [ ] Validation Token Type rỗng
- [ ] Validation Access Token rỗng
- [ ] Lấy thông tin user thành công với token hợp lệ
- [ ] Xử lý token hết hạn (401 Unauthorized)
- [ ] Xử lý token không hợp lệ (401 Unauthorized)
- [ ] Xử lý lỗi kết nối (mất Internet)
- [ ] Xử lý URL không hợp lệ
- [ ] Button disable khi đang xử lý
- [ ] Hiển thị "Đang xử lý..." khi request đang chạy
- [ ] Format output đúng khi thành công (thông tin user + JSON)
- [ ] Format output đúng khi thất bại

---

## LIÊN KẾT VỚI BÀI 5

Bài 6 sử dụng kết quả từ Bài 5:
- **Bài 5**: Đăng nhập và lấy Access Token
- **Bài 6**: Sử dụng Access Token để lấy thông tin user

**Workflow hoàn chỉnh**:
1. Chạy Bai05 → Đăng nhập → Lấy Access Token
2. Copy Access Token
3. Chạy Bai6 → Paste Token → Lấy thông tin user

---

**Ngày tạo**: 2024  
**Phiên bản**: 1.0  
**Ứng dụng**: Bai6 - HTTP GET User Info

