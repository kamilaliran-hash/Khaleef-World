using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>Parent dashboard: screen time, language, toggles, learning report and data deletion.</summary>
    public static class ParentDashboardScreen
    {
        static readonly int[] Limits = { 15, 30, 45, 60, 90, 120, 0 };

        public static void Open()
        {
            var gm = GameManager.I;
            var s = gm.Save;
            var p = s.parental;

            var dim = UIFactory.Overlay("ParentDashboard", 950, out var canvas);
            var go = canvas.gameObject;
            var box = UIFactory.Dialog(dim, new Vector2(1800, 1000));

            var title = UIFactory.Label(box, Loc.Get("dashboard"), 56, UIFactory.Purple);
            UIFactory.Anchor(title.rectTransform, 0.03f, 0.9f, 0.8f, 0.98f);
            var close = UIFactory.Button(box, "X", UIFactory.Grey, () => Object.Destroy(go), 44);
            UIFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(100, 100), new Vector2(-70, -60));

            // ---- Left column: settings ----
            var left = UIFactory.CreateRect("Settings", box);
            UIFactory.Anchor(left, 0.02f, 0.03f, 0.5f, 0.89f);
            UIFactory.VLayout(left, 14, 10);

            var played = UIFactory.Label(left, Loc.Format("played_today", Mathf.RoundToInt(gm.Parental.MinutesPlayedToday)), 36, UIFactory.Dark, TextAnchor.MiddleLeft);
            UIFactory.Size(played, -1, 60);

            // Daily limit stepper
            var limitRow = Row(left, Loc.Get("daily_limit"));
            Text limitValue = null;
            System.Action refreshLimit = () => limitValue.text = p.dailyLimitMinutes <= 0 ? Loc.Get("unlimited") : p.dailyLimitMinutes + " min";
            var minus = UIFactory.Button(limitRow, "-", UIFactory.Orange, () => { p.dailyLimitMinutes = StepLimit(p.dailyLimitMinutes, -1); refreshLimit(); gm.SaveNow(); }, 48);
            UIFactory.Size(minus, 100, 80);
            limitValue = UIFactory.Label(limitRow, "", 36, UIFactory.Dark);
            UIFactory.Size(limitValue, 200, 80);
            var plus = UIFactory.Button(limitRow, "+", UIFactory.Green, () => { p.dailyLimitMinutes = StepLimit(p.dailyLimitMinutes, 1); refreshLimit(); gm.SaveNow(); }, 48);
            UIFactory.Size(plus, 100, 80);
            refreshLimit();

            // Language
            var langRow = Row(left, Loc.Get("language"));
            UIFactory.Size(UIFactory.Button(langRow, "BM", s.language == "ms" ? UIFactory.Teal : UIFactory.Grey, () => SetLanguage("ms", go), 36), 160, 80);
            UIFactory.Size(UIFactory.Button(langRow, "EN", s.language == "en" ? UIFactory.Teal : UIFactory.Grey, () => SetLanguage("en", go), 36), 160, 80);

            Toggle(left, Loc.Get("sound"), () => p.soundOn, v => { p.soundOn = v; gm.Audio.RefreshMute(); });
            Toggle(left, Loc.Get("voice"), () => p.voiceOver, v => p.voiceOver = v);
            Toggle(left, Loc.Get("unlock_all"), () => p.unlockAllStates, v => p.unlockAllStates = v);

            var mpRow = Row(left, Loc.Get("multiplayer"));
            var mp = UIFactory.Button(mpRow, Loc.Get("online_later"), UIFactory.Grey, null, 30);
            mp.interactable = false;
            UIFactory.Size(mp, 380, 80);

            var del = UIFactory.Button(left, Loc.Get("delete_data"), UIFactory.Red, () =>
                UIFactory.Confirm(Loc.Get("delete_confirm"), () =>
                {
                    Object.Destroy(go);
                    gm.ResetAllData();
                    SceneLoader.Load(SceneLoader.Boot);
                }), 34);
            UIFactory.Size(del, -1, 80);

            var privacy = UIFactory.Label(left, Loc.Get("privacy_note"), 26, UIFactory.Grey, TextAnchor.MiddleLeft);
            UIFactory.Size(privacy, -1, 70);

            // ---- Right column: learning report ----
            var right = UIFactory.Panel(box, new Color(0.93f, 0.95f, 1f), "Report");
            UIFactory.Anchor(right.rectTransform, 0.52f, 0.03f, 0.98f, 0.89f);
            UIFactory.VLayout(right, 12, 24);

            var rt = UIFactory.Label(right.transform, Loc.Get("report"), 42, UIFactory.Blue);
            UIFactory.Size(rt, -1, 70);

            for (int i = 0; i < 5; i++)
            {
                var subj = (Subject)i;
                float skill = gm.Difficulty.GetSkill(subj);
                var row = UIFactory.CreateRect("Skill_" + subj, right.transform);
                UIFactory.Size(row, -1, 70);
                var name = UIFactory.Label(row, Loc.SubjectName(subj), 32, UIFactory.Dark, TextAnchor.MiddleLeft);
                UIFactory.Anchor(name.rectTransform, 0f, 0f, 0.42f, 1f);
                var barBg = UIFactory.Panel(row, new Color(0.8f, 0.82f, 0.88f), "BarBg");
                UIFactory.Anchor(barBg.rectTransform, 0.43f, 0.25f, 0.8f, 0.75f);
                var bar = UIFactory.Panel(barBg.transform, UIFactory.SubjectColor(subj), "Bar");
                UIFactory.Anchor(bar.rectTransform, 0f, 0f, Mathf.Max(0.05f, skill), 1f);
                var lvl = UIFactory.Label(row, Loc.DifficultyName(gm.Difficulty.Get(subj)), 28, UIFactory.Dark, TextAnchor.MiddleRight);
                UIFactory.Anchor(lvl.rectTransform, 0.81f, 0f, 1f, 1f);
            }

            int totalStates = gm.Config != null ? gm.Config.states.Count : 16;
            var stats = UIFactory.Label(right.transform,
                $"{Loc.Get("stars")}: {s.totalStars}    {Loc.Get("passport")}: {s.stamps.Count}/{totalStates}    {Loc.Get("coins")}: {s.coins}\n" +
                $"{Loc.Get("achievement")}: {s.achievements.Count}",
                32, UIFactory.Dark);
            UIFactory.Size(stats, -1, 130);
        }

        static int StepLimit(int current, int dir)
        {
            int idx = System.Array.IndexOf(Limits, current);
            if (idx < 0) idx = 3;
            idx = Mathf.Clamp(idx + dir, 0, Limits.Length - 1);
            return Limits[idx];
        }

        static void SetLanguage(string lang, GameObject dashboard)
        {
            GameManager.I.Save.language = lang;
            GameManager.I.SaveNow();
            Object.Destroy(dashboard);
            // Reload the current scene so every label is rebuilt in the new language.
            SceneLoader.Load(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        static RectTransform Row(Transform parent, string label)
        {
            var row = UIFactory.CreateRect("Row_" + label, parent);
            UIFactory.Size(row, -1, 84);
            var h = UIFactory.HLayout(row, 14);
            h.childForceExpandWidth = false;
            var l = UIFactory.Label(row, label, 34, UIFactory.Dark, TextAnchor.MiddleLeft);
            UIFactory.Size(l, 330, 80);
            return row;
        }

        static void Toggle(Transform parent, string label, System.Func<bool> get, System.Action<bool> set)
        {
            var row = Row(parent, label);
            Button b = null;
            b = UIFactory.Button(row, "", UIFactory.Grey, () =>
            {
                set(!get());
                GameManager.I.SaveNow();
                Refresh(b, get());
            }, 34);
            UIFactory.Size(b, 220, 80);
            Refresh(b, get());
        }

        static void Refresh(Button b, bool on)
        {
            ((Image)b.targetGraphic).color = on ? UIFactory.Green : UIFactory.Grey;
            UIFactory.ButtonText(b).text = Loc.Get(on ? "on" : "off");
        }
    }
}
