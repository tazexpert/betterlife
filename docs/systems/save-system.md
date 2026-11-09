# Task 14: Save System (JSON + Migration + Undo snapshot)

> สถานะ: ✅ **Implemented** — 2026-11-09 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- เซฟทั้งชีวิตเป็น JSON ไฟล์เดียวต่อ Slot: ตัวละคร (การเงิน การศึกษา อาชีพ ความสัมพันธ์ อบายมุข) + เวลา/งบเวลา + เหตุการณ์ + Random streams + เศรษฐกิจ
- โหลดแล้วต้องเล่นต่อได้ **เหมือนไม่เคยเซฟ** (seed + สถานะ random เดิม = ผลลัพธ์เดิมทุกครั้ง)
- มี Migration ของรูปแบบเซฟ (saveVersion) เป็นขั้นๆ และล้าง id ที่ถูกลบออกจาก content (ได้คำเตือน ไม่พัง)
- Undo: เก็บ Snapshot ก่อนการตัดสินใจล่าสุด ใช้ได้วันละ 3 ครั้ง ย้อนได้แค่ครั้งล่าสุด ปิดในโหมด Ironman และย้อนผลการพนันไม่ได้

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| รูปแบบไฟล์ | Envelope `{ "format": "betterlife.save", "saveVersion": 1, "checksum": "<FNV-1a 64 hex>", "payload": { LifeSaveDto } }` ใช้ Newtonsoft (DTO เป็น public field ทั้งหมด, เงินเป็น decimal string) | ตรวจไฟล์เสียหายได้ และอ่านเวอร์ชันได้ก่อนแปลงเป็น DTO |
| Checksum | FNV-1a 64 ของ payload แบบไม่มีช่องว่าง (ป้องกันไฟล์เสีย ไม่ได้ป้องกันการโกง) | ไม่ต้องใช้ crypto |
| Migration | `ISaveMigration` (FromVersion → FromVersion + 1) แก้ JObject ของ payload ทีละขั้นใน `SaveMigrationRegistry` ก่อนแปลงเป็น DTO ตอนนี้เวอร์ชันปัจจุบันคือ 1 จึงยังไม่มี migration จริง แต่มี Test ที่ใช้ migration ทดสอบ | เพิ่มเวอร์ชันใหม่ = เพิ่ม class + ลงทะเบียน |
| เซฟใหม่กว่าเกม | saveVersion > ที่เกมรู้จัก หรือ contentVersion ของเซฟ > content ปัจจุบัน → ปฏิเสธ (`SaveErrorKind.TooNew`) | ไม่เดาข้อมูลจากอนาคต |
| Content ถูกลบ | ใช้ `DropUnknown` ของแต่ละระบบ (การเงิน การศึกษา อาชีพ อบายมุข) + mapper เดิมของ Character/Time/Event/Economy คืนคำเตือนทั้งหมดใน `LifeSessionLoadResult.Warnings` | |
| ที่เก็บ | `ISaveStorage` (Interface ใน Core) + `InMemorySaveStorage` (Fake) + `FileSaveStorage` (System.IO, เขียนไฟล์ชั่วคราวแล้วค่อยแทนที่ = atomic) Unity ส่ง `Application.persistentDataPath/saves` ให้ | Service ภายนอกต้องมี Interface + Fake (Cloud Save ใช้ Interface เดียวกันในอนาคต) |
| Slot | ชื่อ `a-z0-9_-` ยาวไม่เกิน 32 ตัว + Slot พิเศษ `autosave` | กัน path traversal |
| สิ่งที่ไม่เซฟ | Cache ชั่วโมงของสัปดาห์ที่แล้ว (ใช้แค่แสดงผล Laziness/รายงาน) และ `FinanceService.LastReport` จะว่างหลังโหลดจนจบเดือนถัดไป ส่วนชั่วโมง socialize ที่สะสมในเดือนนั้นจะ **ถูกเซฟ** (`PendingSocialHours`) | ส่วนที่มีผลต่อผลลัพธ์ต้องเซฟครบ |
| ประกอบ Session | `LifeSessionFactory.CreateNew` และ `Load` ใช้ `Compose` เดียวกัน (ต่างกันแค่ว่าสร้างครอบครัว/เศรษฐกิจใหม่หรือไม่) | ระบบใหม่ต้องเพิ่มที่เดียว |
| Ironman / โหมด | `LifeSession.Mode` (Random/Custom/Challenge) และ `IsIronman` อยู่ในเซฟ | |
| Undo | `UndoService.Checkpoint(session)` เก็บเซฟในหน่วยความจำ 1 ชุด (UI เรียกก่อนทุกการตัดสินใจ) `Undo` คืน Session ใหม่จาก Snapshot แล้วลบ Snapshot | ย้อนได้แค่การตัดสินใจล่าสุด |
| เงื่อนไข Undo | ไม่ใช่ Ironman, มี Snapshot, ยังไม่ครบ 3 ครั้งของวันนั้น (วันตามนาฬิกาเครื่อง `IRealTimeClock`) และ `CharacterVices.IrreversibleActions` ไม่เพิ่มหลัง Snapshot (มีการพนัน = ย้อนไม่ได้) | ตาม GDD ข้อ 11 และ 13 |
| โควตา Undo | `UndoQuota` (วันที่ + จำนวนที่ใช้) เก็บใน Slot `profile_undo` แยกจากเซฟของชีวิต | เปลี่ยนชีวิตก็ไม่ได้โควตาใหม่ |
| Rewarded Ad | การดูโฆษณาก่อน Undo เป็นงานข้อ 19 (UI เรียก Ads ก่อน แล้วค่อยเรียก `Undo`) | Core ไม่รู้จัก Ads |
| Unity | `GameBootstrap`: เปิดเกมแล้วโหลด `autosave` ถ้ามี (ไม่มีหรือเสีย → ชีวิตใหม่) และ autosave ตอน `OnApplicationPause(true)` / `OnApplicationQuit` + `link.xml` กัน IL2CPP ตัด DTO | |

