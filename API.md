# Tacho Learning API - Implemented API Documentation

> **Last updated:** 2026-04-10

## Response Contract

Most business and validation failures are returned in HTTP 200 with this shape:

```json
{
  "code": 400,
  "success": false,
  "message": "Error_Code_400",
  "data": {},
  "metaData": null
}
```

Unhandled exceptions are returned as HTTP 500.

## Auth Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Đăng ký tài khoản |
| POST | `/api/auth/login` | No | Đăng nhập |
| POST | `/api/auth/refresh-token` | No | Làm mới access token |
| POST | `/api/auth/refresh` | No | Alias của refresh-token |
| GET | `/api/auth/me` | Yes | Lấy user hiện tại |
| PATCH | `/api/auth/me/profile` | Yes | Cập nhật profile |
| POST | `/api/auth/me/avatar` | Yes | Upload avatar image |
| PATCH | `/api/auth/change-password` | Yes | Đổi mật khẩu |
| POST | `/api/auth/logout` | Yes | Đăng xuất |
| POST | `/api/auth/forgot-password` | No | Gửi email reset password |
| POST | `/api/auth/reset-password` | No | Reset password |

### POST `/api/auth/me/avatar`

Upload ảnh avatar mới cho user hiện tại.

- Content-Type: `multipart/form-data`
- Form field: `avatar`
- Allowed mime: `image/jpeg`, `image/png`, `image/webp`
- Max size: `5 MB`

Response data (`AuthUserDTO`):

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

## Uploads Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/uploads/audio` | Yes | Upload audio resource |

### POST `/api/uploads/audio`

Upload file audio và lưu metadata vào `MediaAssets`.

- Content-Type: `multipart/form-data`
- Form field: `audio`
- Allowed mime: `audio/mpeg`, `audio/wav`, `audio/mp4`
- Max size: `20 MB`

Response data:

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

## Voicevox Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/voicevox/speakers` | Yes | Lấy danh sách speaker VOICEVOX được phép dùng |
| POST | `/api/voicevox/preview` | Yes | Generate preview audio theo `speakerId` |

### POST `/api/voicevox/preview`

Generate audio preview để frontend phát thử khi admin đổi speaker.

Request body:

```json
{
  "speakerId": 3,
  "text": "こんにちは。こちらは音声プレビューです。"
}
```

- `speakerId`: bắt buộc
- `text`: tùy chọn, nếu bỏ trống backend dùng sample text mặc định

Response data:

```json
{
  "speakerId": 3,
  "text": "こんにちは。こちらは音声プレビューです。",
  "audioUrl": "/audio-cache/example.wav"
}
```

---

## Sentences Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/sentences` | Editor/Admin | Tìm kiếm sentence có phân trang |
| GET | `/api/sentences/{id}` | Editor/Admin | Lấy chi tiết sentence |
| POST | `/api/sentences` | Editor/Admin | Tạo sentence mới |
| PATCH | `/api/sentences/{id}` | Editor/Admin | Cập nhật sentence |
| DELETE | `/api/sentences/{id}` | Editor/Admin | Xóa sentence |

### Sentence create/update note

Từ ngày 2026-04-09, `sentence` chuyển sang luồng **VOICEVOX-only**:

- Client không gửi `audioUrl` nữa.
- Backend luôn tự generate `audioUrl` từ VOICEVOX dựa trên `text` và `speakerId`.
- `speakerId` là speaker dùng để generate audio và được lưu lại trong DB.

Request body cho `POST /api/sentences` và `PATCH /api/sentences/{id}`:

```json
{
  "text": "日本へ行きたいです。",
  "meaning": "Tôi muốn đi Nhật.",
  "speakerId": 3,
  "level": "N5"
}
```

Response data:

```json
{
  "id": "string",
  "text": "string",
  "meaning": "string",
  "audioUrl": "string | null",
  "speakerId": 3,
  "level": "N5 | N4 | N3 | N2 | N1 | null"
}
```

---

## Vocabulary Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/vocabulary` | Editor/Admin | Tìm kiếm vocabulary có phân trang |
| GET | `/api/vocabulary/{cardId}` | Public | Lấy chi tiết vocabulary |
| GET | `/api/vocabulary/import-template` | Editor/Admin | Tải JSON template cho import vocabulary |
| GET | `/api/vocabulary/export` | Editor/Admin | Tải JSON export vocabulary theo bộ lọc |
| POST | `/api/vocabulary/import/preview` | Editor/Admin | Preview file import vocabulary, chưa ghi DB |
| POST | `/api/vocabulary/import/commit` | Editor/Admin | Commit batch import vocabulary |
| POST | `/api/vocabulary` | Editor/Admin | Tạo vocabulary mới |
| PATCH | `/api/vocabulary/{cardId}` | Editor/Admin | Cập nhật vocabulary |
| DELETE | `/api/vocabulary/{cardId}` | Editor/Admin | Soft delete vocabulary |

### Vocabulary create/update note

Từ ngày 2026-04-09, `vocabulary` chuyển sang luồng **VOICEVOX-only** cho audio:

- Client không gửi `audioUrl` nữa.
- Backend luôn tự generate `audioUrl` từ VOICEVOX.
- Backend ưu tiên dùng `reading` để generate audio; nếu `reading` trống thì fallback sang `writing`.
- `speakerId` là speaker dùng để generate audio và được lưu lại trong DB.
- `pitchPattern` vẫn được phép gửi để frontend-admin override thủ công nếu pitch từ VOICEVOX chưa chuẩn.
- `sentences` trong request là danh sách nested upsert cho example sentences của vocabulary.
- Nếu một sentence item có `id`, backend sẽ update sentence đó rồi giữ/gắn association vào vocabulary.
- Nếu một sentence item không có `id`, backend sẽ tạo sentence mới, generate audio bằng VOICEVOX, rồi gắn vào vocabulary.
- Với `PATCH /api/vocabulary/{cardId}`, danh sách `sentences` gửi lên được xem là trạng thái cuối cùng; association nào không còn trong request sẽ bị gỡ khỏi vocabulary.

