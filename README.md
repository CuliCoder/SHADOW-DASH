# Game Design Document — Shadow Dash
**Version 1.0 | Unity 2D | Fresher Portfolio Project**

---

## 1. Tổng quan

| | |
|---|---|
| Tên game | Shadow Dash |
| Thể loại | Endless Runner 2D |
| Engine | Unity 2022 LTS |
| Platform | WebGL (itch.io) + Windows build |
| Thời gian làm | 7 ngày |
| Mục tiêu | Portfolio xin intern/fresher Unity Developer |

### Concept

Nhân vật silhouette chạy tự động qua thành phố đêm, nhảy và trượt tránh chướng ngại vật, tốc độ tăng dần. Mỗi obstacle đều được hệ thống **Playability Guarantee Algorithm** kiểm tra trước khi spawn — đảm bảo người chơi luôn có cơ hội thoát, không bao giờ chết oan.

### Twist phân biệt với clone thông thường

- **Ghost trail:** Nhân vật để lại bóng mờ dần theo sau khi chạy nhanh.
- **Playability Guarantee System:** Thuật toán độc đáo simulate physics để đảm bảo fairness — điểm showcase chính trong phỏng vấn.

---

## 2. Gameplay Loop

```
[Main Menu]
    │
    ▼  Space / Tap
[Gameplay] ──────────────────────────────────────────────────────────┐
    │  Nhân vật chạy tự động từ trái sang phải                       │
    │  Người chơi: Nhảy / Double Jump / Trượt                        │
    │  Obstacle spawn từ phải (đã qua Playability Check)             │
    │  Tốc độ + spawn rate tăng dần mỗi 10 giây                     │
    │  Va chạm obstacle → chết                                        │
    ▼                                                                  │
[Game Over]  ◄────────────────────────────────────────────────────────┘
    │  Hiện Score + Best Score
    ├──► [RETRY] → Gameplay
    └──► [MENU]  → Main Menu
```

---

## 3. Controls

| Input | Hành động |
|---|---|
| Space / W / ↑ / Tap | Nhảy (lần 1) |
| Space / W / ↑ / Tap (khi đang bay) | Double Jump |
| S / ↓ / Swipe Down | Trượt (0.6 giây) |

---

## 4. Nhân vật (Player)

### 4.1 State Machine

Nhân vật có 5 state rõ ràng:

```
              ┌─────────┐
    ┌─────────► RUNNING ◄─────────┐
    │         └────┬────┘         │ chạm đất
    │              │ jump         │
    │         ┌────▼────┐         │
    │         │ JUMPING │         │
    │         └────┬────┘         │
    │              │ jump (lần 2) │
    │         ┌────▼──────────┐   │
    │         │ DOUBLE_JUMPING├───┘
    │         └───────────────┘
    │
    │  [S / ↓]
    │         ┌─────────┐
    └─────────┤ SLIDING │  (0.6s, hitbox nhỏ lại)
              └─────────┘
```

State `DEAD` kích hoạt khi va chạm bất kỳ obstacle nào.

### 4.2 Thông số vật lý

| Thông số | Giá trị | Ghi chú |
|---|---|---|
| Run speed (khởi đầu) | 7 units/s | Tăng dần theo thời gian |
| Jump force | 15 | Lực nhảy lần 1 |
| Double jump force | 12 | Lực nhảy lần 2 (nhẹ hơn) |
| Gravity scale | 3.5 | Nặng hơn default để feel snappy |
| Slide duration | 0.6s | Hitbox co lại trong thời gian này |
| Hitbox đứng | 0.6 × 1.2 | Width × Height (units) |
| Hitbox trượt | 1.0 × 0.5 | Rộng hơn, thấp hơn |

### 4.3 Game Feel — bắt buộc implement

Đây là thứ phân biệt code tốt với code trung bình. Nhà tuyển dụng nhìn vào code của mình sẽ thấy ngay:

**Coyote Time (0.12s):**
Sau khi bước ra khỏi edge, vẫn cho phép nhảy trong 0.12 giây. Loại bỏ cảm giác "nhảy không lên" oan ức.

```csharp
private float coyoteTimeCounter;
private const float COYOTE_TIME = 0.12f;

void Update() {
    if (IsGrounded) coyoteTimeCounter = COYOTE_TIME;
    else coyoteTimeCounter -= Time.deltaTime;

    bool canJump = coyoteTimeCounter > 0f;
}
```

