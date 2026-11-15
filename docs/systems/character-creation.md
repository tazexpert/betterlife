# Task 08: Character Creation

> สถานะ: ✅ **Implemented** — 2026-11-08 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
สร้างชีวิตใหม่จาก Seed เดียว ประกอบด้วย:
- วันเกิด + ชั่วโมงเกิด (ยุคที่เกิด = ปีเกิด)
- ประเทศและเมือง (Modifier: ค่าครองชีพ, การศึกษา, สาธารณสุข, ภาษี)
- ฐานะครอบครัว และพ่อแม่ 2 คน (บุคลิก + พันธุกรรม)

มี 3 โหมด: **Random** (สุ่มทุกอย่าง), **Custom** (ผู้เล่นกำหนดบางส่วน ที่เหลือสุ่ม), **Challenge Seed** (รหัสที่แชร์กันได้ → ได้ชีวิตเดียวกันทุกครั้ง)
และประกอบทุกระบบของชีวิตหนึ่งเข้าด้วยกันเป็น `LifeSession` แล้วต่อเข้ากับ `GameBootstrap`

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ |
|---|---|
| เริ่มเล่นเมื่อไร | สัปดาห์ที่เกิด (อายุ 0) ตาม Life Stage ทารก ส่วนการแบ่งเวลาตามวัยจะมาในข้อ 09 |
| ปีเกิด | สุ่มในช่วง 1950–2026 (ตั้งค่าได้) ส่วน Custom เลือกเองได้ในช่วงนี้ |
| ประเทศ/เมือง | สุ่มตามน้ำหนัก (ไทย 90%) แต่ละเมืองมี modifier ของตัวเอง ถ้าไม่กำหนด จะใช้ค่าของประเทศ |
| ฐานะครอบครัว | 6 ระดับตามน้ำหนัก: ยากจน 25% … ร่ำรวยมาก 1% มีผลต่อ Wealth และค่าย่อยตอนเกิดผ่าน `statModifiers` (มีที่มาใน Breakdown) |
| บุคลิกพ่อแม่ | 6 แบบ (เข้มงวด, อบอุ่น, ปล่อยปละ, ปกป้องมากเกินไป, เน้นวิชาการ, รักอิสระ) แต่ละแบบมี statModifiers + โอกาสได้ Trait |
| พันธุกรรม | ปู่ย่า (รุ่นแรก) สุ่มยีนแบบ normal (50, 15) ส่วนลูกได้ค่า = h × ค่าเฉลี่ยพ่อแม่ + (1−h) × 50 + noise และ Trait ส่งต่อได้ด้วยโอกาส 1 − (1−h)^จำนวนพ่อแม่ที่มี Trait นั้น (ไม่น้อยกว่าความชุกในประชากร) |
| Genome | เก็บค่าศักยภาพทางพันธุกรรมไว้ใน Character แยกจาก Stat ปัจจุบัน เพื่อส่งต่อให้ลูกหลานในข้อ 18 |
| พ่อแม่ | เก็บเป็น `ParentRecord` แบบเบา ข้อ 12 จะเปลี่ยนเป็น NPC เต็มรูปแบบ |
| ชื่อ | สุ่มจากรายชื่อของแต่ละประเทศ เก็บทั้งตัวอักษรท้องถิ่นและอักษรโรมัน ผู้เรียกเลือกได้ว่าจะใช้แบบไหน (UI ภาษาไทยใช้ตัวอักษรไทย) |
| Seed | ใช้ `ulong` ตัวเดียวกับ `RandomRegistry` และแต่ละด้านของการสร้างใช้ชุดเลขสุ่มของตัวเอง (`creation.birth`, `.location`, `.family`, `.genetics`, `.names`) ใน Custom การกำหนดด้านหนึ่งจึงไม่ทำให้ด้านอื่นเปลี่ยน |
| Challenge Code | Crockford Base32 ของ seed 64 บิต 13 ตัวอักษร รูปแบบ `XXXX-XXXX-XXXXX` (ไม่สนตัวพิมพ์เล็กใหญ่, I/L = 1, O = 0) |
| Challenge mode | ห้ามกำหนดค่าเอง ทุกคนจึงได้ชีวิตเดียวกันจาก seed เดียวกัน |
| Rule ใหม่ | Condition `location` (ประเทศ/เมือง) และ `familyBackground` |
| Seed ตอนเปิดเกม | Bootstrap ใช้เวลาปัจจุบันเป็นแหล่งสุ่มของ seed ใหม่เท่านั้น ส่วนการสุ่มทั้งหมดหลังจากนั้นผ่าน PCG32 |

