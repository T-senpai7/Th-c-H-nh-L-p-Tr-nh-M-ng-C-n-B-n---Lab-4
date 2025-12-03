# Cập nhật Movie Scraper và Viewing.html

## Các thay đổi chính

### 1. MovieScraper.cs - Crawl dữ liệu từ betacinemas.vn

#### Thông tin cần crawl:
- ✅ **Tên phim** (title)
- ✅ **Hình ảnh** (poster) - với URL đầy đủ
- ✅ **Mô tả phim** (description) 
- ✅ **Giá vé** (basePrice)
- ✅ **Link chi tiết** (detailUrl) - để mở detail page
- ✅ **Thời lượng** (duration)
- ✅ **Thể loại** (genre)

#### Cải thiện:
- Crawl từ trang chủ: https://betacinemas.vn/phim.htm
- Tìm 3 phim mới nhất (3 phim đầu tiên trong danh sách)
- Lọc bỏ các item không phải phim (như "TRAILER", "LỊCH CHIẾU", etc.)
- Tự động fetch detail page để lấy thông tin đầy đủ hơn
- Xử lý URL hình ảnh (absolute/relative)
- Parse giá vé từ text

### 2. Viewing.html - Hiển thị phim từ API

#### Tính năng mới:
- ✅ **Tự động load** danh sách phim từ API `/api/movies`
- ✅ **Hiển thị hình ảnh** phim từ dữ liệu crawl
- ✅ **Hiển thị mô tả** phim ngắn gọn
- ✅ **Hiển thị giá vé** theo định dạng VNĐ
- ✅ **Link đến detail page** khi click vào tên phim
- ✅ **Chọn phim** để đặt vé

#### Cải thiện UI:
- Loading spinner khi đang tải
- Error message nếu load thất bại
- Format giá tiền theo VNĐ (ví dụ: 85.000 VNĐ)
- Hiển thị thời lượng và thể loại
- Link mở tab mới khi click vào tên phim

### 3. Cấu trúc dữ liệu JSON

```json
{
  "title": "Tên phim",
  "poster": "https://betacinemas.vn/path/to/poster.jpg",
  "description": "Mô tả phim...",
  "duration": "117 phút",
  "genre": "Kinh dị",
  "basePrice": 85000,
  "detailUrl": "https://betacinemas.vn/chi-tiet-phim.htm?gf=..."
}
```

## Cách sử dụng

1. **Crawl phim:**
   - Mở Web Server form
   - Click "Crawl Movies from BetaCinemas"
   - Đợi quá trình crawl hoàn tất
   - Phim sẽ được lưu vào `movies.json`

2. **Xem phim trên web:**
   - Start HTTP Server
   - Mở http://localhost:8888/Viewing.html
   - Danh sách phim sẽ tự động load từ API
   - Click vào tên phim để xem detail page

3. **Chọn phim để đặt vé:**
   - Click vào card phim để chọn
   - Chọn ngày, giờ chiếu
   - Chọn ghế
   - Xem tổng tiền và đặt vé

## Lưu ý

- Scraper sẽ tự động lọc bỏ các item không phải phim
- Nếu crawl thất bại, sẽ dùng dữ liệu mẫu
- Hình ảnh sẽ tự động xử lý nếu URL là relative
- Detail page sẽ được fetch để lấy thông tin đầy đủ hơn

