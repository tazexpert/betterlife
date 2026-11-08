# Task 11: Education + Career

> สถานะ: ✅ **Implemented** — 2026-11-08 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **การศึกษา:** ระบบการศึกษาไทย (อนุบาล → ประถม → ม.ต้น → ม.ปลาย/ปวช. → ปวส./ปริญญาตรี → โท → เอก) + ทางเลือกนอกระบบ (กศน.)
  มีการสอบแข่งขันเข้าสถานศึกษาที่มีชื่อเสียง ทุนการศึกษา และกู้ กยศ.
  ผลการเรียน (Performance 0–100 แสดงเป็น GPA) มาจากชั่วโมงเรียนจริงในงบเวลา + IQ + Knowledge + Mental
- **อาชีพ:** Career Ladder (ตำแหน่งหลายขั้น), Performance, ความพอใจของหัวหน้า (Boss satisfaction), ความเครียดจากงาน
  ฟรีแลนซ์/ธุรกิจ (รายได้ผันผวนตามเศรษฐกิจ), สายเสี่ยงสูงและสายผิดกฎหมาย (ความเสี่ยงรายเดือนที่ใช้ Rule effect จาก content),
  การเลิกจ้างตามช่วงเศรษฐกิจ, การไล่ออก, เกษียณ + บำนาญ
