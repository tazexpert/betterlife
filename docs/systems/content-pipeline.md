# Task 04: Content Pipeline

> สถานะ: ⏸ **ข้อเสนอ Data Structure — รอผู้ใช้ยืนยัน** (ยังไม่มี Logic)

## 1. สรุปความเข้าใจ
ระบบที่อ่านไฟล์ `content/*.json` ทั้งหมด → ตรวจความถูกต้อง → แปลงเป็น catalog จากข้อ 02/03 → ส่งให้ระบบเกมใช้ผ่าน `IContentRepository`
การตรวจมี 2 ชั้น:
1. **Schema** (โครงสร้าง: ชนิดข้อมูล, ฟิลด์บังคับ, ฟิลด์เกิน) — รันใน `dotnet test` / CI
2. **Semantic** (ความหมาย: id ที่อ้างอิงมีจริง, น้ำหนักรวม = 1, Flag ถูกชนิด, id ซ้ำข้ามไฟล์) — รันทุกครั้งที่เกมโหลด content และใน test
ข้อผิดพลาดถูก **รวบรวมทั้งหมด** (ไม่หยุดที่จุดแรก) พร้อมชื่อไฟล์ + JSON path เพื่อให้คนเขียน content แก้ได้ทีเดียว

## 2. Data Structure

### 2.1 `content/manifest.json` (ใหม่) → `docs/schemas/manifest.schema.json`
```json
{
  "contentVersion": 1,
  "files": [
    { "type": "stats",  "path": "stats.json" },
    { "type": "traits", "path": "traits.json" },
    { "type": "events", "path": "events/career_001.json" }
  ]
}
```
- **ทำไมต้องมี manifest:** บน Android อ่านรายชื่อไฟล์ในโฟลเดอร์ที่อยู่ใน APK ไม่ได้ เกมจึงต้องรู้รายชื่อไฟล์ล่วงหน้า
- ชนิดเดียวกันมีได้หลายไฟล์ เช่น เหตุการณ์ชุดละ 30–50 อันในข้อ 22 ระบบจะรวมให้ และตรวจ id ซ้ำข้ามไฟล์
- `contentVersion`: เพิ่มเลขเมื่อ content เปลี่ยนแบบที่กระทบเซฟ (Save System ข้อ 14 ใช้ค่านี้)
- มี test ตรวจว่าไฟล์ `.json` ทุกไฟล์ใน `content/` อยู่ใน manifest และทุกไฟล์ใน manifest มีอยู่จริง

### 2.2 Content ใหม่ 2 ชนิด (เดิมในข้อ 03 ตรวจได้แค่รูปแบบ id)
```json
// content/reputation_domains.json
{ "domains": [ { "id": "business", "nameKey": "reputation.business.name" },
               { "id": "underworld", "nameKey": "reputation.underworld.name" } ] }

// content/health_conditions.json  (ข้อ 09 จะเพิ่มฟิลด์ เช่น ระยะเวลา และผลต่อร่างกาย)
{ "conditions": [ { "id": "flu", "nameKey": "condition.flu.name", "descKey": "condition.flu.desc" } ] }
```
→ เพิ่ม `ReputationDomainDefinition`, `HealthConditionDefinition` และเพิ่ม catalog 2 ตัวเข้าไปใน `CharacterCatalogs` เพื่อให้ Rules ตรวจ id ได้ครบ

### 2.3 C# (`Runtime/Content/`)
| Class | หน้าที่ |
|---|---|
| `ContentManifest`, `ContentFileEntry` | ข้อมูลจาก manifest.json |
| `IContentSource` | `string ReadText(string relativePath)`: แหล่งไฟล์ (สลับได้ตาม platform) |
| `FileSystemContentSource` | อ่านจากโฟลเดอร์จริง (ใช้ใน Editor และ test) |
| `InMemoryContentSource` | ใช้ใน unit test |
| `ContentDocument` | ไฟล์ที่ parse แล้ว: `Path`, `JObject Root` |
| `IContentTypeLoader` | `TypeName`, `DependsOn[]`, `Load(documents, context)`: แปลงไฟล์ชนิดหนึ่งเป็น catalog |
| `ContentLoadContext` | ให้ loader ดึง catalog ที่โหลดแล้ว (`Require<T>()`), `RuleRegistry` และที่ส่งผลลัพธ์ (`Add<T>()`) |
| `StatsLoader`, `TraitsLoader`, `SkillsLoader`, `FlagsLoader`, `ReputationDomainsLoader`, `HealthConditionsLoader` | 1 loader ต่อ 1 ชนิด |
| `ContentPipeline` | อ่าน manifest → จัดลำดับ loader ตาม `DependsOn` (ตรวจ dependency วน) → โหลด → รวบรวม error |
| `IContentRepository` / `ContentRepository` | `T Get<T>()`, `bool TryGet<T>(out T)`, `int ContentVersion` |
| `ContentError` | `File`, `Path`, `Message` |
| `ContentLoadException` | รวม `ContentError` ทั้งหมด |
| `ContentLoadResult` | `Repository` (null ถ้ามี error) + `Errors` |