## 3. JSON Schema (content ใหม่ 6 ชนิด)
```jsonc
// countries.json
{ "countries": [ { "id": "thailand", "nameKey": "country.thailand.name", "weight": 90,
    "modifiers": { "costOfLiving": 1.0, "education": 60, "healthcare": 65, "taxRate": 0.1 },
    "cities": [ { "id": "thailand.bangkok", "nameKey": "city.thailand.bangkok.name", "weight": 30,
                  "modifiers": { "costOfLiving": 1.35, "education": 75, "healthcare": 80 } } ] } ] }

// family_backgrounds.json
{ "backgrounds": [ { "id": "poor", "nameKey": "family.poor.name", "weight": 25, "householdIncomeLevel": 1,
    "statModifiers": [ { "stat": "wealth", "amount": -20 }, { "stat": "health.nutrition", "amount": -10 } ] } ] }

// parent_personalities.json
{ "personalities": [ { "id": "strict", "nameKey": "parent.strict.name", "weight": 20,
    "statModifiers": [ { "stat": "hidden.willpower", "amount": 6 }, { "stat": "mental.stress", "amount": 5 } ],
    "traitChances": [ { "trait": "anxious", "chance": 0.15 } ] } ] }

// genetics.json
{ "population": { "mean": 50, "sd": 15 },
  "stats":  [ { "stat": "intelligence.iq", "heritability": 0.6, "noise": 10 } ],
  "traits": [ { "trait": "anxious", "prevalence": 0.15, "heritability": 0.4 } ] }

// names.json
{ "countries": [ { "country": "thailand",
    "given": { "female": [ { "native": "สมศรี", "latin": "Somsri" } ], "male": [ ... ] },
    "family": [ { "native": "ใจดี", "latin": "Jaidee" } ] } ] }

// creation_settings.json
{ "birthYear": { "min": 1950, "max": 2026 }, "parentAgeAtBirth": { "min": 20, "max": 42 } }
```

## 4. C# Domain
| Namespace | Class |
|---|---|
| `Domain.World` | `LocationModifiers`, `CityDefinition`, `CountryDefinition`, `Residence` |
| `Domain.Family` | `FamilyBackgroundDefinition`, `ParentPersonalityDefinition`, `StatAmount`, `TraitChance`, `ParentRecord`, `FamilyInfo` |
| `Domain.Genetics` | `Genome`, `GeneticsSettings`, `HeritableStat`, `HeritableTrait`, `GeneticsService` |
| `Domain.Characters` | Character ได้ `Residence`, `Family`, `Genome` และ CharacterCatalogs ได้ `Countries`, `FamilyBackgrounds` |
| `Creation` | `CreationMode`, `NameScript`, `CreationRequest`, `CreationIssue`, `CreationResult`, `NameEntry`, `NameList`, `NameCatalog`, `CreationSettings`, `ChallengeCode`, `CharacterCreator` |
| `Randomness` | `RandomExtensions.NextGaussian` (Box–Muller) |
| `Sessions` | `LifeSession`, `LifeSessionFactory`: Character + RandomRegistry + TimeState + EventState + TimeSystem + EventSystem + LuckCalculator |
| Rules | `LocationCondition`, `FamilyBackgroundCondition` |

