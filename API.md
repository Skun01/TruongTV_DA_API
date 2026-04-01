# Tacho Learning API - Implemented API Documentation

> **Last updated:** 2026-04-01  
---

## Auth Module

### Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/auth/register` | ❌ | Đăng ký tài khoản mới |
| POST | `/auth/login` | ❌ | Đăng nhập |
| POST | `/auth/refresh-token` | ❌ | Làm mới access token |
| POST | `/auth/refresh` | ❌ | Alias của refresh-token |
| GET | `/auth/me` | ✅ | Lấy thông tin user hiện tại |
| PATCH | `/auth/me/profile` | ✅ | Cập nhật profile |
| PATCH | `/auth/change-password` | ✅ | Đổi mật khẩu |
| POST | `/auth/logout` | ✅ | Đăng xuất |
| POST | `/auth/forgot-password` | ❌ | Gửi email reset password |
| POST | `/auth/reset-password` | ❌ | Reset password bằng token |

---

### POST `/auth/register`

Đăng ký tài khoản mới.

**Request Body:**
```json
{
  "username": "string | null",
  "displayName": "string | null",
  "email": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "code": 200,
  "message": "Success",
  "data": {
    "accessToken": "string",
    "user": {
      "id": "string",
      "email": "string",
      "displayName": "string",
      "avatarUrl": "string | null",
      "role": "user | editor | admin",
      "createdAt": "datetime"
    }
  }
}
```

**Cookie:** `refreshToken` (HttpOnly)

---

### POST `/auth/login`

Đăng nhập vào hệ thống.

**Request Body:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response:** Giống `/auth/register`

**Cookie:** `refreshToken` (HttpOnly)

---

### POST `/auth/refresh-token`

Làm mới access token khi token cũ hết hạn.

**Request:** Không cần body, đọc `refreshToken` từ cookie.

**Response:** Giống `/auth/register`

**Cookie:** `refreshToken` mới (HttpOnly)

---

### GET `/auth/me`

Lấy thông tin user đang đăng nhập.

**Headers:** `Authorization: Bearer <accessToken>`

**Response:**
```json
{
  "code": 200,
  "message": "Success",
  "data": {
    "id": "string",
    "email": "string",
    "displayName": "string",
    "avatarUrl": "string | null",
    "role": "user | editor | admin",
    "createdAt": "datetime"
  }
}
```

---

### PATCH `/auth/me/profile`

Cập nhật thông tin profile.

**Headers:** `Authorization: Bearer <accessToken>`

**Request Body:**
```json
{
  "displayName": "string",
  "avatarUrl": "string | null"
}
```

**Response:** Giống `/auth/me`

---

### PATCH `/auth/change-password`

Đổi mật khẩu.

**Headers:** `Authorization: Bearer <accessToken>`

**Request Body:**
```json
{
  "currentPassword": "string",
  "newPassword": "string"
}
```

**Response:**
```json
{
  "code": 200,
  "message": "Success",
  "data": true
}
```

---

### POST `/auth/logout`

Đăng xuất, revoke refresh token.

**Headers:** `Authorization: Bearer <accessToken>`

**Request:** Đọc `refreshToken` từ cookie.

**Response:**
```json
{
  "code": 200,
  "message": "Success",
  "data": true
}
```

**Cookie:** Xóa `refreshToken`

---

### POST `/auth/forgot-password`

Gửi email chứa link reset password.

**Request Body:**
```json
{
  "email": "string"
}
```

**Response:**
```json
{
  "code": 200,
  "message": "Success",
  "data": true
}
```

---

### POST `/auth/reset-password`

Reset password bằng token từ email.

**Request Body:**
```json
{
  "token": "string",
  "newPassword": "string"
}
```

**Response:**
```json
{
  "code": 200,
  "message": "Success",

---