Request body cho `POST /api/vocabulary` và `PATCH /api/vocabulary/{cardId}`:

```json
{
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
  "meanings": [
    {
      "partOfSpeech": "VerbRu",
      "definitions": ["ăn", "dùng bữa"]
    }
  ],
  "synonyms": [],
  "antonyms": [],
  "relatedPhrases": [],
  "sentences": [
    {
      "id": "optional-existing-sentence-id",
      "text": "毎朝パンを食べる。",
      "meaning": "Mỗi sáng tôi ăn bánh mì.",
      "speakerId": 3,
      "level": "N5"
    },
    {
      "text": "野菜をもっと食べたほうがいい。",
      "meaning": "Bạn nên ăn rau nhiều hơn.",
      "speakerId": 8,
      "level": "N4"
    }
  ]
}
```

Response detail vẫn trả `audioUrl`, `speakerId`, `pitchPattern` như trước để frontend phát audio và hiển thị accent.

### GET `/api/vocabulary/import-template`

Trả về file `application/json` theo đúng shape import. File mẫu hiện tại gồm 1 item sample, có thể tải về, sửa dữ liệu rồi gọi preview/import sau.

Response file body:

```json
{
  "items": [
    {
      "rowNumber": 1,
      "mode": "create",
      "existingCardId": null,
      "title": "食べる",
      "summary": "Động từ ăn",
      "level": "N5",
      "tags": ["verb", "daily-life"],
      "status": "Draft",
      "writing": "食べる",
      "reading": "たべる",
      "pitchPattern": [0, 1, 0],
      "speakerId": 3,
      "wordType": "Native",
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
          "id": null,
          "text": "毎朝パンを食べる。",
          "meaning": "Mỗi sáng tôi ăn bánh mì.",
          "speakerId": 3,
          "level": "N5"
        }
      ]
    }
  ]
}
```

### GET `/api/vocabulary/export`

Tải file `application/json` cùng shape với payload import, để có thể round-trip chỉnh sửa và import lại.

Query params hỗ trợ:

- `q`
- `level`
- `status`
- `wordType`
- `hasAudio`
- `createdByMe`

Export item có:

- `mode = "upsert"`
- `existingCardId = <id của vocabulary>`

Response file body:

```json
{
  "items": [
    {
      "rowNumber": null,
      "mode": "upsert",
      "existingCardId": "ebc8715e-0ea7-40cf-9930-093d4b74afee",
      "title": "断る",
      "summary": "động từ từ chối",
      "level": "N5",
      "tags": ["verb"],
      "status": "Draft",
      "writing": "断る",
      "reading": "ことわる",
      "pitchPattern": [1, 1, 1, 1],
      "speakerId": 8,
      "wordType": "Native",
      "meanings": [
        {
          "partOfSpeech": "VerbRu",
          "definitions": ["từ chối"]
        }
      ],
      "synonyms": [],
      "antonyms": [],
      "relatedPhrases": [],
      "sentences": []
    }
  ]
}
```

### POST `/api/vocabulary/import/preview`

Preview payload import, validate theo từng item và trả về danh sách lỗi/cảnh báo, chưa ghi DB.

Request body:

```json
{
  "items": [
    {
      "rowNumber": 1,
      "mode": "create",
      "existingCardId": null,
      "title": "食べる",
      "summary": "Động từ ăn",
      "level": "N5",
      "tags": ["verb", "daily-life"],
      "status": "Draft",
      "writing": "食べる",
      "reading": "たべる",
      "pitchPattern": [0, 1, 0],
      "speakerId": 3,
      "wordType": "Native",
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
          "id": null,
          "text": "毎朝パンを食べる。",
          "meaning": "Mỗi sáng tôi ăn bánh mì.",
          "speakerId": 3,
          "level": "N5"
        }
      ]
    }
  ]
}
```

Response data:

```json
{
  "totalItems": 1,
  "validItems": 1,
  "invalidItems": 0,
  "items": [
    {
      "rowNumber": 1,
      "mode": "create",
      "existingCardId": null,
      "title": "食べる",
      "writing": "食べる",
      "isValid": true,
      "errors": [],
      "warnings": []
    }
  ]
}
```

### POST `/api/vocabulary/import/commit`

Commit batch import sau khi payload đã hợp lệ. Endpoint này sẽ:

- chạy `preview` nội bộ trước
- nếu còn item invalid thì không ghi DB
- nếu hợp lệ thì xử lý tuần tự từng item
- `mode = create` sẽ tạo card mới
- `mode = upsert` sẽ update theo `existingCardId`

Request body cùng shape với `import/preview`.

Response data:

```json
{
  "totalItems": 2,
  "successfulItems": 2,
  "failedItems": 0,
  "hasValidationErrors": false,
  "items": [
    {
      "rowNumber": 1,
      "mode": "create",
      "existingCardId": null,
      "title": "食べる",
      "writing": "食べる",
      "isSuccess": true,
      "action": "created",
      "cardId": "new-card-id",
      "errors": []
    },
    {
      "rowNumber": 2,
      "mode": "upsert",
      "existingCardId": "existing-card-id",
      "title": "断る",
      "writing": "断る",
      "isSuccess": true,
      "action": "updated",
      "cardId": "existing-card-id",
      "errors": []
    }
  ]
}
```
