# BetterLife — Game Design Document

> สถานะ: v0.1 (สร้างจากสรุปใน Master Instructions) — แต่ละหัวข้อจะขยายเป็นสเปกเต็มใน `docs/systems/<system>.md` เมื่อเริ่มพัฒนาระบบนั้น

## 1. Design Pillars
1. **อ่านตัวเลขได้ทุกตัว** — ทุกค่ามี Breakdown (กดค้างเพื่อดูว่ามาจากอะไร)
2. **ทุกทางเลือกมีราคา** — ไม่มีตัวเลือกที่ดีกว่าทุกด้าน
3. **ดวงไม่ได้ตัดสินทุกอย่าง** — โอกาสสำเร็จถูก Clamp 5–95% เสมอ
4. **Butterfly Effect** — การตัดสินใจเล็กๆ ส่งผลในอนาคตผ่าน Story Flags และ Delayed Consequences
5. **Monetization ไม่ Pay-to-Win** — ไม่มี Loot box / Gacha, ไอเท็ม IAP มีเพดาน

## 2. Core Loop และเวลา
Dashboard → จัดสรรงบเวลา 168 ชม./สัปดาห์ → ข้ามเวลา → เหตุการณ์/การตัดสินใจ → ผลลัพธ์

| ระดับ | หน่วย | ระบบที่ทำงาน |
|---|---|---|
| Micro | สัปดาห์ | Time allocation, Stats, Skills, Vice |
| การเงิน | เดือน | รายได้, ค่าใช้จ่าย, หนี้, ดอกเบี้ย |
| Macro | ปี | อายุ, Life stage, เศรษฐกิจ, ดวงรายปี |

- **Interrupt:** หยุดการข้ามเวลาเมื่อเจอเหตุการณ์ระดับ Major/Critical
- **ลำดับ Tick:** Time allocation → Natural decay → Delayed consequences → Event roll → NPC update → Death check

## 3. Character Creation
วันเกิด, ยุคที่เกิด, ประเทศ/เมือง (Modifier: ค่าครองชีพ การศึกษา สาธารณสุข ภาษี),
ฐานะครอบครัว, บุคลิกพ่อแม่, พันธุกรรม, Seed ต่อชีวิต
โหมด: **Random / Custom / Challenge Seed**

## 4. Astrology & Karma
- ดวง 5 ด้าน (การเงิน ความรัก สุขภาพ การงาน การเรียน) ค่า 0–100
  คำนวณจาก ราศี + วันเกิด (โหราไทย) + ธาตุปีเกิด และมีรอบดวงขึ้น/ตกรายปี
- กรรมดี/กรรมชั่วเก็บแยกกัน ไม่หักลบ + หนี้กรรม (ส่งผลหน่วงเวลา) ทำบุญชดเชยได้บางส่วน
- `FinalChance = Base + Stat + Luck + Karma + Item` → Clamp 5%–95%, ไอเท็ม IAP เพดาน +5%

## 5. Stats (3 ชั้น)
| ชั้น | ค่า |
|---|---|
| Core | Health, Mental, Wealth, Intelligence, Relationships |
| ค่าย่อย | Fitness, Nutrition, Fatigue, Diseases, Happiness, Stress, Confidence, Loneliness, Cash, Assets, Debts, NetWorth, CreditScore, IQ, Knowledge |
| Hidden | Traits, Charisma, Looks, Reputation (แยกวงการ), Willpower |

- Skills แยกออกมา และเสื่อมลงเมื่อไม่ได้ใช้
- Stats ส่งผลถึงกัน (Mental ต่ำ → เรียนรู้ช้า, หนี้สูง → Stress สูง)

## 6. Life Stages
| Stage | อายุ |
|---|---|
| ทารก | 0–2 |
| เด็กเล็ก | 3–6 |
| วัยเรียน | 7–12 |
| วัยรุ่น | 13–17 |
| ผู้ใหญ่ตอนต้น | 18–29 |
| วัยสร้างตัว | 30–44 |
| วัยกลางคน | 45–59 |
| วัยเกษียณ | 60+ |

## 7. Event Engine
- Conditions, Base weight + Weight modifiers, Cooldown, Severity, Choices
  (เงื่อนไข / โอกาสสำเร็จ / ผลเมื่อสำเร็จและล้มเหลว)
