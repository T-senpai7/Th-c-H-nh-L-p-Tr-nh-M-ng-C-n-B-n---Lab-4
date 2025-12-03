# HƯỚNG DẪN CHI TIẾT WEB BOOKING SYSTEM (TỰ ĐỘNG KẾT NỐI)

## 📋 TỔNG QUAN

Hệ thống đặt vé phim qua Web với tính năng **TỰ ĐỘNG KẾT NỐI** - Client chỉ cần nhập IP và Port đúng, hệ thống sẽ tự động kết nối với Server để đặt vé mà không cần click nút "Kết nối".

### 🎯 Điểm nổi bật:

- **Tự động kết nối thông minh**: Hệ thống tự động phát hiện và kết nối khi nhập IP/Port đúng
- **Hai chế độ kết nối linh hoạt**: HTTP Mode cho local, TCP Mode cho remote
- **Giao diện hiện đại**: UI đẹp với Tailwind CSS, responsive trên mọi thiết bị
- **Đồng bộ real-time**: Cập nhật trạng thái ghế ngay lập tức khi có người đặt
- **Tính giá tự động**: Tính toán giá vé theo loại (vớt/thường/VIP) tự động

## ✨ TÍNH NĂNG TỰ ĐỘNG KẾT NỐI

### 🎯 Cách hoạt động chi tiết:

#### 1. Tự động kết nối khi nhập IP/Port:

**a) Sau khi ngừng gõ 1 giây:**
- Hệ thống tự động kiểm tra IP và Port có hợp lệ không
- Nếu hợp lệ, tự động thử kết nối
- **Chế độ im lặng**: Không hiển thị alert nếu thất bại (chỉ log vào console)
- Giúp trải nghiệm mượt mà, không làm gián đoạn người dùng

**b) Khi rời khỏi ô nhập (blur):**
- Khi người dùng click ra ngoài ô nhập IP hoặc Port
- Hệ thống tự động thử kết nối
- **Chế độ im lặng**: Không hiển thị alert nếu thất bại

**c) Khi nhấn Enter:**
- Người dùng nhấn Enter trong ô nhập IP hoặc Port
- Hệ thống kết nối ngay lập tức
- **Có hiển thị alert**: Nếu có lỗi, sẽ hiển thị thông báo rõ ràng

#### 2. Chế độ im lặng:

- **Tự động kết nối**: Không hiển thị alert khi thất bại (chỉ log vào console)
- **Nhấn Enter**: Vẫn hiển thị alert nếu có lỗi (người dùng chủ động)
- **Lợi ích**: Tránh làm phiền người dùng khi đang nhập liệu

#### 3. Hai chế độ kết nối:

**HTTP Mode (Kết nối local):**
- Khi nhập: `127.0.0.1`, `localhost` hoặc IP của Web Server hiện tại
- Port: `8888` (port của Web Server)
- Kết nối trực tiếp với Web Server qua HTTP API
- Sử dụng database của Web Server

**TCP Mode (Kết nối remote):**
- Khi nhập: IP khác (ví dụ: `192.168.1.100`)
- Port: `8080` (port của TCP Server)
- Web Server hoạt động như proxy
- Client → Web Server → TCP Server → Database

## 🚀 HƯỚNG DẪN SỬ DỤNG CHI TIẾT

### BƯỚC 1: Chuẩn bị và khởi động Web Server

#### 1.1. Trên máy Server:

**a) Mở Command Prompt hoặc PowerShell:**
```powershell
# Di chuyển đến thư mục dự án
cd "D:\LAB04-NT106-main\LAB04-NT106-main\Bai4"

# Hoặc đường dẫn của bạn
cd "đường-dẫn-đến-thư-mục-Bai4"
```

**b) Chạy ứng dụng:**
```powershell
dotnet run
```

**c) Chọn "Web Server" từ menu:**
- Ứng dụng sẽ hiển thị menu
- Chọn option tương ứng với "Web Server"

**d) Click nút "Start HTTP Server":**
- Server sẽ khởi động trên port `8888`
- Thông báo: "HTTP Server: Running on port 8888"
- Log hiển thị: "✓ HTTP Server started on port 8888"

