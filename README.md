# LAB04 - TỔNG HỢP CÁC BÀI TẬP

## 📋 TỔNG QUAN

Repository này chứa các bài tập về lập trình mạng và ứng dụng Windows Forms sử dụng .NET 8.0.

## 🎯 YÊU CẦU CHUNG

- **.NET 8.0 SDK** hoặc cao hơn
- **Windows OS** (do sử dụng Windows Forms)
- **Kết nối Internet** (cho các bài sử dụng API)

## 📦 CÀI ĐẶT

### Kiểm tra .NET SDK

```bash
dotnet --version
```

Đảm bảo phiên bản là 8.0 trở lên. Nếu chưa có, tải về từ: https://dotnet.microsoft.com/download

### Restore Dependencies

```bash
dotnet restore
```

---

## 📚 DANH SÁCH CÁC BÀI TẬP

### Bai 1

**Mô tả**: Bài tập cơ bản về Windows Forms

**Lệnh chạy**:
```bash
cd "Bai 1"
dotnet run
```

---

### Bai 02

**Mô tả**: Bài tập về Windows Forms

**Lệnh chạy**:
```bash
cd "Bai 02"
dotnet run
```

---

### Bai 3

**Mô tả**: Bài tập về WebView và web scraping

**Lệnh chạy**:
```bash
cd "Bai 3"
dotnet run
```

**Lưu ý**: Sử dụng WebView2 và HtmlAgilityPack

---

### Bai4 - Quản lý phòng vé rạp phim

**Mô tả**: Hệ thống quản lý phòng vé rạp phim với kiến trúc 1 Server - Multi Client

**Tính năng**:
- TCP/IP Server-Client
- HTTP Web Server
- SQLite Database
- Đồng bộ real-time giữa các clients
- Giao diện Web và Desktop

**Lệnh chạy**:
```bash
cd Bai4
dotnet run
```

**Cách sử dụng**:
1. Chọn "TCP Server" → Click "Listen" (cho TCP mode)
2. Hoặc chọn "Web Server" → Click "Start HTTP Server" (cho Web mode)
3. Mở client (Desktop hoặc Web) để đặt vé

**Tài liệu chi tiết**: Xem `Bai4/README.md`

---

### Bai05 - HTTP POST Login

**Mô tả**: Ứng dụng đăng nhập qua HTTP POST API

**Tính năng**:
- Đăng nhập qua API endpoint
- Nhận JWT access token
- Validation input
- Xử lý lỗi

**Lệnh chạy**:
```bash
cd Bai05
dotnet run
```

**API Endpoint**: `https://nt106.uitiot.vn/auth/token`

**Cách sử dụng**:
1. Nhập URL, Username, Password
2. Click "LOGIN"
3. Copy Access Token từ kết quả (dùng cho Bai6)

**Tài liệu chi tiết**: Xem `Bai05/README.md` và `Bai05/Document_check.md`

---

### Bai6 - HTTP GET User Info

**Mô tả**: Ứng dụng lấy thông tin user qua HTTP GET API với JWT authentication

**Tính năng**:
- Lấy thông tin user hiện tại
- Sử dụng JWT Bearer token
- Hiển thị thông tin user format đẹp
- Hiển thị JSON response đầy đủ

**Lệnh chạy**:
```bash
cd Bai6
dotnet run
```

**API Endpoint**: `https://nt106.uitiot.vn/api/v1/user/me`

**Cách sử dụng**:
1. Lấy Access Token từ Bai05
2. Nhập URL, Token Type (Bearer), Access Token
3. Click "GET USER INFO"
4. Xem thông tin user

**Tài liệu chi tiết**: Xem `Bai6/README.md` và `Bai6/Document_check.md`

**Lưu ý**: Cần Access Token từ Bai05

---

### Bai07 - Hôm nay ăn gì?

**Mô tả**: Ứng dụng quản lý món ăn với đầy đủ chức năng CRUD

**Tính năng**:
- Đăng ký/Đăng nhập
- Xem danh sách món ăn (tất cả và của tôi) với phân trang
- Thêm món ăn mới
- Xóa món ăn
- Chọn ngẫu nhiên món ăn
- Phân trang (5-50 món/trang)

**Lệnh chạy**:
```bash
cd Bai07
dotnet run
```

**API Base URL**: `https://nt106.uitiot.vn`

**Cách sử dụng**:
1. Đăng ký tài khoản mới hoặc đăng nhập
2. Xem danh sách món ăn (tab "Tất cả món ăn" hoặc "Món ăn của tôi")
3. Thêm món ăn: Click "➕ Thêm món ăn"
4. Xóa món ăn: Click vào card → Click "🗑️ Xóa món ăn"
5. Chọn ngẫu nhiên: Click "🎲 Ngẫu nhiên (Cộng đồng)" hoặc "🎲 Ngẫu nhiên (Của tôi)"

