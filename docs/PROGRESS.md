# Progress

## สถานะปัจจุบัน
**00 Repo Setup** ✅ · **01 Foundation** ✅ · **02 Domain Model + Stats** ✅ · **03 Rules Engine** ✅ · **04 Content Pipeline** ✅ · **05 Time System** ✅ · **06 Astrology + Karma + Luck** ✅ · **07 Event System** ✅ · **08 Character Creation** ✅ · **09 Life Stages + Aging + Death** ✅ · **10 Economy + Finance** ✅ · **11 Education + Career** ✅ · **12 NPC + Relationships** ✅ · **13 Vice System** ✅ · **14 Save System** ✅ · **13b Scams** ✅
**ถัดไป:** 15 UI Shell

## กติกาการทำงาน
- ผู้ใช้อนุญาตให้ใช้ตัวเลือกที่แนะนำได้เลยโดยไม่ต้องหยุดรอยืนยัน (2026-11-08) แต่ยังต้องเขียน Data Structure ลงสเปกก่อนเขียน Logic
- ทำทีละข้อ 1 ข้อ = 1 Session ใน Claude Code
- เริ่มข้อถัดไปได้เมื่อ `dotnet test dotnet/BetterLife.sln` ของข้อก่อนผ่านครบแล้วเท่านั้น
- ตอนเริ่ม Session ใหม่ ให้พิมพ์เลขข้อ เช่น "เริ่มข้อ 02"

## Roadmap
| # | Task | ผลลัพธ์ | สถานะ |
|---|---|---|---|
| 00 | Repo Setup | โครงสร้างโฟลเดอร์, asmdef, package.json, csproj, manifest.json, ProjectSetupMenu, GameBootstrap ว่าง | ✅ |
| 01 | GameDate + Pcg32Random + IEventBus | พื้นฐานที่ทุกระบบใช้ (`docs/systems/foundation.md`) | ✅ |
| 02 | Domain Model + Stats | Character, CoreStats, Traits, Skills, FlagSet (`docs/systems/domain-model.md`) | ✅ |
| 03 | Rules Engine | ICondition, IEffect, Registry, JSON converters (`docs/systems/rules-engine.md`) | ✅ |
| 04 | Content Pipeline | IContentRepository, Schema, Validator Test (`docs/systems/content-pipeline.md`) | ✅ |
| 05 | Time System + Time Budget | Tick, Interrupt, การจัดสรร 168 ชั่วโมง (`docs/systems/time-system.md`) | ✅ |
| 06 | Astrology + Karma + LuckCalculator | สูตร FinalChance (`docs/systems/astrology-karma-luck.md`) | ✅ |
| 07 | Event System | Weighted pool, Cooldown, Choices, Delayed consequences, Storylines (`docs/systems/event-system.md`) | ✅ |
| 08 | Character Creation | Birth, Country, Family, Genetics, Seed (`docs/systems/character-creation.md`) | ✅ |
| 09 | Life Stages + Aging + Death | ช่วงวัย 8 ช่วง, วันเกิด, โรค, ความเสี่ยงตาย, สาเหตุการตาย (`docs/systems/life-stages-aging-death.md`) | ✅ |
| 10 | Economy + Finance | Macro, Income/Expense, Assets, Debt, Credit (`docs/systems/economy-finance.md`) | ✅ |
| 11 | Education + Career | การศึกษาไทย, สอบ, ทุน, กยศ., Career ladder, หัวหน้า, เลิกจ้าง, เสี่ยง/ผิดกฎหมาย, เกษียณ (`docs/systems/education-career.md`) | ✅ |
| 12 | NPC + Relationships | NPC + พันธุกรรม, ความสัมพันธ์ 9 ประเภท, การกระทำ 11 แบบ, Compatibility, แต่งงาน/หย่า/ลูก, NPC เสียชีวิต, Network (`docs/systems/relationships.md`) | ✅ |
| 13 | Vice System | อบายมุข 6, Temptation/Resist, Addiction/Tolerance/Withdrawal, พนัน EV ติดลบ, ครอบครัวจับได้, วิธีเลิก 5 แบบ (`docs/systems/vices.md`) | ✅ |
| 13b | Scam System (เพิ่มตามคำขอผู้ใช้) | มิจฉาชีพ 10 แบบ, การเล็งเป้า/จับได้, แจ้งความขอเงินคืน, เหตุการณ์ชวนลงทุน/แฟนออนไลน์ (`docs/systems/scams.md`) | ✅ |
| 14 | Save System | JSON + Migration + Undo snapshot (`docs/systems/save-system.md`) | ✅ |
| 15 | UI Shell | ScreenRouter, Theme USS, Top bar, Bottom nav | ⬜ |
| 16 | Dashboard + Inbox + Event Dialog | | ⬜ |
| 17 | หน้าจอย่อย | Finance, Relationships, Astrology, Timeline | ⬜ |
| 18 | Legacy + Life Summary + Achievements | | ⬜ |
| 19 | Ads + IAP | Fake ก่อน → SDK จริง | ⬜ |
| 20 | Localization TH/EN + Settings | | ⬜ |
| 21 | Balancing Simulator | รัน 10,000 ชีวิต แล้วออกรายงาน | ⬜ |
| 22 | Content Batch | เขียนเหตุการณ์เป็นชุดละ 30–50 อัน | ⬜ |
| 23 | Build Checklist Android/iOS | | ⬜ |

