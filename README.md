﻿# 🎧 AudioGo - Ứng dụng Thuyết minh Du lịch Đa ngôn ngữ
## Phố Ẩm Thực Vĩnh Khánh, TP.HCM

**Công nghệ:** .NET MAUI (C#) | **Target:** Android, iOS, Windows

---

## 📋 Mục lục
- [Tổng quan](#1-tổng-quan-hệ-thống)
- [Chức năng](#2-chức-năng-trọng-tâm-poc)
- [Kiến trúc](#3-kiến-trúc-kỹ-thuật)
- [Cấu trúc dự án](#4-cấu-trúc-dự-án-mvvm)
- [Cài đặt](#5-hướng-dẫn-cài-đặt)
- [Changelog](#6-changelog)

---

## 1. TỔNG QUAN HỆ THỐNG

### 1.1. Các tác nhân (Actors)
| Actor | Platform | Mô tả |
|-------|----------|-------|
| **Du khách** | Mobile App | Sử dụng app nghe thuyết minh tự động |
| **Quản lý quán** | Web | Quản lý thông tin quán (Phase 2) |
| **Admin** | Web | Quản trị hệ thống (Phase 2) |

### 1.2. Kiến trúc Server - Client
| Mode | Database | Yêu cầu | Trạng thái |
|------|----------|---------|------------|
| **Offline (PoC)** | SQLite | Không cần Wifi | ✅ Đang phát triển |
| **Online** | SQL Server | Cần Wifi đồng bộ | 🔜 Phase 2 |

---

## 2. CHỨC NĂNG TRỌNG TÂM (PoC)

### 2.1. 📍 Định vị & Geofence (Core Feature)
- **GPS Tracking:** Realtime, hỗ trợ background service
- **Geofencing Engine:**
  - Xác định POI: Tọa độ (Lat/Lng), Bán kính, Mức ưu tiên
  - Trigger: Khi đi vào vùng hoặc đến gần điểm
  - **Anti-Spam:** Debounce/Cooldown 5 phút

### 2.2. 🎙️ Thuyết minh tự động (Narration Engine)
- **Định dạng:** Text-to-Speech (TTS) + Audio file
- **Logic phát:**
  - Queue management (xử lý xung đột)
  - Priority-based playback
  - Auto stop/skip khi di chuyển

### 2.3. 💾 Quản lý dữ liệu (Offline)
- SQLite local storage
- 7 POI mẫu Phố Vĩnh Khánh (seed data)
- Hỗ trợ đa ngôn ngữ: VI, EN, ZH, KO, JA

---

## 3. KIẾN TRÚC KỸ THUẬT

### 3.1. Tech Stack
| Component | Technology |
|-----------|------------|
| Framework | .NET MAUI (.NET 10) |
| Pattern | MVVM |
| Database | SQLite (sqlite-net-pcl) |
| Maps | Microsoft.Maui.Controls.Maps |
| Audio | Plugin.Maui.Audio |
| MVVM Toolkit | CommunityToolkit.Mvvm |

### 3.2. NuGet Packages
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="Microsoft.Maui.Controls.Maps" Version="10.0.30" />
<PackageReference Include="Plugin.Maui.Audio" Version="4.0.0" />
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.11" />
```

### 3.3. Quy trình xử lý (Flow)
```
[App Start] → [Load POI từ SQLite] → [Start GPS Tracking]
                                            ↓
[User di chuyển] → [Tính khoảng cách] → [Check Geofence]
                                            ↓
[Vào vùng POI] → [Thêm vào Queue] → [Phát Audio/TTS]
```

---