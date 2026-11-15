# Task 06: Astrology + Karma + LuckCalculator

> สถานะ: ✅ **Implemented** — 2026-11-08 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **ดวง 5 ด้าน** (การเงิน ความรัก สุขภาพ การงาน การเรียน) ค่า 0–100 คำนวณจาก ราศี + วันเกิดแบบโหราไทย (รวมพุธกลางคืน) + ธาตุปีนักษัตร
  และมีรอบดวงขึ้น/ตกรายปี
- **กรรม:** กรรมดีกับกรรมชั่วเก็บแยกกัน ไม่หักลบกัน กรรมชั่วส่วนหนึ่งกลายเป็น **หนี้กรรม** ที่ย้อนกลับมาภายหลัง ส่วนการทำบุญลดหนี้กรรมได้บางส่วน
- **FinalChance = Base + Stat + Luck + Karma + Item** แล้ว Clamp 5–95% โดยไอเท็ม (IAP) ช่วยได้ไม่เกิน +5% (ไม่ Pay-to-Win)
- ทุกตัวเลขมี Breakdown (Design Pillar ข้อ 1)

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ราศี | ราศีแบบไทย (นิรายนะ): เมษ เริ่ม 13 เม.ย. … มีน เริ่ม 14 มี.ค. วันเริ่มของแต่ละราศีอยู่ใน JSON | ตรงกับโหราไทยที่ผู้เล่นคุ้นเคย |
| ปีนักษัตร | นับตามปีปฏิทิน (1 ม.ค.) โดยปีอ้างอิงคือ 2020 = ชวด (ตั้งค่าได้) | ทำให้ง่าย เพราะตำราไทยใช้วันเปลี่ยนปีนักษัตรไม่ตรงกัน (สงกรานต์ / ตรุษจีน / 1 ธ.ค.) |
| ธาตุ | 5 ธาตุของปีนักษัตรแบบไทย: ดิน น้ำ ไฟ ไม้ ทอง | |
| รอบดวงรายปี | คลื่น sine (คาบ 12 ปี, แอมพลิจูด 8) โดยแต่ละด้านมีจังหวะต่างกัน + ความผันผวนรายปี ±5 | ดวงขึ้นลงเป็นรอบที่คาดเดาได้บางส่วน (ดวงไม่ได้ตัดสินทุกอย่าง) |
| ความผันผวนรายปี | สุ่มจาก id ตัวละคร + ปี + ด้าน (ไม่ต้องเซฟ ได้ค่าเดิมทุกครั้ง) | โหลดเซฟแล้วดวงไม่เปลี่ยน และ Undo ไม่ทำให้ดวงเปลี่ยน |
| Luck term | (ดวง − 50) × 0.2 = −10 ถึง +10 จุด | ดวงมีผล แต่ไม่ล้นจน Stat ไม่มีความหมาย |
| Karma term | กรรมดี × 0.1 − กรรมชั่ว × 0.1 แล้ว Clamp ±10 (ค่ากรรมแต่ละฝั่งยังเก็บแยกกัน) | |
| หนี้กรรม | 50% ของกรรมชั่วแต่ละครั้งกลายเป็นหนี้ ที่ครบกำหนดใน 26–260 สัปดาห์ (สุ่ม) เมื่อครบกำหนดจะ Publish `KarmicDebtDueEvent` และ Event System (ข้อ 07) เลือกเหตุการณ์ร้ายตามขนาดหนี้ | Butterfly Effect: ทำชั่ววันนี้ ผลมาทีหลัง |
| ทำบุญ | เพิ่มกรรมดีเต็มจำนวน และลดหนี้กรรมเก่าสุดก่อน 50% ของยอดทำบุญ (กรรมชั่วสะสมไม่ลด) | "ชดเชยได้บางส่วน" ตาม GDD |
| Item | รวมโบนัสจากแหล่งไอเท็ม (`IChanceBonusSource`) แล้ว Clamp 0–5 | GDD: เพดาน +5% |
| ที่เก็บกรรม | ใน `Character` (`KarmaLedger`) | ข้อ 18 ส่งต่อกรรมให้ลูกหลานได้ |
| Rule ที่ต้องใช้ content | Loader ของดวงและของ luck ลงทะเบียน converter ของ `fortune` / `karma.*` เอง และเพิ่ม loader ตัวกลาง `rule_types` ให้ content ที่มี rule รอจนชนิด rule ครบก่อน | content ที่เพิ่มทีหลังใช้ Condition/Effect ใหม่ได้โดยไม่ต้องแก้ Engine |

## 3. JSON Schema

### `content/astrology.json` → `astrology.schema.json`
```json
{ "baseFortune": 50,
  "cycle": { "periodYears": 12, "amplitude": 8,
             "areaPhaseYears": { "finance": 0, "love": 3, "health": 6, "career": 9, "study": 1.5 } },
  "yearlyNoise": 5,
  "yearAnimalAnchorYear": 2020,
  "zodiacSigns": [ { "id": "aries", "nameKey": "zodiac.aries.name", "startMonth": 4, "startDay": 13,
                     "modifiers": { "career": 4, "love": -2 } } ],
  "birthDays":   [ { "day": "wednesday_night", "nameKey": "thaiday.wednesday_night.name", "modifiers": { "finance": 3 } } ],
  "elements":    [ { "id": "water", "nameKey": "element.water.name", "modifiers": { "love": 3 } } ],
  "yearAnimals": [ { "id": "rat", "nameKey": "animal.rat.name", "element": "water" } ] }
```
**Fortune(ด้าน, ปี)** = base + ราศี + วันเกิด + ธาตุ + amplitude·sin(2π·(ปี + phase) / period) + noise → Clamp 0–100
โดย phase = ลำดับราศี × period/12 + areaPhaseYears[ด้าน]

