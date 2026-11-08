# Task 10: Economy + Finance

> สถานะ: ✅ **Implemented** — 2026-11-08 (ใช้ตัวเลือกที่แนะนำตามที่ผู้ใช้อนุญาต)

## 1. สรุปความเข้าใจ
- **เศรษฐกิจมหภาค:** วัฏจักร 4 ช่วง (ขยายตัว → จุดสูงสุด → ถดถอย → ฟื้นตัว), เงินเฟ้อ, ดอกเบี้ยนโยบาย, ผลตอบแทนตลาดรายเดือน
- **รายรับ–รายจ่ายรายเดือน:** เงินช่วยจากครอบครัว (ตอนยังพึ่งพ่อแม่), แหล่งรายได้อื่น (อาชีพในข้อ 11), ภาษีตามประเทศ, ค่าครองชีพตาม Lifestyle Tier × ค่าครองชีพของเมือง
- **สินทรัพย์:** เงินฝาก, พันธบัตร, กองทุน, หุ้นบริษัทสมมติ, อสังหาฯ (มีค่าเช่า), รถ (เสื่อมราคา), คริปโต
- **หนี้:** บัตรเครดิต, สินเชื่อส่วนบุคคล, บ้าน, รถ, กยศ., นอกระบบ
  รวมถึง Credit score, การผิดนัด, การยึดทรัพย์, การล้มละลาย และหนี้ทำให้เครียด (GDD: หนี้สูง → Stress สูง)
- **Wealth (Core)** คำนวณจากทรัพย์สินสุทธิ เทียบกับจำนวนเดือนของค่าครองชีพที่ครอบคลุมได้

