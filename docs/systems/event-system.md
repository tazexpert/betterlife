# Task 07: Event System

> สถานะ: ✅ **Implemented** — 2026-11-08 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
เหตุการณ์ชีวิตที่เขียนใน JSON ล้วนๆ: มีเงื่อนไข (Rules), น้ำหนัก + ตัวคูณน้ำหนัก, Cooldown, Severity และ Choices
(แต่ละตัวเลือกมีเงื่อนไข / โอกาสสำเร็จ (FinalChance จากข้อ 06) / ผลเมื่อสำเร็จและล้มเหลว)
Butterfly Effect = Story Flags (ข้อ 02) + Delayed consequences (`event.schedule`) + Storylines
Anti-frustration ลดน้ำหนักเหตุการณ์ร้ายชั่วคราวเมื่อเจอติดกัน และหนี้กรรม (ข้อ 06) ทำให้เกิดเหตุร้าย

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ |
|---|---|
| Namespace | `BetterLife.Core.LifeEvents` (คำว่า "Event" ใน `Core.Events` หมายถึงข้อความบน EventBus) |
| ชนิดการเกิด (`trigger`) | `random` (สุ่มจาก pool), `scheduled` (เกิดจาก `event.schedule` เท่านั้น), `karmic_debt` (เกิดเมื่อหนี้กรรมครบกำหนด) |
| ความถี่ | ทุกสัปดาห์มีโอกาส 15% ที่จะสุ่มเหตุการณ์จาก pool (≈ 8 ครั้ง/ปี) ส่วนเหตุการณ์ที่ตั้งเวลาไว้ และเหตุจากหนี้กรรม เกิดตามกำหนดเสมอ |
| เหตุจากหนี้กรรม | เลือกจาก pool `karmic_debt` เฉพาะเหตุการณ์ที่ช่วง `debtAmount` ครอบคลุมขนาดของหนี้ (หนี้ใหญ่ = เหตุร้ายใหญ่) |
| Inbox แบบ FM | เหตุการณ์ที่มีตัวเลือกจะเข้า Inbox ถ้าระดับ Major/Critical จะหยุดการข้ามเวลา ส่วนระดับต่ำกว่านั้นรอได้ 4 สัปดาห์ ถ้าไม่ตอบ ระบบจะเลือก `defaultChoice` ให้อัตโนมัติ |
| เหตุการณ์ไม่มีตัวเลือก | ผล `onFire` เกิดทันที แล้วแจ้งใน Inbox (ไม่ต้องตอบ) |
| Cooldown / จำนวนครั้ง | `cooldownWeeks` และ `maxOccurrences` (ไม่บังคับ) ใช้กับ random และ karmic ส่วน scheduled ใช้แค่ `maxOccurrences` |
| Anti-frustration | ถ้าเหตุการณ์ล่าสุด ≥ 2 ครั้งติดกัน (ภายใน 12 สัปดาห์) เป็นเหตุร้าย น้ำหนักของเหตุร้ายจะ × 0.4 จนกว่าจะเจอเรื่องดีหรือกลางๆ |
| Delayed consequences | Effect `event.schedule` ส่งคำขอผ่าน `EffectResult.Messages` (Effect ไม่รู้จัก state ของเกม) แล้ว EventSystem รับไปตั้งเวลา ระยะหน่วงอย่างน้อย 1 สัปดาห์ และถ้าเงื่อนไขไม่ผ่านตอนครบกำหนด เหตุการณ์จะหายไปเงียบๆ |
| Storyline | ป้าย `storyline` บนเหตุการณ์ (ใช้จัดกลุ่มใน Timeline ข้อ 17) ส่วนการต่อเรื่องใช้ Flag + Condition + `event.schedule` |
| ที่มาใน Breakdown | ผลของเหตุการณ์ใช้ `titleKey` เป็นที่มา เช่น "Stress +6 จาก: หัวหน้าขอให้ทำ OT" |
| Random stream | `events.roll` (สุ่มว่าเกิดอะไร) แยกจาก `events.choice` (ทอยโอกาสและช่วงตัวเลข) การตัดสินใจของผู้เล่นจึงไม่เปลี่ยนว่าเหตุการณ์ไหนจะเกิดในอนาคต |

