# API Change Summary — Voicevox → Azure TTS Migration

> Gửi cho AI agent ở frontend admin. Backend đã hoàn toàn loại bỏ Voicevox, thay bằng Azure TTS + Cloudinary.

---

## 1. Tổng quan thay đổi

### Đã xóa
- Module `/api/voicevox/*` — toàn bộ endpoints Voicevox đã xóa.
- `speakerId` trên mọi DTO/response/request liên quan.
- File local audio cache (`/audio-cache/`) — không còn dùng.

### Thay đổi audio generation
- Backend tự động gọi Azure TTS → upload Cloudinary → trả về `audioUrl`.
- **Client không cần gửi `audioUrl`**, backend tự xử lý.
- `pitchPattern` giờ là **input thủ công của admin** (không còn auto-extract từ Voicevox).

### Thay đổi database
- Cột `SpeakerId` trên bảng `sentences` và `vocabulary_details` đã bị xóa.
- Cần chạy migration `dotnet ef database update`.

---

## 2. DTOs — Trường bị xóa

### Vocabulary Cards

| DTO | Trường bị xóa |
|-----|--------------|
| `CreateVocabularyCardRequest` | `speakerId` |
| `UpdateVocabularyCardRequest` | `speakerId` |
| `ImportVocabularyItemRequest` | `speakerId` |
| `VocabularyDetailResponse` | `speakerId` |
| `VocabularySentenceUpsertRequest` | `speakerId` |

**Request body mẫu — POST/PATCH `/api/vocabulary`:**
```json
{
  "title": "食べる",
  "summary": "Động từ ăn",
  "writing": "食べる",
  "reading": "たべる",
  "pitchPattern": [0, 1, 0],    // ✅ Thủ công — admin nhập tay
  "level": "N5",
  "status": "Published",
  "wordType": "Native",
  "meanings": [{ "partOfSpeech": "VerbRu", "definitions": ["ăn"] }],
  "sentences": [
    {
      "text": "毎朝パンを食べる。",
      "meaning": "Mỗi sáng ăn bánh mì。",
      "level": "N5",
      "position": 1
      // ❌ speakerId — đã bỏ
    }
  ]
}
```

### Sentences

| DTO | Trường bị xóa |
|-----|--------------|
| `CreateSentenceRequest` | `speakerId` |
| `UpdateSentenceRequest` | `speakerId` |
| `ImportSentenceItemRequest` | `speakerId` |
| `SentenceResponse` | `speakerId` |

**Request body mẫu — POST/PATCH `/api/sentences`:**
```json
{
  "text": "日本へ行きたいです。",
  "meaning": "Tôi muốn đi Nhật。",
  "level": "N5"
  // ❌ speakerId — đã bỏ
}
```

### Grammar

| DTO | Trường bị xóa |
|-----|--------------|
| `GrammarSentenceUpsertRequest` | `speakerId` |
| `GrammarSentenceResponse` | `speakerId` |

---

## 3. Import Templates — Đã cập nhật

Template JSON không còn chứa `speakerId`:

**Vocabulary import preview/request:**
```json
{
  "items": [
    {
      "rowNumber": 1,
      "title": "食べる",
      "summary": "Động từ ăn",
      "writing": "食べる",
      "reading": "たべる",
      "pitchPattern": [0, 1, 0],
      // ❌ speakerId — bỏ
      "meanings": [{ "partOfSpeech": "VerbRu", "definitions": ["ăn"] }],
      "sentences": [
        {
          "text": "毎朝パンを食べる。",
          "meaning": "Mỗi sáng ăn bánh mì。",
          "level": "N5",
          "position": 1
          // ❌ speakerId — bỏ
        }
      ]
    }
  ]
}
```

**Sentence import preview/request:**
```json
{
  "items": [
    {
      "rowNumber": 1,
      "text": "日本へ行きたいです。",
      "meaning": "Tôi muốn đi Nhật。",
      "level": "N5"
      // ❌ speakerId — bỏ
    }
  ]
}
```

---

## 4. Endpoints bị xóa

| Method | Endpoint | Hành động |
|--------|----------|----------|
| GET | `/api/voicevox/speakers` | **Xóa toàn bộ** — không còn speaker list |
| POST | `/api/voicevox/preview` | **Xóa toàn bộ** — không còn preview |

Frontend admin cần xóa code gọi 2 endpoints trên.

---

## 5. Validation thay đổi

Các validators sau đã bỏ quy tắc `speakerId`:

- `CreateSentenceRequestValidator`
- `UpdateSentenceRequestValidator`
- `ImportSentenceItemRequestValidator`
- `CreateVocabularyCardRequestValidator`
- `UpdateVocabularyCardRequestValidator`
- `VocabularySentenceUpsertRequestValidator`
- `GrammarSentenceUpsertRequestValidator`

Frontend không cần thay đổi gì về validation — backend tự validate.

---

## 6. Điểm cần cập nhật ở Frontend Admin

### Xóa
- [ ] Gỡ UI chọn speaker cho vocabulary/sentence/grammar.
- [ ] Xóa code gọi `GET /api/voicevox/speakers`.
- [ ] Xóa code gọi `POST /api/voicevox/preview`.
- [ ] Xóa state `selectedSpeakerId` / `speakerId` trong các form vocabulary, sentence, grammar.
- [ ] Bỏ `speakerId` khỏi các form schemas (Zod validation).
- [ ] Bỏ `speakerId` khỏi request payload khi gửi lên backend.

### Giữ nguyên
- [ ] Gửi `pitchPattern` — giờ là input thủ công, admin nhập tay (mảng `int[]`).
- [ ] `audioUrl` trong response — vẫn có, nhưng do backend tự generate, frontend chỉ hiển thị.

### Validation
- [ ] Nếu frontend validate `speakerId` bắt buộc hoặc có trong schema → bỏ.

---

## 7. Chạy migration

Backend cần chạy migration trước khi deploy:

```bash
cd learning/
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --context AppDbContext
```

Migration: `RemoveVoicevoxSpeakerIdColumns` — xóa cột `SpeakerId` trên `sentences` và `vocabulary_details`.