## 2. การตัดสินใจ (ใช้ตัวเลือกที่แนะนำ)
| หัวข้อ | การตัดสินใจ | เหตุผล |
|---|---|---|
| **หน่วยเงิน** | ทุกจำนวนเงินในเกมและใน content เป็น **บาทมูลค่าจริงของปีฐาน 2026** ส่วนราคาตามยุคคำนวณจาก `PriceLevel` เพื่อแสดงผลเท่านั้น | content และ Rule ทุกตัวใช้ตัวเลขเดียวกันได้ทุกยุค (ทำกระเป๋าหาย 500 บาทในปี 1960 จึงไม่ทำให้หมดตัว) |
| เงินเฟ้อ | กัดกร่อนเงินสดและหนี้ (เงินต้นคงที่ตามราคาตลาด) ทุกเดือน ส่วนเงินฝาก/พันธบัตร = ดอกเบี้ยตามราคาตลาด − เงินเฟ้อ และหุ้น/อสังหาฯ/คริปโตใช้ผลตอบแทนที่หักเงินเฟ้อแล้ว | สมจริง: เงินเฟ้อทำให้คนเป็นหนี้ได้เปรียบ และทำให้คนถือเงินสดเสียเปรียบ |
| PriceLevel ตอนเริ่มชีวิต | (1 + 3.5%)^(ปีเกิด − 2026) แล้วเพิ่มตามเงินเฟ้อจริงทุกปี | ปี 1950 ราคาเหลือประมาณ 7% ของปัจจุบัน |
| วัฏจักร | ช่วงละ 1–6 ปีตาม content แต่ละช่วงมีช่วงเงินเฟ้อ / ดอกเบี้ย / ผลตอบแทนตลาด (mean, volatility) ของตัวเอง ช่วงเริ่มต้นสุ่ม | |
| ราคาตลาด | ดัชนีหุ้น / อสังหาฯ / คริปโต สุ่มรายเดือนด้วย normal(mean/12, vol/√12) ส่วนหุ้นรายตัว = ดัชนี × beta + noise ของตัวเอง | กองทุนทุกตัวขึ้นลงพร้อมตลาด แต่หุ้นแต่ละตัวต่างกัน |
| พึ่งพ่อแม่ | อายุ < 18 = พึ่งพ่อแม่: ไม่มีค่าครองชีพ, ได้เงินช่วยตามฐานะครอบครัว (ตั้งแต่ 7 ขวบ), ถ้าจ่ายไม่พอครอบครัวช่วยจ่าย (ไม่เป็นหนี้) และ Wealth ใช้ค่าจากฐานะครอบครัว | |
| อายุ 18 | ได้เงินก้อนตั้งตัวตามฐานะครอบครัว เริ่มจ่ายค่าครองชีพ (Lifestyle เริ่มต้น `modest`) และ Wealth เริ่มคำนวณจากทรัพย์สินสุทธิ | |
| จ่ายไม่พอ (`CashShortfallEvent`) | ใช้วงเงินบัตรเครดิตที่ยังเหลือก่อน ถ้าไม่มีจะกลายเป็น **หนี้นอกระบบ** อัตโนมัติ (Debt spiral ตาม GDD) และตั้ง Flag `finance.loan_shark` | |
| ผิดนัด | จ่ายงวดไม่ได้ = ผิดนัด (Credit −5, ค่าปรับ) ถ้าผิดนัด 3 งวดติดกันถือว่า default: หนี้มีหลักประกันจะถูกยึดทรัพย์ (ขายลด 30%) ส่วนหนี้ไม่มีหลักประกันจะถูกส่งติดตามหนี้ | |
| ล้มละลาย | มีหนี้ไม่มีหลักประกันที่ default + ทรัพย์สินสุทธิ < −500,000 → ขายทรัพย์สินทั้งหมดใช้หนี้ ยกหนี้ในระบบที่เหลือ (ยกเว้น กยศ. และหนี้นอกระบบ) Credit = 5 และตั้ง Flag `finance.bankrupt` 10 ปี (กู้ในระบบไม่ได้) | |
| เพดานหนี้ | ยอดหนี้ต่อสัญญาไม่เกิน 1,000 ล้านบาท | กันไม่ให้ decimal ล้นเมื่อหนี้นอกระบบทบต้นหลายสิบปี |
| Credit score | จ่ายตรงเวลา +0.2/เดือน, ผิดนัด −5, default −15, ถูกยึดทรัพย์ −20, ใช้วงเงินบัตร > 70% −0.5/เดือน (ทุกครั้งมีที่มาใน Breakdown) | |
| อนุมัติสินเชื่อ | ต้องอายุ ≥ 18, ไม่ล้มละลาย, Credit ≥ เกณฑ์ของสินเชื่อ, วงเงิน ≤ รายได้ต่อปี × ตัวคูณ (กยศ. ไม่ดูรายได้) ถ้าไม่อนุมัติจะได้รายการเหตุผลเป็น Loc key | อ่านตัวเลขได้ทุกตัว |
| Wealth | กราฟจาก content: จำนวนเดือนที่ครอบคลุมค่าครองชีพระดับ `modest` เช่น −24 เดือน → 5, 0 → 30, 6 → 45, 24 → 60, 120 → 80, 600 → 95 ระบบตั้งค่าให้ทุกต้นเดือน (มีที่มา `finance.net_worth`) | สูตรอยู่ใน content ปรับได้ |
| ที่เก็บ | `CharacterFinances` (พอร์ต + หนี้ + Lifestyle) อยู่ใน Character เพื่อให้ Rule อ่านได้ ส่วน `EconomyState` (มหภาค) อยู่ใน LifeSession | |
| Phase | `economy_year` (รายปี ลำดับ 60) และ `finance_month` (รายเดือน ลำดับ 250) | |
| รายได้จากงาน | ยังไม่มีจนถึงข้อ 11 (ตอนนี้ผู้ใหญ่จะติดหนี้หลังเงินตั้งตัวหมด ซึ่งเป็นสภาพชั่วคราวที่ตั้งใจไว้) จุดเสียบคือ `IIncomeSource` | |

