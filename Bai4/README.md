# BÀI 4: QUẢN LÝ PHÒNG VÉ RẠP PHIM

## Ý TƯỞNG CHÍNH

Hệ thống quản lý phòng vé rạp phim được xây dựng với kiến trúc **1 Server - Multi Client**, cho phép nhiều khách hàng đồng thời đặt vé qua giao diện Web hoặc ứng dụng Desktop. Hệ thống sử dụng:

- **TCP/IP Protocol**: Giao tiếp giữa Client và Server qua TCP socket
- **HTTP Server**: Phục vụ giao diện Web và API RESTful
- **SQLite Database**: Lưu trữ dữ liệu phim, phòng chiếu, ghế ngồi và đặt vé
- **Real-time Synchronization**: Đồng bộ trạng thái ghế ngay lập tức giữa tất cả clients khi có người đặt vé

### Kiến trúc hệ thống:

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│   Client 1   │────────▶│             │◀────────│   Client 2   │
│  (Web/App)   │  TCP   │   Server    │  TCP    │  (Web/App)   │
└─────────────┘         │             │         └─────────────┘
                         │  ┌─────────┐│
                         │  │ SQLite  ││
                         │  │Database ││
                         │  └─────────┘│
                         └─────────────┘
                                ▲
                                │ HTTP
                                │
                         ┌─────────────┐
                         │ Web Browser │
                         │  (booking)  │
                         └─────────────┘
```

### Tính năng chính:

1. **Đặt vé trực tuyến**: Khách hàng có thể đặt vé qua Web hoặc ứng dụng Desktop
2. **Tự động kết nối**: Web client tự động kết nối đến server sau khi nhập IP/Port
3. **Đồng bộ real-time**: Khi một client đặt vé, tất cả clients khác được cập nhật ngay lập tức
4. **Tính giá tự động**: Hệ thống tự động tính giá theo loại vé (Vé vớt 25%, Vé thường 100%, Vé VIP 200%)
5. **Kiểm tra ghế trống**: Server kiểm tra và từ chối nếu ghế đã được đặt

---

## CÁC BƯỚC THỰC HIỆN

### 1. Nhận sự kiện người dùng

Hệ thống nhận và xử lý các sự kiện từ người dùng thông qua giao diện Web hoặc Desktop:

#### 1.1. Sự kiện kết nối server

**Trong Web (`booking.js`):**
- **Tự động kết nối**: Khi người dùng nhập IP và Port, hệ thống tự động kết nối sau 1 giây ngừng gõ
- **Kết nối thủ công**: Người dùng có thể nhấn nút "Kết nối" hoặc nhấn Enter
- **Hai chế độ kết nối**:
  - **HTTP Mode**: Khi kết nối localhost → Sử dụng HTTP API trực tiếp
  - **TCP Mode**: Khi kết nối IP khác → Sử dụng TCP qua proxy

```javascript
// Auto-connect khi người dùng nhập IP/Port
serverIpInput.addEventListener('input', triggerAutoConnect);
serverIpInput.addEventListener('blur', () => {
    if (!isConnected) {
        connectToServer(true); // Silent mode
    }
});
```

**Trong Desktop (`Bai4Client.cs`):**
- Người dùng nhập Server IP và Port
- Click nút "Kết nối" để thiết lập TCP connection

#### 1.2. Sự kiện chọn phim và phòng

- **Chọn phim**: Khi người dùng chọn phim từ dropdown → Hệ thống tự động load danh sách phòng chiếu
- **Chọn phòng**: Khi người dùng chọn phòng → Hệ thống load trạng thái ghế của phòng đó

```javascript
document.getElementById('movie-select').addEventListener('change', (e) => {
    selectedMovie = e.target.value;
    if (selectedMovie && isConnected) {
        requestRooms(selectedMovie); // Gửi yêu cầu lấy danh sách phòng
    }
});
```

#### 1.3. Sự kiện chọn ghế

- **Click ghế**: Người dùng click vào ghế để chọn/bỏ chọn
- **Validation**: Kiểm tra ghế có bị đặt chưa trước khi cho phép chọn
- **Cập nhật giá**: Tự động tính lại tổng tiền khi chọn/bỏ chọn ghế

```javascript
function toggleSeat(seatName, seatElement) {
    if (seatBookedStatus[seatName]) {
        alert(`Ghế ${seatName} đã được đặt!`);
        return;
    }
    // Toggle selection và cập nhật UI
    const index = selectedSeats.indexOf(seatName);
    if (index > -1) {
        selectedSeats.splice(index, 1);
    } else {
        selectedSeats.push(seatName);
    }
    updateSummary(); // Cập nhật tổng tiền
}
```

#### 1.4. Sự kiện đặt vé

- **Click nút "Đặt Vé"**: Người dùng nhập đầy đủ thông tin và click đặt vé
- **Validation**: Kiểm tra tất cả thông tin bắt buộc trước khi gửi yêu cầu

```javascript
async function bookTickets() {
    // Kiểm tra validation
    if (!customerName) {
        alert('Vui lòng nhập họ và tên!');
        return;
    }
    if (!selectedMovie || !selectedRoom || selectedSeats.length === 0) {
        alert('Vui lòng chọn đầy đủ thông tin!');
        return;
    }
    // Gửi yêu cầu đặt vé đến server
    const response = await fetch('/api/web/booking', {
        method: 'POST',
        body: JSON.stringify(bookingData)
    });
}
```

---

### 2. Đọc dữ liệu từ web

Hệ thống đọc dữ liệu từ server thông qua HTTP API hoặc TCP Protocol:

#### 2.1. Đọc danh sách phim

**HTTP Mode (`booking.js`):**
```javascript
async function requestMovies() {
    const response = await fetch('/api/web/movies');
    const result = await response.json();
    if (result.success) {
        parseMovies(result.response); // Parse: MOVIES|Tên phim1:Giá1;Tên phim2:Giá2;...
    }
}
```

**TCP Mode (`booking.js`):**
```javascript
async function requestMoviesTcp() {
    const message = 'GET_MOVIES|';
    const response = await fetch('/api/tcp-send', {
        method: 'POST',
        body: JSON.stringify({ message, serverIP, serverPort })
    });
    parseMovies(result.response);
}
```

**Server xử lý (`Bai4Server.cs`):**
```csharp
case "GET_MOVIES":
    var movies = database.GetMovies();
    string response = "MOVIES|" + string.Join(";", 
        movies.Select(m => $"{m.Name}:{m.BasePrice}"));
    SendResponse(clientSocket, response);
    break;