**e) (Tùy chọn) Crawl Movies:**
- Click nút **"Crawl Movies"** để lấy dữ liệu phim từ betacinemas.vn
- Dữ liệu sẽ được lưu vào `movies.json`
- Nếu không crawl, hệ thống sẽ dùng dữ liệu mặc định

#### 1.2. Lấy IP của máy Server (Nếu client từ máy khác):

**Windows:**
```powershell
ipconfig
```

**Tìm dòng:**
```
IPv4 Address. . . . . . . . . . . . : 192.168.1.100
```

**Ghi lại IP này** (ví dụ: `192.168.1.100`)

#### 1.3. Mở Firewall (Nếu client từ máy khác):

**Cách 1: Mở port qua Windows Defender Firewall:**

1. Mở **Windows Defender Firewall with Advanced Security**
2. Click **Inbound Rules** → **New Rule**
3. Chọn **Port** → Next
4. Chọn **TCP** → Nhập port **8888** → Next
5. Chọn **Allow the connection** → Next
6. Check tất cả (Domain, Private, Public) → Next
7. Đặt tên: "Bai4 HTTP Server" → Finish

**Lặp lại cho port 8080** (nếu dùng TCP mode)

**Cách 2: Mở port qua PowerShell (Admin):**
```powershell
# Mở port 8888
New-NetFirewallRule -DisplayName "Bai4 HTTP Server" -Direction Inbound -LocalPort 8888 -Protocol TCP -Action Allow

# Mở port 8080 (nếu dùng TCP mode)
New-NetFirewallRule -DisplayName "Bai4 TCP Server" -Direction Inbound -LocalPort 8080 -Protocol TCP -Action Allow
```

### BƯỚC 2: Khởi động TCP Server (Nếu dùng TCP Mode)

**Trên máy Server (cùng ứng dụng):**

1. Chọn **"TCP Server"** từ menu
2. Click nút **"Start TCP Server"** hoặc **"Listen"**
3. TCP Server sẽ chạy trên port `8080`
4. Thông báo: "TCP Server: Running on port 8080"
5. Log hiển thị: "✓ TCP Server started on port 8080"

**Lưu ý:** 
- TCP Server chỉ cần thiết nếu client từ máy khác và muốn dùng TCP mode
- Nếu client cùng máy hoặc dùng HTTP mode, không cần TCP Server

### BƯỚC 3: Client truy cập Web Booking

#### 3.1. Mở trình duyệt:

**Trên máy Client:**

1. Mở trình duyệt (Chrome, Edge, Firefox, Safari...)
2. Truy cập URL:

   **Nếu cùng máy với Server:**
   ```
   http://localhost:8888/booking.html
   ```

   **Nếu máy khác:**
   ```
   http://[IP_SERVER]:8888/booking.html
   ```
   
   Ví dụ: `http://192.168.1.100:8888/booking.html`

#### 3.2. Giao diện Web Booking:

Bạn sẽ thấy giao diện với các phần:

- **Header**: Logo "CineBook" và nút "Quay lại"
- **Kết nối Server**: Ô nhập IP và Port
- **Thông tin khách hàng**: Ô nhập họ và tên
- **Chọn phim và phòng**: Dropdown chọn phim và phòng
- **Chọn ghế**: Lưới ghế với màu sắc khác nhau
- **Thông tin đặt vé**: Panel bên phải hiển thị thông tin và tổng tiền

### BƯỚC 4: Tự động kết nối và đặt vé

#### 4.1. Nhập thông tin kết nối:

**Trong phần "Kết nối Server":**

**a) Server IP:**
- **Nếu cùng máy**: Nhập `127.0.0.1` hoặc `localhost`
- **Nếu máy khác**: Nhập IP của máy Server (ví dụ: `192.168.1.100`)
- **Lưu ý**: KHÔNG dùng `127.0.0.1` khi kết nối từ máy khác

**b) Port:**
- **HTTP Mode**: `8888` (port của Web Server)
- **TCP Mode**: `8080` (port của TCP Server)

#### 4.2. Tự động kết nối:

**Hệ thống sẽ tự động kết nối trong các trường hợp sau:**

✅ **Sau 1 giây ngừng gõ:**
- Bạn nhập IP hoặc Port
- Ngừng gõ 1 giây
- Hệ thống tự động kiểm tra và kết nối
- Không hiển thị alert nếu thất bại (im lặng)

