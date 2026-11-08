# Task 00: Repo Setup

> สถานะ: ✅ Done (2026-11-08)

## ไฟล์ที่ได้
| Path | หน้าที่ |
|---|---|
| `packages/com.betterlife.core/package.json` | UPM package ของ Core |
| `packages/com.betterlife.core/Runtime/BetterLife.Core.asmdef` | `noEngineReferences: true` |
| `packages/com.betterlife.core/Tests/Runtime/BetterLife.Core.Tests.asmdef` | NUnit (Unity Test Runner) |
| `dotnet/BetterLife.sln`, `BetterLife.Core.csproj`, `BetterLife.Core.Tests.csproj` | build/test นอก Unity |
| `unity/Packages/manifest.json` | Core (local file) + test-framework + newtonsoft-json + UI Toolkit module; `testables` ทำให้เทสต์ของ Core โผล่ใน Test Runner |
| `unity/ProjectSettings/ProjectVersion.txt` | ทำให้ Unity Hub รู้จักโฟลเดอร์นี้ว่าเป็นโปรเจกต์ (เปิดด้วย 2022.3 เวอร์ชันย่อยไหนก็ได้) |
| `unity/Assets/BetterLife/*/*.asmdef` | Bootstrap, Infrastructure, Presentation, EditorTools |
| `unity/Assets/BetterLife/Bootstrap/GameBootstrap.cs` | Composition Root ที่สร้างตัวเองตอนเริ่มเกม ตอนนี้สร้างแค่ `EventBus` |
| `unity/Assets/BetterLife/EditorTools/ProjectSetupMenu.cs` | เมนู **BetterLife → Setup Project** |
| `.github/workflows/core-tests.yml` | CI |

## ProjectSetupMenu ทำอะไร (รันซ้ำได้ ผลเหมือนเดิม)
1. สร้างโฟลเดอร์ `Scenes`, `Presentation/UI/Uxml`, `Presentation/UI/Uss`
2. Player Settings: ชื่อบริษัท/ชื่อเกม, Portrait อย่างเดียว, Bundle ID `com.betterlife.game` (Android + iOS), IL2CPP, .NET Standard, Android ARM64
3. สร้าง `Scenes/Main.unity` (ฉากว่าง) แล้วตั้งเป็นฉากแรกใน Build Settings

## ข้อจำกัด
- คอมไพล์โค้ดฝั่ง Unity (Bootstrap, EditorTools) ในเครื่อง CI ไม่ได้ เพราะไม่มี Unity Engine DLL ต้องตรวจตอนเปิดใน Unity Editor
- API ที่ใช้ เช่น `NamedBuildTarget` และ `ApiCompatibilityLevel.NET_Standard` มีตั้งแต่ Unity 2021.2 ขึ้นไป
