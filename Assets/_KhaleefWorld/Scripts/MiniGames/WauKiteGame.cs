using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>
    /// SAMPLE MINI-GAME 3 – "Wau Maker" in Kelantan (science: wind, lift and stability).
    /// Phase 1 – Build: pick a wau type, frame size and tail length. Bigger frames give more lift;
    ///           longer tails give more stability but cost some lift. Live bars show the trade-off.
    /// Phase 2 – Fly: hold "Tarik!" to pull the string and climb on the wind; let go to glide down.
    ///           Keep the wau inside the golden zone long enough to win. Gusts push it sideways –
    ///           a stable wau shrugs them off, an unstable one can spin into the ground.
    ///
    ///   Easy   – wide zone, gentle gusts, build gets a stability assist, 15 s target
    ///   Medium – normal zone and gusts, 20 s target
    ///   Hard   – narrow zone that slowly drifts, strong gusts, 25 s target
    /// </summary>
    public class WauKiteGame : MiniGameBase
    {
        static readonly string[] WauTypes = { "Wau Bulan", "Wau Kucing", "Wau Merak" };
        static readonly Color[] WauColors = { new Color(0.2f, 0.55f, 0.95f), new Color(0.95f, 0.45f, 0.2f), new Color(0.25f, 0.7f, 0.45f) };
        static readonly float[] TypeLift = { 0.05f, 0f, 0.08f };
        static readonly float[] TypeStability = { 0.05f, 0.1f, 0f };
        static readonly string[] SizeMS = { "Kecil", "Sederhana", "Besar" };
        static readonly string[] SizeEN = { "Small", "Medium", "Large" };
        static readonly string[] TailMS = { "Pendek", "Sederhana", "Panjang" };
        static readonly string[] TailEN = { "Short", "Medium", "Long" };

        int wauType, frame = 1, tail = 1;
        float lift, stability;
        bool flying;

        // Build UI
        RectTransform buildPanel, previewKite;
        Image liftBar, stabBar;

        // Fly state
        RectTransform sky, kite, line, zone, anchorPoint;
        Image zoneFill;
        Text timerLabel;
        HoldButton pullButton;
        Vector2 pos, vel;
        float wind = 0.7f, windTarget = 0.7f, gustTimer, timeLeft, zoneTime, targetZoneTime, checkpoint;
        float zoneMin, zoneMax, zoneDrift;
        int crashes;
        bool airborne;
        float noiseSeed;

        protected override void OnBegin()
        {
            noiseSeed = Random.value * 100f;
            BuildPhase();
        }

        // ---------- Phase 1: build ----------

        void BuildPhase()
        {
            SetInstruction("Bina wau kamu! Rangka besar = lebih daya angkat. Ekor panjang = lebih stabil.",
                           "Build your wau! Big frame = more lift. Long tail = more stable.");
            buildPanel = UIFactory.CreateRect("Build", Content);
            UIFactory.Stretch(buildPanel);

            var previewBg = UIFactory.Panel(buildPanel, new Color(0.6f, 0.85f, 1f), "PreviewSky");
            UIFactory.Anchor(previewBg.rectTransform, 0f, 0f, 0.4f, 1f);
            previewKite = UIFactory.CreateRect("PreviewKite", previewBg.transform);
            previewKite.anchorMin = previewKite.anchorMax = new Vector2(0.5f, 0.6f);
            previewKite.sizeDelta = new Vector2(320, 320);

            var options = UIFactory.CreateRect("Options", buildPanel);
            UIFactory.Anchor(options, 0.42f, 0f, 1f, 1f);
            UIFactory.VLayout(options, 14, 8);

            Picker(options, Loc.Pick("Jenis wau", "Wau type"), () => WauTypes[wauType], d => wauType = (wauType + d + 3) % 3);
            Picker(options, Loc.Pick("Saiz rangka", "Frame size"), () => Loc.Pick(SizeMS[frame], SizeEN[frame]), d => frame = Mathf.Clamp(frame + d, 0, 2));
            Picker(options, Loc.Pick("Panjang ekor", "Tail length"), () => Loc.Pick(TailMS[tail], TailEN[tail]), d => tail = Mathf.Clamp(tail + d, 0, 2));

            liftBar = Bar(options, Loc.Pick("Daya angkat", "Lift"), UIFactory.Orange);
            stabBar = Bar(options, Loc.Pick("Kestabilan", "Stability"), UIFactory.Teal);

            var fly = UIFactory.Button(options, Loc.Pick("Terbangkan!", "Fly!"), UIFactory.Green, StartFlying, 56);
            UIFactory.Size(fly, -1, 130);

            Recalculate();
        }

        void Picker(Transform parent, string label, System.Func<string> value, System.Action<int> change)
        {
            var row = UIFactory.CreateRect("Picker", parent);
            UIFactory.Size(row, -1, 105);
            var l = UIFactory.Label(row, label, 34, UIFactory.Dark, TextAnchor.MiddleLeft);
            UIFactory.Anchor(l.rectTransform, 0f, 0f, 0.32f, 1f);
            var valueText = UIFactory.Label(row, value(), 36, UIFactory.Purple);
            UIFactory.Anchor(valueText.rectTransform, 0.46f, 0f, 0.84f, 1f);
            var prev = UIFactory.Button(row, "<", UIFactory.Blue, () => { change(-1); valueText.text = value(); Recalculate(); }, 48);
            UIFactory.Anchor((RectTransform)prev.transform, 0.33f, 0.08f, 0.45f, 0.92f);
            var next = UIFactory.Button(row, ">", UIFactory.Blue, () => { change(1); valueText.text = value(); Recalculate(); }, 48);
            UIFactory.Anchor((RectTransform)next.transform, 0.86f, 0.08f, 0.98f, 0.92f);
        }

        Image Bar(Transform parent, string label, Color color)
        {
            var row = UIFactory.CreateRect("Bar", parent);
            UIFactory.Size(row, -1, 70);
            var l = UIFactory.Label(row, label, 32, UIFactory.Dark, TextAnchor.MiddleLeft);
            UIFactory.Anchor(l.rectTransform, 0f, 0f, 0.32f, 1f);
            var bg = UIFactory.Panel(row, new Color(0.85f, 0.87f, 0.9f), "BarBg");
            UIFactory.Anchor(bg.rectTransform, 0.33f, 0.2f, 0.98f, 0.8f);
            var fill = UIFactory.Panel(bg.transform, color, "Fill");
            UIFactory.Anchor(fill.rectTransform, 0f, 0f, 0.5f, 1f);
            return fill;
        }

        void Recalculate()
        {
            lift = Mathf.Clamp(0.45f + frame * 0.2f - tail * 0.08f + TypeLift[wauType], 0.1f, 1f);
            stability = Mathf.Clamp(0.35f + tail * 0.22f - frame * 0.05f + TypeStability[wauType], 0.1f, 1f);
            if (CurrentDifficulty == Difficulty.Easy) stability = Mathf.Min(1f, stability + 0.25f);

            liftBar.rectTransform.anchorMax = new Vector2(lift, 1f);
            stabBar.rectTransform.anchorMax = new Vector2(stability, 1f);
            DrawKite(previewKite, 0.6f + frame * 0.2f, tail);
        }

        /// <summary>Draws a wau from blocks: diamond body, crescent wings, tail ribbons.</summary>
        void DrawKite(RectTransform holder, float scale, int tailLength)
        {
            UIFactory.Clear(holder);
            Color c = WauColors[wauType];
            float s = 140f * scale;

            var body = UIFactory.Panel(holder, c, "Body", false);
            body.rectTransform.sizeDelta = new Vector2(s, s);
            body.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            var inner = UIFactory.Panel(body.transform, UIFactory.Yellow, "Pattern", false);
            UIFactory.Anchor(inner.rectTransform, 0.3f, 0.3f, 0.7f, 0.7f);

            var wings = UIFactory.Panel(holder, Color.Lerp(c, Color.white, 0.3f), "Wings");
            wings.rectTransform.sizeDelta = new Vector2(s * 2.1f, s * 0.35f);
            wings.rectTransform.anchoredPosition = new Vector2(0, s * 0.25f);

            if (wauType == 0) // Wau Bulan: crescent "moon" tail section
            {
                var moon = UIFactory.Panel(holder, c, "Moon");
                moon.rectTransform.sizeDelta = new Vector2(s * 1.4f, s * 0.3f);
                moon.rectTransform.anchoredPosition = new Vector2(0, -s * 0.75f);
            }

            int ribbons = 2 + tailLength * 2;
            for (int i = 0; i < ribbons; i++)
            {
                var r = UIFactory.Panel(holder, i % 2 == 0 ? UIFactory.Red : UIFactory.Yellow, "Tail", false);
                r.rectTransform.sizeDelta = new Vector2(18, 26);
                r.rectTransform.anchoredPosition = new Vector2(Mathf.Sin(i * 0.9f) * 12f, -s * 0.85f - i * 28f);
            }
            foreach (var g in holder.GetComponentsInChildren<Graphic>()) g.raycastTarget = false;
        }

        // ---------- Phase 2: fly ----------

        void StartFlying()
        {
            Destroy(buildPanel.gameObject);
            flying = true;
            SetInstruction("Tekan dan tahan TARIK untuk naik. Kekalkan wau dalam zon emas!",
                           "Hold PULL to climb. Keep the wau inside the golden zone!");

            switch (CurrentDifficulty)
            {
                case Difficulty.Easy: zoneMin = 0.42f; zoneMax = 0.85f; targetZoneTime = 15f; timeLeft = 50f; break;
                case Difficulty.Medium: zoneMin = 0.52f; zoneMax = 0.82f; targetZoneTime = 20f; timeLeft = 50f; break;
                default: zoneMin = 0.58f; zoneMax = 0.78f; targetZoneTime = 25f; timeLeft = 45f; break;
            }

            sky = UIFactory.Panel(Content, new Color(0.55f, 0.82f, 1f), "Sky").rectTransform;
            UIFactory.Anchor(sky, 0f, 0f, 0.8f, 1f);

            zone = UIFactory.Panel(sky, new Color(1f, 0.85f, 0.2f, 0.35f), "GoldenZone", false).rectTransform;
            UpdateZoneRect();

            var ground = UIFactory.Panel(sky, new Color(0.45f, 0.75f, 0.35f), "Field", false);
            UIFactory.Anchor(ground.rectTransform, 0f, 0f, 1f, 0.07f);

            anchorPoint = UIFactory.CreateRect("Anchor", sky);
            anchorPoint.anchorMin = anchorPoint.anchorMax = new Vector2(0.5f, 0.06f);

            line = UIFactory.Panel(sky, new Color(1f, 1f, 1f, 0.9f), "String", false).rectTransform;
            line.anchorMin = line.anchorMax = new Vector2(0.5f, 0.06f);
            line.pivot = new Vector2(0.5f, 0f);
            line.GetComponent<Image>().raycastTarget = false;

            kite = UIFactory.CreateRect("Kite", sky);
            kite.anchorMin = kite.anchorMax = Vector2.zero;
            kite.sizeDelta = new Vector2(200, 200);
            DrawKite(kite, 0.45f + frame * 0.1f, tail);

            // Side panel: timer, progress, pull button
            var side = UIFactory.CreateRect("Side", Content);
            UIFactory.Anchor(side, 0.82f, 0f, 1f, 1f);

            var timerBox = UIFactory.Panel(side, Color.white, "Timer");
            UIFactory.Anchor(timerBox.rectTransform, 0f, 0.84f, 1f, 1f);
            timerLabel = UIFactory.Label(timerBox.transform, "", 40, UIFactory.Dark);
            UIFactory.Stretch(timerLabel.rectTransform, 6);

            var progBg = UIFactory.Panel(side, new Color(0.85f, 0.87f, 0.9f), "ZoneProgress");
            UIFactory.Anchor(progBg.rectTransform, 0.3f, 0.42f, 0.7f, 0.8f);
            zoneFill = UIFactory.Panel(progBg.transform, UIFactory.Yellow, "Fill");
            UIFactory.Anchor(zoneFill.rectTransform, 0f, 0f, 1f, 0.02f);

            var pull = UIFactory.Circle(side, UIFactory.Orange, 300, "Pull");
            pull.rectTransform.anchorMin = pull.rectTransform.anchorMax = new Vector2(0.5f, 0.18f);
            var pullText = UIFactory.Label(pull.transform, Loc.Pick("TARIK!", "PULL!"), 50, Color.white);
            UIFactory.Stretch(pullText.rectTransform);
            pullButton = pull.gameObject.AddComponent<HoldButton>();

            ResetKite();
        }

        void ResetKite()
        {
            pos = new Vector2(0.5f, 0.12f);
            vel = Vector2.zero;
            airborne = false;
        }

        void UpdateZoneRect()
        {
            zone.anchorMin = new Vector2(0f, zoneMin);
            zone.anchorMax = new Vector2(1f, zoneMax);
            zone.offsetMin = zone.offsetMax = Vector2.zero;
        }

        protected override void OnTick(float dt)
        {
            if (!flying) return;
            timeLeft -= dt;

            // Wind: slowly changing strength plus occasional gusts
            gustTimer -= dt;
            if (gustTimer <= 0f)
            {
                windTarget = Random.Range(0.45f, 1f);
                gustTimer = Random.Range(1.5f, 3.5f);
            }
            wind = Mathf.Lerp(wind, windTarget, dt * 1.5f);
            float gustStrength = CurrentDifficulty == Difficulty.Easy ? 0.6f : CurrentDifficulty == Difficulty.Medium ? 1f : 1.4f;
            float side = (Mathf.PerlinNoise(Time.time * 0.7f, noiseSeed) - 0.5f) * 2f * (1.15f - stability) * gustStrength;

            bool pulling = pullButton != null && pullButton.Held;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.spaceKey.isPressed) pulling = true;
#endif
            float up = pulling ? wind * lift * 1.6f : 0f;
            float sink = 0.32f * (1.1f - lift * 0.5f);
            vel.y += (up - sink) * dt * 1.3f;
            vel.y *= 1f - 1.5f * dt;
            vel.x += side * dt * 0.9f + (0.5f - pos.x) * stability * 1.6f * dt;   // string pulls back to centre
            vel.x *= 1f - 1.2f * dt;
            pos += vel * dt;
            if (pos.y > 0.95f) { pos.y = 0.95f; vel.y = Mathf.Min(vel.y, 0f); }
            if (pos.y > 0.2f) airborne = true;

            if (CurrentDifficulty == Difficulty.Hard)
            {
                zoneDrift += dt * 0.25f;
                float offset = Mathf.Sin(zoneDrift) * 0.08f;
                zoneMin = 0.58f + offset; zoneMax = 0.78f + offset;
                UpdateZoneRect();
            }

            // Crash: fell to the ground or blown off the side
            if ((airborne && pos.y < 0.07f) || pos.x < 0.03f || pos.x > 0.97f)
            {
                crashes++;
                RegisterAnswer(false);
                ShowHint(stability < 0.5f
                    ? Loc.Pick("Wau tidak stabil! Cuba ekor yang lebih panjang.", "Wobbly wau! Try a longer tail next time.")
                    : Loc.Pick("Tarik lebih lama bila angin kuat!", "Pull longer when the wind is strong!"));
                ResetKite();
            }

            bool inZone = pos.y >= zoneMin && pos.y <= zoneMax;
            if (inZone)
            {
                zoneTime += dt;
                checkpoint += dt;
                if (checkpoint >= 5f) { checkpoint = 0f; RegisterAnswer(true); }
            }

            DrawFrame(inZone);

            if (zoneTime >= targetZoneTime || timeLeft <= 0f)
            {
                flying = false;
                Finish();
            }
        }

        void DrawFrame(bool inZone)
        {
            Vector2 size = sky.rect.size;
            Vector2 p = new Vector2(pos.x * size.x, pos.y * size.y);
            kite.anchoredPosition = p;
            float wobble = Mathf.Sin(Time.time * 6f) * (1f - stability) * 15f;
            kite.localRotation = Quaternion.Euler(0, 0, -vel.x * 120f + wobble);

            Vector2 a = new Vector2(0.5f * size.x, 0.06f * size.y);
            Vector2 d = p - a;
            line.sizeDelta = new Vector2(4f, d.magnitude);
            line.localRotation = Quaternion.Euler(0, 0, -Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg);

            zoneFill.rectTransform.anchorMax = new Vector2(1f, Mathf.Clamp01(zoneTime / targetZoneTime));
            ((Image)zone.GetComponent<Image>()).color = inZone ? new Color(1f, 0.85f, 0.2f, 0.55f) : new Color(1f, 0.85f, 0.2f, 0.3f);
            timerLabel.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)) + " s";
        }

        protected override int CalculateStars()
        {
            float progress = zoneTime / Mathf.Max(1f, targetZoneTime);
            if (progress >= 1f && crashes <= 1) return 3;
            if (progress >= 1f || progress >= 0.6f) return 2;
            return 1;
        }

        protected override float Accuracy
        {
            get
            {
                float progress = Mathf.Clamp01(zoneTime / Mathf.Max(1f, targetZoneTime));
                return Mathf.Clamp01(progress - crashes * 0.1f);
            }
        }
    }
}
