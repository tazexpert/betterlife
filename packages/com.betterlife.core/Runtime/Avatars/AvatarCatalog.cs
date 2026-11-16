using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Randomness;

namespace BetterLife.Core.Avatars
{
    /// <summary>
    /// The avatar set: faces (with sex) and age bands. Resolves which picture shows a person at an age.
    /// A chosen face is used when it matches the person's sex; otherwise a face of that sex is derived from a stable key,
    /// so the same person always gets the same face without saving it.
    /// </summary>
    public sealed class AvatarCatalog
    {
        private const string FaceNumberFormat = "00";

        /// <summary>
        /// Creates the catalog.
        /// </summary>
        /// <param name="resourceRoot">Resources folder of the pictures, e.g. "BetterLife/Avatars".</param>
        /// <param name="bands">Age bands; together they must start at age 0.</param>
        /// <param name="faces">Faces; each sex needs at least one.</param>
        public AvatarCatalog(string resourceRoot, IEnumerable<AvatarBand> bands, IEnumerable<AvatarFace> faces)
        {
            if (string.IsNullOrWhiteSpace(resourceRoot))
            {
                throw new ArgumentException("A resource root is required.", nameof(resourceRoot));
            }
            Bands = (bands ?? throw new ArgumentNullException(nameof(bands))).OrderBy(b => b.MinAge).ToList();
            Faces = (faces ?? throw new ArgumentNullException(nameof(faces))).ToList();
            if (Bands.Count == 0 || Bands[0].MinAge != 0)
            {
                throw new ArgumentException("Avatar bands must start at age 0.", nameof(bands));
            }
            if (Faces.Select(f => f.Id).Distinct().Count() != Faces.Count)
            {
                throw new ArgumentException("Face numbers must be unique.", nameof(faces));
            }
            foreach (BiologicalSex sex in new[] { BiologicalSex.Female, BiologicalSex.Male })
            {
                if (!Faces.Any(f => f.Sex == sex))
                {
                    throw new ArgumentException("Every sex needs at least one face.", nameof(faces));
                }
            }
            ResourceRoot = resourceRoot.TrimEnd('/');
        }

        /// <summary>Resources folder of the pictures.</summary>
        public string ResourceRoot { get; }

        /// <summary>Age bands, youngest first.</summary>
        public IReadOnlyList<AvatarBand> Bands { get; }

        /// <summary>All faces.</summary>
        public IReadOnlyList<AvatarFace> Faces { get; }

        /// <summary>
        /// Faces of one sex, in number order.
        /// </summary>
        /// <param name="sex">Sex.</param>
        /// <returns>The faces.</returns>
        public IReadOnlyList<AvatarFace> FacesFor(BiologicalSex sex) => Faces.Where(f => f.Sex == sex).OrderBy(f => f.Id).ToList();

        /// <summary>
        /// The band of an age (the oldest band for ages past the last one).
        /// </summary>
        /// <param name="age">Age in years.</param>
        /// <returns>The band.</returns>
        public AvatarBand BandFor(int age) => Bands.FirstOrDefault(b => b.Covers(Math.Max(0, age))) ?? Bands[Bands.Count - 1];

        /// <summary>
        /// The face of a person: the chosen one if it exists and matches the sex, otherwise one derived from the stable key.
        /// </summary>
        /// <param name="sex">Person's sex.</param>
        /// <param name="chosen">Face the player chose, if any.</param>
        /// <param name="stableKey">Key that identifies the person for good (e.g. character id, or "character:npc").</param>
        /// <returns>The face number.</returns>
        public int FaceFor(BiologicalSex sex, int? chosen, string stableKey)
        {
            if (chosen.HasValue && Faces.Any(f => f.Id == chosen.Value && f.Sex == sex))
            {
                return chosen.Value;
            }
            IReadOnlyList<AvatarFace> faces = FacesFor(sex);
            ulong hash = StableHash.Fnv1a64(stableKey ?? string.Empty);
            return faces[(int)(hash % (ulong)faces.Count)].Id;
        }

        /// <summary>
        /// Resources path (without extension) of a face at an age.
        /// </summary>
        /// <param name="faceId">Face number.</param>
        /// <param name="age">Age in years.</param>
        /// <returns>The path, e.g. "BetterLife/Avatars/age_00_05/age_00_05_03".</returns>
        public string PathFor(int faceId, int age)
        {
            return ResourceRoot + "/" + BandFor(age).FilePrefix + faceId.ToString(FaceNumberFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Stable key of an NPC in someone's life (their face never changes while the life goes on).
        /// </summary>
        /// <param name="owner">Character whose life the NPC belongs to.</param>
        /// <param name="npcId">NPC id.</param>
        /// <returns>The key.</returns>
        public static string NpcKey(Character owner, int npcId)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
            return owner.Id.Value + ":" + npcId.ToString(CultureInfo.InvariantCulture);
        }
    }
}
