# TEST CASES - BÀI 5: HTTP POST LOGIN

## 📋 TỔNG QUAN

File này chứa danh sách đầy đủ các test cases với input data cụ thể và expected output. Sử dụng file này để:
- Theo dõi tiến độ test
- Ghi lại kết quả thực tế
- So sánh với expected output

**Cách sử dụng**: Đánh dấu ✅ nếu PASS, ❌ nếu FAIL, ⏸️ nếu SKIP

---

## ✅ TEST CASES - THÀNH CÔNG

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-01 | Đăng nhập với tài khoản hợp lệ (phatpt) | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `phatpt`<br>**Password**: `[password của phatpt]` | ```Bearer<br>[access_token]<br><br>Đăng nhập thành công``` | ⬜ | Access token sẽ khác mỗi lần |
| TC-02 | Đăng nhập với tài khoản khác hợp lệ | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `[username hợp lệ]`<br>**Password**: `[password tương ứng]` | ```Bearer<br>[access_token]<br><br>Đăng nhập thành công``` | ⬜ | Thay bằng tài khoản thực tế |

---

## ❌ TEST CASES - THẤT BẠI

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-03 | Sai password | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `phatpt`<br>**Password**: `wrongpassword` | ```Detail: Incorrect username or password<br>Status Code: 401 Unauthorized``` | ⬜ | |
| TC-04 | Sai username | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `invaliduser`<br>**Password**: `123456` | ```Detail: Incorrect username or password<br>Status Code: 401 Unauthorized``` | ⬜ | |
| TC-05 | Username và password đều sai | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `fakeuser123`<br>**Password**: `fakepass456` | ```Detail: Incorrect username or password<br>Status Code: 401 Unauthorized``` | ⬜ | |
| TC-06 | Username rỗng (server validation) | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `` (rỗng)<br>**Password**: `123456` | ```Detail: [Thông báo lỗi từ server]<br>Status Code: 422 Unprocessable Entity``` | ⬜ | Có thể bị chặn ở client validation |

---

## ⚠️ TEST CASES - VALIDATION ERRORS

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-07 | URL rỗng | **URL**: `` (rỗng)<br>**Username**: `phatpt`<br>**Password**: `[password]` | **MessageBox**: "Vui lòng nhập URL!"<br>Không gửi request | ⬜ | |
| TC-08 | Username rỗng | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `` (rỗng)<br>**Password**: `[password]` | **MessageBox**: "Vui lòng nhập Username!"<br>Không gửi request | ⬜ | |
| TC-09 | Password rỗng | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `phatpt`<br>**Password**: `` (rỗng) | **MessageBox**: "Vui lòng nhập Password!"<br>Không gửi request | ⬜ | |
| TC-10 | URL không hợp lệ | **URL**: `not-a-valid-url`<br>**Username**: `phatpt`<br>**Password**: `[password]` | ```Lỗi kết nối: [Thông báo lỗi]<br>Chi tiết: [Chi tiết]``` | ⬜ | |
| TC-11 | URL sai domain | **URL**: `https://invalid-domain-12345.com/auth/token`<br>**Username**: `phatpt`<br>**Password**: `[password]` | ```Lỗi kết nối: [Thông báo lỗi DNS]<br>Chi tiết: [Chi tiết]``` | ⬜ | |

---

## 🌐 TEST CASES - NETWORK ERRORS

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-12 | Mất kết nối Internet | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `phatpt`<br>**Password**: `[password]`<br>**Bước**: Tắt WiFi/Ethernet trước khi click LOGIN | ```Lỗi kết nối: [Thông báo lỗi]<br>Chi tiết: [Chi tiết]``` | ⬜ | Cần tắt Internet trước |
| TC-13 | Server timeout | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `phatpt`<br>**Password**: `[password]` | ```Lỗi kết nối: [Thông báo timeout]<br>Chi tiết: [Chi tiết]``` | ⬜ | Khó reproduce |

---

