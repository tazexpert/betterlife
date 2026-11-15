# Task 09: Life Stages + Aging + Death

> สถานะ: ✅ **Implemented** — 2026-11-08 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **Life Stages 8 ช่วง** ตาม GDD: ทารก 0–2, เด็กเล็ก 3–6, วัยเรียน 7–12, วัยรุ่น 13–17, ผู้ใหญ่ตอนต้น 18–29, วัยสร้างตัว 30–44, วัยกลางคน 45–59, วัยเกษียณ 60+
- **Aging:** ทุกวันเกิดมีผลของวัย (เช่น วัยกลางคน Fitness ลดลง) และมีความเสี่ยงโรคเรื้อรังตามวัย
- **โรค/อาการ** มีผลรายสัปดาห์ มีโอกาสหาย และบางโรคเพิ่มความเสี่ยงตาย (เป็นส่วนที่ข้อ 04 ส่งต่อมา)
- **Death:** ทุกสัปดาห์ตรวจความเสี่ยงตายจากอายุ (Gompertz), สุขภาพ, โรค, สาธารณสุขของเมือง และช่วงวัย เมื่อตายจะเลือกสาเหตุตามน้ำหนัก (Rules) แล้วหยุดเกม (Critical)
  เหตุการณ์ก็ทำให้ตายได้ ผ่าน Effect `character.die`

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ |
|---|---|
| วันขึ้นวัย | ตรวจทุกสัปดาห์ว่าตรงสัปดาห์วันเกิดไหม (ไม่ใช่ทุกต้นปี) อายุจึงขึ้นตรงวันเกิดจริง |
| Phase ใหม่ | `aging` (ลำดับ 50 ก่อนการแบ่งเวลา เพื่อให้ budget ของวัยใหม่มีผลในสัปดาห์นั้นเลย), `health_conditions` (210), `death_check` (600) |
| Time budget ตามวัย | เมื่อเข้าวัยใหม่ budget จะถูกตั้งเป็นค่าของวัยนั้น (เช่น วัยเรียน: เรียน 35 ชม.) โดยทารกและเด็กเล็ก `playerControlsTime = false` คือผู้เล่นยังแบ่งเวลาเองไม่ได้ |
| ที่เก็บ budget ตามวัย | `time_settings.json` → `stageBudgets` (เพราะ budget อ้างถึงกิจกรรม ซึ่งโหลดหลัง catalog ของตัวละคร) |
| ความเสี่ยงตายรายปี | (Gompertz(อายุ) + ส่วนเพิ่มของช่วงวัย + Σ ความเสี่ยงของโรค × severity) × ตัวคูณสุขภาพ × ตัวคูณสาธารณสุข แล้วแปลงเป็นรายสัปดาห์ = 1 − (1 − p)^(1/52) |
| Gompertz | 0.08% ต่อปีที่อายุ 30 และโตขึ้นปีละ 8.5% (อายุขัยเฉลี่ยราว 75–80) ส่วนอายุ 120 ตายแน่นอน |
| ตัวคูณสุขภาพ | Health 50 = ×1, Health 0 = ×8, Health 100 = ×0.6 (log-linear) |
| ตัวคูณสาธารณสุข | healthcare ของเมือง 0 = ×1.4, 100 = ×0.85 |
| ทารก | มีความเสี่ยงเพิ่ม 0.4% ต่อปี (ตัวเลขใกล้อัตราการตายของทารกไทย) |
| สาเหตุการตาย | ใช้น้ำหนักแบบเดียวกับ Event (`base` + modifier ที่เป็น Condition) และเงื่อนไขว่าใช้ได้กับใคร เช่น "ชราภาพ" ×50 เมื่ออายุ ≥ 70 |
| หลังตาย | `Character.Death` ถูกตั้ง → TimeSystem จะไม่เดินต่อ และ EventSystem จะไม่สุ่มเหตุการณ์อีก ส่วนการสรุปชีวิตอยู่ในข้อ 18 |
| พ่อแม่แก่และตาย | ยกไปข้อ 12 (NPC) เพราะตอนนี้พ่อแม่ยังเป็น record แบบเบา |
| โรค | `weeklyStats` × severity/100 (ที่มาใน Breakdown = ชื่อโรค), `recoveryChancePerWeek` (null = เรื้อรัง), `annualMortality` (ที่ severity 100) |
| disease_burden | ใส่ decay เข้าหา 0 ทีละ 1 ต่อสัปดาห์ เพื่อให้หายป่วยแล้วค่อยๆ ฟื้นตัว |