### `content/luck.json` (`luck_settings`) → `luck_settings.schema.json`
```json
{ "chance":  { "min": 5, "max": 95, "itemBonusCap": 5 },
  "fortune": { "neutral": 50, "pointsPerFortune": 0.2 },
  "karma":   { "goodWeight": 0.1, "badWeight": 0.1, "cap": 10,
               "debtRatio": 0.5, "debtDelayWeeks": { "min": 26, "max": 260 }, "meritOffsetRatio": 0.5 } }
```

### Rule ใหม่ (เพิ่มลงใน `rules.schema.json`)
| ชนิด | JSON |
|---|---|
| Condition `fortune` | `{ "type": "fortune", "area": "career", "op": ">=", "value": 60 }` |
| Condition `karma` | `{ "type": "karma", "kind": "bad", "op": ">", "value": 30 }` |
| Effect `karma.good` | `{ "type": "karma.good", "amount": 5 }` |
| Effect `karma.bad` | `{ "type": "karma.bad", "amount": 8 }` (ตั้งหนี้กรรมให้อัตโนมัติ) |
| Effect `karma.merit` | `{ "type": "karma.merit", "amount": 10 }` (ทำบุญ) |

### ChanceSpec (ใช้ในตัวเลือกของเหตุการณ์ ข้อ 07)
```json
"chance": { "base": 40, "stats": [ { "stat": "intelligence", "weight": 0.3 } ], "fortune": "study", "karma": true }
```
Stat term = Σ (ค่า Stat − 50) × weight

## 4. C# Domain
| Namespace | Class |
|---|---|
| `Astrology` | `FortuneArea`, `FortuneAreas` (ชื่อใน JSON), `FortuneModifiers`, `ZodiacSignDefinition`, `BirthDayDefinition`, `ElementDefinition`, `YearAnimalDefinition`, `FortuneCycle`, `AstrologyTables`, `AstrologyProfile`, `FortuneBreakdown`, `FortuneCalculator`, `FortuneCondition` |
| `Karma` | `KarmaLedger` (ใน Character), `KarmicDebt`, `KarmaSettings`, `KarmaService`, `KarmicDebtPhase` (Tick 300), `KarmicDebtDueEvent`, `KarmaCondition`, `GoodKarmaEffect`, `BadKarmaEffect`, `MeritEffect` |
| `Luck` | `LuckSettings`, `ChanceSpec`, `ChanceStatTerm`, `ChanceSpecReader`, `IChanceBonusSource`, `ChanceBreakdown`, `ChanceRoll`, `LuckCalculator` |
| Content | `AstrologyLoader`, `LuckSettingsLoader`, `RuleTypesBarrier` |

## 5. Save DTO
`CharacterDto.Karma = KarmaDto { float Good; float Bad; List<KarmicDebtDto { float Amount; int DueWeek; string SourceKey }> Debts }`
ดวงไม่ต้องเซฟ เพราะคำนวณจากวันเกิดและ id ได้ทุกครั้ง

## 6. Tests
- Core 45 test ใหม่:
  - ราศี: วันเริ่มของแต่ละราศี และการวนข้ามปี
  - ปีนักษัตร: นับถอยหลังและไปข้างหน้าจากปีอ้างอิง
  - Profile (รวมพุธกลางคืน) และตาราง content ไม่ครบ
  - ดวง: modifier, รอบ 12 ปี, ความผันผวนรายปีได้ค่าเดิมทุกครั้งและอยู่ในช่วง, Clamp
  - กรรม: แยกดี/ชั่ว, หนี้กรรม, ทำบุญจ่ายหนี้เก่าสุดก่อน, karma term, Phase หนี้กรรม
  - FinalChance: ครบ 5 term, Clamp 5–95, เพดาน Item 5%, อัตราสำเร็จจริงตรงกับโอกาส
  - ChanceSpec reader
  - Rule ใหม่: round-trip, การมองเห็น, การเซฟกรรม, เซฟเก่าที่ไม่มีกรรม
- Content 1 test ใหม่: สุ่มคน 400 คน × 24 ปี ดวงทุกด้านมีค่าเฉลี่ย 51–52 และอยู่ในช่วงประมาณ 34–70 (ไม่มีใครดวงดีหรือดวงตกถาวร)

## 7. Unity / Composition
`TimeSystemFactory` ใส่ `KarmicDebtPhase` ให้อัตโนมัติแล้ว ส่วน `LuckCalculator` ให้ประกอบใน Bootstrap:
`new LuckCalculator(repo.Get<LuckSettings>(), repo.Get<FortuneCalculator>(), repo.Get<KarmaService>(), itemSources)`

## 8. ส่งต่อ
- ข้อ 07: ใช้ `ChanceSpecReader` + `LuckCalculator.Roll` ในตัวเลือก และรับ `KarmicDebtDueEvent` ไปเลือกเหตุการณ์ร้าย
- ข้อ 13: Beginner's luck ในการพนัน (เพิ่ม `IChanceBonusSource` ที่ไม่ถูกจำกัดด้วยเพดาน Item)
- ข้อ 17: หน้าดวง (แสดง `FortuneBreakdown`)
- ข้อ 18: กรรมตกทอด
- ข้อ 19: ไอเท็มเครื่องรางเป็น `IChanceBonusSource`
