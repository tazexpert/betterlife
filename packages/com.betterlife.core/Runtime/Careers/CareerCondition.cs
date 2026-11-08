using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Checks the current job: employed or not, its category, or its track and level.
    /// </summary>
    public sealed class CareerCondition : ICondition
    {
        private readonly CareerTrackIndex _index;

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="index">Track index.</param>
        /// <param name="mode">What to check.</param>
        /// <param name="employed">Expected employment for Employed.</param>
        /// <param name="category">Category for Category.</param>
        /// <param name="trackId">Track for Track.</param>
        /// <param name="minLevelIndex">Lowest level index for Track (0 = any).</param>
        public CareerCondition(CareerTrackIndex index, CareerConditionMode mode, bool employed, CareerCategory category, string trackId, int minLevelIndex)
        {
            _index = index ?? throw new ArgumentNullException(nameof(index));
            if (mode == CareerConditionMode.Track && (!index.Contains(trackId) || minLevelIndex < 0))
            {
                throw new ArgumentException($"Unknown track '{trackId}' or level.", nameof(trackId));
            }
            Mode = mode;
            Employed = employed;
            Category = category;
            TrackId = trackId;
            MinLevelIndex = minLevelIndex;
        }

        /// <summary>What to check.</summary>
        public CareerConditionMode Mode { get; }

        /// <summary>Expected employment for Employed.</summary>
        public bool Employed { get; }

        /// <summary>Category for Category.</summary>
        public CareerCategory Category { get; }

        /// <summary>Track for Track.</summary>
        public string TrackId { get; }

        /// <summary>Lowest level index for Track.</summary>
        public int MinLevelIndex { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Job job = context.Character.Career.Current;
            switch (Mode)
            {
                case CareerConditionMode.Employed:
                    return (job != null) == Employed;
                case CareerConditionMode.Category:
                    return job != null && _index.CategoryOf(job.TrackId) == Category;
                default:
                    return job != null && job.TrackId == TrackId && job.LevelIndex >= MinLevelIndex;
            }
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text;
            switch (Mode)
            {
                case CareerConditionMode.Employed:
                    text = RuleText.Of(Employed ? RuleTextKeys.CondEmployed : RuleTextKeys.CondUnemployed);
                    break;
                case CareerConditionMode.Category:
                    text = RuleText.Of(RuleTextKeys.CondCareerCategory, (RuleTextKeys.ArgSubject, CareerNames.NameOf(Category)));
                    break;
                default:
                    text = RuleText.Of(RuleTextKeys.CondCareerTrack, (RuleTextKeys.ArgSubject, TrackId),
                        (RuleTextKeys.ArgValue, RuleText.Number((float)MinLevelIndex)));
                    break;
            }
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
