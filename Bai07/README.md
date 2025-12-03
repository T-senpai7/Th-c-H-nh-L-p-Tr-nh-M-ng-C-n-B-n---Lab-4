# BÀI 7: HÔM NAY ĂN GÌ? - ỨNG DỤNG QUẢN LÝ MÓN ĂN

## 📋 MÔ TẢ
URL VD: https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400
Ứng dụng quản lý món ăn với các chức năng:
- **Đăng ký/Đăng nhập**: Tạo tài khoản và đăng nhập vào hệ thống
- **Thêm món ăn**: Thêm món ăn mới vào hệ thống
- **Xóa món ăn**: Xóa món ăn đã tạo
- **Xem danh sách món ăn**: 
  - Tất cả món ăn từ cộng đồng (phân trang)
  - Món ăn do bản thân tạo (phân trang)
- **Chọn ngẫu nhiên**: 
  - Ngẫu nhiên từ tất cả món ăn cộng đồng
  - Ngẫu nhiên từ món ăn của bản thân

**API Base URL**: https://nt106.uitiot.vn

**Tài liệu API**: https://nt106.uitiot.vn/docs

## 🎯 YÊU CẦU

- .NET 8.0 SDK hoặc cao hơn
- Windows OS (do sử dụng Windows Forms)
- Kết nối Internet để truy cập API

## 📦 CÀI ĐẶT

### Bước 1: Kiểm tra .NET SDK

```bash
dotnet --version
```

Đảm bảo phiên bản là 8.0 trở lên.

### Bước 2: Restore Dependencies

```bash
cd Bai07
dotnet restore
```

### Bước 3: Build Project

```bash
dotnet build
```

## 🚀 HƯỚNG DẪN SỬ DỤNG

### Chạy ứng dụng

```bash
cd Bai07
dotnet run
```

Hoặc mở Visual Studio và chạy project Bai07.

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
   - Nhập mô tả (tùy chọn)
   - Nhập URL hình ảnh (tùy chọn)
   - Click "Thêm món ăn"

4. **Xóa món ăn**
   - Chọn món ăn trong danh sách
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

## 🔧 CẤU TRÚC PROJECT

```
Bai07/
├── Bai07.csproj          # Project file
├── Program.cs             # Entry point
├── Models.cs              # Model classes (User, Food, etc.)
├── ApiHelper.cs           # API helper class
├── LoginForm.cs           # Form đăng nhập/đăng ký
├── MainForm.cs            # Form chính với các chức năng
├── AddFoodForm.cs         # Form thêm món ăn
├── App.config             # Configuration
└── README.md              # File này
```

## 💻 GIẢI THÍCH CODE

### 1. Models.cs

Định nghĩa các class:
- **User**: Thông tin người dùng
- **Food**: Thông tin món ăn
- **FoodListResponse**: Response từ API khi lấy danh sách (có phân trang)
- **LoginResponse**: Response từ API đăng nhập
- **ApiError**: Error response từ API

### 2. ApiHelper.cs

Class xử lý tất cả các API calls:
- `RegisterAsync()`: Đăng ký tài khoản
- `LoginAsync()`: Đăng nhập
- `GetAllFoodsAsync()`: Lấy danh sách tất cả món ăn (phân trang)
- `GetMyFoodsAsync()`: Lấy danh sách món ăn của bản thân (phân trang)
- `AddFoodAsync()`: Thêm món ăn mới
- `DeleteFoodAsync()`: Xóa món ăn
- `GetRandomFoodAsync()`: Chọn ngẫu nhiên từ cộng đồng
- `GetRandomMyFoodAsync()`: Chọn ngẫu nhiên từ món ăn của bản thân
- `GetCurrentUserAsync()`: Lấy thông tin user hiện tại

### 3. LoginForm.cs

Form đăng nhập/đăng ký với 2 tabs:
- **Tab Đăng nhập**: Username, Password
- **Tab Đăng ký**: Username, Password, Email (tùy chọn), Họ và tên (tùy chọn)

Sau khi đăng nhập/đăng ký thành công, mở MainForm.

