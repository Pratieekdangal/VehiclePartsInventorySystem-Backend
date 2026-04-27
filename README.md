# Vehicle Parts System — Backend

ASP.NET Core 9 Web API for the **Vehicle Parts System** (VPS) — a vehicle parts retail and service-center management platform for a Kathmandu-based garage.

CS6004NT Application Development · Coursework 2 · London Metropolitan University / Itahari International College.

---

## Stack

- **.NET 9** + ASP.NET Core Web API
- **EF Core 9** with SQLite (PostgreSQL-ready — single line swap in `Infrastructure/DependencyInjection.cs`)
- **JWT Bearer** auth · BCrypt password hashing
- **MailKit** SMTP for invoice emails and overdue reminders
- **Background services** for low-stock + overdue-credit auto-notifications
- Clean Architecture: API · Application · Domain · Infrastructure

---

## Group task division (CS6004NT 2025/26)

| Member | Features |
|---|---|
| **Pratik Dangal** (Leader) | F1, F4, F6, F15, F16 |
| Ribesh Raut | F3, F5, F7, F11 |
| Prajwal Niroula | F8, F9, F10 |
| Shreya Basnet | F2, F12, F13, F14 |

See git log for per-feature commit attribution.

---

## Running locally

```bash
dotnet restore
dotnet ef database update --project src/VehiclePartsSystem.Infrastructure --startup-project src/VehiclePartsSystem.API
dotnet run --project src/VehiclePartsSystem.API --urls=http://localhost:5000
```

Swagger UI: http://localhost:5000/swagger

Seeded admin (created on first boot): `admin@vps.local` / `Admin@123`
