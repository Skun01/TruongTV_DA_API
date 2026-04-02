# Tacho Learning API - Implemented API Documentation

> **Last updated:** 2026-04-02

## Response Contract

Most business and validation failures are returned in HTTP 200 with this shape:

```json
{
  "code": 200,
  "success": true,
  "message": "optional",
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
- Behavior: avatar cũ (nếu có) sẽ bị xóa trong storage và record cũ trong DB cũng bị xóa.

Response data (AuthUserDTO):

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

## Resources Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/resources/audio` | Yes | Upload audio resource |

### POST `/api/resources/audio`

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
  "fileType": "audio",
  "usageType": "audio",
  "sizeInBytes": 12345,
  "createdAt": "datetime"
}
```

---