## 3. JSON Schema (content ใหม่ 5 ชนิด)
```jsonc
// economy.json (ไฟล์เดียว)
{ "baseYear": 2026, "historicalInflation": 0.035, "maxDebtBalance": 1000000000,
  "phases": [ { "id": "expansion", "nameKey": "economy.expansion.name", "next": "peak",
      "durationYears": { "min": 3, "max": 6 }, "inflation": { "min": 0.02, "max": 0.04 }, "policyRate": { "min": 0.02, "max": 0.035 },
      "markets": { "stocks": { "mean": 0.08, "volatility": 0.15 }, "property": { "mean": 0.04, "volatility": 0.05 },
                   "crypto": { "mean": 0.3, "volatility": 0.8 } } } ] }

// assets.json
{ "assets": [ { "id": "savings_account", "nameKey": "asset.savings_account.name", "class": "deposit",
    "minPurchase": 100, "sellFeeRate": 0, "rateSpread": -0.005 },
  { "id": "siam_tech", "class": "stock", "minPurchase": 1000, "sellFeeRate": 0.002, "beta": 1.4, "idiosyncraticVolatility": 0.3 },
  { "id": "condo_bangkok", "class": "real_estate", "minPurchase": 1500000, "sellFeeRate": 0.06, "rentalYield": 0.04 } ] }

// debts.json
{ "products": [ { "id": "credit_card", "nameKey": "debt.credit_card.name", "kind": "credit_card",
    "annualRate": 0.16, "rateSpread": null, "termMonths": 0, "minPaymentRate": 0.08, "minPayment": 300,
    "limitIncomeMultiple": 1.5, "minCreditScore": 40, "maxIncomeMultiple": null, "collateralClass": null,
    "downPaymentRate": 0, "dischargeable": true, "graceMonths": 0, "lateFee": 300 } ] }

// lifestyle_tiers.json
{ "tiers": [ { "id": "modest", "nameKey": "lifestyle.modest.name", "monthlyCost": 12000, "monthlyStats": [] } ] }

// finance_settings.json (ไฟล์เดียว)
{ "independenceAge": 18, "allowanceStartAge": 7, "defaultLifestyle": "modest", "referenceLifestyle": "modest",
  "familySupport": [ { "incomeLevel": 3, "monthlyAllowance": 2000, "independenceGift": 50000 } ],
  "shortfall": { "revolvingFirst": true, "fallbackProduct": "loan_shark" },
  "defaultAfterMissed": 3, "forcedSaleDiscount": 0.3,
  "creditScore": { "onTimeGain": 0.2, "missedLoss": 5, "defaultLoss": 15, "foreclosureLoss": 20, "bankruptcyScore": 5,
                   "highUtilization": 0.7, "utilizationLoss": 0.5 },
  "bankruptcy": { "netWorthBelow": -500000, "flagWeeks": 520 },
  "debtStress": { "perMonthAtOneYearOfCosts": 2, "max": 6, "informalExtra": 2 },
  "wealthCurve": [ { "months": -24, "score": 5 }, { "months": 0, "score": 30 } ] }
```
**Rule ใหม่:** Condition `{ "type": "netWorth", "op": "<", "amount": 0 }` และ `{ "type": "totalDebt", "op": ">", "amount": 100000, "kind": "informal" }`

## 4. C# Domain (`Runtime/Economy/`)
| กลุ่ม | Class |
|---|---|
| มหภาค | `EconomyPhaseDefinition`, `MarketParameters`, `EconomySettings`, `EconomyState`, `MarketMonth`, `EconomyService`, `EconomyYearPhase` |
| สินทรัพย์ | `AssetClass`, `AssetDefinition`, `Holding`, `Portfolio` |
| หนี้ | `DebtKind`, `DebtProductDefinition`, `Debt`, `DebtBook`, `LoanQuote` |
| ค่าตั้ง | `LifestyleTierDefinition`, `FamilySupportLevel`, `CreditScoreRules`, `WealthCurve`, `FinanceSettings` |
| บุคคล | `CharacterFinances` (ใน Character), `FinanceSnapshot` |
| บริการ | `IIncomeSource`, `FamilyAllowanceSource`, `FinanceService` (ซื้อ/ขาย/กู้/เปิดบัตร/จ่ายหนี้/ตั้ง Lifestyle/ประมวลผลรายเดือน/จัดการยอดขาด), `FinanceMonthPhase`, `FinanceLine`, `FinanceMonthReport` |
| Events | `FinanceMonthReportEvent`, `DebtDefaultedEvent`, `ForeclosureEvent`, `BankruptcyEvent` |
| Rules | `NetWorthCondition`, `TotalDebtCondition` |

## 5. Save DTO
- `CharacterDto.Finances = { LifestyleTierId, BankruptUntilWeek?, Holdings[{Id, AssetId, Value, CostBasis, AcquiredWeek}], Debts[{Id, ProductId, Balance, AnnualRate, MonthlyPayment, MonthsRemaining, CreditLimit, CollateralHoldingId, MissedPayments, Defaulted, StartedWeek}], NextHoldingId, NextDebtId }`
- `EconomyStateDto = { PhaseId, YearsLeftInPhase, AnnualInflation, PolicyRate, PriceLevel }` (ข้อ 14 รวมไว้ในเซฟของ LifeSession)

## 6. ส่งต่อ
- ข้อ 11: `IIncomeSource` ของงาน/ฟรีแลนซ์/ธุรกิจ + ความเสี่ยงตกงานตามช่วงเศรษฐกิจ + สินทรัพย์ประเภทธุรกิจ
- ข้อ 13: Effect กู้นอกระบบเพื่อเล่นพนัน, เหตุการณ์ทวงหนี้ (ใช้ Flag `finance.loan_shark`)
- ข้อ 14: เซฟ `EconomyState`
- ข้อ 17: หน้า Finance (ใช้ `FinanceSnapshot` + `FinanceMonthReport` + `PriceLevel` แปลงเป็นราคาตามยุค)
- ข้อ 18: มรดก/ภาษีมรดก/หนี้ตกทอด (ใช้ `CharacterFinances`)
