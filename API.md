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
| GET | `/api/sentences/import-template` | Editor/Admin | Tải JSON template cho import sentences |
| GET | `/api/sentences/export` | Editor/Admin | Tải JSON export sentences theo bộ lọc |
| POST | `/api/sentences/import/preview` | Editor/Admin | Preview file import sentences, chưa ghi DB |
| POST | `/api/sentences/import/commit` | Editor/Admin | Commit batch import sentences |
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

### Sentences import/export note

- Sentence import/export dùng JSON file tương tự vocabulary nhưng đơn giản hơn.
- Import sentence hiện tại là **create-only**.
- Backend không nhận `audioUrl`; khi commit import, backend sẽ tự synth audio bằng VOICEVOX từ `text` và `speakerId`.
- Export sentence trả về đúng shape import để frontend có thể chỉnh sửa rồi import tạo mới hàng loạt.

### GET `/api/sentences/import-template`

Trả về file `application/json` theo đúng shape import sentence.

Response file body:

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

### GET `/api/sentences/export`

Tải file `application/json` cùng shape với payload import, để có thể chỉnh sửa và import tạo mới hàng loạt.

Query params hỗ trợ:

- `q`
- `level`
- `hasAudio`
- `createdByMe`

Response file body:

```json
{
  "items": [
    {
      "rowNumber": null,
      "text": "日本へ行きたいです。",
      "meaning": "Tôi muốn đi Nhật.",
      "speakerId": 3,
      "level": "N5"
    }
  ]
}
```

### POST `/api/sentences/import/preview`

Preview payload import, validate theo từng item và trả về danh sách lỗi/cảnh báo, chưa ghi DB.

Với lỗi field-level, backend trả theo format:

- `<MessageCode>:<fieldPath>`

Ví dụ:

- `Sentence_ImportFieldRequired_400:text`
- `Sentence_ImportFieldTooLong_400:meaning`
- `Sentence_ImportFieldInvalid_400:level`
- `Sentence_ImportSpeakerIdNotSupported_400:speakerId`

Request body:

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

Response data:

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

Ví dụ response khi payload không hợp lệ:

```json
{
  "totalItems": 1,
  "validItems": 0,
  "invalidItems": 1,
  "items": [
    {
      "rowNumber": 1,
      "text": "",
      "isValid": false,
      "errors": [
        "Sentence_ImportFieldRequired_400:text",
        "Sentence_ImportFieldInvalid_400:level",
        "Sentence_ImportSpeakerIdNotSupported_400:speakerId"
      ],
      "warnings": []
    }
  ]
}
```

### POST `/api/sentences/import/commit`

Commit batch import sau khi payload đã hợp lệ. Endpoint này sẽ:

- chạy `preview` nội bộ trước
- nếu còn item invalid thì không ghi DB
- nếu hợp lệ thì xử lý tuần tự từng item
- mỗi item hợp lệ sẽ tạo sentence mới
- mỗi sentence mới sẽ được generate audio bằng VOICEVOX như `POST /api/sentences`

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
      "text": "日本へ行きたいです。",
      "isSuccess": true,
      "action": "created",
      "sentenceId": "new-sentence-id-1",
      "errors": []
    },
    {
      "rowNumber": 2,
      "text": "毎日日本語を勉強します。",
      "isSuccess": true,
      "action": "created",
      "sentenceId": "new-sentence-id-2",
      "errors": []
    }
  ]
}
```

---

## Grammar Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/grammar` | Editor/Admin | Tìm kiếm grammar có phân trang |
| GET | `/api/grammar/{cardId}` | Public | Lấy chi tiết grammar |
| GET | `/api/grammar/import-template` | Editor/Admin | Tải JSON template cho import grammar |
| GET | `/api/grammar/export` | Editor/Admin | Tải JSON export grammar theo bộ lọc |
| POST | `/api/grammar/import/preview` | Editor/Admin | Preview payload import grammar, chưa ghi DB |
| POST | `/api/grammar/import/commit` | Editor/Admin | Commit batch import grammar |
| POST | `/api/grammar` | Editor/Admin | Tạo grammar mới |
| PATCH | `/api/grammar/{cardId}` | Editor/Admin | Cập nhật grammar |
| DELETE | `/api/grammar/{cardId}` | Editor/Admin | Soft delete grammar (Archived) |

