# Kiến trúc Kết nối - Booking System

## Tổng quan

Hệ thống có 2 cơ chế kết nối độc lập:

### 1. HTTP Web Server (SimpleHttpServer) - Dùng cho booking.html
- **File:** `SimpleHttpServer.cs`
- **Port:** 8888 (mặc định)
- **Database:** `CinemaWebDatabase` → `cinema_dataweb.db`
- **API Endpoints:**
  - `GET /api/web/movies` - Lấy danh sách phim
  - `GET /api/web/rooms?movie=<tên phim>` - Lấy danh sách phòng cho phim
  - `GET /api/web/seats?movie=<tên phim>&room=<tên phòng>` - Lấy trạng thái ghế
  - `POST /api/web/booking` - Đặt vé

### 2. TCP Server (Bai4Server) - Dùng cho Bai4Client
- **File:** `Bai4Server.cs`
- **Port:** 8080
- **Database:** `CinemaDatabase` → `cinema_database.db`
- **Protocol:** TCP Socket với giao thức riêng

## Tại sao booking.html không cần Bai4Server.cs?

`booking.html` kết nối trực tiếp qua HTTP API đến `SimpleHttpServer`, không cần TCP server:

```javascript
// booking.js
const response = await fetch('/api/web/movies');  // Gọi trực tiếp HTTP API
```

Flow kết nối:
```
booking.html (JavaScript)
    ↓ HTTP Request
SimpleHttpServer.cs (port 8888)
    ↓ Gọi methods
CinemaWebDatabase.cs
    ↓ SQLite
cinema_dataweb.db
```

## Database Mapping

| Component | Database Class | Database File |
|-----------|---------------|---------------|
| **booking.html** → SimpleHttpServer | `CinemaWebDatabase` | `cinema_dataweb.db` ✅ |
| Bai4Client → Bai4Server | `CinemaDatabase` | `cinema_database.db` |

**✅ Xác nhận:** Data từ `booking.html` đã đổ về đúng `cinema_dataweb.db`

## Kết nối từ máy khác

### Cách 1: Phục vụ HTML từ máy server (Khuyến nghị)

1. **Trên máy server:**
   - Chạy `WebServerForm` (từ menu chương trình)
   - Server sẽ lắng nghe trên port 8888 (tất cả network interfaces)
   - Truy cập: `http://<IP-máy-server>:8888/booking.html`

2. **Từ máy client:**
   - Mở browser, truy cập: `http://<IP-máy-server>:8888/booking.html`
   - HTML sẽ tự động kết nối đến server của máy đó

**Ưu điểm:**
- Không cần cấu hình gì thêm
- Tự động kết nối đến database của máy server
- HTML và JS được serve từ cùng server

### Cách 2: Sửa booking.js để kết nối đến server từ xa

Nếu bạn muốn phục vụ HTML từ một nơi khác nhưng vẫn kết nối đến database server:

1. Sửa `booking.js` để sử dụng connection panel (hiện tại chưa dùng):
```javascript
// Thay đổi từ:
const response = await fetch('/api/web/movies');

// Thành:
const serverIP = document.getElementById('server-ip').value;
const serverPort = document.getElementById('server-port').value;
const response = await fetch(`http://${serverIP}:${serverPort}/api/web/movies`);
```

**Lưu ý:** Cần xử lý CORS nếu HTML và API server khác domain/port.

## Cấu hình Firewall

Để kết nối từ máy khác, đảm bảo:

1. **Windows Firewall:**
   - Cho phép port 8888 qua firewall
   - Hoặc tắt firewall tạm thời khi test

2. **Router/Network:**
   - Đảm bảo không có firewall block port 8888

## Xác minh Database

Để kiểm tra database nào đang được dùng:

1. **Kiểm tra code:**
   - `CinemaWebDatabase.cs` dòng 15: `private const string DB_NAME = "cinema_dataweb.db";`
   - `SimpleHttpServer.cs` dòng 30: `webDatabase = new CinemaWebDatabase();`

2. **Kiểm tra file:**
   - Tìm file `cinema_dataweb.db` trong thư mục chạy chương trình
   - File này chứa dữ liệu từ booking.html

## Tóm tắt

✅ **booking.html KHÔNG cần Bai4Server.cs** - Đúng như bạn đã nhận thấy

✅ **Database đã đúng:** `cinema_dataweb.db` - Không cần sửa gì

✅ **SimpleHttpServer đã được sửa:** Lắng nghe trên tất cả interface để hỗ trợ kết nối từ máy khác

🔧 **Để kết nối từ máy khác:**
- Chạy WebServerForm trên máy server
- Truy cập `http://<IP-server>:8888/booking.html` từ máy client

