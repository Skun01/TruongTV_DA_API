# 🎨 Style Guide & Coding Guidelines — Learning (Backend)

> File này ghi lại các quy tắc về phong cách code và quy tắc kiến trúc backend.
> Khi bắt đầu đoạn chat mới hoặc dùng AI khác, hãy đọc file này trước.
> **Cập nhật lần cuối**: 2026-02-28

---

## 1. Kiến trúc (Clean Architecture)

- **Domain Layer**: Chỉ chứa Entities, Value Objects, Enums. KHÔNG chứa logic nghiệp vụ đặc thù hay dependency external nào.
- **Application Layer**: Business logic (Service), Interface của Repository (`IRepositories`), DTOs. Layer này KHÔNG biết gì về cách dữ liệu được lưu xuống DB.
- **Infrastructure Layer**: DbContext, Entity Framework Configurations (`IEntityTypeConfiguration`), Repositories. Tuyệt đối KHÔNG chứa business logic tại đây.
- **API Layer**: Controllers, API setup, Middleware. Controllers phải **mỏng** (thin controller), chỉ làm nhiệm vụ tiếp nhận HTTP request, delegate vào `Service` sau đó trả về chuẩn `ApiResponse<T>`.

---

## 2. Quy trình triển khai tính năng mới (Feature Workflow)

Trước khi tiến hành thực hiện một chức năng nào đó, **BẮT BUỘC** tuân thủ thứ tự xây dựng sau:

1. **Database (Entities)**: Thiết kế các Entity và cấu hình DB (Fluent API) trước để định hình cấu trúc dữ liệu cốt lõi.
2. **API Design (Controllers)**: Thiết kế API endpoints cho các chức năng tương ứng (định nghĩa route, method GET/POST/...).
3. **DTOs (Data Transfer Objects)**: Xây dựng các request/response DTO khớp với API đã thiết kế.
4. **Business Logic (Services)**: Cuối cùng mới triển khai logic nghiệp vụ chi tiết vào tầng Services.
   => Và chỉ thực hiện một chức năng trong một lần để người dùng dễ dàng debug và hiểu logic.

---

## 3. Quy tắc Naming & Code Style

### Case Styles

- **Class / Interface / Enum / Method / Property**: `PascalCase`
- **Private fields**: `_camelCase` (Prefix với dấu `_` để phân biệt).
- **Interface**: Luôn bắt đầu bằng chữ `I` (vd: `IAuthService`, `IUserRepository`).
- **Variables / Parameters**: `camelCase`.

### DTOs (Data Transfer Objects)

- DTO KHÔNG chứa logic đặc biệt.
- Chia nhỏ DTO theo chức năng request: `CreateDeckRequest`, `UpdateVocabularyCardRequest`, `LoginRequest`.
- Output DTO: `DeckDTO`, `DeckSummaryDTO`, `AuthDTO`.
- Lưu giữ tất cả trong thư mục `Application/DTOs/`.

### Controllers

- Tên Controller tận cùng bằng `Controller` (vd: `AuthController`).
- Base route thông thường: `[Route("api/[controller]")]` hoặc custom cho gọn `[Route("v1/auth")]` tuỳ convention API. Trong project này, các route thường map root như `[Route("[controller]")]`.
- Trả kết quả: Luôn dùng pattern trả chuỗi JSON bọc trong đối tượng chuẩn (Response DTO + Error/Success code) - xem trong file `api_flows.md` list mảng error codes rỗng.

---

## 3. Quản lý Dữ liệu (Entity Framework)

### Fluent API

- KHÔNG dùng Data Annotations (`[Required]`, `[MaxLength]`) trên Entity.
- Cấu hình (Table name, relations, index, length) phải để toàn bộ trong các class thực thi `IEntityTypeConfiguration<T>` ở tầng **Infrastructure/Persistence/Configurations**.

### Repository Pattern

- Luôn sử dụng interface cho repository.
- Các hàm queries chung chung như `GetAll`, `GetById` có thể để ở base interface. Các query phức tạp phải có tên hàm riêng truyền đạt rõ ngữ nghĩa (vd: `GetDeckWithCardsByIdAsync`).
- Luôn sử dụng `AsNoTracking()` cho các truy vấn chỉ lấy dữ liệu (READ-ONLY) để tăng performance cho API.

---

## 4. Xử lý Lỗi & Log

### Error Handling

- Hạn chế ném `Exception` tràn xuống cho Controller.
- Với các lỗi nghiệp vụ dự đoán trước (vd: Account không tồn tại, sai mật khẩu), Service trả về error code qua logic của app (có thể dùng tuple trả về kiểu `<T Data, string ErrorCode>` hoặc wrap qua một class kết quả).
- Error messages phải map với hằng số Frontend để hiển thị tiếng Việt (VD: `Email_Exist_409`, `Invalid_400`... Xem trong `api_flows.md`).

### Logging

- Inject `ILogger<T>` tại Controller / Service.
- Ghi log (Information) cho các thay đổi dữ liệu quan trọng, (Error) cho các exception.
- Không log các thông tin nhạy cảm: Mật khẩu plaintext, Token.

---

## 5. Tổng kết quy tắc

| Hạng mục            | Quy tắc                                                                     |
| ------------------- | --------------------------------------------------------------------------- |
| **Kiến trúc**       | Clean Architecture. Dependency chảy từ ngoài vào trong.                     |
| **Controllers**     | Mỏng. Chỉ routing & trả Response.                                           |
| **Services**        | Chứa core business logic. Dùng IRepository để nói chuyện DB.                |
| **Entities**        | Clean, không Data Annotation. Cấu hình bằng Fluent API.                     |
| **DTOs**            | Request / Response rõ ràng, không tái sử dụng bừa bãi.                      |
| **EF Core**         | Luôn dùng `AsNoTracking()` cho HTTP GET queries.                            |
| **Response Format** | Chuẩn chung `ApiResponse<T>` với code logic string cho client tự map error. |
