# Task 04: Content Pipeline

> สถานะ: ✅ **Implemented** — 2026-11-08

## 0. การตัดสินใจที่ผู้ใช้ยืนยันแล้ว
| หัวข้อ | ผลการตัดสินใจ |
|---|---|
| รายชื่อไฟล์ | `content/manifest.json` |
| ใส่ content ลงในเกมที่ Build แล้ว | คัดลอกไปที่ `Resources/Content` อัตโนมัติก่อน Build (และ Build จะล้มถ้า content มี error) |
| ตรวจ Schema ใน test | JsonSchema.Net 7.3.4 (MIT) ในโปรเจกต์ `dotnet/BetterLife.Content.Tests` |
| Content ใหม่ | `reputation_domains` (9 วงการ) + `health_conditions` (9 อาการ) |

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

## 4. ไฟล์ที่ได้
- `Runtime/Content/`: `IContentSource`, `FileSystemContentSource`, `InMemoryContentSource`, `ContentPaths`, `ContentJson`, `ContentManifest`, `ContentFileEntry`, `ContentDocument`, `IContentTypeLoader`, `ContentLoadContext`, `ContentPipeline`, `IContentRepository`, `ContentRepository`, `ContentError`, `ContentLoadException`, `ContentLoadResult`, `ContentTypes`, `DefaultContent`
- `Runtime/Content/Loaders/`: `DefinitionLoader<T>` (base), `StatsLoader` + `IStatFormulaReader`/`WeightedStatFormulaReader`, `TraitsLoader`, `SkillsLoader`, `FlagsLoader`, `ReputationDomainsLoader`, `HealthConditionsLoader`, `CharacterCatalogsBuilder`, `ContentReading`
- Domain: `ReputationDomainDefinition`, `HealthConditionDefinition` + เพิ่มเข้า `CharacterCatalogs` ตอนนี้ `ReputationBook` และ `Character.SetCondition` รับเฉพาะ id ที่ประกาศไว้ และ Rules ตรวจ id ของวงการและโรคตั้งแต่ตอนโหลด
- `content/manifest.json`, `content/reputation_domains.json`, `content/health_conditions.json`
- `docs/schemas/manifest.schema.json`, `reputation_domains.schema.json`, `health_conditions.schema.json` (ทุก Schema ของ content รับ `$schema` ได้ เพื่อให้ VS Code ช่วยเติมคำ)
- Unity: `Infrastructure/Content/{ContentLocations, ResourcesContentSource, ContentSourceFactory}.cs`, `EditorTools/{ContentSync, ContentBuildStep}.cs`, `GameBootstrap` โหลด content ตอนเริ่มเกม

## 5. Tests
- **Core (`ContentPipelineTests`, 17 test):**
  - โหลดครบทุกชนิด
  - หลายไฟล์ต่อชนิด และ id ซ้ำข้ามไฟล์
  - รวบรวม error พร้อมไฟล์และ path แล้วข้ามชนิดที่ขึ้นกับชนิดที่พัง
  - น้ำหนักสูตรผิด
  - manifest ผิด 5 แบบ
  - ไฟล์หาย / JSON เสีย / property ซ้ำ
  - ฟิลด์ nullable ต้องมีอยู่
  - formula ชนิดที่ไม่รู้จัก
  - การตั้งค่า loader ผิด (ซ้ำ / dependency ไม่มี / วน)
  - ลำดับการโหลด
  - การตรวจ path
- **Content (`ContentValidationTests`, 6 test):** ตรวจโฟลเดอร์ `content/` จริง
  - manifest และทุกไฟล์ผ่าน Schema
  - ทุกชนิดมี Schema
  - manifest ตรงกับไฟล์ในโฟลเดอร์
  - โหลดผ่าน Pipeline ได้โดยไม่มี error
  - Schema จับฟิลด์ที่พิมพ์ผิดได้

## 6. วิธีเพิ่ม content ชนิดใหม่ (ข้อ 07 เป็นต้นไป)
1. เขียน loader (สืบทอด `DefinitionLoader<T>` หรือ implement `IContentTypeLoader` เอง) และใส่ `DependsOn` = `character_catalogs` ถ้าใน content มี rule
2. ใช้ `context.ReadContext(path)` + `context.Rules.ParseCondition/ParseEffects` อ่าน rule
3. เพิ่ม loader ลงใน `DefaultContent.CreateLoaders()`
4. สร้าง `docs/schemas/<type>.schema.json` (อ้างถึง rule ด้วย `"$ref": "rules.schema.json#/$defs/effects"`)
5. ใส่ไฟล์ลงใน `content/manifest.json`

## 7. งานที่ส่งต่อ
- **ข้อ 09:** เพิ่มฟิลด์ของ `health_conditions` (ระยะเวลา และผลต่อ disease burden)
- **ข้อ 14:** เก็บ `ContentVersion` ลงไฟล์เซฟ
- **ข้อ 20:** test ตรวจว่าทุก Loc key ใน content มีข้อความแปล