```

#### 2.2. Đọc danh sách phòng chiếu

**Client gửi yêu cầu:**
```javascript
async function requestRooms(movieName) {
    const url = `/api/web/rooms?movie=${encodeURIComponent(movieName)}`;
    const response = await fetch(url);
    const result = await response.json();
    parseRooms(result.response); // Parse: ROOMS|Phòng 1;Phòng 2;...
}
```

**Server trả về (`SimpleHttpServer.cs`):**
```csharp
// GET /api/web/rooms?movie=Tên phim
var rooms = webDatabase.GetRoomsForMovie(movieName);
string response = "ROOMS|" + string.Join(";", rooms);
```

#### 2.3. Đọc trạng thái ghế

**Client gửi yêu cầu:**
```javascript
async function requestSeats(movieName, roomName) {
    const url = `/api/web/seats?movie=${encodeURIComponent(movieName)}&room=${encodeURIComponent(roomName)}`;
    const response = await fetch(url);
    parseSeats(result.response); // Parse: SEATS|A1:Vé thường:0;A2:Vé VIP:1;...
}
```

**Server trả về:**
```csharp
// Format: SEATS|Ghế1:Loại:Trạng thái;Ghế2:Loại:Trạng thái;...
// Trạng thái: 0 = chưa đặt, 1 = đã đặt
var seats = database.GetSeatsForRoom(movieName, roomName);
string response = "SEATS|" + string.Join(";", 
    seats.Select(s => $"{s.SeatName}:{s.SeatType}:{(s.IsBooked ? 1 : 0)}"));