## 3. JSON (Save DTO)
```jsonc
{ "format": "betterlife.save", "saveVersion": 1, "checksum": "9f3a…",
  "payload": { "SaveVersion": 1, "ContentVersion": 3, "Mode": "Random", "Ironman": false,
    "Summary": { "GivenName": "สมชาย", "FamilyName": "ใจดี", "Week": 2730, "AgeYears": 32, "Alive": true },
    "Character": { ... CharacterDto ... }, "Time": { ... TimeStateDto ... }, "Events": { ... EventStateDto ... },
    "Random": { ... RandomRegistryDto ... }, "Economy": { ... EconomyStateDto ... }, "PendingSocialHours": 24 } }
```
- `CharacterVices` เพิ่ม `IrreversibleActions` (และ DTO `IrreversibleActions`)
- โควตา Undo: `{ "Day": 20261109, "Used": 2 }`

## 4. C# Domain
| Namespace | Class |
|---|---|
| `BetterLife.Core.Persistence` | `LifeSaveDto`, `SaveSummaryDto`, `LifeSaveMapper` |
| `BetterLife.Core.Saving` | `SaveFormat`, `ISaveStorage`, `InMemorySaveStorage`, `FileSaveStorage`, `SlotNames`, `ISaveMigration`, `SaveMigrationRegistry`, `SaveSerializer`, `SaveReadResult`, `SaveErrorKind`, `SaveLoadException`, `SaveService`, `IRealTimeClock`, `SystemRealTimeClock`, `UndoQuota`, `UndoQuote`, `UndoService` |
| `BetterLife.Core.Sessions` | `LifeSessionFactory.Load` / `Compose`, `LifeSessionLoadResult` |

## 5. Tests
- `SaveRoundTripTests` (4): เซฟแล้วโหลด → สถานะเหมือนเดิมทุกไบต์ และเล่นต่ออีก 8 ปีก็ยังเหมือนต้นฉบับทุกไบต์ (รวมการพนัน), โหมด/Ironman, content ที่ถูกลบ → คำเตือน, Summary
- `SaveSerializerTests` (7): Envelope, ไฟล์เสีย/ไม่ใช่เซฟ, checksum ไม่ตรง, migration หลายขั้นตามลำดับ, ไม่มี migration / เซฟใหม่กว่า
- `SaveStorageTests` (8): ไฟล์จริง (เขียนทับแบบ atomic, ไม่เหลือไฟล์ชั่วคราว), ชื่อ Slot ไม่ถูกต้อง, รายการ Slot (เสียแสดงเป็น Corrupt, ไม่แสดง profile), content ใหม่กว่า
- `UndoServiceTests` (4): ย้อนได้ครั้งเดียว, วันละ 3 ครั้ง (ข้ามวันได้ใหม่ และจำไว้แม้สร้าง Service ใหม่), Ironman, พนันหลัง Checkpoint ย้อนไม่ได้
- `SaveSimulationTests` (content จริง 5 ชีวิต): เซฟตอนอายุ 25 แล้วเล่นต่อ 10 ปีทั้งต้นฉบับและตัวที่โหลด → เหมือนกันทุกไบต์ ขนาดเซฟใหญ่สุด ≈ 46 KB

## 6. ส่งต่อ
- ข้อ 15–17: หน้าเลือก Slot (`SaveService.Summaries`), ปุ่ม Undo (`UndoService.Quote`), เรียก `Checkpoint` ก่อนตอบเหตุการณ์/ทำ interaction/สมัครงาน/พนัน
- ข้อ 18: เซฟ Legacy (เล่นต่อเป็นลูก) ใช้ `LifeSaveDto` เดิม + ข้อมูลตระกูล
- ข้อ 19: Rewarded Ad ก่อน Undo, Cloud Save ใช้ `ISaveStorage`
- ข้อ 20: ตั้งค่าผู้เล่น (ปิดเนื้อหาอบายมุข ฯลฯ) เก็บใน Slot profile แยก
