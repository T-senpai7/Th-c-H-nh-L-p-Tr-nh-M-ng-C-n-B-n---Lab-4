# HƯỚNG DẪN KẾT NỐI BAI4 GIỮA 2 MÁY KHÁC NHAU

## 📋 TỔNG QUAN

Hướng dẫn này sẽ giúp bạn kết nối ứng dụng Bai4 giữa 2 máy tính khác nhau trên cùng một mạng (LAN) hoặc qua Internet.

## 🖥️ MÔI TRƯỜNG KẾT NỐI
### IP máy Duyên:  172.20.10.13  (mạng iphone em Tiến)
### Kết nối trong mạng LAN (Cùng mạng WiFi/Ethernet)
- ✅ Dễ dàng nhất, không cần cấu hình phức tạp
- ✅ Tốc độ nhanh, độ trễ thấp
- ✅ Không cần mở port trên router

### Kết nối qua Internet (Khác mạng)
- ⚠️ Cần cấu hình router (Port Forwarding)
- ⚠️ Cần biết IP công cộng (Public IP) của máy Server
- ⚠️ Có thể bị firewall chặn

---

## 🚀 HƯỚNG DẪN CHI TIẾT

### PHẦN 1: THIẾT LẬP MÁY SERVER

#### Bước 1: Lấy địa chỉ IP của máy Server

**Trên Windows:**

1. Mở **Command Prompt** (cmd) hoặc **PowerShell**
2. Gõ lệnh: `ipconfig` 
IPV4 Address: 172.27.176.1 
3. Tìm dòng **IPv4 Address** trong phần **Ethernet adapter** hoặc **Wireless LAN adapter**
   - Ví dụ: `IPv4 Address. . . . . . . . . . . . : 192.168.1.100`
4. Ghi lại địa chỉ IP này (ví dụ: `192.168.1.100`)

**Hoặc cách khác:**
- Nhấn `Win + R`, gõ `cmd`, Enter
- Gõ: `ipconfig | findstr IPv4`
- Sẽ hiển thị IP của bạn

#### Bước 2: Kiểm tra Firewall

**Windows Firewall:**

1. Mở **Windows Defender Firewall**
   - Nhấn `Win + S`, gõ "Firewall", chọn "Windows Defender Firewall"
