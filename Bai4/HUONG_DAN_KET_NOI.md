# HƯỚNG DẪN KHẮC PHỤC LỖI KẾT NỐI

## 🔴 Lỗi: "No connection could be made because the target machine actively refused it"

### Nguyên nhân:
Lỗi này xảy ra khi Client không thể kết nối đến Server. Có thể do:

1. **Server chưa được khởi động**
2. **Server chưa click nút "Listen"**
3. **Port 8080 bị chặn bởi Firewall**
4. **IP address không đúng**

## ✅ CÁCH KHẮC PHỤC

### Bước 1: Khởi động Server TRƯỚC

1. Chạy ứng dụng: `dotnet run`
2. Chọn **"TCP Server"**
3. Click nút **"Listen"**
4. Kiểm tra log hiển thị: "Server started on port 8080"

### Bước 2: Khởi động Client SAU

1. Chạy ứng dụng: `dotnet run` (có thể chạy nhiều instance)
2. Chọn **"TCP Client"**
3. Nhập IP: `127.0.0.1` (cho localhost)
4. Click nút **"Kết nối"**

### Bước 3: Kiểm tra Firewall

Nếu vẫn không kết nối được:

1. Mở **Windows Defender Firewall**
2. Cho phép ứng dụng qua firewall
3. Hoặc tạm thời tắt firewall để test

### Bước 4: Kiểm tra Port

Kiểm tra xem port 8080 có đang được sử dụng:

```powershell
netstat -an | findstr :8080
```

Nếu có process đang sử dụng port 8080, có thể:
- Đóng process đó
- Hoặc đổi port trong code (sửa PORT = 8080 thành PORT = 8081)

## 📝 THỨ TỰ CHẠY ĐÚNG

1. **Bước 1**: Chạy Server → Click "Listen"
2. **Bước 2**: Chạy Client → Nhập IP → Click "Kết nối"
3. **Bước 3**: Đặt vé trên Client

## ⚠️ LƯU Ý

- **Luôn chạy Server trước Client**
- **Server phải click "Listen" trước khi Client kết nối**
- **IP address mặc định: 127.0.0.1 (localhost)**
- **Port mặc định: 8080**

## 🔧 TROUBLESHOOTING

### Lỗi: "Timeout: Không thể kết nối đến server trong 5 giây"

**Nguyên nhân**: Server chưa sẵn sàng

**Giải pháp**:
1. Kiểm tra Server đã click "Listen" chưa
2. Kiểm tra log trên Server có hiển thị "Server started on port 8080" không
3. Đợi vài giây sau khi click "Listen" rồi mới kết nối Client

### Lỗi: "Connection refused"

**Nguyên nhân**: Server không chấp nhận kết nối

**Giải pháp**:
1. Đảm bảo Server đang chạy và đã click "Listen"
2. Kiểm tra Firewall không chặn port 8080
3. Thử đóng và mở lại Server

### Lỗi: "Address already in use"

**Nguyên nhân**: Port 8080 đang được sử dụng bởi process khác

**Giải pháp**:
1. Tìm và đóng process đang sử dụng port 8080
2. Hoặc đổi port trong code

---

**Nếu vẫn gặp lỗi**, vui lòng kiểm tra:
1. Server log để xem có lỗi gì không
2. Firewall settings
3. Port có bị chặn không

