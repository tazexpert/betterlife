# Task 16: Dashboard + Inbox + Event Dialog

> สถานะ: ✅ **Implemented** — 2026-11-15 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **Dashboard (แท็บ Home):** การ์ด Core 5 ใบ (Health, Mental, Intelligence, Wealth, Relationships) แสดงค่า + แนวโน้ม 4 สัปดาห์ + Sparkline 52 สัปดาห์
  การ์ด **Habits** โผล่เฉพาะอบายมุขที่เริ่มติด (ตาม `ViceService.VisibleHabits`) และทางลัดไป Inbox เมื่อมีเรื่องรอตัดสินใจ
- **Breakdown:** แตะ/กดค้างที่การ์ด → Modal บอกว่าค่ามาจากอะไร (สูตรของค่า Core หรือการเปลี่ยนแปลงล่าสุดตามแหล่งที่มา)
- **Inbox แบบ FM:** "รอตัดสินใจ" (เหตุการณ์ที่มีตัวเลือก + กำหนดเวลา) และ "ข่าว" (ทุกเรื่องที่เกิดระหว่างข้ามเวลา: ได้เลื่อนตำแหน่ง ถูกหลอก เรียนจบ ฯลฯ)
- **Event Dialog:** Modal แสดงหัวเรื่อง เนื้อเรื่อง ตัวเลือก (โอกาสสำเร็จ %, ผลที่คาดได้, ล็อกหรือไม่) → เลือกแล้วแสดงผลลัพธ์ในหน้าเดียวกัน

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| แยกชั้น | ทุกข้อความ/ตัวเลขที่แสดงสร้างใน Core `BetterLife.Core.ViewModels` (ทดสอบได้) Presenter ใน Unity แค่วางลง UI | เหมือนข้อ 15 |
| แนวโน้ม | ค่าล่าสุด − ค่าเมื่อ 4 สัปดาห์ก่อน (จาก `StatHistory`) ต่างไม่ถึง 0.5 = คงที่ สีเขียว/แดงตาม `StatPolarity` | ตรงกับ ChangeLog ที่เก็บ 4 สัปดาห์ |
| Sparkline | จุด 0..1 จาก `StatHistory` (สูงสุด 52 สัปดาห์) ช่วงแกน Y ปรับตามข้อมูลแต่กว้างอย่างน้อย 10 แต้ม วาดด้วย `Painter2D` | ค่าเปลี่ยนช้า ถ้าใช้แกน 0–100 จะเห็นเป็นเส้นตรง |
| Breakdown | ใช้ `Character.Explain`: ค่า Core = สูตร (ค่าย่อย × น้ำหนัก) ค่าย่อย = การเปลี่ยนแปลง 4 สัปดาห์ตามแหล่งที่มา | ระบบมีอยู่แล้ว |
| แตะ vs กดค้าง | ใช้ `Clickable` (ทำงานตอนปล่อยนิ้ว) จึงได้ทั้งแตะและกดค้าง | ค้นพบง่าย และตรง GDD |
| ข่าวใน Inbox | `NoticeFeed` เก็บ Interrupt ทุกตัวจาก `AdvanceResult` (ใหม่สุดก่อน สูงสุด 30) **ไม่บันทึกลงเซฟ** ข้ามเรื่องที่เป็นเหตุการณ์รอตัดสินใจ (ซ้ำกับรายการรอตัดสินใจ) | ประวัติถาวรอยู่ใน `EventState.History` และไทม์ไลน์ข้อ 17 |
| เปิด Dialog อัตโนมัติ | ถ้าการข้ามเวลาถูกหยุดโดยเหตุการณ์ที่รอตัดสินใจ → เปิด Event Dialog ทันที | แบบ FM |
| ตัดสินใจ | `DecisionService.Resolve` เรียก `UndoService.Checkpoint` ก่อน `EventSystem.Resolve` เสมอ | สเปกข้อ 14 (ปุ่ม Undo + โฆษณาอยู่ข้อ 19) |
| ข้อความ Rule | `RuleTextFormatter`: arg ที่ชื่อลงท้าย `Key` แปลเป็นข้อความก่อน, ช่วง `min`/`max` รวมเป็น `{value}` = "a–b", ค่าสถิติ/บุญมีเครื่องหมาย +/−, เงินแปลงเป็นราคาตามยุคและรูปแบบ `฿#,0` | เทมเพลตเขียนแบบเดียว ใช้ได้ทั้ง Preview และผลจริง |
| ตัวเลือกที่ล็อก | แสดง "ยังไม่ผ่านเงื่อนไข" แบบรวม (รายละเอียดเงื่อนไข `rule.cond.*` แปลในข้อ 20) | ข้อความเงื่อนไขมี ~35 แบบ |
| ข้อความ | เพิ่ม `strings` ทั้ง en/th: UI ของข้อนี้, ชื่อ stat/trait/skill/vice, interrupt, เหตุการณ์ทั้งหมด, effect, แหล่งที่มาของ Breakdown และมี Test ว่าทุก key ที่ content/โค้ดอ้างถึงมีข้อความ | เล่นทดสอบได้จริง |
| หน้าจอ | `inbox` (Push ในแท็บ Home), `event` (Modal, argument = instanceId), `breakdown` (Modal, argument = stat id) | |

