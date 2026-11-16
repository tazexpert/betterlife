using System.IO;
using System.Linq;
using BetterLife.Core.Avatars;
using BetterLife.Core.Content;
using BetterLife.Core.Domain.Characters;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Real content and the user's pictures: every face exists in every age band under the Unity Resources folder.
    /// </summary>
    [TestFixture]
    public class AvatarFilesTests
    {
        private static string ResourcesDir => Path.Combine(RepoContent.Root, "unity", "Assets", "BetterLife", "Presentation", "Resources");

        [Test]
        public void EveryFace_HasAPicture_InEveryAgeBand()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            AvatarCatalog avatars = content.Get<AvatarCatalog>();
            Assert.That(avatars.FacesFor(BiologicalSex.Male), Has.Count.EqualTo(16));
            Assert.That(avatars.FacesFor(BiologicalSex.Female), Has.Count.EqualTo(16));

            var missing = (from band in avatars.Bands
                           from face in avatars.Faces
                           let path = avatars.PathFor(face.Id, band.MinAge)
                           where !File.Exists(Path.Combine(ResourcesDir, path + ".png"))
                           select path).ToList();
            Assert.That(missing, Is.Empty, "pictures listed in avatars.json but not found");
        }
    }
}
