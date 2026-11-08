# Task 03: Rules Engine

> สถานะ: ⏸ **ข้อเสนอ Data Structure — รอผู้ใช้ยืนยัน** (ยังไม่มี Logic)

## 1. สรุปความเข้าใจ
Rules Engine คือ "ภาษากลาง" ที่ไฟล์ content ใช้บอกเกมว่า **เมื่อไร** (Condition) และ **เกิดอะไรขึ้น** (Effect)
เช่น "ถ้า Stress ≥ 70 และไม่เคยถูกไล่ออก → Happiness −5 และตั้ง Flag" ระบบ Event (ข้อ 07), Vice (13) และ Career (11) จะใช้ภาษานี้ร่วมกัน
ใช้ Strategy Pattern + Registry: ชนิดใหม่ = class ใหม่ + converter + ลงทะเบียน 1 บรรทัด โดยไม่ต้องแก้ Engine
Converter อ่าน JSON แล้วตรวจ id ที่อ้างอิงกับ catalog จากข้อ 02 **ตั้งแต่ตอนโหลด** ถ้าพิมพ์ id ผิดจะรู้ทันที พร้อมบอกตำแหน่งในไฟล์ (JSON path)

## 2. JSON (ใช้ภายในไฟล์ content ทุกชนิด)

### 2.1 Condition
```json
{ "type": "all", "conditions": [
    { "type": "stat",  "stat": "mental.stress", "op": ">=", "value": 70 },
    { "type": "not",   "condition": { "type": "flag", "flag": "career.fired_once" } },
    { "type": "any",   "conditions": [
        { "type": "trait", "trait": "anxious" },
        { "type": "age",   "op": "<", "value": 25 } ] }
] }
```
| type | ฟิลด์ | ความหมาย |
|---|---|---|
| `all` / `any` | `conditions[]` | ต้องจริงทุกข้อ / จริงอย่างน้อย 1 ข้อ |
| `not` | `condition` | กลับค่า |
| `stat` | `stat`, `op`, `value` | ใช้ได้ทั้งค่าที่คำนวณได้และค่าที่เก็บตรงๆ |
| `flag` | `flag`, (`op`, `value`) | ไม่ใส่ op แปลว่าต้อง "ตั้งอยู่" / ใส่ op ใช้เทียบจำนวน (counter) |
| `trait` | `trait`, (`revealed`: bool) | มี Trait (ถ้าใส่ `revealed` ด้วย จะดูว่าผู้เล่นรู้แล้วหรือยัง) |
| `skill` | `skill`, `op`, `value` | ระดับ Skill |
| `age` | `op`, `value` | อายุ (ปี) |
| `year` | `op`, `value` | ปี ค.ศ. ในเกม (ใช้กับเหตุการณ์ตามยุค) |
| `cash` | `op`, `amount` | เงินสด (decimal) |
| `reputation` | `domain`, `op`, `value` | ชื่อเสียงในวงการ |
| `healthCondition` | `condition` | กำลังเป็นโรคนั้นอยู่ |

`op`: `==` `!=` `<` `<=` `>` `>=`

### 2.2 Effect (เป็น array เสมอ ทำตามลำดับ)
```json
[
  { "type": "stat", "stat": "mental.stress", "amount": { "min": 3, "max": 8 } },
  { "type": "cash", "amount": -5000 },
  { "type": "flag.set", "flag": "relationship.recent_breakup", "expiryWeeks": 26 },
  { "type": "if", "condition": { "type": "trait", "trait": "calm" },
    "then": [ { "type": "stat", "stat": "mental.happiness", "amount": -2 } ],
    "else": [ { "type": "stat", "stat": "mental.happiness", "amount": -8 } ] }
]
```
| type | ฟิลด์ | หมายเหตุ |
|---|---|---|
| `stat` | `stat`, `amount`, (`source`) | ผ่าน `Character.ApplyStatChange` (Trait มีผล + บันทึกที่มา) ห้ามแก้ค่าที่คำนวณได้ (จับได้ตอนโหลด) |
| `cash` | `amount` | บวก = ได้เงิน / ลบ = จ่ายเงิน |
| `flag.set` / `flag.clear` / `flag.increment` | `flag`, (`expiryWeeks`, `amount`) | ตรวจชนิด Flag ตอนโหลด (increment ใช้ได้กับ counter เท่านั้น) |
| `trait.add` / `trait.remove` / `trait.reveal` | `trait` | |
| `skill.practice` | `skill`, `gain` | |
| `reputation` | `domain`, `amount` | |
| `healthCondition.add` / `.remove` | `condition`, (`severity`) | |
| `if` | `condition`, `then[]`, (`else[]`) | Butterfly Effect: ผลขึ้นกับอดีต |

- `amount` เป็นตัวเลข หรือเป็นช่วง `{min, max}` ก็ได้ ถ้าเป็นช่วง จะสุ่มใหม่ทุกครั้งที่เกิดผล ผ่าน `IRandomProvider` (ไม่ใช้ System.Random)
- `source` (ที่มาที่แสดงใน Breakdown) ปกติระบบผู้เรียกจะใส่ให้ เช่น `event.lost_wallet` แต่ effect แต่ละตัวกำหนดเองทับได้
- `scheduleEvent` (Delayed consequences) จะเพิ่มในข้อ 07 ผ่าน Registry ตัวนี้

