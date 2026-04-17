# Tacho Learning API — Frontend Integration Guide

> **Last updated:** 2026-04-17

---

## Mục lục

1. [Quy ước chung](#1-quy-ước-chung)
2. [Enum Reference](#2-enum-reference)
3. [Auth Module](#3-auth-module)
4. [Cards Module — User](#4-cards-module--user)
5. [Card Notes Module — User](#5-card-notes-module--user)
6. [Vocabulary Module — Admin](#6-vocabulary-module--admin)
7. [Grammar Module — Admin](#7-grammar-module--admin)
8. [Sentences Module — Admin](#8-sentences-module--admin)
9. [Uploads Module — Admin](#9-uploads-module--admin)
10. [Voicevox Module — Admin](#10-voicevox-module--admin)
11. [Kanji Module — Admin](#8-kanji-module--admin)
12. [Decks Module — User](#12-decks-module--user)
13. [Decks Module — Admin](#13-decks-module--admin)

---

## 1. Quy ước chung

### 1.1 Base URL

```
http://localhost:5062/api
```

### 1.2 Response envelope

Tất cả API trả về cùng một shape:

```json
{
  "code": 200,
  "success": true,
  "message": null,
  "data": { ... },
  "metaData": null
}
```

Khi có lỗi nghiệp vụ / validation, **HTTP vẫn trả 200** nhưng:

```json
{
  "code": 400,
  "success": false,
  "message": "Error_Code_400",
  "data": null,
  "metaData": null
}
```

Lỗi server không xử lý được → HTTP 500.

### 1.3 Pagination

Với các endpoint có phân trang, response sẽ kèm `metaData`:

```json
{
  "metaData": {
    "page": 1,
    "pageSize": 20,
    "total": 150,
    "totalPage": 8
  }
}
```

Query params phân trang mặc định:

| Param      | Type  | Default | Mô tả          |
| ---------- | ----- | ------- | -------------- |
| `page`     | `int` | `1`     | Trang hiện tại |
| `pageSize` | `int` | `20`    | Số item/trang  |

### 1.4 Authentication

- **Bearer Token**: Gửi `Authorization: Bearer <accessToken>` cho các endpoint yêu cầu auth.
- **Refresh Token**: Lưu trong **HttpOnly cookie** `refreshToken`, được server tự set/xóa.
- Các endpoint đánh dấu `🔒 Auth` cần access token hợp lệ.
- Các endpoint đánh dấu `🔑 Editor/Admin` chỉ dành cho role `editor` hoặc `admin` (frontend admin).
- Các endpoint đánh dấu `🌐 Public` không cần token.

### 1.5 Message code pattern

Lỗi được trả trong field `message` theo pattern:

```
<Module>_<ErrorName>_<HttpStatusCode>
```

Ví dụ: `Vocabulary_CardNotFound_404`, `Grammar_InvalidRichText_400`

Lỗi import field-level kèm fieldPath sau dấu `:`:

```
<Module>_Import<ErrorName>_<StatusCode>:<fieldPath>
```

Ví dụ: `Vocabulary_ImportFieldRequired_400:title`, `Grammar_ImportFieldInvalid_400:structures[0].pattern`

---

## 2. Enum Reference

Tất cả enum gửi/nhận dưới dạng **string** (case-sensitive).

### JlptLevel

| Value | Mô tả         |
| ----- | ------------- |
| `N5`  | Sơ cấp        |
| `N4`  | Sơ cấp trên   |
| `N3`  | Trung cấp     |
| `N2`  | Trung cao cấp |
| `N1`  | Cao cấp       |

### PublishStatus

| Value       | Mô tả                 |
| ----------- | --------------------- |
| `Draft`     | Bản nháp, chưa public |
| `Published` | Đã xuất bản, public   |
| `Archived`  | Đã xóa mềm            |

### CardType

| Value     | Mô tả        |
| --------- | ------------ |
| `Vocab`   | Thẻ từ vựng  |
| `Grammar` | Thẻ ngữ pháp |
| `Kanji`   | Thẻ Kanji    |

### UserRole

| Value    | Mô tả             |
| -------- | ----------------- |
| `user`   | Người dùng thường |
| `editor` | Biên tập viên     |
| `admin`  | Quản trị viên     |

### WordType (Vocabulary)

| Value          | Mô tả                            |
| -------------- | -------------------------------- |
| `Native`       | 和語 (Wago) — Từ thuần Nhật      |
| `SinoJapanese` | 漢語 (Kango) — Từ Hán-Nhật       |
| `Loanword`     | 外来語 (Gairaigo) — Từ ngoại lai |

### PartOfSpeech (Vocabulary)

| Value          | Mô tả                 |
| -------------- | --------------------- |
| `Noun`         | Danh từ               |
| `VerbU`        | Động từ nhóm 1 (五段) |
| `VerbRu`       | Động từ nhóm 2 (一段) |
| `IAdj`         | Tính từ đuôi い       |
| `NaAdj`        | Tính từ đuôi な       |
| `Adverb`       | Phó từ                |
| `Particle`     | Trợ từ                |
| `Conjunction`  | Liên từ               |
| `Interjection` | Thán từ               |

### RegisterType (Grammar)

| Value      | Mô tả       |
| ---------- | ----------- |
| `Standard` | Chuẩn       |
| `Formal`   | Trang trọng |
| `Polite`   | Lịch sự     |
| `Casual`   | Thân mật    |

### GrammarRelationType

| Value         | Mô tả               |
| ------------- | ------------------- |
| `Similar`     | Ngữ pháp tương tự   |
| `Contrasting` | Ngữ pháp tương phản |

---

## 3. Auth Module

> API xác thực và quản lý tài khoản người dùng.

### Tổng quan

| Method | Endpoint                    | Auth      | Mô tả                       |
| ------ | --------------------------- | --------- | --------------------------- |
| POST   | `/api/auth/register`        | 🌐 Public | Đăng ký tài khoản mới       |
| POST   | `/api/auth/login`           | 🌐 Public | Đăng nhập                   |
| POST   | `/api/auth/refresh-token`   | 🌐 Public | Làm mới access token        |
| POST   | `/api/auth/refresh`         | 🌐 Public | Alias của `refresh-token`   |
| GET    | `/api/auth/me`              | 🔒 Auth   | Lấy thông tin user hiện tại |
| PATCH  | `/api/auth/me/profile`      | 🔒 Auth   | Cập nhật profile            |
| POST   | `/api/auth/me/avatar`       | 🔒 Auth   | Upload avatar               |
| PATCH  | `/api/auth/change-password` | 🔒 Auth   | Đổi mật khẩu                |
| POST   | `/api/auth/logout`          | 🔒 Auth   | Đăng xuất                   |
| POST   | `/api/auth/forgot-password` | 🌐 Public | Gửi email reset password    |
| POST   | `/api/auth/reset-password`  | 🌐 Public | Xác nhận reset password     |

---

### POST `/api/auth/register`

Đăng ký tài khoản mới.

**Request body:**

```json
{
  "username": "string | null",
  "displayName": "string | null",
  "email": "string", // ⚠ bắt buộc
  "password": "string" // ⚠ bắt buộc
}
```

**Response data** (`AuthDTO`):

```json
{
  "accessToken": "jwt-string",
  "user": {
    "id": "string",
    "email": "string",
    "displayName": "string",
    "avatarUrl": "string | null",
    "role": "user",
    "createdAt": "datetime"
  }
}
```

> ℹ `refreshToken` được server tự set vào HttpOnly cookie, không nằm trong JSON response.

**Error codes:**

| Code              | Khi nào          |
| ----------------- | ---------------- |
| `Email_Exist_409` | Email đã tồn tại |

---

### POST `/api/auth/login`

Đăng nhập.

**Request body:**

```json
{
  "email": "string", // ⚠ bắt buộc
  "password": "string" // ⚠ bắt buộc
}
```

**Response data:** Cùng shape `AuthDTO` như register.

**Error codes:**

| Code          | Khi nào                 |
| ------------- | ----------------------- |
| `Invalid_400` | Sai email hoặc password |

---

### POST `/api/auth/refresh-token`

Làm mới access token. Backend đọc `refreshToken` từ cookie.

- Không cần gửi request body.
- Response: `AuthDTO` (access token mới + user info).

**Error codes:**

| Code                | Khi nào                                 |
| ------------------- | --------------------------------------- |
| `Token_Expired_409` | Refresh token hết hạn hoặc không hợp lệ |

---

### POST `/api/auth/refresh`

Alias của `/api/auth/refresh-token`. Hoạt động giống hệt.

---

### GET `/api/auth/me` 🔒

Lấy thông tin user đang đăng nhập.

**Response data** (`AuthUserDTO`):

```json
{
  "id": "string",
  "email": "string",
  "displayName": "string",
  "avatarUrl": "string | null",
  "role": "user | editor | admin",
  "createdAt": "datetime"
}
```

---

### PATCH `/api/auth/me/profile` 🔒

Cập nhật thông tin profile.

**Request body:**

```json
{
  "displayName": "string",
  "avatarUrl": "string | null"
}
```

**Response data:** `AuthUserDTO`

---

### POST `/api/auth/me/avatar` 🔒

Upload ảnh avatar mới cho user hiện tại.

- **Content-Type:** `multipart/form-data`
- **Form field:** `avatar`
- **Allowed MIME:** `image/jpeg`, `image/png`, `image/webp`
- **Max size:** `5 MB`

**Response data:** `AuthUserDTO`

---

### PATCH `/api/auth/change-password` 🔒

Đổi mật khẩu.

**Request body:**

```json
{
  "currentPassword": "string", // ⚠ bắt buộc
  "newPassword": "string" // ⚠ bắt buộc
}
```

**Response data:** `true`

**Error codes:**

| Code                         | Khi nào               |
| ---------------------------- | --------------------- |
| `Wrong_Current_Password_400` | Sai mật khẩu hiện tại |

---

### POST `/api/auth/logout` 🔒

Đăng xuất. Backend xóa refresh token cookie.

**Response data:** `true`

---

### POST `/api/auth/forgot-password`

Gửi email chứa link reset password.

**Request body:**

```json
{
  "email": "string" // ⚠ bắt buộc
}
```

**Response data:** `true`

---

### POST `/api/auth/reset-password`

Xác nhận reset password bằng token nhận từ email.

**Request body:**

```json
{
  "token": "string", // ⚠ bắt buộc, token từ email
  "newPassword": "string" // ⚠ bắt buộc
}
```

**Response data:** `true`

**Error codes:**

| Code                | Khi nào                         |
| ------------------- | ------------------------------- |
| `Token_Expired_409` | Token hết hạn hoặc không hợp lệ |

---

## 4. Cards Module — User

> API search card tổng hợp dành cho **frontend user**. Gộp kết quả Vocabulary + Grammar + Kanji.

### Tổng quan

| Method | Endpoint             | Auth      | Mô tả                |
| ------ | -------------------- | --------- | -------------------- |
| GET    | `/api/cards/search`  | 🌐 Public | Search card tổng hợp |
| GET    | `/api/kanji/{cardId}`| 🌐 Public | Lấy chi tiết kanji   |

---

### GET `/api/cards/search`

Tìm kiếm card đã Published cho user. Gộp cả Vocabulary + Grammar + Kanji.

**Query params:**

| Param      | Type     | Bắt buộc | Enum                          | Mô tả                                   |
| ---------- | -------- | -------- | ----------------------------- | --------------------------------------- |
| `cardType` | `string` | ❌       | `Vocab`, `Grammar`, `Kanji`   | Lọc theo loại card. Bỏ trống = tìm cả 3 |
| `q`        | `string` | ❌       | —                             | Từ khóa tìm kiếm                        |
| `level`    | `string` | ❌       | `JlptLevel`                   | Lọc theo trình độ JLPT                  |
| `page`     | `int`    | ❌       | —                             | Mặc định `1`                            |
| `pageSize` | `int`    | ❌       | —                             | Mặc định `20`                           |

**Quy tắc search:**

- Backend **chỉ trả card có `status = Published`**.
- Nếu không truyền `cardType`, kết quả được gộp rồi sort theo `updatedAt ?? createdAt` giảm dần.
- `q` tìm theo:
  - **Vocabulary:** `title`, `summary`, `writing`, `reading`
  - **Grammar:** `title`, `summary`, `alternateForms`, `structures.pattern` (KHÔNG search trong `explanation`)
  - **Kanji:** `title`, `summary`, `kanji`, `meaningVi`, `hanViet`

**Response data item:**

```json
{
  "id": "string",
  "cardType": "Vocab | Grammar | Kanji",
  "title": "string",
  "summary": "string",
  "level": "N5 | N4 | N3 | N2 | N1 | null",
  "alternateForms": ["〜てからです"]
}
```

> ℹ `alternateForms` chỉ có dữ liệu khi `cardType = Grammar`. Với `Vocab` và `Kanji` luôn trả `[]`.

---

## 5. Card Notes Module — User

> API ghi chú cộng đồng cho thẻ học. Tất cả endpoint yêu cầu **đăng nhập**.  
> Áp dụng cho cả Vocabulary và Grammar card.

### Tổng quan

| Method | Endpoint                          | Auth    | Mô tả                         |
| ------ | --------------------------------- | ------- | ----------------------------- |
| GET    | `/api/cards/{cardId}/notes`       | 🔒 Auth | Lấy danh sách community notes |
| POST   | `/api/cards/{cardId}/notes`       | 🔒 Auth | Tạo/cập nhật ghi chú cá nhân  |
| DELETE | `/api/cards/{cardId}/notes/me`    | 🔒 Auth | Xóa ghi chú cá nhân           |
| POST   | `/api/notes/{noteId}/toggle-like` | 🔒 Auth | Bật/tắt like cho một note     |

---

### GET `/api/cards/{cardId}/notes` 🔒

Lấy danh sách community notes có phân trang.

**Path params:**

| Param    | Type     | Mô tả                            |
| -------- | -------- | -------------------------------- |
| `cardId` | `string` | ID của card (vocab hoặc grammar) |

**Query params:**

| Param      | Type  | Default | Mô tả          |
| ---------- | ----- | ------- | -------------- |
| `page`     | `int` | `1`     | Trang hiện tại |
| `pageSize` | `int` | `10`    | Số note/trang  |

**Response data item** (`CardNoteResponse`):

```json
{
  "id": "string",
  "userId": "string",
  "userName": "string",
  "content": "string",
  "likesCount": 3,
  "isLikedByMe": false,
  "createdAt": "datetime"
}
```

---

### POST `/api/cards/{cardId}/notes` 🔒

Tạo mới hoặc cập nhật ghi chú **của chính user** cho card.

- Mỗi user chỉ có **1 note** duy nhất trên mỗi card.
- Nếu đã có note → cập nhật content.
- Nếu chưa có → tạo mới.

**Request body:**

```json
{
  "content": "string" // ⚠ bắt buộc, nội dung ghi chú
}
```

**Response data:** `CardNoteResponse`

---

### DELETE `/api/cards/{cardId}/notes/me` 🔒

Xóa ghi chú **của chính user** trên card.

**Response data:** `true`

---

### POST `/api/notes/{noteId}/toggle-like` 🔒

Bật/tắt trạng thái like cho một ghi chú.

**Path params:**

| Param    | Type     | Mô tả                   |
| -------- | -------- | ----------------------- |
| `noteId` | `string` | ID note cần toggle like |

**Response data:**

```json
{
  "isLiked": true, // trạng thái like mới
  "likesCount": 4 // tổng số like hiện tại
}
```

---

## 6. Vocabulary Module — Admin

> 🔑 **Tất cả endpoint trong module này yêu cầu quyền `Editor` hoặc `Admin`.**  
> Trừ `GET /api/vocabulary/{cardId}` là endpoint Public dùng cho cả user lẫn admin.

### Tổng quan

| Method | Endpoint                          | Auth            | Mô tả                             |
| ------ | --------------------------------- | --------------- | --------------------------------- |
| GET    | `/api/vocabulary`                 | 🔑 Editor/Admin | Tìm kiếm vocabulary có phân trang |
| GET    | `/api/vocabulary/{cardId}`        | 🌐 Public       | Lấy chi tiết vocabulary           |
| POST   | `/api/vocabulary`                 | 🔑 Editor/Admin | Tạo vocabulary mới                |
| PATCH  | `/api/vocabulary/{cardId}`        | 🔑 Editor/Admin | Cập nhật vocabulary               |
| DELETE | `/api/vocabulary/{cardId}`        | 🔑 Editor/Admin | Xóa mềm vocabulary (Archived)     |
| GET    | `/api/vocabulary/import-template` | 🔑 Editor/Admin | Tải JSON template import          |
| GET    | `/api/vocabulary/export`          | 🔑 Editor/Admin | Export vocabulary ra JSON         |
| POST   | `/api/vocabulary/import/preview`  | 🔑 Editor/Admin | Preview import, chưa ghi DB       |
| POST   | `/api/vocabulary/import/commit`   | 🔑 Editor/Admin | Commit batch import               |

---

### GET `/api/vocabulary` 🔑

Tìm kiếm danh sách vocabulary cho admin.

**Query params:**

| Param         | Type     | Bắt buộc | Enum            | Mô tả                                             |
| ------------- | -------- | -------- | --------------- | ------------------------------------------------- |
| `q`           | `string` | ❌       | —               | Tìm theo `title`, `summary`, `writing`, `reading` |
| `level`       | `string` | ❌       | `JlptLevel`     | Lọc theo trình độ                                 |
| `status`      | `string` | ❌       | `PublishStatus` | Lọc theo trạng thái                               |
| `wordType`    | `string` | ❌       | `WordType`      | Lọc theo loại từ                                  |
| `hasAudio`    | `bool`   | ❌       | —               | `true`/`false` lọc có audio hay không             |
| `createdByMe` | `bool`   | ❌       | —               | `true` = chỉ lấy card do mình tạo                 |
| `page`        | `int`    | ❌       | —               | Mặc định `1`                                      |
| `pageSize`    | `int`    | ❌       | —               | Mặc định `20`                                     |

**Response data item:**

```json
{
  "id": "string",
  "title": "string",
  "summary": "string",
  "level": "N5 | null",
  "tags": ["verb"],
  "status": "Draft | Published | Archived",
  "createdAt": "datetime",
  "updatedAt": "datetime | null",
  "writing": "食べる",
  "reading": "たべる | null",
  "wordType": "Native | SinoJapanese | Loanword | null"
}
```

---

### GET `/api/vocabulary/{cardId}` 🌐

Lấy chi tiết vocabulary card.

**Quy tắc truy cập:**

- ✅ Card `Published`: ai cũng xem được (public).
- ⚠ Card `Draft` / `Archived`: chỉ user tạo card mới xem được.

**Response data** (`VocabularyDetailResponse`):

```json
{
  "id": "string",
  "cardType": "Vocab",
  "title": "食べる",
  "summary": "Động từ ăn",
  "level": "N5 | null",
  "tags": ["verb"],
  "status": "Published | Draft | Archived",
  "createdAt": "datetime",
  "updatedAt": "datetime | null",
  "writing": "食べる",
  "reading": "たべる | null",
  "pitchPattern": [0, 1, 0],
  "audioUrl": "string | null",
  "speakerId": 3,
  "wordType": "Native | SinoJapanese | Loanword | null",
  "meanings": [
    {
      "partOfSpeech": "VerbRu",
      "definitions": ["ăn", "dùng bữa"]
    }
  ],
  "synonyms": ["食事する"],
  "antonyms": [],
  "relatedPhrases": ["ご飯を食べる"],
  "sentences": [
    {
      "id": "sentence-id",
      "text": "毎朝パンを食べる。",
      "meaning": "Mỗi sáng tôi ăn bánh mì.",
      "audioUrl": "https://cdn.example.com/audio/sentence.wav",
      "level": "N5"
    }
  ],
  "userNotes": [
    {
      "id": "note-id",
      "userId": "user-id",
      "userName": "Tran Thi B",
      "content": "Từ cơ bản, nên thuộc ở N5.",
      "likesCount": 5,
      "isLikedByMe": true,
      "createdAt": "datetime"
    }
  ]
}
```

**Field details:**

| Field                     | Type       | Enum            | Mô tả                                                      |
| ------------------------- | ---------- | --------------- | ---------------------------------------------------------- |
| `cardType`                | `string`   | `CardType`      | Luôn là `Vocab`                                            |
| `level`                   | `string?`  | `JlptLevel`     | Nullable                                                   |
| `status`                  | `string`   | `PublishStatus` |                                                            |
| `wordType`                | `string?`  | `WordType`      | Nullable                                                   |
| `pitchPattern`            | `int[]?`   | —               | Mảng pitch accent, mỗi phần tử = `0` (thấp) hoặc `1` (cao) |
| `audioUrl`                | `string?`  | —               | URL file audio, do backend tự generate bằng VOICEVOX       |
| `speakerId`               | `int?`     | —               | ID speaker VOICEVOX                                        |
| `meanings[].partOfSpeech` | `string`   | `PartOfSpeech`  | Từ loại                                                    |
| `meanings[].definitions`  | `string[]` | —               | Danh sách nghĩa                                            |

**Error codes:**

| Code                           | Khi nào                                      |
| ------------------------------ | -------------------------------------------- |
| `Vocabulary_CardNotFound_404`  | Card không tồn tại                           |
| `Vocabulary_ReadForbidden_401` | Card chưa Published và user không phải owner |

---

### POST `/api/vocabulary` 🔑

Tạo mới một vocabulary card.

**Lưu ý VOICEVOX-only:**

- ❌ Client **không gửi** `audioUrl`.
- ✅ Backend tự generate audio bằng VOICEVOX từ `reading` (fallback `writing` nếu rỗng).
- `speakerId` là ID speaker dùng generate, được lưu DB.
- `pitchPattern` nếu gửi sẽ override pitch mặc định.

**Request body:**

```json
{
  "title": "食べる", // ⚠ bắt buộc
  "summary": "Động từ ăn", // ⚠ bắt buộc
  "level": "N5", // ❌ nullable — enum JlptLevel
  "tags": ["verb"], // ❌ optional, mảng string
  "status": "Draft", // ❌ nullable — enum PublishStatus
  "writing": "食べる", // ⚠ bắt buộc
  "reading": "たべる", // ❌ nullable
  "pitchPattern": [0, 1, 0], // ❌ nullable, mảng int (0=thấp, 1=cao)
  "speakerId": 3, // ❌ nullable, int
  "wordType": "Native", // ❌ nullable — enum WordType
  "meanings": [
    // ⚠ bắt buộc, ít nhất 1 item
    {
      "partOfSpeech": "VerbRu", // ⚠ bắt buộc — enum PartOfSpeech
      "definitions": ["ăn", "dùng bữa"] // ⚠ bắt buộc, ít nhất 1 item
    }
  ],
  "synonyms": [], // ❌ optional
  "antonyms": [], // ❌ optional
  "relatedPhrases": [], // ❌ optional
  "sentences": [
    // ❌ optional, nested upsert
    {
      "id": "existing-sentence-id", // ❌ có id → update sentence, không id → tạo mới
      "text": "毎朝パンを食べる。", // ⚠ bắt buộc
      "meaning": "Mỗi sáng ăn bánh mì.", // ⚠ bắt buộc
      "speakerId": 3, // ❌ nullable
      "level": "N5" // ❌ nullable — enum JlptLevel
    }
  ]
}
```

**Response data:** `VocabularyDetailResponse` (full detail của card vừa tạo)

---

### PATCH `/api/vocabulary/{cardId}` 🔑

Cập nhật vocabulary card. Body giống `POST`.

**⚠ Quy tắc `sentences`:**

- Danh sách `sentences` gửi lên = **trạng thái cuối cùng**.
- Sentence nào **không có** trong request → bị gỡ khỏi vocabulary.
- Sentence có `id` → update, không có `id` → tạo mới + generate audio VOICEVOX.

**Response data:** `VocabularyDetailResponse`

---

### DELETE `/api/vocabulary/{cardId}` 🔑

Xóa mềm vocabulary card (chuyển `status = Archived`).

**Response data:** `true`

---

### GET `/api/vocabulary/import-template` 🔑

Tải file JSON template mẫu cho import vocabulary.

- Response: file `application/json` (`Content-Disposition: attachment`).
- JSON dùng `camelCase`.
- Có thêm object `guide` để mô tả `allowedValues` và `fieldNotes` cho các field quan trọng.
- Payload import thực tế vẫn nằm trong `items` (shape cùng với request body của `import/preview`).

---

### GET `/api/vocabulary/export` 🔑

Export vocabulary ra file JSON theo bộ lọc.

**Query params:**

| Param         | Type     | Enum            | Mô tả   |
| ------------- | -------- | --------------- | ------- |
| `q`           | `string` | —               | Từ khóa |
| `level`       | `string` | `JlptLevel`     |         |
| `status`      | `string` | `PublishStatus` |         |
| `wordType`    | `string` | `WordType`      |         |
| `hasAudio`    | `bool`   | —               |         |
| `createdByMe` | `bool`   | —               |         |

- Response: file `application/json` với shape tương tự import payload.

---

### POST `/api/vocabulary/import/preview` 🔑

Preview payload import. Validate từng item, **chưa ghi vào DB**.

**Import rules:**

- Import hiện tại là **create-only** (chỉ tạo mới).
- `sentences[*].id` **KHÔNG được gửi** (vì create-only).
- `writing` không được trùng trong batch + không trùng DB.

**Request body:**

```json
{
  "items": [
    {
      "rowNumber": 1, // ⚠ số thứ tự hàng
      "title": "食べる",
      "summary": "Động từ ăn",
      "level": "N5",
      "tags": ["verb"],
      "status": "Draft",
      "writing": "食べる",
      "reading": "たべる",
      "pitchPattern": [0, 1, 0],
      "speakerId": 3,
      "wordType": "Native",
      "meanings": [{ "partOfSpeech": "VerbRu", "definitions": ["ăn"] }],
      "synonyms": [],
      "antonyms": [],
      "relatedPhrases": [],
      "sentences": [
        {
          "text": "毎朝パンを食べる。",
          "meaning": "Mỗi sáng ăn bánh mì.",
          "speakerId": 3,
          "level": "N5"
        }
      ]
    }
  ]
}
```

**Response data:**

```json
{
  "totalItems": 1,
  "validItems": 1,
  "invalidItems": 0,
  "items": [
    {
      "rowNumber": 1,
      "title": "食べる",
      "writing": "食べる",
      "isValid": true,
      "errors": [],
      "warnings": []
    }
  ]
}
```

**Ví dụ response lỗi:**

```json
{
  "totalItems": 1,
  "validItems": 0,
  "invalidItems": 1,
  "items": [
    {
      "rowNumber": 1,
      "title": "食べる",
      "writing": "食べる",
      "isValid": false,
      "errors": [
        "Vocabulary_ImportWritingAlreadyExists_400",
        "Vocabulary_ImportSentenceIdNotAllowed_400:sentences[0].id"
      ],
      "warnings": []
    }
  ]
}
```

**Error codes cho import:**

| Code                                                | Mô tả                                       |
| --------------------------------------------------- | ------------------------------------------- |
| `Vocabulary_ImportInvalidPayload_400`               | Payload tổng thể không hợp lệ               |
| `Vocabulary_ImportBatchHasErrors_400`               | Batch còn item lỗi, không commit            |
| `Vocabulary_ImportFieldRequired_400:<field>`        | Field bắt buộc bị thiếu                     |
| `Vocabulary_ImportFieldTooLong_400:<field>`         | Field vượt quá độ dài cho phép              |
| `Vocabulary_ImportFieldInvalid_400:<field>`         | Giá trị enum không hợp lệ                   |
| `Vocabulary_ImportDuplicateWritingInBatch_400`      | `writing` trùng trong batch                 |
| `Vocabulary_ImportWritingAlreadyExists_400`         | `writing` đã có trong DB                    |
| `Vocabulary_ImportSentenceIdNotAllowed_400:<field>` | Không được gửi `sentences[*].id` khi import |
| `Vocabulary_ImportMeaningsRequired_400`             | Thiếu `meanings`                            |
| `Vocabulary_ImportDefinitionsRequired_400`          | Thiếu `definitions` trong meaning           |
| `Vocabulary_ImportSpeakerIdNotSupported_400`        | `speakerId` không hợp lệ                    |
| `Vocabulary_ImportListTooManyItems_400`             | Vượt quá số item cho phép                   |
| `Vocabulary_ImportSentencesTooMany_400`             | Quá nhiều sentences                         |
| `Vocabulary_ImportRowNumberInvalid_400`             | `rowNumber` không hợp lệ                    |

---

### POST `/api/vocabulary/import/commit` 🔑

Commit batch import vào DB.

**Quy trình:**

1. Backend chạy `preview` nội bộ trước.
2. Nếu còn item invalid → **không ghi DB**, trả lỗi `Vocabulary_ImportBatchHasErrors_400`.
3. Nếu tất cả hợp lệ → tạo tuần tự từng vocabulary card mới.

**Request body:** Cùng shape với `import/preview`.

**Response data:**

```json
{
  "totalItems": 2,
  "successfulItems": 2,
  "failedItems": 0,
  "hasValidationErrors": false,
  "items": [
    {
      "rowNumber": 1,
      "title": "食べる",
      "writing": "食べる",
      "isSuccess": true,
      "action": "created",
      "cardId": "new-card-id",
      "errors": []
    }
  ]
}
```

---

## 7. Grammar Module — Admin

> 🔑 **Tất cả endpoint trong module này yêu cầu quyền `Editor` hoặc `Admin`.**  
> Trừ `GET /api/grammar/{cardId}` là endpoint Public dùng cho cả user lẫn admin.

### Tổng quan

| Method | Endpoint                       | Auth            | Mô tả                          |
| ------ | ------------------------------ | --------------- | ------------------------------ |
| GET    | `/api/grammar`                 | 🔑 Editor/Admin | Tìm kiếm grammar có phân trang |
| GET    | `/api/grammar/{cardId}`        | 🌐 Public       | Lấy chi tiết grammar           |
| POST   | `/api/grammar`                 | 🔑 Editor/Admin | Tạo grammar mới                |
| PATCH  | `/api/grammar/{cardId}`        | 🔑 Editor/Admin | Cập nhật grammar               |
| DELETE | `/api/grammar/{cardId}`        | 🔑 Editor/Admin | Xóa mềm grammar (Archived)     |
| GET    | `/api/grammar/import-template` | 🔑 Editor/Admin | Tải JSON template import       |
| GET    | `/api/grammar/export`          | 🔑 Editor/Admin | Export grammar ra JSON         |
| POST   | `/api/grammar/import/preview`  | 🔑 Editor/Admin | Preview import, chưa ghi DB    |
| POST   | `/api/grammar/import/commit`   | 🔑 Editor/Admin | Commit batch import            |

---

### GET `/api/grammar` 🔑

Tìm kiếm danh sách grammar cho admin.

**Query params:**

| Param         | Type     | Bắt buộc | Enum            | Mô tả                                                                                                  |
| ------------- | -------- | -------- | --------------- | ------------------------------------------------------------------------------------------------------ |
| `q`           | `string` | ❌       | —               | Tìm theo `title`, `summary`, `alternateForms`, `structures.pattern`. **KHÔNG** tìm trong `explanation` |
| `level`       | `string` | ❌       | `JlptLevel`     |                                                                                                        |
| `status`      | `string` | ❌       | `PublishStatus` |                                                                                                        |
| `register`    | `string` | ❌       | `RegisterType`  |                                                                                                        |
| `createdByMe` | `bool`   | ❌       | —               |                                                                                                        |
| `page`        | `int`    | ❌       | —               | Mặc định `1`                                                                                           |
| `pageSize`    | `int`    | ❌       | —               | Mặc định `20`                                                                                          |

**Response data item:**

```json
{
  "id": "string",
  "title": "〜ながら",
  "summary": "Vừa làm A vừa làm B.",
  "level": "N4 | null",
  "tags": ["grammar", "simultaneous"],
  "status": "Draft | Published | Archived",
  "createdAt": "datetime",
  "updatedAt": "datetime | null",
  "register": "Standard | Formal | Polite | Casual | null",
  "structuresCount": 1,
  "alternateForms": ["〜つつ"]
}
```

---

### GET `/api/grammar/{cardId}` 🌐

Lấy chi tiết grammar card.

**Quy tắc truy cập:**

- ✅ Card `Published`: public.
- ⚠ Card `Draft` / `Archived`: chỉ owner xem được.

**Response data** (`GrammarDetailResponse`):

```json
{
  "id": "grammar-card-id",
  "cardType": "Grammar",
  "title": "〜ながら",
  "summary": "Vừa làm A vừa làm B.",
  "level": "N4 | null",
  "tags": ["grammar", "simultaneous"],
  "status": "Published | Draft | Archived",
  "createdAt": "datetime",
  "updatedAt": "datetime | null",
  "structures": [
    {
      "pattern": "V1(1) + ながら + V2(2)",
      "annotations": {
        "1": "Hành động phụ diễn ra đồng thời",
        "2": "Hành động chính"
      }
    }
  ],
  "explanation": "Dùng khi chủ thể vừa làm A vừa làm B.",
  "caution": "Hai hành động cần cùng chủ thể.",
  "register": "Standard | Formal | Polite | Casual | null",
  "alternateForms": ["〜つつ"],
  "relations": [
    {
      "relatedId": "grammar-card-001",
      "title": "〜てから",
      "summary": "Làm A rồi mới làm B.",
      "relationType": "Similar | Contrasting"
    }
  ],
  "resources": [
    {
      "id": "resource-id",
      "title": "Bài giảng",
      "url": "https://example.com/grammar/nagara"
    }
  ],
  "sentences": [
    {
      "id": "sentence-id",
      "text": "音楽を聞きながら勉強します。",
      "meaning": "Tôi vừa nghe nhạc vừa học.",
      "audioUrl": "https://cdn.example.com/audio/sentence.wav",
      "level": "N4"
    }
  ],
  "userNotes": [
    {
      "id": "note-id",
      "userId": "user-id",
      "userName": "Nguyen Van A",
      "content": "Mẫu này hay đi với hành động kéo dài.",
      "likesCount": 3,
      "isLikedByMe": false,
      "createdAt": "datetime"
    }
  ]
}
```

**Field details:**

| Field                      | Type      | Enum                  | Mô tả                                     |
| -------------------------- | --------- | --------------------- | ----------------------------------------- |
| `cardType`                 | `string`  | `CardType`            | Luôn là `Grammar`                         |
| `structures[].pattern`     | `string`  | —                     | Mẫu cấu trúc, hỗ trợ **rich text**        |
| `structures[].annotations` | `object?` | —                     | Key = số thứ tự `(1)`, value = chú thích  |
| `explanation`              | `string?` | —                     | Giải thích chi tiết, hỗ trợ **rich text** |
| `caution`                  | `string?` | —                     | Lưu ý, hỗ trợ **rich text**               |
| `register`                 | `string?` | `RegisterType`        | Ngữ cảnh sử dụng                          |
| `relations[].relationType` | `string`  | `GrammarRelationType` | `Similar` hoặc `Contrasting`              |

**Error codes:**

| Code                        | Khi nào                                      |
| --------------------------- | -------------------------------------------- |
| `Grammar_CardNotFound_404`  | Card không tồn tại                           |
| `Grammar_ReadForbidden_401` | Card chưa Published và user không phải owner |

---

### Rich Text Rules (Markdown subset)

Áp dụng cho các field: `structures[].pattern`, `structures[].annotations[*]`, `explanation`, `caution`.

**Cho phép:**

| Syntax              | Mô tả              |
| ------------------- | ------------------ |
| `**bold**`          | In đậm             |
| `*italic*`          | In nghiêng         |
| `~~strikethrough~~` | Gạch ngang         |
| `{u}text{/u}`       | Gạch chân          |
| `{red}text{/red}`   | Tô màu (whitelist) |

**Màu whitelist:** `red`, `blue`, `green`, `yellow`, `orange`, `purple`, `gray`

**KHÔNG cho phép:**

- Raw HTML (`<tag>...</tag>`)
- Token sai cú pháp hoặc không đóng cặp
- Token màu ngoài whitelist

**Giới hạn độ dài:**

| Field                         | Max ký tự   |
| ----------------------------- | ----------- |
| `structures[].pattern`        | 1,000       |
| `structures[].annotations[*]` | 1,000/value |
| `explanation`                 | 10,000      |
| `caution`                     | 5,000       |

---

### POST `/api/grammar` 🔑

Tạo mới grammar card.

**Request body:**

```json
{
  "title": "〜てから", // ⚠ bắt buộc
  "summary": "Diễn tả hành động B xảy ra sau", // ⚠ bắt buộc
  "level": "N5", // ❌ nullable — enum JlptLevel
  "tags": ["grammar", "sequence"], // ❌ optional
  "status": "Draft", // ❌ nullable — enum PublishStatus
  "structures": [
    // ❌ optional
    {
      "pattern": "**V[て形]** + から", // rich text allowed
      "annotations": {
        // ❌ nullable
        "1": "Hành động trước"
      }
    }
  ],
  "explanation": "Dùng khi hành động sau xảy ra sau khi hành động trước hoàn tất.", // ❌ nullable, rich text
  "caution": "~~Không~~ dùng cho hai hành động đồng thời.", // ❌ nullable, rich text
  "register": "Standard", // ❌ nullable — enum RegisterType
  "alternateForms": ["〜てからです"], // ❌ optional
  "relations": [
    // ❌ optional
    {
      "relatedId": "grammar-card-id-1", // ⚠ bắt buộc, ID card ngữ pháp liên quan
      "relationType": "Similar" // ⚠ bắt buộc — enum GrammarRelationType
    }
  ],
  "resources": [
    // ❌ optional
    {
      "title": "Bài giảng", // ⚠ bắt buộc
      "url": "https://example.com/te-kara" // ⚠ bắt buộc
    }
  ],
  "sentences": [
    // ❌ optional, nested upsert
    {
      "id": "optional-existing-id", // ❌ có id → update, không id → tạo mới
      "text": "ご飯を食べてから、勉強します。", // ⚠ bắt buộc
      "meaning": "Ăn cơm xong rồi học.", // ⚠ bắt buộc
      "speakerId": 3, // ❌ nullable
      "level": "N5" // ❌ nullable — enum JlptLevel
    }
  ]
}
```

**Response data:** `GrammarDetailResponse`

**Error codes:**

| Code                              | Khi nào                                              |
| --------------------------------- | ---------------------------------------------------- |
| `Grammar_InvalidRichText_400`     | Rich text pattern/explanation/caution sai cú pháp    |
| `Grammar_RelatedCardNotFound_404` | `relatedId` không tìm thấy grammar card              |
| `Grammar_InvalidRelation_400`     | Relation không hợp lệ (VD: tự tham chiếu chính mình) |

---

### PATCH `/api/grammar/{cardId}` 🔑

Cập nhật grammar card. Body giống `POST`.

**⚠ Quy tắc `sentences`:**

- Danh sách `sentences` gửi lên = **trạng thái cuối cùng**.
- Sentence nào KHÔNG có trong request → bị gỡ association.

**Response data:** `GrammarDetailResponse`

---

### DELETE `/api/grammar/{cardId}` 🔑

Xóa mềm grammar card (chuyển `status = Archived`).

**Response data:** `true`

---

### GET `/api/grammar/import-template` 🔑

Tải file JSON template mẫu cho import grammar.

- Response: file `application/json` (`Content-Disposition: attachment`).
- JSON dùng `camelCase`.
- Có thêm object `guide` để mô tả `allowedValues` và `fieldNotes` cho các field quan trọng.

---

### GET `/api/grammar/export` 🔑

Export grammar ra file JSON.

**Query params:**

| Param         | Type     | Enum            | Mô tả   |
| ------------- | -------- | --------------- | ------- |
| `q`           | `string` | —               | Từ khóa |
| `level`       | `string` | `JlptLevel`     |         |
| `status`      | `string` | `PublishStatus` |         |
| `register`    | `string` | `RegisterType`  |         |
| `createdByMe` | `bool`   | —               |         |

- Response: file `application/json` cùng shape với import payload.

---

### POST `/api/grammar/import/preview` 🔑

Preview payload import grammar, validate từng item, **chưa ghi DB**.

**Import rules:**

- Import hiện tại là **create-only**.
- `sentences[*].id` **KHÔNG được gửi** (vì create-only).

**Request body:**

```json
{
  "guide": {
    "jsonNamingConvention": "camelCase",
    "allowedValues": {
      "level": ["N5", "N4", "N3", "N2", "N1"],
      "speakerId": ["2", "3", "8", "10", "11"]
    },
    "fieldNotes": {
      "items": "Danh sách bản ghi import.",
      "rowNumber": "Số dòng trong file import, dùng để đối chiếu lỗi."
    }
  },
  "items": [
    {
      "rowNumber": 1,
      "title": "〜ながら",
      "summary": "Vừa làm A vừa làm B.",
      "level": "N4",
      "tags": ["grammar"],
      "status": "Draft",
      "structures": [
        {
          "pattern": "V1(1) + ながら + V2(2)",
          "annotations": { "1": "Hành động phụ", "2": "Hành động chính" }
        }
      ],
      "explanation": "Dùng khi chủ thể vừa làm A vừa làm B.",
      "caution": "Hai hành động cần cùng chủ thể.",
      "register": "Standard",
      "alternateForms": ["〜つつ"],
      "relations": [
        { "relatedId": "grammar-card-001", "relationType": "Similar" }
      ],
      "resources": [
        { "title": "Bài giảng", "url": "https://example.com/nagara" }
      ],
      "sentences": [
        {
          "text": "音楽を聞きながら勉強します。",
          "meaning": "Vừa nghe nhạc vừa học.",
          "speakerId": 3,
          "level": "N4"
        }
      ]
    }
  ]
}
```

**Response data:**

```json
{
  "totalItems": 1,
  "validItems": 1,
  "invalidItems": 0,
  "items": [
    {
      "rowNumber": 1,
      "title": "〜ながら",
      "isValid": true,
      "errors": [],
      "warnings": []
    }
  ]
}
```

**Error codes cho import:**

| Code                                               | Mô tả                            |
| -------------------------------------------------- | -------------------------------- |
| `Grammar_ImportInvalidPayload_400`                 | Payload tổng thể không hợp lệ    |
| `Grammar_ImportBatchHasErrors_400`                 | Batch còn item lỗi               |
| `Grammar_ImportFieldRequired_400:<field>`          | Field bắt buộc bị thiếu          |
| `Grammar_ImportFieldTooLong_400:<field>`           | Field vượt quá giới hạn          |
| `Grammar_ImportFieldInvalid_400:<field>`           | Enum/giá trị không hợp lệ        |
| `Grammar_ImportRelatedGrammarNotFound_404:<field>` | `relatedId` không tìm thấy       |
| `Grammar_ImportDuplicateRelation_400:<field>`      | Relation trùng lặp               |
| `Grammar_ImportSentenceIdNotAllowed_400:<field>`   | Không được gửi `sentences[*].id` |
| `Grammar_ImportSpeakerIdNotSupported_400`          | `speakerId` không hợp lệ         |
| `Grammar_ImportListTooManyItems_400`               | Quá số item cho phép             |
| `Grammar_ImportSentencesTooMany_400`               | Quá nhiều sentences              |
| `Grammar_ImportRowNumberInvalid_400`               | `rowNumber` không hợp lệ         |

---

### POST `/api/grammar/import/commit` 🔑

Commit batch import grammar.

**Quy trình:**

1. Backend chạy `preview` nội bộ trước.
2. Nếu còn item invalid → trả `Grammar_ImportBatchHasErrors_400`.
3. Hợp lệ → tạo tuần tự. Mỗi sentence sẽ generate audio bằng VOICEVOX.

**Request body:** Cùng shape với `import/preview`.

**Response data:**

```json
{
  "totalItems": 1,
  "successfulItems": 1,
  "failedItems": 0,
  "hasValidationErrors": false,
  "items": [
    {
      "rowNumber": 1,
      "title": "〜ながら",
      "isSuccess": true,
      "action": "created",
      "cardId": "new-grammar-card-id",
      "errors": []
    }
  ]
}
```

---

## 8. Kanji Module — Admin

> 🔑 **Tất cả endpoint trong module này yêu cầu quyền `Editor` hoặc `Admin`.**  
> Trừ `GET /api/kanji/{cardId}` là endpoint Public dùng cho cả user lẫn admin.

### Tổng quan

| Method | Endpoint                     | Auth            | Mô tả                        |
| ------ | ---------------------------- | --------------- | ---------------------------- |
| GET    | `/api/kanji`                 | 🔑 Editor/Admin | Tìm kiếm kanji có phân trang |
| GET    | `/api/kanji/{cardId}`        | 🌐 Public       | Lấy chi tiết kanji           |
| POST   | `/api/kanji`                 | 🔑 Editor/Admin | Tạo kanji mới                |
| PATCH  | `/api/kanji/{cardId}`        | 🔑 Editor/Admin | Cập nhật kanji               |
| DELETE | `/api/kanji/{cardId}`        | 🔑 Editor/Admin | Xóa mềm kanji (Archived)     |
| GET    | `/api/kanji/import-template` | 🔑 Editor/Admin | Tải JSON template import     |
| GET    | `/api/kanji/export`          | 🔑 Editor/Admin | Export kanji ra JSON         |
| POST   | `/api/kanji/import/preview`  | 🔑 Editor/Admin | Preview import, chưa ghi DB  |
| POST   | `/api/kanji/import/commit`   | 🔑 Editor/Admin | Commit batch import          |

---

### GET `/api/kanji` 🔑

Tìm kiếm danh sách kanji cho admin.

**Query params:**

| Param            | Type     | Bắt buộc | Enum            | Mô tả                                                        |
| ---------------- | -------- | -------- | --------------- | ------------------------------------------------------------ |
| `q`              | `string` | ❌       | —               | Tìm theo `title`, `summary`, `kanji`, `meaningVi`, `hanViet` |
| `level`          | `string` | ❌       | `JlptLevel`     | Lọc theo trình độ                                            |
| `status`         | `string` | ❌       | `PublishStatus` | Lọc theo trạng thái                                          |
| `strokeCountMin` | `int`    | ❌       | —               | Số nét tối thiểu, phải > `0`                                 |
| `strokeCountMax` | `int`    | ❌       | —               | Số nét tối đa, phải > `0`                                    |
| `radical`        | `string` | ❌       | —               | Lọc theo đúng radical character, ví dụ `日`, `口`, `氵`      |
| `createdByMe`    | `bool`   | ❌       | —               | `true` = chỉ lấy card do mình tạo                            |
| `page`           | `int`    | ❌       | —               | Mặc định `1`                                                 |
| `pageSize`       | `int`    | ❌       | —               | Mặc định `20`                                                |

**Response data item** (`KanjiListItemResponse`):

```json
{
  "id": "string",
  "title": "明",
  "summary": "Kanji diễn tả sự sáng, rõ ràng.",
  "level": "N5 | null",
  "tags": ["kanji", "co-ban"],
  "status": "Draft | Published | Archived",
  "createdAt": "datetime",
  "updatedAt": "datetime | null",
  "kanji": "明",
  "strokeCount": 8,
  "hanViet": "minh | null",
  "meaningVi": "sáng, rõ ràng",
  "radicalCount": 2
}
```

---

### GET `/api/kanji/{cardId}` 🌐

Lấy chi tiết một kanji card.

**Quy tắc truy cập:**

- ✅ Card `Published`: ai cũng xem được (public).
- ⚠ Card `Draft` / `Archived`: chỉ user tạo card mới xem được.

**Response data** (`KanjiDetailResponse`):

```json
{
  "id": "string",
  "cardType": "Kanji",
  "title": "明",
  "summary": "Kanji diễn tả sự sáng, rõ ràng.",
  "level": "N5 | null",
  "tags": ["kanji", "co-ban"],
  "status": "Published | Draft | Archived",
  "createdAt": "datetime",
  "updatedAt": "datetime | null",
  "kanji": "明",
  "strokeCount": 8,
  "strokeOrderUrl": "https://cdn.example.com/kanji/mei.gif | null",
  "onyomi": ["メイ", "ミョウ"],
  "kunyomi": ["あ.かり", "あか.るい"],
  "hanViet": "minh | null",
  "meaningVi": "sáng, rõ ràng",
  "radicals": [
    {
      "id": "radical-id-1",
      "character": "日",
      "meaningVi": "mặt trời",
      "kanjiCardId": "kanji-card-id-cua-日 | null"
    },
    {
      "id": "radical-id-2",
      "character": "月",
      "meaningVi": "mặt trăng",
      "kanjiCardId": null
    }
  ],
  "userNotes": [
    {
      "id": "note-id",
      "userId": "user-id",
      "userName": "Tran Thi B",
      "content": "Chữ này ghép từ mặt trời và mặt trăng.",
      "likesCount": 2,
      "isLikedByMe": false,
      "createdAt": "datetime"
    }
  ]
}
```

**Field details:**

| Field                    | Type       | Enum       | Mô tả                                                             |
| ------------------------ | ---------- | ---------- | ----------------------------------------------------------------- |
| `cardType`               | `string`   | `CardType` | Luôn là `Kanji`                                                   |
| `kanji`                  | `string`   | —          | Bản thân chữ kanji, ví dụ `明`                                    |
| `strokeCount`            | `int`      | —          | Số nét, phải > `0`                                                |
| `strokeOrderUrl`         | `string?`  | —          | URL ảnh/GIF/video mô tả thứ tự nét                                |
| `onyomi`                 | `string[]` | —          | Danh sách âm On, ví dụ `["メイ", "ミョウ"]`                       |
| `kunyomi`                | `string[]` | —          | Danh sách âm Kun, ví dụ `["あ.かるい", "あ.ける"]`                |
| `hanViet`                | `string?`  | —          | Âm Hán Việt, ví dụ `minh`                                         |
| `meaningVi`              | `string`   | —          | Nghĩa tiếng Việt                                                  |
| `radicals[].character`   | `string`   | —          | Ký tự radical, ví dụ `日`, `口`, `氵`                             |
| `radicals[].meaningVi`   | `string`   | —          | Nghĩa tiếng Việt của radical                                      |
| `radicals[].kanjiCardId` | `string?`  | —          | Nếu radical này cũng có kanji card riêng, backend tự link card đó |

**Error codes:**

| Code                      | Khi nào                                      |
| ------------------------- | -------------------------------------------- |
| `Kanji_CardNotFound_404`  | Card không tồn tại                           |
| `Kanji_ReadForbidden_401` | Card chưa Published và user không phải owner |

---

### POST `/api/kanji` 🔑

Tạo mới một kanji card.

**Request body:**

```json
{
  "title": "明", // ⚠ bắt buộc
  "summary": "Kanji diễn tả sự sáng, rõ ràng.", // ⚠ bắt buộc
  "level": "N5", // ❌ nullable — enum JlptLevel
  "tags": ["kanji", "co-ban"], // ❌ optional, mảng string
  "status": "Draft", // ❌ nullable — enum PublishStatus
  "kanji": "明", // ⚠ bắt buộc, duy nhất trong hệ thống
  "strokeCount": 8, // ⚠ bắt buộc, int > 0
  "strokeOrderUrl": "https://example.com/mei.gif", // ❌ nullable
  "onyomi": ["メイ", "ミョウ"], // ❌ optional, mảng string
  "kunyomi": ["あ.かり", "あか.るい"], // ❌ optional, mảng string
  "hanViet": "minh", // ❌ nullable
  "meaningVi": "sáng, rõ ràng", // ⚠ bắt buộc
  "radicals": [
    // ⚠ bắt buộc, ít nhất 1 item
    {
      "character": "日", // ⚠ bắt buộc
      "meaningVi": "mặt trời" // ⚠ bắt buộc
    },
    {
      "character": "月",
      "meaningVi": "mặt trăng"
    }
  ]
}
```

**Field rules quan trọng:**

| Field                  | Hợp lệ khi                                       | Ghi chú                                                 |
| ---------------------- | ------------------------------------------------ | ------------------------------------------------------- |
| `title`                | string không rỗng, max `200` ký tự               | Thường nên đặt cùng giá trị với `kanji` để đồng nhất UI |
| `summary`              | string không rỗng, max `2000` ký tự              | Mô tả ngắn cho card                                     |
| `level`                | `N5`, `N4`, `N3`, `N2`, `N1` hoặc bỏ trống       | Không được gửi giá trị khác                             |
| `tags`                 | tối đa `20` phần tử, mỗi phần tử max `100` ký tự | Backend tự trim và loại bỏ phần tử rỗng                 |
| `status`               | `Draft`, `Published`, `Archived` hoặc bỏ trống   | Nếu bỏ trống khi create, backend mặc định `Draft`       |
| `kanji`                | string không rỗng, max `20` ký tự                | Phải duy nhất toàn hệ thống                             |
| `strokeCount`          | số nguyên > `0`                                  | Không chấp nhận `0` hoặc số âm                          |
| `strokeOrderUrl`       | string max `2000` ký tự hoặc `null`              | Chỉ là URL string, không upload file qua endpoint này   |
| `onyomi`               | tối đa `20` item, mỗi item max `100` ký tự       | Ví dụ `["メイ", "ミョウ"]`                              |
| `kunyomi`              | tối đa `20` item, mỗi item max `100` ký tự       | Ví dụ `["あ.かるい", "あ.ける"]`                        |
| `hanViet`              | string max `200` ký tự hoặc `null`               | Ví dụ `minh`, `nhật`                                    |
| `meaningVi`            | string không rỗng, max `1000` ký tự              | Nghĩa tiếng Việt chính                                  |
| `radicals`             | bắt buộc có ít nhất `1` item, tối đa `30` item   | Đây là danh sách thành phần cấu tạo của kanji           |
| `radicals[].character` | string không rỗng, max `20` ký tự                | Nên là đúng ký tự radical, ví dụ `日`, `月`, `氵`       |
| `radicals[].meaningVi` | string không rỗng, max `500` ký tự               | Nghĩa tiếng Việt của radical                            |

**Lưu ý về radicals:**

- Client **không gửi** `radicalId`.
- Client **không gửi** `kanjiCardId`.
- Backend tự:
  - tìm radical theo `character`
  - nếu đã tồn tại thì reuse record đó
  - nếu chưa có thì tạo mới
  - tự gắn `kanjiCardId` nếu tồn tại một kanji card có `kanji` trùng `radicals[].character`

**Response data:** `KanjiDetailResponse`

---

### PATCH `/api/kanji/{cardId}` 🔑

Cập nhật kanji card. Body giống `POST`.

**⚠ Quy tắc `radicals`:**

- Danh sách `radicals` gửi lên = **trạng thái cuối cùng**.
- Radical nào **không có** trong request → bị gỡ khỏi liên kết của kanji này.
- Radical trùng `character` trong cùng một payload là dữ liệu không hợp lệ.

**Response data:** `KanjiDetailResponse`

---

### DELETE `/api/kanji/{cardId}` 🔑

Xóa mềm kanji card (chuyển `status = Archived`).

**Response data:** `true`

---

### GET `/api/kanji/import-template` 🔑

Tải file JSON template mẫu cho import kanji.

- Response: file `application/json` (`Content-Disposition: attachment`).
- JSON dùng `camelCase`.
- Có thêm object `guide` để mô tả `allowedValues` và `fieldNotes` cho các field quan trọng.
- Payload import thực tế vẫn nằm trong `items` (shape cùng với request body của `import/preview`).

---

### GET `/api/kanji/export` 🔑

Export kanji ra file JSON theo bộ lọc.

**Query params:**

| Param            | Type     | Enum            | Mô tả                     |
| ---------------- | -------- | --------------- | ------------------------- |
| `q`              | `string` | —               | Từ khóa                   |
| `level`          | `string` | `JlptLevel`     |                           |
| `status`         | `string` | `PublishStatus` |                           |
| `strokeCountMin` | `int`    | —               |                           |
| `strokeCountMax` | `int`    | —               |                           |
| `radical`        | `string` | —               | Radical character cần lọc |
| `createdByMe`    | `bool`   | —               |                           |

- Response: file `application/json` với shape tương tự import payload.

---

### POST `/api/kanji/import/preview` 🔑

Preview payload import. Validate từng item, **chưa ghi vào DB**.

**Import rules:**

- Import hiện tại là **create-only** (chỉ tạo mới).
- `kanji` không được trùng trong batch + không trùng DB.
- `radicals` là bắt buộc.
- Trong cùng một item import, `radicals[*].character` không được trùng nhau.
- `radical` trong import chỉ cần gửi `character` + `meaningVi`.

**Request body:**

```json
{
  "items": [
    {
      "rowNumber": 1, // ⚠ số thứ tự hàng, > 0
      "title": "明", // ⚠ bắt buộc, max 200
      "summary": "Kanji diễn tả sự sáng.", // ⚠ bắt buộc, max 2000
      "level": "N5", // ❌ nullable — JlptLevel
      "tags": ["kanji", "co-ban"], // ❌ optional, tối đa 20 item
      "status": "Draft", // ❌ nullable — PublishStatus
      "kanji": "明", // ⚠ bắt buộc, duy nhất
      "strokeCount": 8, // ⚠ bắt buộc, int > 0
      "strokeOrderUrl": "https://example.com/mei.gif", // ❌ nullable
      "onyomi": ["メイ", "ミョウ"], // ❌ optional
      "kunyomi": ["あ.かり", "あか.るい"], // ❌ optional
      "hanViet": "minh", // ❌ nullable
      "meaningVi": "sáng, rõ ràng", // ⚠ bắt buộc
      "radicals": [
        // ⚠ bắt buộc, ít nhất 1 item
        {
          "character": "日", // ⚠ bắt buộc
          "meaningVi": "mặt trời" // ⚠ bắt buộc
        },
        {
          "character": "月",
          "meaningVi": "mặt trăng"
        }
      ]
    }
  ]
}
```

**Field guide để tự viết file import JSON hợp lệ:**

| JSON path                      | Kiểu dữ liệu hợp lệ  | Bắt buộc | Ví dụ hợp lệ                                  | Ghi chú                                              |
| ------------------------------ | -------------------- | -------- | --------------------------------------------- | ---------------------------------------------------- |
| `items`                        | `array`              | ⚠        | `[ {...} ]`                                   | Danh sách item import                                |
| `items[].rowNumber`            | `int` > `0`          | ❌       | `1`                                           | Nếu bỏ trống, backend tự lấy số thứ tự theo vị trí   |
| `items[].title`                | `string`             | ⚠        | `"明"`                                        | Không được rỗng                                      |
| `items[].summary`              | `string`             | ⚠        | `"Kanji diễn tả sự sáng."`                    | Không được rỗng                                      |
| `items[].level`                | `string` hoặc `null` | ❌       | `"N5"`                                        | Chỉ nhận `N5/N4/N3/N2/N1`                            |
| `items[].tags`                 | `string[]`           | ❌       | `["kanji", "co-ban"]`                         | Mỗi phần tử là string                                |
| `items[].status`               | `string` hoặc `null` | ❌       | `"Draft"`                                     | Chỉ nhận `Draft/Published/Archived`                  |
| `items[].kanji`                | `string`             | ⚠        | `"明"`                                        | Không được trùng DB hoặc trùng item khác trong batch |
| `items[].strokeCount`          | `int`                | ⚠        | `8`                                           | Phải > `0`                                           |
| `items[].strokeOrderUrl`       | `string` hoặc `null` | ❌       | `"https://example.com/mei.gif"`               | Không phải object/file                               |
| `items[].onyomi`               | `string[]`           | ❌       | `["メイ", "ミョウ"]`                          | Không dùng number/object                             |
| `items[].kunyomi`              | `string[]`           | ❌       | `["あ.かり", "あか.るい"]`                    | Không dùng number/object                             |
| `items[].hanViet`              | `string` hoặc `null` | ❌       | `"minh"`                                      |                                                      |
| `items[].meaningVi`            | `string`             | ⚠        | `"sáng, rõ ràng"`                             | Không được rỗng                                      |
| `items[].radicals`             | `array`              | ⚠        | `[{"character":"日","meaningVi":"mặt trời"}]` | Phải có ít nhất 1 phần tử                            |
| `items[].radicals[].character` | `string`             | ⚠        | `"日"`                                        | Trong cùng item không được trùng nhau                |
| `items[].radicals[].meaningVi` | `string`             | ⚠        | `"mặt trời"`                                  | Không được rỗng                                      |

**Response data:**

```json
{
  "totalItems": 1,
  "validItems": 1,
  "invalidItems": 0,
  "items": [
    {
      "rowNumber": 1,
      "title": "明",
      "kanji": "明",
      "isValid": true,
      "errors": []
    }
  ]
}
```

**Ví dụ response lỗi:**

```json
{
  "totalItems": 1,
  "validItems": 0,
  "invalidItems": 1,
  "items": [
    {
      "rowNumber": 1,
      "title": "明",
      "kanji": "明",
      "isValid": false,
      "errors": [
        "Kanji_ImportKanjiAlreadyExists_400",
        "Kanji_ImportDuplicateRadicalInItem_400:radicals[1].character",
        "Kanji_ImportFieldInvalid_400:strokeCount"
      ]
    }
  ]
}
```

**Error codes cho import:**

| Code                                             | Mô tả                              |
| ------------------------------------------------ | ---------------------------------- |
| `Kanji_ImportInvalidPayload_400`                 | Payload tổng thể không hợp lệ      |
| `Kanji_ImportBatchHasErrors_400`                 | Batch còn item lỗi, không commit   |
| `Kanji_ImportFieldRequired_400:<field>`          | Field bắt buộc bị thiếu            |
| `Kanji_ImportFieldTooLong_400:<field>`           | Field vượt quá độ dài cho phép     |
| `Kanji_ImportFieldInvalid_400:<field>`           | Giá trị enum / number không hợp lệ |
| `Kanji_ImportDuplicateKanjiInBatch_400`          | `kanji` trùng trong batch          |
| `Kanji_ImportKanjiAlreadyExists_400`             | `kanji` đã có trong DB             |
| `Kanji_ImportRadicalsRequired_400`               | Thiếu `radicals`                   |
| `Kanji_ImportDuplicateRadicalInItem_400:<field>` | Radical trùng trong cùng item      |
| `Kanji_ImportListTooManyItems_400:<field>`       | Vượt quá số item cho phép          |
| `Kanji_ImportRowNumberInvalid_400`               | `rowNumber` không hợp lệ           |

---

### POST `/api/kanji/import/commit` 🔑

Commit batch import vào DB.

**Quy trình:**

1. Backend chạy `preview` nội bộ trước.
2. Nếu còn item invalid → **không ghi DB**, trả `HasValidationErrors = true`.
3. Nếu tất cả hợp lệ → tạo tuần tự từng kanji card mới.

**Request body:** Cùng shape với `import/preview`.

**Response data:**

```json
{
  "totalItems": 1,
  "successfulItems": 1,
  "failedItems": 0,
  "hasValidationErrors": false,
  "items": [
    {
      "rowNumber": 1,
      "title": "明",
      "kanji": "明",
      "isSuccess": true,
      "action": "created",
      "cardId": "new-kanji-card-id",
      "errors": []
    }
  ]
}
```

---

## 9. Sentences Module — Admin

> 🔑 **Toàn bộ module này yêu cầu quyền `Editor` hoặc `Admin`.**  
> Sentences là câu ví dụ dùng chung cho Vocabulary và Grammar.

### Tổng quan

| Method | Endpoint                         | Auth            | Mô tả                           |
| ------ | -------------------------------- | --------------- | ------------------------------- |
| GET    | `/api/sentences`                 | 🔑 Editor/Admin | Tìm kiếm sentence có phân trang |
| GET    | `/api/sentences/{id}`            | 🔑 Editor/Admin | Lấy chi tiết sentence           |
| POST   | `/api/sentences`                 | 🔑 Editor/Admin | Tạo sentence mới                |
| PATCH  | `/api/sentences/{id}`            | 🔑 Editor/Admin | Cập nhật sentence               |
| DELETE | `/api/sentences/{id}`            | 🔑 Editor/Admin | Xóa sentence                    |
| GET    | `/api/sentences/import-template` | 🔑 Editor/Admin | Tải JSON template import        |
| GET    | `/api/sentences/export`          | 🔑 Editor/Admin | Export sentences ra JSON        |
| POST   | `/api/sentences/import/preview`  | 🔑 Editor/Admin | Preview import, chưa ghi DB     |
| POST   | `/api/sentences/import/commit`   | 🔑 Editor/Admin | Commit batch import             |

---

### GET `/api/sentences` 🔑

Tìm kiếm danh sách sentence.

**Query params:**

| Param         | Type     | Bắt buộc | Enum        | Mô tả               |
| ------------- | -------- | -------- | ----------- | ------------------- |
| `q`           | `string` | ❌       | —           | Từ khóa tìm kiếm    |
| `level`       | `string` | ❌       | `JlptLevel` | Lọc theo trình độ   |
| `hasAudio`    | `bool`   | ❌       | —           | Lọc có/không audio  |
| `createdByMe` | `bool`   | ❌       | —           | Chỉ lấy do mình tạo |
| `page`        | `int`    | ❌       | —           | Mặc định `1`        |
| `pageSize`    | `int`    | ❌       | —           | Mặc định `20`       |

**Response data item** (`SentenceResponse`):

```json
{
  "id": "string",
  "text": "日本へ行きたいです。",
  "meaning": "Tôi muốn đi Nhật.",
  "audioUrl": "string | null",
  "speakerId": 3,
  "level": "N5 | null",
  "createdAt": "datetime",
  "updatedAt": "datetime | null"
}
```

---

### GET `/api/sentences/{id}` 🔑

Lấy chi tiết sentence theo ID.

**Response data:** `SentenceResponse`

**Error codes:**

| Code                    | Khi nào                |
| ----------------------- | ---------------------- |
| `Sentence_NotFound_404` | Sentence không tồn tại |

---

### POST `/api/sentences` 🔑

Tạo sentence mới.

**Lưu ý VOICEVOX-only:**

- ❌ Client **không gửi** `audioUrl`.
- ✅ Backend tự generate audio bằng VOICEVOX từ `text` và `speakerId`.

**Request body:**

```json
{
  "text": "日本へ行きたいです。", // ⚠ bắt buộc
  "meaning": "Tôi muốn đi Nhật.", // ⚠ bắt buộc
  "speakerId": 3, // ❌ nullable
  "level": "N5" // ❌ nullable — enum JlptLevel
}
```

**Response data:** `SentenceResponse`

---

### PATCH `/api/sentences/{id}` 🔑

Cập nhật sentence.

**Request body:** Cùng shape với `POST`.

**Response data:** `SentenceResponse`

---

### DELETE `/api/sentences/{id}` 🔑

Xóa sentence.

**Response data:** `true`

---

### GET `/api/sentences/import-template` 🔑

Tải file JSON template mẫu cho import sentences.

- JSON dùng `camelCase`.
- Có thêm object `guide` để mô tả `allowedValues` và `fieldNotes` cho các field quan trọng.

**Response file body:**

```json
{
  "items": [
    {
      "rowNumber": 1,
      "text": "日本へ行きたいです。",
      "meaning": "Tôi muốn đi Nhật.",
      "speakerId": 3,
      "level": "N5"
    }
  ]
}
```

---

### GET `/api/sentences/export` 🔑

Export sentences ra JSON.

**Query params:**

| Param         | Type     | Enum        | Mô tả   |
| ------------- | -------- | ----------- | ------- |
| `q`           | `string` | —           | Từ khóa |
| `level`       | `string` | `JlptLevel` |         |
| `hasAudio`    | `bool`   | —           |         |
| `createdByMe` | `bool`   | —           |         |

---

### POST `/api/sentences/import/preview` 🔑

Preview import sentences, validate từng item, **chưa ghi DB**.

**Import rules:**

- Import là **create-only**.
- Backend **không nhận** `audioUrl`; khi commit sẽ tự synth audio.

**Request body:**

```json
{
  "items": [
    {
      "rowNumber": 1,
      "text": "日本へ行きたいです。",
      "meaning": "Tôi muốn đi Nhật.",
      "speakerId": 3,
      "level": "N5"
    }
  ]
}
```

**Response data:**

```json
{
  "totalItems": 1,
  "validItems": 1,
  "invalidItems": 0,
  "items": [
    {
      "rowNumber": 1,
      "text": "日本へ行きたいです。",
      "isValid": true,
      "errors": [],
      "warnings": []
    }
  ]
}
```

**Error codes cho import:**

| Code                                       | Mô tả                                       |
| ------------------------------------------ | ------------------------------------------- |
| `Sentence_ImportInvalidPayload_400`        | Payload không hợp lệ                        |
| `Sentence_ImportBatchHasErrors_400`        | Batch còn lỗi                               |
| `Sentence_ImportFieldRequired_400:<field>` | Field bắt buộc thiếu. VD: `text`, `meaning` |
| `Sentence_ImportFieldTooLong_400:<field>`  | Vượt độ dài                                 |
| `Sentence_ImportFieldInvalid_400:<field>`  | Giá trị enum không hợp lệ. VD: `level`      |
| `Sentence_ImportSpeakerIdNotSupported_400` | `speakerId` không hợp lệ                    |
| `Sentence_ImportRowNumberInvalid_400`      | `rowNumber` không hợp lệ                    |

---

### POST `/api/sentences/import/commit` 🔑

Commit batch import sentences.

**Quy trình:**

1. Backend chạy `preview` nội bộ trước.
2. Nếu còn lỗi → không ghi DB.
3. Hợp lệ → tạo tuần tự, mỗi sentence generate audio VOICEVOX.

**Request body:** Cùng shape với `import/preview`.

**Response data:**

```json
{
  "totalItems": 2,
  "successfulItems": 2,
  "failedItems": 0,
  "hasValidationErrors": false,
  "items": [
    {
      "rowNumber": 1,
      "text": "日本へ行きたいです。",
      "isSuccess": true,
      "action": "created",
      "sentenceId": "new-sentence-id",
      "errors": []
    }
  ]
}
```

---

## 9. Uploads Module — Admin

> 🔒 Yêu cầu đăng nhập.

### Tổng quan

| Method | Endpoint             | Auth    | Mô tả                 |
| ------ | -------------------- | ------- | --------------------- |
| POST   | `/api/uploads/audio` | 🔒 Auth | Upload audio resource |
| POST   | `/api/uploads/image` | 🔑 Editor/Admin | Upload image resource |

---

### POST `/api/uploads/audio` 🔒

Upload file audio và lưu metadata vào `MediaAssets`.

- **Content-Type:** `multipart/form-data`
- **Form field:** `audio`
- **Allowed MIME:** `audio/mpeg`, `audio/wav`, `audio/mp4`
- **Max size:** `20 MB`

**Response data:**

```json
{
  "id": "string",
  "fileUrl": "string",
  "fileType": "Audio",
  "usageType": "Audio",
  "sizeInBytes": 12345,
  "createdAt": "datetime"
}
```

---

### POST `/api/uploads/image` 🔑 Editor/Admin

Upload file image và lưu metadata vào `MediaAssets`.

- **Content-Type:** `multipart/form-data`
- **Form field:** `image`
- **Allowed MIME:** `image/jpeg`, `image/png`, `image/webp`, `image/gif`
- **Max size:** `10 MB`

**Response data:**

```json
{
  "id": "string",
  "fileUrl": "string",
  "fileType": "Image",
  "usageType": "Image",
  "sizeInBytes": 12345,
  "createdAt": "datetime"
}
```

---

## 10. Voicevox Module — Admin

> 🔒 Yêu cầu đăng nhập.  
> VOICEVOX là engine Text-to-Speech để generate audio cho sentences.

### Tổng quan

| Method | Endpoint                 | Auth    | Mô tả                          |
| ------ | ------------------------ | ------- | ------------------------------ |
| GET    | `/api/voicevox/speakers` | 🔒 Auth | Lấy danh sách speaker khả dụng |
| POST   | `/api/voicevox/preview`  | 🔒 Auth | Generate preview audio         |

---

### GET `/api/voicevox/speakers` 🔒

Lấy danh sách speaker VOICEVOX được phép sử dụng.

**Response data item:**

```json
{
  "speakerId": 3,
  "characterName": "ずんだもん",
  "styleName": "ノーマル"
}
```

---

### POST `/api/voicevox/preview` 🔒

Generate audio preview để phát thử khi admin đổi speaker.

**Request body:**

```json
{
  "speakerId": 3, // ⚠ bắt buộc, int
  "text": "こんにちは。" // ❌ nullable, nếu rỗng backend dùng text mặc định
}
```

**Response data:**

```json
{
  "speakerId": 3,
  "text": "こんにちは。こちらは音声プレビューです。",
  "audioUrl": "/audio-cache/example.wav"
}
```

---

## 12. Decks Module — User

> User-facing deck APIs for discovery, bookmarks, fork, and personal deck management.

### Overview

| Method | Endpoint                                  | Auth      | Description |
| ------ | ----------------------------------------- | --------- | ----------- |
| GET    | `/api/deck-types`                         | 🌐 Public | List deck types for filters |
| GET    | `/api/decks`                              | 🌐 Public | List public published decks |
| GET    | `/api/decks/{deckId}`                     | 🌐 Public | Get deck detail. Private decks are owner-only |
| POST   | `/api/decks/{deckId}/bookmark`            | 🔒 Auth   | Bookmark or unbookmark a readable deck |
| POST   | `/api/decks/{deckId}/fork`                | 🔒 Auth   | Fork a public published deck into personal library |
| GET    | `/api/me/decks`                           | 🔒 Auth   | List my own decks |
| POST   | `/api/me/decks`                           | 🔒 Auth   | Create my own deck |
| PATCH  | `/api/me/decks/{deckId}`                  | 🔒 Auth   | Update my own deck |
| DELETE | `/api/me/decks/{deckId}`                  | 🔒 Auth   | Delete my own deck |
| GET    | `/api/me/decks/bookmarks`                 | 🔒 Auth   | List my bookmarked decks |
| POST   | `/api/me/decks/{deckId}/folders`          | 🔒 Auth   | Create folder inside my deck |
| PUT    | `/api/me/decks/{deckId}/folders/order`    | 🔒 Auth   | Reorder folders inside my deck |
| PATCH  | `/api/me/folders/{folderId}`              | 🔒 Auth   | Update my folder |
| DELETE | `/api/me/folders/{folderId}`              | 🔒 Auth   | Delete my folder |
| POST   | `/api/me/folders/{folderId}/cards`        | 🔒 Auth   | Add card into my folder |
| DELETE | `/api/me/folders/{folderId}/cards/{cardId}` | 🔒 Auth | Remove card from my folder |
| PUT    | `/api/me/folders/{folderId}/cards/order`  | 🔒 Auth   | Reorder cards inside my folder |

### Visibility and access rules

- `GET /api/decks` only returns decks with `status = Published` and `visibility = Public`.
- `GET /api/decks/{deckId}` returns:
  - public published decks for everyone
  - private decks only for the owner
- Bookmark is allowed only when the current user can read the deck.
- Fork is allowed only for `Published + Public` source decks.
- A forked deck is created with:
  - `visibility = Private`
  - `status = Published`
  - `isOfficial = false`
- Personal deck write endpoints are strictly owner-only.
- A card can appear only once inside a deck, even if the deck has multiple folders.

### Common response shapes

**DeckTypeResponse**

```json
{
  "id": "string",
  "name": "string"
}
```

**DeckListItemResponse**

```json
{
  "id": "string",
  "title": "string",
  "description": "string",
  "coverImageUrl": "string | null",
  "visibility": "Public | Private",
  "status": "Draft | Published | Archived",
  "isOfficial": false,
  "cardsCount": 12,
  "foldersCount": 3,
  "type": {
    "id": "string | null",
    "name": "string | null"
  },
  "createdBy": {
    "id": "string",
    "username": "string",
    "avatarUrl": "string | null"
  },
  "forkedFromId": "string | null",
  "isBookmarked": false,
  "isOwner": false,
  "createdAt": "2026-04-16T08:00:00Z",
  "updatedAt": "2026-04-16T08:00:00Z"
}
```

**DeckDetailResponse**

```json
{
  "id": "string",
  "title": "string",
  "description": "string",
  "coverImageUrl": "string | null",
  "visibility": "Private",
  "status": "Published",
  "isOfficial": false,
  "cardsCount": 12,
  "foldersCount": 2,
  "type": {
    "id": "string | null",
    "name": "string | null"
  },
  "createdBy": {
    "id": "string",
    "username": "string",
    "avatarUrl": "string | null"
  },
  "forkedFromId": "string | null",
  "isBookmarked": false,
  "isOwner": true,
  "folders": [
    {
      "id": "string",
      "title": "Basic",
      "description": "",
      "position": 1000,
      "cardsCount": 2,
      "cards": [
        {
          "cardId": "string",
          "position": 1000,
          "addedAt": "2026-04-16T08:00:00Z",
          "card": {
            "id": "string",
            "title": "食べる",
            "summary": "to eat",
            "cardType": "Vocab",
            "level": "N5"
          }
        }
      ]
    }
  ],
  "createdAt": "2026-04-16T08:00:00Z",
  "updatedAt": "2026-04-16T08:00:00Z"
}
```

### GET `/api/deck-types` 🌐

List deck types for filter dropdowns and deck creation forms.

**Response data:** `DeckTypeResponse[]`

### GET `/api/decks` 🌐

List public published decks for the user app.

**Query params**

| Param | Type | Default | Notes |
| ----- | ---- | ------- | ----- |
| `q` | `string` | `null` | Search by title or description |
| `typeId` | `string` | `null` | Filter by deck type |
| `officialOnly` | `bool` | `null` | When `true`, only official decks are returned |
| `page` | `int` | `1` | Pagination |
| `pageSize` | `int` | `20` | Max `100` |

**Frontend notes**

- Frontend must not send `status` or `visibility` filters here.
- Backend already enforces `Published + Public`.
- If the user is authenticated, each item includes `isBookmarked`.

### GET `/api/decks/{deckId}` 🌐

Get full deck detail including folders and cards.

**Access**

- Public published deck: readable by everyone.
- Private or non-public deck: readable only by owner.

**Frontend notes**

- Use `isOwner` to decide whether to show edit/manage actions.
- For personal decks, user app can reuse the same detail page as public decks.

### POST `/api/decks/{deckId}/bookmark` 🔒

Create or remove bookmark for a readable deck.

**Request body**

```json
{
  "bookmarked": true
}
```

**Response data**

```json
{
  "deckId": "string",
  "bookmarked": true,
  "savedAt": "2026-04-16T08:00:00Z"
}
```

**Frontend notes**

- This endpoint is idempotent for the requested final state.
- When `bookmarked = false`, `savedAt` returns `null`.

### POST `/api/decks/{deckId}/fork` 🔒

Fork a public published deck into the current user's library.

**Request body:** none

**Response data:** `DeckDetailResponse`

**Frontend notes**

- The returned deck is already the newly created personal deck.
- Fork result is `Private + Published`, so the user can use it immediately.
- Recommended UX: after success, redirect to personal deck detail or edit page.

### GET `/api/me/decks` 🔒

List all decks created by the current user.

**Query params**

| Param | Type | Default | Notes |
| ----- | ---- | ------- | ----- |
| `q` | `string` | `null` | Search by title or description |
| `typeId` | `string` | `null` | Filter by deck type |
| `page` | `int` | `1` | Pagination |
| `pageSize` | `int` | `20` | Max `100` |

**Frontend notes**

- Returns both `Public` and `Private` decks of the owner.
- Current implementation also returns personal decks regardless of status, but user-created decks are created as `Published` in the current flow.

### POST `/api/me/decks` 🔒

Create a personal deck.

**Request body**

```json
{
  "title": "My N5 deck",
  "description": "Optional",
  "coverImageUrl": "https://... | null",
  "visibility": "Private",
  "typeId": "string | null"
}
```

**Response data:** `DeckDetailResponse`

**Frontend notes**

- If `visibility` is omitted, backend defaults to `Private`.
- New personal deck is created as `Published`.
- `isOfficial` is always `false`.

### PATCH `/api/me/decks/{deckId}` 🔒

Update a personal deck.

**Request body**

```json
{
  "title": "Updated title",
  "description": "Updated description",
  "coverImageUrl": "https://...",
  "visibility": "Public",
  "typeId": "string | null"
}
```

**Response data:** `DeckDetailResponse`

**Frontend notes**

- All fields are optional.
- Sending `"typeId": null` removes the current type.
- If `visibility` changes to `Public`, the deck still remains owner-created, not official.

### DELETE `/api/me/decks/{deckId}` 🔒

Delete a personal deck.

**Response data**

```json
true
```

### GET `/api/me/decks/bookmarks` 🔒

List decks bookmarked by the current user.

**Query params**

| Param | Type | Default | Notes |
| ----- | ---- | ------- | ----- |
| `q` | `string` | `null` | Search by title or description |
| `typeId` | `string` | `null` | Filter by deck type |
| `page` | `int` | `1` | Pagination |
| `pageSize` | `int` | `20` | Max `100` |

**Frontend notes**

- If a bookmarked deck is no longer readable, it will not be returned.
- Owner's own bookmarked decks may also appear here.

### POST `/api/me/decks/{deckId}/folders` 🔒

Create a folder inside the current user's deck.

**Request body**

```json
{
  "title": "Basic",
  "description": "Optional",
  "position": 1000
}
```

**Response data:** `DeckFolderResponse`

**Frontend notes**

- If `position` is omitted, backend appends to the end using sparse positions.
- Current sparse position convention is `1000`, `2000`, `3000`, ...

### PUT `/api/me/decks/{deckId}/folders/order` 🔒

Replace folder order for the entire deck.

**Request body**

```json
{
  "items": [
    { "folderId": "folder-1", "position": 1000 },
    { "folderId": "folder-2", "position": 2000 }
  ]
}
```

**Response data:** `DeckFolderResponse[]`

**Frontend notes**

- Payload must contain every folder in the deck exactly once.
- Backend rejects partial reorder payloads.

### PATCH `/api/me/folders/{folderId}` 🔒

Update a folder owned by the current user.

**Request body**

```json
{
  "title": "Updated folder",
  "description": "Updated description"
}
```

**Response data:** `DeckFolderResponse`

### DELETE `/api/me/folders/{folderId}` 🔒

Delete a folder owned by the current user.

**Response data**

```json
true
```

**Frontend notes**

- Deleting a folder also removes all folder-card links in that folder.
- Deck-level `cardsCount` and `foldersCount` are updated by backend.

### POST `/api/me/folders/{folderId}/cards` 🔒

Add a card into a folder in the current user's deck.

**Request body**

```json
{
  "cardId": "string",
  "position": 1000
}
```

**Response data:** `DeckFolderResponse`

**Frontend notes**

- If `position` is omitted, backend appends to the end.
- The same card cannot appear twice in the same deck, even across different folders.
- Current rule allows adding:
  - `Published` cards
  - or cards created by the same user

### DELETE `/api/me/folders/{folderId}/cards/{cardId}` 🔒

Remove a card from a folder.

**Response data**

```json
true
```

### PUT `/api/me/folders/{folderId}/cards/order` 🔒

Replace card order for the entire folder.

**Request body**

```json
{
  "items": [
    { "cardId": "card-1", "position": 1000 },
    { "cardId": "card-2", "position": 2000 }
  ]
}
```

**Response data:** `DeckFolderCardItemResponse[]`

**Frontend notes**

- Payload must contain every card in the folder exactly once.
- Backend rejects partial reorder payloads.

### Error codes

| Code | Description |
| ---- | ----------- |
| `Deck_NotFound_404` | Deck does not exist |
| `Deck_FolderNotFound_404` | Folder does not exist or does not belong to the current user |
| `Deck_CardNotFound_404` | Card does not exist or is not found in the target folder |
| `Deck_Forbidden_403` | Current user cannot read or mutate this deck |
| `Deck_ForkSourceInvalid_400` | Fork source is not `Published + Public` |
| `Deck_CardDuplicatedInDeck_400` | The same card already exists somewhere else in the deck |
| `Deck_InvalidReorderPayload_400` | Reorder payload is incomplete, duplicated, or inconsistent |

### Suggested frontend flows

1. Library discovery
- Load `GET /api/deck-types`
- Load `GET /api/decks`
- Open `GET /api/decks/{deckId}` on click

2. Bookmark flow
- Optimistically toggle bookmark in UI
- Call `POST /api/decks/{deckId}/bookmark`
- If request fails, rollback local state

3. Fork flow
- Call `POST /api/decks/{deckId}/fork`
- Redirect to the returned personal deck

4. Personal deck edit flow
- Load `GET /api/me/decks`
- Create deck with `POST /api/me/decks`
- Add folders and cards incrementally
- Use full reorder payloads for drag-and-drop save

---

## 13. Decks Module — Admin

> Implemented admin-facing API surface for `learning-admin` deck and deck type management.

### Overview

| Method | Endpoint | Auth | Purpose |
| ------ | -------- | ---- | ------- |
| GET | `/api/admin/deck-types` | 🔑 Editor/Admin | List deck types |
| GET | `/api/admin/deck-types/{id}` | 🔑 Editor/Admin | Get deck type detail |
| POST | `/api/admin/deck-types` | 🔑 Editor/Admin | Create deck type |
| PATCH | `/api/admin/deck-types/{id}` | 🔑 Editor/Admin | Update deck type |
| DELETE | `/api/admin/deck-types/{id}` | 🔑 Editor/Admin | Delete deck type |
| GET | `/api/admin/decks` | 🔑 Editor/Admin | Search all decks |
| GET | `/api/admin/decks/{deckId}` | 🔑 Editor/Admin | Get deck detail |
| POST | `/api/admin/decks` | 🔑 Editor/Admin | Create deck |
| PATCH | `/api/admin/decks/{deckId}` | 🔑 Editor/Admin | Update deck |
| DELETE | `/api/admin/decks/{deckId}` | 🔑 Editor/Admin | Delete deck |
| POST | `/api/admin/decks/{deckId}/publish` | 🔑 Editor/Admin | Publish deck |
| POST | `/api/admin/decks/{deckId}/archive` | 🔑 Editor/Admin | Archive deck |
| POST | `/api/admin/decks/{deckId}/unpublish` | 🔑 Editor/Admin | Move deck back to draft |
| POST | `/api/admin/decks/{deckId}/folders` | 🔑 Editor/Admin | Create folder in deck |
| PUT | `/api/admin/decks/{deckId}/folders/order` | 🔑 Editor/Admin | Reorder folders |
| PATCH | `/api/admin/folders/{folderId}` | 🔑 Editor/Admin | Update folder |
| DELETE | `/api/admin/folders/{folderId}` | 🔑 Editor/Admin | Delete folder |
| POST | `/api/admin/folders/{folderId}/cards` | 🔑 Editor/Admin | Add card to folder |
| DELETE | `/api/admin/folders/{folderId}/cards/{cardId}` | 🔑 Editor/Admin | Remove card from folder |
| PUT | `/api/admin/folders/{folderId}/cards/order` | 🔑 Editor/Admin | Reorder cards in folder |

### Admin access rules

- Admin endpoints can read `Draft`, `Published`, and `Archived` decks.
- Admin list endpoints are not restricted by `visibility`.
- Admin can manage both official decks and user-created decks.
- `deckType` is admin-managed only. User app remains read-only for deck types.
- `isOfficial` is mutable only through admin deck create/update.

### Shared response shapes

Admin reuses most user-facing models and extends list/detail items with management fields.

`AdminDeckTypeResponse`

```json
{
  "id": "string",
  "name": "string",
  "createdAt": "2026-04-17T08:00:00Z"
}
```

`AdminDeckListItemResponse`

```json
{
  "id": "string",
  "title": "string",
  "description": "string",
  "coverImageUrl": "string | null",
  "visibility": "Public | Private",
  "status": "Draft | Published | Archived",
  "isOfficial": false,
  "cardsCount": 12,
  "foldersCount": 3,
  "type": {
    "id": "string | null",
    "name": "string | null"
  },
  "createdBy": {
    "id": "string",
    "username": "string",
    "avatarUrl": "string | null"
  },
  "forkedFromId": "string | null",
  "bookmarkCount": 5,
  "createdAt": "2026-04-17T08:00:00Z",
  "updatedAt": "2026-04-17T08:00:00Z | null"
}
```

`AdminDeckDetailResponse`

```json
{
  "id": "string",
  "title": "string",
  "description": "string",
  "coverImageUrl": "string | null",
  "visibility": "Public | Private",
  "status": "Draft | Published | Archived",
  "isOfficial": false,
  "cardsCount": 12,
  "foldersCount": 3,
  "type": {
    "id": "string | null",
    "name": "string | null"
  },
  "createdBy": {
    "id": "string",
    "username": "string",
    "avatarUrl": "string | null"
  },
  "forkedFromId": "string | null",
  "bookmarkCount": 5,
  "folders": [DeckFolderResponse],
  "createdAt": "2026-04-17T08:00:00Z",
  "updatedAt": "2026-04-17T08:00:00Z | null"
}
```

### DeckType endpoints

### GET `/api/admin/deck-types` 🔑

List all deck types for admin management.

**Query params**

| Param | Type | Default | Notes |
| ----- | ---- | ------- | ----- |
| `q` | `string` | `null` | Search by type name |
| `page` | `int` | `1` | Pagination |
| `pageSize` | `int` | `20` | Max `100` |

**Response data:** `AdminDeckTypeResponse[]`

### GET `/api/admin/deck-types/{id}` 🔑

Get a single deck type.

**Response data:** `AdminDeckTypeResponse`

### POST `/api/admin/deck-types` 🔑

Create a new deck type.

**Request body**

```json
{
  "name": "Kanji by Radical"
}
```

**Response data:** `AdminDeckTypeResponse`

**Frontend notes**

- `name` must be unique.
- Recommended max length: `100`.

### PATCH `/api/admin/deck-types/{id}` 🔑

Update a deck type.

**Request body**

```json
{
  "name": "Updated type name"
}
```

**Response data:** `AdminDeckTypeResponse`

### DELETE `/api/admin/deck-types/{id}` 🔑

Delete a deck type.

**Response data**

```json
true
```

**Frontend notes**

- Backend should reject delete when the type is still referenced by any deck.
- Frontend should show a dependency warning instead of assuming hard delete always succeeds.

### Deck endpoints

### GET `/api/admin/decks` 🔑

Search all decks for admin moderation and management.

**Query params**

| Param | Type | Default | Notes |
| ----- | ---- | ------- | ----- |
| `q` | `string` | `null` | Search by title or description |
| `typeId` | `string` | `null` | Filter by deck type |
| `createdBy` | `string` | `null` | Filter by creator id |
| `status` | `PublishStatus` | `null` | Filter by `Draft`, `Published`, or `Archived` |
| `visibility` | `DeckVisibility` | `null` | Filter by `Public` or `Private` |
| `isOfficial` | `bool` | `null` | Filter official vs non-official |
| `page` | `int` | `1` | Pagination |
| `pageSize` | `int` | `20` | Max `100` |

**Response data:** `AdminDeckListItemResponse[]`

**Frontend notes**

- Admin table should support combined filtering by `status`, `visibility`, and `isOfficial`.
- `bookmarkCount` is intended for table insight only, not for optimistic mutations.

### GET `/api/admin/decks/{deckId}` 🔑

Get full deck detail for admin.

**Response data:** `AdminDeckDetailResponse`

### POST `/api/admin/decks` 🔑

Create a new deck from admin panel.

**Request body**

```json
{
  "title": "JLPT N5 Core Vocabulary",
  "description": "Optional",
  "coverImageUrl": null,
  "visibility": "Public",
  "status": "Draft",
  "isOfficial": true,
  "typeId": "string | null",
  "createdBy": "string"
}
```

**Response data:** `AdminDeckDetailResponse`

**Frontend notes**

- `createdBy` should normally default to the current admin user.
- If admin UI does not support impersonated ownership yet, keep `createdBy` hidden and let backend fill it.

### PATCH `/api/admin/decks/{deckId}` 🔑

Update deck metadata.

**Request body**

```json
{
  "title": "Updated title",
  "description": "Updated description",
  "coverImageUrl": "https://cdn.example.com/decks/cover.png",
  "visibility": "Public",
  "status": "Published",
  "isOfficial": true,
  "typeId": "string | null"
}
```

**Response data:** `AdminDeckDetailResponse`

**Frontend notes**

- `typeId: null` removes the current type.
- Admin can directly update `status`, but dedicated status actions below are still recommended for table UX.

### DELETE `/api/admin/decks/{deckId}` 🔑

Delete a deck.

**Response data**

```json
true
```

**Frontend notes**

- Delete should cascade to folders, folder cards, and bookmarks.
- Admin UI should confirm destructive action explicitly.

### POST `/api/admin/decks/{deckId}/publish` 🔑

Publish a deck.

**Request body**

```json
{}
```

**Response data:** `AdminDeckDetailResponse`

### POST `/api/admin/decks/{deckId}/archive` 🔑

Archive a deck.

**Request body**

```json
{}
```

**Response data:** `AdminDeckDetailResponse`

### POST `/api/admin/decks/{deckId}/unpublish` 🔑

Move a deck back to draft.

**Request body**

```json
{}
```

**Response data:** `AdminDeckDetailResponse`

**Frontend notes**

- Recommended table actions:
  - `Draft -> Publish`
  - `Published -> Unpublish`
  - `Draft/Published -> Archive`
- Admin status actions should refresh both detail and table query caches.

### Folder and card management endpoints

Admin folder and card mutations mirror the personal deck endpoints, but without owner restriction.

### POST `/api/admin/decks/{deckId}/folders` 🔑

**Request body**

```json
{
  "title": "Basic expressions",
  "description": "Optional",
  "position": 1000
}
```

**Response data:** `DeckFolderResponse`

### PUT `/api/admin/decks/{deckId}/folders/order` 🔑

**Request body**

```json
{
  "items": [
    { "folderId": "folder-1", "position": 1000 },
    { "folderId": "folder-2", "position": 2000 }
  ]
}
```

**Response data:** `DeckFolderResponse[]`

### PATCH `/api/admin/folders/{folderId}` 🔑

**Request body**

```json
{
  "title": "Updated folder title",
  "description": "Updated description"
}
```

**Response data:** `DeckFolderResponse`

### DELETE `/api/admin/folders/{folderId}` 🔑

**Response data**

```json
true
```

### POST `/api/admin/folders/{folderId}/cards` 🔑

**Request body**

```json
{
  "cardId": "string",
  "position": 1000
}
```

**Response data:** `DeckFolderResponse`

### DELETE `/api/admin/folders/{folderId}/cards/{cardId}` 🔑

**Response data**

```json
true
```

### PUT `/api/admin/folders/{folderId}/cards/order` 🔑

**Request body**

```json
{
  "items": [
    { "cardId": "card-1", "position": 1000 },
    { "cardId": "card-2", "position": 2000 }
  ]
}
```

**Response data:** `DeckFolderCardItemResponse[]`

**Frontend notes**

- Reorder payloads remain full-replacement payloads.
- The same card must not appear twice in the same deck, even across folders.
- Admin can add cards regardless of original deck ownership, but card-level publish/edit policy should still follow existing admin card rules.

### Admin error codes

| Code | Description |
| ---- | ----------- |
| `Deck_NotFound_404` | Deck does not exist |
| `Deck_FolderNotFound_404` | Folder does not exist |
| `Deck_CardNotFound_404` | Card does not exist or is not found in target folder |
| `Deck_CardDuplicatedInDeck_400` | The same card already exists somewhere else in the deck |
| `Deck_InvalidReorderPayload_400` | Reorder payload is incomplete, duplicated, or inconsistent |
| `DeckType_NotFound_404` | Deck type does not exist |
| `DeckType_NameExists_409` | Deck type name already exists |
| `DeckType_InUse_400` | Deck type cannot be deleted because at least one deck still uses it |

### Frontend integration notes for `learning-admin`

#### Suggested screen split

1. Deck list page
- Main table uses `GET /api/admin/decks`.
- Filters should include `q`, `status`, `visibility`, `typeId`, and `isOfficial`.
- Row actions should include `Publish`, `Unpublish`, `Archive`, `Edit`, and `Delete`.

2. Deck edit page
- Load `GET /api/admin/decks/{deckId}` once on page open.
- Use the same detail payload as the source of truth for deck metadata, folders, and folder cards.
- After every write mutation, either invalidate detail and refetch or patch local state carefully.

3. Deck type page
- Table or modal list uses `GET /api/admin/deck-types`.
- Create and rename can be inline or modal-based.
- Delete must handle `DeckType_InUse_400` explicitly.

#### Recommended query key shape

If `learning-admin` uses React Query, keep query keys stable and filter-driven.

```ts
const deckAdminKeys = {
  all: ['admin-decks'] as const,
  lists: () => [...deckAdminKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...deckAdminKeys.lists(), params] as const,
  details: () => [...deckAdminKeys.all, 'detail'] as const,
  detail: (deckId: string) => [...deckAdminKeys.details(), deckId] as const,
  deckTypes: ['admin-deck-types'] as const,
  deckTypeList: (params: Record<string, unknown>) => [...deckAdminKeys.deckTypes, 'list', params] as const,
  deckTypeDetail: (id: string) => [...deckAdminKeys.deckTypes, 'detail', id] as const,
}
```

#### Query param serialization notes

- Send enums exactly as backend expects:
  - `status`: `Draft`, `Published`, `Archived`
  - `visibility`: `Public`, `Private`
- Do not send empty strings for optional filters.
- For list filters, omit nullish values from query params instead of sending `status=` or `typeId=`.
- Keep `page` and `pageSize` numeric.

#### Mutation invalidation rules

1. After `POST/PATCH/DELETE /api/admin/deck-types`
- Invalidate `admin-deck-types` list queries.
- If editing inside a deck form, refresh the deck type dropdown source.

2. After `POST/PATCH/DELETE /api/admin/decks`
- Invalidate deck list queries.
- Invalidate the affected deck detail query when applicable.

3. After `POST /api/admin/decks/{deckId}/publish`
- Invalidate deck list queries.
- Invalidate detail for that `deckId`.

4. After `POST /api/admin/decks/{deckId}/archive`
- Invalidate deck list queries.
- Invalidate detail for that `deckId`.

5. After `POST /api/admin/decks/{deckId}/unpublish`
- Invalidate deck list queries.
- Invalidate detail for that `deckId`.

6. After folder or card mutations
- At minimum invalidate detail for the owning deck.
- If the admin table shows `cardsCount` or `foldersCount`, also invalidate deck list queries.

#### Form behavior notes

- `Create deck` should default to:
  - `visibility = Public`
  - `status = Draft`
  - `isOfficial = false`
- `createdBy` can stay hidden in the first admin UI version and let backend fall back to current admin user.
- `typeId = null` should be treated as "no type selected".
- `coverImageUrl` is plain text URL in current backend contract. There is no dedicated upload endpoint for deck cover yet.

#### Drag and drop notes

- Folder reorder endpoint requires the full folder set in one payload.
- Folder card reorder endpoint requires the full card set in one payload.
- Frontend should not send only changed rows.
- Sparse positions like `1000`, `2000`, `3000` are accepted, but backend also accepts any integer values as long as the payload is complete.

#### Error handling notes

- Business errors still come back inside the standard JSON envelope, not necessarily as HTTP 4xx.
- Frontend should inspect:
  - `success`
  - `code`
  - `message`
- Recommended error mapping:
  - `DeckType_NameExists_409`: show duplicate-name message on deck type form
  - `DeckType_InUse_400`: show dependency warning on delete action
  - `Deck_CardDuplicatedInDeck_400`: show "card already exists in this deck"
  - `Deck_InvalidReorderPayload_400`: refetch current detail state and ask user to retry reorder
  - `Deck_NotFound_404` or `Deck_FolderNotFound_404`: redirect back to list if the resource was removed elsewhere

#### Minimal service split recommendation

- `deckAdminService`
  - `getDecks`
  - `getDeckDetail`
  - `createDeck`
  - `updateDeck`
  - `deleteDeck`
  - `publishDeck`
  - `archiveDeck`
  - `unpublishDeck`
  - `createFolder`
  - `updateFolder`
  - `deleteFolder`
  - `addCardToFolder`
  - `removeCardFromFolder`
  - `reorderFolders`
  - `reorderFolderCards`

- `deckTypeAdminService`
  - `getDeckTypes`
  - `getDeckTypeDetail`
  - `createDeckType`
  - `updateDeckType`
  - `deleteDeckType`

### Suggested admin frontend flows

1. Deck admin table
- Load `GET /api/admin/deck-types` for type filter
- Load `GET /api/admin/decks` with status and visibility filters
- Open `GET /api/admin/decks/{deckId}` from table row

2. Deck editor
- Create draft with `POST /api/admin/decks`
- Add folders and cards incrementally
- Save drag-and-drop order with full reorder payloads
- Publish from detail page or table action

3. Deck type management
- Load `GET /api/admin/deck-types`
- Create or rename types inline or in modal
- Handle in-use delete failure gracefully

---

## Phụ lục: Tổng hợp Error Codes

### Common

| Code         | HTTP | Mô tả                |
| ------------ | ---- | -------------------- |
| `Common_500` | 500  | Lỗi server nội bộ    |
| `Common_404` | 404  | Không tìm thấy       |
| `Common_400` | 400  | Yêu cầu không hợp lệ |
| `Common_401` | 401  | Không có quyền       |

### Auth

| Code                         | Mô tả                                  |
| ---------------------------- | -------------------------------------- |
| `Invalid_400`                | Sai email/password khi login           |
| `Email_Exist_409`            | Email đã tồn tại khi register          |
| `Token_Expired_409`          | Refresh token / reset token hết hạn    |
| `Wrong_Current_Password_400` | Sai mật khẩu hiện tại khi đổi mật khẩu |

### Vocabulary

| Code                                  | Mô tả                                  |
| ------------------------------------- | -------------------------------------- |
| `Vocabulary_CardNotFound_404`         | Card không tồn tại                     |
| `Vocabulary_DetailNotFound_404`       | Chi tiết vocabulary không tìm thấy     |
| `Vocabulary_ReadForbidden_401`        | Không có quyền xem card chưa Published |
| `Vocabulary_AudioSynthesisFailed_500` | Lỗi generate audio VOICEVOX            |

### Grammar

| Code                              | Mô tả                                  |
| --------------------------------- | -------------------------------------- |
| `Grammar_CardNotFound_404`        | Card không tồn tại                     |
| `Grammar_DetailNotFound_404`      | Chi tiết grammar không tìm thấy        |
| `Grammar_ReadForbidden_401`       | Không có quyền xem card chưa Published |
| `Grammar_InvalidRelation_400`     | Relation không hợp lệ                  |
| `Grammar_RelatedCardNotFound_404` | Card liên quan không tìm thấy          |
| `Grammar_InvalidRichText_400`     | Rich text sai cú pháp                  |

### Sentence

| Code                                | Mô tả                       |
| ----------------------------------- | --------------------------- |
| `Sentence_NotFound_404`             | Sentence không tồn tại      |
| `Sentence_AudioSynthesisFailed_500` | Lỗi generate audio VOICEVOX |

### Deck

| Code | Mô tả |
| ---- | ----- |
| `Deck_NotFound_404` | Deck không tồn tại |
| `Deck_FolderNotFound_404` | Folder không tồn tại hoặc không thuộc user hiện tại |
| `Deck_CardNotFound_404` | Card không tồn tại hoặc không có trong folder mục tiêu |
| `Deck_Forbidden_403` | Không có quyền xem hoặc sửa deck |
| `Deck_ForkSourceInvalid_400` | Deck nguồn để fork không hợp lệ |
| `Deck_CardDuplicatedInDeck_400` | Card đã tồn tại ở folder khác trong cùng deck |
| `Deck_InvalidReorderPayload_400` | Payload reorder không hợp lệ |
