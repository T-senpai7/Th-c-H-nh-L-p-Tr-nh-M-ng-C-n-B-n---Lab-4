# Hướng Dẫn Kết Nối Từ Xa - Web Server Booking System

## Tổng Quan

Tài liệu này hướng dẫn cách để các máy tính khác trong mạng có thể kết nối vào máy tính của bạn để sử dụng hệ thống đặt vé qua Web Server.

## Yêu Cầu

- Máy server đã cài đặt và chạy được ứng dụng booking system
- Máy server và máy client phải cùng mạng LAN (Local Area Network)
- Port 8888 phải được mở trên firewall

---

## Bước 1: Lấy Địa Chỉ IP Của Máy Server

### Trên Windows:

1. **Cách 1: Sử dụng Command Prompt**
   - Nhấn `Win + R`, gõ `cmd` và nhấn Enter
   - Gõ lệnh: `ipconfig`
   - Tìm dòng **IPv4 Address** trong phần **Ethernet adapter** hoặc **Wireless LAN adapter**
   - Ví dụ: `192.168.1.100`

2. **Cách 2: Sử dụng PowerShell**
   - Nhấn `Win + X`, chọn **Windows PowerShell**
   - Gõ lệnh: `Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -notlike "*Loopback*"}`
   - Tìm địa chỉ IP trong kết quả

3. **Cách 3: Qua Settings**
   - Mở **Settings** → **Network & Internet** → **Wi-Fi** (hoặc **Ethernet**)
   - Click vào tên mạng đang kết nối
   - Xem **IPv4 address**

### Trên Linux/Mac:

```bash
# Linux
ip addr show
# hoặc
ifconfig

# Mac
ifconfig | grep "inet "
```

**Lưu ý:** Ghi lại địa chỉ IP này, bạn sẽ cần nó ở bước sau.

---

## Bước 2: Cấu Hình Firewall

### Windows Firewall:

1. **Mở Windows Defender Firewall:**
   - Nhấn `Win + R`, gõ `wf.msc` và nhấn Enter
   - Hoặc vào **Control Panel** → **System and Security** → **Windows Defender Firewall**

2. **Thêm Rule cho Port 8888:**
   - Click **Advanced settings** ở bên trái
   - Click **Inbound Rules** → **New Rule...**
   - Chọn **Port** → **Next**
   - Chọn **TCP**, nhập **8888** vào **Specific local ports** → **Next**
   - Chọn **Allow the connection** → **Next**
   - Chọn tất cả profiles (Domain, Private, Public) → **Next**
   - Đặt tên: `Web Server Booking System - Port 8888` → **Finish**

3. **Hoặc tắt Firewall tạm thời (Chỉ dùng khi test):**
   - ⚠️ **Cảnh báo:** Chỉ làm điều này trong môi trường test an toàn
   - Vào **Windows Defender Firewall** → **Turn Windows Defender Firewall on or off**
   - Tắt cho **Private network** (không nên tắt cho Public)

### Router Firewall (Nếu cần):

- Thông thường không cần cấu hình router nếu cả hai máy cùng mạng LAN
- Nếu kết nối từ internet (khác mạng), cần:
  1. Cấu hình port forwarding trên router (port 8888)
  2. Cho phép máy server trong DMZ (không khuyến nghị vì lý do bảo mật)

---

## Bước 3: Khởi Động Web Server

1. **Trên máy server:**
   - Chạy ứng dụng booking system
   - Chọn **"Web Server"** từ menu
   - Click **"Start HTTP Server"**
   - Server sẽ khởi động và hiển thị:
     ```
     HTTP Server started at http://0.0.0.0:8888
     Server accessible from localhost: http://localhost:8888
     Server accessible from network: http://<your-ip>:8888
     ```

2. **Kiểm tra server đang chạy:**
   - Trên máy server, mở browser và truy cập: `http://localhost:8888/booking.html`
   - Nếu trang web hiển thị bình thường, server đã sẵn sàng

---

## Bước 4: Kết Nối Từ Máy Client

### Cách 1: Truy Cập Trực Tiếp (Khuyến Nghị)

1. **Trên máy client:**
   - Mở trình duyệt web (Chrome, Firefox, Edge, Safari...)
   - Truy cập địa chỉ: `http://<IP-máy-server>:8888/booking.html`
   
   **Ví dụ:**
   - Nếu IP máy server là `192.168.1.100`
   - Truy cập: `http://192.168.1.100:8888/booking.html`

2. **Các trang có sẵn:**
   - Trang đặt vé: `http://<IP>:8888/booking.html`
   - Trang xem phim: `http://<IP>:8888/Viewing.html`
   - Trang chủ: `http://<IP>:8888/` (sẽ tự động chuyển đến Viewing.html)

### Cách 2: Tạo Bookmark/Shortcut

Để tiện sử dụng, bạn có thể:

1. **Tạo bookmark trong browser:**
   - Truy cập trang booking
   - Nhấn `Ctrl + D` (hoặc `Cmd + D` trên Mac)
   - Đặt tên: "Đặt Vé - Cinema Booking"
   - Lưu bookmark

2. **Tạo shortcut trên Desktop (Windows):**
   - Right-click trên Desktop → **New** → **Shortcut**
   - Nhập URL: `http://<IP-máy-server>:8888/booking.html`
   - Đặt tên: "Cinema Booking"
   - Click **Finish**

---

## Bước 5: Sử Dụng Hệ Thống

1. **Kết nối Server:**
   - Trên trang booking, click nút **"Kết nối"**
   - Hệ thống sẽ tự động kết nối đến server

