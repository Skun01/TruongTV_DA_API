# Backend Architecture & Code Structure Guide (Tacho Learning API)

This document is intended for AI agents and developers to quickly understand the backend organization and architectural patterns used in this project.

## 1. High-Level Architecture (Clean Architecture / N-Tier)
The project `learning` adopts a layered architecture separating concerns across 4 main class-library projects:

### 1.1 `Domain` layer
- **Role:** The core of the system. Contains all business entities, enums, constants.
- **Dependencies:** None. It has zero external dependencies or references to other projects.
- **Key Folders:**
  - `Entities/`: Contains Entity models mirroring DB schema (e.g., `User.cs`, `MediaAsset.cs`, `BaseEntity.cs`).
  - `Enums/`: System-wide Enums like `FileType`, `UserRole`.
  - `Constants/`: Hardcoded references (e.g. `MessageConstants.cs` containing string templates like `INVALID_LOGIN`).

### 1.2 `Application` layer
- **Role:** Defines the business logic, use cases, and interfaces that abstract away infrastructure.
- **Dependencies:** Depends only on `Domain`.
- **Key Folders:**
  - `DTOs/`: Used to transfer data between frontend, controllers, and services (e.g., `LoginRequest`, `AuthUserDTO`).
  - `IServices/` & `IRepositories/`: Abstractions (interfaces) mapping out behavior without implementations.
  - `Services/`: Concrete implementations of business logic (e.g., `AuthService`, `ResourceService`).
  - `Mappings/`: AutoMapper profiles to convert Entities <-> DTOs.
  - `Settings/`: Type-safe configuration maps (e.g., `JwtSettings`).

### 1.3 `Infrastructure` layer
- **Role:** Implements the `Application` interfaces providing data access, external API integrations, and low-level system services.
- **Dependencies:** Depends on `Application` and `Domain`.
- **Key Folders:**
  - `Persistence/`: Entity Framework Core setup, `DbContext`.
  - `Repositories/`: Generic repository implementations and `UnitOfWork`.
  - `InternalServices/`: Specific third-party/SDK implementations such as `FileUploadService` (Cloudinary) and `EmailService`.
  - `DependencyInjection.cs`: Registration of infrastructure services.

### 1.4 `API` layer
- **Role:** The application entry-point and presentation layer. Accepts HTTP requests, validates them, and routes them to `Application` services.
- **Dependencies:** References `Application`, `Domain`, and `Infrastructure`.
- **Key Folders:**
  - `Controllers/`: Route definitions and REST endpoints. Should be "thin" and only delegate tasks.
  - `Middlewares/`: Central request interceptors (e.g., `GlobalExceptionMiddleware.cs`).
  - `Validators/`: FluentValidation classes for incoming request payload validation.
  - `Program.cs`: Bootstraps the DI container, middleware pipeline, and the ASP.NET Host.

---

## 2. API Response Convention (CRITICAL)

**All responses returned to the client are strictly encapsulated by a standard wrapper format:**
```json
{
  "Code": 200,          // Business status code (200, 400, 401, 404, 409, 500...)
  "Success": true,      // Boolean indicator for operational outcome
  "Message": "string",  // System message code mapping to UI dictionary (e.g., "Invalid_400", "Validation_400")
  "Data": {},           // Payload data
  "MetaData": null      // Pagination / additional stats context
}
```
**Important Behaviors to follow:**
1. **HTTP 200 Rule:** Most business exceptions and validation errors are returned with **HTTP Status `200 OK`**. They **DO NOT** use HTTP 4xx natively. The client evaluates the failure based on the `Success` property inside the JSON body.
2. **Global Exception Handling:** The `GlobalExceptionMiddleware` catches `ApplicationException` (business logic errors thrown anywhere down the pipeline) and translates them into an `ApiResponse` object with `Success: false`.
3. **PascalCase Properties:** ASP.NET is serializing these models without strict camelCase overrides, meaning frontend JSON payloads will contain PascalCase properties (`Success`, `Message`, `Code`). If you modify client interceptors, always check for BOTH casing variants (e.g. `body.success ?? body.Success`).
4. **Validation Pipeline:** FluentValidation executes synchronously per ModelBinder. It traps errors gracefully and packages them under `"Message": "Validation_400"` with `"Data"` containing the detailed field breakdown.

---

## 3. Recommended Workflow for Modifying Backend

Whenever creating new features:
1. Define any new data requirements in `Domain/Entities`.
2. Map out expected request and response models in `Application/DTOs`.
3. Create abstractions (e.g., `INewFeatureService`) in `Application/IServices`.
4. Implement the logic internally in `Application/Services`. Inject repositories/UnitOfWork as needed.
5. Provide data access through `Infrastructure/Repositories`. Update migrations if `Entities` changed.
6. Create input validators in `API/Validators`.
7. Wire up the HTTP routing in `API/Controllers`.
8. Never throw untyped `Exception`s for domain errors. Use `throw new ApplicationException(MessageConstants...);` so the global handler intercepts and wraps it uniformly for the client.
