using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Persistent root object. Created automatically before the first scene loads, so any scene can be
    /// played directly in the Editor. Owns the save file and all game services.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public SaveData Save { get; private set; }
        public RewardManager Rewards { get; private set; }
        public DifficultyManager Difficulty { get; private set; }
        public QuestManager Quests { get; private set; }
        public ParentalControlManager Parental { get; private set; }
        public AudioManager Audio { get; private set; }
        public GameConfig Config => GameConfig.Instance;

        public StateDefinition CurrentState { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoCreate()
        {
            if (I != null) return;
            var go = new GameObject("[Khaleef World]");
            go.AddComponent<GameManager>();
        }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);

            // 30 FPS on 3 GB phones keeps them cool; 60 FPS where there is headroom.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = SystemInfo.systemMemorySize >= 4000 ? 60 : 30;

            Audio = gameObject.AddComponent<AudioManager>();
            Parental = gameObject.AddComponent<ParentalControlManager>();
            InitServices(SaveSystem.Load());
            Audio.Init(this);
        }

        void InitServices(SaveData data)
        {
            Save = data;
            Difficulty = new DifficultyManager(Save);
            Rewards = new RewardManager(this);
            Quests = new QuestManager(this);
            Quests.Refresh();
            Parental.Init(this);

            Rewards.MiniGameCompleted += (def, stars) =>
            {
                Quests.Notify(QuestType.PlayMiniGame, 1);
                Quests.Notify(QuestType.EarnStars, stars);
                if (def.subject == Subject.Maths) Quests.Notify(QuestType.PlayMaths, 1);
                if (def.subject == Subject.Language) Quests.Notify(QuestType.PlayLanguage, 1);
                if (def.subject == Subject.Science) Quests.Notify(QuestType.PlayScience, 1);
                if (def.subject == Subject.History) Quests.Notify(QuestType.PlayHistory, 1);
            };
            Rewards.AchievementUnlocked += id =>
                UIFactory.Toast(Loc.Get("achievement") + ": " + Loc.Get("ach_" + id), 3.5f);
        }

        public void SaveNow() => SaveSystem.Save(Save);

        /// <summary>Parent dashboard "delete all data".</summary>
        public void ResetAllData()
        {
            SaveSystem.DeleteAll();
            var fresh = new SaveData();
            fresh.Validate();
            InitServices(fresh);
            SaveNow();
        }

        void OnApplicationPause(bool paused) { if (paused) SaveNow(); }
        void OnApplicationQuit() { SaveNow(); }
    }
}
