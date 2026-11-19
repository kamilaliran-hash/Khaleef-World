using System;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Screen-time limits (with a gentle 5-minute warning) and the entry point to the Parent Dashboard.
    /// All sensitive actions go through ParentalGate.
    /// </summary>
    public class ParentalControlManager : MonoBehaviour
    {
        GameManager gm;
        bool warned;
        float extraSeconds;
        float dateCheckTimer;
        GameObject restOverlay;

        public void Init(GameManager g)
        {
            gm = g;
            warned = false;
            extraSeconds = 0f;
            CheckDate();
        }

        public ParentalSettings Settings => gm.Save.parental;

        public float MinutesPlayedToday => gm.Save.playSecondsToday / 60f;

        float LimitSeconds => Settings.dailyLimitMinutes <= 0 ? float.MaxValue : Settings.dailyLimitMinutes * 60f + extraSeconds;

        void CheckDate()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (gm.Save.playDate == today) return;
            gm.Save.playDate = today;
            gm.Save.playSecondsToday = 0f;
            warned = false;
            extraSeconds = 0f;
        }

        void Update()
        {
            if (gm == null || gm.Save == null || restOverlay != null) return;

            gm.Save.playSecondsToday += Time.unscaledDeltaTime;
            dateCheckTimer += Time.unscaledDeltaTime;
            if (dateCheckTimer > 30f) { dateCheckTimer = 0f; CheckDate(); }

            float remaining = LimitSeconds - gm.Save.playSecondsToday;
            if (!warned && remaining > 0f && remaining < 300f)
            {
                warned = true;
                UIFactory.Toast(Loc.Get("five_min_left"), 4f);
            }
            if (remaining <= 0f) ShowRestScreen();
        }

        void ShowRestScreen()
        {
            gm.SaveNow();
            Time.timeScale = 0f;
            var dim = UIFactory.Overlay("RestOverlay", 900, out var canvas);
            restOverlay = canvas.gameObject;
            var box = UIFactory.Dialog(dim, new Vector2(1200, 700));
            var title = UIFactory.Label(box, Loc.Get("time_to_rest"), 80, UIFactory.Purple);
            UIFactory.Anchor(title.rectTransform, 0.05f, 0.68f, 0.95f, 0.95f);
            var body = UIFactory.Label(box, Loc.Get("rest_body"), 46, UIFactory.Dark);
            UIFactory.Anchor(body.rectTransform, 0.08f, 0.35f, 0.92f, 0.68f);
            var btn = UIFactory.Button(box, Loc.Get("grownup_more_time"), UIFactory.Grey, () =>
                ParentalGate.Show(() => { extraSeconds += 15 * 60f; warned = false; CloseRest(); }), 36);
            UIFactory.Anchor((RectTransform)btn.transform, 0.2f, 0.07f, 0.8f, 0.28f);
        }

        void CloseRest()
        {
            if (restOverlay != null) Destroy(restOverlay);
            restOverlay = null;
            Time.timeScale = 1f;
        }

        /// <summary>Called by the "Parents" button anywhere in the game.</summary>
        public void OpenDashboard() => ParentalGate.Show(ParentDashboardScreen.Open);
    }
}