## 3. JSON Schema (ใหม่ / แก้ไข)
```jsonc
// life_stages.json (ใหม่)
{ "stages": [ { "id": "middle_age", "nameKey": "stage.middle_age.name", "minAge": 45, "playerControlsTime": true,
    "extraAnnualMortality": 0,
    "birthdayStats": [ { "stat": "health.fitness", "amount": -0.8 } ],
    "conditionRisks": [ { "condition": "hypertension", "annualChance": 0.01 } ] } ] }

// mortality.json (ใหม่, ไฟล์เดียว)
{ "gompertz": { "referenceAge": 30, "annualAtReference": 0.0008, "growthPerYear": 0.085 }, "maxAge": 120,
  "health":     { "multiplierAtZero": 8, "multiplierAtFull": 0.6 },
  "healthcare": { "multiplierAtZero": 1.4, "multiplierAtFull": 0.85 },
  "maxWeeklyProbability": 0.5 }

// death_causes.json (ใหม่)
{ "causes": [ { "id": "old_age", "nameKey": "death.old_age.name", "eulogyKey": "eulogy.death.old_age",
    "condition": { "type": "age", "op": ">=", "value": 60 },
    "weight": { "base": 0.2, "modifiers": [ { "condition": { "type": "age", "op": ">=", "value": 75 }, "multiplier": 25 } ] } } ] }

// health_conditions.json (เพิ่มฟิลด์)
{ "conditions": [ { "id": "flu", "nameKey": "…", "descKey": "…",
    "weeklyStats": [ { "stat": "health.fatigue", "amount": 4 } ], "recoveryChancePerWeek": 0.35, "annualMortality": 0.002 } ] }

// time_settings.json (เพิ่มฟิลด์)
"stageBudgets": [ { "stage": "child", "budget": [ { "activity": "sleep", "hours": 70 }, { "activity": "study", "hours": 35 } ] } ]
```
**Rule ใหม่:**
- Condition `{ "type": "lifeStage", "stages": ["teen", "young_adult"] }`
- Effect `{ "type": "character.die", "cause": "accident" }`

## 4. C# Domain
| Namespace | Class |
|---|---|
| `Domain.Life` | `LifeStageDefinition`, `ConditionRisk`, `LifeStageCatalog`, `DeathRecord`, `LifeStageCondition` |
| `Domain.Characters` | `HealthConditionDefinition` (+ weeklyStats, recovery, mortality), Character + `Death`, `IsAlive`, `Die()`, `LifeStage(now)` |
| `Life` | `MortalitySettings`, `MortalityModel` (+ `MortalityBreakdown`), `DeathCauseDefinition`, `DeathService`, `DeathRequest`, `DieEffect`, `AgingPhase`, `HealthConditionPhase`, `DeathCheckPhase`, Events: `BirthdayEvent`, `LifeStageChangedEvent`, `CharacterDiedEvent`, `HealthConditionRecoveredEvent`, `HealthConditionStartedEvent` |
| `Time.Budget` | `TimeBudget.Reset(plan)`, `TimeSettings.StageBudgets` |

## 5. Save DTO
`CharacterDto` เพิ่ม `HasDied`, `DeathWeek`, `DeathCauseId` ส่วนช่วงวัยไม่ต้องเซฟ เพราะคำนวณจากอายุได้

## 6. Content เริ่มต้น
- 8 ช่วงวัย พร้อม budget ของแต่ละวัย เช่น วัยเรียน: นอน 70, เรียน 35, บันเทิง 10 ส่วนวัยเกษียณ: นอน 63, เข้าสังคม 10
- ผลวันเกิด:
  - วัยสร้างตัว Fitness −0.3/ปี, วัยกลางคน −0.8/ปี, วัยเกษียณ −1.5/ปี
  - ความเสี่ยงโรคความดัน/เบาหวาน/ซึมเศร้าเพิ่มตามวัย
- 9 โรค พร้อมผลรายสัปดาห์และโอกาสหาย (ความดันและเบาหวานเป็นโรคเรื้อรัง)
- 6 สาเหตุการตาย: เจ็บป่วย, ชราภาพ (60+), โรคหัวใจ (35+), มะเร็ง (30+), อุบัติเหตุ (×2.5 ในวัยรุ่น/ผู้ใหญ่ตอนต้น), โรคในเด็กเล็ก (ทารก/เด็กเล็ก)

## 7. Tests
- Core 19 test ใหม่:
  - ช่วงวัย: ขอบเขตอายุ + การตรวจความถูกต้อง
  - วันเกิด: เกิดเฉพาะสัปดาห์วันเกิด + ผลพร้อมที่มา
  - เข้าวัยใหม่: reset budget + interrupt
  - ความเสี่ยงโรคตามวัย
  - โรค: ผลรายสัปดาห์ตาม severity + การหาย + โรคเรื้อรังไม่หาย
  - Mortality: ทุก term, แปลงเป็นรายสัปดาห์, อายุสูงสุด
  - Death check, น้ำหนักและเงื่อนไขของสาเหตุ
  - Effect ทำให้ตาย → เวลาหยุด และคนตายไม่แก่ต่อ
  - Save การตาย, Condition `lifeStage`, `TimeBudget.Reset`
- Content 1 test ใหม่: เล่น 80 ชีวิตจนตายด้วย content จริง
  - **อายุเฉลี่ยตอนตาย 82.3 (มัธยฐาน 85, ต่ำสุด 16, สูงสุด 107)**
  - สาเหตุ: ชราภาพ 41, มะเร็ง 12, โรคหัวใจ 12, เจ็บป่วย 10, อุบัติเหตุ 5
  - สูงกว่าอายุขัยไทยจริงเล็กน้อย เพราะตอนนี้ยังไม่มีงาน หนี้ และอบายมุข (ข้อ 10–13) จะปรับสมดุลในข้อ 21

## 8. ส่งต่อ
- ข้อ 10: ค่ารักษาพยาบาลตาม healthcare / ประกัน
- ข้อ 12: พ่อแม่และ NPC แก่และตาย
- ข้อ 13: อบายมุขเพิ่มสาเหตุการตาย (overdose ฯลฯ) ผ่าน `death_causes.json` + Condition
- ข้อ 18: Life Summary + คำไว้อาลัยจาก `DeathRecord` + `eulogyKey`
- ข้อ 21: ปรับสมดุลอายุขัยจาก 10,000 ชีวิต