✅ **Khi rời khỏi ô nhập (blur):**
- Bạn click ra ngoài ô nhập IP hoặc Port
- Hệ thống tự động thử kết nối
- Không hiển thị alert nếu thất bại (im lặng)

✅ **Khi nhấn Enter:**
- Bạn nhấn Enter trong ô nhập IP hoặc Port
- Hệ thống kết nối ngay lập tức
- **Có hiển thị alert** nếu có lỗi

**Hoặc click nút "Kết nối"** để kết nối thủ công

#### 4.3. Kiểm tra trạng thái kết nối:

**Sau khi kết nối thành công:**

- ✅ Nút "Kết nối" đổi thành:
  - **"Đã kết nối (HTTP)"** (màu xanh) - nếu dùng HTTP mode
  - **"Đã kết nối (TCP)"** (màu xanh) - nếu dùng TCP mode
- ✅ Trạng thái hiển thị: **"Đã kết nối"** (màu xanh, có dấu chấm xanh)
- ✅ ComboBox "Tên phim" được load tự động (có danh sách phim)
- ✅ Có thể bắt đầu đặt vé ngay

**Nếu kết nối thất bại:**
- ❌ Trạng thái hiển thị: **"Chưa kết nối"** (màu đỏ)
- ❌ ComboBox "Tên phim" bị disable
- ❌ Không thể đặt vé

### BƯỚC 5: Đặt vé chi tiết

#### 5.1. Nhập thông tin khách hàng:

1. Tìm phần **"Thông tin khách hàng"**
2. Nhập họ và tên vào ô **"Nhập họ và tên"**
3. Ví dụ: "Nguyễn Văn A"
4. Thông tin sẽ tự động cập nhật vào panel "Thông tin đặt vé" bên phải

#### 5.2. Chọn phim:

1. Tìm phần **"Chọn phim và phòng"**
2. Click vào dropdown **"Tên phim"**
3. Chọn một phim từ danh sách
4. Ví dụ: "Đào, phở và piano - 45,000 VNĐ"
5. Sau khi chọn phim:
   - Dropdown "Phòng chiếu" sẽ tự động load các phòng có phim đó
   - Dropdown "Phòng chiếu" sẽ được enable

#### 5.3. Chọn phòng:

1. Click vào dropdown **"Phòng chiếu"**
2. Chọn một phòng từ danh sách
3. Ví dụ: "Phòng 1"
4. Sau khi chọn phòng:
   - Lưới ghế sẽ tự động load trạng thái ghế của phòng đó
   - Ghế đã đặt sẽ hiển thị màu xám
   - Ghế còn trống sẽ hiển thị màu tương ứng với loại vé

#### 5.4. Chọn ghế:

**Màu sắc ghế:**
- 🟡 **Vàng (Vé vớt)**: A1, A5, C1, C5 - Giá = 25% giá cơ bản
- 🟢 **Xanh lá (Vé thường)**: A2, A3, A4, C2, C3, C4 - Giá = 100% giá cơ bản
- 🔴 **Đỏ (Vé VIP)**: B1, B2, B3, B4, B5 - Giá = 200% giá cơ bản
- ⚫ **Xám**: Ghế đã được đặt (không thể chọn)

**Cách chọn:**
1. Click vào ghế còn trống để chọn
2. Ghế đã chọn sẽ có:
   - Viền vàng sáng
   - Highlight (scale lớn hơn)
   - Hiển thị trong panel "Thông tin đặt vé"
3. Click lại để bỏ chọn
4. Có thể chọn nhiều ghế cùng lúc

**Ví dụ:**
- Chọn ghế A1 (Vé vớt) → Giá: 11,250 VNĐ (25% của 45,000)
- Chọn ghế A2 (Vé thường) → Giá: 45,000 VNĐ
- Chọn ghế B1 (Vé VIP) → Giá: 90,000 VNĐ
- **Tổng tiền**: 146,250 VNĐ

#### 5.5. Xem thông tin đặt vé:

**Panel bên phải hiển thị:**

