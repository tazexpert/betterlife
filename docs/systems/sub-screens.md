# Task 17: หน้าจอย่อย (Finance, Relationships, Astrology, Timeline)

> สถานะ: ✅ **Implemented** — 2026-11-15 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **แท็บ Money (การเงิน):** เงินสด ทรัพย์สินสุทธิ เงินลงทุน หนี้ รายรับ-รายจ่ายเดือนล่าสุด สภาพเศรษฐกิจ ระดับการใช้ชีวิต (เลือกได้เมื่อโต)
  และ **ประวัติถูกมิจฉาชีพหลอก + ปุ่มแจ้งความ** (ส่งต่อจากข้อ 13b)
- **แท็บ People (ความสัมพันธ์):** รายชื่อคนในชีวิตแยกกลุ่ม → แตะเพื่อดูรายละเอียด (ความสนิท ความเข้ากันได้ ความทรงจำ) และทำ Interaction (คุย ให้ของขวัญ ชวนเดท ขอแต่งงาน ...)
- **แท็บ More:** เมนูไปหน้า **ดวงชะตา** (ราศี วันเกิด ปีนักษัตร ธาตุ ดวง 5 ด้านปีนี้พร้อม Breakdown กรรมดี/ชั่ว หนี้กรรม) และ **ไทม์ไลน์ชีวิต**
- **ไทม์ไลน์:** รวมเหตุการณ์สำคัญจากข้อมูลที่เซฟอยู่แล้ว: เกิด เหตุการณ์ที่ตอบไป เรียนจบ เริ่ม/จบงาน แฟน/แต่งงาน/เลิก ลูกเกิด คนใกล้ชิดเสีย ถูกหลอก เสียชีวิต

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ขอบเขต | ข้อนี้ทำ 4 หน้าตาม Roadmap + รายการถูกหลอก/แจ้งความ ส่วน **ซื้อขายสินทรัพย์ / กู้เงิน / ปิดหนี้** และแท็บ **Life (งบเวลา 168 ชม.)** และ **Work (เรียน/งาน)** ยังเป็น Placeholder | หน้าจอ "ทำธุรกรรม" แต่ละแบบต้องมีฟอร์มและการยืนยันของตัวเอง ควรเป็นงานแยก (เสนอเป็นข้อ 17b) |
| เงินที่แสดง | ทุกยอดเป็น **ราคาตามยุค** (`EconomyState.Nominal`) รูปแบบเต็ม `฿#,0` | สอดคล้องกับ Top bar |
| รายงานเดือนล่าสุด | ใช้ `FinanceService.LastReport` จัดกลุ่มตาม `FinanceLineCategory` ถ้ายังไม่มี (หลังโหลดเซฟ/ยังไม่ครบเดือน) แสดงข้อความแทน | Report ไม่อยู่ในเซฟ (สเปกข้อ 14) |
| เลือกระดับการใช้ชีวิต | แสดงทุกระดับ + ค่าใช้จ่าย/เดือน เลือกได้เมื่ออายุ ≥ อายุพึ่งพาตัวเอง (`FinanceService.CanChooseLifestyle`) ผ่าน `DecisionService.ChooseLifestyle` (มี Checkpoint) | เด็กใช้ระดับของครอบครัว |
| แจ้งความ | ปุ่มแสดงเมื่อยังไม่แจ้งและยังอยู่ในกรอบเวลา พร้อมโอกาสโดยประมาณ (`base − decay × สัปดาห์`) ผ่าน `DecisionService.ReportScam` | ยิ่งแจ้งเร็วยิ่งดี ผู้เล่นต้องเห็นตัวเลข |
| กลุ่มคน | ครอบครัว (พ่อแม่ พี่น้อง ลูก) / คนรัก (แฟน คู่สมรส) / เพื่อนและเพื่อนร่วมงาน / อื่นๆ (แฟนเก่า คนรู้จัก) เรียงตามความสนิท คนที่เสียชีวิตแล้วอยู่ท้ายกลุ่ม | |
| Interaction | แสดงเฉพาะที่ใช้กับประเภทความสัมพันธ์นั้นได้ (`InteractionDefinition.AppliesTo`) ที่ล็อกแสดงเหตุผล (loc key ของ `InteractionQuote`) ทำแล้วแสดงผลในหน้าเดียวกัน ผ่าน `DecisionService.Interact` (Checkpoint) | ทุกการตัดสินใจย้อนได้ตามสเปกข้อ 14 |
| ดวง | ดวงปีนี้ 5 ด้าน + Breakdown (พื้นฐาน ราศี วันเกิด ธาตุ รอบดวง ดวงรายปี) กรรม: บุญ บาป แต้มโอกาสจากกรรม หนี้กรรมที่รอ | Pillar "อ่านตัวเลขได้ทุกตัว" |
| ไทม์ไลน์ | สร้างจากข้อมูลที่มีอยู่ (ไม่เพิ่มอะไรในเซฟ) ใหม่สุดก่อน แสดงอายุ ณ ตอนนั้น สูงสุด 200 รายการ | ข่าวใน Inbox (ไม่เซฟ) ไม่อยู่ในไทม์ไลน์ |
| เข้าถึงระบบ | เพิ่ม read-only: `FinanceService.Assets` / `Products` / `CanChooseLifestyle`, `LuckCalculator.Fortune` / `Karma` | หน้าจอต้องอ่านชื่อและสูตร |
| ข้อความ | เพิ่ม en/th: ชื่อสินทรัพย์ หนี้ ระดับชีวิต เศรษฐกิจ Interaction ความทรงจำ หลักสูตร สาขาอาชีพ/ตำแหน่ง ดวง สาเหตุการตาย + UI ทั้งหมด และ Test ว่าครบ | |

