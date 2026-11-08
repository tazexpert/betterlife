# Progress

## สถานะปัจจุบัน
**00 Repo Setup** ✅ · **01 Foundation** ✅ · **02 Domain Model + Stats** ✅ · **03 Rules Engine** ✅ · **04 Content Pipeline** ✅ · **05 Time System** ✅
**ถัดไป:** 06 Astrology + Karma + LuckCalculator

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
| 06 | Astrology + Karma + LuckCalculator | สูตร FinalChance | ⬜ |
| 07 | Event System | Weighted pool, Cooldown, Choices, Delayed consequences, Storylines | ⬜ |
| 08 | Character Creation | Birth, Country, Family, Genetics, Seed (รวม BirthHour) | ⬜ |
| 09 | Life Stages + Aging + Death | | ⬜ |
| 10 | Economy + Finance | Macro, Income/Expense, Assets, Debt, Credit | ⬜ |
| 11 | Education + Career | | ⬜ |
| 12 | NPC + Relationships | | ⬜ |
| 13 | Vice System | | ⬜ |
| 14 | Save System | JSON + Migration + Undo snapshot | ⬜ |
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