- Butterfly Effect = Story Flags + Delayed Consequences (`scheduleEvent`) + Event Chains/Storylines
- Anti-frustration: ลดน้ำหนักเหตุการณ์ร้ายชั่วคราวเมื่อเจอติดกัน

## 8. Economy
- เงินเฟ้อ, วัฏจักรเศรษฐกิจ 4 ช่วง, ดอกเบี้ยนโยบาย, Lifestyle Tier
- สินทรัพย์: เงินฝาก, พันธบัตร, กองทุน, หุ้น (บริษัทสมมติ), อสังหาฯ, ธุรกิจ, คริปโต
- หนี้: บัตรเครดิต, ส่วนบุคคล, บ้าน, รถ, กยศ., นอกระบบ / Credit score / ยึดทรัพย์ / ล้มละลาย

## 9. Education & Career
- ระบบการศึกษาไทย + การสอบแข่งขัน + ทุน + กยศ. + ทางเลือกนอกระบบ
- Career Ladder, Performance, Boss satisfaction, Job stress, ฟรีแลนซ์, ธุรกิจ, สายเสี่ยงสูง, สายผิดกฎหมาย
- อาชีพพรีเมียม = ระบบย่อยที่ลึกกว่า แต่ **ไม่ได้เปรียบเรื่องรายได้**

## 10. Relationships & NPC
- ค่าความสัมพันธ์ −100..100 + ประเภท, Traits, Hidden Potential, ความทรงจำ (Flags)
- NPC เติบโตเองแบบ Lightweight Simulation, ความสัมพันธ์ลดลงเองถ้าไม่ดูแล
- Compatibility, แต่งงาน/หย่า/ลูก (พันธุกรรม), Network ทางธุรกิจ

## 11. Vice System (อบายมุข 6)
1. น้ำเมา/บุหรี่/สารเสพติด (ใช้หมวดสมมติ)
2. เที่ยวกลางคืน
3. หมกมุ่นความบันเทิง
4. การพนัน
5. คบคนชั่ว (ตัวคูณ)
6. เกียจคร้าน (ตัวคูณ)

- ผลดีระยะสั้น + ต้นทุนระยะยาว / Addiction, Tolerance, Withdrawal, Relapse, Clean streak
- `TemptationWeight = Base × Stress × Addiction × BadCompany × Laziness × LifeStage`
- `ResistChance = 50 + (Willpower−50)×0.5 − Addiction×0.4 + Support + Karma` → Clamp 5–95%
- การพนันมี Beginner's luck แต่ EV ติดลบ, Debt spiral → หนี้นอกระบบ
- Discovery โดยครอบครัว, วิธีเลิก 5 แบบ (รวมบวช/ปฏิบัติธรรม)
- เชื่อมกับ Karma, Health, Career, กฎหมาย, Legacy
- **ข้อห้าม:** ห้ามมี IAP หรือ Ads ใดๆ ผูกกับการพนัน, Undo ใช้ยกเลิกผลพนันไม่ได้
- มีตัวเลือกปิดเนื้อหาในหน้าตั้งค่า + หน้าข้อมูลสายด่วนช่วยเหลือจริง

## 12. Death & Legacy
สาเหตุการตาย, Life Summary + คำไว้อาลัยที่สร้างจาก Flags (แชร์ได้),
Legacy Mode เล่นต่อเป็นลูกหลาน (มรดก ภาษี ชื่อเสียง หนี้ กรรมตกทอด), Family Tree, Achievements

## 13. Monetization
- Rewarded Ads: คำใบ้โชคชะตา, Undo (วันละ 3 ครั้ง ย้อนได้แค่การตัดสินใจล่าสุด)
- IAP: Starter Packs, เครื่องราง (มีเพดาน), อาชีพพรีเมียม/Expansion, Remove Ads, Cosmetics
- ห้าม Loot box / Gacha, มีโหมด Ironman (ปิด Undo)

## 14. UI
Portrait เล่นมือเดียวได้, Top bar + การ์ด Core 5 ใบ (ค่า + แนวโน้ม + Sparkline),
Inbox แบบ FM, Bottom nav 6 แท็บ, กดค้างเพื่อดู Breakdown,
การ์ด Habits จะโผล่เมื่อเริ่มติดเท่านั้น
