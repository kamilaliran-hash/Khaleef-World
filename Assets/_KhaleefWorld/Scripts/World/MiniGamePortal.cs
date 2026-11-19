using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// A glowing ring in the world that launches a mini-game. Shows the game's title and best stars.
    /// Visuals are generated at runtime so designers only place the object and assign a MiniGameDefinition.
    /// </summary>
    public class MiniGamePortal : Interactable
    {
        public MiniGameDefinition miniGame;

        Transform icon;
        TextMesh label;
        Camera cam;

        public override string Prompt => miniGame != null ? Loc.Get("play") + "  " + miniGame.Title : "?";

        void Start()
        {
            radius = 2.8f;
            Color c = miniGame != null ? UIFactory.SubjectColor(miniGame.subject) : Color.white;

            AvatarBuilder.Cylinder(transform, new Vector3(0, 0.05f, 0), new Vector3(3f, 0.1f, 3f), c);
            AvatarBuilder.Cylinder(transform, new Vector3(0, 0.08f, 0), new Vector3(2.4f, 0.1f, 2.4f), Color.white);
            var iconGo = AvatarBuilder.Box(transform, new Vector3(0, 1.6f, 0), new Vector3(0.7f, 0.7f, 0.7f), c);
            icon = iconGo.transform;
            AvatarBuilder.Box(icon, Vector3.zero, new Vector3(1.25f, 0.35f, 0.35f), UIFactory.Yellow);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0, 2.9f, 0);
            label = labelGo.AddComponent<TextMesh>();
            label.font = UIFactory.DefaultFont;
            labelGo.GetComponent<MeshRenderer>().sharedMaterial = UIFactory.DefaultFont.material;
            label.fontSize = 64;
            label.characterSize = 0.09f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
            Refresh();
            cam = Camera.main;
        }

        public void Refresh()
        {
            if (label == null || miniGame == null) return;
            int stars = GameManager.I.Rewards.StarsFor(miniGame.id);
            string starText = new string('*', stars) + new string('-', 3 - stars);
            label.text = miniGame.Title + "\n" + starText;
        }

        void Update()
        {
            if (icon != null)
            {
                icon.Rotate(0, 60f * Time.deltaTime, 0, Space.World);
                icon.localPosition = new Vector3(0, 1.6f + Mathf.Sin(Time.time * 2f) * 0.15f, 0);
            }
            if (label != null)
            {
                if (cam == null) cam = Camera.main;
                if (cam != null) label.transform.rotation = Quaternion.LookRotation(label.transform.position - cam.transform.position);
            }
        }

        public override void Interact(PlayerController player)
        {
            if (miniGame == null) return;
            MiniGameLauncher.Launch(miniGame, StateZone.Current != null ? StateZone.Current.state : null);
        }

        public static void RefreshAll()
        {
            foreach (var it in All) if (it is MiniGamePortal p) p.Refresh();
        }
    }
}