```

#### 2.4. Đọc dữ liệu từ Database

**Database operations (`CinemaDatabase.cs`, `CinemaWebDatabase.cs`):**
- Sử dụng SQLite để lưu trữ và truy vấn dữ liệu
- Các bảng: Movies, Rooms, MovieRooms, Seats, Bookings, BookingSeats
- Sử dụng Entity Framework hoặc ADO.NET để thao tác database

---

### 3. Xác thực dữ liệu

Hệ thống xác thực dữ liệu ở cả phía Client và Server để đảm bảo tính toàn vẹn:

#### 3.1. Xác thực phía Client

**Kiểm tra thông tin đặt vé (`booking.js`):**
```javascript
async function bookTickets() {
    // Kiểm tra tên khách hàng
    if (!customerName || customerName.trim() === '') {
        alert('Vui lòng nhập họ và tên!');
        return;
    }
    
    // Kiểm tra đã chọn phim
    if (!selectedMovie) {
        alert('Vui lòng chọn phim!');
        return;
    }
    
    // Kiểm tra đã chọn phòng
    if (!selectedRoom) {
        alert('Vui lòng chọn phòng!');
        return;
    }
    
    // Kiểm tra đã chọn ít nhất 1 ghế
    if (selectedSeats.length === 0) {
        alert('Vui lòng chọn ít nhất 1 ghế!');
        return;
    }
    
    // Kiểm tra kết nối server
    if (!isConnected) {
        alert('Vui lòng kết nối server trước!');
        return;
    }
}
```

**Kiểm tra định dạng IP/Port:**
```javascript
const triggerAutoConnect = () => {
    const ip = serverIpInput.value.trim();
    const port = serverPortInput.value.trim();
    
    // Validate port
    const portNum = parseInt(port);
    if (isNaN(portNum) || portNum <= 0 || portNum > 65535) {
        return; // Invalid port
    }
    
    // Validate IP format
    const ipPattern = /^(\d{1,3}\.){3}\d{1,3}$|^localhost$/;
    if (!ipPattern.test(ip) && ip !== 'localhost') {
        return; // Invalid IP
    }
};
```

#### 3.2. Xác thực phía Server

**Kiểm tra ghế có trống không (`Bai4Server.cs`):**
```csharp
case "BOOK_SEATS":
    // Parse: BOOK_SEATS|Tên khách hàng|Tên phim|Tên phòng|Ghế1,Ghế2,...|Tổng tiền
    var parts = message.Split('|');
    string customerName = parts[1];
    string movieName = parts[2];
    string roomName = parts[3];
    string[] seatNames = parts[4].Split(',');
    
    // Kiểm tra từng ghế có bị đặt chưa
    foreach (string seatName in seatNames) {
        if (database.IsSeatBooked(movieName, roomName, seatName)) {
            SendResponse(clientSocket, $"BOOK_ERROR|Ghế {seatName} đã được đặt!");
            return;
        }
    }
    
    // Nếu tất cả ghế đều trống → Tiến hành đặt vé
    database.BookSeats(customerName, movieName, roomName, seatNames, totalPrice);
    SendResponse(clientSocket, "BOOK_SUCCESS|");
    
    // Broadcast cập nhật cho tất cả clients
    BroadcastUpdate(roomName, seatNames);
    break;
