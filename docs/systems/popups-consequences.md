# Task 16b: Popup ทันที + ผลตามมาของทุกเหตุการณ์ + สถานะครอบครัวบนหน้าแรก

> สถานะ: ✅ **Implemented** — 2026-11-15 (ผู้ใช้ขอเพิ่มก่อนข้อ 21; ข้อ 28 คงแนว Dashboard แต่เพิ่ม Animation ที่การแจ้งเตือน)

## 1. สรุปความเข้าใจ
- **เหตุการณ์ที่ต้องตัดสินใจหยุดเวลาเสมอ:** แม้กดข้าม 1 ปี ถ้าเหตุการณ์เกิดในสัปดาห์แรก เวลาจะหยุดที่สัปดาห์นั้นและ Popup เปิดทันทีให้ตอบก่อน (Popup แสดงผลที่คาดได้ของแต่ละตัวเลือกอยู่แล้ว)
- **การแจ้งเตือนเป็น Popup พร้อม Animation:** Event Dialog / Breakdown เลื่อนขึ้นและค่อยๆ ชัด ข่าวที่ไม่ต้องตอบ (ได้เลื่อนตำแหน่ง ถูกหลอก Achievement...) แสดงเป็น Toast ด้านบนแล้วหายไปเอง แตะเพื่อเปิด Inbox
- **ทุกตัวเลือกของทุกเหตุการณ์ต้องมีผล:** เพิ่ม Effect `meet` (มีโอกาสเจอคนใหม่) เช่น ไปปาร์ตี้/งานวัด อาจได้เพื่อนใหม่ (โอกาสต่ำ) และเติมผลให้ตัวเลือกที่ยังว่าง พร้อม Test บังคับกับ content ทุกชุดต่อจากนี้
- **หน้าแรกแสดงสถานะครอบครัว:** โสด / มีแฟน / แต่งงาน / หย่า / เป็นหม้าย และจำนวนลูก (แตะไปแท็บ People)

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| หยุดเวลาเมื่อมีเรื่องต้องตอบ | `InterruptRequest.RequiresAnswer` + `InterruptPolicy.StopForDecisions` (ค่าเริ่มต้น **true**) หยุดเมื่อมีคำขอที่ต้องตอบ ไม่ว่าระดับความรุนแรงเท่าไร | ไม่เปลี่ยนความหมายของ Severity เดิม การจำลองหลายพันชีวิตปิดได้ (`StopForDecisions = false`) |
| เหตุการณ์ไม่มีตัวเลือก | ไม่หยุดเวลา (ยกเว้น Major ขึ้นไปเหมือนเดิม) แสดงเป็น Toast | ไม่ขัดจังหวะเกินจำเป็น |
| Animation | USS Transition: `opacity` + `translate` + `scale` 0.25 วินาที (Popup), Toast เลื่อนลงจากบน แสดง 3.5 วินาที สูงสุด 3 อัน | UI Toolkit รองรับ Transition ตั้งแต่ 2021.2 ไม่ต้องใช้ Asset |
| Effect `meet` | `{ "type": "meet", "relation": "friend", "chance": 0.15 }` relation = `friend`/`colleague`/`contact` ทอยด้วย Random ของการตัดสินใจ สำเร็จ → ส่ง `MeetRequestedEvent` → `RelationshipService` สร้างคนใหม่ (อายุใกล้เคียง) | ใช้ระบบพบเจอเดิม Strategy + Registry |
| ผลของตัวเลือกที่ว่าง | ปฏิเสธโอที: เครียดน้อยลง + หัวหน้าพอใจลดลงนิดหน่อย; เดินผ่านงานวัด: พักผ่อน (เหนื่อยลด) | ทุกทางเลือกมีราคา (Pillar 2) |
| Test content | ทุก outcome ของทุกตัวเลือก ต้องมี effect ≥ 1 และเหตุการณ์ที่ไม่มีตัวเลือกต้องมี `onFire` ≥ 1 | กันข้อ 22 หลุด |
| สถานะครอบครัว | คู่สมรสที่มีชีวิต → "แต่งงานกับ X", แฟน → "คบกับ X", เคยมีคู่สมรสที่เสียชีวิต → "เป็นหม้าย", มีแฟนเก่า/คู่หย่า → "โสด (เคยมีคู่)", นอกนั้น "โสด"; ลูก = ลูกที่ยังมีชีวิต (+ จำนวนที่เสียชีวิตถ้ามี) | |

## 3. JSON
```jsonc
// effect ใหม่ (rules.schema.json)
{ "type": "meet", "relation": "friend", "chance": 0.15 }
// event_settings.json ไม่เปลี่ยน
```

## 4. C# Domain
| ที่อยู่ | Class |
|---|---|
| Core `Time.Ticking` | `InterruptRequest.RequiresAnswer`, `TickContext.RequestDecision`, `InterruptPolicy.StopForDecisions` |
| Core `Relationships` | `MeetEffect`, `MeetEffectConverter`, `MeetRequestedEvent`, `RelationshipService` (รับคำขอพบคนใหม่) |
| Core `ViewModels` | `DashboardState.Family` (สถานะ + ลูก) |
| Core `Notices` | `NoticeFeed.Add` คืนข่าวที่เพิ่มใหม่ (สำหรับ Toast) |
| Unity | `ShellPresenter`: Animation Popup + `ShowToasts`, `Shell.uxml` (toast-layer), `Shell.uss` transitions, `DashboardScreenPresenter` การ์ดครอบครัว (แตะไปแท็บ People), `GameBootstrap` แสดง Toast ของข่าวและ Achievement หลังข้ามเวลา |
| Core อื่นๆ | `LegacyService.ContinueAs(..., policy)` |

## 5. Save
ไม่มีข้อมูลใหม่ในเซฟ

## 6. Tests
- `PopupAndConsequenceTests` (4): เหตุการณ์ระดับ minor ที่มีตัวเลือกหยุดการข้าม 1 ปีตั้งแต่สัปดาห์แรก + เปิด Dialog ได้, การจำลอง (`StopForDecisions = false`) ปล่อยให้หมดเวลาเอง, ไปปาร์ตี้แล้วได้เพื่อนใหม่ (Preview โอกาส + ผลจริง), หน้าแรก: โสด → คบ → แต่งงาน + ลูก → เป็นหม้าย
- `ContentValidationTests.EveryEventChoice_HasAConsequence` (content จริง): ทุก outcome มี effect และเหตุการณ์ไม่มีตัวเลือกมี `onFire`
- `RuleRegistryTests`: ตัวอย่าง effect `meet`
- Content simulation ทั้งหมดใช้ `RepoContent.Simulation` (ไม่หยุดเพื่อรอคำตอบ)

## 7. ส่งต่อ
- ข้อ 22: เหตุการณ์ใหม่ทุกอันต้องมีผล (Test บังคับ) และใช้ `meet` ได้
- ข้อ 26: เพศตรงข้ามเข้ามาทำความรู้จัก (relation `partner_candidate`) ต่อยอดจาก `meet`
