# Hướng Dẫn Setup Movement và Double Jump với DOTween

## Tổng Quan

Hệ thống này đã được triển khai với:
- ✅ Movement mượt mà sử dụng DOTween
- ✅ Double jump cho cả 2 players
- ✅ Input System hỗ trợ 2 players
- ✅ Visual feedback khi jump/double jump

---

## Bước 1: Cài Đặt DOTween

### Cách 1: Sử dụng Unity Package Manager (Khuyến nghị)

1. Mở Unity Editor
2. Vào **Window** → **Package Manager**
3. Click vào dấu **+** ở góc trên bên trái
4. Chọn **Add package from git URL...**
5. Nhập URL: `https://github.com/Demigiant/dotween.git?path=/Assets/DOTween`
6. Hoặc sử dụng: `com.demigiant.dotween`
7. Click **Add**

### Cách 2: Import từ Asset Store

1. Mở Unity Asset Store
2. Tìm kiếm "DOTween (HOTween v2)"
3. Click **Add to My Assets** và **Import**
4. Trong cửa sổ import, chọn tất cả và click **Import**

### Setup DOTween sau khi cài đặt

1. Sau khi import, DOTween sẽ tự động hiện cửa sổ setup
2. Nếu không thấy, vào **Tools** → **DOTween Utility Panel**
3. Chọn **Setup DOTween...**
4. Đảm bảo tất cả các modules được chọn:
   - ✅ Default
   - ✅ UI
   - ✅ TextMeshPro
   - ✅ 2D
5. Click **Apply**

---

## Bước 2: Setup Input System

### 2.1. Cài Đặt Input System Package

1. Mở **Window** → **Package Manager**
2. Chọn **Unity Registry** trong dropdown
3. Tìm kiếm "Input System"
4. Click **Install**
5. Khi được hỏi về **Backend**, chọn **Both** (hỗ trợ cả Input Manager cũ và Input System mới)

### 2.2. Tạo Input Action Asset

1. Tạo folder: `Assets/Input/` (nếu chưa có)
2. Click chuột phải trong Project window → **Create** → **Input Actions**
3. Đặt tên: `PlayerInputActions.inputactions`

### 2.3. Cấu Hình Input Actions

Mở file `PlayerInputActions.inputactions` và cấu hình như sau:

#### Action Map: Player1

**LƯU Ý QUAN TRỌNG:** Với platform 2D game, input được xử lý từ **Move** action (Vector2):
- **W hoặc Up Arrow** = Jump (y > 0)
- **S hoặc Down Arrow** = Drop Down (y < 0, chỉ khi đang trên platform)
- **A/D hoặc Left/Right** = Di chuyển ngang (x)

```
Player1/
├── Move (Action Type: Value, Control Type: Vector2)
│   ├── Up: W [Keyboard]
│   ├── Down: S [Keyboard]
│   ├── Left: A [Keyboard]
│   ├── Right: D [Keyboard]
│   └── Gamepad: Left Stick (tùy chọn)
├── Shoot (Action Type: Button)
│   ├── Keyboard: T (hoặc Left Mouse Button)
│   └── Gamepad: Right Trigger
└── Special (Action Type: Button)
    ├── Keyboard: Y (hoặc Left Shift)
    └── Gamepad: Left Shoulder
```

**Lưu ý:** Không cần tạo action **Jump** riêng vì jump được xử lý từ Move.y > 0

#### Action Map: Player2

```
Player2/
├── Move (Action Type: Value, Control Type: Vector2)
│   ├── Up: Up Arrow [Keyboard]
│   ├── Down: Down Arrow [Keyboard]
│   ├── Left: Left Arrow [Keyboard]
│   ├── Right: Right Arrow [Keyboard]
│   └── Gamepad: Left Stick (nếu có gamepad thứ 2)
├── Shoot (Action Type: Button)
│   ├── Keyboard: Right Mouse Button hoặc phím tùy chọn
│   └── Gamepad: Right Trigger
└── Special (Action Type: Button)
    ├── Keyboard: Right Shift hoặc phím tùy chọn
    └── Gamepad: Left Shoulder
```

**Lưu ý:** Tương tự Player1, jump và drop down được xử lý từ Move Vector2 (Up Arrow = jump, Down Arrow = drop down)

#### Action Map: UI

```
UI/
├── Pause (Action Type: Button)
│   └── Keyboard: Escape
└── Menu (Action Type: Button)
    └── Keyboard: Tab
```

**Lưu ý:** Đảm bảo mỗi Action Map có **Player Index** tương ứng:
- Player1: **Player Index = 0** (hoặc None)
- Player2: **Player Index = 1** (nếu sử dụng gamepad)

