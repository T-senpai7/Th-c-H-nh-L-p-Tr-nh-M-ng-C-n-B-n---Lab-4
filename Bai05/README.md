# BÀI 5: HTTP POST - ĐĂNG NHẬP QUA API

## 📋 MÔ TẢ

Chương trình cho phép đăng nhập vào ứng dụng Web thông qua API được cung cấp sẵn. Ứng dụng sử dụng HTTP POST request để gửi thông tin đăng nhập (username và password) dưới dạng form-data, sau đó nhận và xử lý JSON response để lấy access token.

**API Endpoint**: https://nt106.uitiot.vn/auth/token

**Tài liệu API**: https://nt106.uitiot.vn/docs

## 🎯 YÊU CẦU

- .NET 8.0 SDK hoặc cao hơn
- Windows OS (do sử dụng Windows Forms)
- Kết nối Internet để truy cập API

## 📦 CÀI ĐẶT

### Bước 1: Kiểm tra .NET SDK

Mở Command Prompt hoặc PowerShell và chạy lệnh:

```bash
dotnet --version
```

Đảm bảo phiên bản là 8.0 trở lên. Nếu chưa có, tải về từ: https://dotnet.microsoft.com/download

### Bước 2: Restore Dependencies

Di chuyển vào thư mục `Bai05` và restore các package cần thiết:

```bash
cd Bai05
dotnet restore
```

Lệnh này sẽ tự động tải về package `Newtonsoft.Json` (version 13.0.3) được khai báo trong file `Bai05.csproj`.

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
2. Di chuyển vào thư mục `Bai05`:
   ```bash
   cd Bai05
   ```
3. Chạy ứng dụng:
   ```bash
   dotnet run
   ```

### Cách 2: Chạy từ Visual Studio

1. Mở file `LAB04.sln` trong Visual Studio
2. Chọn project `Bai05` trong Solution Explorer
3. Nhấn `F5` hoặc click nút "Start" để chạy

### Cách 3: Chạy file .exe đã build

1. Build project:
   ```bash
   dotnet build -c Release
   ```
2. Chạy file .exe từ thư mục `bin\Release\net8.0-windows\Bai05.exe`

## 📝 HƯỚNG DẪN SỬ DỤNG GIAO DIỆN

### Bước 1: Mở ứng dụng

Sau khi chạy, cửa sổ ứng dụng sẽ hiển thị với các trường:
- **URL**: Địa chỉ API endpoint (mặc định: `https://nt106.uitiot.vn/auth/token`)
- **Username**: Tên đăng nhập (mặc định: `phatpt`)
- **Password**: Mật khẩu (trường này sẽ bị ẩn khi nhập)
- **LOGIN**: Nút để thực hiện đăng nhập
- **Kết quả**: Vùng hiển thị kết quả đăng nhập

### Bước 2: Nhập thông tin đăng nhập

1. Kiểm tra URL đã đúng chưa (mặc định đã được điền sẵn)
2. Nhập **Username** của bạn (hoặc giữ nguyên `phatpt` nếu đây là tài khoản mẫu)
3. Nhập **Password** của bạn (ký tự sẽ bị ẩn bằng dấu `*`)

### Bước 3: Thực hiện đăng nhập

1. Click nút **"LOGIN"**
2. Nút sẽ bị disable và hiển thị "Đang xử lý..." trong vùng kết quả
3. Ứng dụng sẽ gửi HTTP POST request đến API với thông tin đăng nhập

### Bước 4: Xem kết quả

#### Nếu đăng nhập thành công:

Vùng kết quả sẽ hiển thị:
```
Bearer
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InBoYXRwdCIsImV4cCI6MTcxMzYyMTA0N30.re7JotDf35TM83qpLxVlbiAZIBv1esy_92Ye-xXXgDY

Đăng nhập thành công
```

Trong đó:
- **Bearer**: Loại token (token_type)
- Dòng tiếp theo: Access token (JWT token)
- **Đăng nhập thành công**: Thông báo xác nhận

#### Nếu đăng nhập thất bại:

Vùng kết quả sẽ hiển thị:
```
Detail: [Thông tin lỗi từ API]
Status Code: [Mã lỗi HTTP]
```

Ví dụ:
```
Detail: Incorrect username or password
Status Code: 401 Unauthorized
```

## 🔧 CẤU TRÚC PROJECT

```
Bai05/
├── Bai05.csproj          # File cấu hình project, khai báo dependencies
├── Program.cs            # Entry point của ứng dụng
├── Form1.cs              # Logic xử lý form và HTTP POST request
├── Form1.Designer.cs     # Thiết kế giao diện (auto-generated)
├── Form1.resx            # Resource file cho form
├── App.config            # File cấu hình ứng dụng
└── README.md             # File hướng dẫn này
```

