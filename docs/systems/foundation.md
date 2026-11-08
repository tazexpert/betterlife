# System: Foundation (Time / Randomness / EventBus)

> สถานะ: ✅ **Implemented** (Milestone 1) — 2026-11-08

## 1. สรุป
ระบบพื้นฐาน 3 ส่วนที่ทุกระบบในเกมใช้ร่วมกัน
1. **เวลา:** `GameDate` (หน่วยสัปดาห์ ใช้เดินเกม), `CalendarDate` (หน่วยวัน ใช้กับวันเกิด) และ `ThaiDayOfWeekResolver` (หาวันเกิดแบบโหราไทย)
2. **การสุ่ม:** `IRandomProvider` + `Pcg32` + `RandomRegistry` แยก Stream ตามระบบ
3. **การสื่อสาร:** `IEventBus` / `EventBus`

ระบบนี้ไม่มี content JSON (ค่าคงที่ทั้งหมดเป็นค่าเชิงโครงสร้าง จึงเก็บเป็น `const`) จึงไม่มี JSON Schema

## 2. การตัดสินใจที่ผู้ใช้ยืนยันแล้ว
| หัวข้อ | ผลการตัดสินใจ |
|---|---|
| ปฏิทิน | ปีละ 52 สัปดาห์ เดือนคำนวณจากวันในปี (เดือนละ 4–5 สัปดาห์ เดือนเปลี่ยนครบ 12 ครั้งต่อปีเสมอ) |
| Epoch | ค.ศ. **1926** (= 2026 − 100) ในโค้ดใช้ ค.ศ. ส่วน UI แสดงเป็น พ.ศ. ปีก่อน 1926 ใช้ค่า `TotalWeeks` ติดลบได้ (เช่น ปีเกิดของปู่ย่าตายาย NPC) |
| Publish ซ้อน | เข้าคิวรอ (FIFO) |
| เวลาเกิด | เก็บชั่วโมงเกิด โดยฟิลด์ `BirthHour` จะอยู่ใน Character Creation ส่วน Foundation มีตัวคำนวณวันเกิดแบบโหราไทยไว้ให้แล้ว |

### เรื่อง Publish ซ้อน อธิบายด้วยตัวอย่าง
ระบบ Career ประกาศ `JobLost` แล้วระบบ Mental ที่ฟังอยู่ประกาศต่อว่า `StressRaised`
→ `StressRaised` จะถูกส่ง **หลังจาก** `JobLost` ไปถึงทุกระบบครบแล้วเท่านั้น
ผลคือลำดับเหตุการณ์คาดเดาได้เสมอ และไม่มีระบบไหนได้รับข่าวสองเรื่องสลับลำดับกัน

## 3. Domain (path: `packages/com.betterlife.core/Runtime/`)
| ไฟล์ | หน้าที่ |
|---|---|
| `Time/GameCalendar.cs` | ค่าคงที่: `EpochYear=1926`, `WeeksPerYear=52`, ตารางวันเริ่มต้นของแต่ละเดือน |
| `Time/GameDate.cs` | struct ที่เก็บค่าเดียวคือ `TotalWeeks` มี `Year`, `WeekOfYear`, `Month`, `IsMonthStart`, `IsYearStart`, `AddWeeks`, `WeeksUntil` |
| `Time/CalendarDate.cs` | วันที่จริงที่ผ่านการตรวจแล้ว (รวมปีอธิกสุรทิน) มี `DayOfWeek`, `DayOfYear`, `ToGameDate()`, `AgeAt(GameDate)` |
| `Time/ThaiDayOfWeek.cs` | enum 8 วัน (มีพุธกลางคืน) |
| `Time/ThaiDayOfWeekResolver.cs` | วันทางโหราศาสตร์เริ่มตอน 06:00 ถ้าเกิดก่อน 06:00 นับเป็นวันก่อนหน้า และช่วงพุธ 18:00 ถึงพฤหัส 05:59 นับเป็นพุธกลางคืน (ราหู) |
| `Randomness/IRandomProvider.cs` | สัญญาการสุ่ม |
| `Randomness/Pcg32.cs` | PCG-XSH-RR 64/32 ให้ผลตรงกับ reference implementation |
| `Randomness/RandomState.cs` | `Seed`, `State`, `Increment` |
| `Randomness/StableHash.cs` | FNV-1a 64 บน UTF-8 ให้ค่าเดิมทุกเครื่องและทุก runtime |
| `Randomness/RandomRegistry.cs` | Stream ตามชื่อของชีวิตนั้น สร้างเมื่อเรียกใช้ครั้งแรก และ Capture/Restore ได้ทั้งหมด |
| `Events/IGameEvent.cs`, `IEventBus.cs`, `EventBus.cs` | Bus แบบ synchronous ส่งตามชนิด event ตรงตัว Publish ซ้อนจะเข้าคิว และถ้า handler ตัวใดพัง ตัวที่เหลือยังได้รับ event ต่อ จากนั้นจึงโยน `AggregateException` |

