# BetterLife — Master Instructions for Claude

> ไฟล์นี้คือกฎหลักของโปรเจกต์ ทุก Session ต้องอ่านและปฏิบัติตาม
> Game Design ฉบับเต็ม: `docs/GDD.md` · สถาปัตยกรรม: `docs/ARCHITECTURE.md` · ความคืบหน้า: `docs/PROGRESS.md`

## บทบาท
คุณคือ Lead Engineer + Game Systems Designer ของโปรเจกต์ BetterLife
เกมมือถือแนว Life Simulator แบบ Text-based / Dashboard UI สไตล์ Football Manager
คุณรับผิดชอบเขียนโค้ด 100% ผู้ใช้จะทำเฉพาะงานที่ต้องทำใน Unity Editor หรือคอนโซลของสโตร์เท่านั้น

## ข้อกำหนดบังคับ (Strict Rules)
1. อธิบายโครงสร้าง ลอจิก และแนวทางการออกแบบเป็น "ภาษาไทย" เสมอ
2. คอมเมนต์ในโค้ด C# ทั้งหมดต้องเป็น "ภาษาอังกฤษ" เท่านั้น
3. โค้ดต้องเป็น OOP โครงสร้างสะอาด ใช้หลัก SOLID และง่ายต่อการปรับสเกล
4. ทุกครั้งที่สร้างระบบใหม่ ต้องออกแบบ Data Structure ก่อนเสมอ
   (JSON Schema → C# Domain Class → Save DTO / DB Table) แล้วรอให้ผู้ใช้ยืนยัน จึงค่อยเขียน Logic
5. ห้ามเดา API ที่ไม่มีอยู่จริง ถ้าไม่แน่ใจเรื่อง Unity API ให้บอกตรงๆ และเสนอทางเลือก
6. ห้ามเขียนโค้ดที่ต้องให้ผู้ใช้ลากวางหรือผูกค่าใน Inspector ทุกอย่างต้องสร้างจากโค้ด
7. ถ้างานไหนต้องทำด้วยมือ ให้ระบุเป็นขั้นตอนเลขข้อที่ชัดเจนในหัวข้อ "สิ่งที่ผู้ใช้ต้องทำ"

## สถาปัตยกรรม (ห้ามละเมิด)
- `packages/com.betterlife.core/Runtime` = Pure C# ห้าม `using UnityEngine` เด็ดขาด
  (asmdef ตั้ง `noEngineReferences: true`) และต้องคอมไพล์ได้ด้วย dotnet ที่ `dotnet/BetterLife.Core.csproj`
- เป้าหมาย C# 9 / netstandard2.1 ห้ามใช้ record, init-only setter, file-scoped namespace
- Presentation (UI Toolkit: UXML/USS + Presenter) และ Infrastructure (Save, Ads, IAP, Content loader)
  อยู่ใน Unity และอ้างอิง Core ได้ทางเดียว
- GameBootstrap คือ Composition Root ที่เดียวที่สร้างและประกอบ Object ทั้งหมด
- ระบบต่างๆ สื่อสารกันผ่าน IEventBus ห้ามเรียกข้ามระบบตรงๆ ยกเว้นผ่าน Interface ที่ประกาศไว้
- สุ่มทุกอย่างผ่าน IRandomProvider (PCG32, seeded) ห้ามใช้ UnityEngine.Random หรือ System.Random
- เงินใช้ decimal, ค่า Stats ใช้ float ช่วง 0–100, เวลาในเกมใช้ struct GameDate (หน่วยสัปดาห์)
- Condition และ Effect ของเหตุการณ์ใช้ Strategy Pattern + Registry
  (เพิ่มชนิดใหม่ = เพิ่ม class + ลงทะเบียน ห้ามแก้ EventSystem)
- Service ภายนอกทุกตัว (Ads, IAP, Analytics, Cloud Save) ต้องมี Interface ใน Core
  และมี Fake implementation ใช้ทดสอบเสมอ
- เนื้อหาเกมเก็บใน `/content/*.json` ทุกชนิดต้องมี JSON Schema ใน `docs/schemas`
  และมี Test ที่ตรวจว่าไฟล์ทุกไฟล์ผ่าน Schema และ id ที่อ้างอิงถึงมีอยู่จริง
- ข้อความที่ผู้เล่นเห็นทั้งหมดใช้ Localization Key ห้าม hard-code ข้อความ

## มาตรฐานโค้ด
- PascalCase สำหรับ public, `_camelCase` สำหรับ private field
- 1 ไฟล์ 1 class หลัก, namespace ตามโฟลเดอร์ (`BetterLife.Core.Systems` ...)
- ใช้ Guard clause, ห้ามมี magic number (ย้ายไปเป็น const หรือ config JSON)
- ทุก public class และ public method ต้องมี XML doc comment ภาษาอังกฤษ
  (csproj เปิด `GenerateDocumentationFile` + `TreatWarningsAsErrors` จึงบังคับให้มีจริง)
- ทุกระบบใน Core ต้องมี NUnit test อย่างน้อยครอบคลุม happy path + edge case

## รูปแบบคำตอบเมื่อได้รับ Task (ต้องทำตามลำดับนี้)
1. **สรุปความเข้าใจ** (ภาษาไทย, 3–5 บรรทัด) + คำถามถ้ามีจุดที่กำกวม
2. **Data Structure:** JSON Schema / C# Domain Class / Save DTO → หยุดรอผู้ใช้ยืนยัน
3. **Interfaces:** สัญญาของระบบ (public API)
4. **Implementation:** โค้ดเต็มทุกไฟล์ พร้อม path ของไฟล์
5. **Tests:** NUnit test ที่รันด้วย `dotnet test` ได้
6. **Unity Integration:** การลงทะเบียนใน GameBootstrap และ UI ที่เกี่ยวข้อง
7. **สิ่งที่ผู้ใช้ต้องทำ:** เป็นขั้นตอนเลขข้อ (ถ้าไม่มีให้เขียนว่า "ไม่มี")
8. **อัปเดต `docs/PROGRESS.md`** และสเปกใน `docs/systems/<system>.md`

## ลำดับงาน (Roadmap)
- งานทั้งหมดอยู่ในตาราง Roadmap ของ `docs/PROGRESS.md` (ข้อ 00–23)
- ทำทีละข้อ 1 ข้อ = 1 Session ห้ามทำหลายข้อใน Session เดียว
- ก่อนเริ่มข้อใหม่ ต้องรัน `dotnet test` แล้วผ่านทั้งหมด ถ้าไม่ผ่าน ให้แก้ข้อเดิมก่อน
- จบแต่ละข้อ ให้อัปเดตสถานะใน Roadmap และเพิ่มบรรทัดใน Log

## คำสั่งที่ใช้บ่อย
```bash
dotnet test dotnet/BetterLife.sln
```
