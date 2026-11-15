// Compile-check stand-ins for the Unity APIs that the runtime code under unity/Assets uses (NOT the real Unity API).
// They mirror the documented signatures of Unity 2022.3 so `dotnet test` catches mistakes in our own code
// (wrong names, missing arguments). A new Unity API used by the game must be added here deliberately,
// and the real check is still opening the project in Unity.
#pragma warning disable
using System;
using System.Collections.Generic;
namespace UnityEngine
{
    public class Object { public string name; public static void Destroy(Object o) {} public static void DontDestroyOnLoad(Object o) {} public static implicit operator bool(Object o) => o != null; }
    public class Component : Object { public GameObject gameObject; public Transform transform; public string tag; }
    public class Behaviour : Component { public bool enabled; }
    public class MonoBehaviour : Behaviour {}
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : ScriptableObject => null; }
    public class Transform : Component { public void SetParent(Transform p, bool w) {} }
    public class GameObject : Object { public GameObject(string n) {} public Transform transform; public string tag; public void SetActive(bool v) {} public T AddComponent<T>() where T : Component => null; }
    public class Camera : Behaviour { public static Camera main; public CameraClearFlags clearFlags; public Color backgroundColor; public int cullingMask; }
    public enum CameraClearFlags { Skybox, SolidColor }
    public struct Color { public Color(float r, float g, float b, float a) {} public static implicit operator Color(Color32 c) => default; }
    public struct Color32 { public Color32(byte r, byte g, byte b, byte a) {} }
    public struct Vector2 { public float x, y; public Vector2(float x, float y) { this.x = x; this.y = y; } }
    public struct Vector2Int { public int x, y; public Vector2Int(int x, int y) { this.x = x; this.y = y; } public static bool operator ==(Vector2Int a, Vector2Int b) => true; public static bool operator !=(Vector2Int a, Vector2Int b) => false; public override bool Equals(object o) => true; public override int GetHashCode() => 0; }
    public struct Rect { public float xMin, xMax, yMin, yMax, width, height; public static bool operator ==(Rect a, Rect b) => true; public static bool operator !=(Rect a, Rect b) => false; public override bool Equals(object o) => true; public override int GetHashCode() => 0; }
    public static class Screen { public static Rect safeArea; public static int width, height; }
    public static class Input { public static bool GetKeyDown(KeyCode k) => false; }
    public enum KeyCode { Escape }
    public static class Debug { public static void Log(object m) {} public static void LogWarning(object m) {} public static void LogError(object m) {} }
    public static class Application { public static string persistentDataPath, dataPath; public static SystemLanguage systemLanguage; public static void Quit() {} }
    public enum SystemLanguage { English, Thai, Unknown }
    public static class Resources { public static T Load<T>(string p) where T : Object => null; public static void UnloadAsset(Object o) {} }
    public class TextAsset : Object { public string text; }
    public static class GUIUtility { public static string systemCopyBuffer; }
    public enum RuntimeInitializeLoadType { SubsystemRegistration, BeforeSceneLoad }
    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute { public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t) {} }
}
namespace UnityEngine.UIElements
{
    public interface IPanel {}
    public enum DisplayStyle { Flex, None }
    public enum PanelScaleMode { ConstantPixelSize, ScaleWithScreenSize }
    public enum PanelScreenMatchMode { MatchWidthOrHeight }
    public enum LineJoin { Miter, Bevel, Round }
    public enum LineCap { Butt, Round }
    public struct StyleLength { public static implicit operator StyleLength(float v) => default; }
    public struct StyleFloat { public static implicit operator StyleFloat(float v) => default; }
    public struct StyleEnum<T> { public static implicit operator StyleEnum<T>(T v) => default; }
    public interface IStyle { StyleLength paddingLeft { get; set; } StyleLength paddingTop { get; set; } StyleLength paddingRight { get; set; } StyleLength paddingBottom { get; set; } StyleFloat flexGrow { get; set; } StyleEnum<DisplayStyle> display { get; set; } }
    public interface IResolvedStyle { Color color { get; } }
    public class Painter2D { public Color strokeColor; public float lineWidth; public LineJoin lineJoin; public LineCap lineCap; public void BeginPath() {} public void MoveTo(Vector2 p) {} public void LineTo(Vector2 p) {} public void Stroke() {} }
    public class MeshGenerationContext { public Painter2D painter2D => null; }
    public interface IManipulator {}
    public interface IVisualElementScheduledItem { IVisualElementScheduledItem StartingIn(long delayMs); }
    public interface IVisualElementScheduler { IVisualElementScheduledItem Execute(Action timerUpdateEvent); }
    public class Clickable : IManipulator { public Clickable(Action a) {} }
    public class VisualElement
    {
        public string name; public object userData; public IStyle style; public IResolvedStyle resolvedStyle; public IPanel panel; public Rect contentRect;
        public Action<MeshGenerationContext> generateVisualContent;
        public void Add(VisualElement e) {} public void Clear() {} public void AddToClassList(string c) {} public void EnableInClassList(string c, bool on) {}
        public void SetEnabled(bool v) {} public void MarkDirtyRepaint() {}
        public IVisualElementScheduler schedule; public int childCount; public void RemoveAt(int index) {} public void RemoveFromHierarchy() {} public void RemoveFromClassList(string c) {}
    }
    public class TemplateContainer : VisualElement {}
    public class TextElement : VisualElement { public string text; }
    public class Label : TextElement { public Label() {} public Label(string t) {} }
    public class Button : TextElement { public Button() {} public Button(Action a) {} public event Action clicked; }
    public struct UQueryBuilder<T> where T : VisualElement { public void ForEach(Action<T> a) {} }
    public static class UQueryExtensions
    {
        public static T Q<T>(this VisualElement e, string name = null, string className = null) where T : VisualElement => null;
        public static UQueryBuilder<T> Query<T>(this VisualElement e, string name = null, string className = null) where T : VisualElement => default;
    }
    public static class VisualElementExtensions { public static void AddManipulator(this VisualElement e, IManipulator m) {} }
    public class VisualTreeAsset : Object { public TemplateContainer Instantiate() => null; }
    public class StyleSheet : Object {}
    public class ThemeStyleSheet : StyleSheet {}
    public class PanelSettings : ScriptableObject { public ThemeStyleSheet themeStyleSheet; public PanelScaleMode scaleMode; public Vector2Int referenceResolution; public PanelScreenMatchMode screenMatchMode; public float match; }
    public class UIDocument : MonoBehaviour { public PanelSettings panelSettings; public VisualTreeAsset visualTreeAsset; public VisualElement rootVisualElement; }
    public static class RuntimePanelUtils { public static Vector2 ScreenToPanel(IPanel p, Vector2 s) => s; }
}