## 💻 GIẢI THÍCH CODE

### 1. Bai05.csproj

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

#### b) Tạo HTTP POST request với form-data

```csharp
var content = new MultipartFormDataContent
{
    { new StringContent(username), "username" },
    { new StringContent(password), "password" }
};
```

Tạo nội dung request dưới dạng `multipart/form-data` với 2 trường:
- `username`: Tên đăng nhập
- `password`: Mật khẩu

#### c) Gửi request và nhận response

```csharp
var response = await client.PostAsync(url, content);
var responseString = await response.Content.ReadAsStringAsync();
```

Sử dụng `HttpClient.PostAsync()` để gửi POST request và đọc response dưới dạng string.

#### d) Parse JSON response

```csharp
var responseObject = JObject.Parse(responseString);
```

Sử dụng `Newtonsoft.Json` để parse JSON string thành `JObject`, cho phép truy cập các trường dễ dàng.

#### e) Xử lý kết quả

**Nếu thành công** (status code 200-299):
```csharp
var tokenType = responseObject["token_type"]?.ToString() ?? "";
var accessToken = responseObject["access_token"]?.ToString() ?? "";

txtResult.Text = "Bearer\r\n";
txtResult.Text += $"{accessToken}\r\n";
txtResult.Text += "\r\n";
txtResult.Text += "Đăng nhập thành công\r\n";
```

Lấy `token_type` và `access_token` từ JSON, hiển thị theo format yêu cầu.

**Nếu thất bại**:
```csharp
var detail = responseObject["detail"]?.ToString() ?? "Không có thông tin chi tiết";
txtResult.Text = $"Detail: {detail}\r\n";
txtResult.Text += $"Status Code: {(int)response.StatusCode} {response.StatusCode}\r\n";
```

Lấy thông tin lỗi từ trường `detail` và hiển thị status code.

## 🔍 CÁC TRƯỜNG HỢP XỬ LÝ

### 1. Lỗi kết nối mạng

Nếu không thể kết nối đến server:
```
Lỗi kết nối: [Thông báo lỗi]
Chi tiết: [Chi tiết lỗi]
```

### 2. Lỗi parse JSON

Nếu response không phải JSON hợp lệ, sẽ hiển thị thông báo lỗi parse.

### 3. Lỗi validation

Nếu thiếu thông tin (URL, Username, hoặc Password), sẽ hiển thị MessageBox cảnh báo.

## 📚 TÀI LIỆU THAM KHẢO

- [Microsoft Docs - HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
- [Microsoft Docs - WebRequest](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-send-data-using-the-webrequest-class)
- [Newtonsoft.Json Documentation](https://www.newtonsoft.com/json/help/html/Introduction.htm)
- [API Documentation](https://nt106.uitiot.vn/docs)

## 🐛 XỬ LÝ LỖI THƯỜNG GẶP

### Lỗi: "Could not load file or assembly 'Newtonsoft.Json'"

**Giải pháp**: Chạy lại `dotnet restore` để tải về package.

### Lỗi: "The remote server returned an error: (401) Unauthorized"

**Giải pháp**: Kiểm tra lại username và password. Đảm bảo thông tin đăng nhập chính xác.

### Lỗi: "The remote name could not be resolved"

**Giải pháp**: Kiểm tra kết nối Internet và URL API có đúng không.

### Ứng dụng không hiển thị giao diện

**Giải pháp**: 
1. Đảm bảo đang chạy trên Windows
2. Kiểm tra `UseWindowsForms` đã được set thành `true` trong `.csproj`
3. Build lại project: `dotnet clean` sau đó `dotnet build`

## ✅ KIỂM TRA KẾT QUẢ

Sau khi đăng nhập thành công, bạn sẽ nhận được:
1. **Token Type**: "Bearer"
2. **Access Token**: Một chuỗi JWT token dài
3. **Thông báo**: "Đăng nhập thành công"

Token này có thể được sử dụng để xác thực các API request tiếp theo bằng cách thêm vào header:
```
Authorization: Bearer <access_token>
```

## 📞 HỖ TRỢ

Nếu gặp vấn đề, hãy kiểm tra:
1. Kết nối Internet
2. URL API có đúng không
3. Thông tin đăng nhập có chính xác không
4. Đã restore dependencies chưa (`dotnet restore`)

---

**Lưu ý**: Đây là ứng dụng mẫu cho mục đích học tập. Trong môi trường production, cần thêm các biện pháp bảo mật như:
- Mã hóa password trước khi gửi
- Lưu trữ token an toàn
- Xử lý token expiration
- Validate và sanitize input

