# 🎨 Hướng Dẫn Tạo UI Stat Allocation - Từng Bước Chi Tiết

## 📋 Mục Lục
1. [Tạo Canvas](#bước-1-tạo-canvas)
2. [Tạo StatPanel](#bước-2-tạo-statpanel)
3. [Tạo Title Text](#bước-3-tạo-title)
4. [Tạo Available Points Text](#bước-4-available-points)
5. [Tạo Stat Rows](#bước-5-tạo-stat-rows)
6. [Gán Script](#bước-6-gán-script)
7. [Kết Nối References](#bước-7-kết-nối)

---

## 🎯 BƯỚC 1: Tạo Canvas

### 1.1. Tạo Canvas

1. Trong **Hierarchy** (cửa sổ bên trái), **Right-click** vào khoảng trống
2. Chọn: **UI → Canvas**
3. Canvas mới xuất hiện trong Hierarchy

### 1.2. Setup Canvas

Click vào **Canvas** vừa tạo, xem **Inspector** (bên phải):

```
Canvas Component:
├─ Render Mode: Screen Space - Overlay ✓
├─ Pixel Perfect: ❌ (không check)
└─ Sort Order: 0

Canvas Scaler Component:
├─ UI Scale Mode: Scale With Screen Size
├─ Reference Resolution: 1920 x 1080
└─ Match: 0.5 (giữa Width và Height)

Graphic Raycaster Component:
└─ Giữ nguyên default
```

**Cách set:**
1. Tìm component **Canvas Scaler** trong Inspector
2. Click dropdown **UI Scale Mode** → Chọn **"Scale With Screen Size"**
3. Đặt **Reference Resolution**:
   - X: 1920
   - Y: 1080

### 1.3. Kiểm Tra EventSystem

- Trong Hierarchy, bạn sẽ thấy **EventSystem** tự động được tạo
- **KHÔNG XÓA** nó (cần cho click button)

---

## 🎨 BƯỚC 2: Tạo StatPanel (Panel Chính)

### 2.1. Tạo Panel

1. **Right-click** vào **Canvas** trong Hierarchy
2. Chọn: **UI → Panel**
3. Đổi tên thành **"StatPanel"**

### 2.2. Setup Rect Transform

Click **StatPanel** → Inspector → **Rect Transform**:

1. Click nút **Anchor Presets** (icon hình vuông góc trên bên trái)
   - Giữ **Alt + Shift** → Click vào **Center** (giữa)
   - Hoặc click Center bình thường

2. Set Position và Size:
   ```
   Pos X: 0
   Pos Y: 0
   Pos Z: 0
   Width: 400
   Height: 550
   ```

### 2.3. Setup Image (Background)

Trong Inspector, tìm **Image Component**:

```
Image:
├─ Source Image: UI-Sprite-Background (hoặc để mặc định)
├─ Color: Click vào màu → Đặt:
│  ├─ R: 30
│  ├─ G: 30
│  ├─ B: 30
│  └─ A: 240 (gần đục)
└─ Material: None
```

**Cách đổi màu:**
1. Click vào ô màu trắng
2. Color Picker mở ra
3. Kéo thanh trượt hoặc nhập giá trị RGB
4. Đặt Alpha (A) = 240 để hơi trong suốt

### 2.4. Thêm Canvas Group

1. Với **StatPanel** đang được chọn
2. Click **Add Component** (nút dưới Inspector)
3. Gõ: **"Canvas Group"**
4. Click chọn **Canvas Group**
5. Set **Alpha = 1**

---

## 📝 BƯỚC 3: Tạo Title Text

### 3.1. Tạo Text

1. **Right-click** vào **StatPanel** trong Hierarchy
2. Chọn: **UI → Text - TextMeshPro**
3. *(Nếu lần đầu dùng TMP, popup hiện lên → Click "Import TMP Essentials")*
4. Đổi tên thành **"TitleText"**

### 3.2. Setup Rect Transform

```
Anchor Preset: Top Center (hàng trên, cột giữa)

Position:
├─ Pos X: 0
├─ Pos Y: -30
└─ Pos Z: 0

Size:
├─ Width: 350
└─ Height: 50
```

**Cách set Anchor:**
1. Click icon Anchor Presets
2. Click ô **Top Center** (giữa hàng trên)

### 3.3. Setup TextMeshPro

Trong Inspector, component **TextMeshProUGUI**:

```
Text Input Box (ô lớn):
└─ Gõ: "CHARACTER STATS"

Main Settings:
├─ Font Asset: LiberationSans SDF ✓ (mặc định)
├─ Font Style: Bold (chọn B)
├─ Font Size: 32
└─ Auto Size: ❌ (không check)

Vertex Color:
└─ Click màu → Chọn Yellow (#FFD700)
   - R: 255
   - G: 215
   - B: 0

Alignment:
└─ Click nút giữa hàng trên: ⬆️ (Center + Top)
```

**Đổi Font Style:**
- Tìm dòng **Font Style**
- Click chữ **B** (Bold) để in đậm

---

## 💎 BƯỚC 4: Tạo Available Points Text

### 4.1. Tạo Text

1. **Right-click** vào **StatPanel**
2. **UI → Text - TextMeshPro**
3. Đổi tên: **"AvailablePointsText"**

### 4.2. Setup Rect Transform

```
Anchor: Top Center

Position:
├─ Pos X: 0
├─ Pos Y: -90
└─ Pos Z: 0

Size:
├─ Width: 350
└─ Height: 40
```

### 4.3. Setup TextMeshPro

```
Text: "Available Points: 0"

Main Settings:
├─ Font Size: 24
└─ Font Style: Normal (không bold)

Vertex Color: White (255, 255, 255)

Alignment: Center + Middle
```

---

## 📊 BƯỚC 5: Tạo Stat Rows (5 Rows)

### 5.1. Tạo HealthRow (Mẫu)

#### A. Tạo Panel cho Row

1. **Right-click** vào **StatPanel**
2. **UI → Panel**
3. Đổi tên: **"HealthRow"**

#### B. Setup Rect Transform

```
Anchor: Top Stretch (hàng trên, kéo ngang)

Position:
├─ Pos X: 0
├─ Pos Y: -140
├─ Pos Z: 0
├─ Left: 20 (margin trái)
└─ Right: 20 (margin phải)

Size:
└─ Height: 50
```

**Cách set Anchor "Top Stretch":**
1. Click Anchor Presets
2. Giữ **Shift + Alt**
3. Click ô **Top Stretch** (hàng trên, cột giữa có mũi tên ngang)

#### C. Setup Image (Background Row)

```
Image:
├─ Color:
│  ├─ R: 50
│  ├─ G: 50
│  ├─ B: 50
│  └─ A: 200
└─ Image Type: Sliced (nếu có)
```

#### D. Thêm Horizontal Layout Group

1. **HealthRow** đang được chọn
2. **Add Component** → Gõ: **"Horizontal Layout Group"**
3. Setup:

```
Horizontal Layout Group:
├─ Padding:
│  ├─ Left: 10
│  ├─ Right: 10
│  ├─ Top: 5
│  └─ Bottom: 5
├─ Spacing: 10
├─ Child Alignment: Middle Left
├─ Control Child Size:
│  ├─ Width: ❌
│  └─ Height: ❌
└─ Child Force Expand:
   ├─ Width: ❌
   └─ Height: ✓
```

### 5.2. Tạo HealthText (Text trong Row)

1. **Right-click** vào **HealthRow**
2. **UI → Text - TextMeshPro**
3. Đổi tên: **"HealthText"**

#### Setup:

```
TextMeshProUGUI:
├─ Text: "Health: 100 (+0)"
├─ Font Size: 20
├─ Vertex Color: White
└─ Alignment: Middle Left (giữa trái)

Layout Element (Add Component):
├─ Min Width: 250
└─ Flexible Width: 1
```

**Thêm Layout Element:**
1. **HealthText** đang chọn
2. **Add Component** → **"Layout Element"**
3. Check ✓ vào **Min Width** → Đặt 250
4. Check ✓ vào **Flexible Width** → Đặt 1

### 5.3. Tạo HealthButton (Nút + trong Row)

1. **Right-click** vào **HealthRow**
2. **UI → Button - TextMeshPro**
3. Đổi tên: **"HealthButton"**

#### Setup Button:

```
Rect Transform:
└─ (Không cần chỉnh, Layout Group tự động)

Image (Background):
├─ Color: Green
│  ├─ R: 76
│  ├─ G: 175
│  └─ B: 80 (#4CAF50)
└─ Image Type: Sliced

Button Component:
├─ Transition: Color Tint ✓
├─ Normal Color: Green (như trên)
├─ Highlighted Color: Lighter Green
│  ├─ R: 102
│  ├─ G: 187
│  └─ B: 106
├─ Pressed Color: Dark Green
│  ├─ R: 56
│  ├─ G: 142
│  └─ B: 60
└─ Disabled Color: Gray
   ├─ R: 100
   ├─ G: 100
   └─ B: 100

Layout Element (Add Component):
├─ Min Width: 50
├─ Min Height: 40
└─ Preferred Width: 50
```

#### Setup Text trong Button:

Expand **HealthButton** trong Hierarchy → Click **Text (TMP)**:

```
Text: "+"

Font Size: 32
Font Style: Bold
Vertex Color: White
Alignment: Center Middle
```

---

### 5.4. Duplicate Row (Tạo 4 Rows Còn Lại)

Giờ bạn đã có HealthRow hoàn chỉnh! Duplicate nó:

1. Click **HealthRow** trong Hierarchy
2. **Ctrl + D** (duplicate) 4 lần
3. Bạn có 5 rows:
   - HealthRow
   - HealthRow (1)
   - HealthRow (2)
   - HealthRow (3)
   - HealthRow (4)

4. **Đổi tên từng row:**
   - HealthRow (1) → **ManaRow**
   - HealthRow (2) → **AttackRow**
   - HealthRow (3) → **DefenseRow**
   - HealthRow (4) → **SpeedRow**

5. **Đổi tên các text và button bên trong:**

**ManaRow:**
- Expand ManaRow → Đổi:
  - HealthText → **ManaText**
  - HealthButton → **ManaButton**
- ManaText → Đổi text thành: **"Mana: 100 (+0)"**
- ManaButton → Đổi màu nền: **Blue (#2196F3)**
  - R: 33, G: 150, B: 243

**AttackRow:**
- AttackText: **"Attack: 10 (+0)"**
- AttackButton màu: **Red (#F44336)**
  - R: 244, G: 67, B: 54

**DefenseRow:**
- DefenseText: **"Defense: 5 (+0)"**
- DefenseButton màu: **Orange (#FF9800)**
  - R: 255, G: 152, B: 0

**SpeedRow:**
- SpeedText: **"Speed: 5.0 (+0)"**
- SpeedButton màu: **Purple (#9C27B0)**
  - R: 156, G: 39, B: 176

### 5.5. Sắp Xếp Rows

Giờ bạn có 5 rows nhưng chúng chồng lên nhau. Sắp xếp:

**Cách 1: Kéo thủ công**

1. Click từng row
2. Chỉnh **Pos Y** trong Rect Transform:
   ```
   HealthRow:  Pos Y = -140
   ManaRow:    Pos Y = -200
   AttackRow:  Pos Y = -260
   DefenseRow: Pos Y = -320
   SpeedRow:   Pos Y = -380
   ```

**Cách 2: Dùng Vertical Layout Group (TỰ ĐỘNG - ĐỀ XUẤT)**

1. Tạo container cho rows:
   - Right-click **StatPanel** → **Create Empty**
   - Đổi tên: **"StatList"**

2. Setup StatList Rect Transform:
   ```
   Anchor: Top Stretch
   Pos Y: -140
   Left: 20
   Right: 20
   Height: 300
   ```

3. Thêm **Vertical Layout Group**:
   - StatList chọn → Add Component → **Vertical Layout Group**
   ```
   Padding: 0 (all)
   Spacing: 10
   Child Alignment: Upper Center
   Child Force Expand: Width ✓, Height ❌
   ```

4. **Kéo cả 5 rows vào trong StatList:**
   - Chọn cả 5 rows (giữ Ctrl click từng cái)
   - Kéo vào **StatList**
   - Chúng tự động xếp đều!

---

## 🔗 BƯỚC 6: Gán Script vào Canvas

### 6.1. Thêm Script

1. Click **Canvas** trong Hierarchy
2. Trong Inspector, click **Add Component**
3. Gõ: **"StatAllocationUI"**
4. Click chọn script

### 6.2. Ẩn StatPanel Ban Đầu

1. Click **StatPanel** trong Hierarchy
2. **Uncheck** ✓ ở góc trên bên trái Inspector (bên cạnh tên)
3. StatPanel sẽ ẩn (sẽ hiện khi nhấn Tab)

---

## 🎯 BƯỚC 7: Kết Nối References

Giờ gán các UI elements vào script!

### 7.1. Click Canvas → Inspector → StatAllocationUI

Bạn sẽ thấy các fields cần gán:

```
UI References:
├─ Stat Panel: [None]
└─ Available Points Text: [None]

Stat Rows:
├─ Health Text: [None]
├─ Health Button: [None]
├─ Mana Text: [None]
├─ Mana Button: [None]
... (và các cái khác)
```

### 7.2. Gán Từng Field

**Cách gán:**
1. Click vào nút **◎** (tròn nhỏ) bên phải field
2. Cửa sổ **Select Object** hiện ra
3. Click chọn object tương ứng
4. HOẶC: Kéo thả trực tiếp từ Hierarchy vào field

**Danh Sách Gán:**

```
UI References:
├─ Stat Panel: Kéo "StatPanel" vào
└─ Available Points Text: Kéo "AvailablePointsText" vào

Stat Rows:
├─ Health Text: Expand HealthRow → Kéo "HealthText" vào
├─ Health Button: Kéo "HealthButton" vào
├─ Mana Text: Expand ManaRow → Kéo "ManaText" vào
├─ Mana Button: Kéo "ManaButton" vào
├─ Attack Text: Kéo "AttackText" vào
├─ Attack Button: Kéo "AttackButton" vào
├─ Defense Text: Kéo "DefenseText" vào
├─ Defense Button: Kéo "DefenseButton" vào
├─ Speed Text: Kéo "SpeedText" vào
└─ Speed Button: Kéo "SpeedButton" vào
```

### 7.3. Kiểm Tra

Sau khi gán xong, **KHÔNG CÓ** field nào còn **[None]**!

---

## 🎮 BƯỚC 8: Test!

### 8.1. Thêm Scripts vào Player

1. Click **Player** trong Hierarchy
2. **Add Component** → **"PlayerStats"**
3. **Add Component** → **"StatPointManager"**

### 8.2. Test Stat Points

1. Play game (nhấn Space hoặc nút ▶️)
2. Chọn **Player** trong Hierarchy (trong Play mode)
3. **Right-click** component **StatPointManager**
4. Chọn **"Add 5 Stat Points"**

### 8.3. Mở Bảng Stat

1. Nhấn **Tab**
2. Bảng stat hiện lên!
3. Click nút **+** để cộng điểm
4. Xem số liệu thay đổi!

---

## 🎨 BONUS: Làm Đẹp Hơn

### Thêm Border cho StatPanel

1. Click **StatPanel**
2. **Add Component** → **"Outline"**
3. Setup:
   ```
   Effect Color: White hoặc Gold
   Effect Distance: X=2, Y=-2
   ```

### Thêm Shadow cho Text

1. Click **TitleText**
2. **Add Component** → **"Shadow"**
3. Setup:
   ```
   Effect Color: Black (0,0,0,150)
   Effect Distance: X=2, Y=-2
   ```

Làm tương tự cho các text khác!

---

## ✅ Checklist Hoàn Thành

Hierarchy Structure:
```
Canvas
├─ StatPanel ✓
│  ├─ TitleText ✓
│  ├─ AvailablePointsText ✓
│  └─ StatList (optional) ✓
│     ├─ HealthRow ✓
│     │  ├─ HealthText ✓
│     │  └─ HealthButton ✓
│     ├─ ManaRow ✓
│     │  ├─ ManaText ✓
│     │  └─ ManaButton ✓
│     ├─ AttackRow ✓
│     ├─ DefenseRow ✓
│     └─ SpeedRow ✓
└─ StatAllocationUI (Script) ✓

Player
├─ PlayerStats ✓
└─ StatPointManager ✓
```

---

## 🎯 Xong Rồi!

Bây giờ bạn có:
- ✅ Bảng stat đẹp
- ✅ Nhấn Tab để mở
- ✅ Click + để cộng điểm
- ✅ Hoàn toàn functional!

**Nếu có lỗi, check:**
- EventSystem có trong scene
- StatPanel có Canvas Group
- Tất cả references đã gán
- Player có cả 2 scripts

Chúc bạn thành công! 🎮✨

