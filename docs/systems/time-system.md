# Task 05: Time System + Time Budget

> สถานะ: ✅ **Implemented** — 2026-11-08 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **Time Budget:** ผู้เล่นแบ่งเวลา 168 ชม./สัปดาห์ ให้กิจกรรมต่างๆ (นอน, เรียน, ออกกำลังกาย, เข้าสังคม ฯลฯ)
  ทุกชั่วโมงมีผลต่อ Stat/Skill และบางกิจกรรมมีค่าใช้จ่าย (ทุกทางเลือกมีราคา)
- **Tick:** การข้ามเวลา 1 สัปดาห์ / 1 เดือน / 1 ปี โดยรันขั้นตอน (Phase) ตามลำดับใน GDD:
  Time allocation → Natural decay → Delayed consequences → Event roll → NPC update → Death check
  ข้อนี้สร้าง Phase แรกๆ ส่วน Phase อื่นจะเสียบเพิ่มในข้อ 07, 09, 12 โดยไม่ต้องแก้ TimeSystem
- **Interrupt:** หยุดการข้ามเวลาเมื่อมีเหตุการณ์ระดับ Major/Critical

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ชั่วโมงที่ยังไม่ได้แบ่ง | ไปอยู่ที่กิจกรรม default (`free_time`) อัตโนมัติ | ผลรวมจึงเป็น 168 เสมอ และ UI ไม่ต้องบังคับให้ผู้เล่นแบ่งครบ |
| ผลต่อชั่วโมง | เป็นเส้นตรง (`perHour`) และมี `maxHours` ไว้ตัด: เกินกว่านี้ไม่ได้ประโยชน์เพิ่ม | อ่านตัวเลขได้ง่าย และกันการปั๊มค่าแบบสุดโต่ง |
| ผลของกิจกรรม | ผ่าน `Character.ApplyStatChange` โดยใช้ชื่อกิจกรรมเป็นที่มา | Trait มีผลด้วย และ Breakdown บอกได้ว่า "Fatigue +12 จากการทำงาน" |
| การเรียนรู้ | ตัวคูณ Skill = 0.5 + Mental/100 (ช่วง 0.5–1.5 ตั้งค่าได้ใน JSON) | GDD: Mental ต่ำ → เรียนรู้ช้า |
| ค่าใช้จ่ายของกิจกรรม | `costPerHour` × ชั่วโมง ถ้าเงินไม่พอจะจ่ายเท่าที่มี แล้ว Publish `CashShortfallEvent` | สอดคล้องกับข้อ 03 และข้อ 10 นำไปทำเป็นหนี้ |
| Natural decay | ค่าเคลื่อนเข้าหา `target` ทั้งขึ้นและลง ไม่เกิน `ratePerWeek` ผ่าน ApplyStatChange (Trait มีผล: เช่น คนนักกีฬา Fitness ลดช้ากว่า) | |
| Interrupt | Phase ส่งคำขอได้ ระบบจะทำสัปดาห์นั้นให้จบทุก Phase ก่อน แล้วค่อยหยุด (ระดับเริ่มต้น: Major) | สถานะเกมสม่ำเสมอเสมอ (เช่น Death check ยังได้รัน) |
| Cadence | Phase ระบุได้ว่ารันทุกสัปดาห์ / ต้นเดือน / ต้นปี | GDD: สัปดาห์ = Micro, เดือน = การเงิน, ปี = Macro |
| ที่เก็บ Time Budget | อยู่นอก `Character` ใน `TimeState` | กิจกรรมมี Condition (Rule) ที่ต้องใช้ CharacterCatalogs ถ้าเก็บไว้ใน Character จะเกิดการอ้างอิงวนกัน |

## 3. JSON Schema

