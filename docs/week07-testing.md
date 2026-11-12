# Kiểm thử Tuần 07

Ngày 12/11/2026, chạy local `dotnet test QTKD.sln --logger "trx;LogFileName=week07.trx" --results-directory docs/test-evidence`: **36/36 PASS**, gồm **20 Unit + 16 Integration**, 0 thất bại, 0 bỏ qua. Số liệu lấy từ [TRX](test-evidence/week07.trx), không phải kết quả dự kiến. Cách cấu hình và chạy lại nằm trong [README](../README.md).

## TC01–TC08: kiểm thử Integration tự động

Các tên dưới đây là method thật trong [WorkflowTests.cs](../tests/QTKD.Api.Tests/Integration/WorkflowTests.cs). Một Theory có thể tạo nhiều case, nên tám TC không đồng nghĩa tám lần chạy.

| TC | Loại | Tên test | HTTP, body và dữ liệu được kiểm tra | Kết quả |
| --- | --- | --- | --- | --- |
| TC01 | Tự động (Integration) | `TC01_CreateCustomer_ReturnsLocationBodyGetAndDatabase` | POST Customer hợp lệ → 201; Location/id/name/status đúng; GET 200 và DB khớp. | PASS (1) |
| TC02 | Tự động (Integration) | `TC02_InvalidCustomer_ReturnsValidationErrorsAndNoRow` | Name/Status thiếu, null hoặc whitespace → 400; `errors` đúng trường; số Customer không tăng. | PASS (6) |
| TC03 | Tự động (Integration) | `TC03_UpdateDraft_PersistsExactFields` | PUT Draft → 200; response/GET/DB khớp mọi trường, kể cả Customer mới; giữ Draft. | PASS (1) |
| TC04 | Tự động (Integration) | `TC04_UpdatePendingApproval_ReturnsConflictAndPreservesAllFields` | PUT PendingApproval → 409; ProblemDetails có status và CTR-01; mọi scalar field Contract trong DB giữ nguyên. | PASS (1) |
| TC05 | Tự động (Integration) | `TC05_SubmitMissingCondition_ReturnsCtr02AndPreservesDraft` | Submit thiếu Content hoặc DocumentName → 400; `code=CTR-02`, lỗi chính xác; snapshot DB không đổi, GET vẫn Draft. | PASS (2) |
| TC06 | Tự động (Integration) | `TC06_SubmitValid_PersistsPendingApprovalOnly` | Submit hợp lệ → 200; contractId/status/message đúng; GET/DB PendingApproval, chỉ status thay đổi. | PASS (1) |
| TC07 | Tự động (Integration) | `TC07_RawApprovedStatus_Returns400AndPreservesAllFields` | PUT có trường status=Approved → 400; `errors` có key chứa `status` (không phân biệt hoa thường); mọi scalar field Contract trong DB không đổi. | PASS (1) |
| TC08 | Tự động (Integration) | `TC08_SequentialDuplicateSubmit_ReturnsConflictWithoutNewContract` | Submit tuần tự hai lần → 200 rồi 409; ProblemDetails đúng; snapshot và số Contract không đổi sau lần hai. | PASS (1) |

TC01–TC08 đều được tự động hóa, không có TC nào chỉ chạy thủ công hoặc chưa tự động hóa. Kiểm tra bổ sung ngày 11/11/2026: smoke test HTTP trang Swagger và `GET /api/customers` đều trả 200. Luồng demo Tuần 06 đã chạy lại tự động bằng `Week06_EndToEnd_MissingDraftThenCompleteAndSubmit` trên `qtkd_test`.

Hai test Integration bổ sung (mỗi test 1 case, đều PASS):

- `Week06_EndToEnd_MissingDraftThenCompleteAndSubmit`: tạo Customer Active → tạo Draft thiếu nội dung/tệp → Submit 400 với hai lỗi CTR-02 → GET vẫn Draft → PUT bổ sung → Submit 200 → GET/DB PendingApproval. Đây là một luồng liên tục, dùng cùng hợp đồng.
- `Pricing_ValidItemsPersist_InvalidPriceMissingListAndDuplicateRejected`: tạo PriceList/PriceItem 201, kiểm tra response/GET/DB; đơn giá 0 trả 400 có lỗi UnitPrice, danh sách không tồn tại trả 404, mã dịch vụ trùng trả 409; dữ liệu gốc giữ nguyên.

## Unit tự động: quy tắc và biên JSON

