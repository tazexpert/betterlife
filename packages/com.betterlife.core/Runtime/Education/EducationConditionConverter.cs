using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Reads {"type":"education","completed":"bachelor"}, {"type":"education","enrolled":true} or
    /// {"type":"education","enrolledIn":"bachelor"} (exactly one key). Registered by the education programs loader.
    /// </summary>
    public sealed class EducationConditionConverter : ConditionConverter<EducationCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "education";

        private const string CompletedKey = "completed";
        private const string EnrolledKey = "enrolled";
        private const string EnrolledInKey = "enrolledIn";

        private readonly DefinitionCatalog<EducationProgramDefinition> _programs;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="programs">Program catalog.</param>
        public EducationConditionConverter(DefinitionCatalog<EducationProgramDefinition> programs)
        {
            _programs = programs ?? throw new ArgumentNullException(nameof(programs));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override EducationCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, CompletedKey, EnrolledKey, EnrolledInKey);
            int keys = (json[CompletedKey] != null ? 1 : 0) + (json[EnrolledKey] != null ? 1 : 0) + (json[EnrolledInKey] != null ? 1 : 0);
            if (keys != 1)
            {
                throw context.Error($"needs exactly one of '{CompletedKey}', '{EnrolledKey}' or '{EnrolledInKey}'.");
            }
            if (json[EnrolledKey] != null)
            {
                return new EducationCondition(_programs, EducationConditionMode.Enrolled, null, RuleJson.OptionalBool(json, EnrolledKey, context).Value);
            }
            string key = json[CompletedKey] != null ? CompletedKey : EnrolledInKey;
            string id = RuleJson.RequireId(json, key, context);
            if (!_programs.Contains(id))
            {
                throw context.Property(key).Error($"unknown education program '{id}'.");
            }
            return new EducationCondition(_programs, key == CompletedKey ? EducationConditionMode.Completed : EducationConditionMode.EnrolledIn, id, true);
        }

        /// <inheritdoc />
        protected override void WriteTyped(EducationCondition condition, JObject json, RuleRegistry registry)
        {
            switch (condition.Mode)
            {
                case EducationConditionMode.Completed:
                    json[CompletedKey] = condition.ProgramId;
                    break;
                case EducationConditionMode.Enrolled:
                    json[EnrolledKey] = condition.Enrolled;
                    break;
                default:
                    json[EnrolledInKey] = condition.ProgramId;
                    break;
            }
        }
    }
}
