using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>
    /// In-world touch HUD: floating joystick (left), swipe-to-look (right), big Jump and Action buttons,
    /// and a slim top bar with coins, Map, Wardrobe, Passport, Quests and Parents.
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        public static PlayerHUD Instance { get; private set; }

        PlayerController player;
        ThirdPersonCamera cam;
        Canvas canvas;
        Text coinsLabel;
        Button actionButton;
        Text actionLabel;

        public void Build(PlayerController p, ThirdPersonCamera c, StateDefinition state)
        {
            Instance = this;
            player = p;
            cam = c;
            canvas = UIFactory.CreateCanvas("HUD", 10);
            canvas.transform.SetParent(transform, false);
            var root = canvas.transform;

            // Swipe area (right 60%) – added first so buttons sit on top of it
            var look = UIFactory.Panel(root, new Color(1, 1, 1, 0.01f), "LookArea", false);
            UIFactory.Anchor(look.rectTransform, 0.4f, 0f, 1f, 0.88f);
            look.gameObject.AddComponent<DragArea>().onDrag = d => cam.AddOrbit(d);

            // Joystick area (left 40%)
            var stickArea = UIFactory.Panel(root, new Color(1, 1, 1, 0.01f), "JoystickArea", false);
            UIFactory.Anchor(stickArea.rectTransform, 0f, 0f, 0.4f, 0.8f);
            var baseImg = UIFactory.Circle(stickArea.transform, Color.white, 260, "StickBase");
            baseImg.rectTransform.anchorMin = baseImg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            baseImg.rectTransform.anchoredPosition = new Vector2(-140, -170);
            baseImg.raycastTarget = false;
            var knob = UIFactory.Circle(baseImg.transform, UIFactory.Teal, 120, "Knob");
            knob.raycastTarget = false;
            var joystick = stickArea.gameObject.AddComponent<VirtualJoystick>();
            joystick.baseRect = baseImg.rectTransform;
            joystick.knob = knob.rectTransform;
            joystick.onValue = v => player.SetMove(v);

            // Jump
            var jump = UIFactory.Button(root, Loc.Get("jump"), UIFactory.Orange, () => player.Jump(), 40);
            ((Image)jump.targetGraphic).sprite = UIFactory.CircleSprite;
            ((Image)jump.targetGraphic).type = Image.Type.Simple;
            UIFactory.Place((RectTransform)jump.transform, new Vector2(1, 0), new Vector2(230, 230), new Vector2(-170, 170));

            // Action (only visible near something)
            actionButton = UIFactory.Button(root, "", UIFactory.Green, () => player.Interact(), 34);
            UIFactory.Place((RectTransform)actionButton.transform, new Vector2(1, 0), new Vector2(420, 140), new Vector2(-470, 120));
            actionLabel = UIFactory.ButtonText(actionButton);
            actionButton.gameObject.SetActive(false);

            // Top bar
            var coinPill = UIFactory.Panel(root, UIFactory.Yellow, "Coins");
            UIFactory.Place(coinPill.rectTransform, new Vector2(0, 1), new Vector2(300, 100), new Vector2(180, -70));
            coinsLabel = UIFactory.Label(coinPill.transform, "", 40, UIFactory.Dark);
            UIFactory.Stretch(coinsLabel.rectTransform, 6);

            if (state != null)
            {
                var stateLabel = UIFactory.Label(root, state.DisplayName, 46, Color.white);
                UIFactory.Place(stateLabel.rectTransform, new Vector2(0, 1), new Vector2(440, 100), new Vector2(580, -70));
                stateLabel.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3, -3);
            }

            var bar = UIFactory.CreateRect("TopButtons", root);
            UIFactory.Place(bar, new Vector2(1, 1), new Vector2(1060, 100), new Vector2(-560, -70));
            UIFactory.HLayout(bar, 12);
            UIFactory.Button(bar, Loc.Get("map"), UIFactory.Blue, () => SceneLoader.Load(SceneLoader.WorldMap), 30);
            UIFactory.Button(bar, Loc.Get("wardrobe"), UIFactory.Pink, () =>
            {
                CharacterCreatorScreen.ReturnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                SceneLoader.Load(SceneLoader.CharacterCreator);
            }, 30);
            UIFactory.Button(bar, Loc.Get("passport"), UIFactory.Purple, PassportScreen.Open, 30);
            UIFactory.Button(bar, Loc.Get("quests"), UIFactory.Teal, QuestsScreen.Open, 30);
            UIFactory.Button(bar, Loc.Get("parents"), UIFactory.Grey, () => GameManager.I.Parental.OpenDashboard(), 26);

            var zoom = UIFactory.Button(root, "+/-", UIFactory.Grey, () => cam.CycleZoom(), 34);
            UIFactory.Place((RectTransform)zoom.transform, new Vector2(1, 0), new Vector2(120, 100), new Vector2(-90, 360));

            player.NearbyChanged += OnNearbyChanged;
            GameManager.I.Rewards.CoinsChanged += OnCoinsChanged;
            OnCoinsChanged(GameManager.I.Rewards.Coins);

            if (state != null) ShowBanner(state);
        }

        void OnDestroy()
        {
            if (player != null) player.NearbyChanged -= OnNearbyChanged;
            if (GameManager.I != null && GameManager.I.Rewards != null) GameManager.I.Rewards.CoinsChanged -= OnCoinsChanged;
            if (Instance == this) Instance = null;
        }

        void OnCoinsChanged(int coins) { if (coinsLabel != null) coinsLabel.text = coins + " " + Loc.Get("coins"); }

        void OnNearbyChanged(Interactable it)
        {
            actionButton.gameObject.SetActive(it != null);
            if (it != null) actionLabel.text = it.Prompt;
        }

        public void SetVisible(bool visible)
        {
            if (canvas != null) canvas.gameObject.SetActive(visible);
        }

        void ShowBanner(StateDefinition state)
        {
            var box = UIFactory.Panel(canvas.transform, new Color(1f, 1f, 1f, 0.95f), "Welcome");
            UIFactory.Place(box.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(1300, 260), Vector2.zero);
            box.raycastTarget = false;
            var title = UIFactory.Label(box.transform, Loc.Format("welcome_to", state.DisplayName), 56, UIFactory.Purple);
            UIFactory.Anchor(title.rectTransform, 0.03f, 0.5f, 0.97f, 0.95f);
            var greet = UIFactory.Label(box.transform, $"{state.greetingMS}\n<i>{state.greetingEN}</i>", 34, UIFactory.Dark);
            UIFactory.Anchor(greet.rectTransform, 0.03f, 0.05f, 0.97f, 0.5f);
            box.gameObject.AddComponent<AutoDestroy>().seconds = 4f;
        }
    }
}
