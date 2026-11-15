# Task 12: NPC + Relationships

> สถานะ: ✅ **Implemented** — 2026-11-09 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **NPC** เป็นคนในชีวิตของตัวละคร (พ่อแม่ พี่น้อง เพื่อน แฟน คู่สมรส ลูก เพื่อนร่วมงาน คอนเนกชันธุรกิจ)
  มีชื่อ เพศ วันเกิด พันธุกรรม (Hidden Potential) และ Traits ใช้ระบบพันธุกรรมเดียวกับตัวละครหลัก
- **ความสัมพันธ์** มีค่า −100..100 + ประเภท + ความทรงจำ (เหตุการณ์สำคัญ) ค่าจะลดลงเองถ้าไม่ได้ดูแล
  (ลดลงหาค่าพื้นของแต่ละประเภท) และเพิ่มขึ้นจากชั่วโมง `socialize` ในงบเวลา
- **Lightweight Simulation:** NPC แก่ตัวและเสียชีวิตตามโมเดลการตายเดียวกับทั้งเกม แฟน/คู่สมรสที่ความสัมพันธ์แย่ต่อเนื่องจะเลิกเอง
- **การกระทำกับ NPC** (คุย ให้ของขวัญ ทะเลาะ ชวนเดท ขอแต่งงาน เลิก หย่า มีลูก รับบุตรบุญธรรม เก็บคอนเนกชัน) มาจาก content (`interactions.json`)
  มีเงื่อนไขเป็น Rule และโอกาสสำเร็จเป็น FinalChance + ความเข้ากันได้ (Compatibility)
- **ลูก** สืบพันธุกรรมจากตัวละคร + คู่ (ใช้ในข้อ 18 Legacy) และมีค่าเลี้ยงดูรายเดือนจนอายุ 18
- **Network:** เพื่อนร่วมงาน/คอนเนกชันที่สนิทช่วยเพิ่มโอกาสได้งาน

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ที่เก็บ | `CharacterRelationships` (NPC ทุกคน + ความสัมพันธ์ + การตั้งครรภ์) อยู่ใน Character เพื่อให้ Rule อ่าน/แก้ได้ ส่วน `RelationshipService` อยู่ใน LifeSession | เหมือน Finances/Career |
| ประเภท | `parent, sibling, child, partner, spouse, ex, friend, colleague, contact` (enum ในโค้ด แต่ค่าตัวเลขของแต่ละประเภทอยู่ใน content) | Rule และ UI ต้องใช้ชื่อคงที่ |
| เริ่มชีวิต | พ่อแม่จาก `FamilyInfo` กลายเป็น NPC ค่าเริ่มต้นตามบุคลิกพ่อแม่ (warm 70, neglectful 20 ...) + สุ่มพี่น้อง 0–3 คน อายุห่าง ±8 ปี | |
| การดูแล | ทุกเดือน: ชั่วโมง socialize รวม 4 สัปดาห์ × 0.6 แบ่งให้คนที่สนิทที่สุด 5 คน (ไม่รวมครอบครัว) ส่วนครอบครัวที่อยู่ด้วยกัน (ตอนยังพึ่งพ่อแม่) ได้ +2/เดือน **ค่าที่ได้จะลดลงตามความสนิท** (× (1 − ค่า/100)) และทุกความสัมพันธ์จะลดลงหาค่าพื้นตาม `monthlyDecay` ทุกเดือน ความสัมพันธ์จึงไปหยุดที่จุดสมดุล (เพื่อนที่ดูแลปกติ ≈ 60, ครอบครัวที่อยู่ด้วยกัน ≈ 75) | เวลาเป็นต้นทุนของความสัมพันธ์ (ตัดสินใจเพิ่มหลังการจำลอง: แบบเดิมทุกคนได้ Relationships = 100) |
| พบคนใหม่ | ทุกเดือนสุ่ม: ฐาน 5% + 0.4%/ชม. socialize (เป็นเพื่อน) / 25% ถ้าเรียนอยู่ (เพื่อนร่วมชั้น) / 20% ถ้าทำงาน (เพื่อนร่วมงาน) อายุใกล้เคียง ±5 ปี ค่าเริ่ม 5–25 ถ้าเกินเพดาน 25 คน จะตัดคนนอกครอบครัวที่ค่าต่ำสุด | |
| Compatibility | 50 + 8×Trait ที่ตรงกัน − 12×Trait ที่ขัดกัน (กลุ่ม exclusive เดียวกันแต่คนละตัว) − 2×อายุห่าง (ปี) + 0.2×(Looks−50) + 0.1×(Charisma−50) แล้ว Clamp 0–100 พร้อม Breakdown ทุกบรรทัด | อ่านตัวเลขได้ทุกตัว |
| โอกาสของการกระทำ | FinalChance ของ content + (Compatibility − 50) × `compatibilityPerPoint` (ถ้าเปิดใช้) | ใช้ระบบโอกาสเดียวกับทั้งเกม |
| ความรัก | มีแฟนหรือคู่สมรสได้ทีละ 1 คน (บังคับด้วย Rule condition ใน content) แฟน < −10 ติดต่อกัน 3 เดือนจะเลิกเอง ส่วนคู่สมรส < −20 ติดต่อกัน 6 เดือนจะหย่าเอง | |
| มีลูก | `child: "birth"` ต้องมีเพศกำเนิดต่างกัน และทั้งคู่อายุ 18–45 สำเร็จแล้วจะคลอดใน 40 สัปดาห์ / `child: "adoption"` ได้ลูกทันทีโดยไม่สืบพันธุกรรม | เปิดทางให้คู่รักทุกแบบ |
| ค่าเลี้ยงดู | ลูกอายุ < 18 ปี คนละ 6,000 บาท/เดือน (บาทมูลค่าจริง) ถ้าเงินไม่พอจะเข้าระบบยอดขาดของข้อ 10 | |
| NPC เสียชีวิต | ทุกต้นปีสุ่มตาม `MortalityModel.AnnualForAge` (Gompertz × สาธารณสุขของประเทศ) ตัวละครจะเครียด/เศร้าตามประเภทความสัมพันธ์ และความสัมพันธ์จะยังอยู่เป็นความทรงจำ | |
| Core stat | ทุกเดือน: Relationships ขยับ 30% เข้าหา 50 + (ค่าเฉลี่ย 5 อันดับแรก)/2 (ช่องว่างนับเป็น −50) ส่วน Loneliness ขยับเข้าหา 60 − 8 × จำนวนคนที่สนิท (≥ 50) | |
| Network | คนประเภท colleague/contact/friend ที่ค่า ≥ 50 ให้โอกาสได้งาน +3 ต่อคน (สูงสุด +15) ผ่าน `ICareerNetwork` | ไม่เรียก RelationshipService ตรงๆ |
| ชื่อ NPC | ใช้ตัวเขียน native ของประเทศ (สลับตัวเขียนเป็นงานของ UI ข้อ 20) | |
| Random stream | `relationships` | |