**Tài liệu chi tiết**: Xem `Bai07/README.md` và `Bai07/Document_check.md`

---

## 🔗 LIÊN KẾT GIỮA CÁC BÀI

### Workflow hoàn chỉnh:

1. **Bai05** → Đăng nhập → Lấy Access Token
2. **Bai6** → Sử dụng Access Token từ Bai05 → Lấy thông tin user
3. **Bai07** → Đăng ký/Đăng nhập → Quản lý món ăn

### Bai4 (Độc lập):
- Hệ thống TCP/IP Server-Client riêng biệt
- Có thể chạy độc lập không cần các bài khác

---

## 📁 CẤU TRÚC THƯ MỤC

```
LAB04-NT106-main/
├── Bai 1/              # Bài tập 1
├── Bai 02/             # Bài tập 2
├── Bai 3/              # Bài tập 3 (WebView)
├── Bai4/               # Quản lý phòng vé rạp phim
│   ├── README.md
│   ├── Document_check.md (nếu có)
│   └── ...
├── Bai05/              # HTTP POST Login
│   ├── README.md
│   ├── Document_check.md
│   └── ...
├── Bai6/               # HTTP GET User Info
│   ├── README.md
│   ├── Document_check.md
│   └── ...
├── Bai07/              # Quản lý món ăn
│   ├── README.md
│   ├── Document_check.md
│   └── ...
├── LAB04.sln           # Solution file
└── README.md           # File này
```

---

## 🚀 HƯỚNG DẪN NHANH

### Chạy tất cả các bài:

```bash
# Bai 1
cd "Bai 1" && dotnet run

# Bai 02
cd "Bai 02" && dotnet run

# Bai 3
cd "Bai 3" && dotnet run

# Bai4
cd Bai4 && dotnet run

# Bai05
cd Bai05 && dotnet run

# Bai6
cd Bai6 && dotnet run

# Bai07
cd Bai07 && dotnet run
```

### Build tất cả:

```bash
dotnet build
```

### Clean tất cả (xóa bin/ và obj/):

```bash
dotnet clean
```

---

## 📝 TÀI LIỆU THAM KHẢO

### API Documentation:
- **API Base URL**: https://nt106.uitiot.vn
- **API Docs**: https://nt106.uitiot.vn/docs

### Tài liệu từng bài:
- **Bai4**: `Bai4/README.md`
- **Bai05**: `Bai05/README.md`, `Bai05/Document_check.md`
- **Bai6**: `Bai6/README.md`, `Bai6/Document_check.md`
- **Bai07**: `Bai07/README.md`, `Bai07/Document_check.md`

---

## ⚠️ LƯU Ý

1. **Port conflicts**: 
   - Bai4 sử dụng port 8080 (TCP) và 8888 (HTTP)
   - Đảm bảo các port này không bị chiếm dụng

2. **Firewall**: 
   - Nếu chạy Bai4 trên nhiều máy, cần mở firewall cho port 8080 và 8888

3. **Access Token**: 
   - Token từ Bai05 có thể dùng cho Bai6
   - Token có thời gian hết hạn, cần lấy lại nếu hết hạn

4. **Database**: 
   - Bai4 tự động tạo SQLite database khi chạy lần đầu
   - Database được lưu trong thư mục chạy ứng dụng

5. **Dependencies**: 
   - Chạy `dotnet restore` nếu thiếu packages
   - Một số bài sử dụng Newtonsoft.Json, HtmlAgilityPack, WebView2

---

## 🐛 XỬ LÝ LỖI THƯỜNG GẶP

### Lỗi: "Could not load file or assembly"

**Giải pháp**: Chạy `dotnet restore` trong thư mục bài tương ứng

### Lỗi: "Port already in use"

**Giải pháp**: Đóng ứng dụng khác đang dùng port, hoặc thay đổi port trong code

### Lỗi: "Not authenticated" hoặc 401

**Giải pháp**: 
- Kiểm tra token có hợp lệ không
- Đăng nhập lại để lấy token mới

### Lỗi: "The remote name could not be resolved"

**Giải pháp**: Kiểm tra kết nối Internet và URL API có đúng không

---

## ✅ CHECKLIST

- [ ] Đã cài đặt .NET 8.0 SDK
- [ ] Đã restore dependencies (`dotnet restore`)
- [ ] Đã test chạy từng bài
- [ ] Đã đọc README của từng bài (nếu có)
- [ ] Đã kiểm tra kết nối Internet (cho các bài dùng API)

---

**Ngày tạo**: 2024  
**Phiên bản**: 1.0  
**Repository**: LAB04-NT106

