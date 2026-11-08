using BetterLife.Core.Content;
using BetterLife.Core.Tests.Astrology;

namespace BetterLife.Core.Tests.Content
{
    /// <summary>
    /// Minimal valid content for every required type, as in-memory files. Tests add or replace files on top.
    /// </summary>
    internal static class TestContent
    {
        public const string Stats = @"{ 'stats': [
            { 'id': 'health', 'layer': 'core', 'polarity': 'higherIsBetter', 'default': 50, 'nameKey': 'stat.health.name', 'descKey': 'stat.health.desc' },
            { 'id': 'health.fitness', 'layer': 'sub', 'parent': 'health', 'polarity': 'higherIsBetter', 'default': 40,
              'nameKey': 'stat.health.fitness.name', 'descKey': 'stat.health.fitness.desc', 'decay': { 'target': 30, 'ratePerWeek': 0.5 } },
            { 'id': 'mental', 'layer': 'core', 'polarity': 'higherIsBetter', 'default': 50, 'nameKey': 'stat.mental.name', 'descKey': 'stat.mental.desc' },
            { 'id': 'mental.stress', 'layer': 'hidden', 'polarity': 'lowerIsBetter', 'default': 20, 'nameKey': 'stat.stress.name', 'descKey': 'stat.stress.desc' },
            { 'id': 'health.fatigue', 'layer': 'sub', 'parent': 'health', 'polarity': 'lowerIsBetter', 'default': 20, 'nameKey': 'stat.fatigue.name', 'descKey': 'stat.fatigue.desc' },
            { 'id': 'intelligence', 'layer': 'core', 'polarity': 'higherIsBetter', 'default': 50, 'nameKey': 'stat.intelligence.name', 'descKey': 'stat.intelligence.desc' },
            { 'id': 'intelligence.iq', 'layer': 'sub', 'parent': 'intelligence', 'polarity': 'higherIsBetter', 'default': 50, 'nameKey': 'stat.iq.name', 'descKey': 'stat.iq.desc' },
            { 'id': 'intelligence.knowledge', 'layer': 'sub', 'parent': 'intelligence', 'polarity': 'higherIsBetter', 'default': 40, 'nameKey': 'stat.knowledge.name', 'descKey': 'stat.knowledge.desc' },
            { 'id': 'wealth', 'layer': 'core', 'polarity': 'higherIsBetter', 'default': 30, 'nameKey': 'stat.wealth.name', 'descKey': 'stat.wealth.desc' },
            { 'id': 'wealth.credit_score', 'layer': 'sub', 'parent': 'wealth', 'polarity': 'higherIsBetter', 'default': 50,
              'nameKey': 'stat.credit.name', 'descKey': 'stat.credit.desc' } ],
          'formulas': [ { 'target': 'health', 'type': 'weighted', 'terms': [ { 'stat': 'health.fitness', 'weight': 1 } ] } ] }";

        public const string Traits = @"{ 'traits': [ { 'id': 'anxious', 'nameKey': 'trait.anxious.name', 'descKey': 'trait.anxious.desc',
            'category': 'personality', 'exclusiveGroup': 'temperament', 'startsHidden': true,
            'statModifiers': [ { 'stat': 'mental.stress', 'direction': 'gain', 'multiplier': 1.25 } ] } ] }";

        public const string Skills = @"{ 'skills': [ { 'id': 'cooking', 'nameKey': 'skill.cooking.name', 'category': 'life',
            'gainFalloffExponent': 1.2, 'decay': { 'graceWeeks': 12, 'ratePerWeek': 0.2, 'retentionOfPeak': 0.6 } } ] }";

        public const string Flags = @"{ 'flags': [
            { 'id': 'career.fired_once', 'kind': 'bool', 'defaultExpiryWeeks': null, 'inheritable': false, 'eulogyKey': 'eulogy.fired' },
            { 'id': 'vice.wins', 'kind': 'counter', 'defaultExpiryWeeks': 52, 'inheritable': true, 'eulogyKey': null },
            { 'id': 'finance.loan_shark', 'kind': 'bool', 'defaultExpiryWeeks': null, 'inheritable': false, 'eulogyKey': null },
            { 'id': 'finance.bankrupt', 'kind': 'bool', 'defaultExpiryWeeks': null, 'inheritable': false, 'eulogyKey': null } ] }";

        public const string Domains = "{ 'domains': [ { 'id': 'business', 'nameKey': 'reputation.business.name' } ] }";

        public const string Conditions = "{ 'conditions': [ { 'id': 'flu', 'nameKey': 'condition.flu.name', 'descKey': 'condition.flu.desc' } ] }";

        public const string Activities = @"{ 'activities': [
            { 'id': 'free_time', 'nameKey': 'activity.free_time.name', 'descKey': 'activity.free_time.desc', 'category': 'rest',
              'minHours': 0, 'maxHours': 168, 'costPerHour': 0, 'stats': [], 'skills': [] },
            { 'id': 'gym', 'nameKey': 'activity.gym.name', 'descKey': 'activity.gym.desc', 'category': 'health',
              'minHours': 0, 'maxHours': 20, 'costPerHour': 15.5, 'condition': { 'type': 'age', 'op': '>=', 'value': 12 },
              'stats': [ { 'stat': 'health.fitness', 'perHour': 0.3, 'maxHours': 10 } ], 'skills': [ { 'skill': 'cooking', 'gainPerHour': 0.1 } ] },
            { 'id': 'school', 'nameKey': 'activity.school.name', 'descKey': 'activity.school.desc', 'category': 'education',
              'minHours': 0, 'maxHours': 60, 'costPerHour': 0, 'condition': { 'type': 'education', 'enrolled': true }, 'stats': [], 'skills': [] },
            { 'id': 'study', 'nameKey': 'activity.study.name', 'descKey': 'activity.study.desc', 'category': 'learning',
              'minHours': 0, 'maxHours': 60, 'costPerHour': 0, 'stats': [], 'skills': [] },
            { 'id': 'work', 'nameKey': 'activity.work.name', 'descKey': 'activity.work.desc', 'category': 'work',
              'minHours': 0, 'maxHours': 84, 'costPerHour': 0, 'condition': { 'type': 'career', 'employed': true }, 'stats': [], 'skills': [] } ] }";

        public const string TimeSettings = @"{ 'defaultActivity': 'free_time',
            'learning': { 'multiplierAtZeroMental': 0.5, 'multiplierAtFullMental': 1.5 },
            'defaultBudget': [ { 'activity': 'gym', 'hours': 5 } ] }";

        public const string Countries = @"{ 'countries': [
            { 'id': 'thailand', 'nameKey': 'country.thailand.name', 'weight': 9,
              'modifiers': { 'costOfLiving': 1, 'education': 60, 'healthcare': 65, 'taxRate': 0.1 },
              'cities': [ { 'id': 'thailand.bangkok', 'nameKey': 'city.bangkok.name', 'weight': 3, 'modifiers': { 'costOfLiving': 1.35 } },
                          { 'id': 'thailand.rural', 'nameKey': 'city.rural.name', 'weight': 1 } ] },
            { 'id': 'japan', 'nameKey': 'country.japan.name', 'weight': 1,
              'modifiers': { 'costOfLiving': 1.8, 'education': 80, 'healthcare': 88, 'taxRate': 0.2 },
              'cities': [ { 'id': 'japan.tokyo', 'nameKey': 'city.tokyo.name', 'weight': 1 } ] } ] }";

        public const string FamilyBackgrounds = @"{ 'backgrounds': [
            { 'id': 'poor', 'nameKey': 'family.poor.name', 'weight': 3, 'householdIncomeLevel': 1, 'statModifiers': [ { 'stat': 'health.fitness', 'amount': -5 } ] },
            { 'id': 'wealthy', 'nameKey': 'family.wealthy.name', 'weight': 1, 'householdIncomeLevel': 5, 'statModifiers': [ { 'stat': 'health.fitness', 'amount': 5 } ] } ] }";

        public const string Personalities = @"{ 'personalities': [
            { 'id': 'strict', 'nameKey': 'parent.strict.name', 'weight': 1, 'statModifiers': [ { 'stat': 'mental.stress', 'amount': 5 } ],
              'traitChances': [ { 'trait': 'anxious', 'chance': 0.5 } ] },
            { 'id': 'warm', 'nameKey': 'parent.warm.name', 'weight': 1, 'statModifiers': [ { 'stat': 'mental.stress', 'amount': -5 } ], 'traitChances': [] } ] }";

        public const string Genetics = @"{ 'population': { 'mean': 50, 'sd': 15 },
            'stats': [ { 'stat': 'health.fitness', 'heritability': 0.5, 'noise': 5 } ],
            'traits': [ { 'trait': 'anxious', 'prevalence': 0.2, 'heritability': 0.5 } ] }";

        public const string Names = @"{ 'countries': [
            { 'country': 'thailand', 'given': { 'female': [ { 'native': 'มาลี', 'latin': 'Malee' }, { 'native': 'สุดา', 'latin': 'Suda' } ],
                                                'male': [ { 'native': 'สมชาย', 'latin': 'Somchai' }, { 'native': 'วิชัย', 'latin': 'Wichai' } ] },
              'family': [ { 'native': 'ใจดี', 'latin': 'Jaidee' }, { 'native': 'ศรีสุข', 'latin': 'Srisuk' } ] },
            { 'country': 'japan', 'given': { 'female': [ { 'native': 'さくら', 'latin': 'Sakura' } ], 'male': [ { 'native': 'はると', 'latin': 'Haruto' } ] },
              'family': [ { 'native': '佐藤', 'latin': 'Sato' } ] } ] }";

        public const string CreationSettings = "{ 'birthYear': { 'min': 1980, 'max': 2020 }, 'parentAgeAtBirth': { 'min': 20, 'max': 40 } }";

        public const string LifeStages = @"{ 'stages': [
            { 'id': 'baby', 'nameKey': 'stage.baby.name', 'minAge': 0, 'playerControlsTime': false, 'extraAnnualMortality': 0, 'birthdayStats': [], 'conditionRisks': [] },
            { 'id': 'child', 'nameKey': 'stage.child.name', 'minAge': 6, 'playerControlsTime': true, 'extraAnnualMortality': 0, 'birthdayStats': [], 'conditionRisks': [] },
            { 'id': 'adult', 'nameKey': 'stage.adult.name', 'minAge': 18, 'playerControlsTime': true, 'extraAnnualMortality': 0,
              'birthdayStats': [ { 'stat': 'health.fitness', 'amount': -1 } ], 'conditionRisks': [ { 'condition': 'flu', 'annualChance': 0.1 } ] } ] }";

        public const string Mortality = @"{ 'gompertz': { 'referenceAge': 30, 'annualAtReference': 0.0008, 'growthPerYear': 0.085 }, 'maxAge': 110,
            'health': { 'multiplierAtZero': 8, 'multiplierAtFull': 0.6 }, 'healthcare': { 'multiplierAtZero': 1.4, 'multiplierAtFull': 0.85 },
            'maxWeeklyProbability': 0.5 }";

        public const string DeathCauses = @"{ 'causes': [
            { 'id': 'natural', 'nameKey': 'death.natural.name', 'weight': { 'base': 1 } },
            { 'id': 'old_age', 'nameKey': 'death.old_age.name', 'condition': { 'type': 'age', 'op': '>=', 'value': 60 },
              'weight': { 'base': 1, 'modifiers': [ { 'condition': { 'type': 'age', 'op': '>=', 'value': 80 }, 'multiplier': 50 } ] } },
            { 'id': 'accident', 'nameKey': 'death.accident.name', 'weight': { 'base': 0 } } ] }";

        public const string Economy = @"{ 'baseYear': 2026, 'historicalInflation': 0.035, 'maxDebtBalance': 1000000000, 'phases': [
            { 'id': 'expansion', 'nameKey': 'economy.expansion.name', 'next': 'recession', 'durationYears': { 'min': 2, 'max': 4 },
              'inflation': { 'min': 0.02, 'max': 0.04 }, 'policyRate': { 'min': 0.02, 'max': 0.03 },
              'markets': { 'stocks': { 'mean': 0.08, 'volatility': 0.15 }, 'property': { 'mean': 0.04, 'volatility': 0.05 }, 'crypto': { 'mean': 0.3, 'volatility': 0.8 } } },
            { 'id': 'recession', 'nameKey': 'economy.recession.name', 'next': 'expansion', 'durationYears': { 'min': 1, 'max': 2 },
              'inflation': 0.01, 'policyRate': 0.01,
              'markets': { 'stocks': { 'mean': -0.15, 'volatility': 0.3 }, 'property': { 'mean': -0.05, 'volatility': 0.08 }, 'crypto': { 'mean': -0.4, 'volatility': 1 } } } ] }";

        public const string Assets = @"{ 'assets': [
            { 'id': 'savings_account', 'nameKey': 'asset.savings_account.name', 'class': 'deposit', 'minPurchase': 100, 'sellFeeRate': 0, 'rateSpread': -0.005 },
            { 'id': 'index_fund', 'nameKey': 'asset.index_fund.name', 'class': 'fund', 'minPurchase': 500, 'sellFeeRate': 0.01 },
            { 'id': 'siam_tech', 'nameKey': 'asset.siam_tech.name', 'class': 'stock', 'minPurchase': 1000, 'sellFeeRate': 0.002, 'beta': 1.4, 'idiosyncraticVolatility': 0.3 },
            { 'id': 'condo_city', 'nameKey': 'asset.condo_city.name', 'class': 'real_estate', 'minPurchase': 1500000, 'sellFeeRate': 0.06, 'rentalYield': 0.04 },
            { 'id': 'city_car', 'nameKey': 'asset.city_car.name', 'class': 'vehicle', 'minPurchase': 400000, 'sellFeeRate': 0.1, 'depreciation': 0.15 },
            { 'id': 'crypto_coin', 'nameKey': 'asset.crypto_coin.name', 'class': 'crypto', 'minPurchase': 100, 'sellFeeRate': 0.005 } ] }";

        public const string DebtProducts = @"{ 'products': [
            { 'id': 'credit_card', 'nameKey': 'debt.credit_card.name', 'kind': 'credit_card', 'annualRate': 0.16, 'rateSpread': null, 'termMonths': 0,
              'minPaymentRate': 0.08, 'minPayment': 300, 'limitIncomeMultiple': 1.5, 'minCreditScore': 40, 'maxIncomeMultiple': null, 'collateralClass': null,
              'downPaymentRate': 0, 'dischargeable': true, 'graceMonths': 0, 'lateFee': 300 },
            { 'id': 'personal_loan', 'nameKey': 'debt.personal_loan.name', 'kind': 'personal', 'annualRate': null, 'rateSpread': 0.1, 'termMonths': 60,
              'minPaymentRate': 0, 'minPayment': 0, 'limitIncomeMultiple': 0, 'minCreditScore': 45, 'maxIncomeMultiple': 1.5, 'collateralClass': null,
              'downPaymentRate': 0, 'dischargeable': true, 'graceMonths': 0, 'lateFee': 500 },
            { 'id': 'mortgage', 'nameKey': 'debt.mortgage.name', 'kind': 'mortgage', 'annualRate': null, 'rateSpread': 0.035, 'termMonths': 360,
              'minPaymentRate': 0, 'minPayment': 0, 'limitIncomeMultiple': 0, 'minCreditScore': 50, 'maxIncomeMultiple': 6, 'collateralClass': 'real_estate',
              'downPaymentRate': 0.1, 'dischargeable': true, 'graceMonths': 0, 'lateFee': 1000 },
            { 'id': 'student_loan', 'nameKey': 'debt.student_loan.name', 'kind': 'student', 'annualRate': 0.01, 'rateSpread': null, 'termMonths': 180,
              'minPaymentRate': 0, 'minPayment': 0, 'limitIncomeMultiple': 0, 'minCreditScore': 0, 'maxIncomeMultiple': null, 'collateralClass': null,
              'downPaymentRate': 0, 'dischargeable': false, 'graceMonths': 24, 'lateFee': 100 },
            { 'id': 'loan_shark', 'nameKey': 'debt.loan_shark.name', 'kind': 'informal', 'annualRate': 2.4, 'rateSpread': null, 'termMonths': 0,
              'minPaymentRate': 0.1, 'minPayment': 500, 'limitIncomeMultiple': 0, 'minCreditScore': 0, 'maxIncomeMultiple': null, 'collateralClass': null,
              'downPaymentRate': 0, 'dischargeable': false, 'graceMonths': 0, 'lateFee': 1000 } ] }";

        public const string LifestyleTiers = @"{ 'tiers': [
            { 'id': 'frugal', 'nameKey': 'lifestyle.frugal.name', 'monthlyCost': 6000, 'monthlyStats': [ { 'stat': 'mental.stress', 'amount': 1 } ] },
            { 'id': 'modest', 'nameKey': 'lifestyle.modest.name', 'monthlyCost': 12000, 'monthlyStats': [] },
            { 'id': 'luxury', 'nameKey': 'lifestyle.luxury.name', 'monthlyCost': 120000, 'monthlyStats': [ { 'stat': 'mental.stress', 'amount': -1 } ] } ] }";

        public const string FinanceSettings = @"{ 'independenceAge': 18, 'allowanceStartAge': 7, 'defaultLifestyle': 'modest', 'referenceLifestyle': 'modest',
            'familySupport': [ { 'incomeLevel': 1, 'monthlyAllowance': 500, 'independenceGift': 0 },
                               { 'incomeLevel': 5, 'monthlyAllowance': 8000, 'independenceGift': 1000000 } ],
            'fallbackProduct': 'loan_shark', 'defaultAfterMissed': 3, 'forcedSaleDiscount': 0.3,
            'creditScore': { 'onTimeGain': 0.2, 'missedLoss': 5, 'defaultLoss': 15, 'foreclosureLoss': 20, 'bankruptcyScore': 5, 'highUtilization': 0.7, 'utilizationLoss': 0.5 },
            'bankruptcy': { 'netWorthBelow': -500000, 'flagWeeks': 520 },
            'debtStress': { 'perYearOfCosts': 2, 'max': 6, 'informalExtra': 2 },
            'wealthCurve': [ { 'months': -24, 'score': 5 }, { 'months': 0, 'score': 30 }, { 'months': 6, 'score': 45 }, { 'months': 24, 'score': 60 },
                             { 'months': 120, 'score': 80 }, { 'months': 600, 'score': 95 } ] }";

        public const string EducationPrograms = @"{ 'programs': [
            { 'id': 'primary', 'nameKey': 'education.primary.name', 'rank': 2, 'years': 6, 'entryAge': { 'min': 6, 'max': 8 }, 'requiresRank': 0,
              'compulsory': true, 'autoEnroll': true, 'weeklyHours': 30, 'skills': [ { 'skill': 'cooking', 'gainPerHour': 0.01 } ],
              'institutions': [ { 'id': 'primary.public', 'nameKey': 'institution.primary.public.name', 'prestige': 40, 'tuitionPerYear': 0, 'difficulty': null, 'quality': 0, 'familyMinIncome': 0 },
                                { 'id': 'primary.private', 'nameKey': 'institution.primary.private.name', 'prestige': 60, 'tuitionPerYear': 60000, 'difficulty': null, 'quality': 4, 'familyMinIncome': 5 } ] },
            { 'id': 'secondary', 'nameKey': 'education.secondary.name', 'rank': 4, 'years': 6, 'entryAge': { 'min': 11, 'max': 15 }, 'requiresRank': 2,
              'compulsory': false, 'autoEnroll': true, 'weeklyHours': 32, 'skills': [],
              'institutions': [ { 'id': 'secondary.public', 'nameKey': 'institution.secondary.public.name', 'prestige': 40, 'tuitionPerYear': 0, 'difficulty': null, 'quality': 0, 'familyMinIncome': 0 } ] },
            { 'id': 'bachelor', 'nameKey': 'education.bachelor.name', 'rank': 6, 'years': 4, 'entryAge': { 'min': 17, 'max': null }, 'requiresRank': 4,
              'compulsory': false, 'autoEnroll': false, 'weeklyHours': 25, 'skills': [],
              'institutions': [ { 'id': 'bachelor.top', 'nameKey': 'institution.bachelor.top.name', 'prestige': 90, 'tuitionPerYear': 30000, 'difficulty': 75, 'quality': 5, 'familyMinIncome': null },
                                { 'id': 'bachelor.open', 'nameKey': 'institution.bachelor.open.name', 'prestige': 30, 'tuitionPerYear': 10000, 'difficulty': null, 'quality': -3, 'familyMinIncome': null } ] } ] }";

        public const string EducationSettings = @"{ 'schoolActivity': 'school', 'studyActivity': 'study', 'startPerformance': 60,
            'performance': { 'weeklySmoothing': 0.08, 'attendanceWeight': 0.45, 'iqWeight': 0.2, 'knowledgeWeight': 0.2, 'mentalWeight': 0.15,
                             'studyBonusPerHour': 0.5, 'maxStudyBonus': 10 },
            'yearEnd': { 'passPerformance': 40, 'maxRepeats': 2 },
            'admission': { 'chance': { 'base': 50, 'fortune': 'study' }, 'perPointAboveDifficulty': 1.5, 'performanceWeight': 0.6, 'knowledgeWeight': 0.4, 'retryWeeks': 52 },
            'scholarship': { 'minScore': 85, 'chance': { 'base': 40, 'fortune': 'study' }, 'keepPerformance': 75 },
            'studentLoanProduct': 'student_loan' }";

        public const string Careers = @"{ 'tracks': [
            { 'id': 'clerk', 'nameKey': 'career.clerk.name', 'category': 'corporate', 'premium': false,
              'requirement': { 'type': 'education', 'completed': 'bachelor' }, 'hiring': { 'base': 50, 'fortune': 'career' },
              'weeklyHours': 40, 'performanceSkill': null, 'skills': [ { 'skill': 'cooking', 'gainPerHour': 0.01 } ], 'stressPerHour': 0.05,
              'incomeVolatility': 0, 'marketBeta': 0, 'layoffSensitivity': 1, 'pensionRate': 0.5, 'risk': null,
              'levels': [ { 'id': 'staff', 'titleKey': 'career.clerk.staff', 'monthlySalary': 20000, 'promotionAfterMonths': 12, 'promotionPerformance': 60 },
                          { 'id': 'manager', 'titleKey': 'career.clerk.manager', 'monthlySalary': 40000, 'promotionAfterMonths': null, 'promotionPerformance': 0 } ] },
            { 'id': 'courier', 'nameKey': 'career.courier.name', 'category': 'illegal', 'premium': false,
              'requirement': null, 'hiring': { 'base': 80, 'karma': false }, 'weeklyHours': 30, 'performanceSkill': 'cooking', 'skills': [], 'stressPerHour': 0.1,
              'incomeVolatility': 0.4, 'marketBeta': 0, 'layoffSensitivity': 0, 'pensionRate': 0,
              'risk': { 'monthlyChance': 0.05, 'endsJob': true, 'reasonKey': 'career.risk.arrested', 'effects': [ { 'type': 'cash', 'amount': -5000 } ] },
              'levels': [ { 'id': 'runner', 'titleKey': 'career.courier.runner', 'monthlySalary': 30000, 'promotionAfterMonths': null, 'promotionPerformance': 0 } ] } ] }";

        public const string CareerSettings = @"{ 'workActivity': 'work', 'minWorkAge': 15, 'retirementAge': 60,
            'performance': { 'weeklySmoothing': 0.06, 'attendanceWeight': 0.5, 'skillWeight': 0.2, 'mentalWeight': 0.2, 'restWeight': 0.1, 'start': 55 },
            'boss': { 'weeklySmoothing': 0.03, 'start': 55, 'fireBelow': 20, 'fireAfterMonths': 3, 'stressFromDissatisfaction': 1 },
            'hiring': { 'prestigePerPoint': 0.2, 'retryWeeks': 12 },
            'promotion': { 'chance': { 'base': 40, 'fortune': 'career' }, 'perPointAbove': 1.5, 'minBoss': 50, 'reviewEveryMonths': 12 },
            'layoffs': [ { 'phase': 'recession', 'monthlyChance': 0.01 } ] }";

        public const string EventSettingsTemplate = "{ 'weeklyEventChance': CHANCE, 'responseWindowWeeks': 4, "
            + "'antiFrustration': { 'streakThreshold': 2, 'windowWeeks': 12, 'negativeWeightMultiplier': 0.4 } }";

        public static string EventSettings(float weeklyChance = 0.15f)
        {
            return EventSettingsTemplate.Replace("CHANCE", weeklyChance.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public static InMemoryContentSource Source(string extraFiles = null, float weeklyEventChance = 0.15f)
        {
            string files = "{ 'type': 'stats', 'path': 'stats.json' }, { 'type': 'traits', 'path': 'traits.json' }, "
                + "{ 'type': 'skills', 'path': 'skills.json' }, { 'type': 'flags', 'path': 'flags.json' }, "
                + "{ 'type': 'reputation_domains', 'path': 'reputation_domains.json' }, { 'type': 'health_conditions', 'path': 'health_conditions.json' }, "
                + "{ 'type': 'activities', 'path': 'activities.json' }, { 'type': 'time_settings', 'path': 'time_settings.json' }, "
                + "{ 'type': 'astrology', 'path': 'astrology.json' }, { 'type': 'luck_settings', 'path': 'luck.json' }, "
                + "{ 'type': 'event_settings', 'path': 'event_settings.json' }, "
                + "{ 'type': 'countries', 'path': 'countries.json' }, { 'type': 'family_backgrounds', 'path': 'family_backgrounds.json' }, "
                + "{ 'type': 'parent_personalities', 'path': 'parent_personalities.json' }, { 'type': 'genetics', 'path': 'genetics.json' }, "
                + "{ 'type': 'names', 'path': 'names.json' }, { 'type': 'creation_settings', 'path': 'creation_settings.json' }, "
                + "{ 'type': 'life_stages', 'path': 'life_stages.json' }, { 'type': 'mortality', 'path': 'mortality.json' }, "
                + "{ 'type': 'death_causes', 'path': 'death_causes.json' }, "
                + "{ 'type': 'economy', 'path': 'economy.json' }, { 'type': 'assets', 'path': 'assets.json' }, "
                + "{ 'type': 'debt_products', 'path': 'debts.json' }, { 'type': 'lifestyle_tiers', 'path': 'lifestyle_tiers.json' }, "
                + "{ 'type': 'finance_settings', 'path': 'finance_settings.json' }, "
                + "{ 'type': 'education_programs', 'path': 'education_programs.json' }, { 'type': 'education_settings', 'path': 'education_settings.json' }, "
                + "{ 'type': 'careers', 'path': 'careers.json' }, { 'type': 'career_settings', 'path': 'career_settings.json' }"
                + (extraFiles == null ? string.Empty : ", " + extraFiles);
            return new InMemoryContentSource()
                .Add(ContentPaths.ManifestFile, "{ '$schema': '../docs/schemas/manifest.schema.json', 'contentVersion': 3, 'files': [ " + files + " ] }")
                .Add("stats.json", Stats)
                .Add("traits.json", Traits)
                .Add("skills.json", Skills)
                .Add("flags.json", Flags)
                .Add("reputation_domains.json", Domains)
                .Add("health_conditions.json", Conditions)
                .Add("activities.json", Activities)
                .Add("time_settings.json", TimeSettings)
                .Add("astrology.json", TestAstrology.Json())
                .Add("luck.json", TestAstrology.LuckJson)
                .Add("event_settings.json", EventSettings(weeklyEventChance))
                .Add("countries.json", Countries)
                .Add("family_backgrounds.json", FamilyBackgrounds)
                .Add("parent_personalities.json", Personalities)
                .Add("genetics.json", Genetics)
                .Add("names.json", Names)
                .Add("creation_settings.json", CreationSettings)
                .Add("life_stages.json", LifeStages)
                .Add("mortality.json", Mortality)
                .Add("death_causes.json", DeathCauses)
                .Add("economy.json", Economy)
                .Add("assets.json", Assets)
                .Add("debts.json", DebtProducts)
                .Add("lifestyle_tiers.json", LifestyleTiers)
                .Add("finance_settings.json", FinanceSettings)
                .Add("education_programs.json", EducationPrograms)
                .Add("education_settings.json", EducationSettings)
                .Add("careers.json", Careers)
                .Add("career_settings.json", CareerSettings);
        }

        public static IContentRepository Load(InMemoryContentSource source)
        {
            return DefaultContent.CreatePipeline(source).Load().RequireRepository();
        }
    }
}