Unit dùng repository fake, không host HTTP và không kết nối database. Các method trong [ContractServiceTests.cs](../tests/QTKD.Api.Tests/Unit/ContractServiceTests.cs):

| Method | Số case | Nội dung |
| --- | ---: | --- |
| `CTR01_UpdateEditableContract_UpdatesFieldsAndPreservesStatus` | 2 | Draft/RevisionRequested cập nhật đúng trường, giữ status, lưu một lần. |
| `CTR01_UpdateLockedContract_RejectsWithoutMutation` | 2 | PendingApproval/Approved bị từ chối; snapshot không đổi, không lưu. |
| `CTR02_SubmitInvalidContract_ReportsExactErrorWithoutMutation` | 10 | Customer Inactive; ngày mặc định/đảo ngược; Content/DocumentName null, rỗng, whitespace; lỗi chính xác, không đổi dữ liệu và không lưu. |
| `CTR03_CreateContract_AlwaysStartsInDraft` | 1 | Create luôn Draft, trim dữ liệu, response khớp entity. |
| `CTR02_CTR03_SubmitValidDraft_OnlyChangesStatusToPendingApproval` | 1 | Submit chỉ đổi status sang PendingApproval, response đúng, lưu một lần. |
| `CTR03_SubmitNonDraft_RejectsWithoutMutation` | 3 | RevisionRequested/PendingApproval/Approved không được Submit, không đổi dữ liệu và không lưu. |

Case Unit thứ 20 là `CTR03_RequestWithApprovedStatus_IsRejected` trong [ContractRequestTests.cs](../tests/QTKD.Api.Tests/Unit/ContractRequestTests.cs): deserialize JSON có status=Approved phải ném `JsonException`. Cả 20 case đều PASS trong TRX cuối.

## Lỗi TC07 và hồi quy

Các bước tái hiện trước sửa:

1. Tạo Customer Active và Contract Draft với dữ liệu hợp lệ.
2. PUT `/api/contracts/{id}` với đầy đủ trường hợp lệ, đổi `contractNo` và thêm `"status": "Approved"`.
3. Mong đợi 400 vì client không được gửi status. Thực tế trước sửa là 200: serializer bỏ qua status không khai báo, service vẫn có thể cập nhật ContractNo. **Database không bị chuyển sang Approved.**

Lần chạy hồi quy trước sửa ghi 1/1 FAIL: `Expected: BadRequest; Actual: OK` (mong đợi 400, thực tế 200). Nguyên nhân là JSON mặc định bỏ qua member không ánh xạ trong DTO. TRX kèm bài nộp là kết quả cuối sau sửa.

Sửa tối thiểu tại `ContractRequestDto`: thêm `[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]`. Payload chứa trường ngoài DTO bị từ chối trước service. Sau sửa, TC07 trả 400 và giữ nguyên mọi trường; Unit kiểm tra biên serializer và Integration kiểm tra HTTP/DB đều PASS.

Commit sửa lỗi: `d0d493d6e75b3512ac387a27db9a95e69c68cab7`.

## Database và giới hạn minh chứng

Integration dùng PostgreSQL thật `qtkd_test`, đọc lại dữ liệu qua DbContext mới và `AsNoTracking`. Factory tự áp dụng migration/seed hiện có; không thêm migration mới. Mỗi case quản lý các ID do chính nó tạo và xóa đúng các bản ghi đó theo thứ tự khóa ngoại, giữ seed; không drop/truncate database hoặc bảng.

Thiếu cấu hình test DB làm test thất bại, không skip. Chỉ đọc `QTKD_TEST_CONNECTION_STRING` (ưu tiên) hoặc User Secrets project test `ConnectionStrings:QtkdTestDatabase`, không fallback sang cấu hình API. Đã kiểm tra cấu hình cố ý trỏ `qtkd_db` bị guard từ chối trước kết nối; fingerprint dữ liệu database ứng dụng trước/sau kiểm thử không đổi.

Minh chứng là chạy local, chưa kiểm chứng trên máy khác hoặc từ remote clone. TC08 chỉ chứng minh gửi lặp tuần tự; chưa có bảo đảm cho request đồng thời. Phiên bản chưa có Approval entity/workflow đầy đủ hay Authentication/Authorization. Content và DocumentName vẫn là giả định demo để kiểm tra CTR-02, không phải yêu cầu nghiệp vụ chính thức đã xác nhận.