- **Khách hàng**: Tên đã nhập
- **Phim**: Tên phim đã chọn
- **Phòng**: Tên phòng đã chọn
- **Ghế đã chọn**: Danh sách ghế (ví dụ: "A1, A2, B1")
- **Chi tiết giá**: 
  - A1 (Vé vớt): 11,250 VNĐ
  - A2 (Vé thường): 45,000 VNĐ
  - B1 (Vé VIP): 90,000 VNĐ
- **Tổng tiền**: 146,250 VNĐ (tự động tính)

#### 5.6. Đặt vé:

1. Kiểm tra lại thông tin:
   - ✅ Đã nhập tên khách hàng
   - ✅ Đã chọn phim
   - ✅ Đã chọn phòng
   - ✅ Đã chọn ít nhất 1 ghế
   - ✅ Đã kết nối server

2. Click nút **"Đặt Vé"** (màu vàng, ở cuối panel bên phải)

3. Hệ thống sẽ:
   - Gửi yêu cầu đến Server
   - Server kiểm tra ghế còn trống không
   - Server đặt vé nếu hợp lệ
   - Cập nhật database

4. **Nếu thành công:**
   - Hiển thị popup xác nhận với thông tin chi tiết
   - Ghế đã đặt sẽ chuyển sang màu xám
   - Thông tin booking được lưu vào `output_booking.json`
   - Các client khác sẽ tự động cập nhật trạng thái ghế

5. **Nếu thất bại:**
   - Hiển thị thông báo lỗi
   - Ví dụ: "Ghế A1 đã được đặt bởi người khác"
   - Refresh trạng thái ghế và thử lại

## 🔄 HAI CHẾ ĐỘ KẾT NỐI CHI TIẾT

### HTTP Mode (Kết nối local)

#### Khi nào sử dụng:
- ✅ Client và Server cùng một máy
- ✅ Client truy cập Web Server trực tiếp qua trình duyệt
- ✅ Muốn kết nối nhanh, không cần TCP Server

#### Cách nhận biết:
- **Server IP**: `127.0.0.1`, `localhost` hoặc IP của Web Server hiện tại
- **Port**: `8888` (port của Web Server)
- **Nút hiển thị**: **"Đã kết nối (HTTP)"** (màu xanh)

#### Cách hoạt động:
```
Client (Browser) 
    ↓ HTTP Request
Web Server (Port 8888)
    ↓ Direct API Call
CinemaWebDatabase
    ↓ SQLite
cinema_database.db
```

**Ưu điểm:**
- Kết nối nhanh, trực tiếp
- Không cần TCP Server
- Đơn giản, dễ sử dụng

**Nhược điểm:**
- Chỉ hoạt động khi client và server cùng máy
- Hoặc client truy cập Web Server trực tiếp

### TCP Mode (Kết nối remote)

#### Khi nào sử dụng:
- ✅ Client và Server ở 2 máy khác nhau
- ✅ Cần kết nối với TCP Server remote
- ✅ Muốn tận dụng hệ thống TCP Client-Server có sẵn

#### Cách nhận biết:
- **Server IP**: IP của máy Server (ví dụ: `192.168.1.100`)
- **Port**: `8080` (port của TCP Server)
- **Nút hiển thị**: **"Đã kết nối (TCP)"** (màu xanh)

#### Cách hoạt động:
```
Client (Browser)
    ↓ HTTP Request
Web Server (Port 8888) - Proxy
    ↓ TCP Connection
TCP Server (Port 8080)
    ↓ Database Access
CinemaDatabase
    ↓ SQLite
cinema_database.db
```

**Ưu điểm:**
- Hoạt động giữa 2 máy khác nhau
- Tận dụng hệ thống TCP có sẵn
- Đồng bộ real-time giữa các client

**Nhược điểm:**
- Cần cả Web Server và TCP Server chạy
- Phức tạp hơn HTTP mode

## 📊 LOẠI VÉ VÀ GIÁ CHI TIẾT

Hệ thống hỗ trợ 3 loại vé với giá khác nhau:

| Loại vé | Giá | Ghế | Mô tả |
|---------|-----|-----|-------|
| **Vé vớt** | 25% giá cơ bản | A1, A5, C1, C5 | Ghế góc, giá rẻ nhất |
| **Vé thường** | 100% giá cơ bản | A2, A3, A4, C2, C3, C4 | Ghế tiêu chuẩn |
| **Vé VIP** | 200% giá cơ bản | B1, B2, B3, B4, B5 | Ghế hàng giữa, giá cao nhất |

