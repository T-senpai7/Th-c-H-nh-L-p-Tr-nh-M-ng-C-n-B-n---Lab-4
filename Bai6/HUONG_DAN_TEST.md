# HƯỚNG DẪN TEST VÀ TEST CASES - BÀI 6

## 📋 MỤC LỤC

1. [Hướng dẫn test từng bước](#hướng-dẫn-test-từng-bước)
2. [Test Cases - Lấy thông tin thành công](#test-cases---lấy-thông-tin-thành-công)
3. [Test Cases - Lấy thông tin thất bại](#test-cases---lấy-thông-tin-thất-bại)
4. [Test Cases - Validation Errors](#test-cases---validation-errors)
5. [Test Cases - Network Errors](#test-cases---network-errors)
6. [Test Cases - Edge Cases](#test-cases---edge-cases)
7. [Checklist kiểm tra](#checklist-kiểm-tra)

---

## 🚀 HƯỚNG DẪN TEST TỪNG BƯỚC

### Bước 1: Chuẩn bị môi trường

1. **Kiểm tra kết nối Internet**
   ```powershell
   ping nt106.uitiot.vn
   ```
   Kết quả mong đợi: Có response từ server

2. **Lấy Access Token từ Bài 5**
   - Chạy ứng dụng Bai05
   - Đăng nhập thành công
   - Copy Access Token từ kết quả

3. **Build và chạy ứng dụng Bai6**
   ```powershell
   cd Bai6
   dotnet build
   dotnet run
   ```

4. **Kiểm tra giao diện hiển thị đúng**
   - ✅ Form có tiêu đề "Bai6"
   - ✅ Có 3 trường input: URL, Token Type, Access Token
   - ✅ Có nút "GET USER INFO"
   - ✅ Có vùng hiển thị kết quả (RichTextBox)

### Bước 2: Test từng test case

Với mỗi test case dưới đây:
1. Điền input data vào các trường tương ứng
2. Click nút "GET USER INFO"
3. Quan sát kết quả trong vùng hiển thị
4. So sánh với Expected Output
5. Đánh dấu ✅ nếu đúng, ❌ nếu sai

---

## ✅ TEST CASES - LẤY THÔNG TIN THÀNH CÔNG

### Test Case 1: Lấy thông tin user với token hợp lệ

**Mục đích**: Kiểm tra lấy thông tin user thành công với token hợp lệ từ Bài 5

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: [token từ Bài 5]
```

**Các bước thực hiện**:
1. Mở ứng dụng Bai6
2. Kiểm tra URL đã được điền sẵn: `https://nt106.uitiot.vn/api/v1/user/me`
3. Kiểm tra Token Type đã được điền sẵn: `Bearer`
4. Paste Access Token từ Bài 5 vào trường Access Token
5. Click nút "GET USER INFO"
6. Chờ response (thường 1-3 giây)

**Expected Output**:
```
THÔNG TIN NGƯỜI DÙNG:
========================

ID: 1
Username: phatpt
Email: phatpt@example.com
Họ và tên: [Tên đầy đủ]
Số điện thoại: [Số điện thoại]
Địa chỉ: [Địa chỉ]
Trạng thái: Hoạt động

========================
JSON RESPONSE (ĐẦY ĐỦ):
========================
{
  "id": 1,
  "username": "phatpt",
  "email": "phatpt@example.com",
  ...
}
```

**Kiểm tra**:
- ✅ Nút GET USER INFO bị disable trong khi xử lý
- ✅ Hiển thị "Đang xử lý..." trước khi có kết quả
- ✅ Có phần "THÔNG TIN NGƯỜI DÙNG" với các trường thông tin
- ✅ Có phần "JSON RESPONSE (ĐẦY ĐỦ)" ở cuối
- ✅ Thông tin user được format dễ đọc
- ✅ Nút GET USER INFO được enable lại sau khi xong

**Lưu ý**: 
- Thông tin user sẽ khác nhau tùy theo tài khoản
- Một số trường có thể null hoặc không có trong response

---

### Test Case 2: Lấy thông tin với token mới

**Mục đích**: Kiểm tra với token mới vừa lấy từ Bài 5

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: [token mới nhất từ Bài 5]
```

**Expected Output**: Tương tự Test Case 1

**Lưu ý**: Token mới sẽ luôn hoạt động, token cũ có thể đã hết hạn

---

## ❌ TEST CASES - LẤY THÔNG TIN THẤT BẠI

### Test Case 3: Token hết hạn

**Mục đích**: Kiểm tra xử lý khi token đã hết hạn

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: [token cũ đã hết hạn]
```

**Expected Output**:
```
Detail: Not authenticated
Status Code: 401 Unauthorized
```

Hoặc:
```
Detail: Token expired
Status Code: 401 Unauthorized
```

**Kiểm tra**:
- ✅ Hiển thị thông báo lỗi trong trường "detail"
- ✅ Hiển thị status code 401
- ✅ Không có thông tin user
- ✅ Không có JSON response

---

### Test Case 4: Token không hợp lệ

**Mục đích**: Kiểm tra xử lý khi token không đúng format

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: invalid-token-12345
```

**Expected Output**:
```
Detail: Not authenticated
Status Code: 401 Unauthorized
```

**Kiểm tra**: Tương tự Test Case 3

---

### Test Case 5: Token bị cắt (không đầy đủ)

**Mục đích**: Kiểm tra xử lý khi token bị thiếu một phần

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InBoYXRwdCIsImV4cCI6MTcxMzYyMTA0N30
```

**Lưu ý**: Token bị thiếu phần signature (phần cuối)

**Expected Output**:
```
Detail: Not authenticated
Status Code: 401 Unauthorized
```

---

### Test Case 6: Token rỗng

**Mục đích**: Kiểm tra xử lý khi token rỗng (có thể bị chặn ở client validation)

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: [để trống]
```

**Expected Output** (nếu vượt qua validation):
```
Detail: Not authenticated
Status Code: 401 Unauthorized
```

Hoặc MessageBox: "Vui lòng nhập Access Token!" (nếu bị chặn ở client)

---

## ⚠️ TEST CASES - VALIDATION ERRORS

### Test Case 7: URL rỗng

**Mục đích**: Kiểm tra validation khi URL trống

**Input Data**:
```
URL: [Xóa hết, để trống]
Token Type: Bearer
Access Token: [token hợp lệ]
```

**Expected Output**:
- Hiển thị MessageBox với nội dung: "Vui lòng nhập URL!"
- Không gửi request
- Vùng kết quả không thay đổi hoặc vẫn trống

**Kiểm tra**:
- ✅ MessageBox hiển thị đúng
- ✅ Nút GET USER INFO vẫn enable
- ✅ Không có request được gửi đi

---

### Test Case 8: Token Type rỗng

**Mục đích**: Kiểm tra validation khi Token Type trống

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: [Xóa hết, để trống]
Access Token: [token hợp lệ]
```

**Expected Output**:
- Hiển thị MessageBox: "Vui lòng nhập Token Type!"
- Không gửi request

---

### Test Case 9: Access Token rỗng

**Mục đích**: Kiểm tra validation khi Access Token trống

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: [Xóa hết, để trống]
```

**Expected Output**:
- Hiển thị MessageBox: "Vui lòng nhập Access Token!"
- Không gửi request

---

### Test Case 10: URL không hợp lệ

**Mục đích**: Kiểm tra xử lý khi URL không đúng format

**Input Data**:
```
URL: not-a-valid-url
Token Type: Bearer
Access Token: [token hợp lệ]
```

**Expected Output**:
```
Lỗi kết nối: [Thông báo lỗi về URL không hợp lệ]
Chi tiết: [Chi tiết lỗi nếu có]
```

**Kiểm tra**:
- ✅ Hiển thị lỗi kết nối
- ✅ Không crash ứng dụng

---

### Test Case 11: URL sai domain

**Mục đích**: Kiểm tra xử lý khi URL không tồn tại

**Input Data**:
```
URL: https://invalid-domain-12345.com/api/v1/user/me
Token Type: Bearer
Access Token: [token hợp lệ]
```

**Expected Output**:
```
Lỗi kết nối: [Thông báo lỗi về không thể resolve domain]
Chi tiết: [Chi tiết lỗi DNS]
```

---

## 🌐 TEST CASES - NETWORK ERRORS

### Test Case 12: Mất kết nối Internet

**Mục đích**: Kiểm tra xử lý khi không có Internet

**Các bước**:
1. Tắt WiFi/Ethernet hoặc ngắt kết nối Internet
2. Nhập input data hợp lệ
3. Click GET USER INFO

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: [token hợp lệ]
```

**Expected Output**:
```
Lỗi kết nối: [Thông báo lỗi về không thể kết nối]
Chi tiết: [Chi tiết lỗi network]
```

**Kiểm tra**:
- ✅ Ứng dụng không crash
- ✅ Hiển thị thông báo lỗi rõ ràng
- ✅ Nút GET USER INFO được enable lại

---

### Test Case 13: Server không phản hồi (Timeout)

**Mục đích**: Kiểm tra xử lý khi server quá lâu không phản hồi

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: [token hợp lệ]
```

**Expected Output** (nếu timeout):
```
Lỗi kết nối: [Thông báo về timeout]
Chi tiết: [Chi tiết lỗi]
```

**Lưu ý**: Test case này khó reproduce, nhưng nếu server chậm, có thể xảy ra.

---

## 🔍 TEST CASES - EDGE CASES

### Test Case 14: URL có khoảng trắng thừa

**Mục đích**: Kiểm tra xử lý khi URL có khoảng trắng ở đầu/cuối

**Input Data**:
```
URL:   https://nt106.uitiot.vn/api/v1/user/me   
Token Type: Bearer
Access Token: [token hợp lệ]
```

**Expected Output**: 
- Nếu có Trim(): Hoạt động bình thường như Test Case 1
- Nếu không có Trim(): Có thể lỗi kết nối

**Kiểm tra**: Code đã có `.Trim()` nên phải hoạt động bình thường.

---

### Test Case 15: Token Type có khoảng trắng thừa

**Mục đích**: Kiểm tra xử lý khi Token Type có khoảng trắng

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type:   Bearer   
Access Token: [token hợp lệ]
```

**Expected Output**: 
- Nếu có Trim(): Hoạt động bình thường
- Nếu không có Trim(): Có thể lỗi authentication

---

### Test Case 16: Access Token có khoảng trắng thừa

**Mục đích**: Kiểm tra xử lý khi Access Token có khoảng trắng

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token:   [token hợp lệ]   
```

**Expected Output**: 
- Nếu có Trim(): Hoạt động bình thường
- Nếu không có Trim(): Có thể lỗi authentication

---

### Test Case 17: Token Type khác "Bearer"

**Mục đích**: Kiểm tra xử lý khi Token Type không phải "Bearer"

**Input Data**:
```
URL: https://nt106.uitiot.vn/api/v1/user/me
Token Type: Basic
Access Token: [token hợp lệ]
```

**Expected Output**: 
- Có thể lỗi authentication (tùy vào server có hỗ trợ không)
- Hoặc hoạt động bình thường nếu server chấp nhận

---

### Test Case 18: URL với HTTP thay vì HTTPS

**Mục đích**: Kiểm tra xử lý khi dùng HTTP

**Input Data**:
```
URL: http://nt106.uitiot.vn/api/v1/user/me
Token Type: Bearer
Access Token: [token hợp lệ]
```

**Expected Output**: 
- Có thể redirect hoặc lỗi
- Tùy vào cấu hình server

---

### Test Case 19: Click GET USER INFO nhiều lần liên tiếp

**Mục đích**: Kiểm tra xử lý khi click nút nhiều lần

**Các bước**:
1. Nhập input data hợp lệ
2. Click GET USER INFO nhiều lần liên tiếp (5-10 lần) trước khi có response

**Expected Output**:
- ✅ Nút GET USER INFO bị disable ngay sau lần click đầu tiên
- ✅ Chỉ gửi 1 request (không duplicate)
- ✅ Kết quả hiển thị đúng 1 lần

**Kiểm tra**:
- ✅ Không có duplicate requests
- ✅ Không có race condition

---

### Test Case 20: Thay đổi input trong khi đang xử lý

**Mục đích**: Kiểm tra xử lý khi thay đổi input trong lúc request đang chạy

**Các bước**:
1. Nhập input data hợp lệ
2. Click GET USER INFO
3. Ngay lập tức thay đổi Access Token
4. Chờ response

**Expected Output**:
- Request vẫn sử dụng dữ liệu tại thời điểm click GET USER INFO
- Kết quả hiển thị đúng với dữ liệu đã gửi

---

## 📊 CHECKLIST KIỂM TRA

### Chức năng cơ bản
- [ ] Test Case 1: Lấy thông tin user thành công với token hợp lệ
- [ ] Test Case 3: Xử lý token hết hạn
- [ ] Test Case 4: Xử lý token không hợp lệ

### Validation
- [ ] Test Case 7: Validation URL rỗng
- [ ] Test Case 8: Validation Token Type rỗng
- [ ] Test Case 9: Validation Access Token rỗng
- [ ] Test Case 14: Trim khoảng trắng trong URL
- [ ] Test Case 15: Trim khoảng trắng trong Token Type
- [ ] Test Case 16: Trim khoảng trắng trong Access Token

### Xử lý lỗi
- [ ] Test Case 10: URL không hợp lệ
- [ ] Test Case 11: URL sai domain
- [ ] Test Case 12: Mất kết nối Internet

### Edge Cases
- [ ] Test Case 17: Token Type khác "Bearer"
- [ ] Test Case 19: Click GET USER INFO nhiều lần
- [ ] Test Case 20: Thay đổi input trong khi xử lý

### Giao diện
- [ ] Form hiển thị đúng layout
- [ ] Nút GET USER INFO disable khi đang xử lý
- [ ] Hiển thị "Đang xử lý..." khi request đang chạy
- [ ] Kết quả hiển thị đúng format (thông tin user + JSON)
- [ ] Nút GET USER INFO enable lại sau khi xong

---

## 📝 GHI CHÚ KHI TEST

### 1. Thứ tự test được khuyến nghị:
1. **Test validation trước** (Test Case 7-9) - Đảm bảo không gửi request không hợp lệ
2. **Test thành công** (Test Case 1) - Đảm bảo flow chính hoạt động
3. **Test thất bại** (Test Case 3-5) - Đảm bảo xử lý lỗi đúng
4. **Test edge cases** - Kiểm tra các trường hợp đặc biệt

### 2. Lưu ý về Access Token:
- Token cần được lấy từ Bài 5 (HTTP POST Login)
- Token có thời gian hết hạn (thường 24 giờ hoặc theo cấu hình server)
- Token cần được copy đầy đủ (không bị cắt)
- Format token: `header.payload.signature` (3 phần ngăn cách bởi dấu chấm)

### 3. Lưu ý về Response:
- Response sẽ chứa thông tin user dưới dạng JSON
- Một số trường có thể null hoặc không có
- Thông tin user sẽ khác nhau tùy theo tài khoản

### 4. Debugging:
- Nếu test case fail, kiểm tra:
  - Console output (nếu có)
  - Network tab trong browser DevTools (nếu test qua browser)
  - Response từ server
  - Exception trong code
  - Token có hợp lệ không (thử lại với token mới từ Bài 5)

---

## 🎯 KẾT QUẢ MONG ĐỢI TỔNG THỂ

Sau khi test tất cả các test cases, ứng dụng phải:
- ✅ Xử lý đúng tất cả các trường hợp thành công
- ✅ Xử lý đúng tất cả các trường hợp thất bại
- ✅ Validation đầy đủ input data
- ✅ Hiển thị thông báo lỗi rõ ràng
- ✅ Không crash trong bất kỳ trường hợp nào
- ✅ UI responsive và user-friendly
- ✅ Xử lý async đúng cách (không block UI)
- ✅ Hiển thị thông tin user một cách có định dạng

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề khi test:
1. Kiểm tra kết nối Internet
2. Kiểm tra API endpoint có hoạt động không: https://nt106.uitiot.vn/docs
3. Kiểm tra token có hợp lệ không (lấy token mới từ Bài 5)
4. Kiểm tra log/console để xem chi tiết lỗi
5. Thử lại với token mới

---

**Lưu ý**: Một số test cases có thể không reproduce được trong môi trường thực tế (ví dụ: timeout, server down). Trong trường hợp đó, có thể bỏ qua hoặc test trong môi trường giả lập.