## 5. Save DTO
`CharacterDto` เพิ่ม:
- `Residence { CountryId, CityId }`
- `Family { BackgroundId, Parents: [ParentDto { GivenName, FamilyName, Sex, BirthYear, PersonalityId, Genome[], Traits[] }] }`
- `Genome: [StatValueDto]`

(การเซฟ `LifeSession` ทั้งก้อน รวม seed อยู่ในข้อ 14)

## 6. Content เริ่มต้น
- 2 ประเทศ:
  - ไทย (น้ำหนัก 90): 7 เมือง ได้แก่ กรุงเทพฯ, เชียงใหม่, ขอนแก่น, ภูเก็ต, หาดใหญ่, โคราช, ชนบทอีสาน
  - ญี่ปุ่น (น้ำหนัก 10): 3 เมือง
- ฐานะครอบครัว 6 ระดับ, บุคลิกพ่อแม่ 6 แบบ
- พันธุกรรม: Stat 5 ตัว (IQ, Looks, Charisma, Willpower, Fitness) และ Trait 6 ตัว
- ชื่อ: ไทย 12 + 12 + นามสกุล 12, ญี่ปุ่น 8 + 8 + 8

## 7. Tests
- Core 39 test ใหม่:
  - Challenge Code: round-trip 1,000 seed, พิมพ์ผิดแบบที่พบบ่อยได้, รหัสเสีย 6 แบบ
  - Creator: seed เดียวกันได้ชีวิตเดียวกัน, ต่าง seed ได้ชีวิตต่างกัน, ช่วงค่า, ผลตอนเกิดพร้อมที่มา, Custom override
  - Custom: แก้ด้านหนึ่งแล้วด้านอื่นไม่เปลี่ยน
  - Validation 7 แบบ และโหมดที่ห้าม override
  - ชื่อตามชุดตัวอักษร, การกระจายตามน้ำหนัก (2,000 ชีวิต), modifier ของเมือง, Condition ใหม่, Save round-trip
  - Genetics: Gaussian, การสุ่มตามน้ำหนัก, ค่าลูกถอยเข้าหาค่าเฉลี่ย, noise, โอกาสได้ Trait, กลุ่ม Trait ที่ขัดกัน
  - LifeSession
  - Stat ที่โค้ดต้องใช้ต้องมีใน content
- Content 3 test ใหม่:
  - 500 ชีวิตด้วย content จริง: ไทย 90%, ฐานะครอบครัวกระจายตามน้ำหนัก
  - Challenge Code สร้างชีวิตเดิมซ้ำได้
  - เล่นชีวิตหนึ่งครบ 20 ปีผ่าน `LifeSession` (เกิด 66 เหตุการณ์)

## 8. บั๊กที่พบระหว่างทำ
`TimeAllocationPhase` อ่าน Stat `mental` โดยไม่มีอะไรรับประกันว่า content มี Stat นี้ ตอนนี้ `StatIds.RequiredByCode` + `StatsLoader` จะรายงานเป็น content error ตั้งแต่ตอนโหลด

## 9. Unity Integration
`GameBootstrap` สร้างชีวิตใหม่แบบ Random ทุกครั้งที่เปิดเกม (ชั่วคราว จนกว่าจะมีหน้าสร้างตัวละครในข้อ 15–16) และ log รหัส Challenge + ชื่อ + วันเกิด + เมือง

## 10. ส่งต่อ
- ข้อ 09: budget ตามช่วงวัย, การตายของพ่อแม่
- ข้อ 10: ใช้ `LocationModifiers` (ค่าครองชีพ, ภาษี) และ `householdIncomeLevel`
- ข้อ 11: `education` modifier
- ข้อ 12: เปลี่ยน `ParentRecord` เป็น NPC
- ข้อ 15–16: หน้าสร้างตัวละคร (ใช้ `CreationRequest` + `CreationIssue`)
- ข้อ 18: Legacy ใช้ `GeneticsService` กับพ่อแม่ที่เป็นตัวละครจริง
