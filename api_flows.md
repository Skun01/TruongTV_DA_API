# API Flows — Learning App

> Base URL: `http://localhost:5246`
> Tất cả API trả về format `ApiResponse<T>`:
>
> ```json
> {
>   "code": 200,
>   "success": true,
>   "message": null,
>   "data": { ... },
>   "metaData": { "total": 50, "page": 1, "pageSize": 20 }  // chỉ có khi phân trang
> }
> ```

---

## 1. Auth Flow — Xác thực người dùng

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant API as Backend
    participant DB as Database
    participant Cookie as HTTP Cookie

    Note over FE,Cookie: === ĐĂNG KÝ ===
    FE->>API: POST /auth/register
    API->>DB: Check email tồn tại
    API->>DB: Hash password + lưu User
    API-->>FE: { success: true }

    Note over FE,Cookie: === ĐĂNG NHẬP ===
    FE->>API: POST /auth/login
    API->>DB: Verify email + password
    API->>DB: Tạo RefreshToken mới
    API-->>Cookie: Set-Cookie: refreshToken=xxx (HttpOnly)
    API-->>FE: { data: { accessToken: "jwt..." } }

    Note over FE,Cookie: === GỌI API CÓ AUTH ===
    FE->>API: GET /decks (Header: Authorization: Bearer {accessToken})
    API-->>FE: { data: [...] }

    Note over FE,Cookie: === TOKEN HẾT HẠN ===
    FE->>API: POST /auth/refresh (Cookie tự gửi kèm)
    API->>DB: Validate refreshToken
    API->>DB: Tạo RefreshToken mới
    API-->>Cookie: Set-Cookie: refreshToken=newXxx
    API-->>FE: { data: { accessToken: "newJwt..." } }

    Note over FE,Cookie: === ĐĂNG XUẤT ===
    FE->>API: POST /auth/logout (Cookie tự gửi kèm)
    API->>DB: Xóa/revoke refreshToken
    API-->>Cookie: Xóa cookie refreshToken
    API-->>FE: { success: true }
```

### Endpoints

| Method | Route                           | Body                   | Response               | Auth |
| ------ | ------------------------------- | ---------------------- | ---------------------- | ---- |
| POST   | `/auth/register`                | `RegisterRequest`      | `bool`                 | ❌   |
| POST   | `/auth/login`                   | `LoginRequest`         | `AuthDTO` + Set Cookie | ❌   |
| POST   | `/auth/refresh`                 | _(cookie tự gửi)_      | `AuthDTO` + Set Cookie | ❌   |
| POST   | `/auth/logout`                  | _(cookie tự gửi)_      | `bool` + Xóa Cookie    | ❌   |
| POST   | `/auth/forgot-password?email=x` | _(query)_              | `bool`                 | ❌   |
| POST   | `/auth/reset-password`          | `ResetPasswordRequest` | `bool`                 | ❌   |

### DTOs

```typescript
// === Request ===
RegisterRequest {
  username: string
  email: string
  password: string
}

LoginRequest {
  email: string
  password: string
}

ResetPasswordRequest {
  token: string        // token nhận từ email
  newPassword: string
}

// === Response ===
AuthDTO {
  accessToken: string  // JWT, frontend lưu vào memory/localStorage
  // refreshToken không trả về JSON, chỉ set qua HttpOnly Cookie
}
```

### Frontend cần làm:

1. Lưu `accessToken` vào memory (hoặc localStorage)
2. Gắn `Authorization: Bearer {token}` vào mọi request cần auth
3. Khi nhận 401 → gọi `/auth/refresh` → lấy accessToken mới → retry request
4. Đảm bảo `credentials: 'include'` (fetch) hoặc `withCredentials: true` (axios) để gửi cookie

---

## 2. Deck Flow — Quản lý bộ thẻ

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant API as Backend

    Note over FE,API: === DANH SÁCH DECK ===
    FE->>API: GET /decks?keyword=N5&type=Vocabulary&page=1&pageSize=20
    API-->>FE: { data: [DeckSummaryDTO, ...], metaData: { total, page, pageSize } }

    Note over FE,API: === XEM CHI TIẾT DECK ===
    FE->>API: GET /decks/{id}
    API-->>FE: { data: DeckDTO }

    Note over FE,API: === TẠO DECK MỚI ===
    FE->>API: POST /decks
    API-->>FE: { data: true }

    Note over FE,API: === CẬP NHẬT DECK ===
    FE->>API: PUT /decks/{id}
    API-->>FE: { data: true }

    Note over FE,API: === XÓA DECK ===
    FE->>API: DELETE /decks/{id}
    API-->>FE: { data: true }
```

