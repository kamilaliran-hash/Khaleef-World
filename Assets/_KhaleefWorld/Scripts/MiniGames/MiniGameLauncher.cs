using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>Opens a mini-game as a full-screen overlay on top of the state zone and restores play afterwards.</summary>
    public static class MiniGameLauncher
    {
        public static MiniGameBase Active { get; private set; }

        public static void Launch(MiniGameDefinition def, StateDefinition state)
        {
            if (def == null || Active != null) return;

            var canvas = UIFactory.CreateCanvas("MiniGame_" + def.id, 100);
            MiniGameBase game;
            switch (def.template)
            {
                case MiniGameTemplate.MathsClimb: game = canvas.gameObject.AddComponent<MathsClimbGame>(); break;
                case MiniGameTemplate.MemoryMatch: game = canvas.gameObject.AddComponent<MemoryMatchGame>(); break;
                case MiniGameTemplate.WauKite: game = canvas.gameObject.AddComponent<WauKiteGame>(); break;
                default:
                    Object.Destroy(canvas.gameObject);
                    Debug.LogError("Unknown mini-game template " + def.template);
                    return;
            }

            var player = PlayerController.Local;
            if (player != null) { player.SetMove(Vector2.zero); player.InputEnabled = false; }
            if (PlayerHUD.Instance != null) PlayerHUD.Instance.SetVisible(false);

            Active = game;
            var canvasGo = canvas.gameObject;
            game.Completed += result =>
            {
                Active = null;
                Object.Destroy(canvasGo);
                if (result.playAgain) { Launch(def, state); return; }

                var p = PlayerController.Local;
                if (p != null) { p.SetMove(Vector2.zero); p.InputEnabled = true; }
                if (PlayerHUD.Instance != null) PlayerHUD.Instance.SetVisible(true);
                MiniGamePortal.RefreshAll();
            };
            game.Begin(def, state, (RectTransform)canvas.transform);
        }
    }
}
