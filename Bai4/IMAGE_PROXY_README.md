# Image Proxy - Hướng dẫn sử dụng

## Tổng quan

Hệ thống proxy hình ảnh giúp bypass CORS và hiển thị hình ảnh từ `betacorp.vn` trên trang `Viewing.html`.

## Có 2 cách sử dụng:

### Cách 1: Tích hợp vào Server C# (Khuyến nghị)

Server C# đã được tích hợp endpoint `/api/proxy-image`. Không cần chạy server riêng.

**Cách sử dụng:**
1. Chạy server C# như bình thường (port 8888)
2. Trong `Viewing.html`, đảm bảo `PROXY_CONFIG.useStandaloneProxy = false`
3. Hình ảnh sẽ tự động được proxy qua endpoint `/api/proxy-image`

**Endpoint:**
```
GET /api/proxy-image?url=<encoded-image-url>
```

### Cách 2: Server Node.js riêng

Nếu muốn dùng server Node.js riêng (ví dụ: để test hoặc development):

**Cài đặt:**
```bash
npm install express node-fetch
```

**Chạy server:**
```bash
node image-proxy.js
```

Server sẽ chạy trên port 3001.

**Cấu hình Viewing.html:**
```javascript
const PROXY_CONFIG = {
    useStandaloneProxy: true, // Set true để dùng Node.js server
    standaloneProxyUrl: 'http://localhost:3001',
    integratedProxyUrl: '/api/proxy-image'
};
```

## Cách hoạt động

1. **Viewing.html** gọi `getProxiedImageUrl(originalUrl)` để tạo proxy URL
2. URL được decode nếu bị encode (`%2f` → `/`)
3. Proxy server fetch hình ảnh từ `betacorp.vn` với headers phù hợp
4. Proxy server trả về hình ảnh với CORS headers cho phép

## API Endpoint

### GET /api/proxy-image

**Query Parameters:**
- `url` (required): URL của hình ảnh (có thể đã encode)

**Response:**
- Success: Image binary với Content-Type phù hợp
- Error: JSON error message

**Ví dụ:**
```
GET /api/proxy-image?url=https%3A%2F%2Ffiles.betacorp.vn%2Fmedia%2Fimages%2F...
```

## Troubleshooting

### Lỗi CORS
- Đảm bảo proxy server đang chạy
- Kiểm tra URL proxy trong browser console

### Hình ảnh không hiển thị
- Kiểm tra console để xem lỗi
- Thử decode URL thủ công
- Kiểm tra network tab để xem request có thành công không

### Server C# không proxy được
- Kiểm tra `using System.Net.Http;` đã được thêm
- Kiểm tra HttpClient có hoạt động không
- Xem console logs để debug

## Notes

- Proxy server cache hình ảnh 1 ngày (max-age=86400)
- Timeout: 10 giây
- Hỗ trợ decode URL nhiều lần nếu cần
- Tự động fallback về placeholder nếu proxy thất bại