## 3. JSON
ไม่มีชนิด content ใหม่ เพิ่มแค่ key ใน `content/strings/en.json` และ `th.json`

## 4. C# Domain
| ที่อยู่ | Class |
|---|---|
| Core `BetterLife.Core.ViewModels` | `RuleTextFormatter`, `GameDateText`, `SparkPoint`, `Sparkline`, `TrendDirection`, `StatCardState`, `HabitCardState`, `DashboardState`, `DashboardViewModel`, `BreakdownLineState`, `BreakdownState`, `BreakdownViewModel`, `InboxItemKind`, `InboxItemState`, `InboxState`, `InboxViewModel`, `ChoiceState`, `EventDialogState`, `EventResultState`, `EventDialogViewModel` |
| Core `BetterLife.Core.Notices` | `Notice`, `NoticeFeed` |
| Core `BetterLife.Core.Sessions` | `DecisionService` |
| Core `BetterLife.Core.Navigation` | `ScreenIds.Inbox`, `ScreenIds.EventDialog`, `ScreenIds.Breakdown` |
| Unity `BetterLife.Presentation.Screens` | `ScreenContext`, `DashboardScreenPresenter`, `InboxScreenPresenter`, `EventDialogPresenter`, `BreakdownPresenter`, `SparklineElement` |
| Unity Resources `BetterLife/UI` | `Dashboard.uxml`, `Inbox.uxml`, `EventDialog.uxml`, `Breakdown.uxml`, `Uss/Screens.uss` |

## 5. Save
ไม่มีข้อมูลใหม่ในเซฟ (`NoticeFeed` อยู่ในหน่วยความจำ)

## 6. Tests
- `RuleTextFormatterTests`: arg `*Key`, ช่วง min–max, เครื่องหมาย +/−, เงินตามยุค
- `SparklineAndTrendTests`: จุด 0..1, ช่วงอย่างน้อย 10, ข้อมูลน้อย, ทิศทางแนวโน้ม + polarity
- `NoticeFeedTests`: ใหม่สุดก่อน, เพดาน, ข้ามเหตุการณ์ที่รอตัดสินใจ, หา Dialog ที่ต้องเปิด
- `DashboardAndInboxViewModelTests`: การ์ด 5 ใบตามลำดับ, Breakdown, Inbox เรียงตามกำหนดเวลา
- `EventDialogViewModelTests`: ตัวเลือก (โอกาส %, ล็อก), ตัดสินใจผ่าน `DecisionService` → Checkpoint + ผลลัพธ์
- `StringsContentTests` (content จริง): ทุก key ของเหตุการณ์/stat/trait/skill/vice/interrupt/effect มีข้อความทุกภาษา

## 7. ส่งต่อ
- ข้อ 17: หน้า Money / People / More, ไทม์ไลน์จาก `EventState.History`, หน้าข้อมูลมิจฉาชีพ + ปุ่มแจ้งความจากข่าว "ถูกหลอก"
- ข้อ 19: ปุ่ม Undo ใน Event Dialog (ดูโฆษณาก่อน)
- ข้อ 20: แปลเงื่อนไข `rule.cond.*`, ฟอนต์ไทย, สลับภาษา