## 3. JSON Schema (content ใหม่ 2 ชนิด)
```jsonc
// relationship_settings.json (ไฟล์เดียว)
{ "socialActivity": "socialize", "maxRelationships": 25,
  "types": [ { "type": "friend", "monthlyDecay": 1.5, "floor": 0, "griefStress": 10, "griefHappiness": -8 } ],   // ครบทุกประเภท
  "attention": { "gainPerHour": 0.6, "closestCount": 5, "familyMonthlyGain": 2 },
  "family": { "parentStart": [ { "personality": "warm", "value": 70 } ], "defaultParentStart": 50, "siblingStart": 40, "childStart": 70,
              "siblings": [ { "count": 0, "weight": 3 } ], "siblingAgeGapYears": 8 },
  "meeting": { "monthlyBase": 0.05, "perSocialHour": 0.004, "school": 0.25, "work": 0.2, "startValue": { "min": 5, "max": 25 }, "ageGapYears": 5 },
  "compatibility": { "base": 50, "sharedTrait": 8, "conflictingTrait": -12, "perYearAgeGap": -2, "looksWeight": 0.2, "charismaWeight": 0.1 },
  "romance": { "breakupBelow": -10, "breakupAfterMonths": 3, "divorceBelow": -20, "divorceAfterMonths": 6 },
  "children": { "fertileAge": { "min": 18, "max": 45 }, "pregnancyWeeks": 40, "monthlyCost": 6000, "costUntilAge": 18 },
  "stats": { "closeValue": 50, "topCount": 5, "emptySlotValue": -50, "monthlySmoothing": 0.3, "lonelinessBase": 60, "lonelinessPerCloseTie": -8 },
  "network": { "minValue": 50, "pointsPerContact": 3, "maxPoints": 15 } }

// interactions.json (ชนิด interactions)
{ "interactions": [ {
    "id": "ask_out", "nameKey": "interaction.ask_out.name", "types": ["friend"], "minValue": 40, "maxValue": null,
    "condition": { "type": "relationship", "relation": "partner", "count": { "op": "==", "value": 0 } },
    "costCash": 0, "cooldownWeeks": 12, "chance": { "base": 35, "fortune": "love" }, "compatibilityPerPoint": 1,
    "success": { "value": 10, "becomes": "partner", "child": null, "memoryKey": "memory.first_date", "effects": [] },
    "failure": { "value": -10, "becomes": null, "child": null, "memoryKey": null, "effects": [] } } ] }
```
**Rule ใหม่**
- Condition `{ "type": "relationship", "relation": "spouse", "count": { "op": ">=", "value": 1 } }` (นับคนที่ยังมีชีวิต)
  หรือ `{ "type": "relationship", "relation": "friend", "op": ">=", "value": 60 }` (คนที่ค่าสูงสุดของประเภทนั้น, ไม่มี = เท็จ) ใช้ `"relation": "any"` ได้
