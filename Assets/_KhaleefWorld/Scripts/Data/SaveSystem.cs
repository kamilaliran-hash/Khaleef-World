using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Local, offline-first JSON save with a checksum to detect tampering or corruption.
    /// Writes to a temp file first so a crash mid-save never destroys progress.
    /// Cloud save (Firebase) can later sync the same JSON string.
    /// </summary>
    public static class SaveSystem
    {
        const string FileName = "khaleef_save.json";
        const string Salt = "KhaleefWorld::v1";

        [Serializable]
        class Envelope
        {
            public string data;
            public string hash;
        }

        static string FullPath => Path.Combine(Application.persistentDataPath, FileName);

        public static SaveData Load()
        {
            try
            {
                if (File.Exists(FullPath))
                {
                    var env = JsonUtility.FromJson<Envelope>(File.ReadAllText(FullPath));
                    if (env != null && !string.IsNullOrEmpty(env.data) && env.hash == Hash(env.data))
                    {
                        var data = JsonUtility.FromJson<SaveData>(env.data);
                        if (data != null) { data.Validate(); return data; }
                    }
                    Debug.LogWarning("[SaveSystem] Save file failed checksum – starting a fresh save.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Could not read save: " + e.Message);
            }

            var fresh = new SaveData();
            fresh.Validate();
            return fresh;
        }

        public static void Save(SaveData data)
        {
            if (data == null) return;
            try
            {
                string json = JsonUtility.ToJson(data);
                var env = new Envelope { data = json, hash = Hash(json) };
                string tmp = FullPath + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(env));
                if (File.Exists(FullPath)) File.Delete(FullPath);
                File.Move(tmp, FullPath);
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveSystem] Could not write save: " + e.Message);
            }
        }

        public static void DeleteAll()
        {
            try { if (File.Exists(FullPath)) File.Delete(FullPath); }
            catch (Exception e) { Debug.LogError("[SaveSystem] Could not delete save: " + e.Message); }
        }

        static string Hash(string s)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s + Salt));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