### 2.4. Generate C# Scripts (Tùy chọn)

1. Trong Input Action Asset, click vào **Generate C# Class**
2. Đặt tên class: `PlayerInputActions`
3. Click **Generate**
4. File sẽ được tạo tại: `Assets/Input/PlayerInputActions.cs`

---

## Bước 3: Setup Player GameObject

### 3.1. Tạo Player GameObject cho Player 1

1. Tạo Empty GameObject: `Player1`
2. Thêm các components:
   - **Rigidbody2D**
     - Body Type: Dynamic
     - Gravity Scale: 3 (hoặc tùy chỉnh)
     - Freeze Rotation: Z ✓
   - **CapsuleCollider2D** (hoặc BoxCollider2D)
     - Size: (1, 2) - tùy chỉnh theo sprite
     - Offset: (0, 0)
   - **BasePlayer** script
   - **PlayerCombat** script (nếu có)
   - **PlayerEffectManager** script (nếu có)
   - **PlayerStateMachine** script (nếu có)

### 3.2. Cấu Hình BasePlayer Component

Trong Inspector của Player1:

**Player Settings:**
- Player ID: `1`
- Move Speed: `5`
- Ground Layer Mask: Chọn layer chứa ground/platforms

**Jump Settings:**
- Jump Force: `10`
- Jump Cooldown: `0.2`

**Double Jump Settings:**
- Enable Double Jump: ✓
- Double Jump Force: `10`

**Triple Jump Settings:**
- Enable Triple Jump: ✗ (mặc định tắt, có thể enable qua item effect)
- Triple Jump Force: `10`

**Platform Drop Settings:**
- Drop Down Time: `0.5` (thời gian ignore platform collision khi drop down)

**Movement Smoothing:**
- Movement Smoothing Duration: `0.2`
- Movement Ease: `OutQuad`

**Component References:**
- Kéo thả các components tương ứng nếu chưa được assign tự động

### 3.3. Tạo Player GameObject cho Player 2

Lặp lại các bước trên với:
- GameObject: `Player2`
- Player ID: `2`
- Các settings tương tự Player1

---

## Bước 4: Setup InputManager

1. Tạo GameObject: `InputManager` (hoặc tìm trong scene)
2. Thêm component: **InputManager** script
3. Trong Inspector:
   - **Input Actions**: Kéo thả file `PlayerInputActions.inputactions` vào đây

### Kiểm Tra InputManager

1. Chạy game
2. Mở **Window** → **Analysis** → **Input Debugger**
3. Test các input để đảm bảo events được trigger đúng

---

## Bước 5: Setup Ground/Platform Layer

### 5.1. Tạo Ground Layer

1. Vào **Edit** → **Project Settings** → **Tags and Layers**
2. Tạo layer mới: `Ground` (hoặc sử dụng layer có sẵn)
3. Gán layer này cho tất cả các GameObject ground/platform

### 5.2. Cấu Hình Collider cho Ground

1. Chọn các GameObject ground/platform
2. Thêm **BoxCollider2D** hoặc **TilemapCollider2D**
3. Đảm bảo Ground Layer Mask trong BasePlayer khớp với layer này

---

## Bước 6: Test và Điều Chỉnh

### 6.1. Test Movement

- **Player 1:**
  - **A/D** để di chuyển ngang (trái/phải)
  - **W** để jump (khi đang trên ground)
  - **W** lần nữa khi đang bay để double jump
  - **W** lần thứ 3 để triple jump (nếu đã enable)
  - **S** khi đang đứng trên platform để drop down qua platform

- **Player 2:**
  - **Left/Right Arrow** để di chuyển ngang (trái/phải)
  - **Up Arrow** để jump (khi đang trên ground)
  - **Up Arrow** lần nữa khi đang bay để double jump
  - **Up Arrow** lần thứ 3 để triple jump (nếu đã enable)
  - **Down Arrow** khi đang đứng trên platform để drop down qua platform

### 6.1.1. Debug Input (Nếu có vấn đề)

1. **Thêm InputDebugger script vào scene:**
   - Tạo Empty GameObject: `InputDebugger`
   - Add Component: `InputDebugger`
   - Enable `Enable Debug` trong Inspector
   - Chạy game và xem Console + On-Screen debug info

2. **Thêm GroundDetectionGizmos script vào Player:**
   - Chọn Player GameObject
   - Add Component: `GroundDetectionGizmos`
   - Enable `Show Gizmos` trong Inspector
   - Xem Scene view để thấy ground detection box (xanh = grounded, đỏ = not grounded)

