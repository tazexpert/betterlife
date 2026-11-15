using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using BetterLife.Core.Life;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// The people in the character's life: the starting family, meeting new people, attention (with diminishing returns)
    /// and a steady monthly fade, interactions
    /// (dating, marriage, children...), partners leaving, NPC deaths, child costs, the Relationships and Loneliness stats,
    /// and the professional network that helps job applications.
    /// </summary>
    public sealed class RelationshipService : ICareerNetwork, IDisposable
    {
        /// <summary>Source key of the Relationships stat update.</summary>
        public const string RelationshipsKey = "relationships.score";
        /// <summary>Source key of the loneliness update.</summary>
        public const string LonelinessKey = "relationships.loneliness";
        /// <summary>Source key of grief.</summary>
        public const string GriefKey = "relationships.grief";
        /// <summary>Source key of child-raising costs.</summary>
        public const string ChildCostKey = "relationships.child_cost";
        /// <summary>Memory of a partner who left.</summary>
        public const string BreakupMemoryKey = "memory.broke_up";
        /// <summary>Memory of a spouse who filed for divorce.</summary>
        public const string DivorceMemoryKey = "memory.divorced";
        /// <summary>Interrupt reason of births.</summary>
        public const string ChildInterruptKey = "interrupt.relationship_child";
        /// <summary>Interrupt reason of being left.</summary>
        public const string LeftInterruptKey = "interrupt.relationship_left";
        /// <summary>Interrupt reason of a death.</summary>
        public const string DeathInterruptKey = "interrupt.relationship_death";

        private const float Neutral = 50f;
        private const int WeeksPerMonthApprox = 4;

        private readonly Character _character;
        private readonly RelationshipSettings _settings;
        private readonly DefinitionCatalog<InteractionDefinition> _interactions;
        private readonly NpcFactory _factory;
        private readonly MortalityModel _mortality;
        private readonly LuckCalculator _luck;
        private readonly IRandomProvider _random;
        private readonly IDependencyStatus _dependency;
        private readonly IEventBus _bus;
        private readonly GameClock _clock;
        private readonly IDisposable _subscription;
        private int _socialHours;

        /// <summary>
        /// Creates the service and starts counting social hours.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="settings">Relationship settings.</param>
        /// <param name="interactions">Interaction catalog.</param>
        /// <param name="factory">NPC factory.</param>
        /// <param name="mortality">Mortality model (NPC deaths).</param>
        /// <param name="luck">Chance calculator.</param>
        /// <param name="random">Random stream for relationships.</param>
        /// <param name="dependency">Whether the character still lives with family.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="clock">Game clock (current week for player actions).</param>
        public RelationshipService(Character character, RelationshipSettings settings, DefinitionCatalog<InteractionDefinition> interactions,
            NpcFactory factory, MortalityModel mortality, LuckCalculator luck, IRandomProvider random, IDependencyStatus dependency,
            IEventBus bus, GameClock clock)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _interactions = interactions ?? throw new ArgumentNullException(nameof(interactions));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mortality = mortality ?? throw new ArgumentNullException(nameof(mortality));
            _luck = luck ?? throw new ArgumentNullException(nameof(luck));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _subscription = _bus.Subscribe<TimeAllocationAppliedEvent>(e =>
                _socialHours += e.Hours.TryGetValue(_settings.SocialActivityId, out int hours) ? hours : 0);
        }

        /// <summary>Socialize hours counted so far this month (saved so a reload continues the month).</summary>
        public int PendingSocialHours
        {
            get => _socialHours;
            set => _socialHours = Math.Max(0, value);
        }

        /// <summary>Interaction catalog.</summary>
        public DefinitionCatalog<InteractionDefinition> Interactions => _interactions;

        /// <summary>
        /// Creates the starting family (parents from the family record and random siblings). Does nothing when NPCs exist.
        /// </summary>
        public void InitializeFamily()
        {
            CharacterRelationships people = _character.Relationships;
            if (people.Npcs.Count > 0 || _character.Family == null)
            {
                return;
            }
            GameDate born = _character.Birth.Week;
            FamilyRules family = _settings.Family;
            foreach (var parent in _character.Family.Parents)
            {
                people.Add(id => _factory.FromParent(id, parent), RelationshipType.Parent, family.ParentValue(parent.PersonalityId), born);
            }
            int siblings = family.SiblingWeights.Count == 0 ? 0 : _random.PickWeighted(family.SiblingWeights, w => w.Weight).Count;
            var genomes = _character.Family.Parents.Select(p => p.Genome).ToList();
            var traits = _character.Family.Parents.Select(p => p.TraitIds).ToList();
            for (int i = 0; i < siblings; i++)
            {
                int gapWeeks = _random.NextInt(-family.SiblingAgeGapYears * GameCalendar.WeeksPerYear, family.SiblingAgeGapYears * GameCalendar.WeeksPerYear + 1);
                GameDate siblingBorn = born.AddWeeks(gapWeeks);
                people.Add(id => _factory.Child(id, _character.Birth.CountryId, _character.Name.FamilyName, siblingBorn, genomes, traits),
                    RelationshipType.Sibling, family.SiblingStart, born);
            }
        }

        /// <summary>
        /// Explains how well the character and an NPC fit.
        /// </summary>
        /// <param name="npcId">NPC id.</param>
        /// <returns>The breakdown.</returns>
        public CompatibilityBreakdown Compatibility(int npcId)
        {
            Npc npc = RequireNpc(npcId);
            CompatibilityRules rules = _settings.Compatibility;
            var mine = _character.Traits.Instances.Select(t => t.TraitId).ToList();
            var catalog = _character.Catalogs.Traits;
            int shared = npc.TraitIds.Count(mine.Contains);
            int conflicting = npc.TraitIds.Where(catalog.Contains).Select(t => catalog.Get(t))
                .Count(t => t.ExclusiveGroup != null && mine.Where(catalog.Contains).Any(m => m != t.Id && catalog.Get(m).ExclusiveGroup == t.ExclusiveGroup));
            int gap = Math.Abs(npc.AgeAt(_clock.Now) - _character.AgeAt(_clock.Now));
            return new CompatibilityBreakdown(new[]
            {
                (CompatibilityBreakdown.BaseKey, rules.BaseScore),
                (CompatibilityBreakdown.SharedTraitsKey, shared * rules.SharedTrait),
                (CompatibilityBreakdown.ConflictingTraitsKey, conflicting * rules.ConflictingTrait),
                (CompatibilityBreakdown.AgeGapKey, gap * rules.PerYearAgeGap),
                (CompatibilityBreakdown.LooksKey, (npc.Looks - Neutral) * rules.LooksWeight),
                (CompatibilityBreakdown.CharismaKey, (npc.Charisma - Neutral) * rules.CharismaWeight),
            });
        }

        /// <summary>
        /// Lists every interaction that applies to the NPC's relationship type, with quotes.
        /// </summary>
        /// <param name="npcId">NPC id.</param>
        /// <returns>Interactions and quotes.</returns>
        public IReadOnlyList<(InteractionDefinition Interaction, InteractionQuote Quote)> Options(int npcId)
        {
            Relationship relationship = RequireRelationship(npcId);
            return _interactions.All.Where(i => i.AppliesTo(relationship.Type)).Select(i => (i, Quote(npcId, i.Id))).ToList();
        }

        /// <summary>
        /// Checks whether an interaction is available with an NPC and returns the odds.
        /// </summary>
        /// <param name="npcId">NPC id.</param>
        /// <param name="interactionId">Interaction id.</param>
        /// <returns>The quote.</returns>
        public InteractionQuote Quote(int npcId, string interactionId)
        {
            InteractionDefinition interaction = _interactions.Get(interactionId);
            Relationship relationship = RequireRelationship(npcId);
            Npc npc = RequireNpc(npcId);
            GameDate now = _clock.Now;
            var reasons = new List<string>();
            if (!npc.IsAlive)
            {
                reasons.Add(InteractionQuote.ReasonDead);
            }
            if (!interaction.AppliesTo(relationship.Type))
            {
                reasons.Add(InteractionQuote.ReasonType);
            }
            if (interaction.MinValue.HasValue && relationship.Value < interaction.MinValue.Value)
            {
                reasons.Add(InteractionQuote.ReasonTooLow);
            }
            if (interaction.MaxValue.HasValue && relationship.Value > interaction.MaxValue.Value)
            {
                reasons.Add(InteractionQuote.ReasonTooHigh);
            }
            if (relationship.Cooldowns.TryGetValue(interactionId, out GameDate last) && now.TotalWeeks - last.TotalWeeks < interaction.CooldownWeeks)
            {
                reasons.Add(InteractionQuote.ReasonCooldown);
            }
            if (interaction.Condition != null && !interaction.Condition.Evaluate(new RuleContext(_character, now, _random, interaction.NameKey)))
            {
                reasons.Add(InteractionQuote.ReasonCondition);
            }
            if (_character.Wallet.Cash < interaction.CostCash)
            {
                reasons.Add(InteractionQuote.ReasonCash);
            }
            if (!CanHaveChild(interaction, npc, now))
            {
                reasons.Add(InteractionQuote.ReasonFertility);
            }

            CompatibilityBreakdown compatibility = Compatibility(npcId);
            ChanceBreakdown chance = interaction.Chance == null ? null : _luck.Explain(Spec(interaction, compatibility), _character, now);
            return new InteractionQuote(reasons, chance, compatibility, interaction.CostCash);
        }

        /// <summary>
        /// Does an interaction with an NPC: pays the cost, rolls the chance, and applies the outcome.
        /// </summary>
        /// <param name="npcId">NPC id.</param>
        /// <param name="interactionId">Interaction id.</param>
        /// <returns>The result.</returns>
        public InteractionResult Interact(int npcId, string interactionId)
        {
            InteractionQuote quote = Quote(npcId, interactionId);
            if (!quote.Available)
            {
                return new InteractionResult(quote, false, false, null, 0f, null, null, null);
            }
            InteractionDefinition interaction = _interactions.Get(interactionId);
            Relationship relationship = _character.Relationships.With(npcId);
            Npc npc = _character.Relationships.Npc(npcId);
            GameDate now = _clock.Now;
            if (interaction.CostCash > 0m)
            {
                _character.Wallet.TryWithdraw(interaction.CostCash);
            }
            ChanceRoll roll = interaction.Chance == null ? null : _luck.Roll(Spec(interaction, quote.Compatibility), _character, now, _random);
            bool success = roll == null || roll.Success;
            InteractionOutcome outcome = success ? interaction.Success : interaction.Failure;

            float change = relationship.Change(outcome.Value.Roll(_random));
            relationship.Touch(interactionId, now);
            if (outcome.MemoryKey != null)
            {
                relationship.Remember(new RelationshipMemory(outcome.MemoryKey, now, change));
            }
            if (outcome.Becomes.HasValue)
            {
                ChangeType(relationship, outcome.Becomes.Value, now);
            }
            Npc child = null;
            if (outcome.Child == ChildMode.Birth)
            {
                _character.Relationships.AddPregnancy(new Pregnancy(npcId, now.AddWeeks(_settings.Children.PregnancyWeeks)));
            }
            else if (outcome.Child == ChildMode.Adoption)
            {
                child = AddChild(id => _factory.Adopted(id, _character.Residence.CountryId, _character.Name.FamilyName, now), npc.Id, now);
            }
            EffectResult effects = outcome.Effects.Apply(new RuleContext(_character, now, _random, interaction.NameKey));
            effects.PublishTo(_bus);
            return new InteractionResult(quote, true, success, roll, change, outcome.Becomes, child, effects);
        }

        /// <summary>
        /// Processes one month: births, attention and neglect, partners leaving, meeting people, child costs and stats.
        /// </summary>
        /// <param name="tick">Tick context of the month's first week.</param>
        public void ProcessMonth(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }
            int hours = _socialHours;
            _socialHours = 0;
            if (!_character.IsAlive)
            {
                return;
            }
            GameDate week = tick.Week;
            DeliverBirths(week, tick);
            GiveAttention(hours, week);
            Decay();
            CheckPartners(week, tick);
            Meet(hours, week);
            Prune();
            PayChildCosts(week);
            UpdateStats(week);
        }

        /// <summary>
        /// Processes one year: NPCs may die (grief follows).
        /// </summary>
        /// <param name="tick">Tick context of the year's first week.</param>
        public void ProcessYear(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }
            if (!_character.IsAlive)
            {
                return;
            }
            GameDate week = tick.Week;
            float? healthcare = _character.LocationModifiers().Healthcare;
            foreach (Relationship relationship in _character.Relationships.Living().ToList())
            {
                Npc npc = _character.Relationships.Npc(relationship.NpcId);
                if (!_random.Chance(_mortality.AnnualForAge(npc.AgeAt(week), healthcare)))
                {
                    continue;
                }
                npc.Die(week);
                RelationshipTypeRules rules = _settings.Types[relationship.Type];
                float closeness = Math.Max(0f, relationship.Value) / Relationship.Max;
                Grief(StatIds.Stress, rules.GriefStress * closeness, week);
                Grief(StatIds.Happiness, rules.GriefHappiness * closeness, week);
                _bus.Publish(new NpcDiedEvent(npc, relationship, week));
                Severity severity = RelationshipNames.IsFamily(relationship.Type) || RelationshipNames.IsRomantic(relationship.Type)
                    ? Severity.Major : Severity.Minor;
                tick.RequestInterrupt(severity, DeathInterruptKey, RelationshipNames.NameOf(relationship.Type));
            }
        }

        /// <inheritdoc />
        public float HiringBonus(CareerTrackDefinition track)
        {
            NetworkRules rules = _settings.Network;
            int contacts = _character.Relationships.Living()
                .Count(r => (r.Type == RelationshipType.Colleague || r.Type == RelationshipType.Contact || r.Type == RelationshipType.Friend)
                    && r.Value >= rules.MinValue);
            return Math.Min(rules.MaxPoints, contacts * rules.PointsPerContact);
        }

        /// <summary>
        /// Stops counting social hours.
        /// </summary>
        public void Dispose() => _subscription.Dispose();

        private bool CanHaveChild(InteractionDefinition interaction, Npc npc, GameDate now)
        {
            if (!interaction.InvolvesChild)
            {
                return true;
            }
            ChildRules rules = _settings.Children;
            bool adult = _character.AgeAt(now) >= rules.FertileMinAge;
            if (interaction.Success.Child == ChildMode.Adoption)
            {
                return adult;
            }
            bool fertile(int age) => age >= rules.FertileMinAge && age <= rules.FertileMaxAge;
            return _character.Gender.Sex != npc.Sex && fertile(_character.AgeAt(now)) && fertile(npc.AgeAt(now))
                && _character.Relationships.Pregnancies.Count == 0;
        }

        private static ChanceSpec Spec(InteractionDefinition interaction, CompatibilityBreakdown compatibility)
        {
            return interaction.Chance.WithBonus((compatibility.Score - Neutral) * interaction.CompatibilityPerPoint);
        }

        private void ChangeType(Relationship relationship, RelationshipType type, GameDate week)
        {
            RelationshipType from = relationship.Type;
            relationship.Become(type, week);
            if (from != type)
            {
                _bus.Publish(new RelationshipTypeChangedEvent(relationship, from, type, week));
            }
        }

        private Npc AddChild(Func<int, Npc> create, int otherParentId, GameDate week)
        {
            Relationship relationship = _character.Relationships.Add(create, RelationshipType.Child, _settings.Family.ChildStart, week);
            Npc child = _character.Relationships.Npc(relationship.NpcId);
            _bus.Publish(new ChildBornEvent(child, otherParentId, week));
            return child;
        }

        private void DeliverBirths(GameDate week, TickContext tick)
        {
            foreach (Pregnancy pregnancy in _character.Relationships.Pregnancies.Where(p => p.DueOn <= week).ToList())
            {
                _character.Relationships.RemovePregnancy(pregnancy);
                Npc other = _character.Relationships.Npc(pregnancy.PartnerNpcId);
                var genomes = new List<Genome> { _character.Genome, other.Genome };
                var traits = new List<IReadOnlyList<string>> { _character.Traits.Instances.Select(t => t.TraitId).ToList(), other.TraitIds };
                AddChild(id => _factory.Child(id, _character.Residence.CountryId, _character.Name.FamilyName, week, genomes, traits), other.Id, week);
                tick.RequestInterrupt(Severity.Major, ChildInterruptKey, RelationshipNames.NameOf(RelationshipType.Child));
            }
        }

        private void GiveAttention(int hours, GameDate week)
        {
            AttentionRules rules = _settings.Attention;
            List<Relationship> closest = _character.Relationships.Living()
                .Where(r => !RelationshipNames.IsFamily(r.Type) && r.Type != RelationshipType.Ex)
                .OrderByDescending(r => r.Value).Take(rules.ClosestCount).ToList();
            if (hours > 0 && closest.Count > 0)
            {
                float each = hours * rules.GainPerHour / closest.Count;
                foreach (Relationship relationship in closest)
                {
                    relationship.Change(Diminished(each, relationship));
                    relationship.Touch(null, week);
                }
            }
            if (!_dependency.IsIndependent(week))
            {
                foreach (Relationship relationship in _character.Relationships.Living()
                    .Where(r => r.Type == RelationshipType.Parent || r.Type == RelationshipType.Sibling))
                {
                    relationship.Change(Diminished(rules.FamilyMonthlyGain, relationship));
                    relationship.Touch(null, week);
                }
            }
        }

        // Gains shrink as a relationship gets closer, so steady attention settles where gain equals decay.
        private static float Diminished(float gain, Relationship relationship) => gain * (1f - Math.Max(0f, relationship.Value) / Relationship.Max);

        private void Decay()
        {
            foreach (Relationship relationship in _character.Relationships.Living())
            {
                RelationshipTypeRules rules = _settings.Types[relationship.Type];
                float gap = rules.Floor - relationship.Value;
                relationship.Change(Math.Sign(gap) * Math.Min(Math.Abs(gap), rules.MonthlyDecay));
            }
        }

        private void CheckPartners(GameDate week, TickContext tick)
        {
            RomanceRules rules = _settings.Romance;
            foreach (Relationship relationship in _character.Relationships.Living().Where(r => RelationshipNames.IsRomantic(r.Type)).ToList())
            {
                bool spouse = relationship.Type == RelationshipType.Spouse;
                relationship.CountLowMonth(relationship.Value < (spouse ? rules.DivorceBelow : rules.BreakupBelow));
                if (relationship.LowMonths < (spouse ? rules.DivorceAfterMonths : rules.BreakupAfterMonths))
                {
                    continue;
                }
                relationship.Remember(new RelationshipMemory(spouse ? DivorceMemoryKey : BreakupMemoryKey, week, 0f));
                ChangeType(relationship, RelationshipType.Ex, week);
                tick.RequestInterrupt(Severity.Major, LeftInterruptKey, RelationshipNames.NameOf(spouse ? RelationshipType.Spouse : RelationshipType.Partner));
            }
        }

        private void Meet(int hours, GameDate week)
        {
            MeetingRules rules = _settings.Meeting;
            int age = _character.AgeAt(week);
            if (_random.Chance(Math.Min(1f, rules.MonthlyBase + hours * rules.PerSocialHour)))
            {
                MeetSomeone(RelationshipType.Friend, age, week);
            }
            if (_character.Education.IsEnrolled && _random.Chance(rules.School))
            {
                MeetSomeone(RelationshipType.Friend, age, week);
            }
            if (_character.Career.IsEmployed && _random.Chance(rules.Work))
            {
                MeetSomeone(RelationshipType.Colleague, age, week);
            }
        }

        private void MeetSomeone(RelationshipType type, int age, GameDate week)
        {
            int gap = _settings.Meeting.AgeGapYears;
            int theirAge = Math.Max(0, age + _random.NextInt(-gap, gap + 1));
            GameDate born = week.AddWeeks(-theirAge * GameCalendar.WeeksPerYear - _random.NextInt(0, GameCalendar.WeeksPerYear));
            Relationship relationship = _character.Relationships.Add(id => _factory.Stranger(id, _character.Residence.CountryId, born), type,
                _settings.Meeting.StartValue.Roll(_random), week);
            _bus.Publish(new MetPersonEvent(_character.Relationships.Npc(relationship.NpcId), relationship, week));
        }

        private void Prune()
        {
            var removable = _character.Relationships.All
                .Where(r => !RelationshipNames.IsFamily(r.Type) && !RelationshipNames.IsRomantic(r.Type)
                    && _character.Relationships.Pregnancies.All(p => p.PartnerNpcId != r.NpcId))
                .ToList();
            int excess = removable.Count - _settings.MaxRelationships;
            foreach (Relationship relationship in removable
                .OrderBy(r => _character.Relationships.Npc(r.NpcId).IsAlive ? 1 : 0).ThenBy(r => r.Value).Take(Math.Max(0, excess)))
            {
                _character.Relationships.Forget(relationship.NpcId);
            }
        }

        private void PayChildCosts(GameDate week)
        {
            ChildRules rules = _settings.Children;
            int dependants = _character.Relationships.Living(RelationshipType.Child).Count(r => _character.Relationships.Npc(r.NpcId).AgeAt(week) < rules.CostUntilAge);
            decimal cost = rules.MonthlyCost * dependants;
            if (cost <= 0m)
            {
                return;
            }
            decimal paid = _character.Wallet.Cash > 0m ? _character.Wallet.WithdrawUpTo(cost) : 0m;
            if (paid < cost)
            {
                _bus.Publish(new CashShortfallEvent(cost - paid, ChildCostKey, week));
            }
        }

        private void UpdateStats(GameDate week)
        {
            RelationshipStatRules rules = _settings.Stats;
            List<float> top = _character.Relationships.Living().Where(r => r.Type != RelationshipType.Ex)
                .Select(r => r.Value).OrderByDescending(v => v).Take(rules.TopCount).ToList();
            while (top.Count < rules.TopCount)
            {
                top.Add(rules.EmptySlotValue);
            }
            float score = StatBounds.Clamp(Neutral + top.Average() / 2f);
            Nudge(StatIds.Relationships, score, RelationshipsKey, week);
            int close = _character.Relationships.Living().Count(r => r.Type != RelationshipType.Ex && r.Value >= rules.CloseValue);
            Nudge(StatIds.Loneliness, StatBounds.Clamp(rules.LonelinessBase + close * rules.LonelinessPerCloseTie), LonelinessKey, week);
        }

        private void Nudge(StatId stat, float target, string sourceKey, GameDate week)
        {
            if (!_character.Catalogs.Stats.Contains(stat))
            {
                return;
            }
            float delta = (target - _character.Stats.GetValue(stat)) * _settings.Stats.MonthlySmoothing;
            if (delta != 0f)
            {
                _character.ApplyStatChange(new StatChange(stat, delta, sourceKey), week);
            }
        }

        private void Grief(StatId stat, float amount, GameDate week)
        {
            if (amount != 0f && _character.Catalogs.Stats.Contains(stat))
            {
                _character.ApplyStatChange(new StatChange(stat, amount, GriefKey), week);
            }
        }

        private Npc RequireNpc(int npcId) =>
            _character.Relationships.Npc(npcId) ?? throw new ArgumentException($"No NPC {npcId}.", nameof(npcId));

        private Relationship RequireRelationship(int npcId) =>
            _character.Relationships.With(npcId) ?? throw new ArgumentException($"No relationship with NPC {npcId}.", nameof(npcId));
    }
}