### 2.3 JSON Schema
`docs/schemas/rules.schema.json` มี `$defs/condition` และ `$defs/effect` (`oneOf` ตาม `type`)
Schema ของ content อื่น (events, vices ฯลฯ) ใช้ `$ref` มาที่ไฟล์นี้

## 3. C# Domain (`Runtime/Rules/`)
| Class | หน้าที่ |
|---|---|
| `RuleContext` | สิ่งที่ Rule เห็น: `Character`, `Now` (GameDate), `Random` (IRandomProvider), `SourceKey` |
| `ICondition` | `bool Evaluate(RuleContext)` + `ConditionTrace Explain(RuleContext)` |
| `IEffect` | `EffectResult Apply(RuleContext)` + `IReadOnlyList<RuleText> Preview()` |
| `ComparisonOperator` + `Comparison` | `==`…`>=` และการเปรียบเทียบ float (มี tolerance), int, decimal |
| `FloatRange`, `DecimalRange` | ค่าคงที่ หรือช่วงที่สุ่มได้ |
| `RuleText` | Loc key + arguments เช่น `rule.stat.gte {stat: "stat.mental.stress.name", value: "70"}` |
| `ConditionTrace` | ผลการตรวจแบบต้นไม้ ใช้แสดงว่า "ทำไมตัวเลือกนี้ถูกล็อก" |
| `EffectResult` | สิ่งที่เกิดขึ้นจริง: รายการ `RuleText` (สำหรับ Inbox/Timeline) + `StatChangeResult` ที่ระบบอื่นนำไป Publish ต่อได้ |
| `Conditions/*Condition` (12 class) | ตามตาราง 2.1 |
| `Effects/*Effect` (13 class) | ตามตาราง 2.2 |

**ผลของ Effect ไม่ Publish เอง:** Effect คืน `EffectResult` ให้ระบบที่เรียกใช้ (เช่น Event System) เป็นคน Publish ผ่าน `IEventBus` ทำให้ Rules ไม่ผูกกับ Bus

**ส่วน Serialization (`Runtime/Rules/Serialization/`):**
| Class | หน้าที่ |
|---|---|
| `IConditionConverter` / `IEffectConverter` | `string Type`, `Read(JObject, RuleReadContext)`, `Write(...)` |
| `RuleRegistry` | type → converter, `ParseCondition`, `ParseEffects`, `Write…` |
| `RuleReadContext` | `CharacterCatalogs` (ใช้ตรวจ id) + JSON path ปัจจุบัน |
| `RuleParseException` | ข้อความชี้ตำแหน่ง เช่น `$.events[3].choices[0].effects[1]: unknown stat 'mental.stres'` |
| `*Converter` (1 ไฟล์ต่อ 1 ชนิด) | |
| `DefaultRules.CreateRegistry(catalogs)` | ลงทะเบียนชนิดพื้นฐานทั้งหมด (จุดเดียวที่ต้องแก้เมื่อเพิ่มชนิด) |

**ชนิดที่ต้องอ่านข้อมูลจากระบบอื่น** (เช่น `lifeStage` ในข้อ 09 หรือ `debt` ในข้อ 10): ระบบนั้นสร้าง converter ของตัวเองโดยรับ reader ผ่าน constructor แล้วลงทะเบียนใน GameBootstrap เพื่อให้ `RuleContext` เล็กอยู่เสมอ และไม่ต้องใช้ Service Locator

## 4. Save DTO
**ไม่มี**: Rule เป็น content ไม่ใช่สถานะของเกม ส่วนผลที่ตั้งเวลาไว้ล่วงหน้า (ข้อ 07) จะเซฟเป็น id ของเหตุการณ์ ไม่เซฟตัว Effect

## 5. คำถามที่ต้องยืนยัน
1. **ไลบรารี JSON:** ใช้ **Newtonsoft.Json** (แนะนำ: Unity มีให้เป็นแพ็กเกจทางการ `com.unity.nuget.newtonsoft-json` ซึ่งใส่ไว้ใน manifest แล้ว และ dotnet ใช้ผ่าน NuGet) หรือเขียนตัวอ่าน JSON เองใน Core (ไม่มี dependency แต่โค้ดเยอะกว่าและเสี่ยงบั๊กกว่า)?
2. **จ่ายเงินแต่เงินไม่พอ (ก่อนมีระบบหนี้ในข้อ 10):** จ่ายเท่าที่มีจนเงินเหลือ 0 แล้วรายงานยอดที่ขาด เพื่อให้ข้อ 10 นำไปทำเป็นหนี้ (แนะนำ) หรือให้ Effect นั้นล้มเหลวไปเลย?
3. **ตัวเลขแบบช่วง** สุ่มใหม่ทุกครั้งที่เกิดผล (แนะนำ) หรือสุ่มครั้งเดียวตอนโหลด content?
