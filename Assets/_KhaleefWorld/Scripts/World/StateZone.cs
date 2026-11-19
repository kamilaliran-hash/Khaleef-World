using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Root of every state scene. Spawns the player, camera rig and HUD, marks the state as visited and
    /// starts its music. Landmarks, portals, NPCs and coins are ordinary scene objects.
    /// </summary>
    public class StateZone : MonoBehaviour
    {
        public static StateZone Current { get; private set; }

        public StateDefinition state;
        public Transform spawnPoint;

        void Awake() { Current = this; }
        void OnDestroy() { if (Current == this) Current = null; }

        void Start()
        {
            var gm = GameManager.I;
            gm.CurrentState = state;
            if (state != null)
            {
                if (!gm.Save.visitedStates.Contains(state.id))
                {
                    gm.Save.visitedStates.Add(state.id);
                    gm.Quests.Notify(QuestType.VisitState, 1);
                }
                gm.Save.lastStateId = state.id;
            }

            // Player
            var playerGo = new GameObject("Player");
            if (spawnPoint != null) playerGo.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            else playerGo.transform.position = Vector3.up;
            var player = playerGo.AddComponent<PlayerController>();

            // Camera
            var camObj = Camera.main;
            if (camObj == null)
            {
                camObj = new GameObject("Main Camera").AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            var tpc = camObj.GetComponent<ThirdPersonCamera>();
            if (tpc == null) tpc = camObj.gameObject.AddComponent<ThirdPersonCamera>();
            tpc.target = playerGo.transform;
            player.cameraTransform = camObj.transform;
            player.RebuildAvatar();
            tpc.SnapBehind();

            // HUD
            var hud = new GameObject("HUD").AddComponent<PlayerHUD>();
            hud.Build(player, tpc, state);

            gm.Audio.PlayMusic(state != null ? state.music : null);
            gm.SaveNow();
        }
    }
}