- **อาชีพพรีเมียม** มี Flag `premium` ใน content และมี Test บังคับว่า **รายได้ต้องไม่สูงกว่า** อาชีพปกติในหมวดเดียวกัน

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| ระดับการศึกษา | แต่ละหลักสูตรมี `rank` (1 อนุบาล … 8 ปริญญาเอก) หลักสูตรเทียบเท่ากันใช้ rank เดียวกัน (ม.6 = ปวช. = กศน. = 4) | Condition "จบอย่างน้อยระดับ X" ตรวจด้วย rank ได้ทันที |
| ชั่วโมงเรียน/ทำงาน | เพิ่มกิจกรรม `school` และ `work` ในงบเวลา เมื่อเข้าเรียนหรือได้งาน ระบบจะ**จองชั่วโมง**ตามที่หลักสูตร/อาชีพกำหนด (ไม่เกินชั่วโมงว่าง) และจองซ้ำหลังเปลี่ยนช่วงวัย ผู้เล่นลดชั่วโมงเองได้ แต่ผลการเรียน/งานจะตก | เวลาเป็นทรัพยากรหลักของเกม ทุกอย่างต้องแลกกัน |
| Performance | ทุกสัปดาห์ค่าเป้าหมาย = ถ่วงน้ำหนัก (การเข้าเรียน/ทำงานครบชั่วโมง, IQ/ทักษะ, Knowledge, Mental, ความล้า) + โบนัสคุณภาพสถานศึกษา แล้วค่าจริงขยับเข้าหาเป้าหมายทีละนิด (smoothing) | ค่าไม่กระโดด ผลของการขยันจะสะสมอย่างสมจริง |
| ปีการศึกษา | ครบ 52 สัปดาห์นับจากวันเริ่มปี: Performance ≥ 40 ได้เลื่อนชั้น ถ้าต่ำกว่าจะซ้ำชั้น และซ้ำเกิน 2 ครั้งจะถูกให้ออก | |
| เข้าเรียนอัตโนมัติ | ระหว่างที่ยังพึ่งพ่อแม่ ทุกต้นปีจะเข้าเรียนหลักสูตรที่ `autoEnroll` ระดับถัดไปให้เอง โดยเลือกสถานศึกษาตามฐานะครอบครัว (`familyMinIncome`) ครอบครัวจ่ายค่าเทอม | เด็กไม่ได้เลือกโรงเรียนเอง |
| ภาคบังคับ | ประถม + ม.ต้น ห้ามลาออกระหว่างที่ยังพึ่งพ่อแม่ | กฎหมายการศึกษาภาคบังคับ 9 ปี |
| สอบเข้า | สถานศึกษาที่มี `difficulty` ต้องสอบ: โอกาส = FinalChance (Base + Stat + ดวงการเรียน + กรรม) + (คะแนนสอบ − ความยาก) × 1.5 โดยคะแนนสอบ = 60% ผลการเรียนของวุฒิล่าสุด + 40% Knowledge สอบหลักสูตรเดิมได้ปีละครั้ง | ใช้ระบบโอกาสเดียวกับทั้งเกม |
| ทุน | ถ้าคะแนนสอบ ≥ 85 จะสุ่มได้ทุน (ไม่ต้องจ่ายค่าเทอม) และรักษาทุนไว้ได้ถ้า Performance ≥ 75 ทุกปี | |
| ค่าเทอม | จ่ายทุกต้นปีการศึกษา: ถ้ายังพึ่งพ่อแม่ ครอบครัวจ่าย / จ่ายเองด้วยเงินสด (ถ้าไม่พอจะเข้าระบบยอดขาดของข้อ 10) / กู้ กยศ. (`student_loan`) ผ่าน `ILoanProvider` | ไม่เรียก FinanceService ตรงๆ |
| สมัครงาน | ต้องผ่าน `requirement` (Rule condition เช่น จบปริญญาตรี) และสุ่มตามโอกาสของอาชีพ + (ชื่อเสียงสถานศึกษาของวุฒิสูงสุด − 50) × 0.2 สมัครอาชีพเดิมซ้ำได้ทุก 12 สัปดาห์ ถ้าได้งานใหม่จะลาออกจากงานเดิมอัตโนมัติ | |
| เลื่อนตำแหน่ง | เมื่ออยู่ในตำแหน่งครบ `promotionAfterMonths` จะถูกประเมินทุก 12 เดือน ต้องมี Performance ≥ เกณฑ์ของตำแหน่ง และหัวหน้าพอใจ ≥ 50 จึงจะสุ่มด้วยโอกาส + (Performance − เกณฑ์) × 1.5 + ดวงการงาน | |
| หัวหน้า | ความพอใจขยับตาม Performance (ช้ากว่า) ถ้าต่ำกว่า 20 ติดต่อกัน 3 เดือนจะถูกไล่ออก (ตั้ง Flag `career.fired_once`) ส่วน Rule effect `career.boss` ใช้ในเหตุการณ์ได้ | |
| ความเครียดจากงาน | ทุกสัปดาห์ Stress += ชั่วโมงทำงาน × `stressPerHour` × (1 + (50 − หัวหน้าพอใจ)/50) | งานหนักและหัวหน้าไม่พอใจ = เครียดมาก |
| รายได้ | เงินเดือนตามตำแหน่ง (บาทมูลค่าจริงปี 2026, เสียภาษี) จ่ายผ่าน `IIncomeSource` ของข้อ 10 ส่วนอาชีพที่มี `incomeVolatility` > 0 (ฟรีแลนซ์/ธุรกิจ/เสี่ยงสูง) จะได้ เงินเดือน × max(0, 1 + noise + beta × ผลตอบแทนหุ้นเฉลี่ยของช่วงเศรษฐกิจ) × (0.5 + Performance/100) | ฟรีแลนซ์ขึ้นกับฝีมือ ธุรกิจขึ้นกับเศรษฐกิจ |
| เลิกจ้าง | ทุกเดือนสุ่มตามโอกาสของช่วงเศรษฐกิจ (`career_settings.layoffs`) × `layoffSensitivity` ของอาชีพ (ข้าราชการ = 0) | |
| สายเสี่ยง/ผิดกฎหมาย | `risk = { monthlyChance, endsJob, reasonKey, effects }` effects เป็น Rule effect จาก content (เช่น ปรับเงิน, Flag `career.criminal_record`, กรรมชั่ว) | เพิ่มความเสี่ยงแบบใหม่ได้โดยไม่ต้องแก้โค้ด |
| เกษียณ | ครบ 60 ปี จะเกษียณอัตโนมัติ และได้บำนาญรายเดือน = เงินเดือนสุดท้าย × `pensionRate` (ข้าราชการ 0.5) | |
| อายุทำงาน | ขั้นต่ำ 15 ปี | |
| ที่เก็บ | `CharacterEducation` และ `CharacterCareer` อยู่ใน Character (เพื่อให้ Rule อ่านได้) ส่วน `EducationService` / `CareerService` อยู่ใน LifeSession | เหมือน CharacterFinances |
| Random stream | `education` (สอบ/ทุน) และ `career` (สมัครงาน/เลื่อนตำแหน่ง/ความเสี่ยง/รายได้ผันผวน) | |

