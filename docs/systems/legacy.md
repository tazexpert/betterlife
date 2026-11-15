# Task 18: Legacy + Life Summary + Achievements

> สถานะ: ✅ **Implemented** — 2026-11-15 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- เมื่อตัวละครเสียชีวิต แสดง **Life Summary**: อายุ สาเหตุ คำไว้อาลัยที่สร้างจากข้อมูลจริงของชีวิต (Flags ที่มี `eulogyKey`, สาเหตุการตาย, การศึกษา อาชีพ ครอบครัว ทรัพย์สิน) และ Achievement ที่ปลดล็อกในชีวิตนี้ คัดลอกเป็นข้อความเพื่อแชร์ได้
- **Legacy Mode:** เล่นต่อเป็นลูกที่ยังมีชีวิตอยู่ ได้รับ **มรดก** (หักหนี้และภาษีมรดก แบ่งเท่ากันระหว่างลูกที่มีชีวิต) **ชื่อเสียง** และ **กรรม** บางส่วน Flags ที่ตกทอดได้ และนับเป็นรุ่นถัดไป
- **Family Tree:** รายชื่อทุกรุ่นของตระกูล (เกิด–ตาย สาเหตุ ทรัพย์สินตอนเสียชีวิต)
- **Achievements:** content ชนิดใหม่ ใช้ Condition ของ Rules Engine เดิม ปลดล็อกข้ามชีวิต (เก็บในโปรไฟล์ผู้เล่น)

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ทายาท | (จากการจำลอง: ถ้าผู้เล่นไม่ได้มีลูกเอง ชีวิตอัตโนมัติจะไม่มีลูก) เลือกได้จาก **ลูกที่ยังมีชีวิต** ทุกคน เริ่มเล่น ณ สัปดาห์ที่พ่อ/แม่เสียชีวิต ด้วยอายุจริงของลูก | ตรงกับ GDD "เล่นต่อเป็นลูกหลาน" |
| ตัวตนทายาท | ชื่อ เพศ วันเกิด ยีน (Genome → ค่าพื้นฐาน) และนิสัยจาก NPC เดิม เมือง/ประเทศและพื้นหลังครอบครัวเดียวกับผู้ตาย | ข้อมูลมีอยู่แล้วใน NPC |
| ครอบครัวของทายาท | ผู้ตาย = พ่อ/แม่ (เสียชีวิตแล้ว), คู่สมรส/แฟนที่ยังอยู่ของผู้ตาย = พ่อ/แม่ (ระบบไม่ได้เก็บว่าใครเป็นพ่อแม่อีกคน จึงใช้คู่ปัจจุบัน), ลูกคนอื่น = พี่น้อง ความสนิทเริ่มจากค่าเดิมที่มีกับผู้ตาย | |
| การศึกษาของทายาทที่โตแล้ว | ได้วุฒิของหลักสูตรภาคบังคับ/อัตโนมัติ (`AutoEnroll`) ที่อายุพอจะเรียนจบแล้ว (สถานศึกษาที่ระบบเลือกอัตโนมัติ ผลการเรียน 60) | NPC ไม่มีประวัติการเรียน ถ้าไม่มีวุฒิเลยจะเสียเปรียบเกินจริง |
| งานของทายาท | ไม่มีงาน (สมัครเองในแท็บ Work) | |
| มรดก | กองมรดก = เงินสด + มูลค่าสินทรัพย์ − หนี้ทั้งหมด (ขายทุกอย่างเป็นเงินสด) ถ้า ≤ 0 = ไม่ได้อะไร **และไม่ต้องรับหนี้** (กฎหมายไทย: ทายาทรับผิดไม่เกินมรดก) ภาษีมรดก 5% ของส่วนที่เกิน 100 ล้านบาท (มูลค่าจริง) แล้วแบ่งเท่ากันระหว่างลูกที่มีชีวิต | เรียบง่ายและตรงกฎหมายไทย |
| ชื่อเสียง/กรรม | ชื่อเสียงทุกวงการ × 0.3, กรรมดีและกรรมชั่ว × 0.2 (หนี้กรรมที่ค้างไม่ตกทอด) | "กรรมตกทอด" แต่ไม่ลงโทษลูกเต็มๆ |
| Flags | คัดลอก Flag ที่ `inheritable: true` | content มีอยู่แล้ว |
| เศรษฐกิจ | ดำเนินต่อจากชีวิตเดิม (ช่วงเศรษฐกิจ เงินเฟ้อ ระดับราคา) | โลกเดียวกัน |
| Random | ชีวิตใหม่ใช้ `RandomRegistry(FNV-1a("{seed เดิม}:{npcId}"))` ทำซ้ำได้เสมอ | Deterministic |
| ค่าตั้งต้น | `content/legacy_settings.json` (ไฟล์เดียว) | ห้าม magic number |
| Family Tree | `Lineage` (รายการบรรพบุรุษ) อยู่ใน `LifeSession` และเซฟใน `LifeSaveDto.Lineage` (ฟิลด์ใหม่ เซฟเก่าที่ไม่มี = ว่าง ไม่ต้องเพิ่ม saveVersion) รุ่นปัจจุบัน = `Character.Generation` (เซฟใน `CharacterDto.Generation`) | Rule อ่านรุ่นได้จาก Character |
| Achievement | content `achievements.json`: `{ id, nameKey, descKey, hidden, condition }` ประเมินหลังข้ามเวลาทุกครั้งและตอนเสียชีวิต ปลดล็อกครั้งเดียวตลอดไป เก็บใน Slot โปรไฟล์ `profile_achievements` (เหมือนโควตา Undo) | Condition = Strategy + Registry เดิม |
| Condition ใหม่ | `{ "type": "generation", "op": ">=", "value": 3 }` | Achievement ตระกูล |
| แชร์ | ปุ่มคัดลอกข้อความสรุป (`GUIUtility.systemCopyBuffer`) การแชร์ผ่านแอปอื่นต้องใช้ Native plugin (ข้อ 23) | ไม่เดา API ของ OS |
| เมื่อตาย | เปิด Life Summary อัตโนมัติ ปุ่มข้ามเวลาปิด (มีอยู่แล้ว) ถ้าไม่มีทายาท มีปุ่ม "เริ่มชีวิตใหม่" (ตระกูลใหม่) | |

