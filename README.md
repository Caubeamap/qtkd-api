# QTKD - Tuần 06

Web API .NET 10 dùng Entity Framework Core và PostgreSQL để quản lý Customer, Contract, PriceList/PriceItem và gửi hợp đồng trình duyệt.

## Yêu cầu

- .NET SDK 10 và PostgreSQL đang chạy tại `localhost:5432`.
- EF Core CLI 10.0.12. Nếu chưa có, chạy `dotnet tool install --global dotnet-ef --version 10.0.12`.

Sau khi clone repository, mở terminal tại thư mục chứa `QTKD.sln`.

## Cấu hình database

Tạo database `qtkd_db` trong pgAdmin hoặc `psql` nếu database chưa tồn tại:

```sql
CREATE DATABASE qtkd_db;
```

Lưu connection string bằng User Secrets, thay thông tin PostgreSQL của bạn vào các placeholder:

```powershell
dotnet user-secrets set "ConnectionStrings:QtkdDatabase" "Host=localhost;Port=5432;Database=qtkd_db;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>" --project src/QTKD.Api
```

Không ghi mật khẩu thật vào source hoặc Git.

## Chạy migration và API

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet restore QTKD.sln
dotnet ef database update --project src/QTKD.Api
dotnet run --project src/QTKD.Api --launch-profile http
```

Mở Swagger tại <http://localhost:5000/swagger>. Dừng API bằng `Ctrl+C`.

## Phạm vi Tuần 06

- Customer: tạo, xem danh sách/chi tiết và cập nhật.
- Contract: tạo ở trạng thái `Draft`, xem chi tiết, cập nhật và gửi trình duyệt.
- Pricing: tạo/xem PriceList, thêm/xem PriceItem.
- Submit Contract: kiểm tra CTR-01, CTR-02 và CTR-03 trước khi lưu.

Danh sách endpoint, request/response mẫu, giả định demo và bảng đối chiếu quy tắc nằm trong [docs/week06-api.md](docs/week06-api.md). Có thể chạy lần lượt các request trong `src/QTKD.Api/QTKD.Api.http` để demo luồng thất bại và thành công.

## Giới hạn phiên bản

Tuần 06 chưa xây dựng Authentication, Approval workflow đầy đủ, Payment, giao diện web hoặc test project. Submit được mô phỏng bằng chuyển trạng thái `Draft` sang `PendingApproval`; gửi lặp sau đó bị từ chối và không tạo thêm dữ liệu xử lý.
