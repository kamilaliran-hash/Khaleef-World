using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KhaleefWorld.EditorTools
{
    /// <summary>
    /// One-click Android builds.
    ///   Khaleef World > 2. Apply Android Player Settings
    ///   Khaleef World > 3. Build APK (test on a phone)
    ///   Khaleef World > 4. Build AAB (upload to Google Play)
    /// Command line (CI):
    ///   Unity -batchmode -quit -projectPath . -executeMethod KhaleefWorld.EditorTools.KhaleefBuildScript.BuildApkCommandLine
    /// </summary>
    public static class KhaleefBuildScript
    {
        const string BundleId = "com.khaleefworld.game";   // change to your own reverse-domain id before publishing
        const string OutputDir = "Builds/Android";

        [MenuItem("Khaleef World/2. Apply Android Player Settings", priority = 20)]
        public static void ApplySettings()
        {
            PlayerSettings.companyName = "Khaleef Studio";
            PlayerSettings.productName = "Khaleef World";
            if (string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "0.1") PlayerSettings.bundleVersion = "0.1.0";

            var android = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(android, BundleId);
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Low);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;              // Android 8.0
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;         // newest installed
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.Android.forceInternetPermission = false;                                   // offline single-player

            // Landscape only
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.runInBackground = false;
            Debug.Log("[Khaleef World] Android player settings applied.");
        }

        [MenuItem("Khaleef World/3. Build APK (test on a phone)", priority = 21)]
        public static void BuildApk() => Build(false, true);

        [MenuItem("Khaleef World/4. Build AAB (Google Play)", priority = 22)]
        public static void BuildAab() => Build(true, true);

        public static void BuildApkCommandLine() => Build(false, false);
        public static void BuildAabCommandLine() => Build(true, false);

        static void Build(bool appBundle, bool interactive)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Fail("No scenes in Build Settings. Run 'Khaleef World > 1. Create MVP Content' first.", interactive);
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Fail("Could not switch to Android. Install 'Android Build Support' (with SDK, NDK and OpenJDK) in Unity Hub.", interactive);
                return;
            }

            ApplySettings();
            EditorUserBuildSettings.buildAppBundle = appBundle;
            Directory.CreateDirectory(OutputDir);
            string path = Path.Combine(OutputDir, appBundle ? "KhaleefWorld.aab" : "KhaleefWorld.apk");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                string msg = $"Build succeeded: {path}\nSize: {summary.totalSize / (1024f * 1024f):0.0} MB";
                Debug.Log("[Khaleef World] " + msg);
                if (interactive)
                {
                    EditorUtility.RevealInFinder(path);
                    EditorUtility.DisplayDialog("Khaleef World", msg, "OK");
                }
            }
            else
            {
                Fail($"Build {summary.result}. See the Console for details.", interactive);
                if (!interactive) EditorApplication.Exit(1);
            }
        }

        static void Fail(string message, bool interactive)
        {
            Debug.LogError("[Khaleef World] " + message);
            if (interactive) EditorUtility.DisplayDialog("Khaleef World", message, "OK");
        }
    }
}