3. **Kiểm tra Console logs:**
   - `[BasePlayer] Player X jump input detected` - Input được nhận
   - `[BasePlayer] Player X normal jump executed` - Jump được thực hiện
   - `[BasePlayer] Player X ground state changed` - Ground detection thay đổi
   - `[BasePlayer] Player X not grounded` - Debug ground detection
   - `[InputDebugger] Keyboard Input` - Raw keyboard input

4. **Kiểm tra Ground Detection:**
   - Xem Scene view để thấy ground detection box
   - Box màu xanh = player đang grounded
   - Box màu đỏ = player không grounded
   - Kiểm tra box có overlap với ground collider không

### 6.2. Tùy Chỉnh Parameters

Nếu movement/jump quá nhanh/chậm, điều chỉnh trong Inspector:

**Movement:**
- Tăng `Move Speed` để di chuyển nhanh hơn
- Tăng `Movement Smoothing Duration` để movement mượt hơn nhưng chậm hơn

**Jump:**
- Tăng `Jump Force` để jump cao hơn
- Tăng `Jump Cooldown` để tránh spam jump

**Double Jump:**
- Tăng `Double Jump Force` để double jump mạnh hơn

**Triple Jump:**
- Bật `Enable Triple Jump` trong Inspector để test
- Hoặc gọi `player.EnableTripleJump()` từ code/item effect
- Tăng `Triple Jump Force` để triple jump mạnh hơn

**Platform Drop:**
- Tăng `Drop Down Time` nếu player không kịp drop down qua platform

**Visual Feedback:**
- Có thể tùy chỉnh animation trong code:
  - `transform.DOPunchScale()` trong method `Jump()` và `DoubleJump()`

---

## Bước 7: Troubleshooting

### Vấn đề: Player không di chuyển

**Giải pháp:**
1. Kiểm tra InputManager có được assign Input Actions Asset chưa
2. Kiểm tra Player ID trong BasePlayer có đúng không (1 hoặc 2)
3. Kiểm tra Ground Layer Mask có đúng không
4. Kiểm tra console có lỗi gì không

### Vấn đề: Jump/Double Jump không hoạt động

**Giải pháp:**
1. **Kiểm tra Ground Detection** (Vấn đề phổ biến nhất):
   - Add `GroundDetectionGizmos` script vào Player
   - Xem Scene view - box phải màu xanh khi đứng trên ground
   - Kiểm tra Console logs: `ground state changed: false -> true`
   - Đảm bảo ground GameObject có Collider2D và đúng layer
   - Kiểm tra `Ground Layer Mask` trong BasePlayer Inspector

2. **Kiểm tra Input Actions** - **Jump từ W/Up Arrow (Move.y > 0)**, không phải từ action Jump riêng
3. **Kiểm tra Console logs** - Có thấy `jump input detected` không?
4. **Kiểm tra Ground detection** - Có thấy `Grounded: true` trong logs không?
5. **Kiểm tra Jump Cooldown** - Có thấy `jump on cooldown` không?
6. **Kiểm tra Move action** - Phải là **Vector2** với bindings cho cả 4 hướng
7. **Sử dụng InputDebugger** - Add `InputDebugger` script để xem raw input
8. **Kiểm tra Player ID** - Đảm bảo Player ID đúng (1 hoặc 2)

### Vấn đề: Drop down không hoạt động

**Giải pháp:**
1. **Kiểm tra Ground Detection** (Tương tự jump):
   - Add `GroundDetectionGizmos` script vào Player
   - Xem Scene view - box phải màu xanh khi đứng trên platform
   - Kiểm tra Console logs: `drop down input - Grounded: true`

2. **Kiểm tra Platform Setup:**
   - Platform có trong `Ground Layer Mask` không (không cần tag riêng)
   - Platform có BoxCollider2D hoặc CompositeCollider2D không
   - Platform collider KHÔNG phải là trigger
   - Platform có đúng layer được chọn trong Ground Layer Mask

3. **Kiểm tra Input:**
   - Console logs: `drop down input detected` khi nhấn S/Down Arrow
   - Console logs: `drop down input executed` khi thành công

4. **Tùy chỉnh:**
   - Tăng `Drop Down Time` nếu cần
   - Kiểm tra `Drop Down Time` trong Inspector (mặc định 0.5s)

### Vấn đề: Movement quá mượt/phụ thuộc vào DOTween

**Giải pháp:**
1. Giảm `Movement Smoothing Duration` xuống 0.1 hoặc 0.05
2. Hoặc thay đổi `Movement Ease` thành `Linear` hoặc `OutQuad`

