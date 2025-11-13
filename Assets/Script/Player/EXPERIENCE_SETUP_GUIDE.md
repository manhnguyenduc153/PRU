# 🎮 Hướng Dẫn Thiết Lập Hệ Thống Kinh Nghiệm (Experience Bar)

## 📋 Tổng Quan
Hệ thống này bao gồm:
- ✅ Thanh kinh nghiệm (XP Bar) với animation mượt mà
- ✅ Hệ thống lên cấp (Level Up) tự động
- ✅ Hiệu ứng lên cấp đầy đủ (VFX, camera shake, flash, text popup)
- ✅ Tự động tăng máu & mana khi lên level
- ✅ Enemy tự động thưởng XP khi bị tiêu diệt

---

## 🛠️ BƯỚC 1: Setup Player GameObject

### 1.1. Thêm Scripts vào Player
Mở Player GameObject trong Scene, thêm 3 components:
1. **PlayerExperience** (Script)
2. **LevelUpEffect** (Script)  
3. **AudioSource** (Component - tự động thêm nếu chưa có)

### 1.2. Cấu hình PlayerExperience
```
Level Settings:
├─ Current Level: 1
├─ Current Experience: 0
├─ Base Experience Required: 100
└─ Experience Multiplier: 1.5

Level Up Rewards:
├─ Health Increase Per Level: 50
└─ Mana Increase Per Level: 20

Audio:
└─ Level Up Sound: [Kéo file âm thanh vào đây]
```

### 1.3. Cấu hình LevelUpEffect
```
VFX Settings:
├─ Level Up VFX Prefab: [Kéo Particle System prefab vào đây]
└─ VFX Offset: (0, 0, 0)

Screen Flash:
├─ Use Screen Flash: ✅
├─ Player Sprite Renderer: [Kéo SpriteRenderer của Player]
├─ Flash Color: Yellow
├─ Flash Duration: 0.3
└─ Flash Count: 3

Scale Pulse:
├─ Use Scale Pulse: ✅
├─ Pulse Scale: 1.2
└─ Pulse Duration: 0.3

Text Popup:
├─ Level Up Text Prefab: [Tạo prefab ở bước 3]
└─ Text Offset: (0, 2, 0)

Camera Shake:
├─ Use Camera Shake: ✅
├─ Shake Intensity: 0.3
└─ Shake Duration: 0.3
```

---

## 🎨 BƯỚC 2: Tạo UI Canvas cho Experience Bar

### 2.1. Tạo Canvas
```
Hierarchy:
Canvas (UI)
└─ ExperienceBar (Panel)
   ├─ Background (Image) - màu tối
   ├─ Fill (Image) - màu xanh/vàng, Image Type = Filled
   ├─ LevelText (TextMeshPro) - "Level 1"
   └─ ExpText (TextMeshPro) - "0/100"
```

### 2.2. Cấu hình Fill Image
**QUAN TRỌNG:**
- Image Type: **Filled**
- Fill Method: **Horizontal**
- Fill Origin: **Left**
- Fill Amount: **0** → **1**

### 2.3. Thêm ExperienceBarUI Script
1. Chọn **ExperienceBar** GameObject
2. Add Component → **ExperienceBarUI**
3. Gán các references:
```
References:
├─ Exp Bar Fill: [Fill Image]
├─ Level Text: [LevelText TMP]
└─ Exp Text: [ExpText TMP]

Animation Settings:
├─ Smooth Speed: 5
└─ Use Animation: ✅

Colors (Optional):
├─ Use Color Gradient: ✅ (tuỳ chọn)
├─ Low Exp Color: Red
└─ High Exp Color: Green
```

---

## 📝 BƯỚC 3: Tạo Floating "LEVEL UP!" Text Prefab

### 3.1. Tạo GameObject mới
```
Hierarchy:
Right-click → Create Empty
└─ LevelUpText
   └─ TextMeshPro Object
```

### 3.2. Cấu hình TextMeshPro
- Text: **"LEVEL UP!"**
- Font Size: **6-8** (cho TextMeshPro 3D)
- Alignment: **Center**
- Color: **Yellow/Gold** với Outline
- Sorting Layer: **Foreground** hoặc cao nhất

### 3.3. Thêm FloatingLevelUpText Script
Add Component → **FloatingLevelUpText**
```
Animation Settings:
├─ Move Speed: 2
├─ Fade Speed: 1
├─ Scale Speed: 2
└─ Max Scale: 1.5
```

