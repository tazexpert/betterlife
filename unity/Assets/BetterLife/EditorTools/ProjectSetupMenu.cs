using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterLife.EditorTools
{
    /// <summary>
    /// One-click, idempotent project configuration. Everything a new clone needs is applied from code,
    /// so no manual Inspector or Player Settings work is required.
    /// </summary>
    public static class ProjectSetupMenu
    {
        private const string MenuPath = "BetterLife/Setup Project";
        private const string LogPrefix = "[BetterLife] ";

        private const string CompanyName = "BetterLife";
        private const string ProductName = "BetterLife";
        private const string ApplicationIdentifier = "com.betterlife.game";

        private const string RootFolder = "Assets/BetterLife";
        private const string ScenesFolder = RootFolder + "/Scenes";
        private const string MainScenePath = ScenesFolder + "/Main.unity";

        private static readonly string[] RequiredFolders =
        {
            ScenesFolder,
            RootFolder + "/Presentation/UI/Uxml",
            RootFolder + "/Presentation/UI/Uss",
        };

        private static readonly NamedBuildTarget[] MobileTargets =
        {
            NamedBuildTarget.Android,
            NamedBuildTarget.iOS,
        };

        /// <summary>
        /// Applies folders, player settings, the main scene and build settings. Safe to run repeatedly.
        /// </summary>
        [MenuItem(MenuPath)]
        public static void SetupProject()
        {
            EnsureFolders();
            ApplyPlayerSettings();
            if (!EnsureMainScene())
            {
                Debug.LogWarning(LogPrefix + "Setup stopped: current scene changes were not saved.");
                return;
            }

            AssetDatabase.SaveAssets();
            Debug.Log(LogPrefix + "Project setup complete.");
        }

        private static void EnsureFolders()
        {
            foreach (string folder in RequiredFolders)
            {
                EnsureFolder(folder);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;

            // Portrait only, for one-handed play.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            foreach (NamedBuildTarget target in MobileTargets)
            {
                PlayerSettings.SetApplicationIdentifier(target, ApplicationIdentifier);
                PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetApiCompatibilityLevel(target, ApiCompatibilityLevel.NET_Standard);
            }
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);

            // Google Play requires 64-bit builds.
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        private static bool EnsureMainScene()
        {
            if (!File.Exists(MainScenePath))
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return false;
                }

                // The scene stays empty: GameBootstrap creates itself at runtime.
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, MainScenePath);
            }

            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(s => s.path != MainScenePath)
                .ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(MainScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            return true;
        }
    }
}
