using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// A school or university offering one program.
    /// </summary>
    public sealed class InstitutionDefinition : IDefinition
    {
        /// <summary>
        /// Creates the institution.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="prestige">Prestige 0-100 (helps job applications).</param>
        /// <param name="tuitionPerYear">Real yearly tuition; non-negative.</param>
        /// <param name="difficulty">Entrance exam difficulty 0-100, or null for open admission.</param>
        /// <param name="quality">Performance bonus points while studying here.</param>
        /// <param name="familyMinIncome">Lowest family income level that picks this school automatically, or null for never.</param>
        public InstitutionDefinition(string id, string nameKey, float prestige, decimal tuitionPerYear, float? difficulty, float quality, int? familyMinIncome)
        {
            if (!StatBounds.IsInRange(prestige) || (difficulty.HasValue && !StatBounds.IsInRange(difficulty.Value)))
            {
                throw new ArgumentOutOfRangeException(nameof(prestige), "Prestige and difficulty must be within 0-100.");
            }
            if (tuitionPerYear < 0m || float.IsNaN(quality) || float.IsInfinity(quality))
            {
                throw new ArgumentOutOfRangeException(nameof(tuitionPerYear), "Tuition must be non-negative and quality finite.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Prestige = prestige;
            TuitionPerYear = tuitionPerYear;
            Difficulty = difficulty;
            Quality = quality;
            FamilyMinIncome = familyMinIncome;
        }

        /// <summary>Unique id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Prestige 0-100.</summary>
        public float Prestige { get; }

        /// <summary>Real yearly tuition.</summary>
        public decimal TuitionPerYear { get; }

        /// <summary>Entrance exam difficulty, or null for open admission.</summary>
        public float? Difficulty { get; }

        /// <summary>True when an entrance exam is required.</summary>
        public bool HasExam => Difficulty.HasValue;

        /// <summary>Performance bonus points.</summary>
        public float Quality { get; }

        /// <summary>Lowest family income level that picks this school automatically, or null.</summary>
        public int? FamilyMinIncome { get; }
    }
}