### 3.4. Tạo Prefab
Kéo **LevelUpText** từ Hierarchy vào thư mục **Assets/Prefab/** để tạo prefab.

### 3.5. Gán Prefab vào LevelUpEffect
Quay lại **Player → LevelUpEffect**  
→ Kéo prefab **LevelUpText** vào **Level Up Text Prefab**

---

## ⚔️ BƯỚC 4: Cấu Hình Enemy Thưởng XP

Mỗi Enemy prefab đã tự động có khả năng thưởng XP!

### 4.1. Mở Enemy Prefab
Chọn bất kỳ Enemy nào → Inspector → **EnemyHealth**

### 4.2. Cấu hình XP Reward
```
Experience Reward: 10-50 (tùy loại enemy)

Ví dụ:
├─ Enemy nhỏ: 10 XP
├─ Enemy vừa: 25 XP
├─ Enemy lớn: 50 XP
└─ Boss: 100 XP (sẽ x5 = 500 XP)
```

**Lưu ý:** Boss tự động nhận **x5 XP** so với giá trị cài đặt!

---

## 🎮 BƯỚC 5: Test Hệ Thống

### 5.1. Test trong Editor
1. Chạy game (Play Mode)
2. Chọn **Player** trong Hierarchy
3. Right-click trên **PlayerExperience** component
4. Chọn:
   - **Add 50 XP** (test nhỏ)
   - **Add 500 XP** (test lên nhiều level)

### 5.2. Test bằng cách giết Enemy
- Giết enemy → Nhận XP
- Đủ XP → Lên level → Hiệu ứng xuất hiện!

---

## 🎨 BƯỚC 6: Tạo VFX Particles (Tuỳ Chọn)

### 6.1. Tạo Particle System
```
Hierarchy:
Right-click → Effects → Particle System
└─ LevelUpVFX
```

### 6.2. Cấu hình Particles
```
Main:
├─ Duration: 1-2 seconds
├─ Start Lifetime: 0.5-1.5
├─ Start Speed: 3-5
├─ Start Size: 0.3-0.8
└─ Start Color: Yellow → Orange gradient

Emission:
└─ Rate over Time: 50-100

Shape:
├─ Shape: Sphere/Circle
└─ Radius: 1

Color over Lifetime:
└─ Alpha: 1 → 0 (fade out)

Size over Lifetime:
└─ Size: 1 → 0 (shrink)
```

### 6.3. Tạo Prefab & Gán
- Kéo **LevelUpVFX** vào **Assets/Prefab/**
- Gán vào **Player → LevelUpEffect → Level Up VFX Prefab**

---

## 🔊 Âm Thanh (Optional)

### Thêm Level Up Sound:
1. Import file âm thanh (`.mp3`, `.wav`, `.ogg`)
2. Kéo vào **Player → PlayerExperience → Level Up Sound**

---

## 🧪 Testing Checklist

- [ ] Thanh XP hiển thị đúng
- [ ] Giết enemy → nhận XP
- [ ] Thanh XP tăng với animation mượt
- [ ] Đủ XP → Lên level
- [ ] Hiệu ứng flash player
- [ ] Hiệu ứng scale pulse
- [ ] Camera shake
- [ ] Text "LEVEL UP!" xuất hiện và bay lên
- [ ] Particles xuất hiện (nếu có)
- [ ] Âm thanh level up phát (nếu có)
- [ ] Máu tăng sau khi lên level
- [ ] Mana tăng sau khi lên level
- [ ] UI cập nhật đúng level mới

---

## 🐛 Troubleshooting

### Thanh XP không hiển thị?
- Kiểm tra **Canvas** có **Canvas Scaler** chưa
- Kiểm tra **Fill Image** có **Fill Amount = 0-1** chưa
- Đảm bảo **ExperienceBarUI** đã gán đúng references

### Không nhận XP khi giết enemy?
- Kiểm tra **PlayerExperience** có được attach vào Player không
- Kiểm tra **EnemyHealth.DetectDeath()** có được gọi không
- Xem Console có log `"Player gained X XP"` không

### Hiệu ứng lên level không xuất hiện?
- Kiểm tra **LevelUpEffect** đã được attach vào Player
- Kiểm tra prefabs đã được gán vào các fields
- Xem Console có errors không

### Text "LEVEL UP!" không bay lên?
- Kiểm tra TextMeshPro có component **FloatingLevelUpText**
- Kiểm tra Sorting Layer/Order in Layer
- Đảm bảo camera nhìn thấy được vị trí spawn

---

## 📊 Công Thức Tính Toán

### XP Required cho Level tiếp theo:
```
XP = baseXP × (multiplier ^ (level - 1))

Ví dụ (base=100, multiplier=1.5):
Level 1→2: 100 XP
Level 2→3: 150 XP
Level 3→4: 225 XP
Level 4→5: 337 XP
```

### Boss XP Multiplier:
```
Boss XP = experienceReward × 5

Ví dụ:
Normal enemy: 20 XP
Boss (cài 100): 500 XP
```

---

## 🎯 Mở Rộng (Advanced)

### Thêm tính năng tùy chỉnh:
1. **Skill Points:** Thêm biến `skillPoints` trong PlayerExperience, tăng mỗi level
2. **Stats Allocation:** Tạo UI để player chọn tăng HP/Mana/Damage
3. **Level Cap:** Thêm `maxLevel` để giới hạn level tối đa
4. **Prestige System:** Reset level nhưng giữ bonuses
5. **XP Multipliers:** Buff tăng XP nhận được (x2, x3...)

---

## 💾 Save System Integration

Nếu bạn muốn save/load level & XP, thêm vào **SaveSystem.cs**:

```csharp
// Save
PlayerExperience exp = player.GetComponent<PlayerExperience>();
saveData.playerLevel = exp.GetCurrentLevel();
saveData.playerExperience = exp.GetCurrentExperience();

// Load
PlayerExperience exp = player.GetComponent<PlayerExperience>();
exp.SetLevelAndExperience(saveData.playerLevel, saveData.playerExperience);
```

---

## ✅ Hoàn Thành!

Bây giờ game của bạn đã có:
- ✨ Thanh kinh nghiệm đẹp mắt
- 🎊 Hiệu ứng lên cấp đầy đủ
- ⚔️ Enemy tự động thưởng XP
- 📈 Hệ thống progression hoàn chỉnh

Chúc bạn code game vui vẻ! 🎮✨

