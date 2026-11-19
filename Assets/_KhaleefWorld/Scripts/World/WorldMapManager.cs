using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>
    /// The World Map hub: all 13 states + 3 Federal Territories, unlock rules, the info card and the
    /// plane / ETS train / ferry / car travel animation that hides scene loading.
    /// </summary>
    public class WorldMapManager : MonoBehaviour
    {
        RectTransform root;
        RectTransform infoCard;
        GameManager GM => GameManager.I;

        // ---------- Rules ----------

        public static bool IsUnlocked(StateDefinition st)
        {
            var s = GameManager.I.Save;
            return s.parental.unlockAllStates || s.stamps.Count >= st.requiredStamps;
        }

        public static void TravelTo(StateDefinition st)
        {
            if (SceneLoader.IsLoading) return;
            if (!st.playable || !SceneLoader.Exists(st.sceneName)) { UIFactory.Toast(Loc.Get("coming_soon")); return; }
            if (!IsUnlocked(st)) { UIFactory.Toast(Loc.Format("need_stamps", st.requiredStamps)); return; }
            GameManager.I.StartCoroutine(TravelAnimation(st));
        }

        // ---------- UI ----------

        void Start()
        {
            GM.Audio.PlayMusic(null);
            BuildMap();
        }

        void BuildMap()
        {
            var canvas = UIFactory.CreateCanvas("WorldMap", 10);
            root = (RectTransform)canvas.transform;

            var sea = UIFactory.Panel(root, new Color(0.35f, 0.72f, 0.92f), "Sea", false);
            UIFactory.Stretch(sea.rectTransform);

            // Stylised land masses
            Land(0.06f, 0.06f, 0.37f, 0.93f, "Peninsula");
            Land(0.47f, 0.33f, 0.96f, 0.83f, "Borneo");

            var title = UIFactory.Label(root, "Peta Malaysia", 64, Color.white);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(700, 110), new Vector2(0, -70));
            title.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3, -3);

            // Top-right buttons
            var bar = UIFactory.CreateRect("Buttons", root);
            UIFactory.Place(bar, new Vector2(1, 1), new Vector2(860, 100), new Vector2(-460, -70));
            UIFactory.HLayout(bar, 12);
            UIFactory.Button(bar, Loc.Get("wardrobe"), UIFactory.Pink, () =>
            {
                CharacterCreatorScreen.ReturnScene = SceneLoader.WorldMap;
                SceneLoader.Load(SceneLoader.CharacterCreator);
            }, 30);
            UIFactory.Button(bar, Loc.Get("passport"), UIFactory.Purple, PassportScreen.Open, 30);
            UIFactory.Button(bar, Loc.Get("quests"), UIFactory.Teal, QuestsScreen.Open, 30);
            UIFactory.Button(bar, Loc.Get("parents"), UIFactory.Grey, () => GM.Parental.OpenDashboard(), 26);

            var coins = UIFactory.Panel(root, UIFactory.Yellow, "Coins");
            UIFactory.Place(coins.rectTransform, new Vector2(0, 1), new Vector2(300, 100), new Vector2(180, -70));
            var coinsText = UIFactory.Label(coins.transform, GM.Rewards.Coins + " " + Loc.Get("coins"), 40, UIFactory.Dark);
            UIFactory.Stretch(coinsText.rectTransform, 6);

            var cfg = GM.Config;
            int total = cfg != null ? cfg.states.Count : 16;
            var stamps = UIFactory.Label(root, $"{Loc.Get("passport")}: {GM.Save.stamps.Count}/{total}", 38, Color.white);
            UIFactory.Place(stamps.rectTransform, new Vector2(0, 1), new Vector2(420, 80), new Vector2(560, -70));

            if (cfg == null) return;
            foreach (var st in cfg.states) if (st != null) StateButton(st);

            // Info card
            infoCard = UIFactory.Panel(root, UIFactory.Cream, "InfoCard").rectTransform;
            UIFactory.Anchor(infoCard, 0.5f, 0.03f, 0.97f, 0.3f);
            infoCard.gameObject.SetActive(false);

            // Pre-select the last visited state
            var last = cfg.FindState(GM.Save.lastStateId);
            if (last != null) ShowInfo(last);
        }

        void Land(float x0, float y0, float x1, float y1, string name)
        {
            var land = UIFactory.Panel(root, new Color(0.45f, 0.78f, 0.38f), name);
            UIFactory.Anchor(land.rectTransform, x0, y0, x1, y1);
            land.raycastTarget = false;
            var sand = land.gameObject.AddComponent<Outline>();
            sand.effectColor = new Color(0.98f, 0.9f, 0.6f);
            sand.effectDistance = new Vector2(8, -8);
        }

        void StateButton(StateDefinition st)
        {
            bool unlocked = IsUnlocked(st);
            bool stamped = GM.Rewards.HasStamp(st);
            Color c = !st.playable ? new Color(0.7f, 0.72f, 0.75f) : unlocked ? st.color : new Color(0.55f, 0.55f, 0.6f);

            var holder = UIFactory.CreateRect("State_" + st.id, root);
            UIFactory.Place(holder, st.mapPosition, new Vector2(140, 140), Vector2.zero);

            if (stamped)
            {
                var ring = UIFactory.Circle(holder, UIFactory.Yellow, 112, "Ring");
                ring.rectTransform.anchoredPosition = new Vector2(0, 18);
                ring.raycastTarget = false;
            }
            var dot = UIFactory.Circle(holder, c, 92, "Dot");
            dot.rectTransform.anchoredPosition = new Vector2(0, 18);
            var btn = dot.gameObject.AddComponent<Button>();
            btn.targetGraphic = dot;
            btn.onClick.AddListener(() => { GM.Audio.Play(Sfx.Tap); ShowInfo(st); });
            var code = UIFactory.Label(dot.transform, st.shortCode, 28, Color.white);
            UIFactory.Stretch(code.rectTransform);

            var name = UIFactory.Label(holder, st.DisplayName, 24, UIFactory.Dark);
            UIFactory.Place(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(210, 40), new Vector2(0, 8));
        }

        void ShowInfo(StateDefinition st)
        {
            infoCard.gameObject.SetActive(true);
            UIFactory.Clear(infoCard);

            var name = UIFactory.Label(infoCard, st.DisplayName, 50, UIFactory.Purple, TextAnchor.MiddleLeft);
            UIFactory.Anchor(name.rectTransform, 0.03f, 0.62f, 0.62f, 0.96f);
            var landmark = UIFactory.Label(infoCard, st.Landmark, 32, UIFactory.Dark, TextAnchor.MiddleLeft);
            UIFactory.Anchor(landmark.rectTransform, 0.03f, 0.36f, 0.62f, 0.62f);

            string status;
            if (!st.playable) status = Loc.Get("coming_soon");
            else if (!IsUnlocked(st)) status = Loc.Format("need_stamps", st.requiredStamps);
            else status = $"{Loc.Get("stars")}: {GM.Rewards.StarsForState(st)}/{st.miniGames.Count * 3}";
            var stat = UIFactory.Label(infoCard, status, 32, UIFactory.Teal, TextAnchor.MiddleLeft);
            UIFactory.Anchor(stat.rectTransform, 0.03f, 0.06f, 0.62f, 0.36f);

            bool canGo = st.playable && IsUnlocked(st) && SceneLoader.Exists(st.sceneName);
            var go = UIFactory.Button(infoCard, canGo ? Loc.Get("go") : Loc.Get("locked"), canGo ? UIFactory.Green : UIFactory.Grey,
                () => TravelTo(st), 52);
            UIFactory.Anchor((RectTransform)go.transform, 0.65f, 0.15f, 0.97f, 0.85f);
        }

        // ---------- Travel animation ----------

        static IEnumerator TravelAnimation(StateDefinition st)
        {
            var dim = UIFactory.Overlay("Travel", 1200, out var canvas, false);
            var sky = UIFactory.Panel(dim, new Color(0.55f, 0.82f, 1f), "Sky", false);
            UIFactory.Stretch(sky.rectTransform);

            string key = st.travelMode == TravelMode.Plane ? "travel_plane" : st.travelMode == TravelMode.Train ? "travel_train"
                       : st.travelMode == TravelMode.Ferry ? "travel_ferry" : "travel_car";
            var text = UIFactory.Label(sky.transform, Loc.Get(key) + "\n" + st.DisplayName, 60, UIFactory.Dark);
            UIFactory.Anchor(text.rectTransform, 0.1f, 0.68f, 0.9f, 0.92f);

            // Ground / sea strip
            bool water = st.travelMode == TravelMode.Ferry;
            bool air = st.travelMode == TravelMode.Plane;
            var ground = UIFactory.Panel(sky.transform, water ? new Color(0.2f, 0.55f, 0.85f) : new Color(0.45f, 0.75f, 0.35f), "Ground", false);
            UIFactory.Anchor(ground.rectTransform, 0f, 0f, 1f, 0.25f);

            // Vehicle built from blocks
            var vehicle = UIFactory.CreateRect("Vehicle", sky.transform);
            vehicle.anchorMin = vehicle.anchorMax = new Vector2(0f, air ? 0.5f : 0.3f);
            vehicle.sizeDelta = new Vector2(360, 160);
            BuildVehicle(vehicle, st.travelMode);

            var clouds = new RectTransform[4];
            for (int i = 0; i < clouds.Length; i++)
            {
                clouds[i] = UIFactory.Panel(sky.transform, Color.white, "Cloud").rectTransform;
                clouds[i].anchorMin = clouds[i].anchorMax = new Vector2(Random.value, 0.45f + Random.value * 0.2f);
                clouds[i].sizeDelta = new Vector2(220 + Random.value * 120, 80);
            }

            var barBg = UIFactory.Panel(sky.transform, new Color(1, 1, 1, 0.6f), "BarBg");
            UIFactory.Anchor(barBg.rectTransform, 0.3f, 0.05f, 0.7f, 0.1f);
            var fill = UIFactory.Panel(barBg.transform, UIFactory.Orange, "Fill");
            UIFactory.Anchor(fill.rectTransform, 0f, 0f, 0.02f, 1f);
            Object.DontDestroyOnLoad(canvas.gameObject);

            float progress = 0f;
            SceneLoader.Load(st.sceneName, p => progress = p, 3.5f);

            float t = 0f;
            while (SceneLoader.IsLoading || t < 0.2f)
            {
                t += Time.unscaledDeltaTime;
                float x = Mathf.Repeat(t * 0.32f, 1.3f) - 0.15f;
                vehicle.anchorMin = vehicle.anchorMax = new Vector2(x, vehicle.anchorMin.y);
                vehicle.anchoredPosition = new Vector2(0, Mathf.Sin(t * (air ? 3f : 12f)) * (air ? 25f : 4f));
                foreach (var c in clouds)
                {
                    var a = c.anchorMin;
                    a.x = Mathf.Repeat(a.x - Time.unscaledDeltaTime * 0.15f, 1.2f);
                    c.anchorMin = c.anchorMax = a;
                }
                fill.rectTransform.anchorMax = new Vector2(Mathf.Max(0.02f, progress), 1f);
                yield return null;
            }
            Object.Destroy(canvas.gameObject);
        }

        static void BuildVehicle(RectTransform v, TravelMode mode)
        {
            Image Part(Color c, float x0, float y0, float x1, float y1)
            {
                var p = UIFactory.Panel(v, c, "Part");
                UIFactory.Anchor(p.rectTransform, x0, y0, x1, y1);
                return p;
            }
            switch (mode)
            {
                case TravelMode.Plane:
                    Part(Color.white, 0.05f, 0.35f, 0.95f, 0.65f);
                    Part(UIFactory.Red, 0.4f, 0.0f, 0.6f, 0.4f);
                    Part(UIFactory.Red, 0.4f, 0.6f, 0.6f, 1.0f);
                    Part(UIFactory.Blue, 0.02f, 0.55f, 0.14f, 0.95f);
                    for (int i = 0; i < 5; i++) Part(UIFactory.Blue, 0.25f + i * 0.12f, 0.48f, 0.31f + i * 0.12f, 0.58f);
                    break;
                case TravelMode.Train:
                    Part(new Color(0.95f, 0.95f, 0.95f), 0.0f, 0.2f, 1.0f, 0.85f);
                    Part(UIFactory.Blue, 0.0f, 0.2f, 1.0f, 0.35f);
                    Part(UIFactory.Yellow, 0.85f, 0.45f, 0.97f, 0.75f);
                    for (int i = 0; i < 4; i++) Part(UIFactory.Dark, 0.05f + i * 0.2f, 0.5f, 0.17f + i * 0.2f, 0.75f);
                    break;
                case TravelMode.Ferry:
                    Part(UIFactory.Red, 0.0f, 0.1f, 1.0f, 0.4f);
                    Part(Color.white, 0.15f, 0.4f, 0.85f, 0.7f);
                    Part(UIFactory.Yellow, 0.6f, 0.7f, 0.72f, 0.95f);
                    break;
                default:
                    Part(UIFactory.Orange, 0.05f, 0.25f, 0.95f, 0.6f);
                    Part(UIFactory.Orange, 0.25f, 0.6f, 0.75f, 0.9f);
                    Part(UIFactory.Dark, 0.12f, 0.0f, 0.3f, 0.3f);
                    Part(UIFactory.Dark, 0.7f, 0.0f, 0.88f, 0.3f);
                    break;
            }
        }
    }
}
