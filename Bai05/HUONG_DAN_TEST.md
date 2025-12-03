# HƯỚNG DẪN TEST VÀ TEST CASES - BÀI 5

## 📋 MỤC LỤC

1. [Hướng dẫn test từng bước](#hướng-dẫn-test-từng-bước)
2. [Test Cases - Đăng nhập thành công](#test-cases---đăng-nhập-thành-công)
3. [Test Cases - Đăng nhập thất bại](#test-cases---đăng-nhập-thất-bại)
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

2. **Build và chạy ứng dụng**
   ```powershell
   cd Bai05
   dotnet build
   dotnet run
   ```

3. **Kiểm tra giao diện hiển thị đúng**
   - ✅ Form có tiêu đề "Bai5"
   - ✅ Có 3 trường input: URL, Username, Password
   - ✅ Có nút "LOGIN"
   - ✅ Có vùng hiển thị kết quả (RichTextBox)

### Bước 2: Test từng test case

Với mỗi test case dưới đây:
1. Điền input data vào các trường tương ứng
2. Click nút "LOGIN"
3. Quan sát kết quả trong vùng hiển thị
4. So sánh với Expected Output
5. Đánh dấu ✅ nếu đúng, ❌ nếu sai

---

## ✅ TEST CASES - ĐĂNG NHẬP THÀNH CÔNG

### Test Case 1: Đăng nhập với tài khoản hợp lệ (phatpt)

**Mục đích**: Kiểm tra đăng nhập thành công với tài khoản mặc định

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: phatpt
Password: [password của phatpt] 123456 
```

**Các bước thực hiện**:
1. Mở ứng dụng
2. Kiểm tra URL đã được điền sẵn: `https://nt106.uitiot.vn/auth/token`
3. Kiểm tra Username đã được điền sẵn: `phatpt`
4. Nhập Password (nếu chưa có)
5. Click nút "LOGIN"
6. Chờ response (thường 1-3 giây)

**Expected Output**:
```
Bearer
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InBoYXRwdCIsImV4cCI6MTcxMzYyMTA0N30.re7JotDf35TM83qpLxVlbiAZIBv1esy_92Ye-xXXgDY

Đăng nhập thành công
```

**Kiểm tra**:
- ✅ Nút LOGIN bị disable trong khi xử lý
- ✅ Hiển thị "Đang xử lý..." trước khi có kết quả
- ✅ Có dòng "Bearer"
- ✅ Có access token (chuỗi JWT dài)
- ✅ Có dòng "Đăng nhập thành công"
- ✅ Nút LOGIN được enable lại sau khi xong

**Lưu ý**: Access token sẽ khác nhau mỗi lần đăng nhập, nhưng format phải giống nhau (3 phần ngăn cách bởi dấu chấm)

---

### Test Case 2: Đăng nhập với tài khoản khác hợp lệ

**Mục đích**: Kiểm tra đăng nhập với tài khoản khác (nếu có)

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: [username hợp lệ khác] nguyen long
Password: [password tương ứng] 123@123aA 
```

**Expected Output**: Tương tự Test Case 1

---

## ❌ TEST CASES - ĐĂNG NHẬP THẤT BẠI

### Test Case 3: Sai password

**Mục đích**: Kiểm tra xử lý khi nhập sai password

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: phatpt
Password: wrongpassword
```

**Expected Output**:
```
Detail: Incorrect username or password
Status Code: 401 Unauthorized
```

**Kiểm tra**:
- ✅ Hiển thị thông báo lỗi trong trường "detail"
- ✅ Hiển thị status code 401
- ✅ Không có access token
- ✅ Không có thông báo "Đăng nhập thành công"

---

### Test Case 4: Sai username

**Mục đích**: Kiểm tra xử lý khi nhập sai username

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: invaliduser
Password: 123456
```

**Expected Output**:
```
Detail: Incorrect username or password
Status Code: 401 Unauthorized
```

**Kiểm tra**: Tương tự Test Case 3

---

### Test Case 5: Username và password đều sai

**Mục đích**: Kiểm tra xử lý khi cả username và password đều sai

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: fakeuser123
Password: fakepass456
```

**Expected Output**:
```
Detail: Incorrect username or password
Status Code: 401 Unauthorized
```

---

### Test Case 6: Username hoặc password rỗng (từ phía server)

**Mục đích**: Kiểm tra xử lý khi server trả về lỗi validation

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: 
Password: 
```

**Lưu ý**: Test case này có thể bị chặn ở client-side validation trước, nhưng nếu vượt qua, server sẽ trả về lỗi.

**Expected Output** (nếu vượt qua validation):
```
Detail: [Thông báo lỗi từ server về thiếu username/password]
Status Code: 422 Unprocessable Entity
```

---

## ⚠️ TEST CASES - VALIDATION ERRORS

### Test Case 7: URL rỗng

**Mục đích**: Kiểm tra validation khi URL trống

**Input Data**:
```
URL: [Xóa hết, để trống]
Username: phatpt
Password: [password]
```

**Expected Output**:
- Hiển thị MessageBox với nội dung: "Vui lòng nhập URL!"
- Không gửi request
- Vùng kết quả không thay đổi hoặc vẫn trống

**Kiểm tra**:
- ✅ MessageBox hiển thị đúng
- ✅ Nút LOGIN vẫn enable
- ✅ Không có request được gửi đi

---

### Test Case 8: Username rỗng

**Mục đích**: Kiểm tra validation khi Username trống

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: [Xóa hết, để trống]
Password: [password]
```

**Expected Output**:
- Hiển thị MessageBox: "Vui lòng nhập Username!"
- Không gửi request

---

### Test Case 9: Password rỗng

**Mục đích**: Kiểm tra validation khi Password trống

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: phatpt
Password: [Xóa hết, để trống]
```

**Expected Output**:
- Hiển thị MessageBox: "Vui lòng nhập Password!"
- Không gửi request

---

### Test Case 10: URL không hợp lệ

**Mục đích**: Kiểm tra xử lý khi URL không đúng format

**Input Data**:
```
URL: not-a-valid-url
Username: phatpt
Password: [password]
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
URL: https://invalid-domain-12345.com/auth/token
Username: phatpt
Password: [password]
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
3. Click LOGIN

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: phatpt
Password: [password]
```

**Expected Output**:
```
Lỗi kết nối: [Thông báo lỗi về không thể kết nối]
Chi tiết: [Chi tiết lỗi network]
```

**Kiểm tra**:
- ✅ Ứng dụng không crash
- ✅ Hiển thị thông báo lỗi rõ ràng
- ✅ Nút LOGIN được enable lại

---

### Test Case 13: Server không phản hồi (Timeout)

**Mục đích**: Kiểm tra xử lý khi server quá lâu không phản hồi

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: phatpt
Password: [password]
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
URL:   https://nt106.uitiot.vn/auth/token   
Username: phatpt
Password: [password]
```

**Expected Output**: 
- Nếu có Trim(): Hoạt động bình thường như Test Case 1
- Nếu không có Trim(): Có thể lỗi kết nối

**Kiểm tra**: Code đã có `.Trim()` nên phải hoạt động bình thường.

---

### Test Case 15: Username có khoảng trắng thừa

**Mục đích**: Kiểm tra xử lý khi Username có khoảng trắng

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username:   phatpt   
Password: [password]
```

**Expected Output**: 
- Nếu có Trim(): Hoạt động bình thường
- Nếu không có Trim(): Có thể đăng nhập thất bại

---

### Test Case 16: Password có ký tự đặc biệt

**Mục đích**: Kiểm tra xử lý password có ký tự đặc biệt

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: phatpt
Password: P@ssw0rd!@#$%^&*()
```

**Expected Output**: 
- Nếu password đúng: Đăng nhập thành công
- Nếu password sai: Đăng nhập thất bại (Test Case 3)

---

### Test Case 17: Password rất dài

**Mục đích**: Kiểm tra xử lý password dài

**Input Data**:
```
URL: https://nt106.uitiot.vn/auth/token
Username: phatpt
Password: [password dài 100+ ký tự]
```

**Expected Output**: 
- Tùy vào validation của server
- Nếu hợp lệ: Đăng nhập thành công
- Nếu không hợp lệ: Đăng nhập thất bại

---

### Test Case 18: URL với HTTP thay vì HTTPS

**Mục đích**: Kiểm tra xử lý khi dùng HTTP

**Input Data**:
```
URL: http://nt106.uitiot.vn/auth/token
Username: phatpt
Password: [password]
```

**Expected Output**: 
- Có thể redirect hoặc lỗi
- Tùy vào cấu hình server

---

### Test Case 19: Click LOGIN nhiều lần liên tiếp

**Mục đích**: Kiểm tra xử lý khi click nút nhiều lần

**Các bước**:
1. Nhập input data hợp lệ
2. Click LOGIN nhiều lần liên tiếp (5-10 lần) trước khi có response

**Expected Output**:
- ✅ Nút LOGIN bị disable ngay sau lần click đầu tiên
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
2. Click LOGIN
3. Ngay lập tức thay đổi Username hoặc Password
4. Chờ response

**Expected Output**:
- Request vẫn sử dụng dữ liệu tại thời điểm click LOGIN
- Kết quả hiển thị đúng với dữ liệu đã gửi

---

## 📊 CHECKLIST KIỂM TRA

### Chức năng cơ bản
- [ ] Test Case 1: Đăng nhập thành công với tài khoản hợp lệ
- [ ] Test Case 3: Đăng nhập thất bại với password sai
- [ ] Test Case 4: Đăng nhập thất bại với username sai

### Validation
- [ ] Test Case 7: Validation URL rỗng
- [ ] Test Case 8: Validation Username rỗng
- [ ] Test Case 9: Validation Password rỗng
- [ ] Test Case 14: Trim khoảng trắng trong URL
- [ ] Test Case 15: Trim khoảng trắng trong Username

### Xử lý lỗi
- [ ] Test Case 10: URL không hợp lệ
- [ ] Test Case 11: URL sai domain
- [ ] Test Case 12: Mất kết nối Internet

### Edge Cases
- [ ] Test Case 16: Password có ký tự đặc biệt
- [ ] Test Case 19: Click LOGIN nhiều lần
- [ ] Test Case 20: Thay đổi input trong khi xử lý

### Giao diện
- [ ] Form hiển thị đúng layout
- [ ] Nút LOGIN disable khi đang xử lý
- [ ] Hiển thị "Đang xử lý..." khi request đang chạy
- [ ] Kết quả hiển thị đúng format
- [ ] Nút LOGIN enable lại sau khi xong

---

## 📝 GHI CHÚ KHI TEST

### 1. Thứ tự test được khuyến nghị:
1. **Test validation trước** (Test Case 7-9) - Đảm bảo không gửi request không hợp lệ
2. **Test thành công** (Test Case 1) - Đảm bảo flow chính hoạt động
3. **Test thất bại** (Test Case 3-5) - Đảm bảo xử lý lỗi đúng
4. **Test edge cases** - Kiểm tra các trường hợp đặc biệt

### 2. Lưu ý về Access Token:
- Access token là JWT token, có format: `header.payload.signature`
- Token sẽ khác nhau mỗi lần đăng nhập thành công
- Token có thể có thời gian hết hạn (expiration time)

### 3. Lưu ý về Password:
- Password trong các test case là ví dụ
- Cần thay bằng password thực tế của tài khoản
- Không commit password thực vào code

### 4. Debugging:
- Nếu test case fail, kiểm tra:
  - Console output (nếu có)
  - Network tab trong browser DevTools (nếu test qua browser)
  - Response từ server
  - Exception trong code

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

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề khi test:
1. Kiểm tra kết nối Internet
2. Kiểm tra API endpoint có hoạt động không: https://nt106.uitiot.vn/docs
3. Kiểm tra log/console để xem chi tiết lỗi
4. Thử lại với input data khác

---

**Lưu ý**: Một số test cases có thể không reproduce được trong môi trường thực tế (ví dụ: timeout, server down). Trong trường hợp đó, có thể bỏ qua hoặc test trong môi trường giả lập.