**Jump Buffer (0.1s):**
Nhấn nhảy trước khi chạm đất 0.1 giây vẫn được nhảy. Loại bỏ cảm giác "bấm không ăn".

```csharp
private float jumpBufferCounter;
private const float JUMP_BUFFER = 0.1f;

void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) jumpBufferCounter = JUMP_BUFFER;
    else jumpBufferCounter -= Time.deltaTime;

    if (jumpBufferCounter > 0f && canJump) {
        Jump();
        jumpBufferCounter = 0f;
    }
}
```

**Squash & Stretch:**
- Khi nhảy: `scaleY *= 1.2f, scaleX *= 0.85f`
- Khi chạm đất: `scaleX *= 1.3f, scaleY *= 0.75f`, rồi lerp về (1, 1) trong 0.1s

**Ghost Trail:**
Mỗi 0.08 giây, instantiate một sprite copy tại vị trí hiện tại, alpha bắt đầu từ 0.4f rồi fade về 0 trong 0.3s. Dùng coroutine đơn giản.

**Screen Shake khi chết:**
Magnitude 0.15, duration 0.3s, dùng Cinemachine Impulse hoặc tự viết coroutine offset camera.

---

## 5. Thuật toán chính — Playability Guarantee Algorithm

Đây là điểm showcase quan trọng nhất của project. Mỗi lần chuẩn bị spawn obstacle, hệ thống simulate trước quỹ đạo physics của player để kiểm tra obstacle đó có thể tránh được không.

### 5.1 Tại sao cần thuật toán này?

Endless runner thông thường spawn obstacle random. Vấn đề: đôi khi sinh ra tổ hợp mà dù player phản ứng hoàn hảo vẫn chết — "chết oan". Playability Guarantee giải quyết điều đó bằng cách **hỏi trước khi spawn**.

### 5.2 Cách hoạt động (3 bước)

```
Bước 1: Tính thời gian obstacle đến vị trí player
         timeToReach = spawnDistance / currentSpeed

Bước 2: Simulate quỹ đạo nhảy tốt nhất của player
         Chạy mini physics loop (60fps) từ vị trí hiện tại
         Áp dụng jump force → gravity → double jump ở thời điểm tối ưu
         Ghi lại độ cao player tại t = timeToReach

Bước 3: So sánh và quyết định
         playerHeightAtObstacle > obstacleHeight → SPAWN
         playerHeightAtObstacle ≤ obstacleHeight → LOẠI BỎ, chọn loại khác
```

### 5.3 Code hoàn chỉnh

**PlayabilityChecker.cs:**

```csharp
using UnityEngine;

public class PlayabilityChecker : MonoBehaviour
{
    [Header("Phải khớp với PlayerController")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float doubleJumpForce = 12f;
    [SerializeField] private float gravityScale = 3.5f;

    private float Gravity => Physics2D.gravity.y * gravityScale;

    public bool CanPlayerAvoid(ObstacleType type, float distanceToObstacle,
                               float currentSpeed, PlayerStateInfo state)
    {
        float timeToReach = distanceToObstacle / currentSpeed;

        return type switch
        {
            ObstacleType.LowBlock  => SimulateBestJump(timeToReach, state) > GetHeight(type),
            ObstacleType.Spike     => SimulateBestJump(timeToReach, state) > GetHeight(type),
            ObstacleType.HighBlock => state.isGrounded, // chỉ cần đang đứng để trượt
            ObstacleType.Floating  => CanJumpOverOrRunUnder(timeToReach, state, type),
            _ => true
        };
    }

    private float SimulateBestJump(float timeToReach, PlayerStateInfo state)
    {
        const float dt = 0.016f;
        float y   = state.currentHeight;
        float vy  = state.isGrounded ? jumpForce : state.currentVelocityY;
        bool  usedDouble = !state.canDoubleJump;
        float elapsed = 0f;

        if (state.isGrounded) vy = jumpForce;

        while (elapsed < timeToReach)
        {
            vy += Gravity * dt;
            y  += vy * dt;

            if (y <= 0f) { y = 0f; vy = 0f; }

            // Dùng double jump ở thời điểm tối ưu (đang rơi, còn đủ thời gian)
            if (!usedDouble && vy < 0f && elapsed < timeToReach * 0.6f)
            {
                vy = doubleJumpForce;
                usedDouble = true;
            }

            elapsed += dt;
        }
        return y;
    }

    private bool CanJumpOverOrRunUnder(float timeToReach, PlayerStateInfo state, ObstacleType type)
    {
        float playerH = SimulateBestJump(timeToReach, state);
        bool  canOver  = playerH >  (type);
        bool  canUnder = GetBottom(type) > PlayerStandHeight;
        return canOver || canUnder;
    }

    private float GetHeight(ObstacleType type) => type switch
    {
        ObstacleType.LowBlock => 0.8f,
        ObstacleType.Spike    => 0.5f,
        _ => 1f
    };

    private float GetTop(ObstacleType type)    => 1.8f;
    private float GetBottom(ObstacleType type) => 1.2f;
    private float PlayerStandHeight            => 1.2f;
}
```

