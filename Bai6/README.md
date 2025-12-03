# BÀI 6: HTTP GET - LẤY THÔNG TIN NGƯỜI DÙNG QUA API

## 📋 MÔ TẢ

Chương trình cho phép hiển thị thông tin người dùng hiện tại đang đăng nhập vào ứng dụng Web thông qua API được cung cấp sẵn. Ứng dụng sử dụng HTTP GET request với JWT (JSON Web Token) authentication để lấy thông tin user từ server.

**API Endpoint**: https://nt106.uitiot.vn/api/v1/user/me

**Tài liệu API**: https://nt106.uitiot.vn/docs

**Yêu cầu**: Cần có Access Token từ Bài 5 (HTTP POST Login)

## 🎯 YÊU CẦU

- .NET 8.0 SDK hoặc cao hơn
- Windows OS (do sử dụng Windows Forms)
- Kết nối Internet để truy cập API
- **Access Token hợp lệ** từ Bài 5 (hoặc từ API đăng nhập)

## 📦 CÀI ĐẶT

### Bước 1: Kiểm tra .NET SDK

Mở Command Prompt hoặc PowerShell và chạy lệnh:

```bash
dotnet --version
```

Đảm bảo phiên bản là 8.0 trở lên. Nếu chưa có, tải về từ: https://dotnet.microsoft.com/download

### Bước 2: Restore Dependencies

Di chuyển vào thư mục `Bai6` và restore các package cần thiết:

```bash
cd Bai6
dotnet restore
```

Lệnh này sẽ tự động tải về package `Newtonsoft.Json` (version 13.0.3) được khai báo trong file `Bai6.csproj`.

### Bước 3: Build Project

Build project để kiểm tra không có lỗi:

```bash
dotnet build
```

Nếu build thành công, bạn sẽ thấy thông báo:
```
Build succeeded
```

## 🚀 HƯỚNG DẪN SỬ DỤNG

### Cách 1: Chạy từ Command Line

1. Mở Command Prompt hoặc PowerShell
2. Di chuyển vào thư mục `Bai6`:
   ```bash
   cd Bai6
   ```
3. Chạy ứng dụng:
   ```bash
   dotnet run
   ```

### Cách 2: Chạy từ Visual Studio

1. Mở file `LAB04.sln` trong Visual Studio
2. Chọn project `Bai6` trong Solution Explorer
3. Nhấn `F5` hoặc click nút "Start" để chạy

### Cách 3: Chạy file .exe đã build

1. Build project:
   ```bash
   dotnet build -c Release
   ```
2. Chạy file .exe từ thư mục `bin\Release\net8.0-windows\Bai6.exe`

## 📝 HƯỚNG DẪN SỬ DỤNG GIAO DIỆN

### Bước 1: Lấy Access Token từ Bài 5

Trước tiên, bạn cần có Access Token từ Bài 5:

1. Chạy ứng dụng **Bai05** (HTTP POST Login)
2. Đăng nhập với username và password hợp lệ
3. Copy **Access Token** từ kết quả (chuỗi JWT token dài)

### Bước 2: Mở ứng dụng Bai6

Sau khi chạy, cửa sổ ứng dụng sẽ hiển thị với các trường:
- **URL**: Địa chỉ API endpoint (mặc định: `https://nt106.uitiot.vn/api/v1/user/me`)
- **Token Type**: Loại token (mặc định: `Bearer`)
- **Access Token**: JWT token để xác thực (cần nhập từ Bài 5)
- **GET USER INFO**: Nút để lấy thông tin user
- **Kết quả**: Vùng hiển thị thông tin user

### Bước 3: Nhập thông tin

1. Kiểm tra **URL** đã đúng chưa (mặc định đã được điền sẵn)
2. Kiểm tra **Token Type** (mặc định là `Bearer`, thường không cần thay đổi)
3. **Paste Access Token** từ Bài 5 vào trường **Access Token**

### Bước 4: Lấy thông tin user

1. Click nút **"GET USER INFO"**
2. Nút sẽ bị disable và hiển thị "Đang xử lý..." trong vùng kết quả
3. Ứng dụng sẽ gửi HTTP GET request với Authorization header chứa JWT token

### Bước 5: Xem kết quả

#### Nếu thành công:

Vùng kết quả sẽ hiển thị:
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
  ...
}
```

#### Nếu thất bại:

Vùng kết quả sẽ hiển thị:
```
Detail: [Thông báo lỗi từ API]
Status Code: [Mã lỗi HTTP]
```

Ví dụ:
```
Detail: Not authenticated
Status Code: 401 Unauthorized
```

## 🔧 CẤU TRÚC PROJECT

```
Bai6/
├── Bai6.csproj          # File cấu hình project, khai báo dependencies
├── Program.cs            # Entry point của ứng dụng
├── Form1.cs              # Logic xử lý form và HTTP GET request
├── Form1.Designer.cs    # Thiết kế giao diện (auto-generated)
├── Form1.resx            # Resource file cho form
├── App.config            # File cấu hình ứng dụng
└── README.md             # File hướng dẫn này
```

## 💻 GIẢI THÍCH CODE

### 1. Bai6.csproj

File cấu hình project định nghĩa:
- **TargetFramework**: `net8.0-windows` - Sử dụng .NET 8.0 cho Windows
- **UseWindowsForms**: `true` - Cho phép sử dụng Windows Forms
- **PackageReference**: `Newtonsoft.Json` - Thư viện để parse JSON response

### 2. Program.cs

File entry point khởi tạo và chạy Windows Forms application:

```csharp
[STAThread]
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new Form1());
}
```

### 3. Form1.Designer.cs

File này chứa code tự động tạo bởi Windows Forms Designer, định nghĩa:
- Các controls: Labels, TextBoxes, Button, RichTextBox
- Vị trí và kích thước của các controls
- Event handlers được gán cho các controls

### 4. Form1.cs - Logic chính

#### a) Validation dữ liệu đầu vào

```csharp
if (string.IsNullOrEmpty(url))
{
    MessageBox.Show("Vui lòng nhập URL!", "Lỗi", ...);
    return;
}
```

Kiểm tra các trường bắt buộc trước khi gửi request.

#### b) Thiết lập Authorization Header với JWT Token

```csharp
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue(tokenType, accessToken);
```

Thiết lập header `Authorization` với format: `Bearer <access_token>`

#### c) Gửi HTTP GET request

```csharp
var response = await client.GetAsync(url);
var responseString = await response.Content.ReadAsStringAsync();
```

Sử dụng `HttpClient.GetAsync()` để gửi GET request và đọc response.

#### d) Parse và hiển thị thông tin user

```csharp
var userObject = JObject.Parse(responseString);

// Hiển thị các trường thông tin cơ bản
if (userObject["username"] != null)
    txtResult.Text += $"Username: {userObject["username"]}\r\n";
```

Parse JSON response và hiển thị các trường thông tin user một cách có định dạng.

#### e) Xử lý lỗi

**Nếu request thất bại** (status code không phải 2xx):
```csharp
var errorObject = JObject.Parse(responseString);
var detail = errorObject["detail"]?.ToString() ?? responseString;
txtResult.Text = $"Detail: {detail}\r\n";
txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
```

## 🔐 JWT AUTHENTICATION

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

## 🔍 CÁC TRƯỜNG HỢP XỬ LÝ

### 1. Token hợp lệ

- ✅ Nhận được thông tin user đầy đủ
- ✅ Hiển thị các trường: ID, Username, Email, Họ tên, v.v.

### 2. Token không hợp lệ hoặc hết hạn

- ❌ Status Code: 401 Unauthorized
- ❌ Thông báo: "Not authenticated" hoặc "Token expired"

### 3. Token không đúng format

- ❌ Status Code: 401 Unauthorized
- ❌ Thông báo lỗi từ server

### 4. Lỗi kết nối mạng

- ❌ Hiển thị: "Lỗi kết nối: [Thông báo lỗi]"

## 📚 TÀI LIỆU THAM KHẢO

- [Microsoft Docs - HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
- [Microsoft Docs - WebRequest](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-send-data-using-the-webrequest-class)
- [Newtonsoft.Json Documentation](https://www.newtonsoft.com/json/help/html/Introduction.htm)
- [JWT.io - JWT Introduction](https://jwt.io/introduction)
- [API Documentation](https://nt106.uitiot.vn/docs)

## 🐛 XỬ LÝ LỖI THƯỜNG GẶP

### Lỗi: "Not authenticated" hoặc 401 Unauthorized

**Nguyên nhân**:
- Token đã hết hạn
- Token không đúng format
- Token không hợp lệ

**Giải pháp**: 
1. Lấy token mới từ Bài 5
2. Kiểm tra token có đầy đủ không (không bị cắt)
3. Đảm bảo copy đúng toàn bộ token

### Lỗi: "Could not load file or assembly 'Newtonsoft.Json'"

**Giải pháp**: Chạy lại `dotnet restore` để tải về package.

### Lỗi: "The remote name could not be resolved"

**Giải pháp**: Kiểm tra kết nối Internet và URL API có đúng không.

### Ứng dụng không hiển thị giao diện

**Giải pháp**: 
1. Đảm bảo đang chạy trên Windows
2. Kiểm tra `UseWindowsForms` đã được set thành `true` trong `.csproj`
3. Build lại project: `dotnet clean` sau đó `dotnet build`

## ✅ KIỂM TRA KẾT QUẢ

Sau khi lấy thông tin user thành công, bạn sẽ nhận được:
1. **Thông tin user được format** dễ đọc
2. **JSON response đầy đủ** ở cuối để tham khảo
3. Các trường thông tin cơ bản: ID, Username, Email, Họ tên, v.v.

## 🔗 LIÊN KẾT VỚI BÀI 5

Bài 6 sử dụng kết quả từ Bài 5:
- **Bài 5**: Đăng nhập và lấy Access Token
- **Bài 6**: Sử dụng Access Token để lấy thông tin user

**Workflow hoàn chỉnh**:
1. Chạy Bai05 → Đăng nhập → Lấy Access Token
2. Copy Access Token
3. Chạy Bai6 → Paste Token → Lấy thông tin user

## 📞 HỖ TRỢ

Nếu gặp vấn đề:
1. Kiểm tra kết nối Internet
2. Kiểm tra API endpoint có hoạt động không: https://nt106.uitiot.vn/docs
3. Kiểm tra token có hợp lệ không (thử lại với token mới từ Bài 5)
4. Kiểm tra log/console để xem chi tiết lỗi

---

**Lưu ý**: Đây là ứng dụng mẫu cho mục đích học tập. Trong môi trường production, cần thêm các biện pháp bảo mật như:
- Lưu trữ token an toàn (không hardcode)
- Xử lý token expiration
- Refresh token mechanism
- Validate và sanitize input