- Effect `{ "type": "relationship", "relation": "parent", "amount": 5, "target": "closest" | "all", "memory": "memory.key" }`

## 4. C# Domain (`BetterLife.Core.Relationships`)
| กลุ่ม | Class |
|---|---|
| ข้อมูล | `RelationshipType`, `RelationshipNames`, `Npc`, `Relationship`, `RelationshipMemory`, `Pregnancy`, `CharacterRelationships` |
| ค่าตั้ง | `RelationshipTypeRules`, `AttentionRules`, `FamilyRules`, `MeetingRules`, `CompatibilityRules`, `RomanceRules`, `ChildRules`, `RelationshipStatRules`, `NetworkRules`, `RelationshipSettings` |
| การกระทำ | `InteractionDefinition`, `InteractionOutcome`, `ChildMode`, `InteractionQuote`, `InteractionResult`, `CompatibilityBreakdown` |
| บริการ | `NpcFactory`, `RelationshipService` (+ `ICareerNetwork`), `RelationshipMonthPhase`, `RelationshipYearPhase` |
| Events | `MetPersonEvent`, `RelationshipTypeChangedEvent`, `ChildBornEvent`, `NpcDiedEvent` |
| Rules | `RelationshipCondition`, `RelationshipEffect` |

## 5. Save DTO
`CharacterDto.Relationships = RelationshipsDto { Npcs[NpcDto {Id, GivenName, FamilyName, Sex, BirthWeek, Genome[StatValueDto], Traits[], Alive, DiedWeek}],
Relationships[RelationshipDto {NpcId, Type, Value, SinceWeek, LastContactWeek, LowMonths, Memories[{Key, Week, Delta}], Cooldowns[AttemptDto]}],
Pregnancies[{PartnerNpcId, DueWeek}], NextNpcId }`

## 6. Tests
- `RelationshipServiceTests` (16): ครอบครัวเริ่มต้น, การดูแล/จางหาย, ครอบครัวที่อยู่ด้วยกัน, พบคนใหม่ + ตัดคนที่ห่าง, Compatibility, เหตุผลที่ทำไม่ได้, ของขวัญ + ความทรงจำ + cooldown, ชวนเดทตามโอกาสที่แสดง, ขอแต่งงาน, มีลูก (เพศ/อายุ/40 สัปดาห์), รับบุตรบุญธรรม, ค่าเลี้ยงดู, แฟน/คู่สมรสที่ไม่มีความสุขจากไปเอง, NPC เสียชีวิต + ความเศร้า, Relationships/Loneliness, Network
- `RelationshipRuleTests` (10), `RelationshipPersistenceTests` (2), `RelationshipContentTests` (8, รวมโบนัส Network ใน CareerService), `LifeSessionTests` (พ่อแม่เป็น NPC ตั้งแต่เกิด)
- `RelationshipSimulationTests` (content จริง 30 ชีวิตถึงอายุ 40 พร้อมผู้เล่นจำลองที่ชวนเดท/ขอแต่งงาน/มีลูก): มีชีวิต 28, เคยมีแฟน 28, แต่งงาน 28, มีลูก 14 คน (28 คน), เพื่อนเฉลี่ย 12 คน, Relationships ≈ 78

## 7. ส่งต่อ
- ข้อ 13: เพื่อนชั่ว (คบคนชั่ว = ตัวคูณ) ใช้ Trait/Flag ของ NPC, ครอบครัวจับได้ว่ามีอบายมุข
- ข้อ 16–17: หน้า Relationships (ใช้ `Options` / `Quote` / `Compatibility` ที่มี Breakdown)
- ข้อ 18: เล่นต่อเป็นลูก (NPC ประเภท child มีพันธุกรรมจริง), มรดก, ต้นไม้ครอบครัว
- ข้อ 21: ปรับสมดุลอัตราพบคน/ลดลงของความสัมพันธ์