หมายเหตุข้อ 01: คลาสที่ Roadmap เรียกว่า "Pcg32Random" ในโค้ดใช้ชื่อ `Pcg32` (namespace `BetterLife.Core.Randomness`) และมี `RandomRegistry` สำหรับแยกชุดการสุ่มตามระบบ

## Log
- 2026-11-08 — สร้างโครงโปรเจกต์, `dotnet test` ผ่าน (1 test: ArchitectureRulesTests)
- 2026-11-08 — Foundation เสร็จ: Epoch 1926, ปฏิทิน 52 สัปดาห์, PCG32 + stream registry, EventBus แบบคิว, Save DTO; `dotnet test` ผ่าน 86 tests
- 2026-11-08 — 00 Repo Setup: เพิ่ม Unity project (`unity/`), asmdef ของ Bootstrap/Infrastructure/Presentation/EditorTools, ProjectSetupMenu, GameBootstrap, AssemblyDependencyRulesTests, CI; `dotnet test` ผ่าน 90 tests
- 2026-11-08 — รับ Roadmap 00–23 จากผู้ใช้ แทนตาราง Milestone เดิม ข้อ 00 และ 01 เสร็จแล้ว (90 tests ผ่าน)
- 2026-11-08 — 02 Domain Model + Stats เสร็จ: Core คำนวณจากค่าย่อย, GenderProfile (เพศกำเนิด/เพศสภาพ/รูปแบบการพูด), Traits/Skills/Flags, Save DTO, schemas + content เริ่มต้น; `dotnet test` ผ่าน 151 tests
- 2026-11-08 — 03 Rules Engine เสร็จ: Condition 12 / Effect 13 ชนิด, Registry + converters (Newtonsoft.Json), ข้อความ error ชี้ JSON path, rules.schema.json; `dotnet test` ผ่าน 207 tests
- 2026-11-08 — 04 Content Pipeline เสร็จ: manifest, loader ตามชนิดพร้อม dependency, รวบรวม error พร้อมไฟล์และ path, content ใหม่ 2 ชนิด, ฝั่ง Unity (Resources + ตรวจก่อน Build), test ตรวจ content ด้วย JsonSchema.Net; `dotnet test` ผ่าน 226 + 6 tests
- 2026-11-08 — 05 Time System เสร็จ: TimeBudget 168 ชม. + กิจกรรม 8 อย่าง, TimeSystem ที่มี Phase ตามลำดับ GDD + cadence + Interrupt, Natural decay, Save DTO; `dotnet test` ผ่าน 260 + 7 tests
- 2026-11-08 — 06 Astrology + Karma + Luck เสร็จ: ดวง 5 ด้าน (ราศีไทย, วันเกิดรวมพุธกลางคืน, ธาตุปีนักษัตร, รอบ 12 ปี), กรรมดี/ชั่วแยกกัน + หนี้กรรม + ทำบุญ, FinalChance พร้อม Breakdown และเพดาน Item 5%, rule ใหม่ 5 ชนิด; `dotnet test` ผ่าน 305 + 8 tests
- 2026-11-08 — 07 Event System เสร็จ: pool แบบสุ่มตามน้ำหนักพร้อมตัวคูณ, cooldown, จำนวนครั้ง, Inbox + ตอบอัตโนมัติ, ตัวเลือกที่ทอยโอกาสด้วย FinalChance, `event.schedule`, storyline, anti-frustration, เหตุจากหนี้กรรม, Save DTO, เหตุการณ์เริ่มต้น 9 อัน; `dotnet test` ผ่าน 331 + 8 tests
- 2026-11-08 — 08 Character Creation เสร็จ: 3 โหมด (Random/Custom/Challenge) จาก seed เดียว, ประเทศ/เมือง + modifier, ฐานะครอบครัว, บุคลิกพ่อแม่, พันธุกรรม (stat + trait), ชื่อไทย/ญี่ปุ่น, Challenge Code, LifeSession, GameBootstrap สร้างชีวิตใหม่ตอนเปิดเกม; `dotnet test` ผ่าน 371 + 11 tests
- 2026-11-08 — 09 Life Stages + Aging + Death เสร็จ: 8 ช่วงวัยพร้อม budget ตามวัย, ผลวันเกิด + ความเสี่ยงโรค, โรคมีผลรายสัปดาห์/หาย/เรื้อรัง, mortality (Gompertz × สุขภาพ × สาธารณสุข), สาเหตุการตาย 6 แบบ, `character.die`, เวลาหยุดเมื่อตาย; อายุเฉลี่ยตอนตาย 82 จาก 80 ชีวิต; `dotnet test` ผ่าน 391 + 12 tests
- 2026-11-08 — 10 Economy + Finance เสร็จ: วัฏจักรเศรษฐกิจ 4 ช่วง + เงินเฟ้อ + ดอกเบี้ยนโยบาย + ตลาดรายเดือน (เงินทั้งหมดเป็นบาทมูลค่าจริงปี 2026), เงินช่วยจากครอบครัว/เงินตั้งตัวตอนอายุ 18, Lifestyle 5 ระดับ, สินทรัพย์ 11 ตัว 7 ประเภท, สินเชื่อ 6 แบบ, ยอดขาด → บัตร → หนี้นอกระบบ, ผิดนัด → default → ยึดทรัพย์ → ล้มละลาย, Credit score, หนี้ทำให้เครียด, Wealth จากทรัพย์สินสุทธิ, Rule `netWorth`/`totalDebt`, content 5 ชนิด, Save DTO; `dotnet test` ผ่าน 430 + 13 tests (ผู้ใหญ่ยังไม่มีรายได้จนถึงข้อ 11 จึงติดหนี้นอกระบบในการจำลอง)
- 2026-11-08 — 11 Education + Career เสร็จ: หลักสูตร 10 ระดับ (อนุบาล–ป.เอก, ปวช./ปวส., กศน.) พร้อมสถานศึกษา 24 แห่ง, เข้าเรียนอัตโนมัติตามฐานะ, ผลการเรียนจากชั่วโมงเรียนจริง, สอบแข่งขัน/ทุน/กยศ., อาชีพ 11 สาย 7 หมวด (รวมพรีเมียม 1 สาย), Performance/หัวหน้า/ความเครียด, เลื่อนตำแหน่ง, ไล่ออก/เลิกจ้างตามเศรษฐกิจ, ความเสี่ยงจับกุม/บาดเจ็บ, เกษียณ + บำนาญ, Rule ใหม่ 2 condition + 4 effect, content 4 ชนิด, Save DTO, นักศึกษาได้ครอบครัวช่วยถึงอายุ 25; `dotnet test` ผ่าน 485 + 15 tests (ทรัพย์สินสุทธิมัธยฐานตอนอายุ 30 ≈ +3.2 ล้าน จากเดิม −1,000 ล้าน)
- 2026-11-09 — 12 NPC + Relationships เสร็จ: พ่อแม่จากการสร้างตัวละครเป็น NPC + พี่น้องสุ่ม, ความสัมพันธ์ −100..100 ประเภท 9 แบบพร้อมความทรงจำ, ดูแลด้วยชั่วโมง socialize (ผลตอบแทนลดลงตามความสนิท) และจางหายเอง, พบเพื่อน/เพื่อนร่วมชั้น/เพื่อนร่วมงาน, Compatibility พร้อม Breakdown, การกระทำ 11 แบบจาก content (เดท ขอแต่งงาน เลิก หย่า มีลูก รับบุตรบุญธรรม ...), ลูกสืบพันธุกรรม + ค่าเลี้ยงดู, แฟน/คู่สมรสจากไปเอง, NPC เสียชีวิต + ความเศร้า, Relationships/Loneliness อัตโนมัติ, Network ช่วยสมัครงาน, Rule ใหม่ `relationship` (condition + effect), content 2 ชนิด, Save DTO; `dotnet test` ผ่าน 521 + 16 tests
- 2026-11-09 — 13 Vice System เสร็จ: อบายมุข 6 (สาร 3 หมวดสมมติ, เที่ยวกลางคืน, บันเทิงเกินขนาด, พนัน + ตัวคูณคบคนชั่ว/เกียจคร้าน), TemptationWeight และ ResistChance ตาม GDD พร้อม Breakdown, Addiction/Tolerance/Withdrawal/Clean streak, พนันมี Beginner's luck แต่ EV −10% และเล่นต่อจนเป็นหนี้เมื่อติด, ครอบครัวจับได้, วิธีเลิก 5 แบบ (รวมบวช/ปฏิบัติธรรม) + Relapse, ปิดเนื้อหาได้, ห้าม Undo การพนัน (`LastIrreversibleWeek`), Trait ใหม่ reckless/disciplined, กิจกรรม nightlife, Rule ใหม่ `vice` + `vice.addiction`, content 3 ชนิด + เหตุการณ์ 1, Save DTO; `dotnet test` ผ่าน 551 + 17 tests
- 2026-11-09 — 14 Save System เสร็จ: เซฟทั้งชีวิตเป็น JSON (Envelope + saveVersion + checksum), Migration ทีละขั้นผ่าน Registry, ปฏิเสธเซฟเสีย/ใหม่กว่า, ล้าง content ที่ถูกลบพร้อมคำเตือน, `ISaveStorage` + ไฟล์ (atomic) + หน่วยความจำ, Slot + autosave, `LifeSessionFactory.Load` ใช้ `Compose` เดียวกับชีวิตใหม่, Undo 1 ขั้น วันละ 3 ครั้ง ปิดใน Ironman และห้ามย้อนการพนัน, GameBootstrap โหลด/บันทึก autosave + link.xml; เซฟแล้วเล่นต่อ 10 ปีเหมือนต้นฉบับทุกไบต์; `dotnet test` ผ่าน 573 + 18 tests
- 2026-11-09 — 13b Scam System (ผู้ใช้ขอเพิ่ม) เสร็จ: มิจฉาชีพ 10 แบบ (คอลเซ็นเตอร์, SMS, ร้านปลอม, หลอกรัก, หลอกลงทุน, หลอกงาน, หลอกกู้, หลอกรางวัล, หลอกในเกม, แชร์ลูกโซ่), เล็งเป้าตามอายุ/เหงา/ออนไลน์/เงิน/สถานการณ์ พร้อม Breakdown, จับได้ด้วย Hidden stat ใหม่ street smarts + Knowledge + คนเตือน − ความเครียด, เสียเงินสดและเงินฝาก, บทเรียน, แจ้งความขอเงินคืน, เสียมากหยุดเวลา, Rule ใหม่ `scam` + `scam.loss`, content 2 ชนิด + เหตุการณ์ 2, Save DTO; `dotnet test` ผ่าน 592 + 19 tests
