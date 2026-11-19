using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KhaleefWorld
{
    /// <summary>
    /// Loads scenes asynchronously while a travel animation plays.
    /// Define KHALEEF_ADDRESSABLES (Player Settings > Scripting Define Symbols) after installing the
    /// Addressables package to stream each state as a downloadable pack instead of a built-in scene.
    /// </summary>
    public static class SceneLoader
    {
        public const string Boot = "Boot";
        public const string CharacterCreator = "CharacterCreator";
        public const string WorldMap = "WorldMap";

        public static bool IsLoading { get; private set; }

        public static bool Exists(string scene) => !string.IsNullOrEmpty(scene) && Application.CanStreamedLevelBeLoaded(scene);

        public static void Load(string scene, Action<float> onProgress = null, float minSeconds = 0f)
        {
            if (IsLoading || GameManager.I == null) return;
            GameManager.I.StartCoroutine(LoadRoutine(scene, onProgress, minSeconds));
        }

        static IEnumerator LoadRoutine(string scene, Action<float> onProgress, float minSeconds)
        {
            IsLoading = true;
            Time.timeScale = 1f;
            GameManager.I.SaveNow();
            float t0 = Time.unscaledTime;
#if KHALEEF_ADDRESSABLES
            var handle = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(scene);
            while (!handle.IsDone || Time.unscaledTime - t0 < minSeconds)
            {
                onProgress?.Invoke(Mathf.Min(handle.PercentComplete, (Time.unscaledTime - t0) / Mathf.Max(0.01f, minSeconds)));
                yield return null;
            }
#else
            var op = SceneManager.LoadSceneAsync(scene);
            op.allowSceneActivation = false;
            while (op.progress < 0.9f || Time.unscaledTime - t0 < minSeconds)
            {
                float p = Mathf.Min(op.progress / 0.9f, (Time.unscaledTime - t0) / Mathf.Max(0.01f, minSeconds));
                onProgress?.Invoke(p);
                yield return null;
            }
            onProgress?.Invoke(1f);
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
#endif
            IsLoading = false;
        }
    }
}