### กฎของการสุ่ม
- `Chance(p)` ใช้ค่าสุ่ม 1 ค่าเสมอ แม้ p = 0 หรือ 1 ทำให้การปรับ balance ค่าโอกาสไม่ทำให้ผลสุ่มลำดับถัดไปเลื่อน
- `Fork(name)` ขึ้นกับ seed กับชื่อเท่านั้น ไม่ขึ้นกับว่าใช้ค่าสุ่มไปแล้วกี่ค่า
- ทุกระบบต้องรับ `IRandomProvider` ผ่าน constructor โดย GameBootstrap เป็นผู้ส่งให้จาก `registry.GetStream("<system>")`

## 4. Save DTO (`Runtime/Persistence/`)
```json
{
  "CurrentDate": { "TotalWeeks": 5245 },
  "Random": {
    "LifeSeed": "deadbeefcafef00d",
    "Streams": [
      { "Name": "events", "Seed": "…16 hex…", "State": "…", "Increment": "…" }
    ]
  }
}
```
- `GameDateDto`, `CalendarDateDto`, `RandomStreamDto`, `RandomRegistryDto` ใช้ public fields + `[Serializable]` ใช้ได้ทั้งกับ JsonUtility และ Newtonsoft
- ค่า `ulong` เก็บเป็น hex 16 หลัก เพื่อไม่ให้ความแม่นยำตกหล่นเกิน 2^53
- เก็บ Streams เป็น List ไม่ใช้ Dictionary เพราะ JsonUtility serialize Dictionary ไม่ได้ และเรียงตามชื่อเพื่อให้ diff ไฟล์ save อ่านง่าย
- `FoundationDtoMapper` แปลงไป/กลับ และตรวจข้อมูลเสียด้วย `FormatException` / `ArgumentOutOfRangeException`

**จุดที่เปลี่ยนจากข้อเสนอเดิม:** ใน `RandomState` เพิ่ม `Seed` เพื่อให้ `Fork` ยังได้ผลเดิมหลังโหลดเซฟ, Streams เปลี่ยนจาก Dictionary เป็น List, namespace `Random` เปลี่ยนเป็น `Randomness` เพื่อไม่ให้ชื่อชนกับ `System.Random`

## 5. Tests (`Tests/Runtime/`) — 86 tests (รวม ArchitectureRulesTests)
GameDate, CalendarDate (เทียบวันในสัปดาห์กับ `System.DateTime` ตลอดปี 1920–2040), ThaiDayOfWeekResolver,
Pcg32 (reference vector seed 42/seq 54), StableHash (FNV test vectors), RandomRegistry, EventBus, FoundationDtoMapper

## 6. Unity Integration (ทำใน Milestone 13 เมื่อมี Unity project)
```csharp
// GameBootstrap (composition root) — planned wiring
var bus = new EventBus();
var random = new RandomRegistry(lifeSeed);
var eventStream = random.GetStream("events");   // pass to EventSystem constructor
var npcStream   = random.GetStream("npc");      // pass to NpcSystem constructor
```
