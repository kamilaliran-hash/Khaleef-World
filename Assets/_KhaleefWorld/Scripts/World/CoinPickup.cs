using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>A spinning coin. Collected by walking near it (distance check, no physics).</summary>
    public class CoinPickup : MonoBehaviour
    {
        public int value = 1;
        float baseY;

        void Start() { baseY = transform.position.y; }

        void Update()
        {
            transform.Rotate(0, 0, 180f * Time.deltaTime, Space.Self);
            var p = transform.position;
            p.y = baseY + Mathf.Sin(Time.time * 3f + p.x) * 0.12f;
            transform.position = p;

            var player = PlayerController.Local;
            if (player == null) return;
            Vector3 d = player.transform.position + Vector3.up * 0.8f - transform.position;
            if (d.sqrMagnitude < 1.1f)
            {
                GameManager.I.Rewards.AddCoins(value);
                GameManager.I.Audio.Play(Sfx.Coin);
                Destroy(gameObject);
            }
        }
    }
}
