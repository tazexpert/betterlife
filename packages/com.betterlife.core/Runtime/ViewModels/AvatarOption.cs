namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One face in the avatar picker.
    /// </summary>
    public sealed class AvatarOption
    {
        /// <summary>
        /// Creates the option.
        /// </summary>
        /// <param name="faceId">Face number.</param>
        /// <param name="path">Resources path of the picture at the character's age.</param>
        /// <param name="isSelected">True for the current face.</param>
        public AvatarOption(int faceId, string path, bool isSelected)
        {
            FaceId = faceId;
            Path = path;
            IsSelected = isSelected;
        }

        /// <summary>Face number.</summary>
        public int FaceId { get; }

        /// <summary>Resources path of the picture at the character's age.</summary>
        public string Path { get; }

        /// <summary>True for the current face.</summary>
        public bool IsSelected { get; }
    }
}
