using System;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>Coins, stars, passport stamps, badges, achievements and item unlocks.</summary>
    public class RewardManager
    {
        readonly GameManager gm;
        SaveData S => gm.Save;

        public event Action<int> CoinsChanged;
        public event Action<StateDefinition> StampEarned;
        public event Action<string> AchievementUnlocked;
        public event Action<MiniGameDefinition, int> MiniGameCompleted;

        public RewardManager(GameManager gm) { this.gm = gm; }

        public int Coins => S.coins;

        public void AddCoins(int amount, bool countsForQuests = true)
        {
            if (amount == 0) return;
            S.coins = Mathf.Max(0, S.coins + amount);
            CoinsChanged?.Invoke(S.coins);
            if (countsForQuests && amount > 0 && gm.Quests != null) gm.Quests.Notify(QuestType.CollectCoins, amount);
        }

        public bool SpendCoins(int amount)
        {
            if (S.coins < amount) return false;
            S.coins -= amount;
            CoinsChanged?.Invoke(S.coins);
            gm.SaveNow();
            return true;
        }

        public int StarsFor(string miniGameId)
        {
            var r = S.GetRecord(miniGameId, false);
            return r != null ? r.bestStars : 0;
        }

        public int StarsForState(StateDefinition st)
        {
            int total = 0;
            if (st == null) return 0;
            foreach (var m in st.miniGames) if (m != null) total += StarsFor(m.id);
            return total;
        }

        public bool HasStamp(StateDefinition st) => st != null && S.stamps.Contains(st.id);

        /// <summary>Records a finished mini-game. Returns coins earned. Always rewards effort (min 1 star).</summary>
        public int RecordMiniGame(MiniGameDefinition def, int stars, StateDefinition state, out bool stampEarned)
        {
            stampEarned = false;
            stars = Mathf.Clamp(stars, 1, 3);
            var rec = S.GetRecord(def.id, true);
            bool firstTime = rec.plays == 0;
            rec.plays++;
            int improvement = Mathf.Max(0, stars - rec.bestStars);
            rec.bestStars = Mathf.Max(rec.bestStars, stars);
            S.totalStars += improvement;

            int coins = def.baseCoins / 2 + stars * Mathf.Max(1, def.baseCoins / 3) + (firstTime ? 10 : 0);
            AddCoins(coins);
            MiniGameCompleted?.Invoke(def, stars);

            if (state != null && !HasStamp(state) && state.miniGames.Count > 0 &&
                state.miniGames.TrueForAll(m => m != null && StarsFor(m.id) > 0))
            {
                S.stamps.Add(state.id);
                if (!string.IsNullOrEmpty(state.badgeId) && !S.badges.Contains(state.badgeId)) S.badges.Add(state.badgeId);
                stampEarned = true;
                StampEarned?.Invoke(state);
            }

            CheckAchievements();
            gm.SaveNow();
            return coins;
        }

        void CheckAchievements()
        {
            Unlock("first_game", S.records.Count >= 1);
            Unlock("stars_25", S.totalStars >= 25);
            Unlock("passport_4", S.stamps.Count >= 4);
            Unlock("borneo", S.stamps.Contains("sabah") && S.stamps.Contains("sarawak") && S.stamps.Contains("labuan"));

            int mathsThreeStar = 0;
            var cfg = gm.Config;
            if (cfg != null)
            {
                foreach (var r in S.records)
                {
                    if (r.bestStars < 3) continue;
                    var def = cfg.FindMiniGame(r.id);
                    if (def != null && def.subject == Subject.Maths) mathsThreeStar++;
                }
            }
            Unlock("maths_master", mathsThreeStar >= 5);
        }

        void Unlock(string id, bool condition)
        {
            if (!condition || S.achievements.Contains(id)) return;
            S.achievements.Add(id);
            AchievementUnlocked?.Invoke(id);
        }

        public bool OwnsItem(CosmeticItem item)
        {
            if (item == null) return false;
            switch (item.unlock)
            {
                case UnlockType.Free: return true;
                case UnlockType.Badge: return S.badges.Contains(item.requiredBadge) || S.unlockedItems.Contains(item.id);
                default: return S.unlockedItems.Contains(item.id);
            }
        }

        /// <summary>Buys a cosmetic with in-game coins (never real money).</summary>
        public bool BuyItem(CosmeticItem item)
        {
            if (OwnsItem(item)) return true;
            if (item.unlock != UnlockType.Coins) return false;
            if (!SpendCoins(item.price)) return false;
            S.unlockedItems.Add(item.id);
            gm.SaveNow();
            return true;
        }
    }
}