**PlayerStateInfo.cs:**

```csharp
public struct PlayerStateInfo
{
    public bool  isGrounded;
    public bool  canDoubleJump;
    public float currentHeight;
    public float currentVelocityY;

    public static PlayerStateInfo Capture(PlayerController p) => new()
    {
        isGrounded       = p.IsGrounded,
        canDoubleJump    = p.CanDoubleJump,
        currentHeight    = p.transform.position.y,
        currentVelocityY = p.Rb.velocity.y
    };
}
```

**Tích hợp vào ObstacleSpawner.cs:**

```csharp
private void TrySpawnObstacle()
{
    var state      = PlayerStateInfo.Capture(player);
    float speed    = GameManager.Instance.CurrentSpeed;
    var candidates = GetCandidatesForDifficulty();

    foreach (var type in candidates)
    {
        if (checker.CanPlayerAvoid(type, spawnDistance, speed, state))
        {
            SpawnObstacleFromPool(type);
            return;
        }
    }
    // Không có loại nào passable → delay 0.3s (hiếm xảy ra)
    StartCoroutine(DelayNextSpawn(0.3f));
}
```

### 5.4 Câu giải thích trong phỏng vấn

> "Em xây một Playability Guarantee System — thay vì spawn obstacle random, hệ thống simulate trước quỹ đạo physics của player ở 60fps để kiểm tra obstacle đó có thể tránh được không. Nó dùng công thức kinematic `v = v₀ + at` chạy forward trong thời gian player cần đến obstacle, tính độ cao tại điểm đó, rồi so sánh với chiều cao obstacle. Nếu impossible thì loại bỏ và chọn loại khác. Cách này đảm bảo người chơi không bao giờ chết oan."

---

## 6. Obstacle System

### 6.1 Các loại obstacle

| Loại | Chiều cao | Cách tránh | Phase mở khóa |
|---|---|---|---|
| Low Block | 0.8u | Nhảy qua | Từ đầu |
| Spike | 0.5u | Nhảy qua | 30s |
| High Block | 1.5u | Trượt dưới | 10s |
| Floating Block | 1.2–1.8u | Nhảy qua hoặc chạy dưới | 45s |

Làm theo thứ tự: Low Block → High Block → Spike → Floating. Mỗi loại thêm một chiều phức tạp.

### 6.2 Object Pooling (bắt buộc)

Không dùng `Instantiate/Destroy`. Dùng pool để thể hiện kỹ năng:

```csharp
public class ObstaclePool : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs; // indexed by ObstacleType
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject>[] pools;

    private void Awake()
    {
        pools = new Queue<GameObject>[prefabs.Length];
        for (int i = 0; i < prefabs.Length; i++)
        {
            pools[i] = new Queue<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                var obj = Instantiate(prefabs[i]);
                obj.SetActive(false);
                pools[i].Enqueue(obj);
            }
        }
    }

    public GameObject Get(ObstacleType type)
    {
        int i = (int)type;
        if (pools[i].Count > 0)
        {
            var obj = pools[i].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefabs[i]); // fallback nếu pool cạn
    }

    public void Return(GameObject obj, ObstacleType type)
    {
        obj.SetActive(false);
        pools[(int)type].Enqueue(obj);
    }
}
```

### 6.3 Difficulty Curve

| Thời gian | Tốc độ | Spawn interval | Obstacle available |
|---|---|---|---|
| 0–10s | 7 u/s | 2.0s | Low Block |
| 10–30s | 9 u/s | 1.6s | + High Block |
| 30–45s | 11 u/s | 1.2s | + Spike |
| 45–60s | 13 u/s | 1.0s | + Floating |
| 60s+ | 14+ u/s | 0.85s | Tất cả |

