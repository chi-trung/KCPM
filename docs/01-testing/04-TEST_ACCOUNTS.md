# 🔐 Tài khoản Test — Waste Recycling Platform

> **Cập nhật**: 15/06/2026 — ✅ Đã verify trên production
> **Frontend**: https://kcpm.vercel.app
> **Backend API**: https://kcpm-backend.onrender.com
> **Swagger**: https://kcpm-backend.onrender.com/swagger

---

## ⚠️ Lưu ý quan trọng

- Backend deploy trên **Render Free Tier** → server **tự tắt sau 15 phút không dùng**
- Lần đầu truy cập sẽ mất **30-60 giây** để server khởi động (cold start)
- DB trên **Aiven Free Tier** → cũng có thể bị tắt nếu lâu không dùng
- Nếu gặp lỗi, đợi 1-2 phút rồi thử lại

---

## 📋 Tài khoản Seed (Password: `password`)

### 🔴 Admin

| Email | Password | Tên | Status |
|-------|----------|-----|--------|
| `admin@gmail.com` | `password` | System Administrator | ✅ Verified |

**Quyền Admin**: Bảng điều khiển, Quản lý Báo cáo, Quản lý Khiếu nại, Quản lý Doanh nghiệp, Quản lý Người dùng

---

### 🟢 Citizen (Người dân)

| Email | Password | Tên | SĐT | Status |
|-------|----------|-----|-----|--------|
| `nguyenvana@gmail.com` | `password` | Nguyễn Văn A | 0901234561 | ✅ Verified |
| `lethib@gmail.com` | `password` | Lê Thị B | 0901234562 | ✅ Verified |
| `tranvanc@gmail.com` | `password` | Trần Văn C | 0901234563 | ✅ Verified |

**Quyền Citizen**: Bảng điều khiển, Tạo báo cáo (GPS, categories, ảnh), Quản lý báo cáo, Điểm thưởng

---

### 🔵 Enterprise (Doanh nghiệp)

| Email | Password | Tên | Công ty | Status |
|-------|----------|-----|---------|--------|
| `greenlife@gmail.com` | `password` | Green Life CEO | Công ty Tái chế Green Life | ✅ Verified |
| `ecofriendly@gmail.com` | `password` | EcoFriendly Manager | Eco-Friendly Collection | ✅ Verified |

**Quyền Enterprise**: Quản lý thu gom, Quản lý collector, Thống kê doanh nghiệp

---

### 🟠 Collector (Thu gom)

| Email | Password | Tên | Thuộc DN | Status |
|-------|----------|-----|----------|--------|
| `collector1@gmail.com` | `password` | Phạm Minh Dũng | Green Life | ✅ Verified |
| `collector2@gmail.com` | `password` | Lý Đại Nghĩa | Eco-Friendly | ✅ Verified |

**Quyền Collector**: Xem công việc thu gom, Cập nhật trạng thái thu gom

---

## 🧪 Kết quả Test Login (15/06/2026)

```
8/8 accounts login thành công qua API
Response time: 1.5-2s mỗi login
Admin dashboard: ✅ OK (stats, reports, complaints, enterprises, users)
Citizen dashboard: ✅ OK (create report, rewards store, report history)
```

---

## 🔗 API Login

```bash
# Login bất kỳ account
curl -X POST https://kcpm-backend.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@gmail.com","password":"password"}'

# Health check
curl https://kcpm-backend.onrender.com/api/health
```

---

## 🛠️ Xử lý sự cố

### Server không phản hồi / load mãi
1. Render Free Tier tự tắt sau 15 phút → **đợi 30-60s** cho cold start
2. Nếu vẫn lỗi, vào https://dashboard.render.com → Manual Deploy

### Login fail
- Password mặc định là `password` (tất cả accounts)
- Nếu DB bị reset, tài khoản seed tự tạo lại khi app khởi động

### Aiven DB bị tắt
1. Vào https://console.aiven.io
2. Bật lại service `kcpm-mysql`
3. Đợi 2-3 phút cho DB sẵn sàng
4. Manual Deploy lại trên Render