## 3. JSON Schema (content ใหม่ 2 ชนิด)
```jsonc
// achievements.json
{ "achievements": [
  { "id": "centenarian", "nameKey": "achievement.centenarian.name", "descKey": "achievement.centenarian.desc", "hidden": false,
    "condition": { "type": "age", "op": ">=", "value": 100 } } ] }

// legacy_settings.json (ไฟล์เดียว)
{ "taxExemption": 100000000, "taxRate": 0.05, "reputationShare": 0.3, "karmaShare": 0.2,
  "heirEducationPerformance": 60 }
```

## 4. C# Domain
| ที่อยู่ | Class |
|---|---|
| Core `BetterLife.Core.Legacy` | `LegacySettings`, `AncestorRecord`, `Lineage`, `InheritanceReport` (รวมการคำนวณกองมรดก `Settle`), `HeirCreator`, `LegacyService`, `GenerationCondition` (+ Converter) |
| Core `BetterLife.Core.Achievements` | `AchievementDefinition`, `AchievementUnlock`, `AchievementProfile`, `AchievementStore`, `AchievementTracker` |
| Core `BetterLife.Core.ViewModels` | `LifeSummaryState`, `LifeSummaryViewModel`, `HeirOption`, `FamilyTreeViewModel`, `AchievementRow`, `AchievementsViewModel` |
| Core อื่นๆ | `Character.Generation`, `LifeSession.Lineage`, `LifeSessionFactory.CreateHeir`, `SlotNames.Achievements` + `IsProfile` (รายการเซฟไม่แสดง Slot โปรไฟล์ทุกตัว), `NoticeFeed.Add(Notice)` (แจ้ง Achievement ใน Inbox), `AncestorDto` |
| Unity | `ILifeController` (GameBootstrap ทำ), `LifeSummaryScreenPresenter`, `FamilyTreeScreenPresenter`, `AchievementsScreenPresenter` |

## 5. Save DTO
- `LifeSaveDto.Lineage = [ { Generation, GivenName, FamilyName, BirthWeek, DeathWeek, CauseId, AgeYears, NetWorth (nominal ตอนเสียชีวิต, decimal string) } ]`
- `CharacterDto.Generation` (0 ในเซฟเก่า = รุ่นที่ 1)
- โปรไฟล์ `profile_achievements`: `{ "Unlocked": [ { "Id": "centenarian", "Day": 20261115, "Generation": 2, "Name": "สมชาย ใจดี" } ] }`

## 6. Tests
- `LegacyTests` (6): กองมรดก (หักหนี้ → ภาษีส่วนที่เกินเกณฑ์ → แบ่งเท่า, หนี้เกินมรดก = ได้ 0 ไม่รับหนี้), ทายาท (ชื่อ รุ่น เวลาเริ่ม อายุ เศรษฐกิจเดิม มรดก กรรม Flag ที่ตกทอด/ไม่ตกทอด พ่อแม่ 2 คนโดยคนหนึ่งเสียชีวิต พี่น้อง วุฒิภาคบังคับ Lineage ชีวิตเดินต่อได้), ทายาทซ้ำ = ผลเหมือนกันทุกไบต์ + เซฟ/โหลดแล้ว Generation/Lineage ครบ, ยังมีชีวิต = ไม่มีทายาท, Achievement (ปลดล็อกครั้งเดียว, อยู่ในโปรไฟล์ข้ามชีวิต, Slot โปรไฟล์ไม่อยู่ในรายการเซฟ, `generation` ของทายาท, ซ่อนจนปลดล็อก), Life Summary (คำไว้อาลัย แต่งงาน ลูก Flag ทายาท ข้อความแชร์) + Family Tree
- `LegacySimulationTests` (content จริง): 6 ตระกูล × 3 รุ่น (ผู้เล่นจำลองมีลูก 2 คนตอนอายุ 30 เพราะการมีลูกเป็นการตัดสินใจของผู้เล่น) ทุกตระกูลถึงรุ่นที่ 3 และ Achievement จริงปลดล็อกได้
- `StringsContentTests` (+1): ชื่อ/คำอธิบาย Achievement คำไว้อาลัยจาก Flag และสาเหตุการตาย UI ข้อ 18 ครบทุกภาษา
- `RuleRegistryTests`: ตัวอย่าง condition `generation`

## 7. ส่งต่อ
- ข้อ 19: Achievement ที่ได้รับรางวัล/ปลดล็อก Cosmetic, ของที่ซื้อแล้ว (Remove Ads) อยู่ในโปรไฟล์ร่วมกับ Achievement
- ข้อ 20: หน้า Settings เลือก Slot/ลบความคืบหน้า Achievement
- ข้อ 21: ปรับภาษีมรดก/ส่วนแบ่งกรรมจากการจำลองหลายรุ่น
- ข้อ 22: Achievement เพิ่มเติม
- ข้อ 23: แชร์ผ่าน Native share sheet