2. **Đặt vé:**
   - Nhập tên khách hàng
   - Chọn phim
   - Chọn phòng chiếu
   - Chọn ghế ngồi
   - Xem tổng tiền
   - Click **"Đặt Vé"**

3. **Xác nhận:**
   - Sau khi đặt vé thành công, thông tin sẽ được lưu vào:
     - Database: `cinema_dataweb.db`
     - Log file: `output_booking.json` (trong thư mục Bai4)

---

## Kiểm Tra Kết Nối

### Trên Máy Server:

1. **Kiểm tra server đang lắng nghe:**
   ```cmd
   netstat -an | findstr 8888
   ```
   - Nếu thấy `0.0.0.0:8888` hoặc `[::]:8888`, server đang lắng nghe trên tất cả interfaces

2. **Kiểm tra firewall:**
   ```cmd
   netsh advfirewall firewall show rule name="Web Server Booking System - Port 8888"
   ```

### Trên Máy Client:

1. **Ping máy server:**
   ```cmd
   ping <IP-máy-server>
   ```
   - Nếu ping thành công, hai máy đã kết nối mạng

2. **Test port 8888:**
   ```cmd
   telnet <IP-máy-server> 8888
   ```
   - Nếu kết nối thành công, port đã được mở

---

## Troubleshooting

### Vấn Đề 1: Không thể truy cập từ máy khác

**Nguyên nhân có thể:**
- Firewall chưa được cấu hình
- Server chưa khởi động
- IP address không đúng
- Hai máy không cùng mạng

**Giải pháp:**
1. Kiểm tra server đã khởi động chưa
2. Kiểm tra firewall đã mở port 8888 chưa
3. Kiểm tra IP address của máy server
4. Đảm bảo cả hai máy cùng mạng (cùng router/switch)

### Vấn Đề 2: Trang web không tải được

**Nguyên nhân có thể:**
- Server chưa khởi động
- Port 8888 bị chiếm dụng
- File HTML không tồn tại

**Giải pháp:**
1. Kiểm tra server đã khởi động và không có lỗi
2. Kiểm tra port 8888 có bị ứng dụng khác sử dụng không:
   ```cmd
   netstat -ano | findstr 8888
   ```
3. Đảm bảo file `booking.html` tồn tại trong thư mục Bai4

### Vấn Đề 3: Kết nối bị timeout

**Nguyên nhân có thể:**
- Firewall block
- Router block
- IP address sai

**Giải pháp:**
1. Tạm thời tắt firewall để test
2. Kiểm tra lại IP address
3. Thử ping máy server từ máy client

### Vấn Đề 4: Lỗi CORS (Cross-Origin Resource Sharing)

**Nguyên nhân:**
- Browser chặn request từ domain khác

**Giải pháp:**
- Server đã được cấu hình CORS, nên vấn đề này không xảy ra
- Nếu vẫn gặp, kiểm tra lại cấu hình CORS trong `SimpleHttpServer.cs`

---

## Bảo Mật

### Khuyến Nghị:

1. **Chỉ sử dụng trong mạng LAN:**
   - Không nên expose server ra internet công cộng
   - Chỉ cho phép các máy trong mạng nội bộ truy cập

2. **Sử dụng Firewall:**
   - Luôn bật firewall và chỉ mở port cần thiết (8888)
   - Không tắt firewall hoàn toàn

3. **Kiểm tra kết nối:**
   - Chỉ cho phép các máy đáng tin cậy kết nối
   - Theo dõi log để phát hiện truy cập bất thường

4. **Cập nhật:**
   - Thường xuyên cập nhật hệ điều hành và ứng dụng
   - Sử dụng phần mềm diệt virus

### Cảnh Báo:

- ⚠️ **Không expose server ra internet** nếu không có biện pháp bảo mật phù hợp
- ⚠️ **Không tắt firewall** trong môi trường production
- ⚠️ **Không chia sẻ IP server** với người không đáng tin cậy

---

## Cấu Hình Nâng Cao

### Thay Đổi Port (Nếu cần):

1. Mở file `WebServerForm.cs`
2. Tìm dòng: `private const int HTTP_PORT = 8888;`
3. Thay đổi số port (ví dụ: 8080, 9000...)
4. **Lưu ý:** Cần cấu hình lại firewall cho port mới

### Chạy Server Tự Động Khi Khởi Động:

1. Tạo shortcut của ứng dụng
2. Copy vào thư mục Startup:
   - Windows: `C:\Users\<username>\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup`
3. Server sẽ tự động khởi động khi đăng nhập Windows

---

## Tóm Tắt

✅ **Để kết nối từ máy khác:**

1. Lấy IP của máy server (ví dụ: `192.168.1.100`)
2. Mở port 8888 trên firewall
3. Khởi động Web Server trên máy server
4. Từ máy client, truy cập: `http://<IP-server>:8888/booking.html`

✅ **Lưu ý quan trọng:**

- Cả hai máy phải cùng mạng LAN
- Firewall phải cho phép port 8888
- Server phải đang chạy
- File `booking.html` phải tồn tại trong thư mục Bai4

---

## Hỗ Trợ

Nếu gặp vấn đề, kiểm tra:

1. Console log của server để xem lỗi
2. Browser console (F12) để xem lỗi JavaScript
3. Network tab trong browser để xem request/response
4. File `ARCHITECTURE_CONNECTION.md` để hiểu rõ hơn về kiến trúc

---

**Chúc bạn sử dụng thành công! 🎬🎫**

