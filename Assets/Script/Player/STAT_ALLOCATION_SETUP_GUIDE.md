# 🎯 Hướng Dẫn Setup Hệ Thống Stat Allocation

## 📋 Tổng Quan
Hệ thống này cho phép:
- ✅ Mỗi lần lên cấp nhận 1 stat point
- ✅ Cộng điểm vào HP, Mana, Attack, Defense, Speed
- ✅ Mở bảng stat bằng phím **Tab**
- ✅ UI đẹp với animation fade in/out

---

## 🛠️ BƯỚC 1: Setup Scripts trên Player

### 1.1. Thêm Scripts vào Player GameObject

Mở Player trong Scene, thêm 3 components:
1. **PlayerStats** (Script)
2. **StatPointManager** (Script)

### 1.2. Cấu hình PlayerStats

```
Base Stats:
├─ Base Max Health: 100
├─ Base Max Mana: 100
├─ Base Attack: 10
├─ Base Defense: 5
└─ Base Speed: 5

Stat Points Per Level:
├─ Health Per Point: 20 (mỗi điểm +20 HP)
├─ Mana Per Point: 15 (mỗi điểm +15 Mana)
├─ Attack Per Point: 5 (mỗi điểm +5 Attack)
├─ Defense Per Point: 3 (mỗi điểm +3 Defense)
└─ Speed Per Point: 0.2 (mỗi điểm +0.2 Speed)
```

### 1.3. Cấu hình StatPointManager

```
Settings:
└─ Stat Points Per Level: 1 (mỗi level nhận 1 điểm)
```

**Lưu ý:** Script này tự động đăng ký với PlayerExperience!

---

## 🎨 BƯỚC 2: Tạo UI Canvas cho Stat Panel

### 2.1. Tạo Canvas

```
Hierarchy:
Canvas (UI)
└─ StatPanel (Panel)
   ├─ Title (Text) - "CHARACTER STATS"
   ├─ AvailablePointsText (Text) - "Available Points: 0"
   ├─ StatList (Vertical Layout Group)
   │  ├─ HealthRow (Horizontal Layout Group)
   │  │  ├─ HealthText (Text) - "Health: 100 (+0)"
   │  │  └─ HealthButton (Button) - "+"
   │  ├─ ManaRow
   │  │  ├─ ManaText
   │  │  └─ ManaButton
   │  ├─ AttackRow
   │  │  ├─ AttackText
   │  │  └─ AttackButton
   │  ├─ DefenseRow
   │  │  ├─ DefenseText
   │  │  └─ DefenseButton
   │  └─ SpeedRow
   │     ├─ SpeedText
   │     └─ SpeedButton
   └─ CloseButton (Button) - "Close (ESC)"
```

---

## 📐 BƯỚC 3: Setup UI Layout Chi Tiết

### 3.1. StatPanel (Panel)

```
Rect Transform:
├─ Anchor: Center
├─ Width: 400
├─ Height: 500
└─ Pos: (0, 0)

Image:
├─ Color: Dark gray (0.2, 0.2, 0.2, 0.95)
└─ Material: None

Canvas Group (Add Component):
└─ Alpha: 1
```

### 3.2. Title Text

```
TextMeshProUGUI:
├─ Text: "CHARACTER STATS"
├─ Font Size: 32
├─ Alignment: Center Top
├─ Color: Yellow (#FFD700)

Rect Transform:
├─ Top: -20
├─ Width: 360
└─ Height: 40
```

### 3.3. AvailablePointsText

```
TextMeshProUGUI:
├─ Text: "Available Points: 0"
├─ Font Size: 24
├─ Alignment: Center
├─ Color: White

Rect Transform:
├─ Top: -80
├─ Width: 360
└─ Height: 30
```

### 3.4. StatList (Vertical Layout Group)

```
Rect Transform:
├─ Top: -120
├─ Bottom: 60
├─ Width: 360

Vertical Layout Group:
├─ Spacing: 10
├─ Child Alignment: Upper Center
├─ Child Force Expand: Width ✓
└─ Child Control Size: Height ✓
```

### 3.5. HealthRow (và các row khác tương tự)

```
Horizontal Layout Group:
├─ Spacing: 10
├─ Child Force Expand: Width ✓
├─ Padding: 10 (all sides)

Layout Element:
└─ Preferred Height: 50

Image (Background):
└─ Color: (0.3, 0.3, 0.3, 0.8)
```

#### HealthText:
```
TextMeshProUGUI:
├─ Text: "Health: 100 (+0)"
├─ Font Size: 20
├─ Alignment: Left Center
└─ Color: White

Layout Element:
└─ Flexible Width: 1
```

#### HealthButton:
```
Button:
├─ Normal Color: Green (#4CAF50)
├─ Highlighted: Lighter Green
├─ Pressed: Darker Green
└─ Disabled: Gray

Text (Child):
├─ Text: "+"
├─ Font Size: 28
├─ Alignment: Center
└─ Color: White

Layout Element:
├─ Preferred Width: 50
└─ Preferred Height: 50
```

### 3.6. Duplicate Rows

Duplicate **HealthRow** 4 lần và đổi tên:
- ManaRow → ManaText, ManaButton
- AttackRow → AttackText, AttackButton
- DefenseRow → DefenseText, DefenseButton
- SpeedRow → SpeedText, SpeedButton