2. Click **"Allow an app or feature through Windows Defender Firewall"**
3. Click **"Change settings"** (nếu cần)
4. Tìm ứng dụng **Bai4.exe** hoặc **dotnet.exe**
   - Nếu không thấy, click **"Allow another app..."**
   - Browse đến file `Bai4.exe` trong thư mục `bin\Debug\net8.0-windows\`
   - Đảm bảo cả **Private** và **Public** đều được check
5. Hoặc tạm thời tắt Firewall để test (không khuyến khích)

**Hoặc mở port 8080 trực tiếp:**

1. Mở **Windows Defender Firewall with Advanced Security**
2. Click **Inbound Rules** → **New Rule**
3. Chọn **Port** → Next
4. Chọn **TCP**, nhập port **8080** → Next
5. Chọn **Allow the connection** → Next
6. Check tất cả (Domain, Private, Public) → Next
7. Đặt tên: "Bai4 Server Port 8080" → Finish

#### Bước 3: Khởi động Server

1. Mở thư mục `Bai4` trong Command Prompt hoặc PowerShell:
   ```powershell
   cd "D:\Lab3_LTMCB(1,2,3)\Lab3_LTMCB\Bai4"
   ```

2. Chạy ứng dụng:
   ```powershell
   dotnet run
   ```

3. Trong menu, chọn **"TCP Server"**

4. Click nút **"Listen"**

5. Kiểm tra log hiển thị:
   - ✅ `Server started on port 8080`
   - ✅ `Server IP: 0.0.0.0 (Listening on all interfaces)`

6. **Ghi lại địa chỉ IP của máy Server** (ví dụ: `192.168.1.100`)

---

### PHẦN 2: THIẾT LẬP MÁY CLIENT

#### Bước 1: Khởi động Client

1. Mở thư mục `Bai4` trên máy Client:
   ```powershell
   cd "D:\Lab3_LTMCB(1,2,3)\Lab3_LTMCB\Bai4"
   ```

2. Chạy ứng dụng:
   ```powershell
   dotnet run
   ```

3. Trong menu, chọn **"TCP Client"**

#### Bước 2: Kết nối đến Server

1. Trong ô **"Server IP"**, nhập địa chỉ IP của máy Server
   - Ví dụ: `192.168.1.100` (KHÔNG dùng `127.0.0.1` vì đó là localhost):


2. Trong ô **"Port"**, nhập: `8080`

3. Click nút **"Kết nối"**

4. Nếu kết nối thành công:
   - ✅ Nút "Kết nối" sẽ đổi thành "Ngắt kết nối" (màu đỏ)
   - ✅ ComboBox "Tên phim" sẽ được kích hoạt và load danh sách phim
   - ✅ Có thể bắt đầu đặt vé

---

## 🔧 TROUBLESHOOTING (Xử lý lỗi)

### ❌ Lỗi: "No connection could be made because the target machine actively refused it"

**Nguyên nhân:**
- Server chưa được khởi động hoặc chưa click "Listen"
- IP address không đúng
- Port bị chặn bởi Firewall

**Giải pháp:**
1. ✅ Kiểm tra Server đã click "Listen" chưa
2. ✅ Kiểm tra IP address có đúng không (dùng `ipconfig` trên máy Server)
3. ✅ Kiểm tra Firewall trên máy Server đã cho phép port 8080 chưa
4. ✅ Thử ping từ máy Client đến máy Server:
   ```powershell
   ping 192.168.1.100
   ```
   (Thay `192.168.1.100` bằng IP của máy Server)

### ❌ Lỗi: "Timeout: Không thể kết nối đến server trong 5 giây"

**Nguyên nhân:**
- Server chưa sẵn sàng
- Firewall đang chặn
- Máy Client và Server không cùng mạng

**Giải pháp:**
1. ✅ Đảm bảo Server đã click "Listen" và hiển thị "Server started on port 8080"
2. ✅ Kiểm tra cả 2 máy đều cùng mạng WiFi hoặc cùng switch/router
3. ✅ Tạm thời tắt Firewall trên máy Server để test
4. ✅ Kiểm tra port 8080 có đang được sử dụng:
   ```powershell
   netstat -an | findstr :8080
   ```

### ❌ Lỗi: "Connection refused"

**Nguyên nhân:**
- Server không chấp nhận kết nối từ IP của Client
- Firewall chặn

**Giải pháp:**
1. ✅ Kiểm tra Firewall trên máy Server
2. ✅ Đảm bảo Server đang chạy và đã click "Listen"
3. ✅ Thử đóng và mở lại Server

### ❌ Không ping được máy Server

**Nguyên nhân:**
- Máy Client và Server không cùng mạng
- Firewall chặn ping (ICMP)

**Giải pháp:**
1. ✅ Kiểm tra cả 2 máy đều kết nối cùng WiFi/router
2. ✅ Kiểm tra IP của cả 2 máy có cùng subnet không
   - Ví dụ: `192.168.1.100` và `192.168.1.101` → ✅ Cùng subnet
   - Ví dụ: `192.168.1.100` và `192.168.2.101` → ❌ Khác subnet

---

## 🌐 KẾT NỐI QUA INTERNET (Khác mạng)

Nếu muốn kết nối qua Internet (máy Client và Server ở 2 mạng khác nhau):

### Yêu cầu:
1. **Máy Server cần có IP công cộng (Public IP)**
   - Kiểm tra: Truy cập https://whatismyipaddress.com trên máy Server
   - Ghi lại IP này

2. **Cấu hình Port Forwarding trên Router của máy Server:**
   - Đăng nhập vào router (thường là `192.168.1.1` hoặc `192.168.0.1`)
   - Tìm mục **Port Forwarding** hoặc **Virtual Server**
   - Thêm rule:
     - **External Port**: 8080
     - **Internal IP**: IP của máy Server trong mạng LAN (ví dụ: `192.168.1.100`)
     - **Internal Port**: 8080
     - **Protocol**: TCP
   - Lưu và áp dụng

3. **Máy Client kết nối bằng Public IP:**
   - Nhập Public IP của máy Server vào ô "Server IP"
   - Port: `8080`

### ⚠️ Lưu ý:
- Cần biết cách cấu hình router (mỗi router khác nhau)
- Có thể không an toàn (mở port ra Internet)
- Có thể bị ISP chặn
- Nên dùng VPN hoặc SSH tunnel cho an toàn hơn

---

## 📝 CHECKLIST KẾT NỐI

### Trên máy Server:
- [ ] Đã lấy được IP address của máy Server
- [ ] Đã mở port 8080 trên Firewall
- [ ] Đã chạy ứng dụng và chọn "TCP Server"
- [ ] Đã click nút "Listen"
- [ ] Log hiển thị "Server started on port 8080"

### Trên máy Client:
- [ ] Đã nhập đúng IP của máy Server (KHÔNG phải 127.0.0.1)
- [ ] Đã nhập đúng Port: 8080
- [ ] Đã click "Kết nối"
- [ ] Kết nối thành công (nút đổi màu đỏ, ComboBox phim được load)

### Kiểm tra mạng:
- [ ] Cả 2 máy cùng mạng WiFi/Ethernet
- [ ] Có thể ping được từ Client đến Server
- [ ] Firewall không chặn port 8080

---

## 🎯 VÍ DỤ THỰC TẾ

### Tình huống: Kết nối 2 máy trong cùng mạng WiFi

**Máy Server:**
1. IP: `192.168.1.100` (lấy từ `ipconfig`)
2. Mở Firewall cho port 8080
3. Chạy `dotnet run` → Chọn "TCP Server" → Click "Listen"

**Máy Client:**
1. Chạy `dotnet run` → Chọn "TCP Client"
2. Nhập IP: `192.168.1.100`
3. Nhập Port: `8080`
4. Click "Kết nối"
5. ✅ Kết nối thành công!

---

## 💡 MẸO HỮU ÍCH

1. **Kiểm tra kết nối nhanh:**
   ```powershell
   # Trên máy Client, test kết nối đến Server
   Test-NetConnection -ComputerName 192.168.1.100 -Port 8080
   ```

2. **Xem các port đang mở:**
   ```powershell
   netstat -an | findstr LISTENING
   ```

3. **Tìm process đang dùng port 8080:**
   ```powershell
   netstat -ano | findstr :8080
   ```

4. **Nếu vẫn không kết nối được:**
   - Thử tạm thời tắt Firewall trên cả 2 máy để test
   - Nếu tắt Firewall mà kết nối được → Vấn đề là Firewall
   - Nếu vẫn không được → Vấn đề là mạng hoặc IP

---

## 📞 HỖ TRỢ

Nếu vẫn gặp vấn đề sau khi làm theo hướng dẫn:

1. Kiểm tra log trên Server xem có lỗi gì không
2. Kiểm tra Firewall settings trên cả 2 máy
3. Đảm bảo cả 2 máy cùng mạng
4. Thử ping và test port connection

---

**Chúc bạn kết nối thành công! 🎉**

