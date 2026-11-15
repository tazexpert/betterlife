# Task 20: Localization TH/EN + Settings

> สถานะ: ✅ **Implemented** — 2026-11-15 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **ฟอนต์ไทย:** ฟอนต์เริ่มต้นของ Unity ไม่มีอักษรไทย จึงเพิ่มฟอนต์ที่มีทั้งไทยและละติน
- **สลับภาษา** ไทย/อังกฤษได้ทันทีจากหน้า Settings ภาษาเริ่มต้นตามภาษาของเครื่อง จำค่าไว้ในโปรไฟล์ผู้เล่น
- **ข้อความครบทั้งเกม:** ทุก key ใน content และในโค้ด (รวมเงื่อนไข `rule.cond.*` ที่ค้างจากข้อ 16) มีข้อความทั้งสองภาษา และมี Test กันหลุด
- **Settings:** ภาษา, เปิด/ปิดเนื้อหาอบายมุข (มีตั้งแต่ข้อ 13), สถานะ Undo วันนี้, ล้าง Achievement (ยืนยัน 2 ขั้น), เวอร์ชัน content

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ฟอนต์ | **Sarabun** (SIL Open Font License 1.1, Regular + Bold) อยู่ที่ `Resources/BetterLife/UI/Fonts` พร้อมไฟล์ลิขสิทธิ์ ตั้งใน USS ด้วย `-unity-font` ที่ `.shell` (สืบทอดไปทุก element) | มีไทย 87 ตัว ละติน ฿ – × ครบ ลิขสิทธิ์ใช้ในเกมเชิงพาณิชย์ได้ ได้ไฟล์จากแพ็กเกจ npm `@expo-google-fonts/sarabun` (ไฟล์เดียวกับ Google Fonts) |
| ข้อจำกัดฟอนต์ | Unity 2022.3 (TextCore) อาจวางสระบน/ล่างและวรรณยุกต์ซ้อนกันในบางคำ เพราะยังไม่รองรับ GPOS ของภาษาไทยเต็มที่ ต้องตรวจในเครื่องจริง | ไม่เดาความสามารถของ Engine |
| ภาษาเริ่มต้น | `Application.systemLanguage == SystemLanguage.Thai` → `th` นอกนั้น `en` เมื่อผู้เล่นเลือกเองแล้วใช้ค่าที่เลือก | |
| ที่เก็บค่า | `GameSettings { Language, ViceContent }` ใน Slot โปรไฟล์ `profile_settings` (JSON) เหมือนโควตา Undo/Achievement | ไม่ผูกกับชีวิต |
| สลับภาษา | สร้าง UI ใหม่ทั้งชุด (Localizer → ScreenContext → Presenter ทั้งหมด) แล้วกลับแท็บ More | ทุกข้อความ (รวมที่ UXML) แปลใหม่แน่นอน ไม่ต้องให้ทุก Presenter รองรับการเปลี่ยนภาษาเอง |
| ชื่อภาษา | key `language.self` ในแต่ละตาราง ("English", "ไทย") ปุ่มเลือกภาษาแสดงชื่อในภาษานั้นเอง | ผู้เล่นอ่านภาษาตัวเองออกเสมอ |
| เนื้อหาอบายมุข | ปิด = `ViceService.ContentEnabled = false` ทุกชีวิต (ใหม่/โหลด/ทายาท) | ตาม GDD ข้อ 13 |
| ล้าง Achievement | ปุ่มแรกเปลี่ยนเป็น "กดอีกครั้งเพื่อยืนยัน" | ป้องกันกดพลาด |
| ข้อความเงื่อนไข | แปล `rule.cond.*` ทั้งหมด และ `RuleTextFormatter` แปล `{subject}` ที่เป็น id ดิบตามชนิดเงื่อนไข (ความสัมพันธ์ ชื่อเสียง อาการป่วย ดวง ประเภทอาชีพ ชนิดหนี้) | ตัวเลือกที่ล็อกใน Event Dialog แสดง **เงื่อนไขที่ยังไม่ผ่าน** แทน "ยังไม่ผ่านเงื่อนไข" เฉยๆ |
| Test ครอบคลุม | (1) ทุกค่าของฟิลด์ `*Key` ในไฟล์ content ทุกไฟล์ (2) ทุกค่าคงที่ที่ขึ้นต้นด้วย `rule.` ใน Core (reflection) (3) ทุกภาษามี key ชุดเดียวกัน (มีอยู่แล้ว) | content ใหม่ในข้อ 22 จะไม่หลุดคำแปล |

## 3. JSON
```jsonc
// profile_settings (Slot โปรไฟล์)
{ "Language": "th", "ViceContent": true }
```
ไม่มี content ชนิดใหม่ เพิ่ม key ใน `strings/*.json`

## 4. C# Domain
| ที่อยู่ | Class |
|---|---|
| Core `BetterLife.Core.Settings` | `GameSettings`, `SettingsStore` |
| Core `BetterLife.Core.ViewModels` | `SettingsState`, `SettingsViewModel`, `RuleTextFormatter` (แปล subject), `EventDialogViewModel` (เงื่อนไขที่ล็อก) |
| Core `BetterLife.Core.Saving` | `SlotNames.Settings` |
| Core `BetterLife.Core.Navigation` | `ScreenIds.Settings` |
| Unity | `ISettingsController`, `SettingsScreenPresenter`, ฟอนต์ใน `Theme.uss`, `GameBootstrap` (ภาษาเริ่มต้น, สร้าง UI ใหม่, ใช้ค่าตั้งกับทุกชีวิต) |

## 5. Save
เซฟชีวิตไม่เปลี่ยน ค่าตั้งอยู่ใน `profile_settings`

## 6. Tests
- `SettingsTests` (8): เก็บ/โหลดค่าตั้ง + ไฟล์เสีย = ค่าเริ่มต้น + Slot โปรไฟล์, การเลือกภาษา (เลือกเอง → ภาษาเครื่อง → en) 4 กรณี, หน้าจอแสดงชื่อภาษาในภาษานั้นเอง, `{subject}` ที่เป็น id ดิบแปลเป็นชื่อ (ไม่มีชื่อ = แสดง id)
- `EventDialogViewModelTests`: ตัวเลือกที่ล็อกแสดงเงื่อนไขที่ยังไม่ผ่าน, `LegacyTests`: ล้าง Achievement แล้วบันทึก
- `StringsContentTests` (+3): **ทุก `*Key` ในทุกไฟล์ content**, ทุก key ของข้อความที่ผู้เล่นเห็นในโค้ด Core (`rule.*`, `creation.issue.*`, `undo.reason.*`, `scam.targeting.*`, `vice.temptation.*`, `vice.use.*`, `source.*`), ทุกตารางมีชื่อภาษาตัวเอง
- ตาราง en/th มี 924 key เท่ากัน

## 7. ส่งต่อ
- ข้อ 19: ปุ่ม Remove Ads / Restore purchases ในหน้า Settings
- ข้อ 21: ตรวจความยาวข้อความไทยที่ล้นปุ่มบนหน้าจอเล็ก
- ข้อ 23: ตรวจการแสดงผลอักษรไทยบนอุปกรณ์จริง (ถ้าสระซ้อนมากให้พิจารณา Unity 6 หรือ TextMeshPro ที่รองรับ Mark positioning)
