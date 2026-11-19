using System;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Camera-relative third-person movement for touch (virtual joystick + jump/action buttons),
    /// with a keyboard fallback for testing in the Editor (WASD / arrows, Space = jump, E = action).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Local { get; private set; }

        [Header("Movement")]
        public float moveSpeed = 4.5f;
        public float jumpHeight = 1.3f;
        public float gravity = -22f;
        public float turnSpeed = 720f;

        [Header("References")]
        public Transform cameraTransform;

        public bool InputEnabled { get; set; } = true;
        public bool IsMoving { get; private set; }
        public AvatarRig Rig { get; private set; }
        public Interactable Nearby { get; private set; }
        public event Action<Interactable> NearbyChanged;

        CharacterController cc;
        Vector2 stick;
        bool jumpQueued;
        float verticalVelocity;
        float coyoteTimer;
        Vector3 spawnPosition;
        Quaternion spawnRotation;

        void Awake()
        {
            Local = this;
            cc = GetComponent<CharacterController>();
            cc.height = 1.7f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0.86f, 0);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 50f;
            gameObject.layer = 2; // Ignore Raycast, so the camera's collision check skips the player
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        void OnDestroy()
        {
            if (Local == this) Local = null;
        }

        public void RebuildAvatar()
        {
            var data = GameManager.I.Save.avatar;
            Rig = AvatarBuilder.Build(data, transform);
            PetFollower.Spawn(transform, data);
        }

        // ---- Called by the HUD ----
        public void SetMove(Vector2 v) => stick = Vector2.ClampMagnitude(v, 1f);
        public void Jump() { if (InputEnabled) jumpQueued = true; }
        public void Interact() { if (InputEnabled && Nearby != null) Nearby.Interact(this); }

        void Update()
        {
            Vector2 input = Vector2.zero;
            if (InputEnabled) input = Vector2.ClampMagnitude(stick + KeyboardInput(), 1f);

            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = transform.forward;
            forward.Normalize();
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            Vector3 move = (forward * input.y + right * input.x) * moveSpeed;

            IsMoving = move.sqrMagnitude > 0.05f;
            if (IsMoving)
            {
                var targetRot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }

            if (cc.isGrounded)
            {
                coyoteTimer = 0.12f;
                if (verticalVelocity < 0f) verticalVelocity = -2f;
            }
            else coyoteTimer -= Time.deltaTime;

            if (jumpQueued && coyoteTimer > 0f)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                coyoteTimer = 0f;
                if (GameManager.I != null) GameManager.I.Audio.Play(Sfx.Jump);
            }
            jumpQueued = false;

            verticalVelocity += gravity * Time.deltaTime;
            cc.Move((move + Vector3.up * verticalVelocity) * Time.deltaTime);

            if (transform.position.y < -20f) Respawn();
            if (Rig != null) Rig.Animate(IsMoving ? input.magnitude : 0f, Time.deltaTime);

            UpdateNearby();
        }

        public void Respawn()
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            cc.enabled = true;
            verticalVelocity = 0f;
        }

        void UpdateNearby()
        {
            Interactable best = null;
            float bestDist = float.MaxValue;
            Vector3 p = transform.position;
            foreach (var it in Interactable.All)
            {
                if (it == null || !it.isActiveAndEnabled) continue;
                float d = Vector3.Distance(p, it.transform.position);
                if (d <= it.radius && d < bestDist) { best = it; bestDist = d; }
            }
            if (best != Nearby)
            {
                Nearby = best;
                NearbyChanged?.Invoke(best);
            }
        }

        Vector2 KeyboardInput()
        {
#if ENABLE_INPUT_SYSTEM
            var k = UnityEngine.InputSystem.Keyboard.current;
            if (k == null) return Vector2.zero;
            Vector2 v = Vector2.zero;
            if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1f;
            if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1f;
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1f;
            if (k.spaceKey.wasPressedThisFrame) jumpQueued = true;
            if (k.eKey.wasPressedThisFrame && Nearby != null) Nearby.Interact(this);
            return v;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Space)) jumpQueued = true;
            if (Input.GetKeyDown(KeyCode.E) && Nearby != null) Nearby.Interact(this);
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#else
            return Vector2.zero;
#endif
        }
    }
}
