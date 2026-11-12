# API và quy tắc Tuần 06

## Danh sách API

| Method | Endpoint | Kết quả chính |
| --- | --- | --- |
| GET | `/api/customers` | Xem danh sách Customer |
| GET | `/api/customers/{id}` | Xem chi tiết Customer |
| POST | `/api/customers` | Tạo Customer |
| PUT | `/api/customers/{id}` | Cập nhật Customer |
| GET | `/api/contracts/{id}` | Xem chi tiết Contract |
| POST | `/api/contracts` | Tạo Contract ở trạng thái `Draft` |
| PUT | `/api/contracts/{id}` | Cập nhật Contract hợp lệ theo CTR-01 |
| POST | `/api/contracts/{id}/submit` | Gửi Contract trình duyệt |
| GET | `/api/price-lists` | Xem danh sách PriceList |
| POST | `/api/price-lists` | Tạo PriceList |
| GET | `/api/price-lists/{id}/items` | Xem PriceItem của PriceList |
| POST | `/api/price-lists/{id}/items` | Thêm PriceItem vào PriceList |

## Request/response mẫu theo nhóm

Tạo Customer:

```http
POST /api/customers
Content-Type: application/json

{ "name": "Công ty Demo", "status": "Active" }
```

Phản hồi `201 Created` gồm `id`, `name`, `status`. Tên là bắt buộc; status chỉ nhận `Active` hoặc `Inactive`.

Tạo Contract Draft:

```http
POST /api/contracts
Content-Type: application/json

{
  "contractNo": "HD-DEMO-06",
  "customerId": 3,
  "startDate": "2026-11-01",
  "endDate": "2027-10-31",
  "content": "",
  "documentName": ""
}
```

Phản hồi `201 Created` luôn có status `Draft`. Customer phải tồn tại và `Active`; ngày bắt đầu không được sau ngày kết thúc.

Cập nhật Tuần 07: payload tạo/cập nhật Contract chỉ nhận các trường khai báo trong `ContractRequestDto`. Nếu gửi thêm `status` (ví dụ `"status": "Approved"`), API trả `400 Bad Request` với `errors` ngay khi đọc JSON, trước khi gọi service. PUT bị từ chối không thay đổi các trường của Contract. Xem minh chứng TC07 trong [week07-testing.md](week07-testing.md).

Tạo PriceItem:

```http
POST /api/price-lists/1/items
Content-Type: application/json

{ "serviceCode": "SVC003", "unitPrice": 150000 }
```

Phản hồi `201 Created`. PriceList phải tồn tại, đơn giá phải lớn hơn 0 và mã dịch vụ không được trùng trong cùng PriceList.

## Mẫu request/response Submit

Request:

```http
POST /api/contracts/2/submit
```

Thành công - `200 OK`:

```json
{
  "contractId": 2,
  "status": "PendingApproval",
  "message": "Contract submitted for approval."
}
```

Thiếu nội dung hoặc tệp bắt buộc - `400 Bad Request`:

```json
{
  "code": "CTR-02",
  "errors": [
    "Contract content is required.",
    "A required document is missing."
  ]
}
```

Sai trạng thái hoặc gửi lặp - `409 Conflict`. Không tìm thấy Contract - `404 Not Found`.

## Giả định dùng trong demo

Phase 1 chưa xác định chính xác danh sách nội dung và tệp bắt buộc theo từng loại hợp đồng. Bản demo dùng hai điều kiện đơn giản:

- `Content` không được để trống.
- `DocumentName` không được để trống.

Đây chỉ là giả định để chứng minh CTR-02, không được xem là yêu cầu nghiệp vụ chính thức. Danh sách thực tế vẫn cần được xác nhận.

## Đối chiếu US03 và quy tắc

| Yêu cầu | Đoạn xử lý tương ứng |
| --- | --- |
| Contract phải tồn tại | `ContractService.SubmitAsync` trả `404` khi repository không tìm thấy dữ liệu. |
| CTR-01: chỉ sửa ở Draft hoặc RevisionRequested | `ContractService.UpdateAsync` kiểm tra status trước khi cập nhật; status khác trả `409`. |
| CTR-02: Customer hợp lệ | `ContractService.SubmitAsync` yêu cầu Customer có status `Active`. |
| CTR-02: ngày hiệu lực hợp lệ | `ContractService.ValidateDates` và `SubmitAsync` yêu cầu ngày không rỗng, `StartDate <= EndDate`. |
| CTR-02: đủ nội dung/tệp bắt buộc | `ContractService.SubmitAsync` kiểm tra `Content` và `DocumentName`; thiếu dữ liệu trả `400` trước khi lưu. |
| CTR-03: không chuyển Draft thẳng Approved | API tạo luôn dùng `Draft`; API cập nhật không nhận status; Submit chỉ đặt `PendingApproval`. |
| Gửi lặp | Sau lần thành công, status là `PendingApproval`; lần Submit tiếp theo trả `409` và không tạo dữ liệu mới. |
| Thất bại không đổi Contract | Các kiểm tra hoàn tất trước khi đổi status và gọi `SaveChangesAsync`. |

## Mô phỏng Approval và giới hạn

Phiên bản này chưa có Approval entity hay Approval workflow. Việc gửi được mô phỏng bằng thay đổi trạng thái Contract từ `Draft` sang `PendingApproval`. Vì vậy response không có `approvalId`, không xác định người duyệt kế tiếp và không ghi lịch sử phê duyệt.

Authentication/Authorization cũng chưa được yêu cầu xây dựng trong phạm vi kỹ thuật Tuần 06, nên AC01 về quyền chỉ được ghi nhận là giới hạn, không mô phỏng bằng header giả.