---

## 7. Scoring System

```
Score = floor(timeAlive × multiplier)

Multiplier theo thời gian:
  0–10s  →  ×1
  10–30s →  ×1.5
  30–60s →  ×2
  60s+   →  ×3
```

- Hiển thị real-time góc trên phải, font to, dễ đọc.
- High Score lưu bằng `PlayerPrefs.SetInt("HighScore", value)`.
- Khi phá kỷ lục: text "NEW BEST!" flash màu vàng 3 lần.

---

## 8. Visual Style

### 8.1 Aesthetic: Silhouette Night City

Nhân vật và obstacle: hình khối đen tuyền.
Background: gradient tối (tím đêm → xanh đen).

Lý do chọn style này: không cần artist, trông chuyên nghiệp, đẹp tự nhiên với bất kỳ hình khối nào.

### 8.2 Parallax Background — 3 lớp

| Layer | Scroll speed | Nội dung |
|---|---|---|
| Far (0) | speed × 0.2 | Tòa nhà xa, mặt trăng |
| Mid (1) | speed × 0.5 | Tòa nhà gần, cột điện |
| Near (2) | speed × 0.8 | Cây, rào chắn |

```csharp
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float speedMultiplier;
    private float startX;
    private float width;

    void Start() {
        startX = transform.position.x;
        width  = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update() {
        float dist = GameManager.Instance.CurrentSpeed * speedMultiplier * Time.deltaTime;
        transform.Translate(-dist, 0, 0);

        // Vô hạn: reset khi ra khỏi màn hình
        if (transform.position.x < startX - width)
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
    }
}
```

### 8.3 Particle Effects

| Hiệu ứng | Kích hoạt | Mô tả |
|---|---|---|
| Death burst | Va chạm obstacle | 20 hạt màu tím/trắng bắn ra |
| Jump puff | Rời mặt đất | 5 hạt bụi nhỏ từ chân |
| Land puff | Chạm mặt đất | Puff nhỏ hai bên chân |
| Ghost trail | Mỗi 0.08s | Sprite mờ fade từ 0.4 → 0 trong 0.3s |

---

## 9. Audio

| Sound | Mô tả | Free source |
|---|---|---|
| jump.wav | Nhẹ, nhanh, snappy | freesound.org |
| double_jump.wav | Pitch cao hơn jump.wav | freesound.org |
| slide.wav | Tiếng xào xạo nhẹ | freesound.org |
| death.wav | Dramatic ngắn (< 1s) | freesound.org |
| score_milestone.wav | Ding nhỏ mỗi 100 điểm | freesound.org |
| bgm.mp3 | Lo-fi / synthwave, loop | opengameart.org |

**AudioManager** dùng Singleton pattern, có hai channel: SFX và BGM riêng để điều chỉnh volume độc lập.

---

## 10. UI Screens

### Main Menu
- Tên game to ở giữa
- `"PRESS SPACE TO START"` nhấp nháy (dùng coroutine hoặc DOTween)
- High Score hiển thị nhỏ phía dưới
- Background: parallax chạy nhẹ, nhân vật idle

### HUD (trong gameplay)
- Score: góc trên phải, to, rõ
- Không thêm gì khác — tối giản

### Game Over
- Overlay tối mờ (alpha 0.7)
- "GAME OVER" — font to
- Score vừa đạt
- Best Score (highlight vàng nếu kỷ lục mới)
- Hai nút: `[RETRY]` và `[MENU]`

---

## 11. Kiến trúc Code

Trình bày cấu trúc này trong phỏng vấn khi được hỏi "em organize code như thế nào":

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs         ← Singleton | state: Menu/Playing/Dead
│   ├── ScoreManager.cs        ← score, multiplier, high score, PlayerPrefs
│   └── AudioManager.cs        ← Singleton | SFX + BGM channels
│
├── Player/
│   ├── PlayerController.cs    ← input, state machine, coyote time, jump buffer
│   ├── PlayerAnimator.cs      ← squash/stretch, ghost trail coroutine
│   └── PlayerDeath.cs         ← death FX, trigger GameManager
│
├── Obstacles/
│   ├── ObstacleType.cs        ← enum: LowBlock, HighBlock, Spike, Floating
│   ├── ObstaclePool.cs        ← object pooling với Queue<GameObject>
│   ├── ObstacleSpawner.cs     ← spawn logic, difficulty curve, gọi checker
│   ├── ObstacleBase.cs        ← base class: move, despawn, return to pool
│   └── PlayabilityChecker.cs  ← THUẬT TOÁN CHÍNH — simulate physics
│
├── UI/
│   ├── MainMenuUI.cs
│   ├── GameHUD.cs
│   └── GameOverUI.cs
│
└── Utils/
    ├── CameraShake.cs
    ├── ParallaxLayer.cs
    └── PlayerStateInfo.cs     ← struct capture player state