### Endpoints

| Method | Route         | Body / Query                         | Response                      | Auth |
| ------ | ------------- | ------------------------------------ | ----------------------------- | ---- |
| GET    | `/decks`      | `?keyword=&type=&page=1&pageSize=20` | `DeckSummaryDTO[]` + MetaData | ✅   |
| GET    | `/decks/{id}` | —                                    | `DeckDTO`                     | ✅   |
| POST   | `/decks`      | `CreateDeckRequest`                  | `bool`                        | ✅   |
| PUT    | `/decks/{id}` | `UpdateDeckRequest`                  | `bool`                        | ✅   |
| DELETE | `/decks/{id}` | —                                    | `bool`                        | ✅   |

### DTOs

```typescript
// === Request ===
CreateDeckRequest {
  name: string
  description: string
  type: "Vocabulary" | "Grammar"    // enum gửi dạng string
}

UpdateDeckRequest {
  name: string
  description: string
}

SearchDeckQuery {                   // query params
  keyword: string
  type: string                      // "Vocabulary" | "Grammar" | ""
  page: number                      // default 1
  pageSize: number                  // default 20
}

// === Response ===
DeckSummaryDTO {                    // danh sách (GET /decks)
  id: string
  name: string
  type: "Vocabulary" | "Grammar"
  cardNumber: number
  author: { id: string, username: string }
}

DeckDTO {                           // chi tiết (GET /decks/{id})
  id: string
  name: string
  description: string
  type: "Vocabulary" | "Grammar"
  author: { id: string, username: string }
  cards: PreviewCardDTO[]           // danh sách thẻ preview
}

PreviewCardDTO {
  id: string
  term: string
  meaning: string
}
```

---

## 3. Vocabulary Card Flow — Thẻ từ vựng

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant API as Backend

    Note over FE,API: === TẠO CARD + EXAMPLES ===
    FE->>API: POST /vocabulary-cards
    Note right of FE: Body có sẵn mảng examples[]
    API-->>FE: { data: true }

    Note over FE,API: === DANH SÁCH CARD THEO DECK ===
    FE->>API: GET /vocabulary-cards/deck/{deckId}
    API-->>FE: { data: [VocabularyCardDTO, ...] }

    Note over FE,API: === XEM CHI TIẾT CARD ===
    FE->>API: GET /vocabulary-cards/{id}
    API-->>FE: { data: VocabularyCardDTO }

    Note over FE,API: === CẬP NHẬT CARD ===
    FE->>API: PUT /vocabulary-cards/{id}
    API-->>FE: { data: true }

    Note over FE,API: === XÓA CARD ===
    FE->>API: DELETE /vocabulary-cards/{id}
    API-->>FE: { data: true }
```

### Endpoints

| Method | Route                             | Body                          | Response              | Auth |
| ------ | --------------------------------- | ----------------------------- | --------------------- | ---- |
| POST   | `/vocabulary-cards`               | `CreateVocabularyRequest`     | `bool`                | ✅   |
| GET    | `/vocabulary-cards/deck/{deckId}` | —                             | `VocabularyCardDTO[]` | ✅   |
| GET    | `/vocabulary-cards/{id}`          | —                             | `VocabularyCardDTO`   | ✅   |
| PUT    | `/vocabulary-cards/{id}`          | `UpdateVocabularyCardRequest` | `bool`                | ✅   |
| DELETE | `/vocabulary-cards/{id}`          | —                             | `bool`                | ✅   |

### DTOs

```typescript
// === Request ===
CreateVocabularyRequest {
  term: string
  meaning: string
  deckId: string
  examples: CreateExampleSentenceRequest[]   // tạo kèm examples luôn
}

UpdateVocabularyCardRequest {
  term: string
  meaning: string
}

// === Response ===
VocabularyCardDTO {
  id: string
  term: string
  meaning: string
  deckId: string
  examples: ExampleSentenceDTO[]
}
```

---

## 4. Grammar Card Flow — Thẻ ngữ pháp

### Endpoints

| Method | Route                          | Body                       | Response           | Auth |
| ------ | ------------------------------ | -------------------------- | ------------------ | ---- |
| POST   | `/grammar-cards`               | `CreateGrammarCardRequest` | `bool`             | ✅   |
| GET    | `/grammar-cards/deck/{deckId}` | —                          | `GrammarCardDTO[]` | ✅   |
| GET    | `/grammar-cards/{id}`          | —                          | `GrammarCardDTO`   | ✅   |
| PUT    | `/grammar-cards/{id}`          | `UpdateGrammarCardRequest` | `bool`             | ✅   |
| DELETE | `/grammar-cards/{id}`          | —                          | `bool`             | ✅   |

### DTOs

```typescript
// === Request ===
CreateGrammarCardRequest {
  term: string
  meaning: string
  structure: string
  explanation?: string
  caution?: string
  deckId: string
  examples: CreateExampleSentenceRequest[]
}

