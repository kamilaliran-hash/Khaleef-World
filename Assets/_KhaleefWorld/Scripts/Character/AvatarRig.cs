using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>Limb pivots of a blocky avatar, with a simple procedural walk / wave animation.</summary>
    public class AvatarRig : MonoBehaviour
    {
        public Transform head, body, armL, armR, legL, legR;
        float phase;
        float waveTimer;

        public void Animate(float speed01, float dt)
        {
            phase += dt * Mathf.Lerp(0f, 11f, speed01);
            float swing = Mathf.Sin(phase) * 38f * speed01;
            if (legL) legL.localRotation = Quaternion.Euler(-swing, 0, 0);
            if (legR) legR.localRotation = Quaternion.Euler(swing, 0, 0);

            if (waveTimer > 0f)
            {
                waveTimer -= dt;
                if (armR) armR.localRotation = Quaternion.Euler(0, 0, 150f + Mathf.Sin(waveTimer * 18f) * 20f);
                if (armL) armL.localRotation = Quaternion.Euler(swing, 0, 0);
            }
            else
            {
                if (armL) armL.localRotation = Quaternion.Euler(swing, 0, 0);
                if (armR) armR.localRotation = Quaternion.Euler(-swing, 0, 0);
            }

            // Idle breathing bob
            if (body) body.localScale = new Vector3(1f, 1f + Mathf.Sin(Time.time * 2.2f) * 0.015f, 1f);
        }

        public void Wave(float seconds = 1.6f) => waveTimer = seconds;
    }
}