### Ví dụ tính giá:

**Giả sử giá cơ bản là 100,000 VNĐ:**

- **Vé vớt** (A1): 100,000 × 25% = **25,000 VNĐ**
- **Vé thường** (A2): 100,000 × 100% = **100,000 VNĐ**
- **Vé VIP** (B1): 100,000 × 200% = **200,000 VNĐ**

**Nếu đặt 3 ghế: A1, A2, B1**
- Tổng: 25,000 + 100,000 + 200,000 = **325,000 VNĐ**

### Bố cục ghế:

```
        MÀN HÌNH
    ┌─────────────────┐
    │                 │
A   │ A1 A2 A3 A4 A5  │  A1, A5: Vé vớt (25%)
    │                 │  A2, A3, A4: Vé thường (100%)
B   │ B1 B2 B3 B4 B5  │  B1-B5: Vé VIP (200%)
    │                 │
C   │ C1 C2 C3 C4 C5  │  C1, C5: Vé vớt (25%)
    │                 │  C2, C3, C4: Vé thường (100%)
    └─────────────────┘
```

## 🌐 KẾT NỐI GIỮA 2 MÁY KHÁC NHAU - HƯỚNG DẪN CHI TIẾT

### Trên máy Server:

#### 1. Lấy IP của máy Server:

**Windows:**
```powershell
ipconfig
```

**Tìm dòng:**
```
IPv4 Address. . . . . . . . . . . . : 192.168.1.100
```

**Ghi lại IP này** (ví dụ: `192.168.1.100`)

**Hoặc dùng lệnh nhanh:**
```powershell
ipconfig | findstr IPv4
```

#### 2. Mở Firewall:

**Cách 1: Qua giao diện Windows:**

1. Mở **Windows Defender Firewall with Advanced Security**
2. Click **Inbound Rules** → **New Rule**
3. Chọn **Port** → Next
4. Chọn **TCP** → Nhập port **8888** → Next
5. Chọn **Allow the connection** → Next
6. Check tất cả (Domain, Private, Public) → Next
7. Đặt tên: "Bai4 HTTP Server" → Finish

**Lặp lại cho port 8080** (nếu dùng TCP mode)

**Cách 2: Qua PowerShell (Admin):**
```powershell
# Mở port 8888 (HTTP Server)
New-NetFirewallRule -DisplayName "Bai4 HTTP Server" -Direction Inbound -LocalPort 8888 -Protocol TCP -Action Allow

# Mở port 8080 (TCP Server - nếu dùng TCP mode)
New-NetFirewallRule -DisplayName "Bai4 TCP Server" -Direction Inbound -LocalPort 8080 -Protocol TCP -Action Allow
```

#### 3. Khởi động Server:

```powershell
# Di chuyển đến thư mục dự án
cd "D:\LAB04-NT106-main\LAB04-NT106-main\Bai4"

# Chạy ứng dụng
dotnet run
```

**Trong ứng dụng:**
1. Chọn **"Web Server"** → Click **"Start HTTP Server"**
2. (Nếu dùng TCP mode) Chọn **"TCP Server"** → Click **"Start TCP Server"**

**Kiểm tra:**
- Log hiển thị: "✓ HTTP Server started on port 8888"
- Log hiển thị: "✓ TCP Server started on port 8080" (nếu dùng TCP mode)

### Trên máy Client:

#### 1. Mở trình duyệt:

- Chrome, Edge, Firefox, Safari... (bất kỳ trình duyệt nào)

#### 2. Truy cập URL:

```
http://[IP_SERVER]:8888/booking.html
```

**Ví dụ:**
```
http://192.168.1.100:8888/booking.html
```

**Lưu ý:** Thay `[IP_SERVER]` bằng IP thực của máy Server

#### 3. Nhập thông tin kết nối:

**Trong phần "Kết nối Server":**

- **Server IP**: IP của máy Server (ví dụ: `192.168.1.100`)
  - **KHÔNG dùng** `127.0.0.1` hoặc `localhost`
  - Phải dùng IP thực của máy Server

