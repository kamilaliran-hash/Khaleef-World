using System.Collections.Generic;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Anything the player can walk up to and use with the Action button (mini-game portals, NPCs, signs).
    /// Uses a simple distance check rather than physics triggers, which is cheap and reliable on mobile.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        public static readonly List<Interactable> All = new List<Interactable>();

        public float radius = 2.5f;

        protected virtual void OnEnable() { All.Add(this); }
        protected virtual void OnDisable() { All.Remove(this); }

        public abstract string Prompt { get; }
        public abstract void Interact(PlayerController player);
    }
}
