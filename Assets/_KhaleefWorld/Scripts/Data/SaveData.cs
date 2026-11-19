using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>Everything that defines how the player's blocky avatar looks.</summary>
    [Serializable]
    public class AvatarData
    {
        public const int SlotCount = 7;

        public int skinTone = 4;
        public int eyes;
        public int mouth;
        public int hairStyle;
        public int hairColor;
        public int heightStep = 1;               // 0 short, 1 medium, 2 tall
        // Default look: school uniform, school trousers, school shoes (slots: Head, Top, Bottom, Shoes, Accessory, Back, Pet)
        public string[] equipped = { null, "school_uniform", "seluar_sekolah", "kasut_sekolah", null, null, null };

        public float HeightScale => heightStep == 0 ? 0.88f : heightStep == 2 ? 1.12f : 1f;

        public string Get(CosmeticSlot slot) { Ensure(); return equipped[(int)slot]; }
        public void Set(CosmeticSlot slot, string id) { Ensure(); equipped[(int)slot] = id; }

        public AvatarData Clone() => JsonUtility.FromJson<AvatarData>(JsonUtility.ToJson(this));

        void Ensure()
        {
            if (equipped != null && equipped.Length == SlotCount) return;
            var n = new string[SlotCount];
            if (equipped != null) Array.Copy(equipped, n, Math.Min(equipped.Length, SlotCount));
            equipped = n;
        }
    }

    [Serializable]
    public class OutfitPreset
    {
        public bool used;
        public string name;
        public AvatarData avatar = new AvatarData();
    }

    [Serializable]
    public class MiniGameRecord
    {
        public string id;
        public int bestStars;
        public int plays;
    }

    [Serializable]
    public class ParentalSettings
    {
        public int dailyLimitMinutes = 60;      // 0 = no limit
        public bool multiplayer;                // off by default (online mode is a later milestone)
        public bool leaderboard;                // off by default
        public bool voiceOver = true;
        public bool soundOn = true;
        public bool unlockAllStates;
        public int ageBand = 1;                 // 0: 6-7, 1: 8-9, 2: 10-12
    }

    [Serializable]
    public class QuestState
    {
        public string id;                       // QuestType name
        public int progress;
        public int target;
        public int reward;
        public bool claimed;
    }

    /// <summary>The whole save file. Kept small and free of personal data (no names, no location).</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public bool setupDone;
        public bool avatarCreated;
        public string language = "ms";          // "ms" Bahasa Melayu, "en" English

        public AvatarData avatar = new AvatarData();
        public OutfitPreset[] presets = new OutfitPreset[5];

        public int coins = 50;
        public int totalStars;
        public List<string> unlockedItems = new List<string>();
        public List<string> stamps = new List<string>();
        public List<string> badges = new List<string>();
        public List<string> visitedStates = new List<string>();
        public List<string> achievements = new List<string>();
        public List<MiniGameRecord> records = new List<MiniGameRecord>();
        public float[] skills = { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };   // indexed by Subject

        public ParentalSettings parental = new ParentalSettings();
        public string playDate;
        public float playSecondsToday;

        public string questDate;
        public List<QuestState> quests = new List<QuestState>();
        public string lastStateId;

        public MiniGameRecord GetRecord(string id, bool create)
        {
            var r = records.Find(x => x.id == id);
            if (r == null && create) { r = new MiniGameRecord { id = id }; records.Add(r); }
            return r;
        }

        /// <summary>Repairs nulls after loading older or partial save files.</summary>
        public void Validate()
        {
            if (avatar == null) avatar = new AvatarData();
            if (presets == null || presets.Length != 5) presets = new OutfitPreset[5];
            for (int i = 0; i < presets.Length; i++) if (presets[i] == null) presets[i] = new OutfitPreset();
            if (skills == null || skills.Length != 5) skills = new[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
            if (parental == null) parental = new ParentalSettings();
            if (unlockedItems == null) unlockedItems = new List<string>();
            if (stamps == null) stamps = new List<string>();
            if (badges == null) badges = new List<string>();
            if (visitedStates == null) visitedStates = new List<string>();
            if (achievements == null) achievements = new List<string>();
            if (records == null) records = new List<MiniGameRecord>();
            if (quests == null) quests = new List<QuestState>();
            if (language != "ms" && language != "en") language = "ms";
        }
    }
}
