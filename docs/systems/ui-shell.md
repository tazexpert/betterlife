# Task 15: UI Shell

> สถานะ: ✅ **Implemented** — 2026-11-15 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- โครงหน้าจอหลักของเกม (UI Toolkit, แนวตั้ง, ใช้มือเดียวได้): **Top bar** (ชื่อ อายุ วันที่ เงิน จำนวนเรื่องใน Inbox + ปุ่มข้ามเวลา),
  **พื้นที่หน้าจอ** ตรงกลาง, **Bottom nav 6 แท็บ**, ชั้น **Modal** และการรองรับ **Safe area** (รอยบาก/แถบล่างของมือถือ)
- **ScreenRouter**: แต่ละแท็บมีประวัติหน้าจอ (stack) ของตัวเอง, กดแท็บเดิมซ้ำ = กลับหน้าแรกของแท็บ, ปุ่ม Back ของ Android
  ปิด Modal → ถอยหน้า → กลับแท็บ Home → ขอออกจากเกม
- **Theme USS**: สี ระยะ ขนาดตัวอักษรเป็นตัวแปร USS ที่เดียว
- หน้าจอจริงของแต่ละแท็บเป็นงานข้อ 16–17 ตอนนี้ทุกแท็บแสดงหน้า Placeholder

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| แยกชั้น | ลอจิกที่ไม่ผูกกับ Unity (Router, ข้อความ Top bar, รูปแบบเงิน, ตัวแปลภาษา) อยู่ใน Core และทดสอบด้วย `dotnet test` ได้ ส่วน Unity มีแค่ View (UXML/USS) + Presenter ที่บางที่สุด | โค้ด Unity คอมไพล์ด้วย dotnet ไม่ได้ จึงย้ายส่วนที่ผิดง่ายมาไว้ฝั่งที่ทดสอบได้ |
| 6 แท็บ | `home` (Dashboard), `life` (งบเวลา/แผนชีวิต), `money` (การเงิน), `people` (ความสัมพันธ์), `work` (การเรียน/อาชีพ), `more` (ดวง ไทม์ไลน์ นิสัย เซฟ ตั้งค่า) | ตรงกับระบบที่มีแล้ว |
| สร้างจากโค้ด | `UiRoot` สร้าง GameObject + `PanelSettings` (`ScriptableObject.CreateInstance`) + `UIDocument` เอง และโหลด UXML/USS/TSS จาก `Resources/BetterLife/UI` | ข้อบังคับ: ห้ามลากวางใน Inspector |
| สเกลหน้าจอ | Scale With Screen Size, อ้างอิง 1080×1920, match ตามความกว้าง | เกมแนวตั้ง |
| Safe area | ทุกเฟรมเทียบ `Screen.safeArea` ถ้าเปลี่ยนจะแปลงเป็น padding ของ container ราก | รองรับการหมุนและรอยบาก |
| Back | อ่าน `Input.GetKeyDown(KeyCode.Escape)` (ปุ่ม Back ของ Android) → `ScreenRouter.Back()` ถ้าได้ `Exit` จะเรียก `Application.Quit()` | โปรเจกต์ใช้ Input Manager แบบเดิม |
| ข้อความ | ทุกข้อความผ่าน `ILocalizer` ใน UXML เขียน key ขึ้นต้นด้วย `@` (เช่น `text="@nav.home"`) แล้ว Presenter แปลงให้ | ข้อบังคับ: ห้าม hard-code ข้อความ |
| ตารางภาษา | เริ่ม content ชนิด `strings` (`content/strings/en.json`, `th.json`) เฉพาะ key ของ Shell และมี Test ว่าทุกภาษามี key ครบเท่ากัน ภาษาเริ่มต้นตอนนี้คือ **en** | ฟอนต์ภาษาไทย + สลับภาษา + key ทั้งเกมเป็นงานข้อ 20 (ฟอนต์เริ่มต้นของ Unity ไม่มีอักษรไทย) |
| key ที่ไม่มี | แสดงตัว key เอง (เห็นทันทีว่าขาดอะไร) | |
| เงินบน Top bar | แสดง **ราคาตามยุค** = เงินจริง × `PriceLevel` (ตามสเปกข้อ 10) แบบย่อ (`฿12.3K`, `฿4.5M`) | พื้นที่ Top bar น้อย |
| ปุ่มข้ามเวลา | สัปดาห์ / เดือน / ปี ใน Top bar เรียก `LifeSession.Advance` แล้วรีเฟรช ปิดเมื่อชีวิตจบ | หน้า Dashboard จริงอยู่ข้อ 16 |

## 3. JSON (content ใหม่ 1 ชนิด)
```jsonc
// content/strings/en.json (ชนิด strings, หนึ่งไฟล์ต่อภาษา)
{ "language": "en", "strings": { "nav.home": "Home", "topbar.age": "Age {age}", "money.compact.million": "฿{value}M" } }
```

## 4. C# Domain
| ที่อยู่ | Class |
|---|---|
| Core `BetterLife.Core.Localization` | `ILocalizer`, `StringTable`, `StringTables`, `TableLocalizer` |
| Core `BetterLife.Core.Navigation` | `TabDefinition`, `ShellTabs`, `ScreenIds`, `ScreenRequest`, `BackResult`, `ScreenRouter` |
| Core `BetterLife.Core.ViewModels` | `MoneyFormatter`, `TopBarState`, `TopBarViewModel` |
| Unity `BetterLife.Presentation` | `UiAssets`, `UiRoot`, `UiText`, `SafeArea`, `IScreenPresenter`, `ScreenRegistry`, `PlaceholderScreenPresenter`, `TopBarPresenter`, `ShellPresenter` |
| Unity Resources `BetterLife/UI` | `Shell.uxml`, `TopBar.uxml`, `Placeholder.uxml`, `Uss/Theme.uss`, `Uss/Shell.uss`, `BetterLifeTheme.tss` |

## 5. Save
ไม่มีข้อมูลใหม่ในเซฟ (แท็บที่เปิดค้างไม่ต้องจำ)

## 6. Tests
- `ScreenRouterTests` (7): เริ่มที่ Home, stack แยกต่อแท็บ, กดแท็บเดิมกลับหน้าแรก, Modal ปิดก่อน, ลำดับ Back (Modal → ถอย → Home → Exit), Push/SelectTab ปิด Modal, Reset + เหตุการณ์ Changed
- `LocalizationTests`: ภาษาหลัก → ภาษาสำรอง → ตัว key, แทน `{name}`, ตารางผิด (key ไม่ถูกต้อง/ภาษาซ้ำ), `MoneyFormatter.Compact` หลายช่วงตัวเลข + ติดลบ, `Full`
- `TopBarViewModelTests` (2): ชื่อ/อายุ/เดือน/เงินตามยุค/Inbox และปิดปุ่มเมื่อชีวิตจบ
- `StringsContentTests` (content จริง): ทุกภาษามี key ชุดเดียวกัน และทุก key ที่ Shell ใช้มีข้อความ
- โค้ด Unity (`UiRoot`, Presenter, UXML/USS) คอมไพล์ด้วย dotnet ไม่ได้ ต้องตรวจใน Unity Editor

## 7. ส่งต่อ
- ข้อ 16: หน้า Home (Dashboard + การ์ด Core 5 ใบ + Sparkline), Inbox, Event Dialog เป็น Modal
- ข้อ 17: หน้า Money / People / More (ดวง ไทม์ไลน์)
- ข้อ 20: ตารางภาษาเต็ม, ฟอนต์ไทย, สลับภาษา, หน้าตั้งค่า
