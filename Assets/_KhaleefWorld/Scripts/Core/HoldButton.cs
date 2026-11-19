using UnityEngine;
using UnityEngine.EventSystems;

namespace KhaleefWorld
{
    /// <summary>A UI element that reports whether it is currently being held down.</summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool Held { get; private set; }

        public void OnPointerDown(PointerEventData e) { Held = true; transform.localScale = Vector3.one * 0.94f; }
        public void OnPointerUp(PointerEventData e) { Release(); }
        public void OnPointerExit(PointerEventData e) { Release(); }
        void OnDisable() { Release(); }

        void Release() { Held = false; transform.localScale = Vector3.one; }
    }
}
