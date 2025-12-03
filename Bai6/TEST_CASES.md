# TEST CASES - BÀI 6: HTTP GET USER INFO

## 📋 TỔNG QUAN

File này chứa danh sách đầy đủ các test cases với input data cụ thể và expected output. Sử dụng file này để:
- Theo dõi tiến độ test
- Ghi lại kết quả thực tế
- So sánh với expected output

**Cách sử dụng**: Đánh dấu ✅ nếu PASS, ❌ nếu FAIL, ⏸️ nếu SKIP

**Lưu ý**: Cần có Access Token hợp lệ từ Bài 5 để test các trường hợp thành công.

---

## ✅ TEST CASES - THÀNH CÔNG

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-01 | Lấy thông tin user với token hợp lệ | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `[token từ Bài 5]` | ```THÔNG TIN NGƯỜI DÙNG:<br>========================<br>ID: [id]<br>Username: [username]<br>Email: [email]<br>...<br><br>JSON RESPONSE (ĐẦY ĐỦ):<br>...``` | ⬜ | Cần token hợp lệ từ Bài 5 |
| TC-02 | Lấy thông tin với token mới | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `[token mới nhất]` | Tương tự TC-01 | ⬜ | Token mới sẽ luôn hoạt động |

---

## ❌ TEST CASES - THẤT BẠI

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-03 | Token hết hạn | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `[token cũ đã hết hạn]` | ```Detail: Not authenticated<br>Status Code: 401 Unauthorized``` | ⬜ | |
| TC-04 | Token không hợp lệ | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `invalid-token-12345` | ```Detail: Not authenticated<br>Status Code: 401 Unauthorized``` | ⬜ | |
| TC-05 | Token bị cắt (thiếu phần signature) | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InBoYXRwdCIsImV4cCI6MTcxMzYyMTA0N30` | ```Detail: Not authenticated<br>Status Code: 401 Unauthorized``` | ⬜ | Token thiếu phần cuối |
| TC-06 | Token rỗng | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `` (rỗng) | **MessageBox**: "Vui lòng nhập Access Token!"<br>Hoặc: ```Detail: Not authenticated<br>Status Code: 401``` | ⬜ | Có thể bị chặn ở client validation |

---

## ⚠️ TEST CASES - VALIDATION ERRORS

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-07 | URL rỗng | **URL**: `` (rỗng)<br>**Token Type**: `Bearer`<br>**Access Token**: `[token hợp lệ]` | **MessageBox**: "Vui lòng nhập URL!"<br>Không gửi request | ⬜ | |
| TC-08 | Token Type rỗng | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `` (rỗng)<br>**Access Token**: `[token hợp lệ]` | **MessageBox**: "Vui lòng nhập Token Type!"<br>Không gửi request | ⬜ | |
| TC-09 | Access Token rỗng | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `` (rỗng) | **MessageBox**: "Vui lòng nhập Access Token!"<br>Không gửi request | ⬜ | |
| TC-10 | URL không hợp lệ | **URL**: `not-a-valid-url`<br>**Token Type**: `Bearer`<br>**Access Token**: `[token hợp lệ]` | ```Lỗi kết nối: [Thông báo lỗi]<br>Chi tiết: [Chi tiết]``` | ⬜ | |
| TC-11 | URL sai domain | **URL**: `https://invalid-domain-12345.com/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `[token hợp lệ]` | ```Lỗi kết nối: [Thông báo lỗi DNS]<br>Chi tiết: [Chi tiết]``` | ⬜ | |

---

## 🌐 TEST CASES - NETWORK ERRORS

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-12 | Mất kết nối Internet | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `[token hợp lệ]`<br>**Bước**: Tắt WiFi/Ethernet trước khi click | ```Lỗi kết nối: [Thông báo lỗi]<br>Chi tiết: [Chi tiết]``` | ⬜ | Cần tắt Internet trước |
| TC-13 | Server timeout | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `[token hợp lệ]` | ```Lỗi kết nối: [Thông báo timeout]<br>Chi tiết: [Chi tiết]``` | ⬜ | Khó reproduce |

---

## 🔍 TEST CASES - EDGE CASES

