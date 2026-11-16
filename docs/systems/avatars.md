# Task 27: อวตาร (หน้าตาเปลี่ยนตามอายุ) สำหรับผู้เล่นและ NPC

> สถานะ: ✅ **Implemented** — 2026-11-16 (ทำก่อนข้อ 21 ตามที่ผู้ใช้ขอ; ใช้ชื่อไฟล์และโฟลเดอร์ตามที่ผู้ใช้ push)

## 1. สรุปความเข้าใจ
- ภาพหน้าสำเร็จรูป 32 หน้า × 9 ช่วงอายุ (หน้าเลขเดียวกัน = คนเดิมที่โตขึ้น) อยู่ที่ `unity/Assets/BetterLife/Presentation/Resources/BetterLife/Avatars`
- ผู้เล่นเลือกหน้าตัวเองได้ (เฉพาะหน้าของเพศตัวเอง) และหน้าตาโตขึ้นตามอายุ · NPC ได้หน้าตามเพศและอายุ แสดงในแท็บ People และหน้ารายละเอียดคน (เพศแสดงอยู่แล้วตั้งแต่ 2026-11-16)

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ไฟล์ของผู้ใช้ | ใช้ชื่อตามที่ผู้ใช้ส่ง `age_xx_yy/age_xx_yy_NN.png` (NN = เลขหน้า) ทุกช่วงอายุโครงสร้างเดียวกัน (2026-11-16 ย้ายช่วง 36–45, 46–59, 60+ ออกจากโฟลเดอร์ย่อย `images/` ตามที่ผู้ใช้สั่ง) · content ยังระบุ "path prefix" ต่อช่วงอายุ จึงรองรับโครงสร้างอื่นได้ | ไม่ต้องเปลี่ยนชื่อไฟล์ |
| เพศของหน้า | ตรวจจากภาพ: **03–19 (ไม่มี 11) = ชาย 16 หน้า**, **20–35 = หญิง 16 หน้า** ทุกช่วงอายุเรียงหน้าเดียวกัน | ชื่อไฟล์ไม่มีเพศ |
| ไฟล์ที่ไม่ใช้ | ภาพว่าง `age_36_45_01, 02, 11, 36` และโฟลเดอร์ `Avatars/images/` **ลบแล้ว** (ผู้ใช้อนุญาต) | |
| ช่วงอายุ | 0–5, 6–10, 11–15, 16–19, 20–24, 25–35, 36–45, 46–59, 60+ | ตามโฟลเดอร์ |
| หน้าของผู้เล่น | ถ้ายังไม่เลือก = สุ่มจาก id ตัวละคร (คงที่ตลอดชีวิต ตรงเพศ) · เลือกเองได้จากหน้า "หน้าตาของคุณ" (แตะรูปบนหน้าแรก) เก็บใน `CharacterDto.AvatarFace` · ไม่ต้องใช้ Undo (เป็นเรื่องความสวยงาม) | ข้อ 25 (หน้าสร้างตัวละคร) จะใช้ตัวเลือกเดียวกันตอนเกิด |
| หน้าของ NPC | คำนวณจาก Hash ของ "id ตัวละคร:id NPC" + เพศ → คงที่ ไม่ต้องเซฟ · คนที่ย้ายไปอยู่ในชีวิตของทายาท (พ่อแม่ พี่น้อง) ถูกตรึงหน้าไว้ใน `NpcDto.AvatarFace` | หน้าไม่เปลี่ยนเมื่อโหลดเซฟหรือเล่นต่อเป็นรุ่นลูก |
| ทายาท | เล่นต่อด้วยหน้าเดียวกับที่ลูกคนนั้นมีตอนเป็น NPC | |
| คนที่เสียชีวิต | แสดงหน้าตอนอายุที่เสียชีวิต | |
| เพศเปลี่ยน / หน้าเพศอื่น | ถ้าหน้าที่เลือกไม่ตรงเพศ → ใช้หน้าที่คำนวณได้ | |
| การแสดงผล | `Resources.Load<Texture2D>` + `style.backgroundImage = new StyleBackground(texture)` (UI Toolkit) มี cache · ไม่มีไฟล์ = ซ่อนรูป + Warning ใน Console · ขนาดด้วย USS (`avatar--small/medium/large`) | ไม่ต้องลากวางใน Inspector |

