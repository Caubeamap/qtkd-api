# QTKD - Tuần 07

Web API .NET 10 dùng Entity Framework Core và PostgreSQL để quản lý Customer, Contract, PriceList/PriceItem và gửi hợp đồng trình duyệt.

## Yêu cầu

- .NET SDK 10 và PostgreSQL đang chạy tại `localhost:5432`.
- EF Core CLI 10.0.12. Nếu chưa có, chạy `dotnet tool install --global dotnet-ef --version 10.0.12`.

Clone và mở terminal tại thư mục chứa `QTKD.sln`:

```powershell
git clone https://github.com/Caubeamap/qtkd-api.git
cd qtkd-api
dotnet restore QTKD.sln
```

## Cấu hình database

Tạo hai database trong pgAdmin hoặc `psql` nếu chưa tồn tại (chạy riêng từng câu lệnh):

```sql
CREATE DATABASE qtkd_db;
CREATE DATABASE qtkd_test;
```

Lưu connection string bằng User Secrets, thay thông tin PostgreSQL của bạn vào các placeholder:

```powershell
dotnet user-secrets set "ConnectionStrings:QtkdDatabase" "Host=localhost;Port=5432;Database=qtkd_db;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>" --project src/QTKD.Api
dotnet user-secrets set "ConnectionStrings:QtkdTestDatabase" "Host=localhost;Port=5432;Database=qtkd_test;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>" --project tests/QTKD.Api.Tests
```

Không ghi mật khẩu thật vào source hoặc Git. Tài khoản PostgreSQL cần quyền kết nối và tạo/cập nhật bảng để chạy migration trên database tương ứng. Hai cấu hình thuộc hai project khác nhau.

Integration ưu tiên biến môi trường `QTKD_TEST_CONNECTION_STRING`, sau đó mới đọc User Secrets của project test. Nếu máy đã đặt biến này, hãy kiểm tra nó trước khi chạy. Test chỉ chấp nhận chính xác `Database=qtkd_test`, không dùng connection string ứng dụng làm dự phòng. Thiếu hoặc sai cấu hình làm test thất bại, không bỏ qua test.

## Chạy migration và API

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project src/QTKD.Api
dotnet run --project src/QTKD.Api --launch-profile http
```

Mở Swagger tại <http://localhost:5000/swagger>. Dừng API bằng `Ctrl+C`.

## Chạy kiểm thử Tuần 07

Mở terminal thứ hai tại thư mục `qtkd-api`:

```powershell
dotnet test QTKD.sln
```

Project `tests/QTKD.Api.Tests` gồm Unit dùng repository fake, không gọi database; Integration tự host API bằng `WebApplicationFactory` và dùng PostgreSQL thật tại `qtkd_test`. Không cần có sẵn tiến trình `dotnet run` để chạy test. Migration hiện có tự áp dụng trên database test; migration database ứng dụng vẫn chạy bằng lệnh riêng ở trên. Test không dùng database ứng dụng `qtkd_db`.

Chạy từng nhóm hoặc tạo lại minh chứng:

```powershell
dotnet test QTKD.sln --filter Category=Unit
dotnet test QTKD.sln --filter Category=Integration
dotnet test QTKD.sln --logger "trx;LogFileName=week07.trx" --results-directory docs/test-evidence
```

Minh chứng local ngày 12/11/2026: **36/36 PASS** (20 Unit, 16 Integration, không bỏ qua), tại [week07.trx](docs/test-evidence/week07.trx). Bảng TC01–TC08, lỗi đã sửa và giới hạn minh chứng nằm trong [week07-testing.md](docs/week07-testing.md).

## Phạm vi Tuần 06

- Customer: tạo, xem danh sách/chi tiết và cập nhật.
- Contract: tạo ở trạng thái `Draft`, xem chi tiết, cập nhật và gửi trình duyệt.
- Pricing: tạo/xem PriceList, thêm/xem PriceItem.
- Update Contract kiểm tra CTR-01; Submit kiểm tra CTR-02 và CTR-03 trước khi lưu.

Danh sách endpoint, request/response mẫu, giả định demo và bảng đối chiếu quy tắc nằm trong [docs/week06-api.md](docs/week06-api.md). Có thể chạy lần lượt các request trong `src/QTKD.Api/QTKD.Api.http` để demo luồng thất bại và thành công.

Khi dùng file `.http`, cập nhật `@customerId` theo `id` trong response của request 1 trước khi chạy request 2, rồi cập nhật `@contractId` theo `id` trong response của request 2 trước khi chạy các request tiếp theo.

## Giới hạn phiên bản

Phiên bản hiện tại chưa xây dựng Authentication, Approval workflow đầy đủ, Payment hoặc giao diện web. Submit được mô phỏng bằng chuyển trạng thái `Draft` sang `PendingApproval`; gửi lặp tuần tự sau đó bị từ chối. Kiểm thử chưa chứng minh xử lý gửi đồng thời. `Content`/`DocumentName` vẫn là giả định demo Tuần 06.
