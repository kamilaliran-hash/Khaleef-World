using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>A friendly local who waves and says a short line in Bahasa Melayu and English.</summary>
    public class NpcGreeter : Interactable
    {
        public string npcName = "Kak Siti";
        public AvatarData look = new AvatarData();
        [TextArea] public string lineMS = "Selamat datang!";
        [TextArea] public string lineEN = "Welcome!";

        AvatarRig rig;

        public override string Prompt => Loc.Get("talk") + ": " + npcName;

        void Start()
        {
            radius = 2.6f;
            rig = AvatarBuilder.Build(look, transform);
        }

        void Update()
        {
            if (rig != null) rig.Animate(0f, Time.deltaTime);
            var p = PlayerController.Local;
            if (p != null && Vector3.Distance(p.transform.position, transform.position) < 6f)
            {
                Vector3 dir = p.transform.position - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 4f * Time.deltaTime);
            }
        }

        public override void Interact(PlayerController player)
        {
            if (rig != null) rig.Wave();
            UIFactory.Toast($"{npcName}: {lineMS}\n<i>{lineEN}</i>", 4f);
        }
    }
}
