using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Title screen. On first launch: choose language, then a grown-up picks the child's age band
    /// (behind the parental gate). Then Play goes to character creation or straight to the World Map.
    /// </summary>
    public class BootScreen : MonoBehaviour
    {
        RectTransform root;
        GameManager GM => GameManager.I;

        void Start()
        {
            var canvas = UIFactory.CreateCanvas("Boot", 10);
            root = (RectTransform)canvas.transform;
            if (!GM.Save.setupDone) ShowLanguage();
            else ShowTitle();
        }

        RectTransform Fresh()
        {
            UIFactory.Clear(root);
            var bg = UIFactory.Panel(root, new Color(0.98f, 0.85f, 0.35f), "Bg", false);
            UIFactory.Stretch(bg.rectTransform);
            var band = UIFactory.Panel(bg.transform, new Color(0.07f, 0.62f, 0.62f), "Band", false);
            UIFactory.Anchor(band.rectTransform, 0f, 0f, 1f, 0.22f);
            return bg.rectTransform;
        }

        void ShowLanguage()
        {
            var bg = Fresh();
            var t = UIFactory.Label(bg, "Pilih bahasa\n<i>Choose a language</i>", 70, UIFactory.Dark);
            UIFactory.Anchor(t.rectTransform, 0.1f, 0.6f, 0.9f, 0.92f);
            var row = UIFactory.CreateRect("Langs", bg);
            UIFactory.Anchor(row, 0.2f, 0.3f, 0.8f, 0.52f);
            UIFactory.HLayout(row, 60);
            UIFactory.Button(row, "Bahasa Melayu", UIFactory.Teal, () => { GM.Save.language = "ms"; ShowAgeBand(); }, 56);
            UIFactory.Button(row, "English", UIFactory.Blue, () => { GM.Save.language = "en"; ShowAgeBand(); }, 56);
        }

        void ShowAgeBand()
        {
            var bg = Fresh();
            var t = UIFactory.Label(bg, Loc.Get("for_grownups") + "\n" + Loc.Get("age_band"), 64, UIFactory.Dark);
            UIFactory.Anchor(t.rectTransform, 0.1f, 0.6f, 0.9f, 0.92f);
            var row = UIFactory.CreateRect("Ages", bg);
            UIFactory.Anchor(row, 0.12f, 0.3f, 0.88f, 0.52f);
            UIFactory.HLayout(row, 40);
            string[] labels = { "6 - 7", "8 - 9", "10 - 12" };
            Color[] colors = { UIFactory.Pink, UIFactory.Orange, UIFactory.Purple };
            for (int i = 0; i < 3; i++)
            {
                int band = i;
                UIFactory.Button(row, labels[i], colors[i], () => ParentalGate.Show(() =>
                {
                    GM.Difficulty.SetForAgeBand(band);
                    GM.Save.setupDone = true;
                    GM.SaveNow();
                    ShowTitle();
                }), 60);
            }
            var note = UIFactory.Label(bg, Loc.Get("privacy_note"), 30, Color.white);
            UIFactory.Anchor(note.rectTransform, 0.05f, 0.03f, 0.95f, 0.18f);
        }

        void ShowTitle()
        {
            var bg = Fresh();
            var title = UIFactory.Label(bg, "Khaleef World", 140, UIFactory.Purple);
            UIFactory.Anchor(title.rectTransform, 0.05f, 0.6f, 0.95f, 0.92f);
            title.gameObject.AddComponent<UnityEngine.UI.Shadow>().effectDistance = new Vector2(6, -6);
            var sub = UIFactory.Label(bg, Loc.Pick("Jelajah Malaysia, main & belajar!", "Explore Malaysia, play & learn!"), 52, UIFactory.Dark);
            UIFactory.Anchor(sub.rectTransform, 0.1f, 0.5f, 0.9f, 0.62f);

            var play = UIFactory.Button(bg, Loc.Get("play"), UIFactory.Green, () =>
                SceneLoader.Load(GM.Save.avatarCreated ? SceneLoader.WorldMap : SceneLoader.CharacterCreator), 80);
            UIFactory.Place((RectTransform)play.transform, new Vector2(0.5f, 0.36f), new Vector2(560, 190), Vector2.zero);

            var parents = UIFactory.Button(bg, Loc.Get("parents"), UIFactory.Grey, () => GM.Parental.OpenDashboard(), 34);
            UIFactory.Place((RectTransform)parents.transform, new Vector2(1, 0), new Vector2(280, 100), new Vector2(-170, 80));

            var ver = UIFactory.Label(bg, "v" + Application.version, 26, Color.white);
            UIFactory.Place(ver.rectTransform, new Vector2(0, 0), new Vector2(240, 60), new Vector2(140, 50));
        }
    }
}
