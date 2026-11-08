# Task 02: Domain Model + Stats

> สถานะ: ⏸ **ข้อเสนอ Data Structure — รอผู้ใช้ยืนยัน** (ยังไม่มี Logic)

## 1. สรุปความเข้าใจ
สร้างโมเดลข้อมูลของตัวละครผู้เล่น: `Character` ซึ่งประกอบด้วย Stats 3 ชั้น (Core / ค่าย่อย / Hidden), Traits, Skills และ FlagSet (ความทรงจำของชีวิต สำหรับ Butterfly Effect)
ทุกค่าต้องอ่าน Breakdown ได้ (Design Pillar ข้อ 1) และนิยาม Stat/Trait/Skill/Flag อยู่ใน `content/*.json` เพื่อให้ Rules Engine (ข้อ 03) อ้างถึงด้วย id ได้
ตัวเลขสำหรับ balance ทั้งหมด (น้ำหนัก, อัตราเสื่อม) อยู่ใน JSON ไม่ hard-code
**นอกขอบเขตข้อนี้:** การโหลดไฟล์ JSON และ Validator Test (ข้อ 04), การเสื่อมรายสัปดาห์ (ข้อ 05), สินทรัพย์/หนี้ (ข้อ 10), NPC (ข้อ 12)

## 2. แนวคิดหลัก

### 2.1 Core Stat คำนวณจากค่าย่อย (ไม่เก็บค่าเอง)
```
Health       = Fitness×0.35 + Nutrition×0.25 + (100−Fatigue)×0.15 + (100−DiseaseBurden)×0.25
Mental       = Happiness×0.35 + (100−Stress)×0.30 + Confidence×0.15 + (100−Loneliness)×0.20
Intelligence = IQ×0.5 + Knowledge×0.5
Wealth        → เก็บค่าตรงๆ ชั่วคราว แล้วเปลี่ยนเป็นค่าคำนวณในข้อ 10 (จากความมั่งคั่งสุทธิเทียบค่าครองชีพ)
Relationships → เก็บค่าตรงๆ ชั่วคราว แล้วเปลี่ยนเป็นค่าคำนวณในข้อ 12 (จากความสัมพันธ์กับ NPC)
```
- น้ำหนักอยู่ใน `content/stats.json` และ Breakdown = รายการ "ค่าย่อย × น้ำหนัก" ทุกบรรทัด เช่น
  `Health 68 = Fitness 70×0.35 (24.5) + Nutrition 60×0.25 (15) + …`
- Core ที่คำนวณได้จะแก้ตรงๆ ไม่ได้ ต้องแก้ผ่านค่าย่อยเท่านั้น ทำให้ทุกการเปลี่ยนแปลงย้อนไปหาสาเหตุได้เสมอ
- สูตรเป็น Strategy (`IStatFormula`) ข้อ 10 และ 12 จะเพิ่มสูตรใหม่ได้โดยไม่ต้องแก้ของเดิม

### 2.2 ทุกการเปลี่ยนค่ามี "ที่มา"
`StatChange { StatId, Delta, SourceKey }` → ผ่านตัวคูณจาก Trait → Clamp 0–100 → บันทึกลง `StatChangeLog`
Log เก็บย้อนหลัง 4 สัปดาห์ ใช้แสดง Breakdown ของแนวโน้ม เช่น "สัปดาห์นี้ Stress +6: ทำงานล่วงเวลา +8, ออกกำลังกาย −2"

### 2.3 ประวัติสำหรับ Sparkline
`StatHistory` เก็บค่า Core 5 ตัว 1 ค่าต่อสัปดาห์ ย้อนหลัง 52 สัปดาห์ (ring buffer)

## 3. JSON Schema (content)

