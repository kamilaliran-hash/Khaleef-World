using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KhaleefWorld.EditorTools
{
    /// <summary>
    /// Headless entry point for CI (GitHub Actions / GameCI).
    /// Sets up URP, generates the MVP content and builds the APK in one batch-mode run.
    /// </summary>
    public static class KhaleefCI
    {
        const string SettingsDir = "Assets/_KhaleefWorld/Settings";

        public static void BuildApk() => Run(false);
        public static void BuildAab() => Run(true);

        static void Run(bool aab)
        {
            try
            {
                EnsureUrp();
                KhaleefSetupWizard.CreateAll();
                if (EditorBuildSettings.scenes.Length == 0)
                    throw new Exception("Content generation produced no scenes (is URP installed?).");
                if (aab) KhaleefBuildScript.BuildAabCommandLine();
                else KhaleefBuildScript.BuildApkCommandLine();
            }
            catch (Exception e)
            {
                Debug.LogError("[Khaleef CI] " + e);
                EditorApplication.Exit(1);
            }
        }

        // A project created without the URP template has no pipeline asset, so URP materials would render pink.
        static void EnsureUrp()
        {
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset) return;

            if (!AssetDatabase.IsValidFolder(SettingsDir))
                AssetDatabase.CreateFolder("Assets/_KhaleefWorld", "Settings");

            var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, SettingsDir + "/URP_Renderer.asset");

            var urp = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(urp, SettingsDir + "/URP_Asset.asset");
            AssetDatabase.SaveAssets();

            GraphicsSettings.defaultRenderPipeline = urp;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = urp;
            }
            QualitySettings.SetQualityLevel(current, false);
            AssetDatabase.SaveAssets();
            Debug.Log("[Khaleef CI] URP pipeline asset created and assigned.");
        }
    }
}
