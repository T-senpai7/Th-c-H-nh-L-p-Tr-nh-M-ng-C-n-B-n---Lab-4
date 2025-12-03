# Các sửa đổi để khắc phục lỗi 404

## Vấn đề
Khi mở localhost:8888/Viewing.html, server trả về lỗi 404 Not Found.

## Nguyên nhân
1. Web server không tìm được file Viewing.html vì `baseDirectory` không trỏ đúng thư mục Bai4
2. Khi chạy từ bin/Debug/net8.0-windows, server cần tìm ngược lên thư mục Bai4

## Giải pháp đã áp dụng

### 1. Cải thiện tìm kiếm thư mục (WebServerForm.cs)
- Tìm Viewing.html từ thư mục thực thi, đi lên các thư mục cha tối đa 5 cấp
- Nếu không tìm thấy, thử thư mục hiện tại
- Thêm logging chi tiết để debug

### 2. Copy file vào output directory (Bai4.csproj)
- Thêm cấu hình để copy Viewing.html, index.html, app.js, movies.json vào thư mục output
- Đảm bảo file luôn có sẵn khi server chạy

### 3. Cải thiện error handling (SimpleHttpServer.cs)
- Thêm thông tin chi tiết trong response 404
- Log đường dẫn tìm kiếm để debug

## Cách sử dụng

1. **Build lại project** để copy file vào output directory:
   ```
   dotnet build
   ```

2. **Chạy ứng dụng** và chọn "Web Server"

3. **Click "Start HTTP Server"** - server sẽ:
   - Tìm Viewing.html trong thư mục Bai4
   - Hiển thị log cho biết file nào đã tìm thấy
   - Khởi động server trên port 8888

4. **Mở browser** tại:
   - http://localhost:8888/Viewing.html
   - hoặc http://localhost:8888/ (sẽ tự động chuyển đến Viewing.html)

## Kiểm tra

Nếu vẫn gặp lỗi 404:
1. Kiểm tra log trong WebServerForm để xem base directory
2. Đảm bảo Viewing.html có trong thư mục Bai4
3. Kiểm tra file đã được copy vào bin/Debug/net8.0-windows sau khi build

## Files đã sửa
- `Bai4/WebServerForm.cs` - Cải thiện tìm kiếm thư mục
- `Bai4/SimpleHttpServer.cs` - Thêm error logging
- `Bai4/Bai4.csproj` - Thêm copy files vào output

