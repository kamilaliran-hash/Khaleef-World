using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>
    /// Builds kid-friendly UI from code (big rounded buttons, chunky text) so the MVP needs no prefabs.
    /// Uses Unity's built-in font so it works in a fresh project; swap for a TextMeshPro font with
    /// Jawi / Chinese / Tamil glyphs when those languages are added.
    /// </summary>
    public static class UIFactory
    {
        public static readonly Color Teal = new Color(0.07f, 0.62f, 0.62f);
        public static readonly Color Orange = new Color(1f, 0.55f, 0.15f);
        public static readonly Color Yellow = new Color(1f, 0.82f, 0.2f);
        public static readonly Color Pink = new Color(0.95f, 0.38f, 0.58f);
        public static readonly Color Purple = new Color(0.52f, 0.36f, 0.85f);
        public static readonly Color Green = new Color(0.3f, 0.75f, 0.3f);
        public static readonly Color Red = new Color(0.9f, 0.3f, 0.28f);
        public static readonly Color Blue = new Color(0.2f, 0.5f, 0.95f);
        public static readonly Color Grey = new Color(0.62f, 0.64f, 0.68f);
        public static readonly Color Dark = new Color(0.13f, 0.15f, 0.22f);
        public static readonly Color Cream = new Color(1f, 0.98f, 0.93f);
        public static readonly Color Dim = new Color(0f, 0f, 0f, 0.6f);

        static Font _font;
        static Sprite _rounded, _circle;

        public static Font DefaultFont
        {
            get
            {
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static Sprite RoundedSprite { get { if (_rounded == null) _rounded = MakeRounded(64, 24); return _rounded; } }
        public static Sprite CircleSprite { get { if (_circle == null) _circle = MakeRounded(128, 64, false); return _circle; } }

        public static Color SubjectColor(Subject s)
        {
            switch (s)
            {
                case Subject.Maths: return Blue;
                case Subject.Language: return Pink;
                case Subject.Science: return Green;
                case Subject.History: return Orange;
                default: return Purple;
            }
        }

        // ---------- Canvas & layout ----------

        public static Canvas CreateCanvas(string name, int sortOrder)
        {
            EnsureEventSystem();
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.6f;
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt, float pad = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        /// <summary>Anchors a rect to a fraction of its parent (0..1 coordinates).</summary>
        public static void Anchor(RectTransform rt, float xMin, float yMin, float xMax, float yMax, float pad = 0f)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        /// <summary>Fixed-size rect positioned relative to an anchor point.</summary>
        public static void Place(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 offset)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
        }

        public static LayoutElement Size(Component c, float preferredWidth = -1, float preferredHeight = -1)
        {
            var le = c.GetComponent<LayoutElement>();
            if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
            if (preferredWidth >= 0) { le.preferredWidth = preferredWidth; le.minWidth = preferredWidth; }
            if (preferredHeight >= 0) { le.preferredHeight = preferredHeight; le.minHeight = preferredHeight; }
            return le;
        }

        public static VerticalLayoutGroup VLayout(Component c, float spacing = 12, int pad = 16, TextAnchor align = TextAnchor.UpperCenter)
        {
            var v = c.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(pad, pad, pad, pad);
            v.childAlignment = align;
            v.childControlWidth = true; v.childControlHeight = true;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            return v;
        }

        public static HorizontalLayoutGroup HLayout(Component c, float spacing = 12, int pad = 0, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var h = c.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.padding = new RectOffset(pad, pad, pad, pad);
            h.childAlignment = align;
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = true; h.childForceExpandHeight = true;
            return h;
        }

        // ---------- Widgets ----------

        public static Image Panel(Transform parent, Color color, string name = "Panel", bool rounded = true)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            if (rounded) { img.sprite = RoundedSprite; img.type = Image.Type.Sliced; }
            return img;
        }

        public static Image Circle(Transform parent, Color color, float size, string name = "Circle")
        {
            var rt = CreateRect(name, parent);
            rt.sizeDelta = new Vector2(size, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = CircleSprite;
            img.color = color;
            return img;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter, string name = "Label")
        {
            var rt = CreateRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = DefaultFont;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;   // required for best-fit shrinking
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = Mathf.Max(14, size / 2);
            t.resizeTextMaxSize = size;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.text = text;
            return t;
        }

        public static Button Button(Transform parent, string label, Color color, Action onClick, int fontSize = 44, string name = null)
        {
            var img = Panel(parent, color, name ?? ("Btn_" + label));
            var shadow = img.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.25f);
            shadow.effectDistance = new Vector2(0, -6);

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            btn.colors = colors;

            var t = Label(img.transform, label, fontSize, Color.white);
            Stretch(t.rectTransform, 10);
            btn.onClick.AddListener(() =>
            {
                if (GameManager.I != null && GameManager.I.Audio != null) GameManager.I.Audio.Play(Sfx.Tap);
                onClick?.Invoke();
            });
            return btn;
        }

        public static Text ButtonText(Button b) => b.GetComponentInChildren<Text>();

        /// <summary>A star shape drawn as a rotated square (no font glyph needed).</summary>
        public static Image Star(Transform parent, bool filled, float size)
        {
            var holder = CreateRect("Star", parent);
            holder.sizeDelta = new Vector2(size * 1.3f, size * 1.3f);
            Size(holder, size * 1.3f, size * 1.3f);
            var img = Panel(holder, filled ? Yellow : new Color(1, 1, 1, 0.35f), "Diamond", false);
            img.rectTransform.sizeDelta = new Vector2(size, size);
            img.rectTransform.anchorMin = img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            img.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            img.raycastTarget = false;
            return img;
        }

        /// <summary>A row of 3 stars, `count` of them filled.</summary>
        public static RectTransform StarRow(Transform parent, int count, float size)
        {
            var row = CreateRect("Stars", parent);
            var h = HLayout(row, size * 0.3f);
            h.childControlWidth = false; h.childControlHeight = false;
            h.childForceExpandWidth = false; h.childForceExpandHeight = false;
            for (int i = 0; i < 3; i++) Star(row, i < count, size);
            return row;
        }

        /// <summary>Full-screen dimmed overlay on its own canvas. Destroy `canvas.gameObject` to close.</summary>
        public static RectTransform Overlay(string name, int sortOrder, out Canvas canvas, bool dim = true)
        {
            canvas = CreateCanvas(name, sortOrder);
            var bg = Panel(canvas.transform, dim ? Dim : Color.clear, "Dim", false);
            Stretch(bg.rectTransform);
            return bg.rectTransform;
        }

        /// <summary>White rounded dialog box centred on a parent.</summary>
        public static RectTransform Dialog(Transform parent, Vector2 size, Color? color = null)
        {
            var box = Panel(parent, color ?? Cream, "Dialog");
            Place(box.rectTransform, new Vector2(0.5f, 0.5f), size, Vector2.zero);
            return box.rectTransform;
        }

        public static void Toast(string message, float seconds = 2.5f)
        {
            var canvas = CreateCanvas("Toast", 2000);
            canvas.GetComponent<GraphicRaycaster>().enabled = false;
            var box = Panel(canvas.transform, new Color(0.13f, 0.15f, 0.22f, 0.92f), "ToastBox");
            box.raycastTarget = false;
            Place(box.rectTransform, new Vector2(0.5f, 0.86f), new Vector2(1100, 130), Vector2.zero);
            var t = Label(box.transform, message, 42, Color.white);
            Stretch(t.rectTransform, 16);
            canvas.gameObject.AddComponent<AutoDestroy>().seconds = seconds;
        }

        public static void Confirm(string message, Action onYes, Action onNo = null)
        {
            var dim = Overlay("Confirm", 1500, out var canvas);
            var box = Dialog(dim, new Vector2(1000, 520));
            var t = Label(box, message, 46, Dark);
            Anchor(t.rectTransform, 0.05f, 0.4f, 0.95f, 0.95f);
            var row = CreateRect("Buttons", box);
            Anchor(row, 0.08f, 0.07f, 0.92f, 0.34f);
            HLayout(row, 40);
            var go = canvas.gameObject;
            Button(row, Loc.Get("no"), Grey, () => { UnityEngine.Object.Destroy(go); onNo?.Invoke(); });
            Button(row, Loc.Get("yes"), Green, () => { UnityEngine.Object.Destroy(go); onYes?.Invoke(); });
        }

        public static ScrollRect ScrollGrid(Transform parent, Vector2 cellSize, float spacing, out RectTransform content)
        {
            var root = CreateRect("Scroll", parent);
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.01f);
            var sr = root.gameObject.AddComponent<ScrollRect>();

            var viewport = CreateRect("Viewport", root);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            content.anchoredPosition = Vector2.zero;
            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = cellSize;
            grid.spacing = new Vector2(spacing, spacing);
            grid.padding = new RectOffset(16, 16, 16, 16);
            grid.childAlignment = TextAnchor.UpperCenter;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = viewport;
            sr.content = content;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 30f;
            return sr;
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        // ---------- Procedural sprites ----------

        static Sprite MakeRounded(int size, int radius, bool sliced = true)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[size * size];
            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, r, size - r);
                    float cy = Mathf.Clamp(y + 0.5f, r, size - r);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    byte a = (byte)(Mathf.Clamp01(r - d + 0.5f) * 255f);
                    px[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            var border = sliced ? new Vector4(radius, radius, radius, radius) : Vector4.zero;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }
    }
}