## 3. JSON Schema

### `content/events/*.json` (`events`) → `events.schema.json`
```json
{ "events": [ {
  "id": "career.overtime_request",
  "titleKey": "event.career.overtime_request.title", "bodyKey": "event.career.overtime_request.body",
  "category": "career", "trigger": "random", "severity": "normal", "tone": "neutral",
  "storyline": null,
  "condition": { "type": "age", "op": ">=", "value": 18 },
  "weight": { "base": 10, "modifiers": [ { "condition": { "type": "stat", "stat": "mental.stress", "op": "<", "value": 40 }, "multiplier": 1.5 } ] },
  "cooldownWeeks": 26, "maxOccurrences": null, "debtAmount": null,
  "onFire": [],
  "defaultChoice": "decline",
  "choices": [
    { "id": "accept", "textKey": "event.career.overtime_request.accept", "condition": null, "hideWhenLocked": false,
      "chance": null,
      "success": { "resultKey": "event.career.overtime_request.accept.result", "effects": [ { "type": "cash", "amount": 3000 } ] },
      "failure": null },
    { "id": "decline", "textKey": "…", "chance": { "base": 60, "stats": [ { "stat": "hidden.charisma", "weight": 0.4 } ] },
      "success": { "resultKey": "…", "effects": [] },
      "failure": { "resultKey": "…", "effects": [ { "type": "reputation", "domain": "workplace", "amount": -5 } ] } }
  ] } ] }
```
**กติกาที่ Loader ตรวจ:**
- id ไม่ซ้ำข้ามไฟล์
- ตัวเลือกที่มี `chance` ต้องมี `failure` และตัวเลือกที่ไม่มี `chance` ห้ามมี `failure`
- `defaultChoice` ต้องมีอยู่จริง (บังคับเมื่อมีตัวเลือก)
- `event.schedule` อ้างถึงเหตุการณ์ที่มีจริง (ตรวจหลังอ่าน id จากทุกไฟล์ครบแล้ว)
- `debtAmount` มีได้เฉพาะ `karmic_debt`

### `content/event_settings.json` (`event_settings`, ต้องมีไฟล์เดียว)
```json
{ "weeklyEventChance": 0.15, "responseWindowWeeks": 4,
  "antiFrustration": { "streakThreshold": 2, "windowWeeks": 12, "negativeWeightMultiplier": 0.4 } }
```

### Effect ใหม่
`{ "type": "event.schedule", "event": "social.old_friend_business_offer", "delayWeeks": { "min": 26, "max": 52 } }`

## 4. C# Domain (`Runtime/LifeEvents/`)
| Class | หน้าที่ |
|---|---|
| `EventTrigger`, `EventTone`, `EventOrigin` | enum |
| `WeightModifier`, `EventWeight` | น้ำหนักพื้นฐาน × ตัวคูณที่เงื่อนไขผ่าน |
| `EventOutcome` | `ResultKey` + `EffectSequence` |
| `EventChoice` | id, textKey, condition, hideWhenLocked, chance, success, failure |
| `LifeEventDefinition` | นิยามเหตุการณ์ |
| `EventSettings`, `AntiFrustrationSettings` | ค่าตั้งจาก JSON |
| `EventRecord`, `ScheduledEvent`, `PendingEvent`, `EventHistoryEntry`, `ToneEntry` | สถานะ |
| `EventState` | สถานะทั้งหมดของชีวิต (เซฟได้) |
| `ChoiceView` | สิ่งที่ UI แสดงต่อตัวเลือก: ใช้ได้ไหม, เหตุผลที่ถูกล็อก, โอกาสสำเร็จ + Breakdown, พรีวิวผล |
| `EventResolution` | ผลการเลือก: ทอยได้เท่าไร, สำเร็จหรือไม่, `EffectResult` |
| `EventSystem` | สุ่ม/ตั้งเวลา/เกิดเหตุการณ์, Inbox, `GetChoices`, `Resolve`, ตอบอัตโนมัติเมื่อหมดเวลา |
| `EventRollPhase` | Tick ลำดับ 400 |
| `ScheduleEventEffect`, `ScheduleEventRequest` | Delayed consequences |
| `LifeEventFiredEvent`, `LifeEventResolvedEvent` | ข้อความบน EventBus |
| `EventSystemFactory` | ประกอบระบบจาก content |

