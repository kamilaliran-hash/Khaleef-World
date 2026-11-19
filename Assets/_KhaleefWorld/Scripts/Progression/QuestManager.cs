using System;
using System.Collections.Generic;

namespace KhaleefWorld
{
    /// <summary>Three daily quests, picked deterministically from the date. Missing a day never loses progress.</summary>
    public class QuestManager
    {
        struct QuestTemplate
        {
            public QuestType type; public int target; public int reward; public string ms; public string en;
            public QuestTemplate(QuestType t, int target, int reward, string ms, string en)
            { type = t; this.target = target; this.reward = reward; this.ms = ms; this.en = en; }
        }

        static readonly QuestTemplate[] Pool =
        {
            new QuestTemplate(QuestType.PlayMiniGame, 2, 30, "Main {0} permainan mini", "Play {0} mini-games"),
            new QuestTemplate(QuestType.PlayMaths, 1, 25, "Main {0} permainan matematik", "Play {0} maths game"),
            new QuestTemplate(QuestType.PlayLanguage, 1, 25, "Main {0} permainan bahasa", "Play {0} language game"),
            new QuestTemplate(QuestType.PlayScience, 1, 25, "Main {0} permainan sains", "Play {0} science game"),
            new QuestTemplate(QuestType.PlayHistory, 1, 25, "Main {0} permainan sejarah", "Play {0} history game"),
            new QuestTemplate(QuestType.CollectCoins, 20, 20, "Kumpul {0} syiling", "Collect {0} coins"),
            new QuestTemplate(QuestType.EarnStars, 5, 30, "Dapatkan {0} bintang", "Earn {0} stars"),
            new QuestTemplate(QuestType.VisitState, 1, 20, "Lawat {0} negeri", "Visit {0} state"),
            new QuestTemplate(QuestType.WearTraditional, 1, 20, "Pakai pakaian tradisional", "Wear a traditional outfit"),
        };

        readonly GameManager gm;
        SaveData S => gm.Save;

        public event Action<QuestState> QuestCompleted;

        public QuestManager(GameManager gm) { this.gm = gm; }

        public List<QuestState> Today { get { Refresh(); return S.quests; } }

        public void Refresh()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (S.questDate == today && S.quests.Count == 3) return;
            S.questDate = today;
            S.quests.Clear();
            var rng = new Random(int.Parse(DateTime.Now.ToString("yyyyMMdd")));
            var picked = new List<int>();
            while (picked.Count < 3)
            {
                int i = rng.Next(Pool.Length);
                if (!picked.Contains(i)) picked.Add(i);
            }
            foreach (int i in picked)
                S.quests.Add(new QuestState { id = Pool[i].type.ToString(), target = Pool[i].target, reward = Pool[i].reward });
        }

        public void Notify(QuestType type, int amount)
        {
            if (amount <= 0) return;
            Refresh();
            string id = type.ToString();
            foreach (var q in S.quests)
            {
                if (q.id != id || q.claimed) continue;
                q.progress = Math.Min(q.target, q.progress + amount);
                if (q.progress >= q.target)
                {
                    q.claimed = true;
                    gm.Rewards.AddCoins(q.reward, countsForQuests: false);
                    UIFactory.Toast(Loc.Get("quest_done") + "  +" + q.reward + " " + Loc.Get("coins"), 3f);
                    QuestCompleted?.Invoke(q);
                }
            }
        }

        public string Describe(QuestState q)
        {
            foreach (var t in Pool)
                if (t.type.ToString() == q.id) return string.Format(Loc.Pick(t.ms, t.en), q.target);
            return q.id;
        }
    }
}