- **Port**: 
  - `8888` nếu dùng HTTP mode
  - `8080` nếu dùng TCP mode

#### 4. Tự động kết nối:

**Hệ thống sẽ tự động kết nối:**
- Sau 1 giây ngừng gõ
- Khi rời khỏi ô nhập
- Khi nhấn Enter

**Hoặc click nút "Kết nối"** để kết nối thủ công

#### 5. Bắt đầu đặt vé:

- Sau khi kết nối thành công, có thể đặt vé ngay
- Làm theo hướng dẫn ở **BƯỚC 5** ở trên

## ⚠️ LƯU Ý QUAN TRỌNG

### 1. Tự động kết nối:

- ✅ Hệ thống tự động thử kết nối khi nhập IP/Port đúng
- ✅ Không cần click nút "Kết nối" (nhưng vẫn có thể click nếu muốn)
- ✅ Nếu kết nối thất bại, không hiển thị alert (chỉ log vào console)
- ✅ Nhấn Enter sẽ hiển thị alert nếu có lỗi

### 2. Firewall:

- ⚠️ Đảm bảo mở port 8888 (HTTP) và 8080 (TCP) trên máy Server
- ⚠️ Cả 2 máy phải cùng mạng LAN (WiFi/Ethernet)
- ⚠️ Kiểm tra firewall có chặn không

### 3. IP Address:

- ⚠️ Khi kết nối giữa 2 máy, **KHÔNG dùng** `127.0.0.1` hoặc `localhost`
- ⚠️ Phải dùng IP thực của máy Server (ví dụ: `192.168.1.100`)
- ⚠️ IP phải cùng subnet (ví dụ: cả 2 máy đều `192.168.1.x`)

### 4. Đồng bộ:

- ✅ Khi một client đặt vé, các client khác sẽ tự động cập nhật trạng thái ghế
- ✅ Ghế đã đặt sẽ hiển thị màu xám và không thể chọn
- ✅ Cập nhật real-time, không cần refresh trang

### 5. Database:

- ✅ Dữ liệu được lưu trong SQLite database (`cinema_database.db`)
- ✅ Booking thành công sẽ được lưu vào `output_booking.json`
- ✅ Database được tạo tự động khi khởi động Server lần đầu

### 6. Port:

- ⚠️ Server mặc định chạy trên port `8080` (TCP) và `8888` (HTTP)
- ⚠️ Đảm bảo port không bị sử dụng bởi ứng dụng khác
- ⚠️ Kiểm tra port đang mở: `netstat -an | findstr :8888`

## 🔧 TROUBLESHOOTING CHI TIẾT

### ❌ Không tự động kết nối được

**Triệu chứng:**
- Nhập IP/Port nhưng không tự động kết nối
- Trạng thái vẫn hiển thị "Chưa kết nối"

**Kiểm tra:**
1. ✅ IP và Port có đúng không?
2. ✅ Server đã khởi động chưa?
3. ✅ Firewall đã mở port chưa?
4. ✅ Cả 2 máy có cùng mạng không?

**Giải pháp:**
- Thử click nút "Kết nối" thủ công
- Kiểm tra console (F12) để xem lỗi chi tiết
- Ping từ Client đến Server: `ping [IP_SERVER]`
- Kiểm tra log trên Server

### ❌ Kết nối thành công nhưng không load được phim

**Triệu chứng:**
- Kết nối thành công nhưng dropdown "Tên phim" trống
- Không có phim để chọn

**Kiểm tra:**
1. ✅ Database đã được khởi tạo chưa?
2. ✅ Server có dữ liệu phim không?
3. ✅ Log trên Server có lỗi không?

**Giải pháp:**
- Khởi động lại Server để tạo database mới
- Kiểm tra file `cinema_database.db` có tồn tại không
- Xem log trên Server để tìm lỗi

### ❌ Không đặt được vé

**Triệu chứng:**
- Click "Đặt Vé" nhưng không có phản hồi
- Hoặc hiển thị lỗi

**Kiểm tra:**
1. ✅ Đã nhập đầy đủ thông tin chưa? (Tên, Phim, Phòng, Ghế)
2. ✅ Ghế có còn trống không?
3. ✅ Kết nối còn hoạt động không?

