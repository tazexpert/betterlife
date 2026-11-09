# Task 13b: Scam System (มิจฉาชีพ)

> สถานะ: ✅ **Implemented** — 2026-11-09 (ผู้ใช้ขอเพิ่มต่อจากระบบอบายมุข ใช้ตัวเลือกที่แนะนำ)

## 1. สรุปความเข้าใจ
- ตัวละครอาจถูกมิจฉาชีพเล็งเป้าได้ตลอดชีวิต แบบที่พบบ่อยในไทย:
  แก๊งคอลเซ็นเตอร์อ้างเป็นตำรวจ/ไปรษณีย์, SMS/ลิงก์ดูดเงิน, ร้านค้าออนไลน์ปลอม, หลอกให้รัก (Romance scam), หลอกลงทุน/แชร์ลูกโซ่,
  หลอกทำงาน, หลอกปล่อยกู้ (เก็บค่าธรรมเนียมก่อน), หลอกว่าถูกรางวัล, หลอกเด็กในเกมออนไลน์
- คนที่ถูกเล็งเป้ามากขึ้น: ผู้สูงอายุ คนเหงา คนที่ใช้เวลาออนไลน์เยอะ คนมีเงินมาก คนเป็นหนี้ คนตกงาน
- จับได้หรือไม่ขึ้นกับ **ความรู้เท่าทันมิจฉาชีพ** (Hidden stat ใหม่ `hidden.street_smarts`), Knowledge, ความเครียด และคนใกล้ตัวที่ช่วยเตือน
- ถูกหลอกแล้วเสียเงิน (เงินสดและเงินฝาก) เครียด ได้บทเรียน (street smarts เพิ่ม) และ **แจ้งความ/แจ้งธนาคาร** เพื่อขอเงินคืนได้ โอกาสลดลงทุกสัปดาห์ที่ช้า
- มิจฉาชีพแบบที่ต้องตัดสินใจเอง (ชวนลงทุน แฟนออนไลน์ขอเงิน) อยู่ในระบบเหตุการณ์ (Inbox) และใช้ Effect ใหม่ `scam.loss`

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| สองช่องทาง | 1) **อัตโนมัติรายเดือน** (`ScamService`): ถูกเล็งเป้า → ทอยโอกาสจับได้ → ถ้าพลาดจะเสียเงินทันที 2) **เหตุการณ์ที่ต้องตัดสินใจ** ใน `events/scams.json` | กลอุบายส่วนใหญ่หลอกให้ตัดสินใจผิดในเวลาสั้นๆ ส่วนการลงทุน/ความรักเป็นการเลือกของผู้เล่นจริง |
| โอกาสถูกเล็งเป้า/เดือน | `monthlyChance` × ตัวคูณช่วงอายุ × (1 + Loneliness/100 × `lonelinessWeight`) × ออนไลน์ (1 + 0.02 × ชั่วโมงบันเทิง+สังสรรค์/สัปดาห์ที่เกิน 10, สูงสุด ×2) × ความรวย (`wealthWeight` × log10(เงินสด+เงินฝาก / 10,000)) × ตัวคูณจาก Rule condition (เช่น มีหนี้นอกระบบ ×3, ตกงาน ×2) พร้อม Breakdown | อ่านตัวเลขได้ทุกตัว |
| โอกาสจับได้ | FinalChance จาก `detection` ของแต่ละแบบ (ใช้ street smarts / Knowledge / IQ) − 0.2 × (Stress − 50 ถ้าเกิน) + 5 ต่อคนใกล้ตัวที่สนิท (≥ 50, สูงสุด +15) | คนเครียดถูกหลอกง่าย คนมีคนเตือนรอดง่าย |
| เงินที่เสีย | `cashShare` ของเงินสด + `savingsShare` ของเงินฝาก (สินทรัพย์ประเภท deposit) อยู่ในช่วง `min`–`max` (บาทมูลค่าจริง) ถ้าเงินไม่พอ `min` จะเสียเท่าที่มี | ไม่สร้างหนี้จากการถูกหลอก (หนี้มาจากการพนัน/หนี้นอกระบบอยู่แล้ว) |
| บทเรียน | ถูกหลอก: street smarts +15, จับได้: +3 | ประสบการณ์ทำให้ระวังขึ้น |
| แจ้งความ | `Report(incidentId)` ได้ครั้งเดียวต่อเหตุการณ์ ภายใน `windowWeeks` โอกาส = `recovery.chance.base` − `decayPerWeek` × สัปดาห์ที่ผ่านไป (FinalChance) สำเร็จได้คืน `recovery.share` ของเงินที่เสีย | ยิ่งแจ้งเร็วยิ่งมีโอกาส |
| เด็ก | หลอกในเกมออนไลน์เริ่มอายุ 9 (เสียเงินค่าขนม) | |
| ที่เก็บ | `CharacterScams` (เหตุการณ์ถูกหลอก สูงสุด 20 รายการล่าสุด + จำนวนครั้งที่ถูกเล็ง/จับได้) อยู่ใน Character | Rule อ่านได้ |
| Phase | `scam_month` รายเดือน ลำดับ 260 (หลังรับเงินเดือน) | เงินเพิ่งเข้า = เป้าหมายใหญ่ขึ้น |
| Rule ใหม่ | Effect `{ "type": "scam.loss", "scam": "ponzi_scheme", "cashShare": 0.5, "min": 5000, "max": 2000000 }` และ Condition `{ "type": "scam", "measure": "victims" \| "lost" \| "detected", "op": ">=", "value": 1 }` | ให้เหตุการณ์ใช้ได้ |
| Random stream | `scams` | |
| ค่าที่ปรับหลังจำลอง | โอกาสพื้นฐาน 0.3–1%/เดือนต่อแบบ, ส่วนที่เสีย 5–15% ของเงินสด/เงินฝาก, เพดานแบบใหญ่ 500,000–800,000 บาท | แบบแรกชีวิตที่รวยเสียเงินรวมตลอดชีวิตเฉลี่ย 10 ล้าน (แต่ละครั้งเสีย 40–60%) |
| หยุดเวลา | เสียเงิน ≥ 20,000 บาท = Major (หยุดการข้ามเวลา) ถ้าน้อยกว่านั้น = Normal (แจ้งใน Inbox แต่ไม่หยุด) | จากการจำลอง: ผู้สูงอายุถูกเล็งเป้าบ่อย ถ้าหยุดทุกครั้ง การข้ามทีละปีจะถูกตัดตลอด |