### Search query (`GET /api/grammar`)

Hỗ trợ các query params:

- `q`
- `level`
- `status`
- `register`
- `createdByMe`
- `page`
- `pageSize`

`q` hiện tìm theo:

- `title`
- `summary`
- `alternateForms`
- `structures.pattern`

`q` **không** tìm trong `explanation`.

### Import/export note

- Grammar import hiện tại là **create-only**.
- `import/preview` chỉ validate payload theo item, không ghi DB.
- `import/commit` luôn chạy preview nội bộ trước; nếu còn item invalid sẽ block commit.
- `export` trả đúng shape với payload import để frontend có thể chỉnh sửa rồi import lại.

### Rich text rule (Markdown subset)

Các field rich text:

- `structures[].pattern`
- `structures[].annotations[*]` (nếu có)
- `explanation`
- `caution`

Cho phép:

- `**bold**`
- `*italic*`
- `~~strikethrough~~`
- Underline custom token: `{u}text{/u}`
- Color custom token (whitelist): `{red|blue|green|yellow|orange|purple|gray}text{/<color>}`

Không cho phép:

- Raw HTML (`<tag>...</tag>`)
- Token sai cú pháp hoặc không đóng cặp
- Token màu ngoài whitelist

Giới hạn độ dài:

- `structures[].pattern`: tối đa 1000 ký tự
- `structures[].annotations[*]`: tối đa 1000 ký tự/value
- `explanation`: tối đa 10000 ký tự
- `caution`: tối đa 5000 ký tự

### GET `/api/grammar/import-template`

Trả về file JSON sample đúng shape import grammar.

### GET `/api/grammar/export`

Query params hỗ trợ:

- `q`
- `level`
- `status`
- `register`
- `createdByMe`

Response file body cùng shape với import request.

### POST `/api/grammar/import/preview`

Request body:

```json
{
  "items": [
    {
      "rowNumber": 1,
      "title": "〜ながら",
      "summary": "Vừa làm A vừa làm B.",
      "level": "N4",
      "tags": ["grammar", "simultaneous"],
      "status": "Draft",
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
      "register": "Standard",
      "alternateForms": ["〜つつ"],
      "relations": [
        { "relatedId": "grammar-card-001", "relationType": "Similar" }
      ],
      "resources": [
        { "title": "Bài giảng", "url": "https://example.com/grammar/nagara" }
      ],
      "sentences": [
        {
          "text": "音楽を聞きながら勉強します。",
          "meaning": "Tôi vừa nghe nhạc vừa học.",
          "speakerId": 3,
          "level": "N4"
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
      "title": "〜ながら",
      "isValid": true,
      "errors": [],
      "warnings": []
    }
  ]
}
```

### POST `/api/grammar/import/commit`

Request body cùng shape với `import/preview`.

Response data:

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

Một số message code frontend cần bắt:

- `Grammar_ImportInvalidPayload_400`
- `Grammar_ImportBatchHasErrors_400`
- `Grammar_ImportFieldRequired_400:<fieldPath>`
- `Grammar_ImportFieldTooLong_400:<fieldPath>`
- `Grammar_ImportFieldInvalid_400:<fieldPath>`
- `Grammar_ImportRelatedGrammarNotFound_404:<fieldPath>`
- `Grammar_ImportDuplicateRelation_400:<fieldPath>`
- `Grammar_ImportSentenceIdNotAllowed_400:<fieldPath>`

### Request body (`POST`/`PATCH`)

