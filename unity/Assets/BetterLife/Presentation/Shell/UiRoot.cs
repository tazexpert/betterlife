using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Shell
{
    /// <summary>
    /// Hosts the UI Toolkit panel. Everything is created from code: panel settings, the UI document with the shell
    /// layout, a camera if the scene has none, the safe area and the Android back button.
    /// </summary>
    public sealed class UiRoot : MonoBehaviour
    {
        /// <summary>Name of the created host object.</summary>
        public const string HostObjectName = "[BetterLife] UI";
        /// <summary>Reference resolution of the portrait layout.</summary>
        public static readonly Vector2Int ReferenceResolution = new Vector2Int(1080, 1920);

        private const string CameraObjectName = "[BetterLife] Camera";
        private const string SafeAreaName = "safe-area";
        private const float MatchWidth = 0f;
        private static readonly Color Background = new Color32(0x10, 0x14, 0x1B, 0xFF);

        private PanelSettings _settings;
        private UIDocument _document;
        private SafeArea _safeArea;

        /// <summary>Raised when the device back button (Escape) is pressed.</summary>
        public event Action BackPressed;

        /// <summary>Root of the shell layout.</summary>
        public VisualElement Root => _document.rootVisualElement;

        /// <summary>
        /// Creates the UI host under a parent.
        /// </summary>
        /// <param name="parent">Parent transform (the bootstrap host, which survives scene loads).</param>
        /// <returns>The UI root, with the shell layout already built.</returns>
        public static UiRoot Create(Transform parent)
        {
            EnsureCamera();
            var host = new GameObject(HostObjectName);
            host.SetActive(false);
            host.transform.SetParent(parent, false);
            UiRoot root = host.AddComponent<UiRoot>();
            root.Build();
            host.SetActive(true);
            root.Root.style.flexGrow = 1;
            root._safeArea = new SafeArea(root.Root.Q<VisualElement>(SafeAreaName));
            return root;
        }

        private void Build()
        {
            _settings = ScriptableObject.CreateInstance<PanelSettings>();
            _settings.name = HostObjectName;
            _settings.themeStyleSheet = UiAssets.LoadTheme();
            _settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _settings.referenceResolution = ReferenceResolution;
            _settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            _settings.match = MatchWidth;

            _document = gameObject.AddComponent<UIDocument>();
            _document.panelSettings = _settings;
            _document.visualTreeAsset = UiAssets.Layout(UiAssets.Shell);
        }

        private void Update()
        {
            _safeArea?.Update();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                BackPressed?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_settings != null)
            {
                Destroy(_settings);
            }
        }

        /// <summary>
        /// The main scene is empty, so a clearing camera is created to avoid the "no cameras rendering" screen.
        /// </summary>
        private static void EnsureCamera()
        {
            if (Camera.main != null)
            {
                return;
            }
            var host = new GameObject(CameraObjectName);
            DontDestroyOnLoad(host);
            Camera camera = host.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.cullingMask = 0;
            host.tag = "MainCamera";
        }
    }
}