```

**Kiểm tra dữ liệu đầu vào (`SimpleHttpServer.cs`):**
```csharp
// POST /api/web/booking
var bookingData = JsonSerializer.Deserialize<BookingRequest>(requestBody);
if (string.IsNullOrEmpty(bookingData.CustomerName)) {
    return "{\"success\":false,\"error\":\"Tên khách hàng không được để trống\"}";
}
if (bookingData.SeatNames == null || bookingData.SeatNames.Length == 0) {
    return "{\"success\":false,\"error\":\"Vui lòng chọn ít nhất 1 ghế\"}";
}
```

#### 3.3. Xác thực tính toàn vẹn dữ liệu

- **Kiểm tra trùng lặp**: Server kiểm tra ghế đã được đặt bởi client khác không
- **Transaction**: Sử dụng database transaction để đảm bảo atomicity
- **Lock mechanism**: Đảm bảo không có race condition khi nhiều client cùng đặt ghế

---

### 4. Xử lý dữ liệu

Sau khi xác thực, hệ thống xử lý dữ liệu để thực hiện đặt vé:

#### 4.1. Tính toán giá vé

**Client tính giá (`booking.js`):**
```javascript
function updateSummary() {
    let total = 0;
    selectedSeats.forEach(seatName => {
        const seatType = seatTypes[seatName] || 'Vé thường';
        const multiplier = priceMultipliers[seatType] || 1.0;
        const seatPrice = currentBasePrice * multiplier;
        total += seatPrice;
    });
    document.getElementById('total-price').textContent = 
        total.toLocaleString('vi-VN') + ' VNĐ';
}
```

**Loại vé và hệ số giá:**
- **Vé vớt**: 25% giá cơ bản (Ghế: A1, A5, C1, C5)
- **Vé thường**: 100% giá cơ bản (Ghế: A2, A3, A4, C2, C3, C4)
- **Vé VIP**: 200% giá cơ bản (Ghế: B1, B2, B3, B4, B5)

#### 4.2. Xử lý đặt vé trên Server

**Server xử lý booking (`Bai4Server.cs`, `CinemaDatabase.cs`):**
```csharp
public void BookSeats(string customerName, string movieName, 
    string roomName, string[] seatNames, double totalPrice)
{
    using (var transaction = connection.BeginTransaction())
    {
        try
        {
            // 1. Lấy MovieId và RoomId
            int movieId = GetMovieId(movieName);
            int roomId = GetRoomId(roomName);
            
            // 2. Tạo booking record
            string insertBooking = @"
                INSERT INTO Bookings (CustomerName, MovieId, RoomId, BookingTime, TotalPrice)
                VALUES (@customerName, @movieId, @roomId, @bookingTime, @totalPrice)";
            // Execute và lấy BookingId
            
            // 3. Đánh dấu ghế đã đặt
            foreach (string seatName in seatNames)
            {
                int seatId = GetSeatId(roomId, seatName);
                
                // Update seat status
                string updateSeat = "UPDATE Seats SET IsBooked = 1 WHERE Id = @seatId";
                
                // Link seat to booking
                string insertBookingSeat = @"
                    INSERT INTO BookingSeats (BookingId, SeatId)
                    VALUES (@bookingId, @seatId)";
            }
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

#### 4.3. Đồng bộ dữ liệu giữa các Client

**Broadcast cập nhật (`Bai4Server.cs`):**
```csharp
private void BroadcastUpdate(string roomName, string[] bookedSeats)
{
    string updateMessage = $"UPDATE_SEATS|{roomName}|{string.Join(",", bookedSeats)}";
    
    // Gửi cập nhật cho tất cả clients đang kết nối
    lock (clients)
    {
        foreach (var client in clients)
        {
            try
            {
                client.SendMessage(updateMessage);
            }
            catch
            {
                // Client đã ngắt kết nối, bỏ qua
            }
        }
    }
}
```

**Client nhận và xử lý cập nhật (`booking.js`):**
```javascript
// Trong TCP receive thread hoặc WebSocket
if (message.startsWith('UPDATE_SEATS|')) {
    const parts = message.split('|');
    const roomName = parts[1];
    const bookedSeats = parts[2].split(',');
    
    // Cập nhật trạng thái ghế
    bookedSeats.forEach(seatName => {
        seatBookedStatus[seatName] = true;
    });
    
    // Refresh UI
    updateSeatDisplay();
}
```

#### 4.4. Lưu trữ dữ liệu

- **SQLite Database**: Lưu trữ persistent data (phim, phòng, ghế, booking)
- **JSON File**: Lưu log booking thành công (`output_booking.json`)
- **In-memory State**: Lưu trạng thái kết nối và selection tạm thời

---

### 5. Hiển thị kết quả

Hệ thống hiển thị kết quả cho người dùng thông qua giao diện:

#### 5.1. Hiển thị trạng thái kết nối

**Web (`booking.js`):**
```javascript
function updateConnectionStatus(connected) {
    const status = document.getElementById('connection-status');
    if (connected) {
        status.innerHTML = '<i class="fas fa-circle text-green-500 mr-2"></i>Đã kết nối';
        status.className = 'inline-block px-4 py-2 bg-green-700 rounded';
    } else {
        status.innerHTML = '<i class="fas fa-circle text-red-500 mr-2"></i>Chưa kết nối';
        status.className = 'inline-block px-4 py-2 bg-gray-700 rounded';
    }
}
```

**Desktop (`Bai4Client.cs`):**
- Nút "Kết nối" đổi thành "Ngắt kết nối" (màu đỏ) khi đã kết nối
- ComboBox phim được enable và load dữ liệu

#### 5.2. Hiển thị danh sách phim và phòng

**Parse và hiển thị phim:**
```javascript
function parseMovies(data) {
    // Format: MOVIES|Tên phim1:Giá1;Tên phim2:Giá2;...
    const moviesStr = data.substring(7); // Remove "MOVIES|"
    const movieList = moviesStr.split(';');
    
    const select = document.getElementById('movie-select');
    select.innerHTML = '<option value="">-- Chọn phim --</option>';
    
    movieList.forEach(movie => {
        const [name, price] = movie.split(':');
        const option = document.createElement('option');
        option.value = name;
        option.textContent = `${name} - ${parseFloat(price).toLocaleString('vi-VN')} VNĐ`;
        select.appendChild(option);
    });
}
```

#### 5.3. Hiển thị trạng thái ghế

**Cập nhật UI ghế:**
```javascript
function updateSeatDisplay() {
    const seats = document.querySelectorAll('.seat');
    seats.forEach(seatEl => {
        const seatName = seatEl.dataset.seat;
        const isBooked = seatBookedStatus[seatName] || false;
        const isSelected = selectedSeats.includes(seatName);
        
        seatEl.classList.remove('booked', 'selected');
        
        if (isBooked) {
            seatEl.classList.add('booked'); // Màu xám, disabled
            seatEl.onclick = null; // Không cho click
        } else {
            seatEl.onclick = () => toggleSeat(seatName, seatEl);
        }
        
        if (isSelected) {
            seatEl.classList.add('selected'); // Màu vàng
        }
    });
}
```

**Màu sắc ghế:**
- **Trống**: Màu mặc định (có thể click)
- **Đã chọn**: Màu vàng (highlight)
- **Đã đặt**: Màu xám (disabled, không thể click)

#### 5.4. Hiển thị tổng tiền

**Cập nhật summary:**
```javascript
function updateSummary() {
    document.getElementById('summary-customer').textContent = customerName || '-';
    document.getElementById('summary-movie').textContent = selectedMovie || '-';
    document.getElementById('summary-room').textContent = selectedRoom || '-';
    document.getElementById('summary-seats').textContent = 
        selectedSeats.length > 0 ? selectedSeats.join(', ') : 'Chưa chọn';
    
    // Tính và hiển thị tổng tiền
    let total = 0;
    let priceDetails = [];
    selectedSeats.forEach(seatName => {
        const seatType = seatTypes[seatName] || 'Vé thường';
        const multiplier = priceMultipliers[seatType] || 1.0;
        const seatPrice = currentBasePrice * multiplier;
        total += seatPrice;
        priceDetails.push(`${seatName} (${seatType}): ${seatPrice.toLocaleString('vi-VN')} VNĐ`);
    });
    
    document.getElementById('summary-price-detail').innerHTML = priceDetails.join('<br>') || '-';
    document.getElementById('total-price').textContent = 
        total.toLocaleString('vi-VN') + ' VNĐ';
}
```

#### 5.5. Hiển thị kết quả đặt vé

**Popup thành công (`booking.js`):**
```javascript
function showBookingSuccess(customerName, movie, room, seats, total) {
    const modal = document.getElementById('booking-success');
    const details = document.getElementById('success-details');
    
    // Tạo HTML chi tiết booking
    details.innerHTML = `
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Khách hàng</p>
            <p class="text-xl font-bold">${customerName}</p>
        </div>
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Phim</p>
            <p class="text-xl font-bold">${movie}</p>
        </div>
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Phòng</p>
            <p class="text-xl font-bold">${room}</p>
        </div>
        <div class="border-b border-white/10 pb-4 mb-4">
            <p class="text-gray-400 mb-2">Ghế đã chọn</p>
            <p class="text-xl font-bold text-[#fbbf24]">${seats.join(', ')}</p>
        </div>
        <div class="border-t border-dashed border-white/20 pt-4">
            <div class="flex justify-between items-end">
                <span class="text-gray-400 text-lg">Tổng thanh toán</span>
                <span class="text-3xl font-bold text-[#fbbf24]">${total.toLocaleString('vi-VN')} VNĐ</span>
            </div>
        </div>
    `;
    
    modal.style.display = 'flex'; // Hiển thị modal
}
```

**Thông báo lỗi:**
```javascript
if (result.response && result.response.startsWith('BOOK_ERROR')) {
    const errorMsg = result.response.substring(11);
    alert(`Lỗi đặt vé: ${errorMsg}`);
    // Refresh seat status để cập nhật trạng thái mới nhất
    await requestSeats(selectedMovie, selectedRoom);
}
```

#### 5.6. Cập nhật real-time

**Khi nhận UPDATE_SEATS:**
```javascript
// Client tự động refresh trạng thái ghế khi nhận broadcast
if (message.startsWith('UPDATE_SEATS|')) {
    const parts = message.split('|');
    const roomName = parts[1];
    const bookedSeats = parts[2].split(',');
    
    // Nếu đang xem phòng này → Cập nhật ngay
    if (selectedRoom === roomName) {
        bookedSeats.forEach(seatName => {
            seatBookedStatus[seatName] = true;
        });
        updateSeatDisplay(); // Refresh UI ngay lập tức
    }
}
```

---

## TÓM TẮT LUỒNG XỬ LÝ

```
1. Người dùng nhập IP/Port
   ↓
2. Hệ thống tự động kết nối (sau 1 giây)
   ↓
3. Load danh sách phim từ server
   ↓
4. Người dùng chọn phim → Load phòng chiếu
   ↓
5. Người dùng chọn phòng → Load trạng thái ghế
   ↓
6. Người dùng chọn ghế → Tính tổng tiền
   ↓
7. Người dùng click "Đặt Vé"
   ↓
8. Client xác thực dữ liệu
   ↓
9. Gửi yêu cầu đến Server
   ↓
10. Server xác thực và xử lý đặt vé
    ↓
11. Server broadcast cập nhật cho tất cả clients
    ↓
12. Client hiển thị kết quả (thành công/lỗi)
    ↓
13. Tự động refresh trạng thái ghế
```

---

## CẤU TRÚC FILE

```
Bai4/
├── Program.cs                  # Entry point với menu
├── Bai4Server.cs               # TCP Server implementation
├── Bai4Client.cs               # TCP Client implementation
├── SimpleHttpServer.cs         # HTTP Server implementation
├── WebServerForm.cs            # Web Server UI form
├── CinemaDatabase.cs           # Database helper (TCP mode)
├── CinemaWebDatabase.cs        # Database helper (Web mode)
├── booking.html                # Trang đặt vé Web
├── booking.js                  # Logic đặt vé + auto-connect
├── movies.json                 # Dữ liệu phim
├── cinema_database.db          # SQLite database (TCP)
├── cinema_dataweb.db           # SQLite database (Web)
└── README.md                   # File này
```

---

## HƯỚNG DẪN SỬ DỤNG

### Khởi động Server

1. Chạy: `dotnet run` trong thư mục `Bai4`
2. Chọn **"TCP Server"** → Click **"Listen"** (cho TCP mode)
3. Hoặc chọn **"Web Server"** → Click **"Start HTTP Server"** (cho Web mode)

### Sử dụng Web Client

1. Mở trình duyệt: `http://localhost:8888/booking.html`
2. Nhập Server IP và Port
3. Hệ thống tự động kết nối sau 1 giây
4. Chọn phim, phòng, ghế và đặt vé

### Sử dụng Desktop Client

1. Chạy: `dotnet run` → Chọn **"TCP Client"**
2. Nhập Server IP và Port → Click **"Kết nối"**
3. Chọn phim, phòng, ghế và đặt vé

---

**Ngày tạo**: 2024  
**Phiên bản**: 2.0 (với Web Booking System và Auto-Connect)  
**Ứng dụng**: Bai4 - Quản lý phòng vé rạp phim
