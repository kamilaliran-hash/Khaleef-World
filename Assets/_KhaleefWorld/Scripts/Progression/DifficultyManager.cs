using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Tracks a 0..1 skill value per subject and turns it into Easy / Medium / Hard.
    /// After each game: skill += 0.08 * (accuracy - 0.7). Good play nudges up, struggles nudge down.
    /// </summary>
    public class DifficultyManager
    {
        readonly SaveData save;

        public DifficultyManager(SaveData save) { this.save = save; }

        public float GetSkill(Subject s) => save.skills[(int)s];

        public Difficulty Get(Subject s)
        {
            float k = GetSkill(s);
            if (k < 0.35f) return Difficulty.Easy;
            if (k <= 0.7f) return Difficulty.Medium;
            return Difficulty.Hard;
        }

        public void Report(Subject s, float accuracy, float speedBonus = 0f)
        {
            float k = GetSkill(s) + 0.08f * (accuracy - 0.7f) + speedBonus;
            save.skills[(int)s] = Mathf.Clamp01(k);
        }

        /// <summary>Starting skill chosen by the parent's age band.</summary>
        public void SetForAgeBand(int band)
        {
            float v = band <= 0 ? 0.2f : band == 1 ? 0.5f : 0.75f;
            for (int i = 0; i < save.skills.Length; i++) save.skills[i] = v;
            save.parental.ageBand = band;
        }

        public static Difficulty Lower(Difficulty d) => d == Difficulty.Hard ? Difficulty.Medium : Difficulty.Easy;
    }
}
