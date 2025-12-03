# HƯỚNG DẪN NHANH - KẾT NỐI 2 MÁY KHÁC NHAU

## 🚀 QUICK START (3 BƯỚC)
### BƯỚC 1: Trên máy SERVER
```powershell
# 1. Lấy IP của máy Server
ipconfig
# Ghi lại IPv4 Address (ví dụ: 192.168.1.100)

# 2. Mở Firewall cho port 8080
# - Mở Windows Defender Firewall
# - Allow port 8080 (TCP) cho Inbound Rules

# 3. Chạy Server
cd "D:\Lab3_LTMCB(1,2,3)\Lab3_LTMCB\Bai4"
dotnet run
# → Chọn "TCP Server" → Click "Listen"
```

### BƯỚC 2: Trên máy CLIENT
```powershell
# 1. Chạy Client
cd "D:\Lab3_LTMCB(1,2,3)\Lab3_LTMCB\Bai4"
dotnet run
# → Chọn "TCP Client"

# 2. Nhập thông tin kết nối
# - Server IP: [IP của máy Server] (ví dụ: 192.168.1.100)
# - Port: 8080
# → Click "Kết nối"
```

### BƯỚC 3: Kiểm tra
- ✅ Nút "Kết nối" đổi thành "Ngắt kết nối" (màu đỏ)
- ✅ ComboBox "Tên phim" được load
- ✅ Có thể đặt vé

---

## 🔍 LẤY IP CỦA MÁY SERVER

**Windows:**
```powershell
ipconfig | findstr IPv4
```

**Kết quả ví dụ:**
```
IPv4 Address. . . . . . . . . . . . : 192.168.1.100
```

→ **Ghi lại IP này** (ví dụ: `192.168.1.100`)

---

## 🔥 MỞ FIREWALL (Windows)

### Cách 1: Mở port 8080
1. Mở **Windows Defender Firewall with Advanced Security**
2. **Inbound Rules** → **New Rule**
3. Chọn **Port** → **TCP** → Port **8080**
4. **Allow the connection** → Next → Finish

### Cách 2: Cho phép ứng dụng
1. Mở **Windows Defender Firewall**
2. **Allow an app through firewall**
3. Tìm **Bai4.exe** hoặc thêm mới
4. Check **Private** và **Public**

---

## ❌ XỬ LÝ LỖI THƯỜNG GẶP

### "Connection refused" hoặc "Target machine actively refused"
✅ **Kiểm tra:**
1. Server đã click "Listen" chưa?
2. IP address có đúng không? (KHÔNG dùng 127.0.0.1)
3. Firewall đã mở port 8080 chưa?
4. Cả 2 máy có cùng mạng WiFi/Ethernet không?

### "Timeout"
✅ **Kiểm tra:**
1. Server đã sẵn sàng chưa?
2. Ping được từ Client đến Server không?
   ```powershell
   ping 192.168.1.100
   ```

### Không ping được
✅ **Kiểm tra:**
1. Cả 2 máy cùng mạng không?
2. IP có cùng subnet không? (ví dụ: 192.168.1.x)

---

## 📋 CHECKLIST

### Server:
- [ ] Đã lấy IP (dùng `ipconfig`)
- [ ] Đã mở Firewall port 8080
- [ ] Đã chạy và click "Listen"

### Client:
- [ ] Đã nhập đúng IP Server (KHÔNG phải 127.0.0.1)
- [ ] Đã nhập Port: 8080
- [ ] Đã click "Kết nối"

---

## 💡 MẸO

**Test kết nối nhanh:**
```powershell
Test-NetConnection -ComputerName 192.168.1.100 -Port 8080
```

**Xem port đang mở:**
```powershell
netstat -an | findstr :8080
```

---

## 🌐 WEB BOOKING - TỰ ĐỘNG KẾT NỐI

### BƯỚC 1: Trên máy SERVER
```powershell
# 1. Lấy IP của máy Server
ipconfig
# Ghi lại IPv4 Address (ví dụ: 192.168.1.100)

# 2. Mở Firewall cho port 8888 (HTTP) và 8080 (TCP)
# - Mở Windows Defender Firewall
# - Allow port 8888 và 8080 (TCP) cho Inbound Rules

# 3. Chạy Server
cd "D:\LAB04-NT106-main\LAB04-NT106-main\Bai4"
dotnet run
# → Chọn "Web Server" → Click "Start HTTP Server"
# → Chọn "TCP Server" → Click "Start TCP Server" (nếu dùng TCP mode)
```

### BƯỚC 2: Trên máy CLIENT
```powershell
# 1. Mở trình duyệt
# Truy cập: http://[IP_SERVER]:8888/booking.html
# Ví dụ: http://192.168.1.100:8888/booking.html

# 2. Nhập thông tin kết nối
# - Server IP: [IP của máy Server] (ví dụ: 192.168.1.100)
# - Port: 8888 (HTTP mode) hoặc 8080 (TCP mode)
# → Hệ thống sẽ TỰ ĐỘNG KẾT NỐI sau 1 giây ngừng gõ
# → Hoặc nhấn Enter để kết nối ngay
```

### BƯỚC 3: Kiểm tra
- ✅ Trạng thái hiển thị: **"Đã kết nối"** (màu xanh)
- ✅ ComboBox "Tên phim" được load tự động
- ✅ Có thể đặt vé ngay

### ✨ TÍNH NĂNG TỰ ĐỘNG KẾT NỐI:
- **Sau 1 giây ngừng gõ** → Tự động kết nối (im lặng)
- **Khi rời khỏi ô nhập** → Tự động kết nối (im lặng)
- **Nhấn Enter** → Tự động kết nối (có hiển thị alert nếu lỗi)
- **Không cần click nút "Kết nối"** (nhưng vẫn có thể click nếu muốn)

---

📖 **Xem hướng dẫn chi tiết:**
- **Web Booking**: `WEB_BOOKING_README.md`
- **TCP Client-Server**: `HUONG_DAN_KET_NOI_MAY_KHAC.md`

