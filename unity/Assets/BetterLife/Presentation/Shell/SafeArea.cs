using UnityEngine;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Shell
{
    /// <summary>
    /// Keeps an element inside the device safe area (notch, rounded corners, gesture bar) by turning
    /// <see cref="Screen.safeArea"/> into padding. Call <see cref="Update"/> every frame; it only writes on change.
    /// </summary>
    public sealed class SafeArea
    {
        private readonly VisualElement _target;
        private Rect _lastArea;
        private Vector2Int _lastScreen;

        /// <summary>
        /// Creates the helper.
        /// </summary>
        /// <param name="target">Element that receives the padding.</param>
        public SafeArea(VisualElement target)
        {
            _target = target;
        }

        /// <summary>
        /// Applies the safe area when it or the screen size changed.
        /// </summary>
        public void Update()
        {
            IPanel panel = _target?.panel;
            if (panel == null)
            {
                return;
            }
            Rect area = Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (area == _lastArea && screen == _lastScreen)
            {
                return;
            }
            _lastArea = area;
            _lastScreen = screen;

            // Screen space has its origin bottom-left; panel space is top-left, so the y insets are flipped.
            Vector2 topLeft = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(area.xMin, screen.y - area.yMax));
            Vector2 bottomRight = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screen.x - area.xMax, area.yMin));
            _target.style.paddingLeft = topLeft.x;
            _target.style.paddingTop = topLeft.y;
            _target.style.paddingRight = bottomRight.x;
            _target.style.paddingBottom = bottomRight.y;
        }
    }
}
