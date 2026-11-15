# Content

ไฟล์เนื้อหาเกมทั้งหมด (`*.json`) อยู่ที่นี่

## กติกา
1. **ทุกไฟล์ต้องอยู่ใน `manifest.json`** (ระบุ `type` และ `path`) ถ้าลืมใส่ test จะ fail
2. ทุกชนิดมี JSON Schema ที่ `docs/schemas/<type>.schema.json` (ใส่ `"$schema": "../docs/schemas/<type>.schema.json"` ที่หัวไฟล์ แล้ว VS Code จะช่วยเติมคำและเตือนเมื่อพิมพ์ผิด)
3. ชนิดเดียวกันแบ่งได้หลายไฟล์ (เช่น `events/career_001.json`) และ id ห้ามซ้ำข้ามไฟล์
4. ข้อความที่ผู้เล่นเห็นต้องใช้ Localization Key เท่านั้น
5. เพิ่มเลข `contentVersion` เมื่อลบหรือเปลี่ยน id ที่อาจมีอยู่ในไฟล์เซฟ

## ตรวจ content
```bash
dotnet test dotnet/BetterLife.sln
```
ข้อผิดพลาดจะบอกชื่อไฟล์และตำแหน่ง เช่น `traits.json $.traits[2].statModifiers[0].stat: unknown stat 'mental.stres'.`
