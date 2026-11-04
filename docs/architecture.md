# Kiến trúc QTKD - Tuần 06

## Cấu trúc

```text
source/
|-- QTKD.sln
|-- src/QTKD.Api/
|   |-- Controllers/
|   |-- DTOs/
|   |-- Data/
|   |-- Entities/
|   |-- Migrations/
|   |-- Repositories/
|   `-- Services/
|-- docs/
|   |-- architecture.md
|   `-- week06-api.md
|-- .gitignore
`-- README.md
```

```mermaid
flowchart LR
    Client[Swagger hoặc HTTP Client] --> Controller[Controller]
    Controller --> Service[Service]
    Service --> Repository[Repository]
    Repository --> DbContext[QtkdDbContext]
    DbContext --> PostgreSQL[(PostgreSQL)]
```

- Controller nhận request, gọi service và chọn HTTP status phù hợp.
- Service kiểm tra quy tắc nghiệp vụ, chuyển Entity sang DTO và quyết định khi nào được lưu.
- Repository chứa các truy vấn EF Core.
- `QtkdDbContext` khai báo bảng, khóa, quan hệ và dữ liệu mẫu.

## Luồng Submit Contract

```mermaid
flowchart TD
    A[POST /api/contracts/id/submit] --> B{Contract tồn tại?}
    B -- Không --> C[404 Not Found]
    B -- Có --> D{Status là Draft?}
    D -- Không --> E[409 Conflict]
    D -- Có --> F{Customer, ngày, nội dung và tệp hợp lệ?}
    F -- Không --> G[400 Bad Request và không lưu]
    F -- Có --> H[Đổi sang PendingApproval]
    H --> I[Lưu PostgreSQL và trả 200 OK]
```

Không có đường chuyển trực tiếp từ `Draft` sang `Approved`. Approval workflow đầy đủ chưa nằm trong phạm vi Tuần 06.
