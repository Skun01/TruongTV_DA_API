# Tacho Learning API — Frontend Integration Guide

> **Last updated:** 2026-05-03

---

## Mục lục

1. [Quy ước chung](#1-quy-ước-chung)
2. [Enum Reference](#2-enum-reference)
3. [Auth Module](#3-auth-module)
3.1. [Admin Users Module](#31-admin-users-module)
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
14. [Learning Module — User](#14-learning-module--user)
15. [Learning Module — Admin (Proposed)](#15-learning-module--admin-proposed)
16. [JLPT Exams Module — Admin](#16-jlpt-exams-module--admin)
17. [JLPT Questions Module — Admin](#17-jlpt-questions-module--admin)
18. [JLPT AI Questions Module — Admin](#18-jlpt-ai-questions-module--admin)
19. [JLPT Exam Sessions Module — User](#19-jlpt-exam-sessions-module--user)
20. [Shadowing Module](#20-shadowing-module)

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

### StudyMode

| Value | Description |
| ----- | ----------- |
| `FillInBlank` | Fill-in-blank exercise |
| `MultipleChoice` | Multiple-choice exercise |
| `Flashcard` | Flashcard review |

### FlashcardContentType

| Value | Description |
| ----- | ----------- |
| `Title` | Use card `title` |
| `Summary` | Use card `summary` |

### MultipleChoiceQuestionType

| Value | Description |
| ----- | ----------- |
| `TitleToSummary` | Question is `title`, answer options are `summary` |
| `SummaryToTitle` | Question is `summary`, answer options are `title` |

### FlashcardReviewResult

| Value | Description |
| ----- | ----------- |
| `Learning` | User still does not know the card |
| `Known` | User knows the card |

### SrsLevel

| Value | Description |
| ----- | ----------- |
| `level_1` | Review again after `4 hours` |
| `level_2` | Review again after `8 hours` |
| `level_3` | Review again after `23 hours` |
| `level_4` | Review again after `2 days` |
| `level_5` | Review again after `4 days` |
| `level_6` | Review again after `8 days` |
| `level_7` | Review again after `2 weeks` |
| `level_8` | Review again after `1 month` |
| `level_9` | Review again after `2 months` |
| `level_10` | Review again after `4 months` |
| `level_11` | Review again after `8 months` |
| `level_12` | Mastered, no more review |

### SectionType (JLPT)

| Value    | Mô tả                         |
| -------- | ----------------------------- |
| `Moji`   | Chữ / từ vựng                 |
| `Bunpou` | Ngữ pháp                      |
| `Dokkai` | Đọc hiểu                      |
| `Choukai`| Nghe hiểu                     |

### OptionLabel (JLPT)

| Value | Mô tả       |
| ----- | ----------- |
| `A`   | Đáp án A    |
| `B`   | Đáp án B    |
| `C`   | Đáp án C    |
| `D`   | Đáp án D    |

### OptionType (JLPT)

| Value          | Mô tả                  |
| -------------- | ---------------------- |
| `Text`         | Chỉ có text            |
| `Image`        | Chỉ có hình ảnh        |
| `TextAndImage` | Có cả text và hình ảnh |

### ExamSessionStatus (JLPT)

| Value        | Mô tả                    |
| ------------ | ------------------------ |
| `InProgress` | Đang làm bài             |
| `Submitted`  | Đã nộp bài               |
| `TimedOut`   | Hết giờ, bị đóng tự động |

### AiQuestionStatus (JLPT)

| Value      | Mô tả                             |
| ---------- | --------------------------------- |
| `Pending`  | Mới sinh bởi AI, chưa review      |
| `Edited`   | Đã được biên tập viên chỉnh sửa   |
| `Approved` | Đã duyệt, đã chuyển vào question bank |
| `Rejected` | Đã từ chối                        |

### ChoukaiMondaiType (JLPT)

| Value      | Mô tả |
| ---------- | ----- |
| `Mondai1`  | Dạng bài nghe 1 |
| `Mondai2`  | Dạng bài nghe 2 |
| `Mondai3`  | Dạng bài nghe 3 |
| `Mondai4`  | Dạng bài nghe 4 |
| `Mondai5`  | Dạng bài nghe 5 |

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

## 3.1 Admin Users Module

> API quản trị người dùng dành cho trang admin. Tất cả endpoint trong module này yêu cầu role `admin`.

### Overview

| Method | Endpoint | Auth | Purpose |
| ------ | -------- | ---- | ------- |
| GET | `/api/admin/users` | 🔑 Admin | Search and filter users |
| GET | `/api/admin/users/{id}` | 🔑 Admin | Get user detail |
| PATCH | `/api/admin/users/{id}/role` | 🔑 Admin | Change user role |
| PATCH | `/api/admin/users/{id}/status` | 🔑 Admin | Activate or deactivate account |
| PATCH | `/api/admin/users/{id}/verification` | 🔑 Admin | Update verification status |
| POST | `/api/admin/users/{id}/send-reset-password` | 🔑 Admin | Send reset password email |

### Access and behavior rules

- Chỉ `admin` mới gọi được các endpoint này.
- Khi đổi `role`, backend sẽ thu hồi refresh token hiện có của user mục tiêu.
- Khi khóa tài khoản (`isActive = false`), backend sẽ thu hồi refresh token hiện có của user mục tiêu.
- User bị khóa sẽ không thể login, refresh token, hoặc tiếp tục dùng access token cũ sau khi token được kiểm tra lại với DB.
- Admin không được tự đổi role của chính mình.
- Admin không được tự khóa chính mình.

### Shared response shapes

`AdminUserListItemResponse`

```json
{
  "id": "string",
  "email": "string",
  "displayName": "string",
  "avatarUrl": "string | null",
  "role": "user | editor | admin",
  "isActive": true,
  "isVerified": false,
  "createdAt": "2026-05-03T01:00:00Z",
  "updatedAt": "2026-05-03T01:00:00Z | null"
}
```

`AdminUserDetailResponse`

```json
{
  "id": "string",
  "email": "string",
  "displayName": "string",
  "avatarUrl": "string | null",
  "role": "user | editor | admin",
  "isActive": true,
  "isVerified": false,
  "createdAt": "2026-05-03T01:00:00Z",
  "updatedAt": "2026-05-03T01:00:00Z | null"
}
```

### GET `/api/admin/users` 🔑

Tìm kiếm và lọc danh sách người dùng cho màn hình quản trị.

**Query params**

| Param | Type | Default | Notes |
| ----- | ---- | ------- | ----- |
| `q` | `string` | `null` | Search theo `email` hoặc `displayName` |
| `role` | `string` | `null` | `user`, `editor`, `admin` |
| `isActive` | `boolean` | `null` | Lọc trạng thái hoạt động |
| `isVerified` | `boolean` | `null` | Lọc trạng thái xác minh |
| `page` | `int` | `1` | Pagination |
| `pageSize` | `int` | `20` | Max `100` |

**Response data:** `AdminUserListItemResponse[]`

### GET `/api/admin/users/{id}` 🔑

Lấy thông tin chi tiết của một người dùng theo id.

**Path params**

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `id` | `string` | Yes | User id |

**Response data:** `AdminUserDetailResponse`

### PATCH `/api/admin/users/{id}/role` 🔑

Đổi role của một người dùng.

**Path params**

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `id` | `string` | Yes | User id |

**Request body**

```json
{
  "role": "editor"
}
```

**Response data:** `AdminUserDetailResponse`

**Frontend notes**

- `role` chỉ chấp nhận `user`, `editor`, `admin`.
- Backend chặn admin tự đổi role của chính mình.
- Sau khi đổi role, user mục tiêu cần login lại để nhận token mới.

### PATCH `/api/admin/users/{id}/status` 🔑

Khóa hoặc mở khóa tài khoản người dùng.

**Path params**

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `id` | `string` | Yes | User id |

**Request body**

```json
{
  "isActive": false
}
```

**Response data:** `AdminUserDetailResponse`

**Frontend notes**

- `isActive = false` nghĩa là khóa tài khoản.
- Backend chặn admin tự khóa chính mình.
- Khi khóa tài khoản, refresh token hiện tại của user sẽ bị thu hồi.

### PATCH `/api/admin/users/{id}/verification` 🔑

Cập nhật trạng thái xác minh tài khoản.

**Path params**

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `id` | `string` | Yes | User id |

**Request body**

```json
{
  "isVerified": true
}
```

**Response data:** `AdminUserDetailResponse`

### POST `/api/admin/users/{id}/send-reset-password` 🔑

Gửi email đặt lại mật khẩu cho người dùng theo id.

**Path params**

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `id` | `string` | Yes | User id |

**Response data**

```json
true
```

**Frontend notes**

- API luôn trả `true` nếu xử lý thành công ở tầng service.
- Frontend có thể dùng API này cho action “Send reset email” trong user detail hoặc user list.

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
- Nếu bỏ trống `status`, import sẽ mặc định `Published`.
- `sentences[*]` hỗ trợ thêm cấu hình bài tập: `position`, `blankWord`, `hint`, `answerList`.

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
      "status": "Published",
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
          "position": 1,
          "speakerId": 3,
          "level": "N5",
          "blankWord": "食べる",
          "hint": "Động từ chính trong câu.",
          "answerList": ["食べる", "たべる"]
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
- Nếu bỏ trống `status`, import sẽ mặc định `Published`.
- `sentences[*]` hỗ trợ thêm cấu hình bài tập: `position`, `blankWord`, `hint`, `answerList`.

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
      "status": "Published",
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
          "position": 1,
          "speakerId": 3,
          "level": "N4",
          "blankWord": "聞きながら",
          "hint": "Mẫu vừa làm A vừa làm B.",
          "answerList": ["聞きながら"]
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
- Nếu bỏ trống `status`, import sẽ mặc định `Published`.

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
      "status": "Published", // ❌ nullable — PublishStatus
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

## 14. Learning Module — User

> User-facing study APIs for `learning-app`. All endpoints in this section require authentication.

### Overview

| Method | Endpoint | Auth | Purpose |
| ------ | -------- | ---- | ------- |
| POST | `/api/learning/sessions` | 🔒 Auth | Create a new study session from a deck and a selected set of cards |
| GET | `/api/learning/sessions/{sessionId}` | 🔒 Auth | Get session overview |
| DELETE | `/api/learning/sessions/{sessionId}` | 🔒 Auth | Delete a study session |
| GET | `/api/learning/history` | 🔒 Auth | Get recent study sessions |
| GET | `/api/learning/sessions/{sessionId}/next` | 🔒 Auth | Get the next question in the session |
| POST | `/api/learning/sessions/{sessionId}/submit` | 🔒 Auth | Submit one answer for the current card |
| GET | `/api/learning/sessions/{sessionId}/result` | 🔒 Auth | Get final or current session result summary |
| POST | `/api/learning/sessions/{sessionId}/restart` | 🔒 Auth | Create a new session from an existing session scope |
| GET | `/api/learning/review/today` | 🔒 Auth | Get today due-count summary |
| GET | `/api/learning/review/due-cards` | 🔒 Auth | Get globally due card progresses for the current user |
| GET | `/api/learning/progress/cards/{cardId}` | 🔒 Auth | Get current user progress for one card |
| GET | `/api/learning/settings/me` | 🔒 Auth | Get user default learning settings |
| PUT | `/api/learning/settings/me` | 🔒 Auth | Create or update user default learning settings |

### Authorization and access rules

- User must be authenticated.
- Session ownership is enforced. A user can only read, submit, restart, or delete their own sessions.
- If `deckId` is provided, it must be readable by the current user under existing deck visibility rules.
- If `deckId` is provided, `cardIds[]` must belong to the selected deck. Invalid scope returns `Learning_InvalidScope_400`.
- If `deckId` is omitted, backend creates a global review session from the provided `cardIds[]`.
- `GET /api/learning/review/due-cards` is global-only and does not accept `deckId`.

### Shared business rules

- Session creation now uses `cardIds[]`, not `folderIds[]`.
- If `deckId` is provided and `cardIds[]` is empty, backend uses all cards inside the selected deck.
- If `deckId` is omitted, `cardIds[]` is required.
- Session response still includes `folderIds`. These are derived from the selected cards only when the session belongs to a specific deck. Global review sessions return `[]`.
- Session settings are resolved with this precedence:
  1. `settings` sent in `POST /api/learning/sessions`
  2. user default settings from `GET /api/learning/settings/me`
  3. system defaults:
     - `flashcardFront = Title`
     - `flashcardBack = Summary`
     - `multipleChoiceQuestion = TitleToSummary`
     - `shuffleOptions = true`
- `MultipleChoice` distractors only use cards of the same `cardType` as the current card.
- `MultipleChoice` prefers distractors from the current session scope first, then falls back to same-type cards outside the session.
- `Flashcard` uses `flashcardResult = Known` as correct and `flashcardResult = Learning` as incorrect.
- `GET /api/learning/review/due-cards` is intended to feed `cardIds[]` into `POST /api/learning/sessions`.

### Request and response models

`StudySessionSettingsRequest`

```json
{
  "flashcardFront": "Title",
  "flashcardBack": "Summary",
  "multipleChoiceQuestion": "TitleToSummary",
  "shuffleOptions": true
}
```

| Field | Type | Required | Enum | Notes |
| ----- | ---- | -------- | ---- | ----- |
| `flashcardFront` | `string \| null` | No | `Title`, `Summary` | Overrides default flashcard front |
| `flashcardBack` | `string \| null` | No | `Title`, `Summary` | Overrides default flashcard back |
| `multipleChoiceQuestion` | `string \| null` | No | `TitleToSummary`, `SummaryToTitle` | Controls MCQ direction |
| `shuffleOptions` | `boolean \| null` | No | — | `null` means fallback to user default or system default |

`StudySessionSettingsResponse`

```json
{
  "flashcardFront": "Title",
  "flashcardBack": "Summary",
  "multipleChoiceQuestion": "TitleToSummary",
  "shuffleOptions": true
}
```

`StudySessionResponse`

```json
{
  "id": "session-id",
  "deckId": null,
  "deckTitle": null,
  "mode": "MultipleChoice",
  "folderIds": [],
  "totalCards": 20,
  "completedCards": 4,
  "remainingCards": 16,
  "correctCount": 3,
  "incorrectCount": 1,
  "createdAt": "2026-04-18T11:00:00Z",
  "completedAt": null,
  "settings": {
    "flashcardFront": "Title",
    "flashcardBack": "Summary",
    "multipleChoiceQuestion": "TitleToSummary",
    "shuffleOptions": true
  }
}
```

| Field | Type | Notes |
| ----- | ---- | ----- |
| `id` | `string` | Session id |
| `deckId` | `string \| null` | Owning deck id, `null` for global review session |
| `deckTitle` | `string \| null` | Owning deck title, `null` for global review session |
| `mode` | `StudyMode` | Session mode |
| `folderIds` | `string[]` | Derived folder scope from selected cards; empty for global review session |
| `totalCards` | `int` | Total cards in the session |
| `completedCards` | `int` | Number of cards already submitted |
| `remainingCards` | `int` | `totalCards - completedCards` |
| `correctCount` | `int` | Number of correct or known submissions |
| `incorrectCount` | `int` | Number of incorrect or learning submissions |
| `createdAt` | `datetime` | Session creation time |
| `completedAt` | `datetime \| null` | Set when all cards are submitted |
| `settings` | `StudySessionSettingsResponse` | Snapshot stored on the session |

`StudyQuestionResponse`

```json
{
  "sessionId": "session-id",
  "cardId": "card-id",
  "cardType": "Vocab",
  "mode": "MultipleChoice",
  "prompt": "Chọn nghĩa đúng của thẻ",
  "questionText": "食べる",
  "secondaryText": null,
  "hint": null,
  "frontText": null,
  "backText": null,
  "allowsMultipleSelection": false,
  "options": [
    { "id": "ăn", "text": "ăn" },
    { "id": "uống", "text": "uống" },
    { "id": "ngủ", "text": "ngủ" },
    { "id": "đi", "text": "đi" }
  ],
  "isCompleted": false
}
```

| Field | Type | When used |
| ----- | ---- | --------- |
| `sessionId` | `string` | Always |
| `cardId` | `string` | Always |
| `cardType` | `CardType` | Always |
| `mode` | `StudyMode` | Always |
| `prompt` | `string` | Always |
| `questionText` | `string \| null` | Fill-in-blank and multiple-choice |
| `secondaryText` | `string \| null` | Fill-in-blank support text, usually sentence meaning |
| `hint` | `string \| null` | Fill-in-blank only when configured |
| `frontText` | `string \| null` | Flashcard front |
| `backText` | `string \| null` | Flashcard back |
| `allowsMultipleSelection` | `boolean` | Currently always `false` for multiple-choice |
| `options` | `StudyQuestionOptionResponse[]` | Multiple-choice only |
| `isCompleted` | `boolean` | Currently returned as `false` for active question payloads |

`SubmitStudyAnswerResponse`

```json
{
  "isCorrect": true,
  "cardId": "card-id",
  "mode": "Flashcard",
  "acceptedAnswers": [],
  "srsLevel": "level_2",
  "nextReviewAt": "2026-04-19T11:05:00Z",
  "isMastered": false,
  "consecutiveCorrect": 1,
  "completedCards": 1,
  "remainingCards": 9
}
```

| Field | Type | Notes |
| ----- | ---- | ----- |
| `isCorrect` | `boolean` | For flashcard, `Known = true`, `Learning = false` |
| `cardId` | `string` | Submitted card id |
| `mode` | `StudyMode` | Mode used for evaluation |
| `acceptedAnswers` | `string[]` | Correct answers used to judge submission |
| `srsLevel` | `SrsLevel` | Progress level after update |
| `nextReviewAt` | `datetime` | Next due time after update |
| `isMastered` | `boolean` | `true` when level reaches `level_12` |
| `consecutiveCorrect` | `int` | Current correct streak |
| `completedCards` | `int` | Session progress after this submit |
| `remainingCards` | `int` | Remaining cards after this submit |

### POST `/api/learning/sessions` 🔒

Create a new study session.

**Request body**

```json
{
  "deckId": null,
  "cardIds": ["card-1", "card-2", "card-3"],
  "mode": "MultipleChoice",
  "settings": {
    "multipleChoiceQuestion": "TitleToSummary",
    "shuffleOptions": true
  }
}
```

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `deckId` | `string \| null` | No | Max length `100`; omit for global review session |
| `cardIds` | `string[]` | No | Each item max length `100`; if `deckId` exists, empty array means all cards in deck |
| `mode` | `string` | Yes | `FillInBlank`, `MultipleChoice`, or `Flashcard` |
| `settings` | `StudySessionSettingsRequest \| null` | No | Optional per-session override |

**Response data:** `StudySessionResponse`

**Frontend notes**

- Use `cardIds[]` when building a custom review session from `GET /api/learning/review/due-cards`.
- If frontend needs "study whole deck", send `cardIds: []`.
- If frontend needs a global due-card review session, omit `deckId` and send only the `cardIds[]` returned by `GET /api/learning/review/due-cards`.
- If frontend already filtered cards by folder, it should still send the final card ids only.

### GET `/api/learning/sessions/{sessionId}` 🔒

Get session overview.

**Response data:** `StudySessionResponse`

### DELETE `/api/learning/sessions/{sessionId}` 🔒

Delete a session owned by the current user.

**Response data**

```json
true
```

### GET `/api/learning/history` 🔒

Get recent sessions for the current user.

**Query params**

| Param | Type | Default | Notes |
| ----- | ---- | ------- | ----- |
| `limit` | `int` | `20` | Optional, must be `1..100` |

**Response data:** `StudySessionResponse[]`

**Frontend notes**

- Order is newest first.
- Use this endpoint for recent activity UI, "continue learning", or history list screens.

### GET `/api/learning/sessions/{sessionId}/next` 🔒

Get the next question for the session.

**Response data:** `StudyQuestionResponse | null`

Mode-specific behavior:

- `FillInBlank`
  - `prompt` explains fill-in action.
  - `questionText` contains the sentence or fallback prompt.
  - `secondaryText` usually contains sentence meaning or card title.
  - `hint` is returned when sentence metadata has a hint.
- `MultipleChoice`
  - `questionText` is `title` when `multipleChoiceQuestion = TitleToSummary`.
  - `questionText` is `summary` when `multipleChoiceQuestion = SummaryToTitle`.
  - `options[].text` follows the inverse side of the same rule.
  - Distractors use same `cardType` only.
- `Flashcard`
  - `frontText` and `backText` come from session settings.
  - `questionText` and `options` are not used.

**Frontend notes**

- `null` means the session is completed or no more cards remain.
- Frontend should call this endpoint after each successful submit to fetch the next card.

### POST `/api/learning/sessions/{sessionId}/submit` 🔒

Submit the answer for one card in the session.

**Request body**

```json
{
  "cardId": "card-id",
  "answers": [],
  "selectedOptionIds": ["ăn"],
  "flashcardResult": null
}
```

| Field | Type | Required | Used by | Validation |
| ----- | ---- | -------- | ------- | ---------- |
| `cardId` | `string` | Yes | All modes | Max length `100` |
| `answers` | `string[]` | No | Fill-in-blank | Each item max length `200` |
| `selectedOptionIds` | `string[]` | No | Multiple-choice | Each item max length `200` |
| `flashcardResult` | `string \| null` | No | Flashcard | `Learning` or `Known` |

**Valid examples**

Fill-in-blank:

```json
{
  "cardId": "card-id",
  "answers": ["食べる"],
  "selectedOptionIds": [],
  "flashcardResult": null
}
```

Multiple-choice:

```json
{
  "cardId": "card-id",
  "answers": [],
  "selectedOptionIds": ["Động từ ăn"],
  "flashcardResult": null
}
```

Flashcard:

```json
{
  "cardId": "card-id",
  "answers": [],
  "selectedOptionIds": [],
  "flashcardResult": "Known"
}
```

**Response data:** `SubmitStudyAnswerResponse`

**Frontend notes**

- Backend rejects submit if `cardId` is not in the session.
- Backend rejects duplicate submit for a card already completed in the same session.
- For flashcard UI, map buttons directly to:
  - `Learning`
  - `Known`

### GET `/api/learning/sessions/{sessionId}/result` 🔒

Get session result summary.

**Response data**

```json
{
  "sessionId": "session-id",
  "deckId": null,
  "deckTitle": null,
  "mode": "Flashcard",
  "totalCards": 20,
  "completedCards": 20,
  "correctCount": 14,
  "incorrectCount": 6,
  "accuracy": 70.0,
  "createdAt": "2026-04-18T11:00:00Z",
  "completedAt": "2026-04-18T11:20:00Z",
  "settings": {
    "flashcardFront": "Title",
    "flashcardBack": "Summary",
    "multipleChoiceQuestion": "TitleToSummary",
    "shuffleOptions": true
  }
}
```

| Field | Type | Notes |
| ----- | ---- | ----- |
| `accuracy` | `double` | Percentage rounded to 2 decimals |
| Other fields | Same semantics as session overview | — |

### POST `/api/learning/sessions/{sessionId}/restart` 🔒

Create a brand new session from the original session card scope and settings snapshot.

**Request body**

```json
{}
```

**Response data:** `StudySessionResponse`

**Frontend notes**

- Restart does not mutate the old session.
- Restart is useful for "study again" or "retry failed cards later" flows.

### GET `/api/learning/review/today` 🔒

Get due-card count summary for today.

**Query params**

| Param | Type | Default | Notes |
| ----- | ---- | ------- | ----- |
| `deckId` | `string` | `null` | Optional, max length `100` |
| `folderIds` | `string[]` | `[]` | Optional filter inside a deck; each item max length `100` |

**Response data**

```json
{
  "deckId": "deck-id",
  "folderIds": ["folder-1"],
  "dueCount": 12,
  "totalCards": 30
}
```

| Field | Type | Notes |
| ----- | ---- | ----- |
| `deckId` | `string \| null` | `null` when querying global due summary |
| `folderIds` | `string[]` | Echoes effective folder scope when filtering by deck |
| `dueCount` | `int` | Number of due cards matching the filter |
| `totalCards` | `int` | Total cards in the filter scope |

**Frontend notes**

- If `deckId` is omitted, backend returns total due cards across all progress records for the current user.
- If `deckId` is present and `folderIds` is empty, backend treats it as "all folders in that deck".

### GET `/api/learning/review/due-cards` 🔒

Get all globally due card ids for the current user.

**Query params**

No query params.

**Response data**

```json
{
  "dueCount": 3,
  "cardIds": ["card-1", "card-2", "card-3"]
}
```

| Field | Type | Notes |
| ----- | ---- | ----- |
| `dueCount` | `int` | Number of globally due cards |
| `cardIds` | `string[]` | Full list of due card ids. Use directly in `POST /api/learning/sessions` |

**Frontend notes**

- To create a review session:
  1. call `GET /api/learning/review/due-cards`
  2. let user choose any subset
  3. call `POST /api/learning/sessions` with:
     - `deckId = null`
     - selected `cardIds[]`

### GET `/api/learning/progress/cards/{cardId}` 🔒

Get current user progress for one card.

**Response data**

```json
{
  "cardId": "card-id",
  "cardType": "Vocab",
  "title": "食べる",
  "summary": "Động từ ăn",
  "srsLevel": "level_2",
  "nextReviewAt": "2026-04-19T11:00:00Z",
  "lastReviewedAt": "2026-04-18T11:00:00Z",
  "consecutiveCorrect": 1,
  "isMastered": false,
  "lastSentenceId": "sentence-id"
}
```

| Field | Type | Notes |
| ----- | ---- | ----- |
| `lastReviewedAt` | `datetime \| null` | `null` when never reviewed |
| `lastSentenceId` | `string \| null` | Used internally to reduce repeated fill-in sentence selection |

**Frontend notes**

- If the user never studied the card before, backend returns a synthetic default progress:
  - `srsLevel = level_1`
  - `consecutiveCorrect = 0`
  - `isMastered = false`
  - `nextReviewAt = current server time`

### GET `/api/learning/settings/me` 🔒

Get current user default learning settings.

**Response data:** `StudySessionSettingsResponse`

**Frontend notes**

- This endpoint returns defaults even if the user has never saved settings before.
- Current backend default values:
  - `flashcardFront = Title`
  - `flashcardBack = Summary`
  - `multipleChoiceQuestion = TitleToSummary`
  - `shuffleOptions = true`

### PUT `/api/learning/settings/me` 🔒

Create or update user default learning settings.

**Request body**

```json
{
  "flashcardFront": "Summary",
  "flashcardBack": "Title",
  "multipleChoiceQuestion": "SummaryToTitle",
  "shuffleOptions": false
}
```

**Response data:** `StudySessionSettingsResponse`

**Frontend notes**

- Partial update is supported. Omitted fields keep the previous value.
- Use this endpoint for persistent user preferences.
- Session-specific overrides should still be sent in `POST /api/learning/sessions`.

### Learning error codes

| Code | Description |
| ---- | ----------- |
| `Learning_SessionNotFound_404` | Session does not exist or does not belong to the current user |
| `Learning_SessionCompleted_400` | Session is already completed and can no longer accept submit |
| `Learning_InvalidMode_400` | Invalid study mode value |
| `Learning_InvalidScope_400` | Provided `cardIds` or `folderIds` do not belong to the selected deck |
| `Learning_CardNotInSession_400` | Submitted card is not part of the session |
| `Learning_InvalidSubmission_400` | Duplicate submit or invalid submit state |
| `Learning_NoCardsAvailable_400` | Session scope resolves to zero cards |

### Suggested frontend flows

1. Normal deck study
- Load deck detail or folder cards from existing deck APIs.
- Let user select mode and optional settings.
- Call `POST /api/learning/sessions`.
- Loop:
  - `GET /api/learning/sessions/{sessionId}/next`
  - `POST /api/learning/sessions/{sessionId}/submit`
- End with `GET /api/learning/sessions/{sessionId}/result`.

2. Review due cards
- Call `GET /api/learning/review/due-cards`.
- Let user select an arbitrary number of cards.
- Create one global review session with the selected `cardIds[]`.

3. Settings flow
- Load `GET /api/learning/settings/me` on settings page.
- Save persistent defaults with `PUT /api/learning/settings/me`.
- When opening a one-off study modal, send temporary overrides in `POST /api/learning/sessions` instead of mutating user defaults.

---

## 15. Learning Module — Admin

This section documents the currently implemented admin-facing APIs for learning management.

Authorization:

- all endpoints in this section require `🔑 Editor/Admin`

Current responsibilities covered:

- inspect one card’s learning configuration
- update card-level learning config and sentence metadata
- attach, remove, and reorder card sentences for learning
- list cards with learning-content issues
- inspect deck-level learning coverage
- inspect admin analytics at overview, deck, card, and user level
- preview the exact study content that user-facing APIs will generate

Recommended frontend admin screens that use this module:

- card learning config tab
- sentence management row editor / drag-and-drop list
- learning QA issue list
- deck learning coverage dashboard
- admin learning overview dashboard
- deck analytics page
- card analytics drawer/page
- user learning support page

### 15.1 Endpoint inventory

| Method | Route | Purpose |
| ------ | ----- | ------- |
| `GET` | `/api/admin/learning/cards/{cardId}/config` | Load the full learning configuration of one card |
| `PUT` | `/api/admin/learning/cards/{cardId}/config` | Save summary and all sentence learning metadata for one card |
| `POST` | `/api/admin/learning/cards/{cardId}/sentences` | Attach one sentence to the card with learning metadata |
| `PUT` | `/api/admin/learning/cards/{cardId}/sentences/{sentenceId}` | Update one attached sentence relation |
| `DELETE` | `/api/admin/learning/cards/{cardId}/sentences/{sentenceId}` | Remove one attached sentence relation |
| `POST` | `/api/admin/learning/cards/{cardId}/sentences/reorder` | Reorder sentence positions for one card |
| `GET` | `/api/admin/learning/cards/issues` | List cards that currently have learning-content issues |
| `GET` | `/api/admin/learning/decks/{deckId}/coverage` | Return learning readiness statistics for one deck |
| `GET` | `/api/admin/learning/overview` | Return top-level learning analytics for the admin dashboard |
| `GET` | `/api/admin/learning/decks/{deckId}/analytics` | Return session and progress analytics for one deck |
| `GET` | `/api/admin/learning/cards/{cardId}/analytics` | Return progress and usage analytics for one card |
| `GET` | `/api/admin/learning/users/{userId}/progress` | Return summary learning progress for one user |
| `GET` | `/api/admin/learning/cards/{cardId}/preview` | Preview one generated exercise for the selected card and mode |

### 15.1.1 Endpoint groups by frontend use case

Use this grouping when splitting frontend work across pages or components.

#### Card config page

- `GET /api/admin/learning/cards/{cardId}/config`
- `PUT /api/admin/learning/cards/{cardId}/config`
- `POST /api/admin/learning/cards/{cardId}/sentences`
- `PUT /api/admin/learning/cards/{cardId}/sentences/{sentenceId}`
- `DELETE /api/admin/learning/cards/{cardId}/sentences/{sentenceId}`
- `POST /api/admin/learning/cards/{cardId}/sentences/reorder`
- `GET /api/admin/learning/cards/{cardId}/preview`

#### QA / validation pages

- `GET /api/admin/learning/cards/issues`
- `GET /api/admin/learning/decks/{deckId}/coverage`

#### Analytics pages

- `GET /api/admin/learning/overview`
- `GET /api/admin/learning/decks/{deckId}/analytics`
- `GET /api/admin/learning/cards/{cardId}/analytics`
- `GET /api/admin/learning/users/{userId}/progress`

#### Typical admin flow

1. Open card detail and call `GET /api/admin/learning/cards/{cardId}/config`.
2. Edit summary or sentence metadata with `PUT /config` or row-level sentence APIs.
3. Preview the generated exercise with `GET /api/admin/learning/cards/{cardId}/preview`.
4. Check deck readiness with `GET /api/admin/learning/decks/{deckId}/coverage`.
5. Investigate broken content with `GET /api/admin/learning/cards/issues`.

### 15.2 Shared concepts

#### LearningIssueType

| Value | Meaning |
| ----- | ------- |
| `MissingSummary` | Card summary is empty |
| `MissingSentence` | Non-Kanji card has no attached sentence for fill-in mode |
| `MissingAnswerList` | Sentence has no `answerList` and no `blankWord` |
| `BlankWordNotFoundInSentence` | `blankWord` does not appear in the sentence text |
| `DuplicateSentencePosition` | Sentence positions are duplicated or invalid |
| `UnsupportedCardTypeForMode` | Reserved for future use |

#### Readiness rules

- `fill_in_blank`
  - `Kanji` cards are considered ready by fallback behavior
  - other card types need at least one attached sentence
  - duplicated or invalid sentence positions make the card not ready
- `multiple_choice`
  - requires non-empty `summary`
- `flashcard`
  - requires non-empty `summary`

#### Source of truth

- fill-in metadata is stored on `card_sentences`
- important fields:
  - `position`
  - `blankWord`
  - `hint`
  - `answerList`

### 15.3 `GET /api/admin/learning/cards/{cardId}/config`

Load the admin editing payload for one card.

Typical use cases:

- open the learning tab in card detail
- inspect whether the card is ready for each study mode
- load sentence-level fill-in config in one request

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `cardId` | `string` | Yes | Target card id |

Success response:

```json
{
  "cardId": "card-id",
  "cardType": "Vocab",
  "title": "食べる",
  "summary": "to eat",
  "isFillInBlankReady": true,
  "isMultipleChoiceReady": true,
  "isFlashcardReady": true,
  "availableModes": ["FillInBlank", "MultipleChoice", "Flashcard"],
  "issues": [],
  "sentences": [
    {
      "sentenceId": "sentence-1",
      "position": 1,
      "jp": "毎日パンを食べる。",
      "en": "I eat bread every day.",
      "audioUrl": "https://...",
      "level": "N5",
      "blankWord": "食べる",
      "hint": "Dictionary form",
      "answerList": ["食べる", "たべる"]
    }
  ]
}
```

Response field notes:

| Field | Type | Notes |
| ----- | ---- | ----- |
| `cardId` | `string` | Card id |
| `cardType` | `CardType` | `Vocab`, `Grammar`, `Kanji` |
| `title` | `string` | Card title |
| `summary` | `string` | Card summary used by multiple-choice and flashcard |
| `isFillInBlankReady` | `bool` | Calculated by backend |
| `isMultipleChoiceReady` | `bool` | Calculated by backend |
| `isFlashcardReady` | `bool` | Calculated by backend |
| `availableModes` | `string[]` | Calculated available user study modes |
| `issues` | `LearningAdminCardIssueItemResponse[]` | Detailed problems found on the card |
| `sentences` | `LearningAdminCardSentenceConfigResponse[]` | Attached sentences ordered by `position` |

Frontend notes:

- `issues` is always based on current backend validation logic
- admin UI can use `availableModes` directly for badges or readiness chips
- this endpoint is the best source for initializing the full card learning form state

### 15.4 `PUT /api/admin/learning/cards/{cardId}/config`

Save the card-level learning configuration in a single request.

This endpoint updates:

- card `summary`
- the complete attached sentence config list for the card

Behavior:

- sentences sent in `sentences[]` are kept or updated
- sentences currently attached to the card but omitted from the payload are removed from the relation
- `answerList` is normalized by trim + distinct
- if `blankWord` is present and not already in `answerList`, backend inserts it automatically

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `cardId` | `string` | Yes | Target card id |

Request body:

```json
{
  "summary": "to eat",
  "sentences": [
    {
      "sentenceId": "sentence-1",
      "position": 1,
      "blankWord": "食べる",
      "hint": "Dictionary form",
      "answerList": ["食べる", "たべる"]
    },
    {
      "sentenceId": "sentence-2",
      "position": 2,
      "blankWord": "食べます",
      "hint": "Polite form",
      "answerList": ["食べます", "たべます"]
    }
  ]
}
```

Request field rules:

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `summary` | `string` | Yes | Must not be empty, max `1000` |
| `sentences` | `object[]` | Yes | Full desired sentence config list |
| `sentences[].sentenceId` | `string` | Yes | Must reference an existing sentence, max `50` |
| `sentences[].position` | `int` | Yes | Must be `> 0`, unique within the request |
| `sentences[].blankWord` | `string?` | No | Max `500` |
| `sentences[].hint` | `string?` | No | Max `1000` |
| `sentences[].answerList` | `string[]` | No | Backend normalizes values |

Validation notes:

- duplicated `sentenceId` values are rejected
- duplicated `position` values are rejected
- non-existing `sentenceId` returns `Sentence_NotFound_404`

Success response:

- same shape as `GET /api/admin/learning/cards/{cardId}/config`

Frontend notes:

- this is the preferred save endpoint for the admin learning tab
- frontend should send the full desired sentence config list, not only changed items
- if your UI already has row-level edits, you can still keep `PUT /config` as the single save action for the whole tab

### 15.5 `POST /api/admin/learning/cards/{cardId}/sentences`

Attach one existing sentence to the card with learning metadata.

This endpoint is useful when admin UI adds a sentence row from a searchable sentence picker.

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `cardId` | `string` | Yes | Target card id |

Request body:

```json
{
  "sentenceId": "sentence-1",
  "position": 3,
  "blankWord": "食べる",
  "hint": "Dictionary form",
  "answerList": ["食べる", "たべる"]
}
```

Request field rules:

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `sentenceId` | `string` | Yes | Existing sentence id, max `50` |
| `position` | `int` | Yes | Must be `> 0` |
| `blankWord` | `string?` | No | Max `500` |
| `hint` | `string?` | No | Max `1000` |
| `answerList` | `string[]` | No | Backend normalizes values |

Success response:

```json
{
  "sentenceId": "sentence-1",
  "position": 3,
  "jp": "毎日パンを食べる。",
  "en": "I eat bread every day.",
  "audioUrl": "https://...",
  "level": "N5",
  "blankWord": "食べる",
  "hint": "Dictionary form",
  "answerList": ["食べる", "たべる"]
}
```

Frontend notes:

- backend rejects duplicate relations with `Learning_SentenceAlreadyAttached_400`
- this endpoint does not modify other attached sentences
- good fit for a modal or command-palette style sentence picker

### 15.6 `PUT /api/admin/learning/cards/{cardId}/sentences/{sentenceId}`

Update the learning metadata of one sentence relation already attached to the card.

Use this endpoint when admin UI edits one sentence row inline instead of saving the whole config form.

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `cardId` | `string` | Yes | Target card id |
| `sentenceId` | `string` | Yes | Attached sentence id |

Request body:

```json
{
  "position": 1,
  "blankWord": "食べる",
  "hint": "Dictionary form",
  "answerList": ["食べる", "たべる"]
}
```

Request field rules:

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `position` | `int` | Yes | Must be `> 0` |
| `blankWord` | `string?` | No | Max `500` |
| `hint` | `string?` | No | Max `1000` |
| `answerList` | `string[]` | No | Backend normalizes values |

Success response:

```json
{
  "sentenceId": "sentence-1",
  "position": 1,
  "jp": "毎日パンを食べる。",
  "en": "I eat bread every day.",
  "audioUrl": "https://...",
  "level": "N5",
  "blankWord": "食べる",
  "hint": "Dictionary form",
  "answerList": ["食べる", "たべる"]
}
```

Frontend notes:

- if the sentence is not attached to the card, backend returns `Learning_SentenceNotAttached_404`
- this endpoint does not add a new relation; it only updates an existing one

### 15.7 `DELETE /api/admin/learning/cards/{cardId}/sentences/{sentenceId}`

Remove one attached sentence relation from the card.

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `cardId` | `string` | Yes | Target card id |
| `sentenceId` | `string` | Yes | Attached sentence id |

Success response:

```json
true
```

Frontend notes:

- if the sentence is not attached, backend returns `Learning_SentenceNotAttached_404`
- deleting the relation does not delete the sentence entity itself

### 15.8 `POST /api/admin/learning/cards/{cardId}/sentences/reorder`

Update sentence positions for one card in bulk.

This endpoint is intended for drag-and-drop reorder UIs.

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `cardId` | `string` | Yes | Target card id |

Request body:

```json
{
  "items": [
    { "sentenceId": "sentence-1", "position": 1 },
    { "sentenceId": "sentence-2", "position": 2 },
    { "sentenceId": "sentence-3", "position": 3 }
  ]
}
```

Request field rules:

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `items` | `object[]` | Yes | Must not be empty |
| `items[].sentenceId` | `string` | Yes | Must already be attached to the card |
| `items[].position` | `int` | Yes | Must be `> 0` |

Validation notes:

- duplicated `sentenceId` values are rejected
- duplicated `position` values are rejected

Success response:

```json
[
  {
    "sentenceId": "sentence-1",
    "position": 1,
    "jp": "毎日パンを食べる。",
    "en": "I eat bread every day.",
    "audioUrl": "https://...",
    "level": "N5",
    "blankWord": "食べる",
    "hint": "Dictionary form",
    "answerList": ["食べる", "たべる"]
  }
]
```

Frontend notes:

- the response returns the full attached sentence list ordered by the new positions
- if any sentence in the request is not attached to the card, backend returns `Learning_SentenceNotAttached_404`
- after reorder succeeds, frontend can replace local sentence state with the response payload directly

### 15.9 `GET /api/admin/learning/cards/issues`

List cards that currently have one or more learning-content issues.

This endpoint is designed for:

- admin QA pages
- publish readiness dashboards
- filtered issue lists by deck, mode, or issue type

Query params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `page` | `int` | No | Default `1` |
| `pageSize` | `int` | No | Default `20`, max `100` |
| `cardType` | `CardType?` | No | `Vocab`, `Grammar`, `Kanji` |
| `mode` | `StudyMode?` | No | `FillInBlank`, `MultipleChoice`, `Flashcard` |
| `issueType` | `LearningIssueType?` | No | One issue category |
| `q` | `string?` | No | Search by card title or summary |
| `deckId` | `string?` | No | Restrict result to cards currently used by one deck |

Success response item:

```json
{
  "cardId": "card-id",
  "cardType": "Grammar",
  "title": "〜ている",
  "summary": "",
  "availableModes": ["FillInBlank"],
  "issues": [
    {
      "type": "MissingSummary",
      "message": "Card summary is required for flashcard and multiple-choice.",
      "sentenceId": null
    },
    {
      "type": "BlankWordNotFoundInSentence",
      "message": "blankWord does not appear in the attached sentence text.",
      "sentenceId": "sentence-1"
    }
  ]
}
```

Filtering behavior:

- `mode=FillInBlank` only keeps fill-in-related issue types
- `mode=MultipleChoice` only keeps multiple-choice-related issue types
- `mode=Flashcard` only keeps flashcard-related issue types
- `issueType` is applied after the optional `mode` filter

Frontend notes:

- only cards with at least one matching issue are returned
- use `metaData.total` for issue count after filters
- `availableModes` already reflects the remaining usable modes for the card
- ideal query key shape for frontend caching: `learning-admin-issues`, `{ page, pageSize, cardType, mode, issueType, q, deckId }`

### 15.10 `GET /api/admin/learning/decks/{deckId}/coverage`

Return learning readiness statistics for one deck.

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `deckId` | `string` | Yes | Target deck id |

Success response:

```json
{
  "deckId": "deck-id",
  "deckTitle": "N5 Week 1",
  "totalCards": 120,
  "fillInBlankReadyCount": 72,
  "multipleChoiceReadyCount": 110,
  "flashcardReadyCount": 118,
  "issueCount": 24,
  "cardsByType": [
    {
      "cardType": "Vocab",
      "total": 80,
      "fillInBlankReady": 60,
      "multipleChoiceReady": 78,
      "flashcardReady": 80
    },
    {
      "cardType": "Grammar",
      "total": 30,
      "fillInBlankReady": 12,
      "multipleChoiceReady": 25,
      "flashcardReady": 28
    }
  ]
}
```

Response notes:

- `totalCards` counts distinct cards in the deck
- `issueCount` counts cards that currently have at least one issue
- `cardsByType` groups the same readiness metrics by `cardType`

Frontend notes:

- ideal for deck QA pages and publish-readiness banners
- combine with `GET /api/admin/learning/cards/issues?deckId=...` for drill-down
- this endpoint is summary-oriented; use `cards/issues` when frontend needs actionable rows

### 15.11 `GET /api/admin/learning/overview`

Return top-level learning analytics for the admin dashboard.

This endpoint is derived from:

- `study_sessions` created from the start of the current UTC day
- global due-card count from `user_card_progress`

Success response:

```json
{
  "activeUsersToday": 245,
  "sessionsToday": 612,
  "completedSessionsToday": 480,
  "submissionsToday": 4810,
  "dueCardsNow": 1930,
  "averageAccuracy": 78.42
}
```

Response field notes:

| Field | Type | Notes |
| ----- | ---- | ----- |
| `activeUsersToday` | `int` | Distinct users with at least one session created today |
| `sessionsToday` | `int` | Total sessions created today |
| `completedSessionsToday` | `int` | Sessions with `completedAt != null` created today |
| `submissionsToday` | `int` | Sum of `correctCount + incorrectCount` across today’s sessions |
| `dueCardsNow` | `int` | Global count of non-mastered due progress rows at request time |
| `averageAccuracy` | `double` | Accuracy percentage over today’s session submissions |

Frontend notes:

- this endpoint is suitable for admin dashboard summary cards
- all “today” metrics are computed from the current UTC day on the server
- polling is optional; if you do poll, keep it coarse because these are aggregate metrics

### 15.12 `GET /api/admin/learning/decks/{deckId}/analytics`

Return session-level and progress-level analytics for one deck.

This endpoint aggregates:

- all `study_sessions` with `deckId = {deckId}`
- all `user_card_progress` rows for cards that belong to the deck

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `deckId` | `string` | Yes | Target deck id |

Success response:

```json
{
  "deckId": "deck-id",
  "deckTitle": "N5 Week 1",
  "sessionCount": 842,
  "completedSessionCount": 610,
  "submissionCount": 5420,
  "averageAccuracy": 74.18,
  "trackedCards": 95,
  "masteredCards": 24,
  "dueCards": 31,
  "modeBreakdown": [
    {
      "mode": "FillInBlank",
      "sessionCount": 120,
      "completedSessionCount": 88,
      "submissionCount": 820,
      "averageAccuracy": 58.17
    },
    {
      "mode": "MultipleChoice",
      "sessionCount": 402,
      "completedSessionCount": 310,
      "submissionCount": 2710,
      "averageAccuracy": 81.25
    }
  ]
}
```

Response field notes:

| Field | Type | Notes |
| ----- | ---- | ----- |
| `sessionCount` | `int` | Sessions created for the deck |
| `completedSessionCount` | `int` | Sessions for the deck with `completedAt != null` |
| `submissionCount` | `int` | Sum of `correctCount + incorrectCount` over deck sessions |
| `averageAccuracy` | `double` | Accuracy percentage over deck sessions |
| `trackedCards` | `int` | Distinct deck cards that already have at least one `user_card_progress` row |
| `masteredCards` | `int` | Distinct deck cards with at least one mastered progress row |
| `dueCards` | `int` | Distinct deck cards that are currently due for review |
| `modeBreakdown` | `DeckLearningModeAnalyticsResponse[]` | Session metrics grouped by `StudyMode` |

Frontend notes:

- this is useful for deck analytics dashboards, not for real-time per-attempt monitoring
- `trackedCards`, `masteredCards`, and `dueCards` are card-level counts, not user-level counts
- render `modeBreakdown` as a flat table or compact chart; it is already grouped for you

### 15.13 `GET /api/admin/learning/cards/{cardId}/analytics`

Return progress and usage analytics for one card.

This endpoint is intentionally limited to metrics that can be derived from current schema. It does not include per-answer history because backend does not store detailed submission logs yet.

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `cardId` | `string` | Yes | Target card id |

Success response:

```json
{
  "cardId": "card-id",
  "cardType": "Grammar",
  "title": "〜ている",
  "summary": "is doing",
  "includedSessionCount": 220,
  "includedCompletedSessionCount": 170,
  "trackedUsers": 84,
  "masteredUsers": 19,
  "dueUsers": 27,
  "averageSrsLevel": 4.82,
  "averageConsecutiveCorrect": 2.17,
  "lastReviewedAt": "2026-04-19T03:42:11Z",
  "srsDistribution": [
    { "srsLevel": "level_1", "userCount": 12 },
    { "srsLevel": "level_12", "userCount": 19 }
  ],
  "decks": [
    { "deckId": "deck-1", "deckTitle": "N5 Week 1" }
  ]
}
```

Response field notes:

| Field | Type | Notes |
| ----- | ---- | ----- |
| `includedSessionCount` | `int` | Number of sessions whose `cardIds[]` contains this card |
| `includedCompletedSessionCount` | `int` | Number of completed sessions whose `cardIds[]` contains this card |
| `trackedUsers` | `int` | Distinct users with a progress row for this card |
| `masteredUsers` | `int` | Users whose progress for the card is mastered |
| `dueUsers` | `int` | Users whose progress for the card is currently due |
| `averageSrsLevel` | `double` | Average SRS level number across all progress rows for the card |
| `averageConsecutiveCorrect` | `double` | Average consecutive-correct streak across all progress rows for the card |
| `lastReviewedAt` | `datetime?` | Most recent `lastReviewedAt` across all progress rows |
| `srsDistribution` | `CardLearningSrsDistributionResponse[]` | User count grouped by SRS level |
| `decks` | `CardLearningDeckUsageResponse[]` | Decks that currently contain this card |

Frontend notes:

- this endpoint is useful for content QA and operational visibility
- because there is no per-submission log table yet, this endpoint does not expose wrong-answer breakdowns or sentence-level failure analytics
- if frontend needs a richer analytics page later, backend will need attempt-log APIs rather than expanding this response only

### 15.14 `GET /api/admin/learning/users/{userId}/progress`

Return summary learning progress for one user.

This endpoint is intended for:

- support workflows
- admin audit views
- quick inspection of a user’s overall SRS state

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `userId` | `string` | Yes | Target user id |

Success response:

```json
{
  "userId": "user-id",
  "username": "alice",
  "email": "alice@example.com",
  "totalTrackedCards": 420,
  "masteredCards": 90,
  "dueCards": 57,
  "averageSrsLevel": 4.73,
  "averageConsecutiveCorrect": 2.11,
  "lastReviewedAt": "2026-04-19T05:21:14Z",
  "recentSessionCount": 20,
  "srsDistribution": [
    { "srsLevel": "level_1", "cardCount": 80 },
    { "srsLevel": "level_12", "cardCount": 90 }
  ],
  "decks": [
    {
      "deckId": "deck-1",
      "deckTitle": "N5 Week 1",
      "trackedCards": 120,
      "masteredCards": 24,
      "dueCards": 13
    }
  ]
}
```

Response field notes:

| Field | Type | Notes |
| ----- | ---- | ----- |
| `userId` | `string` | Target user id |
| `username` | `string` | User display/login name |
| `email` | `string` | User email |
| `totalTrackedCards` | `int` | Distinct cards with at least one progress row for the user |
| `masteredCards` | `int` | Distinct tracked cards already mastered |
| `dueCards` | `int` | Distinct tracked cards currently due |
| `averageSrsLevel` | `double` | Average SRS level number across all progress rows |
| `averageConsecutiveCorrect` | `double` | Average consecutive-correct streak across all progress rows |
| `lastReviewedAt` | `datetime?` | Most recent review timestamp across all progress rows |
| `recentSessionCount` | `int` | Count of the latest 20 sessions returned by backend for this user |
| `srsDistribution` | `UserLearningSrsDistributionResponse[]` | Card count grouped by SRS level |
| `decks` | `UserLearningDeckProgressResponse[]` | Deck summaries for decks that currently contain tracked cards |

Frontend notes:

- deck summaries are derived from decks that currently contain the user’s tracked cards
- if the same card exists in multiple decks, it may contribute to multiple deck summaries
- this endpoint is summary-only and does not return full card-level progress rows
- this is intended for support/audit views, not for rendering a full student learning history page

### 15.15 `GET /api/admin/learning/cards/{cardId}/preview`

Preview one generated exercise for the selected card and mode without creating a study session.

This endpoint uses the same content-generation rules as the user learning flow where applicable.

Path params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `cardId` | `string` | Yes | Target card id |

Query params:

| Param | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `mode` | `StudyMode` | Yes | `FillInBlank`, `MultipleChoice`, `Flashcard` |
| `multipleChoiceQuestion` | `MultipleChoiceQuestionType?` | No | `TitleToSummary`, `SummaryToTitle` |
| `flashcardFront` | `FlashcardContentType?` | No | `Title`, `Summary` |
| `flashcardBack` | `FlashcardContentType?` | No | `Title`, `Summary` |
| `shuffleOptions` | `bool?` | No | Only affects multiple-choice preview |

Example: fill-in preview

```json
{
  "cardId": "card-id",
  "mode": "FillInBlank",
  "prompt": "Điền vào chỗ trống",
  "questionText": "毎日パンを____。",
  "secondaryText": "I eat bread every day.",
  "hint": "Dictionary form",
  "frontText": null,
  "backText": null,
  "allowsMultipleSelection": true,
  "options": [],
  "warnings": []
}
```

Example: multiple-choice preview

```json
{
  "cardId": "card-id",
  "mode": "MultipleChoice",
  "prompt": "Chọn nghĩa đúng của thẻ",
  "questionText": "食べる",
  "secondaryText": null,
  "hint": null,
  "frontText": null,
  "backText": null,
  "allowsMultipleSelection": false,
  "options": [
    { "id": "to eat", "text": "to eat", "isCorrect": true },
    { "id": "to drink", "text": "to drink", "isCorrect": false }
  ],
  "warnings": []
}
```

Example: flashcard preview

```json
{
  "cardId": "card-id",
  "mode": "Flashcard",
  "prompt": "Xem flashcard rồi đánh dấu đang học hoặc đã biết",
  "questionText": null,
  "secondaryText": null,
  "hint": null,
  "frontText": "食べる",
  "backText": "to eat",
  "allowsMultipleSelection": false,
  "options": [],
  "warnings": []
}
```

Preview behavior notes:

- fill-in:
  - uses the first attached sentence ordered by `position`
  - if that sentence has no explicit `answerList`, backend may generate fallback answers
  - if no sentence exists, backend returns a fallback fill-in preview based on the card itself
- multiple-choice:
  - correct answer is derived from the selected `multipleChoiceQuestion` direction
  - distractors come from other cards of the same `cardType`
- flashcard:
  - front/back are derived from `flashcardFront` and `flashcardBack`

Frontend notes:

- `warnings[]` is intended for admin UI hints, not blocking errors
- preview never creates `study_sessions` or `user_card_progress`

### 15.16 Frontend integration checklist

If another agent is implementing the admin UI, this is the minimum contract they need to follow:

1. Treat `GET /api/admin/learning/cards/{cardId}/config` as the source of truth for the card learning tab.
2. Use `PUT /api/admin/learning/cards/{cardId}/config` for full-form save.
3. Use row-level sentence APIs only when the UI needs inline add/edit/delete/reorder behavior.
4. Use `GET /api/admin/learning/cards/{cardId}/preview` before publish or before saving if the UX needs immediate preview.
5. Build QA pages from `GET /api/admin/learning/cards/issues` and `GET /api/admin/learning/decks/{deckId}/coverage`.
6. Build dashboard/analytics screens from:
   - `GET /api/admin/learning/overview`
   - `GET /api/admin/learning/decks/{deckId}/analytics`
   - `GET /api/admin/learning/cards/{cardId}/analytics`
   - `GET /api/admin/learning/users/{userId}/progress`
7. Treat all enums as case-sensitive strings exactly as documented in this file.

### 15.17 Frontend task breakdown

Use this breakdown when assigning work to a frontend admin agent.

#### Page 1: Card learning config tab

Primary queries:

- `GET /api/admin/learning/cards/{cardId}/config`
- optional preview query: `GET /api/admin/learning/cards/{cardId}/preview`

Primary mutations:

- `PUT /api/admin/learning/cards/{cardId}/config`
- `POST /api/admin/learning/cards/{cardId}/sentences`
- `PUT /api/admin/learning/cards/{cardId}/sentences/{sentenceId}`
- `DELETE /api/admin/learning/cards/{cardId}/sentences/{sentenceId}`
- `POST /api/admin/learning/cards/{cardId}/sentences/reorder`

Suggested UI blocks:

- readiness badges: `isFillInBlankReady`, `isMultipleChoiceReady`, `isFlashcardReady`
- issue list: `issues[]`
- sentence table with inline edit actions
- add sentence modal / picker
- drag-and-drop sentence reorder
- preview panel with mode switch

Suggested query invalidation after mutation success:

- invalidate `GET /api/admin/learning/cards/{cardId}/config`
- invalidate preview query for the same `cardId`
- optionally invalidate `GET /api/admin/learning/cards/issues`

State guidance:

- server state:
  - config payload
  - preview payload
- local UI state:
  - unsaved form edits
  - sentence row edit mode
  - preview mode selector

#### Page 2: Learning issues page

Primary query:

- `GET /api/admin/learning/cards/issues`

Suggested filters:

- `cardType`
- `mode`
- `issueType`
- `q`
- `deckId`
- pagination

Suggested UI blocks:

- filter bar
- paginated issue table
- mode availability chips from `availableModes`
- issue badges or stacked text rows
- deep links to card detail or deck detail

Suggested query key shape:

- `learning-admin-issues`, `{ page, pageSize, cardType, mode, issueType, q, deckId }`

State guidance:

- server state:
  - issue list and pagination
- local UI state:
  - filter form before apply

#### Page 3: Deck learning coverage page

Primary query:

- `GET /api/admin/learning/decks/{deckId}/coverage`

Suggested companion query:

- `GET /api/admin/learning/cards/issues?deckId={deckId}`

Suggested UI blocks:

- top summary cards
- breakdown by card type
- CTA links to filtered issue list

Suggested invalidation:

- after any card config mutation affecting a card in that deck, invalidate deck coverage and deck-scoped issue list

#### Page 4: Admin learning dashboard

Primary query:

- `GET /api/admin/learning/overview`

Suggested UI blocks:

- summary metric cards
- optional refresh action

State guidance:

- server state only
- optional coarse polling if product wants near-live dashboard numbers

#### Page 5: Deck analytics page

Primary query:

- `GET /api/admin/learning/decks/{deckId}/analytics`

Suggested UI blocks:

- summary cards for sessions, submissions, average accuracy
- mode breakdown table or chart
- tracked/mastered/due card counters

State guidance:

- server state only

#### Page 6: Card analytics drawer/page

Primary query:

- `GET /api/admin/learning/cards/{cardId}/analytics`

Suggested UI blocks:

- summary section for tracked users and due users
- SRS distribution chart
- deck usage list

State guidance:

- server state only

#### Page 7: User learning support page

Primary query:

- `GET /api/admin/learning/users/{userId}/progress`

Suggested UI blocks:

- user identity header
- summary cards for tracked/mastered/due cards
- SRS distribution chart
- deck summaries table

State guidance:

- server state only

#### Recommended cache invalidation map

- After `PUT /cards/{cardId}/config`:
  - invalidate card config
  - invalidate card preview
  - invalidate issues list
  - invalidate deck coverage if current UI knows affected `deckId`
- After `POST /cards/{cardId}/sentences`:
  - invalidate card config
  - invalidate card preview
  - invalidate issues list
- After `PUT /cards/{cardId}/sentences/{sentenceId}`:
  - invalidate card config
  - invalidate card preview
  - invalidate issues list
- After `DELETE /cards/{cardId}/sentences/{sentenceId}`:
  - invalidate card config
  - invalidate card preview
  - invalidate issues list
- After `POST /cards/{cardId}/sentences/reorder`:
  - invalidate card config
  - invalidate card preview only if preview currently depends on sentence order

#### Suggested implementation order for frontend admin

1. Card learning config tab
2. Preview panel inside card config tab
3. Learning issues page
4. Deck learning coverage page
5. Admin overview dashboard
6. Deck analytics page
7. Card analytics page
8. User learning support page

### 15.18 Next recommended admin phases

The following admin APIs are still recommended for later phases:

- attempt history / answer logs if you want wrong-answer analytics, sentence-level failure rates, or common distractor reports

---

## 16. JLPT Exams Module — Admin

> 🔑 **All endpoints in this module require `Editor` or `Admin`.**  
> This module manages JLPT exams, sections, question groups, Choukai audio generation, and JSON import/export for a full exam tree.

### Overview

| Method | Endpoint | Auth | Description |
| ------ | -------- | ---- | ----------- |
| POST | `/api/exams` | 🔑 Editor/Admin | Create a new exam |
| GET | `/api/exams` | 🔑 Editor/Admin | Search exams |
| GET | `/api/exams/import-template` | 🔑 Editor/Admin | Download the JSON import template |
| GET | `/api/exams/import-guide` | 🔑 Editor/Admin | Get import rules and allowed values |
| GET | `/api/exams/{id}` | 🔑 Editor/Admin | Get full exam detail |
| GET | `/api/exams/{id}/export` | 🔑 Editor/Admin | Export one exam as JSON |
| PUT | `/api/exams/{id}` | 🔑 Editor/Admin | Update exam metadata |
| PATCH | `/api/exams/{id}/publish` | 🔑 Editor/Admin | Publish an exam |
| DELETE | `/api/exams/{id}` | 🔑 Editor/Admin | Delete a draft exam |
| POST | `/api/exams/preview-import` | 🔑 Editor/Admin | Validate an import payload without writing data |
| POST | `/api/exams/commit-import` | 🔑 Editor/Admin | Create a new draft exam from a validated payload |
| POST | `/api/exams/{examId}/sections` | 🔑 Editor/Admin | Create a section |
| PUT | `/api/exams/{examId}/sections/{sectionId}` | 🔑 Editor/Admin | Update a section |
| DELETE | `/api/exams/{examId}/sections/{sectionId}` | 🔑 Editor/Admin | Delete a section |
| POST | `/api/exams/sections/{sectionId}/groups` | 🔑 Editor/Admin | Create a question group |
| PUT | `/api/exams/sections/{sectionId}/groups/{groupId}` | 🔑 Editor/Admin | Update a question group |
| DELETE | `/api/exams/sections/{sectionId}/groups/{groupId}` | 🔑 Editor/Admin | Delete a question group |
| POST | `/api/exams/groups/{groupId}/generate-audio` | 🔑 Editor/Admin | Generate TTS audio from `audioScript` |

### Enum Values

| Enum | Values |
| ---- | ------ |
| `JlptLevel` | `N5`, `N4`, `N3`, `N2`, `N1` |
| `SectionType` | `Moji`, `Bunpou`, `Dokkai`, `Choukai` |
| `ChoukaiMondaiType` | `Mondai1`, `Mondai2`, `Mondai3`, `Mondai4`, `Mondai5` |
| `OptionLabel` | `A`, `B`, `C`, `D` |
| `OptionType` | `Text`, `Image`, `TextAndImage` |
| `PublishStatus` | `Draft`, `Published`, `Archived` |

### Shared Response Shapes

**Exam list item**

```json
{
  "id": "exam-id",
  "title": "JLPT N5 Mock Test 01",
  "level": "N5",
  "totalDurationMinutes": 120,
  "status": "Draft",
  "sectionsCount": 4,
  "createdBy": "user-id",
  "creatorName": "Nguyen Van A",
  "createdAt": "datetime",
  "updatedAt": "datetime | null"
}
```

**Exam detail**

```json
{
  "id": "exam-id",
  "title": "JLPT N5 Mock Test 01",
  "level": "N5",
  "totalDurationMinutes": 120,
  "status": "Draft",
  "createdBy": "user-id",
  "creatorName": "Nguyen Van A",
  "sections": [
    {
      "id": "section-id",
      "sectionType": "Moji",
      "orderIndex": 0,
      "durationMinutes": 25,
      "maxScore": 60,
      "passScore": 19,
      "questionGroupsCount": 1,
      "questionsCount": 1,
      "questionGroups": [
        {
          "id": "group-id",
          "passageText": null,
          "audioUrl": null,
          "audioScript": null,
          "instruction": "Choose the correct answer.",
          "orderIndex": 0,
          "mondaiType": null,
          "questions": [
            {
              "id": "question-id",
              "groupId": "group-id",
              "questionText": "What is the correct reading of 食べる?",
              "imageUrl": null,
              "imageCaption": null,
              "explanation": null,
              "score": 1,
              "orderIndex": 0,
              "options": [
                {
                  "id": "option-id",
                  "label": "A",
                  "text": "たべる",
                  "imageUrl": null,
                  "optionType": "Text",
                  "isCorrect": true
                }
              ],
              "createdAt": "datetime",
              "updatedAt": null
            }
          ],
          "createdAt": "datetime",
          "updatedAt": null
        }
      ],
      "createdAt": "datetime",
      "updatedAt": null
    }
  ],
  "createdAt": "datetime",
  "updatedAt": null
}
```

### GET `/api/exams` 🔑

Search exams for the admin UI.

**Query params**

| Param | Type | Required | Enum | Notes |
| ----- | ---- | -------- | ---- | ----- |
| `keyword` | `string` | No | — | Title keyword |
| `level` | `string` | No | `JlptLevel` | Level filter |
| `status` | `string` | No | `PublishStatus` | Status filter |
| `page` | `int` | No | — | Default `1` |
| `pageSize` | `int` | No | — | Default `20` |

### POST `/api/exams` 🔑

Create a new exam shell.

**Request body**

```json
{
  "title": "JLPT N5 Mock Test 01",
  "level": "N5",
  "totalDurationMinutes": 120
}
```

**Rules**

- `title` is required and must be at most `500` characters.
- `level` is required and must be a valid `JlptLevel`.
- `totalDurationMinutes` must be `> 0` and `<= 300`.

**Response data:** `ExamDetailResponse`

### GET `/api/exams/{id}` 🔑

Return the full exam tree for one exam.

**Response data:** `ExamDetailResponse`

### PUT `/api/exams/{id}` 🔑

Update exam metadata.

**Request body:** same shape and rules as `POST /api/exams`.

**Response data:** `ExamDetailResponse`

### PATCH `/api/exams/{id}/publish` 🔑

Publish an exam so it becomes available to the user-facing module.

**Response data**

```json
"Published"
```

**Publish rules**

- The exam must have at least one section.
- Every section must have at least one question group and at least one question.
- A published exam cannot be published again.

**Error codes**

| Code | Trigger |
| ---- | ------- |
| `Exam_NotFound_404` | Exam does not exist |
| `Exam_AlreadyPublished_400` | Exam is already published |
| `Exam_NoSections_400` | Exam has no sections |
| `Exam_NoQuestions_400` | At least one section has no usable questions |

### DELETE `/api/exams/{id}` 🔑

Delete a draft exam.

**Response data**

```json
"Deleted"
```

**Error codes**

| Code | Trigger |
| ---- | ------- |
| `Exam_NotFound_404` | Exam does not exist |
| `Exam_CannotDeletePublished_400` | Published exams cannot be deleted here |

### GET `/api/exams/import-template` 🔑

Download the starter JSON file for exam import.

**Response content type:** `application/json`

**Response body**

```json
{
  "title": "JLPT N5 Mock Test 01",
  "level": "N5",
  "totalDurationMinutes": 120,
  "sections": [
    {
      "sectionType": "Moji",
      "orderIndex": 0,
      "durationMinutes": 25,
      "maxScore": 60,
      "passScore": 19,
      "questionGroups": [
        {
          "passageText": null,
          "audioUrl": null,
          "audioScript": null,
          "instruction": "Choose the correct answer.",
          "orderIndex": 0,
          "mondaiType": null,
          "questions": [
            {
              "questionText": "What is the correct reading of 食べる?",
              "imageUrl": null,
              "imageCaption": null,
              "explanation": null,
              "score": 1,
              "orderIndex": 0,
              "options": [
                {
                  "label": "A",
                  "text": "たべる",
                  "imageUrl": null,
                  "optionType": "Text",
                  "isCorrect": true
                },
                {
                  "label": "B",
                  "text": "のめる",
                  "imageUrl": null,
                  "optionType": "Text",
                  "isCorrect": false
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```

**Template notes**

- The downloaded template is intentionally minimal and does not include a `guide` object.
- Use `GET /api/exams/import-guide` or this API document as the source of truth for allowed values and validation rules.
- Import still accepts an optional `guide` field in the payload, but the backend ignores it.

### GET `/api/exams/import-guide` 🔑

Return the import guide separately from the template file.

**Response body**

```json
{
  "jsonNamingConvention": "camelCase",
  "overview": [
    "Import creates a brand-new exam only. Existing exams are never updated.",
    "Imported exams always start with status Draft.",
    "The payload should contain the full tree: exam -> sections -> questionGroups -> questions -> options.",
    "audioUrl, imageUrl, and audioScript are kept as reference values only. No media copy or TTS generation runs during import."
  ],
  "allowedValues": {
    "level": ["N5", "N4", "N3", "N2", "N1"],
    "sectionType": ["Moji", "Bunpou", "Dokkai", "Choukai"],
    "mondaiType": ["Mondai1", "Mondai2", "Mondai3", "Mondai4", "Mondai5"],
    "optionLabel": ["A", "B", "C", "D"],
    "optionType": ["Text", "Image", "TextAndImage"]
  },
  "rulesByNode": {
    "exam": [
      "title is required and must be at most 500 characters.",
      "level is required and must be a valid JlptLevel value.",
      "totalDurationMinutes is required and must be > 0 and <= 300.",
      "sections is required and must contain at least one section."
    ],
    "section": [
      "sectionType is required and must be a valid SectionType value.",
      "orderIndex must be >= 0 and unique among sibling sections.",
      "durationMinutes must be > 0.",
      "maxScore must be > 0.",
      "passScore must be >= 0 and <= maxScore.",
      "questionGroups is required and must contain at least one group."
    ],
    "questionGroup": [
      "instruction is required and must be at most 2000 characters.",
      "orderIndex must be >= 0 and unique among sibling groups.",
      "passageText is optional and must be at most 10000 characters.",
      "audioUrl is optional and must be at most 512 characters.",
      "audioScript is optional and must be at most 10000 characters.",
      "mondaiType is optional and mainly used for Choukai groups.",
      "questions is required and must contain at least one question."
    ],
    "question": [
      "questionText is required and must be at most 5000 characters.",
      "orderIndex must be >= 0 and unique among sibling questions.",
      "score is required and must be > 0.",
      "imageUrl is optional and must be at most 512 characters.",
      "imageCaption is optional and must be at most 1000 characters.",
      "explanation is optional and must be at most 5000 characters.",
      "options is required and must contain from 2 to 4 items."
    ],
    "option": [
      "label is required, must be one of A/B/C/D, and must be unique within the question.",
      "optionType is required and must be one of Text/Image/TextAndImage.",
      "Exactly one option in each question must have isCorrect = true.",
      "text is optional and must be at most 2000 characters.",
      "imageUrl is optional and must be at most 512 characters."
    ]
  }
}
```

### GET `/api/exams/{id}/export` 🔑

Export one existing exam as a JSON package that can be re-imported later.

**Response content type:** `application/json`

**Response body**

- Same tree shape as `ImportExamRequest`.
- `guide` is omitted in export output.
- `audioUrl`, `imageUrl`, and `audioScript` are preserved as-is.

### POST `/api/exams/preview-import` 🔑

Validate one import payload without writing any records.

**Request body:** same shape as `ImportExamRequest`

**Request field table**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `title` | `string` | Yes | Max `500` |
| `level` | `string` | Yes | `JlptLevel` |
| `totalDurationMinutes` | `int` | Yes | `> 0`, `<= 300` |
| `sections` | `array` | Yes | At least one section |
| `sections[].sectionType` | `string` | Yes | `SectionType` |
| `sections[].orderIndex` | `int` | Yes | `>= 0`, unique within `sections` |
| `sections[].durationMinutes` | `int` | Yes | `> 0` |
| `sections[].maxScore` | `int` | Yes | `> 0` |
| `sections[].passScore` | `int` | Yes | `>= 0`, `<= maxScore` |
| `sections[].questionGroups` | `array` | Yes | At least one group per section |
| `sections[].questionGroups[].passageText` | `string?` | No | Max `10000` |
| `sections[].questionGroups[].audioUrl` | `string?` | No | Max `512` |
| `sections[].questionGroups[].audioScript` | `string?` | No | Max `10000` |
| `sections[].questionGroups[].instruction` | `string` | Yes | Max `2000` |
| `sections[].questionGroups[].orderIndex` | `int` | Yes | `>= 0`, unique within group list |
| `sections[].questionGroups[].mondaiType` | `string?` | No | `ChoukaiMondaiType` |
| `sections[].questionGroups[].questions` | `array` | Yes | At least one question per group |
| `sections[].questionGroups[].questions[].questionText` | `string` | Yes | Max `5000` |
| `sections[].questionGroups[].questions[].imageUrl` | `string?` | No | Max `512` |
| `sections[].questionGroups[].questions[].imageCaption` | `string?` | No | Max `1000` |
| `sections[].questionGroups[].questions[].explanation` | `string?` | No | Max `5000` |
| `sections[].questionGroups[].questions[].score` | `int` | Yes | `> 0` |
| `sections[].questionGroups[].questions[].orderIndex` | `int` | Yes | `>= 0`, unique within question list |
| `sections[].questionGroups[].questions[].options` | `array` | Yes | `2..4` items, exactly one correct |
| `sections[].questionGroups[].questions[].options[].label` | `string` | Yes | `OptionLabel`, unique within question |
| `sections[].questionGroups[].questions[].options[].text` | `string?` | No | Max `2000` |
| `sections[].questionGroups[].questions[].options[].imageUrl` | `string?` | No | Max `512` |
| `sections[].questionGroups[].questions[].options[].optionType` | `string` | Yes | `OptionType` |
| `sections[].questionGroups[].questions[].options[].isCorrect` | `bool` | Yes | Exactly one option must be `true` |

**Response body**

```json
{
  "isValid": false,
  "errorCount": 2,
  "warningCount": 0,
  "item": {
    "title": "JLPT N5 Mock Test 01",
    "level": "N5",
    "sectionsCount": 1,
    "questionGroupsCount": 1,
    "questionsCount": 1,
    "optionsCount": 2,
    "isValid": false,
    "errors": [
      "Exam_ImportFieldRequired_400:sections[0].questionGroups[0].instruction",
      "Exam_ImportCorrectOptionInvalid_400:sections[0].questionGroups[0].questions[0].options"
    ],
    "warnings": []
  }
}
```

### POST `/api/exams/commit-import` 🔑

Create one new exam from an import payload.

**Behavior**

- The payload is previewed again on the server before writing.
- If preview has any error, the commit is blocked and no records are created.
- Import always creates a **new** exam.
- The created exam always has `status = Draft`.
- Existing records are never updated or overwritten in v1.
- `audioUrl`, `imageUrl`, and `audioScript` are copied as reference values only.
- No media upload, no file copy, and no TTS generation runs during import.
- Extra legacy fields such as `id`, `createdAt`, `updatedAt`, `createdBy`, and `creatorName` are ignored if sent by the client.

**Response body when blocked by validation**

```json
{
  "isSuccess": false,
  "hasValidationErrors": true,
  "action": "skipped",
  "title": "JLPT N5 Mock Test 01",
  "examId": null,
  "sectionsCount": 1,
  "questionGroupsCount": 1,
  "questionsCount": 1,
  "optionsCount": 2,
  "errors": [
    "Exam_ImportFieldRequired_400:sections[0].questionGroups[0].instruction"
  ]
}
```

**Response body when successful**

```json
{
  "isSuccess": true,
  "hasValidationErrors": false,
  "action": "created",
  "title": "JLPT N5 Mock Test 01",
  "examId": "new-exam-id",
  "sectionsCount": 4,
  "questionGroupsCount": 12,
  "questionsCount": 40,
  "optionsCount": 160,
  "errors": []
}
```

### Import Error Codes

| Code | Trigger |
| ---- | ------- |
| `Exam_ImportInvalidPayload_400` | The payload is null or cannot be processed |
| `Exam_ImportBatchHasErrors_400` | Commit was blocked because preview found validation errors |
| `Exam_ImportFieldRequired_400` | A required field is missing |
| `Exam_ImportFieldTooLong_400` | A string field exceeds the allowed max length |
| `Exam_ImportFieldInvalid_400` | An enum, score, duration, or order value is invalid |
| `Exam_ImportSectionsRequired_400` | `sections` is empty |
| `Exam_ImportGroupsRequired_400` | A section has no `questionGroups` |
| `Exam_ImportQuestionsRequired_400` | A group has no `questions` |
| `Exam_ImportOptionsInvalidCount_400` | A question has fewer than `2` or more than `4` options |
| `Exam_ImportCorrectOptionInvalid_400` | A question does not have exactly one correct option |
| `Exam_ImportDuplicateOptionLabel_400` | Duplicate option labels exist inside one question |
| `Exam_ImportDuplicateOrderIndex_400` | Duplicate `orderIndex` values exist within sibling nodes |
| `Exam_ImportPassScoreInvalid_400` | `passScore` is negative or greater than `maxScore` |

### POST `/api/exams/{examId}/sections` 🔑

Create a section for an exam.

**Request body**

```json
{
  "sectionType": "Moji",
  "orderIndex": 0,
  "durationMinutes": 25,
  "maxScore": 60,
  "passScore": 19
}
```

**Rules**

- `sectionType` is required and must be a valid `SectionType`.
- `orderIndex` must be `>= 0`.
- `durationMinutes` must be `> 0`.
- `maxScore` must be `> 0`.
- `passScore` must be `>= 0` and `<= maxScore`.

**Response data:** `ExamSectionResponse`

### PUT `/api/exams/{examId}/sections/{sectionId}` 🔑

Update a section.

**Request body:** same shape and rules as `POST /api/exams/{examId}/sections`.

**Response data:** `ExamSectionResponse`

### DELETE `/api/exams/{examId}/sections/{sectionId}` 🔑

Delete a section.

**Response data**

```json
"Deleted"
```

### POST `/api/exams/sections/{sectionId}/groups` 🔑

Create a question group inside a section.

**Request body**

```json
{
  "passageText": "Reading passage...",
  "audioUrl": null,
  "audioScript": null,
  "instruction": "Read the passage and answer the questions.",
  "orderIndex": 0,
  "mondaiType": null
}
```

**Rules**

- `instruction` is required and must be at most `2000` characters.
- `orderIndex` must be `>= 0`.
- `audioUrl` must be at most `512` characters when provided.
- `passageText` must be at most `10000` characters when provided.
- `mondaiType` is optional and uses `ChoukaiMondaiType`.

**Response data:** `QuestionGroupResponse`

### PUT `/api/exams/sections/{sectionId}/groups/{groupId}` 🔑

Update a question group.

**Request body:** same shape and rules as `POST /api/exams/sections/{sectionId}/groups`.

**Response data:** `QuestionGroupResponse`

### DELETE `/api/exams/sections/{sectionId}/groups/{groupId}` 🔑

Delete a question group.

**Response data**

```json
"Deleted"
```

### POST `/api/exams/groups/{groupId}/generate-audio` 🔑

Generate TTS audio for one Choukai group from its `audioScript`.

**Backend flow**

1. Read `audioScript` from the group.
2. Call Azure Cognitive Services Text-to-Speech.
3. Upload the generated MP3 to Cloudinary.
4. Update `audioUrl` on the group.

**Response data:** `QuestionGroupResponse`

**Error codes**

| Code | Trigger |
| ---- | ------- |
| `Exam_GroupNotFound_404` | Question group does not exist |
| `AiQuestion_NoAudioScript_400` | The group has no `audioScript` |
| `Exam_CannotModifyPublished_400` | The parent exam is already published |

---

## 17. JLPT Questions Module — Admin

> 🔑 **Tất cả endpoint trong module này yêu cầu quyền `Editor` hoặc `Admin`.**  
> Dùng để quản lý câu hỏi và đáp án trong question bank / exam groups.

### Tổng quan

| Method | Endpoint | Auth | Mô tả |
| ------ | -------- | ---- | ----- |
| POST | `/api/questions` | 🔑 Editor/Admin | Tạo một câu hỏi |
| GET | `/api/questions` | 🔑 Editor/Admin | Tìm kiếm câu hỏi |
| GET | `/api/questions/{id}` | 🔑 Editor/Admin | Lấy chi tiết câu hỏi |
| PUT | `/api/questions/{id}` | 🔑 Editor/Admin | Cập nhật câu hỏi |
| DELETE | `/api/questions/{id}` | 🔑 Editor/Admin | Xóa câu hỏi |
| POST | `/api/questions/groups/{groupId}/bulk` | 🔑 Editor/Admin | Tạo nhiều câu hỏi cùng lúc |
| PUT | `/api/questions/groups/{groupId}/reorder` | 🔑 Editor/Admin | Cập nhật thứ tự câu hỏi |

---

### GET `/api/questions` 🔑

Tìm kiếm câu hỏi theo keyword, level và loại section.

**Query params:**

| Param | Type | Bắt buộc | Enum | Mô tả |
| ----- | ---- | -------- | ---- | ----- |
| `keyword` | `string` | ❌ | — | Tìm theo nội dung câu hỏi |
| `level` | `string` | ❌ | `JlptLevel` | Lọc theo level đề |
| `sectionType` | `string` | ❌ | `SectionType` | Lọc theo section |
| `page` | `int` | ❌ | — | Mặc định `1` |
| `pageSize` | `int` | ❌ | — | Mặc định `20` |

**Response data item:**

```json
{
  "id": "question-id",
  "groupId": "group-id",
  "questionText": "Từ nào đọc là たべる?",
  "imageUrl": null,
  "imageCaption": null,
  "explanation": "食べる là động từ nhóm 2.",
  "score": 1,
  "orderIndex": 0,
  "options": [
    {
      "id": "option-id",
      "label": "A",
      "text": "食べる",
      "imageUrl": null,
      "optionType": "Text",
      "isCorrect": true
    }
  ],
  "createdAt": "datetime",
  "updatedAt": null
}
```

---

### POST `/api/questions` 🔑

Tạo một câu hỏi cho question group.

**Request body:**

```json
{
  "groupId": "group-id",
  "questionText": "Từ nào đọc là たべる?",
  "imageUrl": null,
  "imageCaption": null,
  "explanation": "食べる là động từ nhóm 2.",
  "score": 1,
  "orderIndex": 0,
  "options": [
    {
      "label": "A",
      "text": "食べる",
      "imageUrl": null,
      "optionType": "Text",
      "isCorrect": true
    },
    {
      "label": "B",
      "text": "飲む",
      "imageUrl": null,
      "optionType": "Text",
      "isCorrect": false
    }
  ]
}
```

**Rules quan trọng:**

- `questionText`: bắt buộc, tối đa `5000` ký tự
- `score`: `> 0`
- `orderIndex`: `>= 0`
- `options`: bắt buộc có từ `2` đến `4` item
- Phải có **chính xác 1 option đúng**
- `label`: enum `OptionLabel` (`A`, `B`, `C`, `D`)
- `optionType`: enum `OptionType`

**Response data:** `QuestionResponse`

---

### GET `/api/questions/{id}` 🔑

Lấy chi tiết một câu hỏi.

**Response data:** `QuestionResponse`

---

### PUT `/api/questions/{id}` 🔑

Cập nhật câu hỏi.

**Request body:**

```json
{
  "questionText": "Từ nào đọc là たべる?",
  "imageUrl": null,
  "imageCaption": null,
  "explanation": "Giải thích...",
  "score": 1,
  "orderIndex": 0,
  "options": [
    {
      "id": "existing-option-id",
      "label": "A",
      "text": "食べる",
      "imageUrl": null,
      "optionType": "Text",
      "isCorrect": true
    }
  ]
}
```

**Lưu ý:**

- Với update, `options[].id` là optional
- Danh sách option gửi lên được hiểu là trạng thái cuối cùng của câu hỏi

**Response data:** `QuestionResponse`

---

### DELETE `/api/questions/{id}` 🔑

Xóa câu hỏi.

**Response data:**

```json
"Deleted"
```

---

### POST `/api/questions/groups/{groupId}/bulk` 🔑

Tạo nhiều câu hỏi cùng lúc cho một group.

**Request body:**

```json
{
  "questions": [
    {
      "groupId": "group-id",
      "questionText": "Câu 1...",
      "score": 1,
      "orderIndex": 0,
      "options": [
        { "label": "A", "text": "A", "optionType": "Text", "isCorrect": true },
        { "label": "B", "text": "B", "optionType": "Text", "isCorrect": false }
      ]
    }
  ]
}
```

**Lưu ý:**

- Backend nhận `groupId` trên route
- Để an toàn frontend nên set `questions[].groupId` trùng với route param

**Response data:** `QuestionResponse[]`

---

### PUT `/api/questions/groups/{groupId}/reorder` 🔑

Cập nhật lại thứ tự câu hỏi trong group.

**Request body:**

```json
{
  "items": [
    {
      "id": "question-id-1",
      "orderIndex": 0
    },
    {
      "id": "question-id-2",
      "orderIndex": 1
    }
  ]
}
```

**Response data:**

```json
"Reordered"
```

---

## 18. JLPT AI Questions Module — Admin

> 🔑 **Tất cả endpoint trong module này yêu cầu quyền `Editor` hoặc `Admin`.**  
> Dùng để sinh câu hỏi bằng AI, biên tập, duyệt hoặc từ chối trước khi đưa vào question bank.

### Tổng quan

| Method | Endpoint | Auth | Mô tả |
| ------ | -------- | ---- | ----- |
| POST | `/api/ai/questions/generate` | 🔑 Editor/Admin | Sinh câu hỏi bằng AI |
| GET | `/api/ai/questions` | 🔑 Editor/Admin | Tìm kiếm câu hỏi AI đã sinh |
| GET | `/api/ai/questions/{id}` | 🔑 Editor/Admin | Lấy chi tiết câu hỏi AI |
| PUT | `/api/ai/questions/{id}` | 🔑 Editor/Admin | Chỉnh sửa JSON câu hỏi AI |
| POST | `/api/ai/questions/{id}/approve` | 🔑 Editor/Admin | Duyệt câu hỏi AI |
| POST | `/api/ai/questions/{id}/reject` | 🔑 Editor/Admin | Từ chối câu hỏi AI |

---

### POST `/api/ai/questions/generate` 🔑

Sinh danh sách câu hỏi JLPT bằng Anthropic.

**Request body:**

```json
{
  "level": "N5",
  "sectionType": "Moji",
  "topic": "Từ vựng gia đình",
  "count": 5,
  "questionGroupId": null
}
```

**Rules quan trọng:**

- `level`: bắt buộc, enum `JlptLevel`
- `sectionType`: bắt buộc, enum `SectionType`
- `topic`: bắt buộc, tối đa `500` ký tự
- `count`: từ `1` đến `20`
- `questionGroupId`: hiện có trong DTO nhưng backend hiện chưa dùng trong flow generate

**Response data item:**

```json
{
  "id": "ai-question-id",
  "level": "N5",
  "sectionType": "Moji",
  "topic": "Từ vựng gia đình",
  "generatedData": "{...json string...}",
  "status": "Pending",
  "reviewedBy": null,
  "reviewerName": null,
  "reviewedAt": null,
  "questionId": null,
  "createdBy": "user-id",
  "creatorName": "Nguyen Van A",
  "createdAt": "datetime",
  "updatedAt": null
}
```

**Ghi chú về `generatedData`:**

- Đây là **JSON string**, frontend cần `JSON.parse()` nếu muốn render editor trực tiếp
- Shape cơ bản:

```json
{
  "passage": "string | null",
  "script": "string | null",
  "questions": [
    {
      "questionText": "string",
      "explanation": "string | null",
      "options": [
        {
          "label": "A",
          "text": "string",
          "isCorrect": true
        }
      ]
    }
  ]
}
```

---

### GET `/api/ai/questions` 🔑

Tìm kiếm danh sách câu hỏi AI đã sinh.

**Query params:**

| Param | Type | Bắt buộc | Enum | Mô tả |
| ----- | ---- | -------- | ---- | ----- |
| `level` | `string` | ❌ | `JlptLevel` | Lọc theo level |
| `sectionType` | `string` | ❌ | `SectionType` | Lọc theo section |
| `status` | `string` | ❌ | `AiQuestionStatus` | Lọc theo trạng thái review |
| `page` | `int` | ❌ | — | Mặc định `1` |
| `pageSize` | `int` | ❌ | — | Mặc định `20` |

**Response data:** `AiGeneratedQuestionResponse[]`

---

### GET `/api/ai/questions/{id}` 🔑

Lấy chi tiết một bản ghi câu hỏi AI.

**Response data:** `AiGeneratedQuestionResponse`

---

### PUT `/api/ai/questions/{id}` 🔑

Chỉnh sửa payload câu hỏi AI trước khi duyệt.

**Request body:**

```json
{
  "generatedData": "{\"passage\":null,\"script\":null,\"questions\":[...]}"
}
```

**Lưu ý:**

- `generatedData` là **string**, không phải object JSON raw
- Sau khi edit, backend chuyển `status` sang `Edited`

**Response data:** `AiGeneratedQuestionResponse`

---

### POST `/api/ai/questions/{id}/approve` 🔑

Duyệt câu hỏi AI và chuyển thành `Question` thực trong database.

**Response data:** `AiGeneratedQuestionResponse`

**Sau khi approve:**

- `status` → `Approved`
- `reviewedBy`, `reviewerName`, `reviewedAt` có giá trị
- `questionId` có thể được gán nếu backend tạo thành công record `Question`

**Error codes:**

| Code | Khi nào |
| ---- | ------- |
| `AiQuestion_NotFound_404` | Không tìm thấy bản ghi AI |
| `AiQuestion_AlreadyReviewed_400` | Bản ghi đã approve hoặc reject trước đó |

---

### POST `/api/ai/questions/{id}/reject` 🔑

Từ chối câu hỏi AI.

**Response data:** `AiGeneratedQuestionResponse`

---

## 19. JLPT Exam Sessions Module — User

> `learning-app` dùng module này để hiển thị danh sách đề public, xem metadata đề, bắt đầu/resume bài thi, autosave, nộp bài và xem kết quả.

### 19.1 Public Exam Discovery

| Method | Endpoint | Auth | Mô tả |
| ------ | -------- | ---- | ----- |
| GET | `/api/jlpt-exams` | 🌐 Public | Lấy danh sách đề JLPT đã publish cho user |
| GET | `/api/jlpt-exams/{id}` | 🌐 Public | Lấy metadata chi tiết của một đề đã publish trước khi bắt đầu làm bài |

---

### GET `/api/jlpt-exams`

Lấy danh sách đề JLPT đã `Published` để hiển thị cho user.

**Query params:**

| Param | Type | Bắt buộc | Enum | Mô tả |
| ----- | ---- | -------- | ---- | ----- |
| `keyword` | `string` | ❌ | — | Tìm theo tiêu đề đề thi |
| `level` | `string` | ❌ | `JlptLevel` | Lọc theo level |
| `page` | `int` | ❌ | — | Mặc định `1` |
| `pageSize` | `int` | ❌ | — | Mặc định `20` |

**Response data item:**

```json
{
  "id": "exam-id",
  "title": "JLPT N5 Mock Test 01",
  "level": "N5",
  "totalDurationMinutes": 120,
  "sectionsCount": 4,
  "questionsCount": 40,
  "createdAt": "datetime",
  "updatedAt": "datetime | null"
}
```

---

### GET `/api/jlpt-exams/{id}`

Lấy metadata chi tiết của một đề JLPT đã `Published` trước khi user bắt đầu làm bài.

**Response data:**

```json
{
  "id": "exam-id",
  "title": "JLPT N5 Mock Test 01",
  "level": "N5",
  "totalDurationMinutes": 120,
  "sectionsCount": 4,
  "questionsCount": 40,
  "sections": [
    {
      "sectionId": "section-id",
      "sectionType": "Moji",
      "orderIndex": 0,
      "durationMinutes": 25,
      "questionGroupsCount": 2,
      "questionsCount": 10
    }
  ],
  "createdAt": "datetime",
  "updatedAt": "datetime | null"
}
```

> ℹ Endpoint này **không trả câu hỏi/options**; frontend chỉ dùng để render màn pre-start.

---

### 19.2 Exam Session Runtime

> 🔒 **Tất cả endpoint trong phần runtime này yêu cầu đăng nhập.**

### Tổng quan

| Method | Endpoint | Auth | Mô tả |
| ------ | -------- | ---- | ----- |
| POST | `/api/exam-sessions` | 🔒 Auth | Bắt đầu làm bài hoặc resume lại session đang `InProgress` của cùng exam |
| GET | `/api/exam-sessions/active` | 🔒 Auth | Kiểm tra user hiện có session `InProgress` cho exam hay không |
| GET | `/api/exam-sessions/{id}` | 🔒 Auth | Resume bài đang làm theo `sessionId` |
| POST | `/api/exam-sessions/{id}/answers` | 🔒 Auth | Auto-save một câu trả lời |
| POST | `/api/exam-sessions/{id}/submit` | 🔒 Auth | Nộp bài và chấm điểm |
| GET | `/api/exam-sessions/{id}/result` | 🔒 Auth | Xem kết quả chi tiết |
| GET | `/api/exam-sessions` | 🔒 Auth | Xem lịch sử làm bài |

---

### GET `/api/exam-sessions/active` 🔒

Kiểm tra xem user hiện tại có session `InProgress` cho một đề cụ thể hay không.

**Query params:**

| Param | Type | Bắt buộc | Mô tả |
| ----- | ---- | -------- | ----- |
| `examId` | `string` | ⚠ | ID đề thi |

**Response data:**

```json
{
  "hasActiveSession": true,
  "sessionId": "session-id"
}
```

> ℹ Nếu không có session đang làm, backend trả `hasActiveSession = false` và `sessionId = null`.

---

### POST `/api/exam-sessions` 🔒

Bắt đầu một phiên làm bài mới.

**Request body:**

```json
{
  "examId": "exam-id"
}
```

**Quy tắc session:**

- Mỗi user chỉ có **1 session `InProgress`** cho mỗi exam.
- Nếu user đã có session đang làm của exam này, backend **không tạo mới** mà trả lại chính session đó để frontend resume.

**Response data:**

```json
{
  "sessionId": "session-id",
  "examId": "exam-id",
  "examTitle": "JLPT N5 Mock Test 01",
  "level": "N5",
  "status": "InProgress",
  "startedAt": "datetime",
  "submittedAt": null,
  "expiresAt": "datetime",
  "serverNow": "datetime",
  "sections": [
    {
      "sectionId": "section-id",
      "sectionType": "Moji",
      "orderIndex": 0,
      "durationMinutes": 25,
      "questionGroups": [
        {
          "groupId": "group-id",
          "passageText": null,
          "audioUrl": null,
          "instruction": "Chọn đáp án đúng nhất.",
          "orderIndex": 0,
          "mondaiType": null,
          "questions": [
            {
              "questionId": "question-id",
              "questionText": "Câu hỏi...",
              "imageUrl": null,
              "imageCaption": null,
              "orderIndex": 0,
              "options": [
                {
                  "optionId": "option-id",
                  "label": "A",
                  "text": "Đáp án A",
                  "imageUrl": null,
                  "optionType": "Text"
                }
              ],
              "selectedOptionId": null
            }
          ]
        }
      ]
    }
  ]
}
```

**Lưu ý rất quan trọng cho frontend user:**

- Response **không chứa đáp án đúng** khi đang làm bài
- `selectedOptionId` dùng để hydrate lại state khi resume
- `expiresAt` là mốc dùng cho countdown timer phía frontend
- `serverNow` giúp frontend hạn chế lệch giờ máy client khi tính countdown

**Error codes:**

| Code | Khi nào |
| ---- | ------- |
| `Exam_NotFound_404` | Đề không tồn tại |
| `ExamSession_ExamNotPublished_400` | Đề chưa publish |

---

### GET `/api/exam-sessions/{id}` 🔒

Lấy lại trạng thái bài làm để resume sau khi reload / mất kết nối.

**Response data:** cùng shape với `POST /api/exam-sessions`.

**Error codes:**

| Code | Khi nào |
| ---- | ------- |
| `ExamSession_NotFound_404` | Session không tồn tại |
| `ExamSession_Forbidden_403` | Session không thuộc về user hiện tại |

---

### POST `/api/exam-sessions/{id}/answers` 🔒

Auto-save một câu trả lời.

**Request body:**

```json
{
  "questionId": "question-id",
  "selectedOptionId": "option-id"
}
```

**Lưu ý:**

- `questionId` là bắt buộc
- `selectedOptionId` có thể `null` nếu frontend muốn clear đáp án đã chọn
- `selectedOptionId` nếu có thì phải thuộc đúng `questionId` tương ứng
- Nên gọi endpoint này theo từng thao tác chọn đáp án

**Response data:**

```json
{
  "questionId": "question-id",
  "selectedOptionId": "option-id",
  "savedAt": "datetime"
}
```

**Error codes:**

| Code | Khi nào |
| ---- | ------- |
| `ExamSession_NotFound_404` | Session không tồn tại |
| `ExamSession_AlreadySubmitted_400` | Session đã nộp bài hoặc hết giờ |
| `ExamSession_Expired_400` | Session đã hết hạn |
| `ExamSession_QuestionNotInExam_400` | `questionId` hoặc `selectedOptionId` không thuộc đề này |
| `ExamSession_Forbidden_403` | Không phải session của user hiện tại |

---

### POST `/api/exam-sessions/{id}/submit` 🔒

Nộp bài và chấm điểm toàn bộ.

**Response data:**

```json
{
  "sessionId": "session-id",
  "totalScore": 92,
  "correctCount": 32,
  "wrongCount": 5,
  "unansweredCount": 3,
  "isPassed": true,
  "sectionScores": [
    {
      "sectionId": "section-id",
      "sectionType": "Moji",
      "score": 28,
      "maxScore": 35,
      "passScore": 19,
      "isPassed": true
    }
  ]
}
```

**Quy tắc pass/fail:**

- `isPassed = true` khi **tổng điểm đạt** và **không trượt section nào**
- Nếu trượt 1 section thì toàn bài vẫn fail

---

### GET `/api/exam-sessions/{id}/result` 🔒

Xem kết quả chi tiết sau khi đã submit.

**Response data:**

```json
{
  "sessionId": "session-id",
  "examId": "exam-id",
  "examTitle": "JLPT N5 Mock Test 01",
  "level": "N5",
  "totalScore": 92,
  "isPassed": true,
  "startedAt": "datetime",
  "submittedAt": "datetime | null",
  "sectionScores": [
    {
      "sectionId": "section-id",
      "sectionType": "Moji",
      "score": 28,
      "maxScore": 35,
      "passScore": 19,
      "isPassed": true
    }
  ],
  "questions": [
    {
      "questionId": "question-id",
      "questionText": "Câu hỏi...",
      "imageUrl": null,
      "explanation": "Giải thích đáp án đúng...",
      "sectionType": "Moji",
      "selectedOptionId": "option-id-a",
      "correctOptionId": "option-id-b",
      "isCorrect": false,
      "options": [
        {
          "optionId": "option-id-a",
          "label": "A",
          "text": "A",
          "imageUrl": null,
          "optionType": "Text"
        }
      ]
    }
  ]
}
```

**Lưu ý:**

- Chỉ response này mới có `correctOptionId`
- Dùng cho màn review sau khi nộp bài

---

### GET `/api/exam-sessions` 🔒

Lấy lịch sử làm bài của user hiện tại.

**Query params:**

| Param | Type | Bắt buộc | Enum | Mô tả |
| ----- | ---- | -------- | ---- | ----- |
| `examId` | `string` | ❌ | — | Lọc theo đề |
| `status` | `string` | ❌ | `ExamSessionStatus` | Lọc theo trạng thái |
| `page` | `int` | ❌ | — | Mặc định `1` |
| `pageSize` | `int` | ❌ | — | Mặc định `20` |

**Response data item:**

```json
{
  "sessionId": "session-id",
  "examId": "exam-id",
  "examTitle": "JLPT N5 Mock Test 01",
  "level": "N5",
  "status": "Submitted",
  "totalScore": 92,
  "isPassed": true,
  "startedAt": "datetime",
  "submittedAt": "datetime | null"
}
```

---

### POST `/api/exam-sessions` 🔒

Bắt đầu một phiên làm bài mới.

**Request body:**

```json
{
  "examId": "exam-id"
}
```

**Response data:**

```json
{
  "sessionId": "session-id",
  "examId": "exam-id",
  "examTitle": "JLPT N5 Mock Test 01",
  "level": "N5",
  "startedAt": "datetime",
  "expiresAt": "datetime",
  "sections": [
    {
      "sectionId": "section-id",
      "sectionType": "Moji",
      "orderIndex": 0,
      "durationMinutes": 25,
      "questionGroups": [
        {
          "groupId": "group-id",
          "passageText": null,
          "audioUrl": null,
          "instruction": "Chọn đáp án đúng nhất.",
          "orderIndex": 0,
          "mondaiType": null,
          "questions": [
            {
              "questionId": "question-id",
              "questionText": "Câu hỏi...",
              "imageUrl": null,
              "imageCaption": null,
              "orderIndex": 0,
              "options": [
                {
                  "optionId": "option-id",
                  "label": "A",
                  "text": "Đáp án A",
                  "imageUrl": null,
                  "optionType": "Text"
                }
              ],
              "selectedOptionId": null
            }
          ]
        }
      ]
    }
  ]
}
```

**Lưu ý rất quan trọng cho frontend user:**

- Response **không chứa đáp án đúng** khi đang làm bài
- `selectedOptionId` dùng để hydrate lại state khi resume
- `expiresAt` là mốc dùng cho countdown timer phía frontend

**Error codes:**

| Code | Khi nào |
| ---- | ------- |
| `Exam_NotFound_404` | Đề không tồn tại |
| `ExamSession_ExamNotPublished_400` | Đề chưa publish |

---

### GET `/api/exam-sessions/{id}` 🔒

Lấy lại trạng thái bài làm để resume sau khi reload / mất kết nối.

**Response data:** cùng shape với `POST /api/exam-sessions`.

**Error codes:**

| Code | Khi nào |
| ---- | ------- |
| `ExamSession_NotFound_404` | Session không tồn tại |
| `ExamSession_Forbidden_403` | Session không thuộc về user hiện tại |

---

### POST `/api/exam-sessions/{id}/answers` 🔒

Auto-save một câu trả lời.

**Request body:**

```json
{
  "questionId": "question-id",
  "selectedOptionId": "option-id"
}
```

**Lưu ý:**

- `questionId` là bắt buộc
- `selectedOptionId` có thể `null` nếu frontend muốn clear đáp án đã chọn
- Nên gọi endpoint này theo từng thao tác chọn đáp án

**Response data:**

```json
"Saved"
```

**Error codes:**

| Code | Khi nào |
| ---- | ------- |
| `ExamSession_NotFound_404` | Session không tồn tại |
| `ExamSession_AlreadySubmitted_400` | Session đã nộp bài hoặc hết giờ |
| `ExamSession_Expired_400` | Session đã hết hạn |
| `ExamSession_QuestionNotInExam_400` | `questionId` không thuộc đề này |
| `ExamSession_Forbidden_403` | Không phải session của user hiện tại |

---

### POST `/api/exam-sessions/{id}/submit` 🔒

Nộp bài và chấm điểm toàn bộ.

**Response data:**

```json
{
  "sessionId": "session-id",
  "totalScore": 92,
  "correctCount": 32,
  "wrongCount": 5,
  "unansweredCount": 3,
  "isPassed": true,
  "sectionScores": [
    {
      "sectionId": "section-id",
      "sectionType": "Moji",
      "score": 28,
      "maxScore": 35,
      "passScore": 19,
      "isPassed": true
    }
  ]
}
```

**Quy tắc pass/fail:**

- `isPassed = true` khi **tổng điểm đạt** và **không trượt section nào**
- Nếu trượt 1 section thì toàn bài vẫn fail

---

### GET `/api/exam-sessions/{id}/result` 🔒

Xem kết quả chi tiết sau khi đã submit.

**Response data:**

```json
{
  "sessionId": "session-id",
  "examId": "exam-id",
  "examTitle": "JLPT N5 Mock Test 01",
  "level": "N5",
  "totalScore": 92,
  "isPassed": true,
  "startedAt": "datetime",
  "submittedAt": "datetime | null",
  "sectionScores": [
    {
      "sectionId": "section-id",
      "sectionType": "Moji",
      "score": 28,
      "maxScore": 35,
      "passScore": 19,
      "isPassed": true
    }
  ],
  "questions": [
    {
      "questionId": "question-id",
      "questionText": "Câu hỏi...",
      "imageUrl": null,
      "explanation": "Giải thích đáp án đúng...",
      "sectionType": "Moji",
      "selectedOptionId": "option-id-a",
      "correctOptionId": "option-id-b",
      "isCorrect": false,
      "options": [
        {
          "optionId": "option-id-a",
          "label": "A",
          "text": "A",
          "imageUrl": null,
          "optionType": "Text"
        }
      ]
    }
  ]
}
```

**Lưu ý:**

- Chỉ response này mới có `correctOptionId`
- Dùng cho màn review sau khi nộp bài

---

### GET `/api/exam-sessions` 🔒

Lấy lịch sử làm bài của user hiện tại.

**Query params:**

| Param | Type | Bắt buộc | Enum | Mô tả |
| ----- | ---- | -------- | ---- | ----- |
| `examId` | `string` | ❌ | — | Lọc theo đề |
| `status` | `string` | ❌ | `ExamSessionStatus` | Lọc theo trạng thái |
| `page` | `int` | ❌ | — | Mặc định `1` |
| `pageSize` | `int` | ❌ | — | Mặc định `20` |

**Response data item:**

```json
{
  "sessionId": "session-id",
  "examId": "exam-id",
  "examTitle": "JLPT N5 Mock Test 01",
  "level": "N5",
  "status": "Submitted",
  "totalScore": 92,
  "isPassed": true,
  "startedAt": "datetime",
  "submittedAt": "datetime | null"
}
```

---

## 20. Shadowing Module

> APIs for topic-based shadowing practice across both learning-app and learning-admin.

### 20.1 Scope and App Ownership

#### `learning-app` Endpoints (User-facing)

| Method | Endpoint | Auth | Description |
| ------ | -------- | ---- | ----------- |
| GET | `/api/shadowing/topics` | 🔒 Auth | List readable shadowing topics for the current user |
| GET | `/api/shadowing/topics/{topicId}` | 🔒 Auth | Get topic detail with ordered sentences |
| GET | `/api/shadowing/topics/{topicId}/progress` | 🔒 Auth | Get aggregated progress for the current user on a topic |
| GET | `/api/shadowing/topics/{topicId}/sentences/progress` | 🔒 Auth | Get per-sentence progress list inside a topic |
| GET | `/api/shadowing/topics/{topicId}/resume` | 🔒 Auth | Get the recommended sentence to continue practicing |
| POST | `/api/shadowing/attempts` | 🔒 Auth | Submit shadowing audio and receive pronunciation assessment |
| GET | `/api/shadowing/attempts/{attemptId}` | 🔒 Auth | Get one attempt detail for the current user |
| GET | `/api/shadowing/attempts/history` | 🔒 Auth | Get paginated attempt history |
| GET | `/api/shadowing/sentences/{sentenceId}/progress` | 🔒 Auth | Get progress for one sentence |

#### `learning-admin` Endpoints (Admin-facing)

| Method | Endpoint | Auth | Description |
| ------ | -------- | ---- | ----------- |
| GET | `/api/admin/shadowing/topics` | 🔑 Editor/Admin | Search shadowing topics in admin scope |
| GET | `/api/admin/shadowing/topics/{topicId}` | 🔑 Editor/Admin | Get topic detail in admin scope |
| POST | `/api/admin/shadowing/topics` | 🔑 Editor/Admin | Create a new official shadowing topic |
| PATCH | `/api/admin/shadowing/topics/{topicId}` | 🔑 Editor/Admin | Update topic metadata |
| GET | `/api/admin/shadowing/topics/{topicId}/available-sentences` | 🔑 Editor/Admin | Search sentences for the shadowing topic builder |
| DELETE | `/api/admin/shadowing/topics/{topicId}` | 🔑 Editor/Admin | Delete a topic |
| POST | `/api/admin/shadowing/topics/{topicId}/sentences` | 🔑 Editor/Admin | Attach one sentence to a topic |
| POST | `/api/admin/shadowing/topics/{topicId}/sentences/bulk` | 🔑 Editor/Admin | Attach multiple sentences in one request |
| PUT | `/api/admin/shadowing/topics/{topicId}/sentences/{sentenceId}` | 🔑 Editor/Admin | Update topic-specific sentence metadata |
| DELETE | `/api/admin/shadowing/topics/{topicId}/sentences/{sentenceId}` | 🔑 Editor/Admin | Remove one sentence from a topic |
| POST | `/api/admin/shadowing/topics/{topicId}/sentences/reorder` | 🔑 Editor/Admin | Reorder topic sentences |
| GET | `/api/admin/shadowing/topics/{topicId}/analytics` | 🔑 Editor/Admin | Get topic-level analytics |
| GET | `/api/admin/shadowing/topics/{topicId}/analytics/sentences` | 🔑 Editor/Admin | Get per-sentence analytics for a topic |

#### Ownership Rules

- `learning-app` implements `/api/shadowing/*` only
- `learning-admin` implements `/api/admin/shadowing/*` only
- Do not mix service modules between apps

### 20.2 Enum and Value Reference

All enum values are serialized as **strings** (case-sensitive).

#### Level

| Value | Description |
| ----- | ----------- |
| `N1` | Cao cấp |
| `N2` | Trung cao cấp |
| `N3` | Trung cấp |
| `N4` | Sơ cấp trên |
| `N5` | Sơ cấp |

#### Visibility

| Value | Description |
| ----- | ----------- |
| `Public` | Public topic |
| `Private` | Private topic |

#### Status

| Value | Description |
| ----- | ----------- |
| `Draft` | Bản nháp, chưa public |
| `Published` | Đã xuất bản, public |
| `Archived` | Đã xóa mềm |

#### Locale for Attempt Submission

| Value | Description |
| ----- | ----------- |
| `ja-JP` | Japanese (default if omitted) |

### 20.3 Common Metadata Contract for List Endpoints

Paginated endpoints return `metaData`:

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `page` | `number` | No | Current page, normalized by backend |
| `pageSize` | `number` | No | Current page size, normalized by backend |
| `total` | `number` | No | Total record count |
| `totalPage` | `number` | No | Total page count |

Default paging: `page = 1`, `pageSize = 20`

### 20.4 Shared Response Models

#### `ShadowingTopicListItemResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `id` | `string` | No | Topic id |
| `title` | `string` | No | Topic title |
| `description` | `string` | No | Topic description |
| `coverImageUrl` | `string` | Yes | Optional topic cover image URL |
| `level` | `string` | Yes | JLPT level |
| `visibility` | `string` | No | `Public` or `Private` |
| `status` | `string` | No | `Draft`, `Published`, or `Archived` |
| `isOfficial` | `boolean` | No | Indicates an official/admin-created topic |
| `sentencesCount` | `number` | No | Number of attached sentences |
| `isOwner` | `boolean` | No | Whether the current user owns the topic |
| `creatorId` | `string` | No | Creator user id |
| `creatorName` | `string` | No | Creator display name |
| `createdAt` | `string` | No | ISO datetime |
| `updatedAt` | `string` | Yes | ISO datetime |

#### `ShadowingTopicSentenceResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `sentenceId` | `string` | No | Sentence id |
| `position` | `number` | No | Display order within the topic |
| `text` | `string` | No | Japanese sentence text |
| `meaning` | `string` | No | Meaning or translation |
| `audioUrl` | `string` | Yes | Sentence audio URL |
| `level` | `string` | Yes | JLPT level of the sentence |
| `note` | `string` | Yes | Admin note stored for this sentence inside the topic |

#### `ShadowingTopicDetailResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `id` | `string` | No | Topic id |
| `title` | `string` | No | Topic title |
| `description` | `string` | No | Topic description |
| `coverImageUrl` | `string` | Yes | Optional cover image URL |
| `level` | `string` | Yes | JLPT level |
| `visibility` | `string` | No | Topic visibility |
| `status` | `string` | No | Topic status |
| `isOfficial` | `boolean` | No | Whether the topic is official |
| `sentencesCount` | `number` | No | Number of attached sentences |
| `isOwner` | `boolean` | No | Whether the current user is the owner |
| `creatorId` | `string` | No | Creator user id |
| `creatorName` | `string` | No | Creator display name |
| `sentences` | `ShadowingTopicSentenceResponse[]` | No | Ordered topic sentences |
| `createdAt` | `string` | No | ISO datetime |
| `updatedAt` | `string` | Yes | ISO datetime |

#### `ShadowingAttemptResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `attemptId` | `string` | No | Attempt id |
| `topicId` | `string` | No | Topic id |
| `topicTitle` | `string` | No | Topic title at response time |
| `sentenceId` | `string` | No | Sentence id |
| `sentenceText` | `string` | No | Sentence text |
| `audioAssetId` | `string` | No | Uploaded media asset id |
| `audioUrl` | `string` | No | Uploaded audio URL |
| `locale` | `string` | No | Attempt locale. Current supported value is `ja-JP` |
| `recognizedText` | `string` | Yes | Speech recognition result |
| `pronScore` | `number` | Yes | Overall pronunciation score |
| `accuracyScore` | `number` | Yes | Accuracy sub-score |
| `fluencyScore` | `number` | Yes | Fluency sub-score |
| `completenessScore` | `number` | Yes | Completeness sub-score |
| `prosodyScore` | `number` | Yes | Prosody sub-score when available from the provider |
| `errorTypes` | `string[]` | No | Provider-reported pronunciation issue tags |
| `durationMs` | `number` | Yes | Audio duration in milliseconds |
| `createdAt` | `string` | No | ISO datetime |

#### `ShadowingAttemptHistoryItemResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `attemptId` | `string` | No | Attempt id |
| `topicId` | `string` | No | Topic id |
| `topicTitle` | `string` | No | Topic title |
| `sentenceId` | `string` | No | Sentence id |
| `sentenceText` | `string` | No | Sentence text |
| `locale` | `string` | No | Attempt locale |
| `pronScore` | `number` | Yes | Pronunciation score |
| `accuracyScore` | `number` | Yes | Accuracy score |
| `fluencyScore` | `number` | Yes | Fluency score |
| `completenessScore` | `number` | Yes | Completeness score |
| `prosodyScore` | `number` | Yes | Prosody score |
| `createdAt` | `string` | No | ISO datetime |

#### `ShadowingSentenceProgressResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `sentenceId` | `string` | No | Sentence id |
| `attemptsCount` | `number` | No | Number of attempts by the current user on this sentence |
| `bestPronScore` | `number` | Yes | Best pronunciation score |
| `latestPronScore` | `number` | Yes | Latest pronunciation score |
| `lastAttemptAt` | `string` | Yes | ISO datetime of latest attempt |

#### `ShadowingTopicProgressResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `topicId` | `string` | No | Topic id |
| `sentencesCount` | `number` | No | Number of sentences attached to the topic |
| `attemptedSentencesCount` | `number` | No | Number of distinct topic sentences attempted by the current user |
| `completedSentencesCount` | `number` | No | Currently equal to attempted sentence count in backend behavior |
| `bestPronScore` | `number` | Yes | Best pronunciation score among attempts in the topic |
| `latestPronScore` | `number` | Yes | Latest pronunciation score in the topic |
| `lastAttemptAt` | `string` | Yes | ISO datetime of the latest topic attempt |

#### `ShadowingTopicSentenceProgressItemResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `sentenceId` | `string` | No | Sentence id |
| `position` | `number` | No | Order in the topic |
| `text` | `string` | No | Sentence text |
| `meaning` | `string` | No | Sentence meaning |
| `audioUrl` | `string` | Yes | Sentence audio URL |
| `level` | `string` | Yes | Sentence JLPT level |
| `attemptsCount` | `number` | No | Number of attempts for this sentence by the current user |
| `bestPronScore` | `number` | Yes | Best pronunciation score for this sentence |
| `latestPronScore` | `number` | Yes | Latest pronunciation score for this sentence |
| `lastAttemptAt` | `string` | Yes | ISO datetime of latest attempt on this sentence |
| `hasAttempted` | `boolean` | No | Whether the user has attempted this sentence at least once |

#### `ShadowingTopicResumeResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `topicId` | `string` | No | Topic id |
| `recommendedSentenceId` | `string` | Yes | Preferred sentence to continue. Usually the first unattempted sentence, otherwise fallback to latest or first sentence |
| `lastAttemptSentenceId` | `string` | Yes | Sentence id from latest attempt |
| `attemptedSentencesCount` | `number` | No | Number of distinct attempted sentences |
| `remainingSentencesCount` | `number` | No | Remaining sentence count |
| `latestPronScore` | `number` | Yes | Latest pronunciation score |
| `lastAttemptAt` | `string` | Yes | ISO datetime of latest attempt |

#### `AdminShadowingAvailableSentenceResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `sentenceId` | `string` | No | Sentence id |
| `text` | `string` | No | Sentence text |
| `meaning` | `string` | No | Sentence meaning |
| `audioUrl` | `string` | Yes | Sentence audio URL |
| `speakerId` | `number` | Yes | VoiceVox speaker id when available |
| `level` | `string` | Yes | JLPT level |
| `isAttached` | `boolean` | No | Whether the sentence is already attached to the topic |
| `attachedPosition` | `number` | Yes | Existing position when attached |
| `attachedNote` | `string` | Yes | Existing topic-specific note when attached |

#### `ShadowingTopicAnalyticsResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `topicId` | `string` | No | Topic id |
| `attemptsCount` | `number` | No | Number of attempts on the topic |
| `distinctUsersCount` | `number` | No | Distinct users who attempted the topic |
| `averagePronScore` | `number` | Yes | Average pronunciation score across attempts with scores |
| `latestAttemptAt` | `string` | Yes | ISO datetime of latest attempt |

#### `ShadowingTopicSentenceAnalyticsResponse`

| Field | Type | Nullable | Description |
| ----- | ---- | -------- | ----------- |
| `sentenceId` | `string` | No | Sentence id |
| `position` | `number` | No | Position inside the topic |
| `text` | `string` | No | Sentence text |
| `attemptsCount` | `number` | No | Number of attempts for this sentence within the topic |
| `distinctUsersCount` | `number` | No | Distinct users who attempted this sentence in the topic |
| `averagePronScore` | `number` | Yes | Average pronunciation score |
| `latestAttemptAt` | `string` | Yes | ISO datetime of latest attempt |

### 20.5 `learning-app` Integration Guide

#### 20.5.1 GET `/api/shadowing/topics`

**Purpose:** Load the topic list screen for the user app.

**Query params:**

| Field | Type | Required | Default | Notes |
| ----- | ---- | -------- | ------- | ----- |
| `q` | `string` | No | none | Keyword search |
| `level` | `string` | No | none | `N1` to `N5` |
| `visibility` | `string` | No | none | Usually omit this in user app unless an owner-only UI needs it |
| `officialOnly` | `boolean` | No | none | Filter official topics only |
| `page` | `number` | No | `1` | Paging |
| `pageSize` | `number` | No | `20` | Paging |

**Success response:**
- `data`: `ShadowingTopicListItemResponse[]`
- `metaData`: paging metadata

**Frontend notes:**
- This endpoint already applies user access rules
- The app should not try to filter out `Draft` topics manually; backend already does that
- `creatorName` and `coverImageUrl` are safe to display directly in list cards

**Example:**
```http
GET /api/shadowing/topics?level=N5&officialOnly=true&page=1&pageSize=12
```

#### 20.5.2 GET `/api/shadowing/topics/{topicId}`

**Purpose:** Load topic detail before entering practice.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Success response:**
- `data`: `ShadowingTopicDetailResponse`

**Frontend notes:**
- `sentences` are already ordered by `position`
- Use `sentencesCount` for summary display, not `sentences.length`, if you want to mirror backend state exactly

#### 20.5.3 GET `/api/shadowing/topics/{topicId}/progress`

**Purpose:** Load topic summary progress for dashboards or a continue-learning card.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Success response:**
- `data`: `ShadowingTopicProgressResponse`

**Frontend notes:**
- `completedSentencesCount` is currently equal to `attemptedSentencesCount` in backend behavior
- If product later defines a stricter completion rule, backend may change this field without changing the route

#### 20.5.4 GET `/api/shadowing/topics/{topicId}/sentences/progress`

**Purpose:** Render a sentence checklist or progress drawer in the user app.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Success response:**
- `data`: `ShadowingTopicSentenceProgressItemResponse[]`

**Frontend notes:**
- Join this list with topic detail by `sentenceId` only if needed. In most cases this response already contains enough UI data
- `hasAttempted = false` means all score/time fields may be `null`

#### 20.5.5 GET `/api/shadowing/topics/{topicId}/resume`

**Purpose:** Restore the best next sentence when the user taps a continue button.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Success response:**
- `data`: `ShadowingTopicResumeResponse`

**Frontend notes:**
- `recommendedSentenceId` may be `null` if the topic has no sentences
- If it exists, the frontend should navigate to that sentence first
- `lastAttemptSentenceId` is useful for showing a "resume from latest" label

#### 20.5.6 POST `/api/shadowing/attempts`

**Purpose:** Submit one recorded audio file and receive assessment scores.

**Content type:** `multipart/form-data`

**Form-data fields:**

| Field | Type | Required | Default | Notes |
| ----- | ---- | -------- | ------- | ----- |
| `topicId` | `string` | Yes | none | Max length 50 |
| `sentenceId` | `string` | Yes | none | Max length 50 |
| `locale` | `string` | No | `ja-JP` | If provided, only `ja-JP` is valid. Max length 20 |
| `audio` | `file` | Yes | none | Required. Max size 20 MB. MIME must be in backend allowed audio MIME list |

**Success response:**
- `data`: `ShadowingAttemptResponse`

**Frontend notes:**
- The frontend should not send a locale picker yet unless product explicitly needs one
- Safe default: omit `locale` entirely and let backend fill `ja-JP`
- Use the response scores directly to update local progress UI
- `audioUrl` is the uploaded attempt recording, not the original sentence audio

**Minimal request example:**
```http
POST /api/shadowing/attempts
Content-Type: multipart/form-data
```
Form fields:
- `topicId = topic-123`
- `sentenceId = sentence-456`
- `audio = <recorded-file>`

#### 20.5.7 GET `/api/shadowing/attempts/{attemptId}`

**Purpose:** Reload a detail screen for a previous attempt.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `attemptId` | `string` | Yes | Attempt id |

**Success response:**
- `data`: `ShadowingAttemptResponse`

**Frontend notes:**
- This endpoint is owner-scoped. If the attempt belongs to another user, backend returns a not-found business error

#### 20.5.8 GET `/api/shadowing/attempts/history`

**Purpose:** Render attempt history screen, user profile activity, or a topic-specific history list.

**Query params:**

| Field | Type | Required | Default | Notes |
| ----- | ---- | -------- | ------- | ----- |
| `topicId` | `string` | No | none | Filter by topic |
| `sentenceId` | `string` | No | none | Filter by sentence |
| `page` | `number` | No | `1` | Paging |
| `pageSize` | `number` | No | `20` | Paging |

**Success response:**
- `data`: `ShadowingAttemptHistoryItemResponse[]`
- `metaData`: paging metadata

#### 20.5.9 GET `/api/shadowing/sentences/{sentenceId}/progress`

**Purpose:** Render progress badge/state for one sentence outside the topic progress list.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `sentenceId` | `string` | Yes | Sentence id |

**Success response:**
- `data`: `ShadowingSentenceProgressResponse`

**Frontend notes:**
- This is useful for shared sentence cards or mini progress widgets

#### 20.5.10 Recommended Implementation Order for `learning-app`

1. Topic list
2. Topic detail
3. Submit attempt
4. Attempt result view
5. Topic resume
6. Topic sentence progress
7. Attempt history
8. Sentence progress widgets

### 16.6 `learning-admin` Integration Guide

#### 16.6.1 GET `/api/admin/shadowing/topics`

**Purpose:** Render admin topic table and filters.

**Query params:**

| Field | Type | Required | Default | Notes |
| ----- | ---- | -------- | ------- | ----- |
| `q` | `string` | No | none | Keyword search |
| `level` | `string` | No | none | `N1` to `N5` |
| `visibility` | `string` | No | none | `Public` or `Private` |
| `status` | `string` | No | none | `Draft`, `Published`, or `Archived` |
| `isOfficial` | `boolean` | No | none | Filter by official flag |
| `createdBy` | `string` | No | none | Filter by creator id |
| `page` | `number` | No | `1` | Paging |
| `pageSize` | `number` | No | `20` | Paging |

**Success response:**
- `data`: `ShadowingTopicListItemResponse[]`
- `metaData`: paging metadata

#### 16.6.2 GET `/api/admin/shadowing/topics/{topicId}`

**Purpose:** Load topic detail for editor screens.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Success response:**
- `data`: `ShadowingTopicDetailResponse`

#### 16.6.3 POST `/api/admin/shadowing/topics`

**Purpose:** Create a new topic.

**JSON body fields:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `title` | `string` | Yes | Max length 200 |
| `description` | `string` | Yes | Max length 2000 |
| `coverImageUrl` | `string` | No | Max length 512 |
| `level` | `string` | No | `N1` to `N5` |
| `visibility` | `string` | No | `Public` or `Private` |
| `status` | `string` | No | `Draft`, `Published`, or `Archived`. If omitted, backend defaults to `Draft` |

**Success response:**
- `data`: `ShadowingTopicDetailResponse`

**Frontend notes:**
- You can create a topic without sending `status`; backend will store `Draft`
- `coverImageUrl` must already be a usable URL. This endpoint does not upload files

**Minimal request example:**
```json
{
  "title": "Shadowing N5 Greetings",
  "description": "Basic Japanese greeting practice",
  "coverImageUrl": "https://cdn.example.com/shadowing/greetings-cover.webp",
  "level": "N5",
  "visibility": "Public"
}
```

#### 16.6.4 PATCH `/api/admin/shadowing/topics/{topicId}`

**Purpose:** Update topic metadata.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**JSON body fields:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `title` | `string` | No | Max length 200 |
| `description` | `string` | No | Max length 2000 |
| `coverImageUrl` | `string` | No | Max length 512 |
| `level` | `string` | No | `N1` to `N5` |
| `visibility` | `string` | No | `Public` or `Private` |
| `status` | `string` | No | `Draft`, `Published`, or `Archived` |

**Success response:**
- `data`: `ShadowingTopicDetailResponse`

**Frontend notes:**
- This is a partial update request. Only send fields you actually want to modify

#### 16.6.5 GET `/api/admin/shadowing/topics/{topicId}/available-sentences`

**Purpose:** Power the sentence picker inside the topic builder.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Query params:**

| Field | Type | Required | Default | Notes |
| ----- | ---- | -------- | ------- | ----- |
| `q` | `string` | No | none | Max length 200. Keyword search |
| `level` | `string` | No | none | `N1` to `N5` |
| `hasAudio` | `boolean` | No | none | `true` only audio, `false` only no audio, omitted means no audio filter |
| `page` | `number` | No | `1` | Paging |
| `pageSize` | `number` | No | `20` | Paging |

**Success response:**
- `data`: `AdminShadowingAvailableSentenceResponse[]`
- `metaData`: paging metadata

**Frontend notes:**
- This response already tells you whether a sentence is attached
- Disable or convert the attach action when `isAttached = true`
- `attachedPosition` and `attachedNote` are useful for edit chips or prefilled UI

#### 16.6.6 DELETE `/api/admin/shadowing/topics/{topicId}`

**Purpose:** Delete a topic.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Success response:**
- `data`: `boolean`

**Frontend notes:**
- Treat `true` as success confirmation and remove the row from local state

#### 16.6.7 POST `/api/admin/shadowing/topics/{topicId}/sentences`

**Purpose:** Attach one sentence to a topic.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**JSON body fields:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `sentenceId` | `string` | Yes | Max length 50 |
| `position` | `number` | Yes | Must be greater than 0 |
| `note` | `string` | No | Max length 1000 |

**Success response:**
- `data`: `ShadowingTopicSentenceResponse`

#### 16.6.8 POST `/api/admin/shadowing/topics/{topicId}/sentences/bulk`

**Purpose:** Attach multiple sentences in a single request.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**JSON body fields:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `items` | `AttachShadowingTopicSentenceRequest[]` | Yes | Must not be empty |
| `items[].sentenceId` | `string` | Yes | Max length 50 |
| `items[].position` | `number` | Yes | Must be greater than 0 |
| `items[].note` | `string` | No | Max length 1000 |

**Success response:**
- `data`: `ShadowingTopicSentenceResponse[]`

**Frontend notes:**
- Backend rejects duplicate positions in the payload
- Backend also rejects sentences already attached to the topic
- Before sending bulk attach, deduplicate selected rows on the frontend to keep UX clean

**Example request:**
```json
{
  "items": [
    {
      "sentenceId": "sentence-1",
      "position": 1,
      "note": "Intro"
    },
    {
      "sentenceId": "sentence-2",
      "position": 2,
      "note": "Follow-up"
    }
  ]
}
```

#### 16.6.9 PUT `/api/admin/shadowing/topics/{topicId}/sentences/{sentenceId}`

**Purpose:** Update position and note for one attached sentence.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |
| `sentenceId` | `string` | Yes | Attached sentence id |

**JSON body fields:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `position` | `number` | Yes | Must be greater than 0 |
| `note` | `string` | No | Max length 1000 |

**Success response:**
- `data`: `ShadowingTopicSentenceResponse`

#### 16.6.10 DELETE `/api/admin/shadowing/topics/{topicId}/sentences/{sentenceId}`

**Purpose:** Remove one attached sentence.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |
| `sentenceId` | `string` | Yes | Attached sentence id |

**Success response:**
- `data`: `boolean`

#### 16.6.11 POST `/api/admin/shadowing/topics/{topicId}/sentences/reorder`

**Purpose:** Persist drag-and-drop ordering for the topic builder.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**JSON body fields:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `items` | `ReorderShadowingTopicSentenceItemRequest[]` | Yes | Must not be empty |
| `items[].sentenceId` | `string` | Yes | Sentence id |
| `items[].position` | `number` | Yes | Intended final position |

**Success response:**
- `data`: `ShadowingTopicSentenceResponse[]`

**Frontend notes:**
- Backend rejects duplicated positions
- Send the full intended order, not only one changed row

#### 16.6.12 GET `/api/admin/shadowing/topics/{topicId}/analytics`

**Purpose:** Show summary analytics on the topic detail page.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Success response:**
- `data`: `ShadowingTopicAnalyticsResponse`

#### 16.6.13 GET `/api/admin/shadowing/topics/{topicId}/analytics/sentences`

**Purpose:** Show per-sentence analytics table or chart.

**Path params:**

| Field | Type | Required | Notes |
| ----- | ---- | -------- | ----- |
| `topicId` | `string` | Yes | Topic id |

**Success response:**
- `data`: `ShadowingTopicSentenceAnalyticsResponse[]`

#### 16.6.14 Recommended Implementation Order for `learning-admin`

1. Admin topic list
2. Create topic
3. Edit topic metadata
4. Available sentence search
5. Attach one sentence
6. Bulk attach
7. Reorder sentence list
8. Delete sentence
9. Topic analytics
10. Sentence analytics

### 16.7 Access and Visibility Rules

#### User APIs

All `/api/shadowing/*` endpoints require authentication.

Access behavior:
- A user can only read topics where:
  - `status = Published`
  - and `visibility = Public` or `creatorId = currentUserId`
- A user can only access their own attempts
- Progress endpoints are always scoped to the current authenticated user

#### Admin APIs

All `/api/admin/shadowing/*` endpoints require `Editor/Admin` policy.

Access behavior:
- Admin endpoints are intended for topic management and analytics
- Topic creation through admin automatically stores `isOfficial = true`
- Topic creation defaults `status` to `Draft` if omitted

### 16.8 Shadowing Error Codes

| Message | Meaning for Frontend |
| ------- | -------------------- |
| `Validation_400` | DTO or query validation failed. `data` contains field-level errors |
| `Common_401` | User is not authenticated or does not have required access |
| `Common_404` | Generic not found fallback |
| `Common_500` | Internal server error |
| `Shadowing_TopicNotFound_404` | Topic does not exist or caller has no access to it |
| `Shadowing_AttemptNotFound_404` | Attempt does not exist or does not belong to the current user |
| `Shadowing_SentenceNotFound_404` | Sentence does not exist |
| `Shadowing_SentenceNotAttached_404` | Topic does not contain the requested sentence |
| `Shadowing_SentenceAlreadyAttached_400` | Admin tried to attach a sentence that is already attached |
| `Shadowing_InvalidAudio_400` | Uploaded audio is empty or otherwise invalid |
| `Shadowing_AssessmentFailed_500` | External pronunciation assessment failed |
| `Shadowing_AzureNotConfigured_500` | Speech provider is not configured on backend |
| `Shadowing_DuplicatePosition_400` | Reorder or bulk attach payload contains duplicate positions |

Error handling guidance:
- Show friendly UI text based on `message`
- Preserve the backend code in logs and analytics
- For `Shadowing_TopicNotFound_404`, redirect away from the detail page or show a no-access state
- For `Shadowing_AttemptNotFound_404`, remove stale cached attempt detail state
- For `Shadowing_DuplicatePosition_400`, keep the user on the builder screen and ask for a refresh or list correction

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

### User

| Code | Mô tả |
| ---- | ----- |
| `User_NotFound_404` | Không tìm thấy user theo id |
| `User_Inactive_403` | Tài khoản đã bị khóa hoặc không còn được phép đăng nhập |
| `User_CannotChangeOwnRole_400` | Admin không được tự đổi role của chính mình |
| `User_CannotDeactivateSelf_400` | Admin không được tự khóa chính mình |

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

### Learning

| Code | Mô tả |
| ---- | ----- |
| `Learning_SessionNotFound_404` | Session không tồn tại hoặc không thuộc user hiện tại |
| `Learning_CardNotFound_404` | Card không tồn tại |
| `Learning_SentenceNotAttached_404` | Sentence không được gắn với card hiện tại |
| `Learning_SentenceAlreadyAttached_400` | Sentence đã được gắn với card hiện tại |
| `Learning_SessionCompleted_400` | Session đã hoàn thành và không thể submit tiếp |
| `Learning_InvalidMode_400` | Mode học không hợp lệ |
| `Learning_InvalidScope_400` | Card hoặc folder không thuộc deck được chọn |
| `Learning_CardNotInSession_400` | Card submit không nằm trong session |
| `Learning_InvalidSubmission_400` | Submit không hợp lệ hoặc submit trùng card đã làm |
| `Learning_NoCardsAvailable_400` | Không có card nào để tạo session |