## 3. JSON (`content/avatars.json`, Schema `docs/schemas/avatars.schema.json`)
```jsonc
{ "resourceRoot": "BetterLife/Avatars",
  "bands": [ { "minAge": 0, "maxAge": 5, "filePrefix": "age_00_05/age_00_05_" }, ...,
             { "minAge": 60, "maxAge": null, "filePrefix": "age_60_up/age_60_up_" } ],
  "faces": [ { "id": 3, "sex": "male" }, ..., { "id": 35, "sex": "female" } ] }
```
Path = `resourceRoot/filePrefix` + เลขหน้า 2 หลัก (ไม่มีนามสกุล)

## 4. C# Domain
| ที่อยู่ | Class |
|---|---|
| Core `BetterLife.Core.Avatars` | `AvatarFace`, `AvatarBand`, `AvatarCatalog` (`FacesFor`, `BandFor`, `FaceFor`, `PathFor`, `NpcKey`), `AvatarFaces` (หน้าของผู้เล่น/NPC) |
| Core อื่นๆ | `AvatarsLoader` (ชนิด content `avatars`), `CharacterCatalogs.Avatars`, `Character.AvatarFace`/`SetAvatarFace`, `Npc.AvatarFace`/`WithAvatarFace`, `HeirCreator` (ตรึงหน้า) |
| Core `ViewModels` | `AvatarPaths`, `AvatarOption`, `AvatarPickerViewModel` (`Build`, `Choose`), `DashboardState.AvatarPath`, `PersonRow.AvatarPath`, `PersonState.AvatarPath`, `ScreenIds.AvatarPicker` |
| Unity | `AvatarImage` (โหลด/แสดงภาพ), `AvatarPickerScreenPresenter`, หน้าแรก (การ์ดโปรไฟล์: รูป + สถานะครอบครัว แตะรูปเพื่อเปลี่ยน), แท็บ People (รูปเล็กหน้ารายชื่อ), หน้าคน (รูปใหญ่), สไตล์ใน `Screens.uss` |

## 5. Save
`CharacterDto.AvatarFace` (int?, เซฟเก่า = null = หน้าที่คำนวณ), `NpcDto.AvatarFace` (int?, เฉพาะคนที่ตรึงหน้า) — ไม่ต้องเพิ่ม saveVersion

## 6. Tests
- `AvatarTests` (5): path ตามช่วงอายุ (รองรับโฟลเดอร์ย่อย), หน้าที่คำนวณตรงเพศและคงที่, ผู้เล่นเลือกหน้า (เฉพาะเพศตัวเอง) + เซฟ/โหลด + หน้าของ NPC เหมือนเดิมหลังโหลด, NPC แสดงหน้าตามอายุ, ทายาทและครอบครัวคงหน้าเดิม
- `AvatarFilesTests` (content จริง): ชาย 16 / หญิง 16 และภาพทั้ง 288 ไฟล์ (32 × 9) มีอยู่จริงใน Resources
- โค้ด Unity ผ่าน `BetterLife.Unity.CompileCheck` (เพิ่มคลาสจำลอง `Texture2D`, `StyleBackground`)

## 7. ส่งต่อ
- ข้อ 25: หน้าสร้างตัวละครใช้ `AvatarPickerViewModel` เลือกหน้าตอนเกิด
- Popup เจอคนใหม่ / ไทม์ไลน์ / Family Tree แสดงรูป (ยังไม่ทำ)
- ยืนยันสิทธิ์ใช้งานเชิงพาณิชย์ของภาพก่อนข้อ 23 (Build/Store)