## 3. JSON Schema (content ใหม่ 4 ชนิด)
```jsonc
// education_programs.json (ชนิด education_programs) — โหลดก่อน rule_types และลงทะเบียน condition "education"
{ "programs": [ {
    "id": "bachelor", "nameKey": "education.bachelor.name", "rank": 6, "years": 4,
    "entryAge": { "min": 17, "max": null }, "requiresRank": 4, "compulsory": false, "autoEnroll": false, "weeklyHours": 25,
    "skills": [ { "skill": "english", "gainPerHour": 0.02 } ],
    "institutions": [ { "id": "bachelor.top_public", "nameKey": "institution.bachelor.top_public.name", "prestige": 90,
                        "tuitionPerYear": 30000, "difficulty": 75, "quality": 5, "familyMinIncome": null } ] } ] }

// education_settings.json (ไฟล์เดียว)
{ "schoolActivity": "school", "studyActivity": "study", "startPerformance": 60,
  "performance": { "weeklySmoothing": 0.08, "attendanceWeight": 0.45, "iqWeight": 0.2, "knowledgeWeight": 0.2, "mentalWeight": 0.15,
                   "studyBonusPerHour": 0.5, "maxStudyBonus": 10 },
  "yearEnd": { "passPerformance": 40, "maxRepeats": 2 },
  "admission": { "chance": { "base": 50, "stats": [], "fortune": "study" }, "perPointAboveDifficulty": 1.5,
                 "performanceWeight": 0.6, "knowledgeWeight": 0.4, "retryWeeks": 52 },
  "scholarship": { "minScore": 85, "chance": { "base": 40, "fortune": "study" }, "keepPerformance": 75 },
  "studentLoanProduct": "student_loan" }

// careers.json (ชนิด careers) — โหลดหลัง rule_types ก่อน activities/events และลงทะเบียน condition "career"
{ "tracks": [ {
    "id": "office_worker", "nameKey": "career.office_worker.name", "category": "corporate", "premium": false,
    "requirement": { "type": "education", "completed": "bachelor" },
    "hiring": { "base": 50, "stats": [ { "stat": "hidden.charisma", "weight": 0.2 } ], "fortune": "career" },
    "weeklyHours": 45, "performanceSkill": null, "skills": [], "stressPerHour": 0.06,
    "incomeVolatility": 0, "marketBeta": 0, "layoffSensitivity": 1, "pensionRate": 0, "risk": null,
    "levels": [ { "id": "staff", "titleKey": "career.office_worker.staff", "monthlySalary": 18000,
                  "promotionAfterMonths": 24, "promotionPerformance": 65 } ] } ] }
// risk: { "monthlyChance": 0.02, "endsJob": true, "reasonKey": "career.risk.arrested", "effects": [ Rule effects ] }

// career_settings.json (ไฟล์เดียว)
{ "workActivity": "work", "minWorkAge": 15, "retirementAge": 60,
  "performance": { "weeklySmoothing": 0.06, "attendanceWeight": 0.5, "skillWeight": 0.2, "mentalWeight": 0.2, "restWeight": 0.1, "start": 55 },
  "boss": { "weeklySmoothing": 0.03, "start": 55, "fireBelow": 20, "fireAfterMonths": 3 },
  "hiring": { "prestigePerPoint": 0.2, "retryWeeks": 12 },
  "promotion": { "chance": { "base": 40, "fortune": "career" }, "perPointAbove": 1.5, "minBoss": 50, "reviewEveryMonths": 12 },
  "layoffs": [ { "phase": "recession", "monthlyChance": 0.01 } ] }
```
**Rule ใหม่**
- Condition `{ "type": "education", "completed": "bachelor" }` / `{ "enrolled": true }` / `{ "enrolledIn": "bachelor" }` (เลือกได้ 1 แบบ)
- Condition `{ "type": "career", "employed": true }` / `{ "category": "illegal" }` / `{ "track": "office_worker", "minLevel": "senior" }`
- Effect `{ "type": "education.performance", "amount": 5 }`, `{ "type": "career.performance", "amount": {min,max} }`, `{ "type": "career.boss", "amount": -10 }`, `{ "type": "career.fire" }`