```

### Patterns sử dụng (cần giải thích được trong phỏng vấn)

**Singleton** — `GameManager`, `AudioManager`: dùng vì cần access từ mọi nơi, chỉ tồn tại một instance duy nhất.

**Object Pooling** — `ObstaclePool`: tránh Instantiate/Destroy liên tục gây GC spike, quan trọng với mobile/WebGL.

**State Machine** — `PlayerController`: Running/Jumping/DoubleJumping/Sliding/Dead. Mỗi state có transition rules rõ ràng, tránh bug logic phức tạp.

**Struct (PlayerStateInfo)** — Pass-by-value, không cần allocate heap, phù hợp cho dữ liệu nhỏ đọc nhiều lần mỗi frame.

---

## 12. Definition of Done

Game hoàn chỉnh khi đạt tất cả:

- [ ] Chạy được trên WebGL tại itch.io (link public)
- [ ] Flow đầy đủ: Main Menu → Gameplay → Game Over → Retry/Menu
- [ ] Ít nhất 3 loại obstacle khác nhau
- [ ] Playability Guarantee System hoạt động (không có obstacle impossible)
- [ ] Object Pooling cho obstacle (không dùng Instantiate/Destroy)
- [ ] Tốc độ tăng dần rõ rệt theo thời gian
- [ ] High Score lưu lại sau khi thoát game
- [ ] Sound effects và BGM
- [ ] Ghost trail + squash/stretch + screen shake
- [ ] Code đẩy lên GitHub với README
- [ ] Không crash trong 5 phút chơi liên tục

---

## 13. Lịch phát triển 7 ngày

| Ngày | Việc làm | Output kiểm tra |
|---|---|---|
| 1 | Setup project + `PlayerController`: chạy + nhảy cơ bản | Nhân vật nhảy được |
| 2 | Coyote time + jump buffer + double jump + slide + state machine | Feel mượt, không bug |
| 3 | `ObstaclePool` + `ObstacleSpawner` + collision | Obstacle spawn và despawn đúng |
| 4 | `PlayabilityChecker` + integrate vào spawner + difficulty curve | Không có obstacle impossible |
| 5 | Score system + UI 3 màn + parallax background | Flow hoàn chỉnh |
| 6 | Particle effects + ghost trail + squash/stretch + screen shake + audio | Juice đầy đủ |
| 7 | Bug fix + WebGL build + itch.io + GitHub README | Link public hoạt động |

---

## 14. README Template (GitHub)

```markdown
# Shadow Dash

Endless runner 2D làm bằng Unity. Nhảy và trượt tránh chướng ngại vật,
tốc độ tăng dần. Điểm kỹ thuật nổi bật: Playability Guarantee Algorithm
đảm bảo mọi obstacle đều có thể tránh được.

## Play online
👉 [link itch.io]

## Điểm kỹ thuật tôi tự implement

- **Playability Guarantee Algorithm:** Mini physics simulator chạy lookahead
  để đảm bảo obstacle luôn passable trước khi spawn. Dùng kinematic simulation
  ở 60fps, tính quỹ đạo tối ưu của player và so sánh với obstacle height.
- **Object Pooling:** Queue-based pool cho obstacle, tránh GC spike.
- **Game Feel:** Coyote time (0.12s), jump buffer (0.1s), squash & stretch,
  ghost trail, screen shake.
- **State Machine:** 5 state rõ ràng cho player (Running/Jumping/DoubleJumping/Sliding/Dead).

## Tech
Unity 2022 LTS · C# · WebGL

## Những gì chưa làm / muốn cải thiện
- Mobile touch controls
- Online leaderboard
- Power-up system (magnet, slow-mo)
- Thêm loại obstacle phức tạp hơn
```

---

*GDD v1.0 — Được thiết kế để hoàn thành trong 7 ngày, đủ gây ấn tượng trong phỏng vấn intern/fresher Unity Developer.*
