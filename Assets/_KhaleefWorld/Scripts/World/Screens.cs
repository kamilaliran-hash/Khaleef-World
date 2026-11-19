using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>The Passport book: one page per state, with its stamp and stars.</summary>
    public static class PassportScreen
    {
        public static void Open()
        {
            var gm = GameManager.I;
            var cfg = gm.Config;
            var dim = UIFactory.Overlay("Passport", 800, out var canvas);
            var go = canvas.gameObject;
            var book = UIFactory.Dialog(dim, new Vector2(1800, 1000), new Color(0.12f, 0.2f, 0.45f));

            var title = UIFactory.Label(book, Loc.Get("passport") + " Malaysia", 60, UIFactory.Yellow);
            UIFactory.Anchor(title.rectTransform, 0.03f, 0.88f, 0.8f, 0.98f);
            var close = UIFactory.Button(book, "X", UIFactory.Grey, () => Object.Destroy(go), 44);
            UIFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(100, 100), new Vector2(-70, -60));

            var grid = UIFactory.CreateRect("Pages", book);
            UIFactory.Anchor(grid, 0.02f, 0.03f, 0.98f, 0.87f);
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(400, 195);
            g.spacing = new Vector2(30, 18);
            g.childAlignment = TextAnchor.MiddleCenter;
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 4;

            if (cfg == null) return;
            foreach (var st in cfg.states)
            {
                if (st == null) continue;
                bool stamped = gm.Rewards.HasStamp(st);
                var page = UIFactory.Panel(grid, UIFactory.Cream, "Page_" + st.id);
                var stamp = UIFactory.Circle(page.transform, stamped ? st.color : new Color(0.85f, 0.85f, 0.85f), 130, "Stamp");
                stamp.rectTransform.anchorMin = stamp.rectTransform.anchorMax = new Vector2(0.2f, 0.5f);
                stamp.rectTransform.anchoredPosition = Vector2.zero;
                var code = UIFactory.Label(stamp.transform, stamped ? st.shortCode : "?", 40, Color.white);
                UIFactory.Stretch(code.rectTransform);

                var name = UIFactory.Label(page.transform, st.DisplayName, 32, UIFactory.Dark, TextAnchor.MiddleLeft);
                UIFactory.Anchor(name.rectTransform, 0.4f, 0.55f, 0.98f, 0.95f);
                int stars = gm.Rewards.StarsForState(st);
                int max = st.miniGames.Count * 3;
                string info = st.playable ? $"{Loc.Get("stars")}: {stars}/{max}" : Loc.Get("coming_soon");
                var sub = UIFactory.Label(page.transform, info, 26, UIFactory.Teal, TextAnchor.MiddleLeft);
                UIFactory.Anchor(sub.rectTransform, 0.4f, 0.1f, 0.98f, 0.5f);
            }
        }
    }

    /// <summary>Today's three quests.</summary>
    public static class QuestsScreen
    {
        public static void Open()
        {
            var gm = GameManager.I;
            var dim = UIFactory.Overlay("Quests", 800, out var canvas);
            var go = canvas.gameObject;
            var box = UIFactory.Dialog(dim, new Vector2(1300, 820));

            var title = UIFactory.Label(box, Loc.Get("quests"), 60, UIFactory.Teal);
            UIFactory.Anchor(title.rectTransform, 0.05f, 0.84f, 0.85f, 0.97f);
            var close = UIFactory.Button(box, "X", UIFactory.Grey, () => Object.Destroy(go), 44);
            UIFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(100, 100), new Vector2(-70, -60));

            var list = UIFactory.CreateRect("List", box);
            UIFactory.Anchor(list, 0.04f, 0.05f, 0.96f, 0.82f);
            UIFactory.VLayout(list, 20, 10);

            foreach (var q in gm.Quests.Today)
            {
                var row = UIFactory.Panel(list, q.claimed ? new Color(0.85f, 0.97f, 0.85f) : Color.white, "Quest");
                UIFactory.Size(row, -1, 170);
                var desc = UIFactory.Label(row.transform, gm.Quests.Describe(q), 40, UIFactory.Dark, TextAnchor.MiddleLeft);
                UIFactory.Anchor(desc.rectTransform, 0.04f, 0.45f, 0.75f, 0.95f);
                var barBg = UIFactory.Panel(row.transform, new Color(0.85f, 0.87f, 0.9f), "BarBg");
                UIFactory.Anchor(barBg.rectTransform, 0.04f, 0.15f, 0.72f, 0.38f);
                var fill = UIFactory.Panel(barBg.transform, UIFactory.Green, "Fill");
                UIFactory.Anchor(fill.rectTransform, 0f, 0f, Mathf.Max(0.04f, q.progress / (float)Mathf.Max(1, q.target)), 1f);
                var reward = UIFactory.Label(row.transform, q.claimed ? Loc.Get("quest_done") : "+" + q.reward + " " + Loc.Get("coins"), 34,
                    q.claimed ? UIFactory.Green : UIFactory.Orange);
                UIFactory.Anchor(reward.rectTransform, 0.75f, 0.1f, 0.98f, 0.9f);
            }
        }
    }
}
