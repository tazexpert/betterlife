using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Checks schooling: a completed level (or higher), being enrolled, or studying a program.
    /// </summary>
    public sealed class EducationCondition : ICondition
    {
        private readonly DefinitionCatalog<EducationProgramDefinition> _programs;

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="programs">Program catalog.</param>
        /// <param name="mode">What to check.</param>
        /// <param name="programId">Program for Completed and EnrolledIn; null for Enrolled.</param>
        /// <param name="enrolled">Expected enrollment for Enrolled.</param>
        public EducationCondition(DefinitionCatalog<EducationProgramDefinition> programs, EducationConditionMode mode, string programId, bool enrolled)
        {
            _programs = programs ?? throw new ArgumentNullException(nameof(programs));
            if (mode != EducationConditionMode.Enrolled)
            {
                _programs.Get(programId);
            }
            Mode = mode;
            ProgramId = programId;
            Enrolled = enrolled;
        }

        /// <summary>What to check.</summary>
        public EducationConditionMode Mode { get; }

        /// <summary>Program id, or null.</summary>
        public string ProgramId { get; }

        /// <summary>Expected enrollment for Enrolled.</summary>
        public bool Enrolled { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            CharacterEducation education = context.Character.Education;
            switch (Mode)
            {
                case EducationConditionMode.Completed:
                    return education.HighestRank(_programs) >= _programs.Get(ProgramId).Rank;
                case EducationConditionMode.Enrolled:
                    return education.IsEnrolled == Enrolled;
                default:
                    return education.Current?.ProgramId == ProgramId;
            }
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text;
            switch (Mode)
            {
                case EducationConditionMode.Completed:
                    text = RuleText.Of(RuleTextKeys.CondEducationCompleted, (RuleTextKeys.ArgSubjectKey, _programs.Get(ProgramId).NameKey));
                    break;
                case EducationConditionMode.Enrolled:
                    text = RuleText.Of(Enrolled ? RuleTextKeys.CondEducationEnrolled : RuleTextKeys.CondEducationNotEnrolled);
                    break;
                default:
                    text = RuleText.Of(RuleTextKeys.CondEducationEnrolledIn, (RuleTextKeys.ArgSubjectKey, _programs.Get(ProgramId).NameKey));
                    break;
            }
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