### `content/activities.json` → `activities.schema.json`
```json
{ "activities": [
  { "id": "exercise", "nameKey": "activity.exercise.name", "descKey": "activity.exercise.desc",
    "category": "health", "minHours": 0, "maxHours": 28, "costPerHour": 0,
    "condition": { "type": "age", "op": ">=", "value": 6 },
    "stats": [ { "stat": "health.fitness", "perHour": 0.3, "maxHours": 10 },
               { "stat": "health.fatigue", "perHour": 0.4 } ],
    "skills": [] }
] }
```
| ฟิลด์ | ความหมาย |
|---|---|
| `minHours` / `maxHours` | ช่วงที่ตั้งได้ต่อสัปดาห์ |
| `costPerHour` | บาทต่อชั่วโมง (decimal) |
| `condition` | ไม่บังคับ: ถ้าไม่ผ่าน กิจกรรมนี้ใช้ไม่ได้ ชั่วโมงจะกลายเป็นเวลาว่าง (เช่น เด็ก 3 ขวบทำงานไม่ได้) |
| `stats[].perHour`, `stats[].maxHours` | ผลต่อชั่วโมง และเพดานชั่วโมงที่ยังให้ผล |
| `skills[].gainPerHour`, `skills[].maxHours` | Skill ที่ได้ฝึก (คูณด้วยตัวคูณการเรียนรู้) |

### `content/time_settings.json` → `time_settings.schema.json` (ต้องมีไฟล์เดียว)
```json
{ "defaultActivity": "free_time",
  "learning": { "multiplierAtZeroMental": 0.5, "multiplierAtFullMental": 1.5 },
  "defaultBudget": [ { "activity": "sleep", "hours": 56 }, { "activity": "chores", "hours": 10 } ] }
```

## 4. C# Domain
| Namespace / Class | หน้าที่ |
|---|---|
| `Domain.Severity` | Minor / Normal / Major / Critical (ใช้ร่วมกับ Event System ข้อ 07) |
| `Time.Budget.ActivityDefinition`, `ActivityStatEffect`, `ActivitySkillEffect` | นิยามกิจกรรม |
| `Time.Budget.TimeSettings` | กิจกรรม default, เส้นการเรียนรู้, budget เริ่มต้น |
| `Time.Budget.TimeBudget` | ชั่วโมงต่อกิจกรรม: `Set`, `GetHours`, `FreeHours`, `Validate`, `EffectiveAllocation` |
| `Time.Budget.BudgetIssue`, `BudgetIssueKind` | ปัญหาที่ UI ใช้แสดง: เกินเวลา / ต่ำกว่าขั้นต่ำ / เกินเพดาน / ใช้ไม่ได้ |
| `Time.Ticking.ITickPhase` | `Name`, `Order`, `Cadence`, `Execute(TickContext)` |
| `Time.Ticking.TickOrder` | ลำดับมาตรฐาน: 100 Allocation, 200 Decay, 300 Delayed, 400 Event, 500 NPC, 600 Death, 900 Record |
| `Time.Ticking.TickCadence` | Weekly / Monthly / Yearly |
| `Time.Ticking.TickContext` | สัปดาห์ปัจจุบัน, Character, `RequestInterrupt(...)` |
| `Time.Ticking.InterruptRequest`, `InterruptPolicy` | คำขอหยุด + เกณฑ์ (ปรับได้ใน Settings ข้อ 20) |
| `Time.Ticking.GameClock` | สัปดาห์ปัจจุบันของเกม |
| `Time.Ticking.TimeSkip`, `AdvanceResult` | ข้าม 1 สัปดาห์ / ถึงต้นเดือนหน้า / ถึงต้นปีหน้า + ผลลัพธ์ |
| `Time.Ticking.TimeSystem` | วนรันสัปดาห์ เรียง Phase ตาม Order และ Publish event |
| `Time.Ticking.Phases.TimeAllocationPhase` | ผลของ Time Budget |
| `Time.Ticking.Phases.NaturalDecayPhase` | Stat decay, Skill decay, Flag หมดอายุ |
| `Time.Ticking.Phases.WeeklyRecordPhase` | บันทึกค่าสำหรับ Sparkline + ตัด ChangeLog เก่า |
| `Time.Ticking.TimeSystemFactory` | ประกอบ TimeSystem พร้อม Phase มาตรฐานจาก content (Bootstrap และ Simulator ใช้ร่วมกัน) |
| Events | `WeekCompletedEvent` (มี `WasMonthStart`/`WasYearStart`), `TimeAdvancedEvent`, `TimeAllocationAppliedEvent` (สรุปรายสัปดาห์), `CashShortfallEvent` |

