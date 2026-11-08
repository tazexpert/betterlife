using System.Reflection;

// Layer marker so the assembly always has at least one source file.
// Infrastructure layer: Unity implementations of Core interfaces (Save, Ads, IAP, Analytics, Content loader).
// References Core only.
[assembly: AssemblyMetadata("BetterLife.Layer", "Infrastructure")]
