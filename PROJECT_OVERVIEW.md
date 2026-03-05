# 📘 Learning (Backend) — Project Overview

> **Mục đích**: File tham chiếu nhanh cho AI hoặc developer mới. Đọc file này thay vì phải scan toàn bộ project backend.
>
> **Cập nhật lần cuối**: 2026-02-28

---

## 1. Tech Stack

| Category  | Tech                             | Version         |
| --------- | -------------------------------- | --------------- |
| Framework | ASP.NET Core Web API             | .NET 8.0        |
| Language  | C#                               | 12.0            |
| Database  | SQL Server                       | via EF Core 8.0 |
| ORM       | Entity Framework Core            | 8.0.23          |
| Auth      | JWT Bearer Authentication        | 8.0.23          |
| Password  | BCrypt.Net-Next                  | 4.0.3           |
| Logging   | Serilog (File & Console, Async)  | 8.0.3           |
| API Docs  | Swagger (Swashbuckle.AspNetCore) | 8.1.4           |

---

## 2. Project Structure (Clean Architecture)

```
Learning/
├── Learning.sln              # Solution file
├── api_flows.md              # Tham chiếu API flows chi tiết
│
├── API/                      # Presentation Layer (Web API)
│   ├── Controllers/          # API Controllers (Auth, Deck, Vocabulary, Grammar, vv.)
│   ├── Extensions/           # Dependency Injection setup, Swagger config, vv.
│   ├── wwwroot/              # Static files
│   ├── appsettings.json      # Configuration (DB connection, JWT secret, vv.)
│   └── Program.cs            # Entry point, Middleware pipeline, CORS
│
├── Application/              # Application logic / Use cases
│   ├── DTOs/                 # Request & Response models (Auth, User, Cards...)
│   ├── Helpers/              # Utilities
│   ├── IRepositories/        # Interfaces cho Data access
│   ├── Services/             # Business logic (AuthService, DeckService, vv.)
│   ├── Common/               # Shared logic, Exceptions
│   └── Settings/             # Strongly typed configurations
│
├── Domain/                   # Core business entities & Enums
│   └── Entities/             # User, Deck, VocabularyCard, GrammarCard, RefreshToken...
│
└── Infrastructure/           # Data access & External services
    ├── Persistence/          # DbContext (ApplicationDbContext), Configurations (Fluent API)
    ├── Repositories/         # Triển khai IRepositories
    └── Migrations/           # EF Core Migrations
```

---

## 3. Configuration Chi Tiết

### Middleware & Services (`API/Program.cs`)

- **CORS**: Mở port cho Frontend React (`http://localhost:5173`), `AllowCredentials()` để hỗ trợ gửi nhận HttpOnly Cookies (cho Refresh Token).
- **JSON Options**: Cho phép convert enum sang string gíup output API thân thiện hơn.
- **Logging**: Sử dụng `Serilog` để ghi log ra File, cấu hình từ appsettings.
- **Authentication**: JWT Bearer setup qua extension method `AddAuthConfigurationExtension`.
- **Swagger**: Sử dụng Swagger UI trong môi trường Development.

### Database (`Infrastructure`)

- EF Core `ApplicationDbContext`.
- Sử dụng SQL Server provider.
- Setup cấu hình Entity mapping riêng rẽ trong thư mục `Persistence/Configurations/`.

---

## 4. Architecture Patterns

### Tách lơp (Clean Architecture)

- **Dependency Rule**: Lớp bên ngoài (API, Infrastructure) phụ thuộc vào lớp bên trong (Application, Domain). Domain không phụ thuộc vào bất cứ đâu.
- **Dependency Injection**: Tách triệt để bằng abstract `IRepository` trong Application layer, được implement trong Infrastructure layer và inject tại tầng API.

### Request Flow

```
Controller (API) -> Service (Application) -> Repository (Infrastructure) -> Database (SQL Server)
```

1. **API Layer**: `Controllers` nhận HTTP request, parse model, gọi `Service` layer, wrap kết quả vào `ApiResponse<T>`.
2. **Application Layer**: `Services` xử lý business logic, validate (nếu cần phối hợp Db), sử dụng `IRepository` để tương tác DB, map sang `DTO` rổi trả về cho Controller.
3. **Infrastructure Layer**: Mọi truy cập vào DB thông qua entity framework, xử lý query hiệu quả (AsNoTracking, Include, vv).

### Authentication

- Dùng **Access Token (JWT)** (thời gian sống ngắn, trả về qua json payload) và **Refresh Token** (thời gian sống dài, lưu trong database & gửi xuống client qua HttpOnly cookie).
- Xác thực mật khẩu thông qua Bcrypt.
- Custom Authorization Policies nếu có.

### API Response Wrapper

Luôn trả về định dạng chuẩn (phù hợp với interface `ApiResponse<T>` bên phía Frontend):

```json
{
  "code": 200,
  "success": true,
  "message": "Error/Success constant",
  "data": { ... },
  "metaData": { "total": 50, "page": 1, "pageSize": 20 }
}
```

---

## 5. Môi trường và Scripts

### Chạy Dự án

- Run/Debug project `API` qua profile `http` hoặc `https` trong IDE (Visual Studio / Rider).
- Hoặc dùng CLI: `dotnet run --project API/API.csproj` (định tuyến gốc thư mục `Learning`).
- Swagger sẽ có mặc định tại port được config (thường là `http://localhost:5246`).

### EF Core Migrations

Có thể khởi chạy migrations từ root `Learning` folder:

- Add Migration: `dotnet ef migrations add <MigrationName> --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj`
- Update Database: `dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj`

---

## 6. TODO (Features / Cải tiến)

- [ ] Thêm Unit/Integration tests cho layer Application.
- [ ] Phân quyền nâng cao (Roles & Claims).
- [ ] Cải thiện global exception pipeline (Middleware xử lý lỗi để không lặp code ở Controller).
- [ ] Caching logic (Redis / In-memory) nếu cần thiết để scale.