### Vấn đề: Input không hoạt động

**Giải pháp:**
1. Kiểm tra Input Actions Asset có được save chưa
2. Kiểm tra InputManager có được enable chưa
3. Kiểm tra Event handlers có được subscribe đúng không (trong OnEnable)

---

## Cấu Trúc File Đã Được Cập Nhật

```
Assets/Scripts/
└── Player/
    ├── BasePlayer.cs  ✅ Đã được cập nhật với:
    │   - Platform 2D movement (chỉ X axis)
    │   - Jump từ W/Up Arrow (Move.y > 0)
    │   - Drop down từ S/Down Arrow (Move.y < 0)
    │   - DOTween movement smoothing
    │   - Double jump logic
    │   - Triple jump system (enable/disable)
    │   - Platform drop down through platforms
    │   - Visual feedback animations
    │   - Debug logging cho troubleshooting
    │   - Ground detection debug
    ├── InputDebugger.cs  ✅ Script debug input:
    │   - Hiển thị raw keyboard/gamepad input
    │   - On-screen debug info
    │   - Console logging
    └── GroundDetectionGizmos.cs  ✅ Script debug ground detection:
        - Hiển thị ground detection box trong Scene view
        - Màu xanh = grounded, màu đỏ = not grounded
        - Debug ground detection parameters
```

## Đặc Điểm Platform 2D Movement

### Input System
- **Jump**: Được xử lý từ Move action Vector2 (y > 0.1)
  - Player 1: **W** hoặc **Up Arrow**
  - Player 2: **Up Arrow**
  
- **Drop Down**: Được xử lý từ Move action Vector2 (y < -0.1 khi grounded)
  - Player 1: **S** hoặc **Down Arrow**
  - Player 2: **Down Arrow**
  
- **Horizontal Movement**: Di chuyển ngang (chỉ X axis)
  - Player 1: **A/D**
  - Player 2: **Left/Right Arrow**

### Jump System
- **Normal Jump**: Jump đầu tiên khi trên ground
- **Double Jump**: Jump thứ 2 khi đang bay (nếu enabled)
- **Triple Jump**: Jump thứ 3 khi đang bay (phải enable trước, thường qua item effect)

### Platform Drop Down
- Nhấn **S/Down Arrow** khi đang đứng trên platform để drop down qua platform
- System sẽ temporarily ignore collision với platform trong khoảng thời gian `Drop Down Time`
- Tự động reset khi landing

---

## Ghi Chú Quan Trọng

1. **DOTween** phải được cài đặt và setup trước khi chạy game
2. **Input System** phải được enable trong Project Settings
3. Đảm bảo mỗi Player có **Player ID** đúng (1 hoặc 2)
4. **Ground Layer Mask** phải được cấu hình đúng để ground detection hoạt động
5. **Platform GameObjects** chỉ cần nằm trong layer được chọn trong `Ground Layer Mask` (không cần tag riêng)
6. **Move Action** phải là **Vector2** với bindings cho cả 4 hướng (Up, Down, Left, Right)
7. **Jump và Drop Down** được xử lý từ Move Vector2, không cần action Jump riêng
8. Khi sử dụng **gamepad**, cần setup Player Index trong Input Actions
9. Triple jump mặc định **TẮT**, chỉ enable khi cần (thường qua item effect)

---

## Tùy Chỉnh Nâng Cao

### Enable Triple Jump từ Item Effect

Triple jump có thể được enable/disable từ item effect:

```csharp
// Trong item effect code
BasePlayer player = GetComponent<BasePlayer>();
player.EnableTripleJump();  // Enable triple jump
// ... sau khi item effect hết hạn
player.DisableTripleJump();  // Disable triple jump
```

### Thêm Visual Effects cho Double/Triple Jump

Có thể thêm particle effects hoặc sound effects trong methods `DoubleJump()` và `TripleJump()`:

```csharp
// Ví dụ: Trigger particle effect
if (doubleJumpEffect != null)
    doubleJumpEffect.Play();

// Ví dụ: Play sound
AudioManager.Instance.PlaySFX(doubleJumpSound);
```

### Thêm Coyote Time (Thời gian nhảy sau khi rời ground)

Có thể thêm coyote time bằng cách lưu thời gian khi rời ground và cho phép jump trong khoảng thời gian ngắn sau đó.

---

## Hỗ Trợ

Nếu gặp vấn đề, kiểm tra:
1. Console logs để xem các debug messages
2. Input Debugger để kiểm tra input events
3. Inspector để đảm bảo tất cả references được assign đúng