## 3. JSON Schema (content ใหม่ 2 ชนิด + stat 1 + เหตุการณ์ 2)
```jsonc
// scams.json
{ "scams": [ { "id": "call_center_police", "nameKey": "scam.call_center_police.name", "channel": "phone", "minAge": 18,
    "monthlyChance": 0.006, "ageBands": [ { "minAge": 60, "multiplier": 2.5 } ], "lonelinessWeight": 0.5, "wealthWeight": 0.3,
    "modifiers": [ { "condition": { "type": "flag", "flag": "finance.loan_shark" }, "multiplier": 3 } ],
    "detection": { "base": 55, "stats": [ { "stat": "hidden.street_smarts", "weight": 0.6 } ], "fortune": "finance" },
    "loss": { "cashShare": 0.1, "savingsShare": 0.1, "min": 5000, "max": 500000 },
    "effects": [ { "type": "stat", "stat": "mental.stress", "amount": 15 } ],
    "recovery": { "chance": { "base": 40 }, "decayPerWeek": 8, "windowWeeks": 8, "share": 0.5 } } ] }

// scam_settings.json (ไฟล์เดียว)
{ "onlineActivities": [ "entertainment", "socialize" ], "onlineBaselineHours": 10, "perOnlineHour": 0.02, "maxOnline": 2,
  "wealthReference": 10000, "stressPenaltyPerPoint": -0.2, "support": { "minValue": 50, "perTie": 5, "max": 15 },
  "lessonGain": 15, "detectedGain": 3, "maxIncidents": 20, "maxMonthlyChance": 0.5, "helplineKey": "scam.helpline", "majorLoss": 20000 }
```

## 4. C# Domain (`BetterLife.Core.Scams`)
| กลุ่ม | Class |
|---|---|
| Content | `ScamDefinition` (+ `ScamChannel`, `ScamLoss`, `ScamRecovery`, `ScamModifier`), `ScamSettings` |
| ข้อมูล | `ScamIncident`, `CharacterScams` |
| คำอธิบายตัวเลข | `ScamTargetingBreakdown`, `ScamDetectionQuote`, `ScamReportResult` |
| บริการ | `ScamService`, `ScamMonthPhase` |
| Events | `ScamDetectedEvent`, `ScammedEvent`, `ScamReportedEvent` |
| Rules | `ScamLossEffect`, `ScamCondition` |

## 5. Save DTO
`CharacterDto.Scams = ScamsDto { Incidents[{Id, ScamId, Week, Lost, Recovered, Reported}], Targeted, Detected, NextIncidentId }` (เงินเป็น decimal string)

## 6. Tests
- `ScamServiceTests` (8): ตัวคูณการเล็งเป้าครบ (อายุ/เหงา/ออนไลน์/เงิน/สถานการณ์ + เพดาน), โอกาสจับได้ (ความเครียด/คนเตือน), ถูกหลอก: เงินสดก่อนแล้วเงินฝาก + บทเรียน + Normal interrupt, เสียมาก = Major และขั้นต่ำไม่เกินเงินที่มี, คนรู้ทันจับได้มากกว่า, เด็กไม่ถูกเล็งด้วยกลโกงผู้ใหญ่, แจ้งความ (คืนครึ่ง/ครั้งเดียว/ต้องทันเวลา), เก็บเหตุการณ์ล่าสุด
- `ScamRuleTests` (5), `ScamPersistenceAndContentTests` (6), `LifeSessionTests` (มี Phase `scam_month`)
- `ScamSimulationTests` (content จริง 30 ชีวิตถึงอายุ 75): เคยถูกหลอก 28/30, ถูกหลอกต่อปี: วัย 18–59 = 0.13, 60+ = 0.20, เสียรวมตลอดชีวิต (มัธยฐาน) ≈ 1.8 ล้าน (ชีวิตจำลองรวยและไม่เคยแจ้งความ) แยกตามแบบ: หลอกลงทุนและแก๊งคอลเซ็นเตอร์เสียมากที่สุด

## 7. ส่งต่อ
- ข้อ 16–17: Inbox แจ้ง "ถูกหลอก" + ปุ่มแจ้งความ, หน้าข้อมูลรูปแบบมิจฉาชีพ (`TargetingBreakdown`, `DetectionQuote`)
- ข้อ 18: คำไว้อาลัย/สรุปชีวิตจากยอดที่ถูกหลอก
- ข้อ 20: หน้าข้อมูลสายด่วน (เช่น ศูนย์รับแจ้งอาชญากรรมออนไลน์) เป็น Loc key `scam.helpline`
- ข้อ 21: ปรับสมดุลความถี่/มูลค่า (ตอนนี้ชีวิตจำลองที่รวยเสียรวมตลอดชีวิตหลักล้าน)
- ข้อ 22: เหตุการณ์มิจฉาชีพเพิ่มเติม
