# 🎧 AudioGo — Hệ thống Thuyết minh Du lịch Đa ngôn ngữ

> **Proof of Concept (PoC)** — Phố Ẩm Thực Vĩnh Khánh, TP.HCM  
> Tự động phát thuyết minh khi du khách di chuyển đến gần điểm tham quan (POI).

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![MAUI](https://img.shields.io/badge/Platform-.NET%20MAUI-blueviolet?logo=dotnet)](https://learn.microsoft.com/dotnet/maui)
[![ASP.NET Core](https://img.shields.io/badge/API-ASP.NET%20Core-blue?logo=dotnet)](https://learn.microsoft.com/aspnet/core)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## 📁 Cấu trúc Repository

```
AudioGo.sln
│
├── Client/          ← 📱 [Mobile Dev] App di động (.NET MAUI — Android + iOS)
│   ├── Models/          # PoiEntity (SQLite local entity)
│   ├── ViewModels/      # BaseViewModel (MVVM / INotifyPropertyChanged)
│   ├── Views/           # XAML Pages
│   ├── Services/        # GeofenceService + Interfaces/IGeofenceService
│   ├── Helpers/         # GeoHelper (Haversine distance)
│   ├── Converters/      # XAML Value Converters
│   ├── Data/            # AppDatabase (SQLite async CRUD)
│   ├── Platforms/       # Android · iOS · MacCatalyst · Windows
│   └── Resources/       # Fonts · Images · Styles · Splash
│
├── MobileApi/       ← 📡 [Mobile Dev] REST API cho Mobile App (ASP.NET Core)
│   ├── Controllers/     # PoiController — đồng bộ POI xuống app
│   └── Program.cs
│
├── WebApi/          ← 🌐 [Web Dev] REST API cho Web CMS & Admin (ASP.NET Core)
│   ├── Controllers/     # PoiController — CRUD POI từ trang web
│   └── Program.cs
│
├── WebApp/          ← 🖥️ [Web Dev] Web Frontend (ASP.NET Core Razor Pages)
│   └── Pages/
│       ├── Cms.cshtml       # Trang CMS — Quản lý quán nhập/sửa nội dung POI
│       └── Admin.cshtml     # Trang Admin — Quản trị hệ thống, người dùng
│
└── Shared/          ← 📦 [Dùng chung] Models & Contracts — cả 2 thành viên đều dùng
    └── POI.cs           # POI model (Id, Name, Lat/Lon, Radius, Audio…)
```

---

## 👥 Phân công thành viên

| Thành viên | Phụ trách | Projects |
|---|---|---|
| **Mobile Dev** | App di động + API cho mobile | `Client/` + `MobileApi/` |
| **Web Dev** | Web CMS, Admin portal + API cho web | `WebApp/` + `WebApi/` |
| **Cả hai** | Models và DTOs dùng chung | `Shared/` |

---

## 📦 Tại sao cần `Shared/`?

`Shared/` chứa model `POI` (và các DTOs dùng chung trong tương lai) được **cả 4 projects** tham chiếu:

- `Client/` — dùng `POI` để map dữ liệu nhận từ `MobileApi/`
- `MobileApi/` — trả về `POI` cho mobile app
- `WebApi/` — trả về và nhận `POI` từ web frontend
- `WebApp/` — hiển thị và gửi dữ liệu `POI`

➡️ Không có `Shared/`, mỗi project phải tự định nghĩa lại model `POI` riêng → dễ bất đồng bộ dữ liệu và khó bảo trì.

---

## 🏗️ Kiến trúc tổng quan

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Du khách (Mobile App)                           │
│                  .NET MAUI — Client/                                │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────┐                   │
│  │ GPS / Maps  │→ │ GeofenceSvc  │→ │ Audio   │                   │
│  │             │  │ (cooldown    │  │ Engine  │                   │
│  └─────────────┘  │  5 min)      │  │ TTS/File│                   │
│                   └──────────────┘  └─────────┘                   │
│               [SQLite — Offline Mode]                               │
└──────────────────────────┬──────────────────────────────────────────┘
                           │ HTTP (Phase 2 — Online sync)
┌──────────────────────────▼──────────────────────────────────────────┐
│             📡  MobileApi — ASP.NET Core                            │
│        REST API /api/poi — Mobile sync (SQL Server — Phase 2)       │
└──────────────────────────┬──────────────────────────────────────────┘
                           │ Shared Models (POI)
┌──────────────────────────▼──────────────────────────────────────────┐
│                  📦 Shared — Class Library                          │
│               POI · DTOs dùng chung cho cả hệ thống                │
└──────────────────────────┬──────────────────────────────────────────┘
                           │ Shared Models (POI)
┌──────────────────────────▼──────────────────────────────────────────┐
│              🌐  WebApi — ASP.NET Core                              │
│       REST API /api/poi — Web CMS & Admin (SQL Server — Phase 2)    │
└──────────────────────────┬──────────────────────────────────────────┘
                           │ HTTP
┌──────────────────────────▼──────────────────────────────────────────┐
│              🖥️  WebApp — Razor Pages                               │
│   /Cms — Quản lý quán nhập POI  │  /Admin — Quản trị hệ thống      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🚀 Chức năng cốt lõi (PoC)

| Tính năng | Mô tả | Trạng thái |
|---|---|---|
| 📍 Geofencing | Phát hiện khi du khách vào vùng POI (bán kính tuỳ chỉnh) | 🔨 In progress |
| 🔇 Anti-spam | Debounce cooldown 5 phút per POI | ✅ Done |
| 🔢 Priority queue | POI ưu tiên cao được trigger trước khi overlap | ✅ Done |
| 🗣️ TTS narration | Phát thuyết minh qua Text-to-Speech | 🔜 Todo |
| 🎵 Audio file | Phát file audio thu sẵn | 🔜 Todo |
| 📴 Offline mode | Dữ liệu POI lưu SQLite local | 🔨 In progress |
| 🌐 Online sync | Đồng bộ từ MobileApi qua REST API | 🔜 Phase 2 |
| 🖥️ Web CMS | Quản lý quán nhập POI qua WebApp + WebApi | 🔜 Phase 2 |
| 🔑 Web Admin | Quản trị hệ thống qua trang Admin | 🔜 Phase 2 |
| 🗺️ Map view | Hiển thị vị trí + toàn bộ POI | 🔜 Todo |
| 📷 QR Code | Fallback thủ công cho GPS yếu | 🔜 Todo |
| 🌏 Đa ngôn ngữ | VI · EN · ZH · KO · JA | 🔜 Todo |

---

## ⚙️ Yêu cầu môi trường

| Công cụ | Phiên bản |
|---|---|
| .NET SDK | 10.0+ |
| Visual Studio / VS Code | VS 2022 17.x+ / VS Code với C# Dev Kit |
| Android SDK | API 21+ |
| iOS | 15.0+ (cần máy Mac để build) |

---

## 🛠️ Chạy dự án

### MobileApi (API cho mobile app)
```bash
cd MobileApi
dotnet run
# API chạy tại: http://localhost:5086/api/poi
```

### WebApi (API cho web CMS & Admin)
```bash
cd WebApi
dotnet run
# API chạy tại: http://localhost:5026/api/poi
```

### WebApp (Web CMS & Admin portal)
```bash
cd WebApp
dotnet run
# Web chạy tại: http://localhost:5031
# Trang CMS:   http://localhost:5031/Cms
# Trang Admin: http://localhost:5031/Admin
```

### Client (MAUI — Android Emulator)
```bash
cd Client
dotnet build -t:Run -f net10.0-android
```

---

## 📌 Branching strategy

```
main          ← Production-ready releases
develop       ← Integration branch (daily work)
feature/*     ← Feature branches (từ develop)
hotfix/*      ← Hotfix branches (từ main)
```

---

## 👥 Actors

- **Du khách** — tương tác qua **App Mobile** (`Client/` + `MobileApi/`)
- **Quản lý quán** — thao tác qua **Web CMS** (`WebApp/Cms` + `WebApi/`) *(Phase 2)*
- **Admin** — quản trị hệ thống qua **Web Admin** (`WebApp/Admin` + `WebApi/`) *(Phase 2)*

---

## 📄 License

MIT © AudioGo Project Team
