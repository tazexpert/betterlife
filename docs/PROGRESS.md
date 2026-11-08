# Progress

## สถานะปัจจุบัน
**00 — Repo Setup** ✅
**Milestone 1 — Foundation** ✅
**ถัดไป:** Milestone 2 — Tick Pipeline + Interrupt

## Milestones
| # | งาน | สถานะ |
|---|---|---|
| 00 | Repo Setup: โครงสร้างโฟลเดอร์, asmdef 5 ตัว, package.json, csproj, manifest.json, ProjectSetupMenu, GameBootstrap ว่าง, CI | ✅ Done |
| 1 | Foundation: GameDate, CalendarDate, ThaiDayOfWeekResolver, IRandomProvider/Pcg32/RandomRegistry, IEventBus, Save DTO | ✅ Done |
| 2 | Tick Pipeline + Interrupt | ⬜ |
| 3 | Stats 3 ชั้น + Breakdown | ⬜ |
| 4 | Content pipeline: JSON Schema + validation test + Localization keys | ⬜ |
| 5 | Event Engine (Condition/Effect Registry, Choices, Delayed consequences) | ⬜ |
| 6 | Character Creation + Astrology & Karma | ⬜ |
| 7 | Economy | ⬜ |
| 8 | Education & Career | ⬜ |
| 9 | Relationships & NPC | ⬜ |
| 10 | Vice System | ⬜ |
| 11 | Death & Legacy | ⬜ |
| 12 | Service interfaces + Fakes (Ads, IAP, Analytics, Cloud Save) | ⬜ |
| 13 | Unity: GameBootstrap, Save, UI Toolkit Dashboard | ⬜ |

## Log
- 2026-11-08 — สร้างโครงโปรเจกต์, `dotnet test` ผ่าน (1 test: ArchitectureRulesTests)
- 2026-11-08 — Foundation เสร็จ: Epoch 1926, ปฏิทิน 52 สัปดาห์, PCG32 + stream registry, EventBus แบบคิว, Save DTO; `dotnet test` ผ่าน 86 tests
- 2026-11-08 — 00 Repo Setup: เพิ่ม Unity project (`unity/`), asmdef ของ Bootstrap/Infrastructure/Presentation/EditorTools, ProjectSetupMenu, GameBootstrap, AssemblyDependencyRulesTests, CI; `dotnet test` ผ่าน 90 tests