- **เพิ่มชนิด content ใหม่ (ข้อ 07 เป็นต้นไป)** = เขียน loader 1 class + Schema 1 ไฟล์ + ลงทะเบียน 1 บรรทัด โดยไม่ต้องแก้ Pipeline
- ถ้าชนิดหนึ่งมี error ชนิดที่ขึ้นกับมันจะถูกข้าม และรายงานว่า "ข้ามเพราะ X มีปัญหา" เพื่อไม่ให้ error ลามจนอ่านไม่รู้เรื่อง
- ตั้งค่า JSON: `FloatParseHandling.Decimal` (เงินแม่นยำ), `DateParseHandling.None` (กันไม่ให้ Newtonsoft แปลงข้อความที่หน้าตาเหมือนวันที่เอง)

### 2.4 Test ตรวจ content (`dotnet/BetterLife.Content.Tests/`, โปรเจกต์ใหม่)
1. ทุกไฟล์ใน manifest ผ่าน Schema ของชนิดนั้น (`docs/schemas/<type>.schema.json`)
2. ทุกชนิดใน manifest มีไฟล์ Schema
3. manifest ตรงกับไฟล์จริงในโฟลเดอร์ (ไม่มีไฟล์ตกหล่นหรือเกินมา)
4. โหลด content ทั้งหมดผ่าน `ContentPipeline` ได้โดยไม่มี error (ตรวจการอ้างอิง id ทั้งหมด)

แยกเป็นโปรเจกต์ dotnet อย่างเดียว เพราะไลบรารีตรวจ Schema ใช้ใน Unity ไม่ได้ และไม่จำเป็นตอนเล่นจริง

### 2.5 Unity (Infrastructure + EditorTools)
| ส่วน | หน้าที่ |
|---|---|
| Editor (กด Play) | อ่านจาก `content/` ของ repo ตรงๆ ผ่าน `FileSystemContentSource` เพื่อไม่ให้มีสำเนาเก่าค้าง |
| `ContentBuildStep` (`IPreprocessBuildWithReport`) | ก่อน Build ระบบจะคัดลอก `content/` ไปที่ `Assets/BetterLife/Resources/Content/` อัตโนมัติ (ไฟล์สำเนาอยู่ใน .gitignore) |
| `ResourcesContentSource` | ใช้ในเกมที่ Build แล้ว: อ่านด้วย `Resources.Load<TextAsset>()` |
| `GameBootstrap` | สร้าง `ContentPipeline` → โหลด → ถ้ามี error จะ log ทุกข้อแล้วหยุดเริ่มเกม |

## 3. Save DTO
**ไม่มี**: content ไม่ถูกเซฟ (ข้อ 14 จะเก็บแค่ `contentVersion` ลงในไฟล์เซฟ)

## 4. คำถามที่ต้องยืนยัน
1. **ใช้ manifest.json** ระบุรายชื่อไฟล์ (แนะนำ: จำเป็นสำหรับ Android และมี test กันลืมใส่ชื่อไฟล์) หรือให้ระบบเดาชนิดจากชื่อโฟลเดอร์เอง?
2. **วิธีใส่ content ลงในเกมที่ Build แล้ว:** คัดลอกไปไว้ใน `Resources` อัตโนมัติตอน Build (แนะนำ: อ่านได้ทันทีทุก platform และโค้ดง่าย) หรือใช้ `StreamingAssets` (เปิดให้ทำ mod ได้ภายหลัง แต่บน Android ต้องอ่านแบบ async)?
3. **ไลบรารีตรวจ Schema สำหรับ test:** `JsonSchema.Net` (แนะนำ: MIT, รองรับ draft 2020-12 เต็มรูปแบบ, ใช้แค่ใน test ไม่ติดไปกับเกม) หรือ `NJsonSchema`?
   (หมายเหตุ: `Newtonsoft.Json.Schema` ใช้ไม่ได้ เพราะเป็น license AGPL/เชิงพาณิชย์)
4. **เพิ่ม content 2 ชนิดใหม่** (วงการชื่อเสียง + โรค/อาการ แบบขั้นต่ำ) ตอนนี้เลย (แนะนำ) หรือรอไปทำในข้อ 09?