## 3. JSON
ไม่มี content ชนิดใหม่ เพิ่ม key ใน `content/strings/en.json`, `th.json`

## 4. C# Domain
| ที่อยู่ | Class |
|---|---|
| Core `BetterLife.Core.ViewModels` | `ValueRow`, `FinanceState`, `HoldingRow`, `DebtRow`, `LifestyleOption`, `ScamIncidentRow`, `FinanceViewModel`, `PersonRow`, `PeopleGroup`, `PeopleState`, `PeopleViewModel`, `InteractionOption`, `PersonState`, `PersonViewModel`, `ActionResultState`, `FortuneRow`, `AstrologyState`, `AstrologyViewModel`, `TimelineEntryKind`, `TimelineEntry`, `TimelineViewModel` |
| Core `BetterLife.Core.Sessions` | `DecisionService.Interact`, `ReportScam`, `ChooseLifestyle` |
| Core `BetterLife.Core.Navigation` | `ScreenIds.Person`, `Astrology`, `Timeline` |
| Core อื่นๆ | `RowTone`, `ValueRow`, `ActionResultState`, `SessionText` (เงินตามยุค อายุ เปอร์เซ็นต์), `DeathService.Causes` |
| Unity `BetterLife.Presentation.Screens` | `ListScreenPresenter` (คลาสแม่: หัวเรื่อง + การ์ดผลลัพธ์ + เนื้อหาที่สร้างใหม่ทุกครั้งที่รีเฟรช), `FinanceScreenPresenter`, `PeopleScreenPresenter`, `PersonScreenPresenter`, `MoreScreenPresenter`, `AstrologyScreenPresenter` (แตะดวงแต่ละด้านเพื่อดูส่วนประกอบ), `TimelineScreenPresenter`, `UiRows` (ตัวช่วยสร้างแถว ปุ่ม การ์ดผลลัพธ์) |
| Unity Resources | `ListScreen.uxml` ใช้ร่วมกันทั้ง 6 หน้า (เนื้อหาเป็นรายการที่สร้างจากโค้ด จึงไม่ต้องมี UXML แยก) + สไตล์ใน `Uss/Screens.uss` |

## 5. Save
ไม่มีข้อมูลใหม่ในเซฟ

## 6. Tests
- `SubScreenTests` (6): การเงิน (ภาพรวม/เงินลงทุน/รายงานเดือน/เปลี่ยนระดับชีวิต + Checkpoint), เด็กเลือกระดับชีวิตไม่ได้ + ยังไม่มีรายงาน, แจ้งความ (ใหม่สุดก่อน, เกินกรอบเวลา, แจ้งได้ครั้งเดียว), คน (จัดกลุ่ม เรียงความสนิท, หน้าคน + Interaction + Checkpoint), ดวง (ดวงกำเนิด 4 แถว, 5 ด้าน × 6 ส่วน, กรรม 4 แถว), ไทม์ไลน์ (ใหม่สุดก่อน, เริ่มที่วันเกิด, นับเรียนจบ/เหตุการณ์ครบ, ถูกหลอก, เสียชีวิต)
- `StringsContentTests` (+2): **ทุกค่าคงที่ที่เป็น key ใน `BetterLife.Core.ViewModels` (ตรวจด้วย reflection)** และทุกชื่อจาก content ที่หน้าจอเหล่านี้ใช้ (สินทรัพย์ หนี้ ระดับชีวิต เศรษฐกิจ Interaction ความทรงจำ หลักสูตร อาชีพ สาเหตุการตาย เมือง ดวงกำเนิดทุกแบบ) มีข้อความทุกภาษา
- โค้ด Unity ผ่าน `BetterLife.Unity.CompileCheck` (คลาสจำลองของ Unity) แต่ยังต้องตรวจใน Unity Editor

## 7. ส่งต่อ
- ข้อ 17b (เสนอ): ซื้อ/ขายสินทรัพย์, กู้/ปิดหนี้, หน้า Life (งบเวลา), หน้า Work (สมัครเรียน/งาน), วิธีเลิกอบายมุข
- ข้อ 18: Life Summary ใช้ `TimelineViewModel` เดียวกัน
- ข้อ 19: ปุ่ม Undo หลังทุก `DecisionService`