### `content/stats.json` → `docs/schemas/stats.schema.json`
```json
{
  "stats": [
    { "id": "health", "layer": "core", "polarity": "higherIsBetter", "default": 50,
      "nameKey": "stat.health.name", "descKey": "stat.health.desc" },
    { "id": "health.fitness", "layer": "sub", "parent": "health", "polarity": "higherIsBetter", "default": 50,
      "nameKey": "stat.health.fitness.name", "descKey": "stat.health.fitness.desc",
      "decay": { "target": 40, "ratePerWeek": 0.5 } },
    { "id": "health.fatigue", "layer": "sub", "parent": "health", "polarity": "lowerIsBetter", "default": 20,
      "nameKey": "stat.health.fatigue.name", "descKey": "stat.health.fatigue.desc" }
  ],
  "formulas": [
    { "target": "health", "type": "weighted", "terms": [
        { "stat": "health.fitness", "weight": 0.35 },
        { "stat": "health.nutrition", "weight": 0.25 },
        { "stat": "health.fatigue", "weight": 0.15, "invert": true },
        { "stat": "health.disease_burden", "weight": 0.25, "invert": true } ] }
  ]
}
```
| ฟิลด์ | กฎ |
|---|---|
| `id` | `^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)*$` ไม่ซ้ำ |
| `layer` | `core` \| `sub` \| `hidden` |
| `polarity` | `higherIsBetter` \| `lowerIsBetter` (ใช้กำหนดสีลูกศรใน UI) |
| `default` | 0–100 |
| `decay` | ไม่บังคับ: ค่าจะค่อยๆ เคลื่อนเข้าหา `target` ทีละ `ratePerWeek` (ข้อ 05 เป็นคนเรียกใช้) |
| `formulas[].terms` | น้ำหนักรวมกันต้องเท่ากับ 1.0 (Validator จะตรวจในข้อ 04) |

**รายการ Stat ทั้งหมด**
| ชั้น | id |
|---|---|
| Core | `health`, `mental`, `intelligence`, `wealth`, `relationships` |
| ค่าย่อย Health | `health.fitness`, `health.nutrition`, `health.fatigue`↓, `health.disease_burden`↓ |
| ค่าย่อย Mental | `mental.happiness`, `mental.stress`↓, `mental.confidence`, `mental.loneliness`↓ |
| ค่าย่อย Intelligence | `intelligence.iq`, `intelligence.knowledge` |
| ค่าย่อย Wealth | `wealth.credit_score` |
| Hidden | `hidden.charisma`, `hidden.looks`, `hidden.willpower` |

(↓ = ยิ่งต่ำยิ่งดี) · Cash, Assets, Debts, NetWorth เป็นเงิน (`decimal`) ไม่ใช่ Stat: ข้อนี้มีแค่ `Cash` ที่เหลือมาในข้อ 10
GDD ระบุ "Diseases" เป็นค่าย่อย ผมแยกเป็น 2 ส่วน: ค่า `health.disease_burden` (0–100) ที่ใช้ในสูตร และรายการโรคที่เป็นอยู่ (`HealthCondition`)

### `content/traits.json` → `traits.schema.json`
```json
{ "traits": [
  { "id": "anxious", "nameKey": "trait.anxious.name", "descKey": "trait.anxious.desc",
    "category": "personality", "exclusiveGroup": "temperament", "startsHidden": true,
    "statModifiers": [ { "stat": "mental.stress", "direction": "gain", "multiplier": 1.25 } ] }
] }
```
- `exclusiveGroup`: Trait ที่อยู่กลุ่มเดียวกันจะมีพร้อมกันไม่ได้ (เช่น ใจเย็น กับ วิตกกังวล)
- `statModifiers`: คูณขนาดการเปลี่ยนค่าตามทิศ (`gain` \| `loss` \| `both`) เช่น คนวิตกกังวลจะ Stress ขึ้นมากกว่าคนอื่น 25%
- `startsHidden`: ผู้เล่นยังไม่รู้ว่าตัวเองมี Trait นี้ จนกว่าเหตุการณ์จะเปิดเผย (เป็น Hidden ตาม GDD)

### `content/skills.json` → `skills.schema.json`
```json
{ "skills": [
  { "id": "programming", "nameKey": "skill.programming.name", "category": "technical",
    "gainFalloffExponent": 1.5,
    "decay": { "graceWeeks": 8, "ratePerWeek": 0.4, "retentionOfPeak": 0.5 } }
] }
```
- Level 0–100 ยิ่งเก่งยิ่งเพิ่มช้า: `gain × (1 − level/100)^gainFalloffExponent`
- เสื่อมเมื่อไม่ได้ใช้: ช่วง `graceWeeks` แรกยังไม่ลด จากนั้นลดทีละ `ratePerWeek` แต่ไม่ต่ำกว่า `peak × retentionOfPeak` ("ความจำของกล้ามเนื้อ": เคยเก่งแล้วไม่กลับไปเป็นศูนย์)