### กติกา Interrupt
- `AdvanceResult.Interrupts` = คำขอทั้งหมดระหว่างการข้ามเวลา (รวมระดับ Minor/Normal) ใช้แสดงใน Inbox
- `StoppedBy` = คำขอที่ทำให้หยุด **ก่อนครบ** ถ้าคำขอเกิดในสัปดาห์สุดท้ายพอดี การข้ามเวลาถือว่าครบแล้ว (`WasInterrupted = false`) แต่คำขอนั้นยังอยู่ใน `Interrupts`

## 5. Save DTO
```
TimeStateDto { int CurrentWeek; List<TimeAllocationDto { string ActivityId; int Hours }> Allocations }
```
ถ้าเจอกิจกรรมที่ไม่มีใน content แล้ว ระบบจะข้ามพร้อมเขียน warning (ชั่วโมงนั้นกลายเป็นเวลาว่าง)

## 6. ส่งต่อ
- ข้อ 07: `DelayedConsequencesPhase` (300) และ `EventRollPhase` (400) ซึ่งส่ง Interrupt ตาม Severity ของเหตุการณ์
- ข้อ 09: `DeathCheckPhase` (600) และ budget เริ่มต้นตามช่วงวัย (ทารกยังแบ่งเวลาเองไม่ได้)
- ข้อ 10: Phase รายเดือน (รายได้/ค่าใช้จ่าย) และรับ `CashShortfallEvent` ไปทำเป็นหนี้
- ข้อ 11: เวลาที่ถูกผูกไว้ (เรียน/ทำงาน) → ใช้ `minHours` ผ่าน Condition หรือเพิ่ม commitment
- ข้อ 12: `NpcUpdatePhase` (500)
- ข้อ 20: ให้ผู้เล่นตั้งเกณฑ์ Interrupt ในหน้าตั้งค่า

## 7. Content เริ่มต้น
8 กิจกรรม: `sleep` (28–84 ชม.), `free_time` (default), `chores`, `exercise`, `study`, `socialize` (30 บาท/ชม.), `entertainment` (10 บาท/ชม.), `side_project`
budget เริ่มต้น: นอน 56, งานบ้าน 7, บันเทิง 10, เข้าสังคม 5, ที่เหลือเป็นเวลาว่าง 90
ตัวเลขเหล่านี้เป็นค่าตั้งต้น จะปรับสมดุลจริงในข้อ 21

## 8. Tests
- Core 34 test ใหม่:
  - TimeBudget: set, ขอบเขต, validate 5 แบบ, effective allocation, ตัวคูณการเรียนรู้
  - Phase: ผลรายชั่วโมงพร้อมเพดาน, ตัวคูณการเรียนรู้, ค่าใช้จ่ายและยอดขาด, decay ไม่ข้ามเป้า, Trait มีผลกับ decay, skill/flag หมดอายุ, ลำดับ Phase
  - TimeSystem: cadence รายสัปดาห์/เดือน/ปี, ความยาวการข้ามเวลา, Interrupt ระดับต่างๆ, event
  - Loader: กิจกรรมและ time settings ผิด 7 แบบ
  - Save mapper
- Content 1 test ใหม่: จำลอง 10 ปีด้วย content จริง ค่าทุกตัวต้องอยู่ในช่วง (หลังจำลอง: fatigue 11.8, health 77.1, mental 91.6)
