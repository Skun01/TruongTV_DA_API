# Backend Organization Note (Team Reference)

Purpose: This file is the source of truth for how backend code must be organized in this project.
If you are unsure where to place code, follow this file.

## 1. Layer Responsibilities

- Domain:
  - Entities, enums, constants, value objects.
  - No dependency on other layers.
- Application:
  - DTOs, service interfaces, repository interfaces, business services.
  - Mapping extensions and helper utilities.
- Infrastructure:
  - EF Core DbContext, repository implementations, external integrations.
- API:
  - Controllers, validators, middleware, program bootstrap.

## 2. Folder Rules (Important)

- DTOs go to `Application/DTOs`.
- One DTO class per file.
- Search/list query parameters must use Query DTOs, not long parameter lists.
- Shared pagination fields (`page`, `pageSize`) must be centralized in a shared query base DTO.
- Mapping code must live in `Application/Mappings`, not in services.
- Reusable utility logic must live in `Application/Helper`, not in services.
- Services should contain business flow only.

## 3. Current Patterns To Follow

- Query DTO pattern:
  - `SentenceSearchQuery`, `VocabularySearchQuery`.
  - Shared base paging DTO: `PagingQuery`.
- Mapping pattern:
  - Extension methods like `ToResponse`, `ToDetailResponse`, `ToAuthUserDto`.
- Helper pattern:
  - `PagingHelper` for page/pageSize normalization.
  - `EnumParsingHelper` for enum parsing + invalid handling.
  - `StringHelper` for normalize optional text and string lists.
  - `SecurityHelper` for token hash utility.
  - `VocabularyHelper` for vocabulary-specific reusable utilities.

## 4. Service Design Rules

- Service methods should orchestrate:
  - validate input state,
  - check authorization/business guards,
  - call repositories,
  - call mappings/helpers,
  - return DTOs.
- Avoid private mapping methods in services.
- Avoid repeated utility snippets in services.
- Throw `ApplicationException` with `MessageConstants` for business errors.

## 5. Controller Design Rules

- Controllers stay thin.
- Controllers accept request/query DTOs and call services.
- Search/list endpoints must receive `[FromQuery] QueryDto`.
- Write endpoints that modify database must be protected by role policy when required (Editor/Admin).

## 6. Validation Rules

- All request DTOs used by write endpoints must have FluentValidation validators in `API/Validators`.
- Keep validators close to feature modules (`Validators/Vocabulary`, `Validators/Sentences`, etc.).

## 7. Naming Rules

- Use clear feature names:
  - DTOs: `CreateXRequest`, `UpdateXRequest`, `XResponse`, `XSearchQuery`.
  - Mappings: `XMappings.cs`.
  - Helpers: `XHelper.cs`.
- Keep methods explicit and intention-revealing.

## 8. Quick Checklist Before Commit

- Is each DTO in its own file?
- Did search/list use Query DTO instead of multiple query params?
- Is mapping code in `Application/Mappings`?
- Is utility code in `Application/Helper`?
- Are services free from mapping/utility clutter?
- Are validators added for new write DTOs?
- Are authorization policies correct for write endpoints?
- Did solution build pass?

## 9. What Not To Do

- Do not add new inline mapping blocks in services.
- Do not add new duplicated parse/normalize helpers inside services.
- Do not put infrastructure concerns in Application layer.
- Do not bypass DTOs by binding many primitive query params in controllers for search endpoints.

## 10. Maintenance Note

When architecture conventions change, update this file in the same PR/commit so future contributors do not drift from the agreed structure.
