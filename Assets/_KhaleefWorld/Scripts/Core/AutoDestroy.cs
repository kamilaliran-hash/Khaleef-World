using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>Destroys its GameObject after a delay (unscaled time, so it works while paused).</summary>
    public class AutoDestroy : MonoBehaviour
    {
        public float seconds = 2f;

        void Update()
        {
            seconds -= Time.unscaledDeltaTime;
            if (seconds <= 0f) Destroy(gameObject);
        }
    }
}
