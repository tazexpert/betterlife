# Task 13: Vice System (อบายมุข 6)

> สถานะ: ✅ **Implemented** — 2026-11-09 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- อบายมุข 6 ตาม GDD:
  1. น้ำเมา/บุหรี่/สารเสพติด (ใช้ชื่อหมวดสมมติ: `drinking`, `smoking`, `synthetic_high`)
  2. เที่ยวกลางคืน (`nightlife`)
  3. หมกมุ่นความบันเทิง (`binge_entertainment`)
  4. การพนัน (`gambling`)
  5. คบคนชั่ว (ตัวคูณ)
  6. เกียจคร้าน (ตัวคูณ)
- แต่ละอย่างให้ผลดีระยะสั้น (คลายเครียด/สนุก ลดลงตาม Tolerance) แต่มีต้นทุนระยะยาว (สุขภาพ เงิน กรรม กฎหมาย การงาน)
  มี Addiction, Tolerance, Withdrawal, Relapse และ Clean streak
- ทุกสัปดาห์: `TemptationWeight = Base × Stress × Addiction × BadCompany × Laziness × LifeStage (× Trait)`
  ถ้าถูกยั่วยุจะทอย `ResistChance = 50 + (Willpower−50)×0.5 − Addiction×0.4 + Support + Karma` (Clamp 5–95%) ถ้าต้านไม่ไหวจะเผลอใช้
