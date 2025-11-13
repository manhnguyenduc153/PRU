# ⚡ Thiết Lập Nhanh - Thanh Kinh Nghiệm (Experience Bar)

## 🎯 Các File Đã Tạo

### Scripts Chính:
- ✅ `PlayerExperience.cs` - Quản lý level & XP
- ✅ `ExperienceBarUI.cs` - Hiển thị thanh XP
- ✅ `LevelUpEffect.cs` - Hiệu ứng lên cấp
- ✅ `FloatingLevelUpText.cs` - Text bay lên "LEVEL UP!"

### Đã Cập Nhật:
- ✅ `EnemyHealth.cs` - Tự động thưởng XP khi chết
- ✅ `SaveSystem.cs` - Lưu/Load level & XP

---

## 🚀 Setup 3 Bước Nhanh

### 1️⃣ THÊM VÀO PLAYER
```
Player GameObject:
├─ PlayerExperience (Script) ⭐ MỚI
├─ LevelUpEffect (Script) ⭐ MỚI
└─ AudioSource (Component)
```

### 2️⃣ TẠO UI CANVAS
```
Canvas:
└─ ExperienceBar (Panel)
   ├─ Background (Image)
   ├─ Fill (Image) ⚠️ Image Type = Filled
   ├─ LevelText (TextMeshPro)
   ├─ ExpText (TextMeshPro)
   └─ ExperienceBarUI (Script) ⭐ MỚI
```

### 3️⃣ GÁN REFERENCES
- **ExperienceBarUI:** Gán Fill, LevelText, ExpText
- **LevelUpEffect:** Gán VFX Prefab, Text Prefab (tuỳ chọn)

---

## ⚙️ Cài Đặt Nhanh

### PlayerExperience Settings:
```
Base Experience Required: 100
Experience Multiplier: 1.5
Health Increase Per Level: 50
Mana Increase Per Level: 20
```

### Enemy Settings:
Mỗi enemy đã tự động có field `Experience Reward`:
- Enemy nhỏ: **10-20 XP**
- Enemy vừa: **25-40 XP**
- Enemy lớn: **50-80 XP**
- Boss: **100+ XP** (tự động x5 = 500 XP)

---

## 🎮 Test Ngay

1. **Chạy game** (Play Mode)
2. **Chọn Player** trong Hierarchy
3. **Right-click** component `PlayerExperience`
4. Chọn **"Add 50 XP"** hoặc **"Add 500 XP"**
5. Xem hiệu ứng lên cấp! 🎉

---

## 📊 Công Thức XP

```
Level 1→2: 100 XP
Level 2→3: 150 XP
Level 3→4: 225 XP
Level 4→5: 337 XP
Level 5→6: 506 XP
```

---

## ✨ Hiệu Ứng Lên Cấp Bao Gồm:

- ⚡ Flash màu vàng (3 lần)
- 📈 Scale pulse (phóng to/thu nhỏ)
- 📷 Camera shake
- 💫 Particles VFX (nếu có)
- 🔊 Âm thanh (nếu có)
- 💬 Text "LEVEL UP!" bay lên
- ❤️ Hồi đầy máu & mana
- 📊 Tăng max HP & mana

---

## 🎨 Prefabs Cần Tạo (Tuỳ Chọn)

### 1. Level Up VFX Prefab:
- Particle System với màu vàng/gold
- Duration: 1-2 giây
- Shape: Sphere/Circle

### 2. Level Up Text Prefab:
- TextMeshPro object
- Text: "LEVEL UP!"
- Font size: 6-8
- Color: Yellow với Outline
- Thêm script `FloatingLevelUpText`

---

## 🐛 Troubleshooting

| Vấn Đề | Giải Pháp |
|--------|-----------|
| Thanh XP không hiển thị | Kiểm tra Fill Image có Image Type = Filled |
| Không nhận XP | Kiểm tra PlayerExperience đã attach vào Player |
| Hiệu ứng không chạy | Kiểm tra LevelUpEffect đã attach vào Player |
| Text không bay lên | Kiểm tra Sorting Layer & prefab gán đúng |

---

## 💾 Save System

✅ **Đã tích hợp sẵn!**

Level & XP sẽ tự động lưu/load khi dùng:
```csharp
SaveSystem.Instance.SaveGame(); // Tự động lưu level & XP
SaveSystem.Instance.LoadGame(); // Tự động load level & XP
```

---

## 📖 Hướng Dẫn Chi Tiết

Xem file `EXPERIENCE_SETUP_GUIDE.md` để có hướng dẫn đầy đủ!

---

## 🎯 Checklist Setup

- [ ] Thêm PlayerExperience vào Player
- [ ] Thêm LevelUpEffect vào Player
- [ ] Tạo UI Canvas với ExperienceBar
- [ ] Gán Fill Image (Image Type = Filled!)
- [ ] Gán references vào ExperienceBarUI
- [ ] Test bằng "Add 50 XP" context menu
- [ ] Thử giết enemy để nhận XP
- [ ] Kiểm tra hiệu ứng lên cấp
- [ ] Tạo VFX prefab (tuỳ chọn)
- [ ] Tạo Text prefab (tuỳ chọn)

---

✅ **Hoàn tất! Game của bạn đã có hệ thống kinh nghiệm!** 🎮✨