```json
{
  "title": "〜てから",
  "summary": "Diễn tả hành động B xảy ra sau khi làm A",
  "level": "N5",
  "tags": ["grammar", "sequence"],
  "status": "Draft",
  "structures": [
    {
      "pattern": "V1(1) + ながら + V2(2)",
      "annotations": {
        "1": "Hành động phụ, có thể nhấn bằng {green}màu{/green}",
        "2": "{u}Hành động chính{/u}"
      }
    },
    {
      "pattern": "**V[て形]** + から"
    }
  ],
  "explanation": "Dùng khi hành động sau xảy ra sau khi hành động trước hoàn tất. Có thể nhấn mạnh bằng **bold**.",
  "caution": "~~Không~~ dùng cho hai hành động đồng thời.",
  "register": "Standard",
  "alternateForms": ["〜てからです"],
  "relations": [
    { "relatedId": "grammar-card-id-1", "relationType": "Similar" },
    { "relatedId": "grammar-card-id-2", "relationType": "Contrasting" }
  ],
  "resources": [
    { "title": "Bài giảng", "url": "https://example.com/te-kara" }
  ],
  "sentences": [
    {
      "id": "optional-existing-sentence-id",
      "text": "ご飯を食べてから、勉強します。",
      "meaning": "Tôi ăn cơm xong rồi học.",
      "speakerId": 3,
      "level": "N5"
    }
  ]
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

Trả về file `application/json` theo đúng shape import create-only. File mẫu hiện tại gồm 1 item sample, có thể tải về, sửa dữ liệu rồi gọi preview/import sau.

Response file body:

```json
{
  "items": [
    {
      "rowNumber": 1,
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

Tải file `application/json` cùng shape với payload import create-only, để frontend có thể chỉnh sửa và import tạo vocabulary mới hàng loạt.

Query params hỗ trợ:

- `q`
- `level`
- `status`
- `wordType`
- `hasAudio`
- `createdByMe`

Response file body:

```json
{
  "items": [
    {
      "rowNumber": null,
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

Ngoài validation field thông thường, backend còn kiểm tra:

- `writing` không được trùng trong chính batch import
- `writing` không được trùng với vocabulary đã có trong database
- `sentences[*].id` không được gửi trong vocabulary import vì import này là create-only

Các message code quan trọng frontend nên bắt:

- `Vocabulary_ImportDuplicateWritingInBatch_400`
- `Vocabulary_ImportWritingAlreadyExists_400`

Với lỗi field-level còn lại, backend trả theo format:

- `<MessageCode>:<fieldPath>`

Ví dụ:

- `Vocabulary_ImportFieldRequired_400:title`
- `Vocabulary_ImportFieldTooLong_400:summary`
- `Vocabulary_ImportFieldInvalid_400:meanings[0].partOfSpeech`
- `Vocabulary_ImportSentenceIdNotAllowed_400:sentences[0].id`

Request body:

```json
{
  "items": [
    {
      "rowNumber": 1,
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
      "title": "食べる",
      "writing": "食べる",
      "isValid": true,
      "errors": [],
      "warnings": []
    }
  ]
}
```

Ví dụ response khi payload không hợp lệ:

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

### POST `/api/vocabulary/import/commit`

Commit batch import sau khi payload đã hợp lệ. Endpoint này sẽ:

- chạy `preview` nội bộ trước
- nếu còn item invalid thì không ghi DB
- nếu hợp lệ thì xử lý tuần tự từng item
- mỗi item hợp lệ sẽ tạo vocabulary card mới

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

## Cards Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/cards/search` | No | Search đơn giản cho frontend user, gộp Vocabulary + Grammar |

### GET `/api/cards/search`

API search tổng hợp cho frontend user, tái sử dụng cùng logic search của Vocabulary và Grammar.

Query params hỗ trợ:

- `cardType`: `Vocab` hoặc `Grammar` (bỏ trống để tìm cả 2 loại)
- `q`
- `level`
- `page`
- `pageSize`

Rule:

- Backend luôn chỉ trả card có `status = Published`.
- Nếu không truyền `cardType`, kết quả Vocabulary và Grammar được gộp rồi sort theo `updatedAt ?? createdAt` giảm dần.

Response data item:

```json
{
  "id": "string",
  "cardType": "Vocab | Grammar",
  "title": "string",
  "summary": "string",
  "level": "N5 | N4 | N3 | N2 | N1 | null",
  "alternateForms": ["〜てからです"]
}
```