- การพนันมี Beginner's luck แต่ EV ติดลบ เมื่อติดหนักจะเล่นต่อแม้ไม่มีเงิน (ยอดขาดจะกลายเป็นหนี้นอกระบบ = Debt spiral)
- ครอบครัวจับได้, วิธีเลิก 5 แบบ (หักดิบ, ค่อยๆ ลด, กลุ่มบำบัด, สถานบำบัด, บวช/ปฏิบัติธรรม)
- ข้อห้าม: ห้ามผูก IAP/Ads กับการพนัน, Undo ใช้ยกเลิกผลพนันไม่ได้, มีตัวเลือกปิดเนื้อหา + Loc key ของหน้าสายด่วนช่วยเหลือ

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ที่เก็บ | `CharacterVices` (สถานะต่ออบายมุข) อยู่ใน Character ส่วน `ViceService` อยู่ใน LifeSession | เหมือนระบบก่อนๆ |
| Phase | `vice_week` รายสัปดาห์ ลำดับ 350 (หลังหนี้กรรม ก่อนสุ่มเหตุการณ์) | |
| การใช้ | 1) ผู้เล่นเลือกใช้เอง (`Indulge`) 2) ชั่วโมงของกิจกรรมที่ผูกไว้ (`nightlife`, `entertainment`) ทุก `hoursPerUse` นับเป็นการใช้ 1 ครั้ง 3) เผลอใช้เพราะต้านความยั่วยุไม่ไหว | เวลาในงบเวลาก็เป็นอบายมุขได้ |
| ผลระยะสั้น | `relief.stress` / `relief.happiness` × (1 − Tolerance/100) | ยิ่งใช้บ่อยยิ่งได้ผลน้อยลง |
| ผลระยะยาว | `useEffects` เป็น Rule effect (เช่น disease_burden, กรรมชั่ว, career.performance) และ `risks` = โอกาสต่อครั้ง + Rule effect (เช่น ถูกจับ) | เพิ่มผลแบบใหม่ได้โดยไม่ต้องแก้โค้ด |
| Addiction | +`gainPerUse` × (1 − Addiction/100) ต่อครั้ง และ −`decayPerCleanWeek` × (ตัวคูณของวิธีเลิก) ต่อสัปดาห์ที่ไม่ได้ใช้ | ไม่เกิน 100 และลดลงได้เสมอ |
| Withdrawal | สัปดาห์ที่ไม่ได้ใช้และ Addiction ≥ `withdrawalFrom`: Stress/Happiness ตาม `withdrawal` × Addiction/100 × ตัวคูณของวิธีเลิก | |
| ลองครั้งแรก | ถ้ายังไม่เคยใช้ ความยั่วยุ × `firstTryMultiplier` (0.3) | |
| ตัวคูณ Stress | 1 + (Stress − 50)/50 × 1.0 (ต่ำสุด 0.2) | |
| ตัวคูณ Addiction | 1 + Addiction/100 × 2 | |
| คบคนชั่ว | 1 + 0.25 × จำนวนเพื่อนที่มีค่า ≥ 30 และมี Trait `reckless` (สูงสุด 2.5) | ใช้ Trait ของ NPC จากข้อ 12 |
| เกียจคร้าน | 1 + 0.02 × ชั่วโมง `free_time` + `entertainment` ที่เกิน 30 ชม./สัปดาห์ (สูงสุด 2) | ใช้งบเวลาจริง |
| Trait ใหม่ | `reckless` (ยั่วยุ ×1.5) / `disciplined` (×0.6) กลุ่ม exclusive `discipline` และมีในพันธุกรรม | |
| Support | +5 ต่อคน (ครอบครัว/คู่/เพื่อนที่ค่า ≥ 50, สูงสุด +20) + โบนัสของวิธีเลิกที่ใช้อยู่ | |
| ต้านได้ | Willpower +0.1 ต่อครั้ง | ฝึกได้ |
| การพนัน | เดิมพัน 500–3,000 × (1 + Addiction/50), โอกาสชนะ 45% จ่าย ×2 (EV −10%), 5 ครั้งแรกโอกาส 65% (Beginner's luck), ชนะเพิ่ม Flag `vice.gambling_wins` | |
| ใช้โดยไม่มีเงิน | ถ้าเงินไม่พอ: Addiction ≥ `chaseFromAddiction` (40) จะใช้ต่อและเกิดยอดขาด (ข้อ 10 แปลงเป็นหนี้) ถ้าน้อยกว่านั้นจะไม่ใช้ | Debt spiral |
| ครอบครัวจับได้ | Addiction ≥ 30 และมีพ่อแม่/คู่สมรสยังมีชีวิต: สุ่ม 3%/สัปดาห์ ครั้งแรกเท่านั้น → Rule effects (ความสัมพันธ์ลด + ความทรงจำ) | |
| วิธีเลิก | ทั้ง 5 แบบอยู่ใน content (`quit_methods.json`): ค่าใช้จ่าย ระยะเวลา โบนัสต้าน ตัวคูณการลด Addiction/Withdrawal เงื่อนไข และผลตอนเริ่ม/สำเร็จ | |
| สำเร็จ/ล้มเหลว | ไม่เผลอใช้จนครบกำหนด = สำเร็จ (Tolerance = 0, Flag `vice.quit_success` +1) ถ้าใช้ระหว่างนั้น = Relapse (Flag `vice.relapse` +1) | |
| Habits card | `ViceState.IsVisible` = Addiction ≥ 10 หรือกำลังเลิก | GDD: การ์ดจะโผล่เมื่อเริ่มติดเท่านั้น |
| ปิดเนื้อหา | `ViceService.ContentEnabled = false`: ไม่มีความยั่วยุ ใช้เองไม่ได้ ไม่มีการจับได้ (Addiction ยังลดลงได้) ส่วนหน้าตั้งค่าเป็นงานข้อ 20 | |
| Undo / IAP | `ViceDefinition.BlocksUndo` (การพนัน = true) ทุกครั้งที่ใช้จะตั้ง `CharacterVices.LastIrreversibleWeek` ข้อ 14 ต้องห้าม Undo ย้อนก่อนสัปดาห์นี้ ข้อ 19 ห้ามผูก Ads/IAP กับ `BlocksUndo` | |
| Random stream | `vices` | |

## 3. JSON Schema (content ใหม่ 3 ชนิด)
```jsonc
// vices.json
{ "vices": [ { "id": "drinking", "nameKey": "vice.drinking.name", "minAge": 18, "blocksUndo": false,
    "activity": null,   // หรือ { "id": "entertainment", "hoursPerUse": 10 }
    "temptation": { "baseWeekly": 0.06, "stages": [ { "stage": "teen", "multiplier": 0.5 } ] },
    "costPerUse": 300, "relief": { "stress": -4, "happiness": 3 },
    "addiction": { "gainPerUse": 2, "decayPerCleanWeek": 0.5, "tolerancePerUse": 1.5, "toleranceDecay": 0.5 },
    "withdrawalFrom": 40, "withdrawal": { "stress": 3, "happiness": -2 },
    "useEffects": [ { "type": "stat", "stat": "health.disease_burden", "amount": 0.2 } ],
    "risks": [ { "chance": 0.002, "reasonKey": "vice.risk.drunk_accident", "severity": "major", "effects": [] } ],
    "discovery": { "fromAddiction": 30, "weeklyChance": 0.03, "effects": [ { "type": "relationship", "relation": "parent", "amount": -10, "target": "all" } ] },
    "gambling": null } ] }   // การพนัน: { "stake": {min,max}, "winChance": 0.45, "payout": 2, "beginnerBets": 5, "beginnerWinChance": 0.65 }

// quit_methods.json
{ "methods": [ { "id": "rehab", "nameKey": "quit.rehab.name", "vices": null, "costCash": 60000, "durationWeeks": 6,
    "resistBonus": 35, "decayMultiplier": 3, "withdrawalMultiplier": 0.3, "condition": null, "startEffects": [], "successEffects": [] } ] }

// vice_settings.json (ไฟล์เดียว)
{ "visibleFromAddiction": 10, "firstTryMultiplier": 0.3, "chaseFromAddiction": 40, "maxWeeklyTemptation": 0.9,
  "stress": { "weight": 1, "minMultiplier": 0.2 }, "addictionWeight": 2,
  "resist": { "chance": { "base": 50, "stats": [ { "stat": "hidden.willpower", "weight": 0.5 } ] }, "perAddiction": -0.4, "willpowerGain": 0.1 },
  "support": { "minValue": 50, "perTie": 5, "max": 20 },
  "badCompany": { "traits": [ "reckless" ], "minValue": 30, "perFriend": 0.25, "max": 2.5 },
  "laziness": { "activities": [ "free_time", "entertainment" ], "baselineHours": 30, "perHour": 0.02, "max": 2 },
  "traits": [ { "trait": "reckless", "multiplier": 1.5 } ], "helplineKey": "vice.helpline" }
```
**Rule ใหม่:** Condition `{ "type": "vice", "vice": "gambling", "measure": "addiction" | "tolerance" | "uses" | "cleanWeeks", "op": ">=", "value": 30 }`
และ Effect `{ "type": "vice.addiction", "vice": "drinking", "amount": 5 }` (ใช้ในเหตุการณ์ เช่น เพื่อนชวน หรือได้กำลังใจ)

## 4. C# Domain (`BetterLife.Core.Vices`)
| กลุ่ม | Class |
|---|---|
| ข้อมูล | `ViceState`, `QuitAttempt`, `CharacterVices` |
| Content | `ViceDefinition` (+ `ViceActivityLink`, `ViceRisk`, `GamblingRules`, `ViceDiscovery`), `QuitMethodDefinition`, `ViceSettings` (+ `ResistRules`, `BadCompanyRules`, `LazinessRules`) |
| คำอธิบายตัวเลข | `TemptationBreakdown`, `ResistQuote`, `QuitQuote`, `ViceUseResult` |
| บริการ | `ViceService`, `ViceWeekPhase` |
| Events | `ViceUsedEvent`, `TemptationResistedEvent`, `GambleResultEvent`, `RelapseEvent`, `ViceDiscoveredEvent`, `QuitSucceededEvent` |
| Rules | `ViceCondition`, `ViceAddictionEffect` |

## 5. Save DTO
`CharacterDto.Vices = VicesDto { States[ViceStateDto {ViceId, Addiction, Tolerance, Uses, HasLastUse, LastUseWeek, Bets, NetWinnings, Discovered, Relapses, Quits,
QuitMethodId?, QuitStartWeek, QuitEndWeek}], HasIrreversible, LastIrreversibleWeek }`

## 6. ส่งต่อ
- ข้อ 14: Undo ห้ามย้อนก่อน `LastIrreversibleWeek`
- ข้อ 16–17: การ์ด Habits (เฉพาะ `IsVisible`), หน้าวิธีเลิก (`QuoteQuit`) และ Breakdown ของความยั่วยุ/การต้าน
- ข้อ 18: คำไว้อาลัยจาก Flag `vice.relapse` / `vice.quit_success` / `career.criminal_record`
- ข้อ 19: ห้ามแสดง Ads/IAP ในหน้าการพนัน (`BlocksUndo`)
- ข้อ 20: หน้าตั้งค่า "ปิดเนื้อหาอบายมุข" (`ContentEnabled`) และหน้าสายด่วน (`helplineKey`)
- ข้อ 22: เหตุการณ์ชุดอบายมุข (เพื่อนชวน, เมาแล้วขับ, เจ้าหนี้ทวง) ใช้ Condition `vice` และ Flag `finance.loan_shark`
