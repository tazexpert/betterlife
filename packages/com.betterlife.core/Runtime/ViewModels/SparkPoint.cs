namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One sparkline point in a unit box: X from 0 (oldest) to 1 (newest), Y from 0 (bottom) to 1 (top).
    /// </summary>
    public readonly struct SparkPoint
    {
        /// <summary>
        /// Creates a point.
        /// </summary>
        /// <param name="x">Horizontal position 0..1.</param>
        /// <param name="y">Vertical position 0..1 (0 = bottom).</param>
        public SparkPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Horizontal position 0..1.</summary>
        public float X { get; }

        /// <summary>Vertical position 0..1 (0 = bottom).</summary>
        public float Y { get; }
    }
}
