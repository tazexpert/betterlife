using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Avatars;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads avatars.json: the Resources root, the age bands with their file prefixes and the faces with their sex.
    /// </summary>
    public sealed class AvatarsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies = new string[0];

        private static readonly IReadOnlyDictionary<string, BiologicalSex> Sexes = new Dictionary<string, BiologicalSex>
        {
            ["female"] = BiologicalSex.Female,
            ["male"] = BiologicalSex.Male,
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Avatars;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "resourceRoot", "bands", "faces", ContentReading.SchemaProperty);
            var (bandArray, bandContext) = RuleJson.RequireArrayProperty(root, "bands", context);
            List<AvatarBand> bands = bandArray.Select((token, i) =>
            {
                RuleReadContext c = bandContext.Index(i);
                JObject band = RuleJson.RequireObject(token, c);
                RuleJson.RequireOnly(band, c, "minAge", "maxAge", "filePrefix");
                return new AvatarBand(RuleJson.RequireInt(band, "minAge", c), ContentReading.RequireNullableInt(band, "maxAge", c), RuleJson.RequireString(band, "filePrefix", c));
            }).ToList();
            var (faceArray, faceContext) = RuleJson.RequireArrayProperty(root, "faces", context);
            List<AvatarFace> faces = faceArray.Select((token, i) =>
            {
                RuleReadContext c = faceContext.Index(i);
                JObject face = RuleJson.RequireObject(token, c);
                RuleJson.RequireOnly(face, c, "id", "sex");
                return new AvatarFace(RuleJson.RequireInt(face, "id", c), ContentReading.RequireEnum(face, "sex", c, Sexes));
            }).ToList();
            content.Add(new AvatarCatalog(RuleJson.RequireString(root, "resourceRoot", context), bands, faces));
        }
    }
}