**Đổi màu button cho dễ phân biệt:**
- Health: Green (#4CAF50)
- Mana: Blue (#2196F3)
- Attack: Red (#F44336)
- Defense: Orange (#FF9800)
- Speed: Purple (#9C27B0)

---

## 🔗 BƯỚC 4: Gán Script StatAllocationUI

### 4.1. Add Script

1. Chọn **Canvas** GameObject (hoặc tạo object mới)
2. Add Component → **StatAllocationUI**

### 4.2. Gán References

```
UI References:
├─ Stat Panel: [Kéo StatPanel vào]
└─ Available Points Text: [Kéo AvailablePointsText vào]

Stat Rows:
├─ Health Text: [Kéo HealthText vào]
├─ Health Button: [Kéo HealthButton vào]
├─ Mana Text: [Kéo ManaText vào]
├─ Mana Button: [Kéo ManaButton vào]
├─ Attack Text: [Kéo AttackText vào]
├─ Attack Button: [Kéo AttackButton vào]
├─ Defense Text: [Kéo DefenseText vào]
├─ Defense Button: [Kéo DefenseButton vào]
├─ Speed Text: [Kéo SpeedText vào]
└─ Speed Button: [Kéo SpeedButton vào]

Animation:
├─ Fade Speed: 5
└─ Use Scale Animation: ✓
```

---

## 🎮 BƯỚC 5: Test Hệ Thống

### 5.1. Test Stat Points

1. Chạy game
2. Chọn **Player** trong Hierarchy
3. Right-click **StatPointManager**
4. Chọn **"Add 5 Stat Points"**
5. Nhấn **Tab** → Bảng stat mở ra
6. Click nút **+** để cộng điểm
7. Xem stats tăng!

### 5.2. Test Level Up

1. Giết enemy để lên level
2. Nhận 1 stat point
3. Nhấn Tab
4. Cộng điểm vào stat bạn muốn!

---

## ⚙️ BƯỚC 6: Tùy Chỉnh (Optional)

### 6.1. Pause Game Khi Mở Stat Panel

Trong `StatAllocationUI.cs`, uncomment:

```csharp
// Dòng 98
Time.timeScale = 0; // Pause game

// Dòng 111
Time.timeScale = 1; // Unpause
```

### 6.2. Thay Đổi Phím Mở

Trong `StatAllocationUI.cs`, dòng 71:

```csharp
// Thay Tab thành phím khác
if (Input.GetKeyDown(KeyCode.Tab)) // Đổi thành KeyCode.I, KeyCode.C, etc.
```

### 6.3. Thay Đổi Số Điểm Mỗi Level

Player → StatPointManager:
```
Stat Points Per Level: 1 → Đổi thành 2, 3, 5...
```

### 6.4. Thay Đổi Bonus Mỗi Điểm

Player → PlayerStats:
```
Health Per Point: 20 → Đổi thành 30, 50...
Attack Per Point: 5 → Đổi thành 10, 15...
```

---

## 🎨 BƯỚC 7: Làm Đẹp UI (Advanced)

### 7.1. Thêm Background Image

Import sprite background đẹp → Set vào StatPanel

### 7.2. Thêm Icons

- Health: ❤️ Heart icon
- Mana: 💧 Droplet icon
- Attack: ⚔️ Sword icon
- Defense: 🛡️ Shield icon
- Speed: ⚡ Lightning icon

Thêm Image component vào mỗi row, set icon tương ứng.

### 7.3. Thêm Sound Effects

```csharp
// Trong StatAllocationUI.cs, thêm:
[SerializeField] private AudioClip statUpSound;
private AudioSource audioSource;

// Trong OnStatButtonClicked():
if (audioSource != null && statUpSound != null)
{
    audioSource.PlayOneShot(statUpSound);
}
```

---

## 🐛 Troubleshooting

### Bảng stat không mở khi nhấn Tab?

- Kiểm tra **StatAllocationUI** đã được attach vào Canvas
- Kiểm tra **Stat Panel** đã được gán vào field
- Xem Console có lỗi không

### Button không click được?

- Kiểm tra **Canvas** có **Graphic Raycaster** component
- Kiểm tra **EventSystem** có trong scene không
- Kiểm tra button có **Image** component

### Stat không tăng khi click?

- Kiểm tra **Player** có **PlayerStats** component
- Kiểm tra **StatPointManager** đã gán đúng vào Player
- Xem Console logs

### Không nhận stat point khi lên cấp?

- Kiểm tra **StatPointManager** đã attach vào Player
- Kiểm tra script đã đăng ký event với PlayerExperience
- Check Console có log "Level Up! Received X stat point(s)"

---

## 📊 Công Thức Tính Toán

### Stat Calculation:

```
Final Stat = Base Stat + (Allocated Points × Points Per Stat)

Ví dụ:
- Base Health: 100
- Health Per Point: 20
- Allocated: 5 points
→ Final Health = 100 + (5 × 20) = 200 HP
```

### Level Up Progression:

```
Total Stat Points = (Current Level - 1) × Points Per Level

Ví dụ với Points Per Level = 1:
- Level 1: 0 points
- Level 5: 4 points
- Level 10: 9 points
- Level 20: 19 points
```

---

## ✅ Checklist

Setup:
- [ ] Player có PlayerStats component
- [ ] Player có StatPointManager component
- [ ] Canvas có StatAllocationUI component
- [ ] UI đã được tạo và gán đầy đủ

Test:
- [ ] Nhấn Tab → Bảng stat mở
- [ ] Nhấn ESC → Bảng stat đóng
- [ ] Click + button → Stat tăng
- [ ] Level up → Nhận stat point
- [ ] UI hiển thị đúng số liệu

---

## 🎯 Hoàn Thành!

Bây giờ game của bạn đã có:
- ✨ Hệ thống stat allocation đầy đủ
- 🎮 Nhấn Tab để mở bảng stat
- 📈 Tự do cộng điểm vào thuộc tính
- 🎊 Nhận điểm mỗi khi lên cấp

Chúc bạn code game vui vẻ! 🚀✨

