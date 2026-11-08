# Architecture

## เลเยอร์และทิศทางการอ้างอิง
```
Presentation (Unity: UI Toolkit UXML/USS + Presenter)
        │
Infrastructure (Unity: Save, Ads, IAP, Analytics, Content loader)
        │            ▲ implements interfaces declared in Core
        ▼            │
Core (packages/com.betterlife.core/Runtime — Pure C#, netstandard2.1, C# 9)
```
- Core ไม่รู้จัก Unity เลย (`noEngineReferences: true`) และคอมไพล์ด้วย `dotnet/BetterLife.Core.csproj`
- Dependency ภายนอกตัวเดียวของ Core: **Newtonsoft.Json** (Unity: `com.unity.nuget.newtonsoft-json`, dotnet: NuGet)
- **GameBootstrap** (ใน Unity) เป็น Composition Root ที่เดียว สร้างทุก Object ด้วยโค้ด ไม่ใช้ Inspector

## โครงสร้างโฟลเดอร์
```
CLAUDE.md                         กฎหลักของโปรเจกต์
.github/workflows/core-tests.yml  CI: dotnet test ทุก push / PR
content/                          เนื้อหาเกม *.json
docs/
  GDD.md  ARCHITECTURE.md  PROGRESS.md
  schemas/                        JSON Schema ของเนื้อหาแต่ละชนิด
  systems/                        สเปกรายระบบ
dotnet/
  BetterLife.sln
  BetterLife.Core.csproj          คอมไพล์ Runtime ของ Core
  BetterLife.Core.Tests.csproj    รันเทสต์ชุดเดียวกับ Unity Test Runner
  BetterLife.Content.Tests/       ตรวจ content จริงด้วย JSON Schema (dotnet อย่างเดียว)
  Directory.Build.props           แยก obj/bin ต่อโปรเจกต์
packages/com.betterlife.core/     UPM package (Pure C#)
  package.json
  Runtime/                        BetterLife.Core.asmdef (noEngineReferences) + โค้ด Core
  Tests/Runtime/                  BetterLife.Core.Tests.asmdef + NUnit tests
unity/                            Unity project
  Packages/manifest.json          อ้าง Core ผ่าน file:../../packages/com.betterlife.core
  ProjectSettings/ProjectVersion.txt
  Assets/BetterLife/
    Bootstrap/                    BetterLife.Bootstrap.asmdef + GameBootstrap (Composition Root)
    Infrastructure/               BetterLife.Infrastructure.asmdef (→ Core) + Content/ (ResourcesContentSource)
    Presentation/                 BetterLife.Presentation.asmdef (→ Core) + UI/Uxml, UI/Uss
    EditorTools/                  BetterLife.EditorTools.asmdef (Editor only, → Core, Infrastructure) + ProjectSetupMenu, ContentSync, ContentBuildStep
    Scenes/Main.unity             สร้างโดย ProjectSetupMenu (ฉากว่าง)
```

## กราฟการอ้างอิง Assembly
```
BetterLife.Bootstrap ──► BetterLife.Infrastructure ──► BetterLife.Core
        │                                                 ▲
        └──────────────► BetterLife.Presentation ─────────┘
```
- ไม่มีใครอ้างถึง Bootstrap ได้ และ Presentation กับ Infrastructure ห้ามอ้างถึงกัน
- **GameBootstrap** โหลด content → สร้าง `LifeSession` ผ่าน `CharacterCreator` + `LifeSessionFactory`
- **GameBootstrap** สร้างตัวเองด้วย `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` และใช้ `DontDestroyOnLoad`
  จึงไม่ต้องวางอะไรในฉากและไม่ต้องผูกค่าใน Inspector