| # | Test Case | Input Data | Expected Output | Status | Notes |
|---|-----------|------------|-----------------|--------|-------|
| TC-14 | URL có khoảng trắng thừa | **URL**: `   https://nt106.uitiot.vn/api/v1/user/me   `<br>**Token Type**: `Bearer`<br>**Access Token**: `[token hợp lệ]` | Lấy thông tin thành công (do có Trim()) | ⬜ | Kiểm tra Trim() hoạt động |
| TC-15 | Token Type có khoảng trắng thừa | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `   Bearer   `<br>**Access Token**: `[token hợp lệ]` | Lấy thông tin thành công (do có Trim()) | ⬜ | Kiểm tra Trim() hoạt động |
| TC-16 | Access Token có khoảng trắng thừa | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `   [token hợp lệ]   ` | Lấy thông tin thành công (do có Trim()) | ⬜ | Kiểm tra Trim() hoạt động |
| TC-17 | Token Type khác "Bearer" | **URL**: `https://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Basic`<br>**Access Token**: `[token hợp lệ]` | Tùy vào server (có thể lỗi hoặc thành công) | ⬜ | |
| TC-18 | URL với HTTP | **URL**: `http://nt106.uitiot.vn/api/v1/user/me`<br>**Token Type**: `Bearer`<br>**Access Token**: `[token hợp lệ]` | Tùy vào cấu hình server | ⬜ | Có thể redirect hoặc lỗi |
| TC-19 | Click GET USER INFO nhiều lần | **Input**: Dữ liệu hợp lệ<br>**Action**: Click 5-10 lần liên tiếp | Chỉ gửi 1 request<br>Nút disable ngay | ⬜ | Kiểm tra không duplicate |
| TC-20 | Thay đổi input khi đang xử lý | **Input**: Dữ liệu hợp lệ<br>**Action**: Click → Thay đổi Token ngay | Request dùng dữ liệu cũ<br>Kết quả đúng | ⬜ | Kiểm tra không bị ảnh hưởng |

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

**Access Token từ Bài 5**:
- Chạy ứng dụng Bai05
- Đăng nhập thành công
- Copy Access Token từ kết quả
- Format: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InBoYXRwdCIsImV4cCI6MTcxMzYyMTA0N30.re7JotDf35TM83qpLxVlbiAZIBv1esy_92Ye-xXXgDY`

**Lưu ý**: 
- Token sẽ khác nhau mỗi lần đăng nhập
- Token có thời gian hết hạn
- Cần copy đầy đủ token (không bị cắt)

### Expected Output Format

**Thành công**:
```
THÔNG TIN NGƯỜI DÙNG:
========================
ID: [id]
Username: [username]
Email: [email]
...

========================
JSON RESPONSE (ĐẦY ĐỦ):
========================
{
  "id": 1,
  "username": "...",
  ...
}
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
3. Token có hợp lệ không? (thử lại với token mới)
4. Screenshot (nếu cần)
5. Điều kiện môi trường (OS, .NET version, etc.)

---

## 🔄 TEMPLATE GHI KẾT QUẢ CHI TIẾT

### Test Case: [TC-XX]

**Ngày test**: [DD/MM/YYYY]
**Người test**: [Tên]
**Môi trường**: 
- OS: [Windows 10/11]
- .NET Version: [8.0.x]
- Kết nối Internet: [Có/Không]
- Token từ: [Bài 5 / Token cũ / Token test]

**Input Data**:
```
URL: [giá trị]
Token Type: [giá trị]
Access Token: [giá trị - có thể ẩn một phần]
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
- [Token có hợp lệ không?]
- [Có lỗi gì đặc biệt không?]

---

## 🔗 LIÊN KẾT VỚI BÀI 5

Bài 6 phụ thuộc vào Bài 5:
- **Bài 5**: Đăng nhập và lấy Access Token
- **Bài 6**: Sử dụng Access Token để lấy thông tin user

**Workflow test**:
1. Chạy Bai05 → Đăng nhập → Lấy Access Token
2. Copy Access Token
3. Chạy Bai6 → Paste Token → Test các test cases

**Lưu ý**: 
- Token từ Bài 5 có thể dùng để test nhiều test cases trong Bài 6
- Nếu token hết hạn, cần lấy token mới từ Bài 5

---

## 📞 LIÊN HỆ

Nếu có vấn đề khi test, tham khảo:
- File `HUONG_DAN_TEST.md` để xem hướng dẫn chi tiết
- File `README.md` để xem hướng dẫn sử dụng tổng quát
- Bài 5 để lấy token mới nếu token cũ hết hạn

---

**Cập nhật lần cuối**: [Ngày cập nhật]