### `content/flags.json` → `flags.schema.json`
```json
{ "flags": [
  { "id": "career.fired_once", "kind": "bool", "defaultExpiryWeeks": null,
    "inheritable": false, "eulogyKey": "eulogy.career.fired_once" },
  { "id": "vice.gambling_wins", "kind": "counter", "defaultExpiryWeeks": null,
    "inheritable": false, "eulogyKey": null }
] }
```
- ต้องประกาศ Flag ทุกตัวไว้ก่อน เพื่อให้ Validator (ข้อ 04) ตรวจได้ว่าเหตุการณ์ไม่ได้อ้างถึง Flag ที่พิมพ์ผิด
- `inheritable` → ส่งต่อให้ลูกหลานใน Legacy Mode (ข้อ 18) / `eulogyKey` → ใช้สร้างคำไว้อาลัย (ข้อ 18)
- `defaultExpiryWeeks` → Flag ชั่วคราว เช่น "เพิ่งเลิกกับแฟน" หายไปเองหลัง 26 สัปดาห์

Localization key ทั้งหมดใช้รูปแบบ `<type>.<id>.name` / `.desc`

## 4. C# Domain Classes (`Runtime/Domain/...`)
| Namespace | Class | หน้าที่ |
|---|---|---|
| `Domain.Stats` | `StatId` (struct) | ห่อ string id ให้ type-safe + ค่าคงที่ของ Stat ที่ระบบต้องรู้จัก |
| | `StatLayer`, `StatPolarity` (enum) | |
| | `StatDefinition`, `StatDecayRule` | นิยามจาก JSON (immutable) |
| | `IStatFormula`, `WeightedStatFormula`, `StatFormulaTerm` | สูตรคำนวณ Core |
| | `StatCatalog` | รวม Definition + Formula ตรวจว่าอ้างอิงครบ |
| | `StatBlock` | ค่าที่เก็บจริงของ Stat ที่ไม่ได้คำนวณ (`Dictionary<StatId,float>`) + Clamp |
| | `StatChange`, `StatChangeResult` | คำขอเปลี่ยนค่า / ผลลัพธ์ (ขอ ±x ได้จริงเท่าไร ค่าใหม่เท่าไร) |
| | `StatBreakdown`, `StatBreakdownLine` | สำหรับกดค้างดูที่มาของตัวเลข |
| | `StatChangeLog`, `StatHistory` | ที่มาย้อนหลัง 4 สัปดาห์ / Sparkline 52 สัปดาห์ |
| | `ReputationBook` | ชื่อเสียงแยกวงการ `domainId → 0–100` (0 = ไม่มีใครรู้จัก) |
| `Domain.Traits` | `TraitDefinition`, `TraitStatModifier`, `ModifierDirection` | |
| | `TraitInstance` | `TraitId`, `AcquiredOn`, `IsRevealed` |
| | `TraitSet` | เพิ่ม/ลบ (ตรวจ exclusiveGroup), เปิดเผย, รวมตัวคูณ |
| `Domain.Skills` | `SkillDefinition`, `SkillDecayRule` | |
| | `SkillProgress` | `Level`, `PeakLevel`, `LastPracticedOn` |
| | `SkillSet` | `Practice(id, baseGain, date)`, `ApplyDecay(date)` |
| `Domain.Flags` | `FlagDefinition`, `FlagKind` | |
| | `FlagEntry` | `Value` (int: bool = 0/1), `SetOn`, `ExpiresOn?` |
| | `FlagSet` | `Set`, `Increment`, `Clear`, `IsSet`, `GetCount`, `RemoveExpired(date)` |
| `Domain.Characters` | `CharacterId` (struct) | id ที่สร้างจาก seed (ได้ค่าเดิมทุกครั้ง) |
| | `PersonName` | ชื่อ/นามสกุล (เป็นข้อมูล ไม่ใช่ข้อความ UI จึงไม่ใช้ Loc key) |
| | `Sex` (enum) | ใช้กับพันธุกรรม (ข้อ 08) และคำลงท้าย ครับ/ค่ะ |
| | `Wallet` | `decimal Cash` |
| | `HealthCondition` | `ConditionId`, `Severity` 0–100, `StartedOn` |
| | `Character` | Aggregate root ที่รวมทุกอย่าง และ `ApplyStatChange(StatChange)` คือทางเดียวที่ใช้แก้ Stat |

