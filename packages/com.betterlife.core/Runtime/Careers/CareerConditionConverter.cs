using System;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Reads {"type":"career","employed":true}, {"type":"career","category":"illegal"} or
    /// {"type":"career","track":"office_worker","minLevel":"senior"} ("minLevel" optional). Registered by the careers loader.
    /// </summary>
    public sealed class CareerConditionConverter : ConditionConverter<CareerCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "career";

        private const string EmployedKey = "employed";
        private const string CategoryKey = "category";
        private const string TrackKey = "track";
        private const string MinLevelKey = "minLevel";

        private readonly CareerTrackIndex _index;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="index">Track index (may be filled after registration).</param>
        public CareerConditionConverter(CareerTrackIndex index)
        {
            _index = index ?? throw new ArgumentNullException(nameof(index));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override CareerCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, EmployedKey, CategoryKey, TrackKey, MinLevelKey);
            int keys = (json[EmployedKey] != null ? 1 : 0) + (json[CategoryKey] != null ? 1 : 0) + (json[TrackKey] != null ? 1 : 0);
            if (keys != 1 || (json[MinLevelKey] != null && json[TrackKey] == null))
            {
                throw context.Error($"needs exactly one of '{EmployedKey}', '{CategoryKey}' or '{TrackKey}' ('{MinLevelKey}' only with '{TrackKey}').");
            }
            if (json[EmployedKey] != null)
            {
                return new CareerCondition(_index, CareerConditionMode.Employed, RuleJson.OptionalBool(json, EmployedKey, context).Value, default, null, 0);
            }
            if (json[CategoryKey] != null)
            {
                CareerCategory category = ContentReading.RequireEnum(json, CategoryKey, context, CareerNames.Categories);
                return new CareerCondition(_index, CareerConditionMode.Category, true, category, null, 0);
            }
            string trackId = RuleJson.RequireId(json, TrackKey, context);
            if (!_index.Contains(trackId))
            {
                throw context.Property(TrackKey).Error($"unknown career track '{trackId}'.");
            }
            int level = 0;
            if (json[MinLevelKey] != null)
            {
                string levelId = RuleJson.RequireId(json, MinLevelKey, context);
                level = _index.LevelIndex(trackId, levelId);
                if (level < 0)
                {
                    throw context.Property(MinLevelKey).Error($"track '{trackId}' has no level '{levelId}'.");
                }
            }
            return new CareerCondition(_index, CareerConditionMode.Track, true, default, trackId, level);
        }

        /// <inheritdoc />
        protected override void WriteTyped(CareerCondition condition, JObject json, RuleRegistry registry)
        {
            switch (condition.Mode)
            {
                case CareerConditionMode.Employed:
                    json[EmployedKey] = condition.Employed;
                    break;
                case CareerConditionMode.Category:
                    json[CategoryKey] = CareerNames.NameOf(condition.Category);
                    break;
                default:
                    json[TrackKey] = condition.TrackId;
                    if (condition.MinLevelIndex > 0)
                    {
                        json[MinLevelKey] = _index.LevelIdAt(condition.TrackId, condition.MinLevelIndex);
                    }
                    break;
            }
        }
    }
}
