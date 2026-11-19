using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Orbit-follow camera: swipe to rotate, tap to cycle 3 zoom distances, gently recentres behind the
    /// player 2 seconds after the last swipe. Avoids clipping into walls with a sphere cast.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float[] distances = { 5f, 7.5f, 10.5f };
        public int zoomIndex = 1;
        public float height = 1.5f;
        public float yaw;
        public float pitch = 20f;
        public float minPitch = 5f, maxPitch = 65f;
        public float followSharpness = 12f;
        public float recenterDelay = 2f;
        public float recenterSpeed = 60f;

        float lastManual = -10f;
        float currentDistance = 7.5f;
        const int IgnoreRaycastLayer = 2;

        public void AddOrbit(Vector2 delta)
        {
            yaw += delta.x * 0.25f;
            pitch = Mathf.Clamp(pitch - delta.y * 0.15f, minPitch, maxPitch);
            lastManual = Time.time;
        }

        public void CycleZoom() => zoomIndex = (zoomIndex + 1) % distances.Length;

        public void SnapBehind()
        {
            if (target == null) return;
            yaw = target.eulerAngles.y;
            currentDistance = distances[zoomIndex];
            transform.position = DesiredPosition(out var focus);
            transform.LookAt(focus);
        }

        void LateUpdate()
        {
            if (target == null) return;
            float dt = Time.deltaTime;

            var player = PlayerController.Local;
            if (Time.time - lastManual > recenterDelay && player != null && player.IsMoving)
                yaw = Mathf.MoveTowardsAngle(yaw, target.eulerAngles.y, recenterSpeed * dt);

            currentDistance = Mathf.Lerp(currentDistance, distances[zoomIndex], 5f * dt);
            Vector3 desired = DesiredPosition(out var focusPoint);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSharpness * dt));
            transform.LookAt(focusPoint);
        }

        Vector3 DesiredPosition(out Vector3 focus)
        {
            focus = target.position + Vector3.up * height;
            var rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 dir = rot * Vector3.back;
            float dist = currentDistance;
            int mask = ~(1 << IgnoreRaycastLayer);
            if (Physics.SphereCast(focus, 0.25f, dir, out var hit, dist, mask, QueryTriggerInteraction.Ignore))
                dist = Mathf.Max(1f, hit.distance - 0.1f);
            return focus + dir * dist;
        }
    }
}