**Giải pháp:**
- Kiểm tra lại thông tin đặt vé
- Refresh trang và thử lại
- Kiểm tra trạng thái ghế trước khi đặt
- Xem console (F12) để tìm lỗi

### ❌ Lỗi "Connection refused" hoặc "Timeout"

**Triệu chứng:**
- Không kết nối được đến Server
- Hiển thị lỗi "Connection refused" hoặc "Timeout"

**Kiểm tra:**
1. ✅ Server đã click "Listen" chưa? (cho TCP mode)
2. ✅ IP address có đúng không?
3. ✅ Firewall có chặn port không?
4. ✅ Server có đang chạy không?

**Giải pháp:**
- Đảm bảo Server đang chạy
- Kiểm tra IP bằng `ipconfig`
- Mở Firewall cho port 8888 và 8080
- Test kết nối: `Test-NetConnection -ComputerName [IP_SERVER] -Port 8888`

### ❌ Vé không đồng bộ

**Triệu chứng:**
- Client A đặt vé nhưng Client B không thấy cập nhật
- Ghế vẫn hiển thị trống sau khi đã đặt

**Kiểm tra:**
1. ✅ Server đang chạy không?
2. ✅ Kết nối mạng giữa client và server ổn định không?
3. ✅ Log trên server có lỗi không?

**Giải pháp:**
- Refresh trang trên Client B
- Kiểm tra kết nối mạng
- Xem log trên Server để kiểm tra lỗi
- Đảm bảo cả 2 client đều kết nối đến cùng Server

### ❌ Database lỗi

**Triệu chứng:**
- Lỗi khi truy cập database
- Không thể đặt vé

**Giải pháp:**
- Xóa file `cinema_database.db` và chạy lại server để tạo database mới
- Kiểm tra quyền ghi file trong thư mục chạy ứng dụng
- Đảm bảo không có ứng dụng khác đang sử dụng database

## 📁 CẤU TRÚC FILE

```
Bai4/
├── booking.html              # Trang đặt vé (giao diện chính)
├── booking.js                # Logic đặt vé + tự động kết nối
├── SimpleHttpServer.cs       # HTTP Server implementation
├── WebServerForm.cs          # UI form quản lý Web Server
├── CinemaWebDatabase.cs      # Database helper class
├── output_booking.json       # File lưu booking thành công
├── movies.json               # Dữ liệu phim (từ scraper)
├── cinema_database.db        # SQLite database (tự động tạo)
└── WEB_BOOKING_README.md     # File này
```

## 🎯 TÍNH NĂNG TỔNG HỢP

✅ **Tự động kết nối** - Không cần click nút "Kết nối"  
✅ **Hai chế độ kết nối** - HTTP Mode và TCP Mode  
✅ **Giao diện đẹp** - Modern UI với Tailwind CSS  
✅ **Đồng bộ real-time** - Cập nhật ghế ngay lập tức  
✅ **Tính giá tự động** - Theo loại vé (vớt/thường/VIP)  
✅ **Xác nhận booking** - Popup hiển thị thông tin chi tiết  
✅ **Xử lý lỗi** - Thông báo rõ ràng khi có lỗi  
✅ **Responsive** - Hoạt động tốt trên mọi thiết bị  

## 📖 TÀI LIỆU THAM KHẢO

- **[README.md](README.md)** - Hướng dẫn tổng quan về hệ thống
- **[WEB_SERVER_README.md](WEB_SERVER_README.md)** - Hướng dẫn Web Server
- **[HUONG_DAN_NHANH.md](HUONG_DAN_NHANH.md)** - Hướng dẫn nhanh kết nối 2 máy
- **[REMOTE_ACCESS_README.md](REMOTE_ACCESS_README.md)** - Hướng dẫn truy cập từ xa

## 🆘 HỖ TRỢ

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra phần **TROUBLESHOOTING** ở trên
2. Xem log trên Server để tìm lỗi
3. Kiểm tra console (F12) trên trình duyệt
4. Đảm bảo đã làm theo đúng hướng dẫn

---

**Ngày cập nhật**: 2024  
**Phiên bản**: 2.0 (với tính năng tự động kết nối)  
**Ứng dụng**: Bai4 - Web Booking System với Auto-Connect

