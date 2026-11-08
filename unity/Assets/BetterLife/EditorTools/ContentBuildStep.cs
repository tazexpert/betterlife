using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace BetterLife.EditorTools
{
    /// <summary>
    /// Runs automatically before every player build: validates content and copies it into Resources.
    /// The build fails when content has errors, so broken content can never ship.
    /// </summary>
    public sealed class ContentBuildStep : IPreprocessBuildWithReport
    {
        /// <summary>Run before other preprocess steps.</summary>
        public int callbackOrder => 0;

        /// <summary>
        /// Validates and copies content.
        /// </summary>
        /// <param name="report">The build report.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            string error = ContentSync.ValidateAndCopy();
            if (error != null)
            {
                throw new BuildFailedException(error);
            }
        }
    }
}