UpdateGrammarCardRequest {
  term: string
  meaning: string
  structure: string
  explanation?: string
  caution?: string
}

// === Response ===
GrammarCardDTO {
  id: string
  term: string
  meaning: string
  structure: string
  explanation?: string
  caution?: string
  deckId?: string
  examples: ExampleSentenceDTO[]
}
```

---

## 5. Example Sentence Flow — Câu ví dụ

> Examples được tạo kèm Card (qua `CreateVocabularyRequest.examples` hoặc `CreateGrammarCardRequest.examples`).
> Sau đó có thể thêm/sửa/xóa riêng lẻ:

### Endpoints

| Method | Route            | Body                       | Response | Auth |
| ------ | ---------------- | -------------------------- | -------- | ---- |
| POST   | `/examples`      | `CreateCardExampleRequest` | `bool`   | ✅   |
| PUT    | `/examples/{id}` | `UpdateCardExampleRequest` | `bool`   | ✅   |
| DELETE | `/examples/{id}` | —                          | `bool`   | ✅   |

### DTOs

```typescript
// === Request ===
CreateCardExampleRequest {
  clozeSentence: string        // "Tôi ___ ăn cơm"
  expectedAnswer: string       // "đã"
  hint?: string
  vocabularyCardId?: string    // 1 trong 2 phải có giá trị
  grammarCardId?: string
}

CreateExampleSentenceRequest {  // dùng khi tạo kèm Card
  clozeSentence: string
  expectedAnswer: string
  hint?: string
}

UpdateCardExampleRequest {
  clozeSentence: string
  expectedAnswer: string
  hint?: string
}

// === Response ===
ExampleSentenceDTO {
  id: string
  clozeSentence: string
  expectedAnswer: string
  fullSentence: string          // câu đầy đủ đã điền answer
  hint?: string
}
```

---

## 6. Luồng tổng quan — Frontend Page Mapping

```mermaid
flowchart TD
    A[Login / Register Page] -->|Đăng nhập thành công| B[Dashboard]
    B --> C[Deck List Page]
    C -->|GET /decks| C
    C -->|Click deck| D[Deck Detail Page]
    D -->|GET /decks/id| D
    D -->|Click card| E{Deck Type?}
    E -->|Vocabulary| F[Vocabulary Card Detail]
    E -->|Grammar| G[Grammar Card Detail]
    F -->|GET /vocabulary-cards/id| F
    G -->|GET /grammar-cards/id| G

    C -->|Tạo deck mới| H[Create Deck Modal]
    H -->|POST /decks| C
    D -->|Tạo card mới| I[Create Card Form]
    I -->|POST /vocabulary-cards hoặc /grammar-cards| D
    F -->|Thêm example| J[Add Example]
    J -->|POST /examples| F
    G -->|Thêm example| J

    B --> K[Forgot Password]
    K -->|POST /auth/forget-password| K
```

## 7. Error Code Reference

| Code                | Message Constant        | Ý nghĩa                 | HTTP Code |
| ------------------- | ----------------------- | ----------------------- | --------- |
| `Common_404`        | `NOT_FOUND`             | Không tìm thấy resource | 200       |
| `Common_400`        | `INVALID`               | Dữ liệu không hợp lệ    | 200       |
| `Common_401`        | `UNAUTHORIZED`          | Chưa xác thực           | 401       |
| `Common_405`        | `NOT_ALLOW`             | Không có quyền (IDOR)   | 401       |
| `Common_505`        | `INTERNAL_SERVER_ERROR` | Lỗi server              | 500       |
| `Invalid_400`       | `INVALID_LOGIN`         | Sai email/password      | 200       |
| `Email_Exist_409`   | `EMAIL_EXIST`           | Email đã tồn tại        | 200       |
| `Token_Expried_409` | `TOKEN_EXPIRED`         | Token hết hạn           | 401       |

> **Frontend**: Check `response.success` trước. Nếu `false`, dùng `response.message` (error code) để hiển thị lỗi phù hợp bằng cách map sang ngôn ngữ (i18n).
