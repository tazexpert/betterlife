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
- **GameBootstrap** (ใน Unity) เป็น Composition Root ที่เดียว สร้างทุก Object ด้วยโค้ด ไม่ใช้ Inspector

## โครงสร้างโฟลเดอร์
```
CLAUDE.md                     กฎหลักของโปรเจกต์
content/                      เนื้อหาเกม *.json
docs/
  GDD.md  ARCHITECTURE.md  PROGRESS.md
  schemas/                    JSON Schema ของเนื้อหาแต่ละชนิด
  systems/                    สเปกรายระบบ
dotnet/
  BetterLife.sln
  BetterLife.Core.csproj      คอมไพล์ Runtime ของ Core
  BetterLife.Core.Tests.csproj รันเทสต์ชุดเดียวกับ Unity Test Runner
packages/com.betterlife.core/
  package.json
  Runtime/                    BetterLife.Core.asmdef + โค้ด Core
  Tests/Runtime/              BetterLife.Core.Tests.asmdef + NUnit tests
```

## Namespace ที่วางแผนไว้ (Core)
| Namespace | หน้าที่ |
|---|---|
| `BetterLife.Core.Time` | GameDate, CalendarDate, Tick pipeline |
| `BetterLife.Core.Random` | IRandomProvider, Pcg32 |
| `BetterLife.Core.Events` | IEventBus, IGameEvent |
| `BetterLife.Core.Stats` | Stats 3 ชั้น + Breakdown |
| `BetterLife.Core.Systems.*` | ระบบเกมแต่ละตัว |
| `BetterLife.Core.Services` | Interface ของ Service ภายนอก (Ads, IAP, Analytics, CloudSave) |
| `BetterLife.Core.Persistence` | Save DTO + Mapper |
| `BetterLife.Core.Localization` | LocKey |

## ตัวป้องกันกฎสถาปัตยกรรม
- **คอมไพเลอร์:** C# 9 ปฏิเสธ file-scoped namespace / record struct, `TreatWarningsAsErrors` + `GenerateDocumentationFile` บังคับ XML doc
- **ArchitectureRulesTests:** สแกน Runtime หา `UnityEngine`, `System.Random`, `record`, `init;`