## Namespace ที่วางแผนไว้ (Core)
| Namespace | หน้าที่ |
|---|---|
| `BetterLife.Core.Time` | GameDate, CalendarDate, ThaiDayOfWeekResolver |
| `BetterLife.Core.Time.Budget` | กิจกรรม, TimeBudget 168 ชม., TimeSettings, TimeState |
| `BetterLife.Core.Time.Ticking` (+ `.Phases`) | TimeSystem, ITickPhase, Interrupt, GameClock |
| `BetterLife.Core.Randomness` | IRandomProvider, Pcg32, RandomRegistry (ไม่ใช้ชื่อ `Random` เพื่อไม่ชนกับ System.Random) |
| `BetterLife.Core.Events` | IEventBus, IGameEvent (ข้อความระหว่างระบบ ส่วนเหตุการณ์ชีวิตอยู่ที่ `LifeEvents`) |
| `BetterLife.Core.Domain` | ContentId, DefinitionCatalog |
| `BetterLife.Core.Domain.Stats` | Stats 3 ชั้น, สูตร, Breakdown, ChangeLog, History, Reputation |
| `BetterLife.Core.Domain.Traits` / `.Skills` / `.Flags` | Trait, Skill, Story flag |
| `BetterLife.Core.Domain.Characters` | Character (aggregate root), GenderProfile, BirthInfo, Wallet |
| `BetterLife.Core.Rules` (+ `.Conditions`, `.Effects`, `.Serialization`) | Rules Engine: Condition/Effect + Registry + JSON converters |
| `BetterLife.Core.Content` (+ `.Loaders`) | Content Pipeline: manifest, loaders, IContentRepository |
| `BetterLife.Core.Astrology` | ดวง 5 ด้าน, AstrologyTables, FortuneCalculator |
| `BetterLife.Core.Karma` | KarmaLedger, หนี้กรรม, KarmaService |
| `BetterLife.Core.Luck` | LuckCalculator (FinalChance), ChanceSpec |
| `BetterLife.Core.LifeEvents` | Event System: เหตุการณ์ชีวิต, Inbox, Choices, Delayed consequences |
| `BetterLife.Core.Domain.World` / `.Family` / `.Genetics` | ประเทศ/เมือง, ครอบครัว, พันธุกรรม |
| `BetterLife.Core.Creation` | CharacterCreator, ChallengeCode, CreationRequest |
| `BetterLife.Core.Sessions` | LifeSession: ประกอบทุกระบบของชีวิตหนึ่ง |
| `BetterLife.Core.Domain.Life` / `BetterLife.Core.Life` | ช่วงวัย, การแก่ตัว, โรค, ความเสี่ยงตาย, การตาย |
| `BetterLife.Core.Economy` | เศรษฐกิจมหภาค (วัฏจักร/เงินเฟ้อ/ตลาด), การเงินส่วนตัว (รายรับ-รายจ่าย, สินทรัพย์, หนี้, Credit, ล้มละลาย) |
| `BetterLife.Core.Education` | การศึกษา: หลักสูตร/สถานศึกษา, เข้าเรียน, สอบ, ทุน, ค่าเทอม, ผลการเรียน, จบการศึกษา |
| `BetterLife.Core.Careers` | อาชีพ: Career ladder, สมัครงาน, Performance, หัวหน้า, เลื่อนตำแหน่ง, ความเสี่ยง, เลิกจ้าง, เกษียณ, รายได้ |
| `BetterLife.Core.Systems.*` | ระบบเกมแต่ละตัว |
| `BetterLife.Core.Services` | Interface ของ Service ภายนอก (Ads, IAP, Analytics, CloudSave) |
| `BetterLife.Core.Persistence` | Save DTO + Mapper |
| `BetterLife.Core.Localization` | LocKey |

## ตัวป้องกันกฎสถาปัตยกรรม
- **คอมไพเลอร์:** C# 9 ปฏิเสธ file-scoped namespace / record struct, `TreatWarningsAsErrors` + `GenerateDocumentationFile` บังคับ XML doc
- **ArchitectureRulesTests:** สแกน Runtime หา `UnityEngine`, `System.Random`, `record`, `init;`
- **AssemblyDependencyRulesTests:** อ่าน asmdef ทุกไฟล์แล้วตรวจทิศทางการอ้างอิงตามกราฟข้างบน
- **CI:** GitHub Actions รัน `dotnet test` ทุก push
