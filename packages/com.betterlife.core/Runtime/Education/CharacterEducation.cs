using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// A character's schooling: the current enrollment, completed credentials and recent entrance exam attempts.
    /// </summary>
    public sealed class CharacterEducation
    {
        private readonly List<Credential> _credentials = new List<Credential>();
        private readonly Dictionary<string, GameDate> _attempts = new Dictionary<string, GameDate>(StringComparer.Ordinal);

        /// <summary>Current enrollment, or null.</summary>
        public Enrollment Current { get; private set; }

        /// <summary>True while studying.</summary>
        public bool IsEnrolled => Current != null;

        /// <summary>Completed programs, oldest first.</summary>
        public IReadOnlyList<Credential> Credentials => _credentials;

        /// <summary>Last entrance exam week per program.</summary>
        public IReadOnlyDictionary<string, GameDate> Attempts => _attempts;

        /// <summary>
        /// Returns the highest completed rank (0 when none).
        /// </summary>
        /// <param name="programs">Program catalog.</param>
        /// <returns>The rank.</returns>
        public int HighestRank(DefinitionCatalog<EducationProgramDefinition> programs)
        {
            if (programs == null)
            {
                throw new ArgumentNullException(nameof(programs));
            }
            return _credentials.Where(c => programs.Contains(c.ProgramId)).Select(c => programs.Get(c.ProgramId).Rank).DefaultIfEmpty(0).Max();
        }

        /// <summary>
        /// Returns the credential with the highest rank (latest among equals), or null.
        /// </summary>
        /// <param name="programs">Program catalog.</param>
        /// <returns>The credential.</returns>
        public Credential Highest(DefinitionCatalog<EducationProgramDefinition> programs)
        {
            if (programs == null)
            {
                throw new ArgumentNullException(nameof(programs));
            }
            return _credentials.Where(c => programs.Contains(c.ProgramId))
                .OrderBy(c => programs.Get(c.ProgramId).Rank).ThenBy(c => c.GraduatedOn).LastOrDefault();
        }

        /// <summary>
        /// Starts studying.
        /// </summary>
        /// <param name="enrollment">The enrollment.</param>
        public void Enroll(Enrollment enrollment)
        {
            if (Current != null)
            {
                throw new InvalidOperationException("Already enrolled.");
            }
            Current = enrollment ?? throw new ArgumentNullException(nameof(enrollment));
        }

        /// <summary>
        /// Ends the current enrollment; graduating adds a credential.
        /// </summary>
        /// <param name="reason">Why it ends.</param>
        /// <param name="week">Week.</param>
        /// <returns>The ended enrollment, or null when not enrolled.</returns>
        public Enrollment End(EducationEndReason reason, GameDate week)
        {
            Enrollment ended = Current;
            if (ended == null)
            {
                return null;
            }
            if (reason == EducationEndReason.Graduated)
            {
                _credentials.Add(new Credential(ended.ProgramId, ended.InstitutionId, week, ended.Performance));
            }
            Current = null;
            return ended;
        }

        /// <summary>
        /// Records an entrance exam attempt.
        /// </summary>
        /// <param name="programId">Program id.</param>
        /// <param name="week">Week.</param>
        public void RecordAttempt(string programId, GameDate week) => _attempts[ContentId.Require(programId, nameof(programId))] = week;

        /// <summary>
        /// Restores saved state.
        /// </summary>
        /// <param name="current">Enrollment, or null.</param>
        /// <param name="credentials">Credentials.</param>
        /// <param name="attempts">Exam attempts.</param>
        public void Restore(Enrollment current, IEnumerable<Credential> credentials, IEnumerable<KeyValuePair<string, GameDate>> attempts)
        {
            Current = current;
            _credentials.Clear();
            _credentials.AddRange(credentials ?? Enumerable.Empty<Credential>());
            _attempts.Clear();
            foreach (KeyValuePair<string, GameDate> attempt in attempts ?? Enumerable.Empty<KeyValuePair<string, GameDate>>())
            {
                _attempts[attempt.Key] = attempt.Value;
            }
        }

        /// <summary>
        /// Removes programs that no longer exist in content; returns one warning per removal.
        /// </summary>
        /// <param name="programExists">True for known programs.</param>
        /// <param name="institutionExists">True for a known institution of a known program.</param>
        /// <returns>Warnings.</returns>
        public IReadOnlyList<string> DropUnknown(Func<string, bool> programExists, Func<string, string, bool> institutionExists)
        {
            if (programExists == null || institutionExists == null)
            {
                throw new ArgumentNullException(nameof(programExists));
            }
            var warnings = new List<string>();
            if (Current != null && (!programExists(Current.ProgramId) || !institutionExists(Current.ProgramId, Current.InstitutionId)))
            {
                warnings.Add($"Unknown program '{Current.ProgramId}' enrollment removed.");
                Current = null;
            }
            foreach (Credential credential in _credentials.Where(c => !programExists(c.ProgramId)).ToList())
            {
                _credentials.Remove(credential);
                warnings.Add($"Unknown program '{credential.ProgramId}' credential removed.");
            }
            foreach (string id in _attempts.Keys.Where(id => !programExists(id)).ToList())
            {
                _attempts.Remove(id);
            }
            return warnings;
        }
    }
}
