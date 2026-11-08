using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Rules
{
    /// <summary>Tests for <see cref="RuleRegistry"/> parsing, validation and writing.</summary>
    [TestFixture]
    public class RuleRegistryTests
    {
        private const string AllConditions = @"{'type':'all','conditions':[
            {'type':'stat','stat':'mental.stress','op':'>=','value':70.5},
            {'type':'not','condition':{'type':'flag','flag':'career.fired_once'}},
            {'type':'flag','flag':'vice.gambling_wins','op':'>','value':2},
            {'type':'any','conditions':[{'type':'trait','trait':'anxious','revealed':true},{'type':'age','op':'<','value':25}]},
            {'type':'skill','skill':'cooking','op':'>=','value':10},
            {'type':'year','op':'>=','value':2000},
            {'type':'cash','op':'>=','amount':1500.25},
            {'type':'reputation','domain':'business','op':'<','value':30},
            {'type':'healthCondition','condition':'flu'}]}";

        private const string AllEffects = @"[
            {'type':'stat','stat':'mental.stress','amount':{'min':3,'max':8},'source':'source.work'},
            {'type':'cash','amount':-5000.5},
            {'type':'flag.set','flag':'relationship.recent_breakup','expiryWeeks':26},
            {'type':'flag.clear','flag':'career.fired_once'},
            {'type':'flag.increment','flag':'vice.gambling_wins','amount':2},
            {'type':'trait.add','trait':'anxious'},
            {'type':'trait.remove','trait':'calm'},
            {'type':'trait.reveal','trait':'anxious'},
            {'type':'skill.practice','skill':'cooking','gain':{'min':1,'max':2.5}},
            {'type':'reputation','domain':'business','amount':-3},
            {'type':'healthCondition.add','condition':'flu','severity':35},
            {'type':'healthCondition.remove','condition':'cold'},
            {'type':'if','condition':{'type':'trait','trait':'calm'},'then':[{'type':'stat','stat':'mental.happiness','amount':-2}],
             'else':[{'type':'stat','stat':'mental.happiness','amount':-8}]}]";

        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
        }

        [Test]
        public void EveryBuiltInType_RoundTripsThroughJson()
        {
            JToken conditions = JToken.Parse(AllConditions);
            JToken effects = JToken.Parse(AllEffects);

            JObject writtenCondition = _f.Registry.WriteCondition(_f.Condition(AllConditions));
            JArray writtenEffects = _f.Registry.WriteEffects(_f.Effects(AllEffects));

            Assert.That(JToken.DeepEquals(writtenCondition, conditions), Is.True, writtenCondition.ToString());
            Assert.That(JToken.DeepEquals(writtenEffects, effects), Is.True, writtenEffects.ToString());
        }

        [Test]
        public void ExamplesCoverEveryRegisteredType()
        {
            var usedConditions = new HashSet<string>(JToken.Parse(AllConditions).SelectTokens("$..type").Select(t => (string)t));
            var usedEffects = new HashSet<string>(JToken.Parse(AllEffects).Children().SelectMany(e => new[] { (string)e["type"] }));

            Assert.That(_f.Registry.ConditionTypes.Where(t => !usedConditions.Contains(t)), Is.Empty);
            Assert.That(_f.Registry.EffectTypes.Where(t => !usedEffects.Contains(t)), Is.Empty);
        }

        [TestCase("{'type':'stat','stat':'mental.stres','op':'>=','value':70}", "$.stat", "unknown stat 'mental.stres'")]
        [TestCase("{'type':'magic'}", "$.type", "unknown condition type 'magic'")]
        [TestCase("{'type':'stat','stat':'mental.stress','op':'=>','value':70}", "$.op", "unknown operator")]
        [TestCase("{'type':'stat','stat':'mental.stress','op':'>=','value':'70'}", "$.value", "expected a number")]
        [TestCase("{'type':'stat','stat':'mental.stress','op':'>=','value':170}", "$", "within stat bounds")]
        [TestCase("{'type':'stat','stat':'mental.stress','op':'>=','value':70,'vaule':1}", "$.vaule", "unknown property")]
        [TestCase("{'type':'flag','flag':'career.fired_once','op':'>'}", "$", "must be given together")]
        [TestCase("{'type':'flag','flag':'career.fired'}", "$.flag", "undeclared flag")]
        [TestCase("{'type':'trait','trait':'brave'}", "$.trait", "unknown trait")]
        [TestCase("{'type':'reputation','domain':'politics','op':'<','value':30}", "$.domain", "unknown reputation domain")]
        [TestCase("{'type':'healthCondition','condition':'plague'}", "$.condition", "unknown health condition")]
        [TestCase("{'type':'all','conditions':[]}", "$", "at least one")]
        [TestCase("{'type':'all','conditions':[{'type':'age','op':'<'}]}", "$.conditions[0].value", "is required")]
        [TestCase("[]", "$", "expected an object")]
        public void InvalidConditions_ReportPathAndProblem(string json, string path, string problem)
        {
            var ex = Assert.Throws<RuleParseException>(() => _f.Condition(json));

            Assert.That(ex.Path, Is.EqualTo(path));
            Assert.That(ex.Problem, Does.Contain(problem));
        }

        [TestCase("[{'type':'stat','stat':'health','amount':5}]", "$[0].stat", "derived")]
        [TestCase("[{'type':'flag.increment','flag':'career.fired_once'}]", "$[0].flag", "expected Counter")]
        [TestCase("[{'type':'flag.set','flag':'vice.gambling_wins'}]", "$[0].flag", "expected Bool")]
        [TestCase("[{'type':'stat','stat':'wealth','amount':{'min':5,'max':1}}]", "$[0].amount", "max must be at least min")]
        [TestCase("[{'type':'skill.practice','skill':'cooking','gain':-1}]", "$[0]", "non-negative")]
        [TestCase("[{'type':'flag.set','flag':'career.fired_once','expiryWeeks':0}]", "$[0]", "positive")]
        [TestCase("[{'type':'if','condition':{'type':'age','op':'<','value':5},'then':[{'type':'teleport'}]}]", "$[0].then[0].type", "unknown effect type 'teleport'")]
        [TestCase("{'type':'cash','amount':1}", "$", "expected an array")]
        public void InvalidEffects_ReportPathAndProblem(string json, string path, string problem)
        {
            var ex = Assert.Throws<RuleParseException>(() => _f.Effects(json));

            Assert.That(ex.Path, Is.EqualTo(path));
            Assert.That(ex.Problem, Does.Contain(problem));
        }

        [Test]
        public void NestedPath_IsReportedFromCustomRoot()
        {
            var context = new RuleReadContext(_f.Catalogs, "$.events[3].choices[0].effects");

            var ex = Assert.Throws<RuleParseException>(() => _f.Registry.ParseEffects(JToken.Parse("[{'type':'cash','amount':1},{'type':'stat','stat':'mental.stres','amount':1}]"), context));

            Assert.That(ex.Message, Is.EqualTo("$.events[3].choices[0].effects[1].stat: unknown stat 'mental.stres'."));
        }

        [Test]
        public void Registry_RejectsDuplicateRegistration()
        {
            Assert.Throws<ArgumentException>(() => _f.Registry.Register(new Core.Rules.Serialization.Converters.AgeConditionConverter()));
        }

        [Test]
        public void NewConditionType_PlugsInWithoutChangingTheEngine()
        {
            _f.Registry.Register(new AlwaysConditionConverter());

            ICondition condition = _f.Condition("{'type':'all','conditions':[{'type':'test.always'}]}");

            Assert.That(condition.Evaluate(_f.Context), Is.True);
            Assert.That(_f.Registry.WriteCondition(condition)["conditions"][0]["type"].ToString(), Is.EqualTo("test.always"));
        }

        [Test]
        public void Schema_ListsExactlyTheRegisteredTypes()
        {
            string root = RepositoryPaths.RequireRoot();
            JObject schema = JObject.Parse(File.ReadAllText(Path.Combine(root, "docs", "schemas", "rules.schema.json")));

            Assert.That(SchemaTypes(schema, "condition"), Is.EquivalentTo(_f.Registry.ConditionTypes));
            Assert.That(SchemaTypes(schema, "effect"), Is.EquivalentTo(_f.Registry.EffectTypes));
        }

        private static IEnumerable<string> SchemaTypes(JObject schema, string def)
        {
            return schema["$defs"][def]["oneOf"].Select(option => (string)option["properties"]["type"]["const"]);
        }

        private sealed class AlwaysCondition : ICondition
        {
            public bool Evaluate(RuleContext context) => true;

            public ConditionTrace Explain(RuleContext context) => new ConditionTrace(true, new RuleText("rule.cond.always"), false);
        }

        private sealed class AlwaysConditionConverter : ConditionConverter<AlwaysCondition>
        {
            public override string Type => "test.always";

            protected override AlwaysCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry) => new AlwaysCondition();

            protected override void WriteTyped(AlwaysCondition condition, JObject json, RuleRegistry registry)
            {
            }
        }
    }
}