```
Character
├─ Id, Name, Sex, BirthDate (CalendarDate), BirthHour, BirthCountryId
├─ Stats: StatBlock ──(StatCatalog)──► Core ที่คำนวณได้ + Breakdown
├─ ChangeLog: StatChangeLog · History: StatHistory
├─ Reputation: ReputationBook
├─ Conditions: List<HealthCondition>
├─ Wallet
├─ Traits: TraitSet · Skills: SkillSet · Flags: FlagSet
```
- ลำดับใน `ApplyStatChange`: ถ้าเป็น Core ที่คำนวณได้ → ปฏิเสธ → คูณด้วยตัวคูณของ Trait → Clamp → บันทึก Log → คืน `StatChangeResult`
- Domain ไม่รู้จัก EventBus (เป็น Pure model) ส่วนระบบในข้อหลังๆ เป็นคน Publish event หลังเรียก Domain
- `Character` ใช้กับผู้เล่น (และลูกหลานใน Legacy) ส่วน NPC จะมีโมเดลที่เบากว่าในข้อ 12

## 5. Save DTO (`Runtime/Persistence/`)
```
CharacterDto
  Id, GivenName, FamilyName, Sex (string)
  Birth: CalendarDateDto, BirthHour, BirthCountryId
  Stats:       List<StatValueDto      { Id, Value }>            ← เฉพาะค่าที่เก็บจริง (Core ที่คำนวณได้ไม่ถูกเซฟ)
  Reputation:  List<ReputationDto     { DomainId, Value }>
  Conditions:  List<HealthConditionDto{ Id, Severity, StartedWeek }>
  Cash:        string                                           ← decimal เก็บเป็นข้อความ (InvariantCulture)
  Traits:      List<TraitDto          { Id, AcquiredWeek, Revealed }>
  Skills:      List<SkillDto          { Id, Level, Peak, LastPracticedWeek }>
  Flags:       List<FlagDto           { Id, Value, SetWeek, HasExpiry, ExpiresWeek }>
  ChangeLog:   List<StatChangeLogDto  { Week, StatId, Delta, SourceKey }>
  History:     List<StatHistoryDto    { StatId, Values: List<float> }>
```
- ใช้ List ทั้งหมด (JsonUtility ใช้กับ Dictionary ไม่ได้) และใช้ `HasExpiry` แทน nullable ด้วยเหตุผลเดียวกัน
- ถ้าเจอ id ที่ไม่มีใน content แล้ว (เช่น Trait ที่ถูกลบออกจากเกม) ตอนโหลดจะข้ามไปและบันทึก warning ไม่ทำให้เซฟเสีย

## 6. คำถามที่ต้องยืนยัน
1. **Core คำนวณจากค่าย่อย** ตามข้อ 2.1 (แนะนำ) หรือให้ Core เก็บค่าเองแยกจากค่าย่อย?
2. **เพศ:** `Sex` มีแค่ Female / Male (ใช้กับพันธุกรรมและคำลงท้าย ครับ/ค่ะ) พอไหม หรือต้องการให้ผู้เล่นเลือกเพศสภาพแยกต่างหากด้วย?
3. **Credit Score:** เก็บภายในเป็น 0–100 เหมือน Stat อื่น แล้วค่อยแปลงเป็นช่วง 300–900 ตอนแสดงผลแบบเครดิตบูโรไทย (แนะนำ) หรือเก็บ 300–900 ตรงๆ?
4. **ข้อเสนอที่ไม่ต้องตอบถ้าเห็นด้วย:** Trait ซ่อนอยู่จนกว่าจะเปิดเผย / Skill เสื่อมแต่ไม่ต่ำกว่าครึ่งของระดับสูงสุดที่เคยทำได้ / Log ย้อนหลัง 4 สัปดาห์ / Sparkline 52 สัปดาห์