**แก้ไขข้อ 03:** `EffectResult.Messages` (รายการ `IGameEvent`) เป็นธรรมเนียมใหม่ว่า "ใครเป็นคน Apply effect ต้อง Publish messages ด้วย" (มีตัวช่วย `EffectResult.PublishTo(bus)`) และ `CashEffect` ส่ง `CashShortfallEvent` ผ่านช่องทางนี้ด้วย

## 5. Save DTO
```
EventStateDto
  Records:   [{ EventId, LastFiredWeek, Occurrences }]
  Scheduled: [{ EventId, DueWeek }]
  Inbox:     [{ InstanceId, EventId, Origin, FiredWeek, DeadlineWeek }]
  Tones:     [{ Week, Tone }]
  History:   [{ InstanceId, EventId, Origin, Week, ChoiceId, Success }]
  NextInstanceId
```
ถ้าเจอเหตุการณ์ที่ถูกลบออกจาก content แล้ว ระบบจะข้ามพร้อมเขียน warning

## 6. Content เริ่มต้น (9 เหตุการณ์ ใน `content/events/`)
| ไฟล์ | เหตุการณ์ | สิ่งที่สาธิต |
|---|---|---|
| `daily.json` | ทำกระเป๋าเงินหาย | เหตุการณ์ประกาศ (ไม่มีตัวเลือก) และช่วงตัวเลข |
| | งานวัด | ทำบุญ (`karma.merit`), ตัวเลือกที่ถูกล็อกด้วยเงิน, น้ำหนัก ×2 เมื่อมีกรรมชั่วสูง |
| `career.json` | หัวหน้าขอให้ทำ OT | ทอยโอกาส (Charisma + ดวงการงาน) และผลเมื่อล้มเหลว |
| `health.json` | ไข้หวัดระบาด | `healthCondition` + โอกาสที่ขึ้นกับ Fitness และดวงสุขภาพ |
| `social.json` | เจอเพื่อนเก่า → ชวนลงทุน (26–52 สัปดาห์ต่อมา) | Storyline + Flag + `event.schedule` + ระดับ Major |
| `karma.json` | เคราะห์เล็ก / อุบัติเหตุ / อุบัติเหตุ (เด็ก) | เหตุจากหนี้กรรมตามขนาดหนี้และอายุ |

## 7. Tests
- Core 26 test ใหม่:
  - เหตุการณ์ประกาศ
  - Inbox และ ChoiceView: ล็อก / ซ่อน / โอกาส / พรีวิว
  - Resolve และ error 3 แบบ
  - Delayed consequence และเหตุการณ์ที่ตั้งเวลาไว้แต่เงื่อนไขไม่ผ่านแล้ว
  - ตอบอัตโนมัติเมื่อหมดเวลา
  - น้ำหนัก / ตัวคูณ / cooldown / จำนวนครั้ง (2,000 สัปดาห์)
  - ความถี่รายสัปดาห์ และ Anti-frustration (สัดส่วนตรงกับสูตร)
  - หนี้กรรมตามขนาด
  - การเลือกของผู้เล่นไม่เปลี่ยนเหตุการณ์ในอนาคต
  - Interrupt หยุดการข้ามเวลา
  - Save round-trip และ Dispose
  - Loader ตรวจผิด 10 แบบ + `event.schedule` ข้ามไฟล์
- Content: smoke test 10 ปีรวม Event System ด้วย content จริง (เกิด 4.8 เหตุการณ์ต่อปี)

## 8. ส่งต่อ
- ข้อ 09: เงื่อนไขช่วงวัย (`lifeStage`) และเหตุการณ์การตาย
- ข้อ 13: เหตุการณ์ของอบายมุข (temptation weight จะเป็น weight modifier ชนิดใหม่)
- ข้อ 14: Undo snapshot (`EventState` + Character)
- ข้อ 16: UI ของ Inbox และ Event Dialog ใช้ `PendingEvent` + `ChoiceView`
- ข้อ 22: เขียนเหตุการณ์เป็นชุด ชุดละ 30–50 อัน