## 🔍 TEST CASES - EDGE CASES

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-14 | URL có khoảng trắng thừa | **URL**: `   https://nt106.uitiot.vn/auth/token   `<br>**Username**: `phatpt`<br>**Password**: `[password]` | Đăng nhập thành công (do có Trim()) | ⬜ | Kiểm tra Trim() hoạt động |
| TC-15 | Username có khoảng trắng thừa | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `   phatpt   `<br>**Password**: `[password]` | Đăng nhập thành công (do có Trim()) | ⬜ | Kiểm tra Trim() hoạt động |
| TC-16 | Password có ký tự đặc biệt | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `phatpt`<br>**Password**: `P@ssw0rd!@#$%^&*()` | Tùy vào password thực tế | ⬜ | Nếu đúng: thành công<br>Nếu sai: thất bại |
| TC-17 | Password rất dài | **URL**: `https://nt106.uitiot.vn/auth/token`<br>**Username**: `phatpt`<br>**Password**: `[100+ ký tự]` | Tùy vào validation server | ⬜ | |
| TC-18 | URL với HTTP | **URL**: `http://nt106.uitiot.vn/auth/token`<br>**Username**: `phatpt`<br>**Password**: `[password]` | Tùy vào cấu hình server | ⬜ | Có thể redirect hoặc lỗi |
| TC-19 | Click LOGIN nhiều lần | **Input**: Dữ liệu hợp lệ<br>**Action**: Click LOGIN 5-10 lần liên tiếp | Chỉ gửi 1 request<br>Nút disable ngay | ⬜ | Kiểm tra không duplicate |
| TC-20 | Thay đổi input khi đang xử lý | **Input**: Dữ liệu hợp lệ<br>**Action**: Click LOGIN → Thay đổi Username ngay | Request dùng dữ liệu cũ<br>Kết quả đúng | ⬜ | Kiểm tra không bị ảnh hưởng |

---

## 📊 BẢNG TỔNG HỢP KẾT QUẢ

### Tổng số test cases: 20

| Loại | Số lượng | Pass | Fail | Skip | Tỷ lệ Pass |
|------|----------|------|------|------|------------|
| Thành công | 2 | ⬜ | ⬜ | ⬜ | - |
| Thất bại | 4 | ⬜ | ⬜ | ⬜ | - |
| Validation | 5 | ⬜ | ⬜ | ⬜ | - |
| Network Errors | 2 | ⬜ | ⬜ | ⬜ | - |
| Edge Cases | 7 | ⬜ | ⬜ | ⬜ | - |
| **TỔNG** | **20** | **⬜** | **⬜** | **⬜** | **- %** |

---

## 📝 GHI CHÚ KHI TEST

### Input Data Mẫu

**Tài khoản test (nếu có)**:
- Username: `phatpt`
- Password: `[cần điền password thực tế]`

**Lưu ý**: 
- Không commit password thực vào code
- Thay đổi password trong test cases bằng password thực tế của bạn

### Expected Output Format

**Thành công**:
```
Bearer
[access_token_jwt]
[blank line]
Đăng nhập thành công
```

**Thất bại**:
```
Detail: [thông báo lỗi]
Status Code: [mã lỗi] [tên lỗi]
```

**Lỗi kết nối**:
```
Lỗi kết nối: [thông báo]
Chi tiết: [chi tiết nếu có]
```

### Cách đánh dấu kết quả

- ✅ **PASS**: Kết quả thực tế khớp với expected output
- ❌ **FAIL**: Kết quả thực tế không khớp hoặc có lỗi
- ⏸️ **SKIP**: Bỏ qua test case (không thể test hoặc không cần thiết)
- ⬜ **PENDING**: Chưa test

### Ghi chú khi Fail

Nếu test case FAIL, ghi lại:
1. Kết quả thực tế là gì?
2. Lỗi cụ thể (nếu có)
3. Screenshot (nếu cần)
4. Điều kiện môi trường (OS, .NET version, etc.)

---

## 🔄 TEMPLATE GHI KẾT QUẢ CHI TIẾT

### Test Case: [TC-XX]

**Ngày test**: [DD/MM/YYYY]
**Người test**: [Tên]
**Môi trường**: 
- OS: [Windows 10/11]
- .NET Version: [8.0.x]
- Kết nối Internet: [Có/Không]

**Input Data**:
```
URL: [giá trị]
Username: [giá trị]
Password: [giá trị]
```

**Expected Output**:
```
[expected output]
```

**Actual Output**:
```
[actual output]
```

**Status**: ✅ PASS / ❌ FAIL / ⏸️ SKIP

**Notes**:
- [Ghi chú nếu có]

---

## 📞 LIÊN HỆ

Nếu có vấn đề khi test, tham khảo:
- File `HUONG_DAN_TEST.md` để xem hướng dẫn chi tiết
- File `README.md` để xem hướng dẫn sử dụng tổng quát

---

**Cập nhật lần cuối**: [Ngày cập nhật]