## 4. C# Domain
| Namespace | Class |
|---|---|
| `BetterLife.Core.Education` | `EducationProgramDefinition`, `InstitutionDefinition`, `EducationSettings` (+ `EducationPerformanceWeights`, `AdmissionRules`, `ScholarshipRules`), `CharacterEducation`, `Enrollment`, `Credential`, `TuitionFunding`, `EducationEndReason`, `AdmissionQuote`, `AdmissionResult`, `EducationService`, `EducationWeekPhase`, events (`EnrolledEvent`, `SchoolYearEndedEvent`, `EducationEndedEvent`) |
| `BetterLife.Core.Careers` | `CareerCategory`, `CareerTrackDefinition`, `CareerLevelDefinition`, `CareerRisk`, `CareerSettings` (+ `WorkPerformanceWeights`, `BossRules`, `PromotionRules`), `CharacterCareer`, `Job`, `PastJob`, `JobEndReason`, `HiringQuote`, `HiringResult`, `CareerService`, `CareerWeekPhase`, `CareerMonthPhase`, `CareerIncomeSource`, `PensionIncomeSource`, events (`JobStartedEvent`, `JobEndedEvent`, `PromotedEvent`) |
| `BetterLife.Core.Economy` (เพิ่ม) | `IDependencyStatus`, `ILoanProvider` (FinanceService implement) |
| Rules | `EducationCondition`, `CareerCondition`, `EducationPerformanceEffect`, `CareerPerformanceEffect`, `BossSatisfactionEffect`, `FireEffect` |

## 5. Save DTO
- `CharacterDto.Education = EducationDto { Current: EnrollmentDto?, Credentials[CredentialDto], Attempts[AttemptDto {Id, Week}] }`
- `CharacterDto.Career = CareerDto { Current: JobDto?, History[PastJobDto], PensionMonthly, Retired, Attempts[AttemptDto] }`
- ชั่วโมงที่จองไว้อยู่ใน `TimeStateDto` อยู่แล้ว · `EducationDtoMapper.DropUnknown` / `CareerDtoMapper.DropUnknown` ลบหลักสูตร/อาชีพที่ถูกลบออกจาก content

## 6. ส่งต่อ
- ข้อ 12: เพื่อนร่วมงาน/หัวหน้าเป็น NPC, Network ทางธุรกิจช่วยสมัครงาน
- ข้อ 13: อบายมุขทำให้ Performance ตก, สายผิดกฎหมายเชื่อมกับการพนัน/หนี้
- ข้อ 16–17: หน้าการศึกษา/อาชีพ (ใช้ `Quote` ที่มีเหตุผลเป็น Loc key และ ChanceBreakdown)
- ข้อ 19: ปลดล็อกอาชีพ `premium`
- ข้อ 21: ปรับสมดุลเงินเดือน/โอกาส ด้วย Balancing Simulator
