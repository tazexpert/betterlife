# System: Foundation (Time / Random / EventBus)

> สถานะ: **ข้อเสนอ Data Structure — รอผู้ใช้ยืนยัน** (ยังไม่มี Logic)

## 1. สรุปความเข้าใจ
ระบบพื้นฐานที่ทุกระบบในเกมใช้ร่วมกัน 3 ส่วน:
1. **เวลา** — `GameDate` (หน่วยสัปดาห์ ใช้เดินเกม) และ `CalendarDate` (หน่วยวัน ใช้กับวันเกิด/โหราศาสตร์)
2. **การสุ่ม** — `IRandomProvider` + `Pcg32` แบบ seeded, แยก Stream ต่อระบบ เพื่อให้ Challenge Seed เล่นซ้ำได้ผลเดิม
3. **การสื่อสาร** — `IEventBus` แบบ synchronous, type-safe
ระบบนี้ไม่มีไฟล์ content JSON (ค่าคงที่เชิงโครงสร้างเป็น `const` ในโค้ด) จึงไม่มี JSON Schema

## 2. Data Structure

### 2.1 GameDate — `BetterLife.Core.Time.GameDate` (struct, immutable)
| สมาชิก | ชนิด | ความหมาย |
|---|---|---|
| `TotalWeeks` | `int` | จำนวนสัปดาห์นับจาก Epoch (สัปดาห์แรกของปี `EpochYear`) — **ค่าเดียวที่เก็บจริง** |
| `Year` | `int` (คำนวณ) | `EpochYear + TotalWeeks / WeeksPerYear` |
| `WeekOfYear` | `int` (คำนวณ) | 0–51 |
| `Month` | `int` (คำนวณ) | 1–12 จาก `WeekOfYear × 7` เทียบตารางวันสะสมของปีปกติ |
| `IsMonthStart` / `IsYearStart` | `bool` (คำนวณ) | ใช้ให้ Tick pipeline รู้ว่าต้องรันรอบการเงิน/รอบปี |

ค่าคงที่: `WeeksPerYear = 52`, `DaysPerWeek = 7`, `EpochYear = 1900`
พฤติกรรม: `AddWeeks(int)`, `WeeksUntil(GameDate)`, `==`, `<`, `IComparable`, `IEquatable`
หมายเหตุ: ปีในเกม = 52 สัปดาห์ (364 วัน) ปฏิทินในเกมเป็นแบบ stylized ไม่ตรงปฏิทินจริง 100% — แต่ละเดือนมี 4 หรือ 5 สัปดาห์ และเดือนเปลี่ยนครบ 12 ครั้งต่อปีเสมอ

### 2.2 CalendarDate — `BetterLife.Core.Time.CalendarDate` (struct, immutable)
| สมาชิก | ชนิด | ความหมาย |
|---|---|---|
| `Year`, `Month`, `Day` | `int` | วันที่จริง (validate ตอนสร้าง, รองรับปีอธิกสุรทิน) |
| `DayOfWeek` | `ThaiDayOfWeek` (คำนวณ) | วันเกิดแบบโหราไทย (อาทิตย์…เสาร์ + พุธกลางคืน ต้องใช้เวลาเกิดเพิ่ม → ดู Q4) |

พฤติกรรม: `ToGameDate()` (สัปดาห์ที่วันนั้นตกอยู่), `static AgeInYears(CalendarDate birth, GameDate now)`

### 2.3 Random — `BetterLife.Core.Random`
```
interface IRandomProvider
    uint  NextUInt()
    int   NextInt(int minInclusive, int maxExclusive)
    float NextFloat()                  // [0, 1)
    bool  Chance(float probability01)  // clamps input to [0, 1]
    IRandomProvider Fork(string streamName)
    RandomState CaptureState()
    void  RestoreState(RandomState state)

struct RandomState { ulong State; ulong Increment; }
```
- `Pcg32` = PCG-XSH-RR 64/32 (อัลกอริทึมมาตรฐานของ O'Neill)
- **Stream แยกต่อระบบ:** `Fork("events")`, `Fork("npc")` … สร้าง Stream ใหม่โดย hash ชื่อ (FNV-1a 64) ร่วมกับ seed ของชีวิต
  → เพิ่มการสุ่มในระบบหนึ่งจะไม่ทำให้ผลสุ่มของระบบอื่นเลื่อน (สำคัญกับ Challenge Seed และ Undo)
- `RandomRegistry` เก็บ Stream ทั้งหมดตามชื่อ เพื่อ Save/Load พร้อมกัน

### 2.4 EventBus — `BetterLife.Core.Events`
```
interface IGameEvent { }                         // marker
interface IEventBus
    IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent
    void        Publish<T>(T gameEvent)         where T : IGameEvent
```
- Synchronous, เรียก handler ตามลำดับการ Subscribe
- Publish ซ้อนระหว่าง dispatch → **เข้าคิว** และส่งหลัง event ปัจจุบันจบ (FIFO) ป้องกันลำดับเพี้ยน
- Handler throw → handler ที่เหลือยังทำงานต่อ แล้วโยน `AggregateException` หลัง dispatch จบ
- Event เป็นข้อมูลชั่วคราว **ไม่ถูก Save**

## 3. Save DTO — `BetterLife.Core.Persistence`
```json
{
  "currentDate": { "totalWeeks": 4160 },
  "random": {
    "lifeSeed": "9f1c2a7b00000000",
    "streams": {
      "events": { "state": "0123456789abcdef", "increment": "fedcba9876543211" },
      "npc":    { "state": "…", "increment": "…" }
    }
  }
}
```
- `GameDateDto { int TotalWeeks }`
- `CalendarDateDto { int Year; int Month; int Day }`
- `RandomStateDto { string State; string Increment }` — เก็บ `ulong` เป็น hex string กันปัญหาความแม่นยำของ JSON number
- `RandomRegistryDto { string LifeSeed; Dictionary<string, RandomStateDto> Streams }`

## 4. คำถามที่ต้องยืนยัน
1. **ปฏิทิน:** ปีละ 52 สัปดาห์ เดือนคำนวณจากวันในปี (แนะนำ) หรือ 48 สัปดาห์ (เดือนละ 4 สัปดาห์เท่ากันหมด แต่ปีสั้นกว่าจริง)?
2. **Epoch = ค.ศ. 1900** พอสำหรับ "ยุคที่เกิด" ไหม? (ใช้ ค.ศ. ภายใน แล้วแสดงผลเป็น พ.ศ. ใน UI)
3. **EventBus:** Publish ซ้อนให้เข้าคิว (แนะนำ) หรือส่งทันทีแบบ recursive?
4. **วันพุธกลางคืน (ราหู):** ต้องเก็บเวลาเกิด (ชั่วโมง) ด้วยไหม? ถ้าใช่จะเพิ่ม `BirthHour` ใน CharacterCreation ไม่ใช่ใน CalendarDate
