using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>Floating joystick: appears wherever the thumb lands inside its area.</summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform baseRect;
        public RectTransform knob;
        public float radius = 120f;
        public Action<Vector2> onValue;

        RectTransform area;
        Image baseImage;
        Vector2 restPosition;

        void Awake()
        {
            area = (RectTransform)transform;
        }

        void Start()
        {
            baseImage = baseRect.GetComponent<Image>();
            restPosition = baseRect.anchoredPosition;
            SetAlpha(0.45f);
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, e.position, e.pressEventCamera, out var lp))
                baseRect.anchoredPosition = lp;
            knob.anchoredPosition = Vector2.zero;
            SetAlpha(0.9f);
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, e.position, e.pressEventCamera, out var lp)) return;
            Vector2 delta = Vector2.ClampMagnitude(lp - baseRect.anchoredPosition, radius);
            knob.anchoredPosition = delta;
            onValue?.Invoke(delta / radius);
        }

        public void OnPointerUp(PointerEventData e)
        {
            knob.anchoredPosition = Vector2.zero;
            baseRect.anchoredPosition = restPosition;
            SetAlpha(0.45f);
            onValue?.Invoke(Vector2.zero);
        }

        void OnDisable()
        {
            onValue?.Invoke(Vector2.zero);
        }

        void SetAlpha(float a)
        {
            if (baseImage != null) { var c = baseImage.color; c.a = a * 0.6f; baseImage.color = c; }
            var ki = knob != null ? knob.GetComponent<Image>() : null;
            if (ki != null) { var c = ki.color; c.a = a; ki.color = c; }
        }
    }
}