### 4. MainForm.cs

Form chính với các chức năng:
- **DataGridView**: Hiển thị danh sách món ăn
- **2 Tabs**: "Tất cả món ăn" và "Món ăn của tôi"
- **Buttons**: Thêm, Xóa, Chọn ngẫu nhiên
- **Pagination**: Điều hướng giữa các trang
- **User Info**: Hiển thị thông tin user hiện tại

### 5. AddFoodForm.cs

Form thêm món ăn mới:
- Tên món ăn (bắt buộc)
- Mô tả (tùy chọn)
- URL hình ảnh (tùy chọn)

## 🔐 API ENDPOINTS

### Authentication
- `POST /api/v1/users/register` - Đăng ký
- `POST /auth/token` - Đăng nhập

### Meals
- `GET /api/v1/meals?page={page}&size={size}` - Lấy danh sách tất cả món ăn
- `GET /api/v1/meals/me?page={page}&size={size}` - Lấy danh sách món ăn của bản thân
- `POST /api/v1/meals` - Thêm món ăn mới
- `DELETE /api/v1/meals/{id}` - Xóa món ăn
- `GET /api/v1/meals/random` - Chọn ngẫu nhiên từ cộng đồng
- `GET /api/v1/meals/me/random` - Chọn ngẫu nhiên từ món ăn của bản thân

### User
- `GET /api/v1/user/me` - Lấy thông tin user hiện tại

## 📝 TÍNH NĂNG CHI TIẾT

### Phân trang

- Mặc định: 10 món ăn/trang
- Có thể thay đổi: 5-50 món ăn/trang
- Hiển thị: "Trang X / Y (Tổng: Z món)"
- Điều hướng: Nút Trước/Sau hoặc nhập số trang trực tiếp

### Xác thực

- Sử dụng JWT Bearer Token
- Token được lưu trong ApiHelper sau khi đăng nhập
- Tự động thêm vào header của mọi request
- Có thể đăng xuất để xóa token

### Xử lý lỗi

- Hiển thị thông báo lỗi rõ ràng
- Validation input trước khi gửi request
- Xử lý lỗi network và API errors

## 🐛 XỬ LÝ LỖI THƯỜNG GẶP

### Lỗi: "Not authenticated" hoặc 401

**Nguyên nhân**: Token hết hạn hoặc không hợp lệ

**Giải pháp**: Đăng xuất và đăng nhập lại

### Lỗi: "Could not load file or assembly 'Newtonsoft.Json'"

**Giải pháp**: Chạy `dotnet restore`

### Lỗi: "The remote name could not be resolved"

**Giải pháp**: Kiểm tra kết nối Internet

### Không hiển thị danh sách món ăn

**Giải pháp**: 
- Kiểm tra đã đăng nhập chưa
- Click nút "🔄 Làm mới"
- Kiểm tra API có hoạt động không

## ✅ KIỂM TRA

Sau khi chạy ứng dụng, kiểm tra:
- ✅ Form đăng nhập hiển thị đúng
- ✅ Có thể đăng ký tài khoản mới
- ✅ Có thể đăng nhập
- ✅ MainForm hiển thị sau khi đăng nhập
- ✅ Có thể xem danh sách món ăn
- ✅ Có thể thêm món ăn mới
- ✅ Có thể xóa món ăn
- ✅ Có thể chọn ngẫu nhiên
- ✅ Phân trang hoạt động đúng
- ✅ Có thể đăng xuất

## 📞 HỖ TRỢ

Nếu gặp vấn đề:
1. Kiểm tra kết nối Internet
2. Kiểm tra API có hoạt động: https://nt106.uitiot.vn/docs
3. Kiểm tra đã đăng nhập chưa
4. Thử đăng xuất và đăng nhập lại

---

**Lưu ý**: Đây là ứng dụng mẫu cho mục đích học tập. Trong môi trường production, cần thêm:
- Validation input đầy đủ hơn
- Xử lý token expiration và refresh token
- Error handling tốt hơn
- Loading indicators
- Confirmation dialogs cho các thao tác quan trọng

