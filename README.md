# QTKD — Tuần 05

Web API .NET 10 dùng Entity Framework Core và PostgreSQL để đọc Customer và Contract.

## Yêu cầu

- .NET SDK 10 và PostgreSQL đang chạy (mặc định `localhost:5432`).
- Tài khoản PostgreSQL có quyền tạo database/bảng; có thể dùng pgAdmin hoặc `psql`.
- EF Core CLI 10.0.12: nếu chưa có, chạy `dotnet tool install --global dotnet-ef --version 10.0.12`.

Sau khi clone repository, mở terminal tại thư mục chứa `QTKD.sln`. Các lệnh dưới đây dùng PowerShell.

## Tạo database và cấu hình

Trong Query Tool của pgAdmin (kết nối database `postgres`) hoặc `psql`, chạy theo yêu cầu bài:

```sql
CREATE DATABASE qtkd_db;
```

Nếu database đã tồn tại thì bỏ qua bước tạo. EF Core `database update` cũng có thể tạo database khi tài khoản đủ quyền; hướng dẫn này tạo `qtkd_db` trước.

Project đã có `UserSecretsId`. Lưu connection string bằng User Secrets, thay các placeholder bằng thông tin PostgreSQL của bạn:

```powershell
dotnet user-secrets set "ConnectionStrings:QtkdDatabase" "Host=localhost;Port=5432;Database=qtkd_db;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>" --project src/QTKD.Api
```

Không ghi mật khẩu thật vào source hoặc Git. User Secrets dùng cho môi trường Development; khi triển khai có thể cung cấp biến môi trường `ConnectionStrings__QtkdDatabase`.

## Áp dụng migration và chạy

Migration `InitialCreate` đã có trong source, gồm bốn bảng và dữ liệu mẫu. Không cần tạo lại migration khi clone:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet restore QTKD.sln
dotnet ef database update --project src/QTKD.Api
dotnet build QTKD.sln
dotnet run --project src/QTKD.Api --launch-profile http
```

Mở Swagger: <http://localhost:5000/swagger>. Swagger được bật trong Development. Dừng API bằng `Ctrl+C`.

## Thử API

Tuần 05 có đúng hai endpoint:

| Endpoint | Kết quả |
| --- | --- |
| `GET /api/customers` | `200`: hai Customer ID 1 (`Công ty Minh Phát`) và ID 2 (`Công ty Đại Dương`), trạng thái `Active`, sắp xếp theo ID. |
| `GET /api/contracts/{id}` | ID `1` trả `200`: `HD-2026-018`, CustomerId `1`, từ `2026-10-01` đến `2027-09-30`, trạng thái `Draft`. ID `999` trả `404`. |

Dùng Swagger **Try it out** hoặc ba request trong `src/QTKD.Api/QTKD.Api.http`. Dữ liệu được truy vấn từ PostgreSQL. Sơ đồ thành phần và mô hình dữ liệu nằm trong [docs/architecture.md](docs/architecture.md).
