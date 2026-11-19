using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KhaleefWorld
{
    /// <summary>Invisible UI area that reports finger drags (camera swipe, avatar preview rotation).</summary>
    public class DragArea : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public Action<Vector2> onDrag;
        public bool IsDragging { get; private set; }

        public void OnBeginDrag(PointerEventData e) { IsDragging = true; }
        public void OnEndDrag(PointerEventData e) { IsDragging = false; }

        public void OnDrag(PointerEventData e)
        {
            // Normalise to a 1080p-tall screen so swipe speed feels the same on every phone.
            float scale = 1080f / Mathf.Max(1, Screen.height);
            onDrag?.Invoke(e.delta * scale);
        }
    }
}
