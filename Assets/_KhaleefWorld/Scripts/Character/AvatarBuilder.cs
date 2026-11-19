using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>
    /// Builds a blocky, Roblox-style avatar from cubes at runtime, using AvatarData + CosmeticItems.
    /// No art assets needed for the MVP; any CosmeticItem with a prefabOverride uses real art instead.
    /// The avatar faces +Z. Total height is ~1.7 units at medium height.
    /// </summary>
    public static class AvatarBuilder
    {
        public static readonly Color[] SkinTones =
        {
            new Color(1.00f, 0.88f, 0.77f), new Color(0.98f, 0.82f, 0.68f), new Color(0.95f, 0.77f, 0.60f),
            new Color(0.91f, 0.72f, 0.55f), new Color(0.86f, 0.65f, 0.47f), new Color(0.80f, 0.58f, 0.40f),
            new Color(0.73f, 0.51f, 0.34f), new Color(0.65f, 0.44f, 0.29f), new Color(0.56f, 0.37f, 0.24f),
            new Color(0.47f, 0.30f, 0.19f), new Color(0.38f, 0.24f, 0.15f), new Color(0.29f, 0.18f, 0.11f),
        };

        public static readonly Color[] HairColors =
        {
            new Color(0.08f, 0.07f, 0.07f), new Color(0.22f, 0.14f, 0.09f), new Color(0.40f, 0.25f, 0.13f),
            new Color(0.62f, 0.42f, 0.20f), new Color(0.85f, 0.70f, 0.38f), new Color(0.55f, 0.18f, 0.10f),
            new Color(0.75f, 0.75f, 0.78f), new Color(1f, 1f, 1f), new Color(0.95f, 0.40f, 0.60f),
            new Color(0.35f, 0.55f, 0.95f), new Color(0.35f, 0.80f, 0.45f), new Color(0.60f, 0.35f, 0.90f),
            new Color(1f, 0.55f, 0.15f), new Color(0.15f, 0.70f, 0.75f), new Color(0.95f, 0.85f, 0.25f),
            new Color(0.85f, 0.20f, 0.25f),
        };

        public static readonly string[] HairStyleNamesMS = { "Pendek", "Panjang", "Sanggul", "Pacak", "Kerinting", "Ekor kuda", "Botak" };
        public static readonly string[] HairStyleNamesEN = { "Short", "Long", "Bun", "Spiky", "Curly", "Ponytail", "Bald" };
        public const int EyeStyles = 4;
        public const int MouthStyles = 4;
        public static int HairStyles => HairStyleNamesEN.Length;

        static Mesh _cube, _sphere, _cylinder;
        static Mesh CubeMesh => _cube != null ? _cube : (_cube = PrimitiveMesh(PrimitiveType.Cube));
        static Mesh SphereMesh => _sphere != null ? _sphere : (_sphere = PrimitiveMesh(PrimitiveType.Sphere));
        static Mesh CylinderMesh => _cylinder != null ? _cylinder : (_cylinder = PrimitiveMesh(PrimitiveType.Cylinder));

        static readonly Color EyeColor = new Color(0.1f, 0.1f, 0.15f);
        static readonly Color MouthColor = new Color(0.65f, 0.2f, 0.25f);

        public static AvatarRig Build(AvatarData d, Transform parent)
        {
            var old = parent.Find("Avatar");
            if (old != null) { old.name = "Avatar_old"; Object.Destroy(old.gameObject); }

            var cfg = GameConfig.Instance;
            CosmeticItem Item(CosmeticSlot s) => cfg != null ? cfg.FindItem(d.Get(s)) : null;

            var root = new GameObject("Avatar").transform;
            root.SetParent(parent, false);
            root.localScale = Vector3.one * d.HeightScale;
            var rig = root.gameObject.AddComponent<AvatarRig>();

            Color skin = SkinTones[Mathf.Clamp(d.skinTone, 0, SkinTones.Length - 1)];
            Color hair = HairColors[Mathf.Clamp(d.hairColor, 0, HairColors.Length - 1)];

            var top = Item(CosmeticSlot.Top);
            bool full = top != null && top.fullOutfit;
            var bottom = full ? null : Item(CosmeticSlot.Bottom);
            var shoes = Item(CosmeticSlot.Shoes);
            var headItem = Item(CosmeticSlot.Head);
            var accessory = Item(CosmeticSlot.Accessory);
            var back = Item(CosmeticSlot.Back);

            Color topColor = top != null ? top.primary : new Color(0.96f, 0.96f, 0.96f);
            Color legColor = full ? top.bottomColor : bottom != null ? bottom.primary : new Color(0.2f, 0.32f, 0.62f);
            Color shoeColor = shoes != null ? shoes.primary : new Color(0.25f, 0.25f, 0.28f);

            // Legs (pivot at hips so they can swing)
            rig.legL = Pivot("LegL", root, new Vector3(-0.15f, 0.55f, 0f));
            rig.legR = Pivot("LegR", root, new Vector3(0.15f, 0.55f, 0f));
            foreach (var leg in new[] { rig.legL, rig.legR })
            {
                Box(leg, new Vector3(0, -0.26f, 0), new Vector3(0.26f, 0.52f, 0.3f), legColor);
                Box(leg, new Vector3(0, -0.5f, 0.03f), new Vector3(0.28f, 0.1f, 0.36f), shoeColor);
            }

            // Body
            rig.body = Pivot("Body", root, new Vector3(0, 0.55f, 0f));
            Box(rig.body, new Vector3(0, 0.3f, 0), new Vector3(0.62f, 0.6f, 0.36f), topColor);
            if (top != null && top.secondary != top.primary)
            {
                Box(rig.body, new Vector3(0, 0.3f, 0.185f), new Vector3(0.12f, 0.58f, 0.02f), top.secondary);   // placket / trim
                Box(rig.body, new Vector3(0, 0.58f, 0f), new Vector3(0.64f, 0.06f, 0.38f), top.secondary);       // collar
            }

            bool skirt = (full && top.longSkirt) || (bottom != null && bottom.shape == ItemShape.LongSkirt);
            if (skirt)
            {
                Color skirtColor = full ? top.bottomColor : bottom.primary;
                Box(root, new Vector3(0, 0.33f, 0), new Vector3(0.6f, 0.6f, 0.4f), skirtColor);
                if (bottom != null && bottom.secondary != bottom.primary)
                    Box(root, new Vector3(0, 0.08f, 0), new Vector3(0.62f, 0.08f, 0.42f), bottom.secondary);
            }
            if (bottom != null && bottom.shape == ItemShape.Samping)
                Box(root, new Vector3(0, 0.5f, 0), new Vector3(0.66f, 0.3f, 0.4f), bottom.secondary);

            // Arms (pivot at shoulders)
            rig.armL = Pivot("ArmL", root, new Vector3(-0.42f, 1.1f, 0f));
            rig.armR = Pivot("ArmR", root, new Vector3(0.42f, 1.1f, 0f));
            foreach (var arm in new[] { rig.armL, rig.armR })
            {
                Box(arm, new Vector3(0, -0.24f, 0), new Vector3(0.2f, 0.5f, 0.24f), topColor);
                Box(arm, new Vector3(0, -0.54f, 0), new Vector3(0.18f, 0.12f, 0.2f), skin);
            }

            // Head
            rig.head = Pivot("Head", root, new Vector3(0, 1.15f, 0f));
            Box(rig.head, new Vector3(0, 0.27f, 0), new Vector3(0.55f, 0.55f, 0.55f), skin);
            BuildFace(rig.head, d);

            bool coversHair = headItem != null && (headItem.shape == ItemShape.Tudung || headItem.shape == ItemShape.Helmet);
            if (!coversHair) BuildHair(rig.head, d.hairStyle, hair, headItem != null);

            if (headItem != null) BuildItem(headItem, rig);
            if (accessory != null) BuildItem(accessory, rig);
            if (back != null) BuildItem(back, rig);

            return rig;
        }

        static void BuildFace(Transform head, AvatarData d)
        {
            Vector2[] eyeSizes = { new Vector2(0.08f, 0.1f), new Vector2(0.1f, 0.1f), new Vector2(0.07f, 0.13f), new Vector2(0.12f, 0.05f) };
            Vector2 e = eyeSizes[Mathf.Abs(d.eyes) % EyeStyles];
            Box(head, new Vector3(-0.12f, 0.32f, 0.276f), new Vector3(e.x, e.y, 0.02f), EyeColor);
            Box(head, new Vector3(0.12f, 0.32f, 0.276f), new Vector3(e.x, e.y, 0.02f), EyeColor);

            Vector2[] mouthSizes = { new Vector2(0.18f, 0.04f), new Vector2(0.12f, 0.04f), new Vector2(0.24f, 0.06f), new Vector2(0.08f, 0.08f) };
            Vector2 m = mouthSizes[Mathf.Abs(d.mouth) % MouthStyles];
            Box(head, new Vector3(0, 0.15f, 0.276f), new Vector3(m.x, m.y, 0.02f), MouthColor);
            // rosy cheeks
            Box(head, new Vector3(-0.2f, 0.2f, 0.276f), new Vector3(0.07f, 0.04f, 0.01f), new Color(1f, 0.6f, 0.6f));
            Box(head, new Vector3(0.2f, 0.2f, 0.276f), new Vector3(0.07f, 0.04f, 0.01f), new Color(1f, 0.6f, 0.6f));
        }

        static void BuildHair(Transform head, int style, Color c, bool wearingHat)
        {
            style = Mathf.Abs(style) % HairStyles;
            if (style == 6) return; // bald
            if (!wearingHat)
            {
                if (style == 3) // spiky
                {
                    for (int i = -2; i <= 2; i++)
                        Box(head, new Vector3(i * 0.11f, 0.6f, 0), new Vector3(0.1f, 0.14f, 0.5f), c, Quaternion.Euler(0, 0, i * 12f));
                }
                else if (style == 4) // curly
                {
                    Sphere(head, new Vector3(0, 0.56f, -0.02f), new Vector3(0.66f, 0.32f, 0.64f), c);
                }
                else
                {
                    Box(head, new Vector3(0, 0.57f, -0.01f), new Vector3(0.59f, 0.1f, 0.59f), c);
                }
                Box(head, new Vector3(0, 0.49f, 0.25f), new Vector3(0.57f, 0.1f, 0.08f), c); // fringe
            }
            Box(head, new Vector3(0, 0.34f, -0.28f), new Vector3(0.58f, 0.46f, 0.06f), c);   // back of head
            Box(head, new Vector3(-0.285f, 0.42f, -0.05f), new Vector3(0.04f, 0.28f, 0.46f), c);
            Box(head, new Vector3(0.285f, 0.42f, -0.05f), new Vector3(0.04f, 0.28f, 0.46f), c);

            if (style == 1) Box(head, new Vector3(0, 0.02f, -0.3f), new Vector3(0.58f, 0.5f, 0.08f), c);            // long
            if (style == 2) Sphere(head, new Vector3(0, 0.62f, -0.24f), Vector3.one * 0.24f, c);                     // bun
            if (style == 5) Box(head, new Vector3(0, 0.15f, -0.36f), new Vector3(0.14f, 0.42f, 0.12f), c);          // ponytail
        }

        static void BuildItem(CosmeticItem it, AvatarRig rig)
        {
            if (it.prefabOverride != null)
            {
                Transform anchor = it.slot == CosmeticSlot.Head ? rig.head : rig.body;
                Object.Instantiate(it.prefabOverride, anchor, false);
                return;
            }

            Transform h = rig.head, b = rig.body;
            Color p = it.primary, s = it.secondary;
            switch (it.shape)
            {
                case ItemShape.Songkok:
                    Box(h, new Vector3(0, 0.62f, 0), new Vector3(0.5f, 0.2f, 0.46f), p);
                    break;
                case ItemShape.Tengkolok:
                    Box(h, new Vector3(0, 0.62f, 0), new Vector3(0.56f, 0.16f, 0.52f), p);
                    Box(h, new Vector3(0.12f, 0.74f, 0.05f), new Vector3(0.22f, 0.22f, 0.06f), s, Quaternion.Euler(0, 0, 45));
                    break;
                case ItemShape.Tudung:
                    Box(h, new Vector3(0, 0.6f, -0.02f), new Vector3(0.63f, 0.1f, 0.62f), p);
                    Box(h, new Vector3(-0.3f, 0.3f, -0.02f), new Vector3(0.05f, 0.62f, 0.62f), p);
                    Box(h, new Vector3(0.3f, 0.3f, -0.02f), new Vector3(0.05f, 0.62f, 0.62f), p);
                    Box(h, new Vector3(0, 0.3f, -0.31f), new Vector3(0.63f, 0.62f, 0.05f), p);
                    Box(h, new Vector3(0, 0.5f, 0.29f), new Vector3(0.6f, 0.05f, 0.03f), s);       // front band
                    Box(b, new Vector3(0, 0.5f, 0.0f), new Vector3(0.7f, 0.22f, 0.44f), p);         // drape over shoulders
                    break;
                case ItemShape.Cap:
                    Box(h, new Vector3(0, 0.6f, 0), new Vector3(0.58f, 0.12f, 0.58f), p);
                    Box(h, new Vector3(0, 0.56f, 0.36f), new Vector3(0.5f, 0.04f, 0.24f), s);
                    break;
                case ItemShape.Helmet:
                    Sphere(h, new Vector3(0, 0.28f, 0), Vector3.one * 0.98f, p);
                    Box(h, new Vector3(0, 0.3f, 0.46f), new Vector3(0.52f, 0.32f, 0.04f), s);       // visor
                    break;
                case ItemShape.ChefHat:
                    Cylinder(h, new Vector3(0, 0.68f, 0), new Vector3(0.46f, 0.12f, 0.46f), p);
                    Sphere(h, new Vector3(0, 0.86f, 0), new Vector3(0.56f, 0.3f, 0.56f), p);
                    break;
                case ItemShape.Glasses:
                    Box(h, new Vector3(-0.12f, 0.32f, 0.29f), new Vector3(0.17f, 0.14f, 0.02f), p);
                    Box(h, new Vector3(0.12f, 0.32f, 0.29f), new Vector3(0.17f, 0.14f, 0.02f), p);
                    Box(h, new Vector3(0, 0.34f, 0.29f), new Vector3(0.08f, 0.03f, 0.02f), p);
                    break;
                case ItemShape.Necklace:
                    for (int i = -3; i <= 3; i++)
                        Box(b, new Vector3(i * 0.065f, 0.5f - Mathf.Abs(i) * 0.02f + 0.0f, 0.19f), new Vector3(0.05f, 0.05f, 0.03f), i % 2 == 0 ? p : s);
                    break;
                case ItemShape.Wristband:
                    Box(rig.armL, new Vector3(0, -0.44f, 0), new Vector3(0.22f, 0.06f, 0.26f), p);
                    Box(rig.armR, new Vector3(0, -0.44f, 0), new Vector3(0.22f, 0.06f, 0.26f), s);
                    break;
                case ItemShape.Backpack:
                    Box(b, new Vector3(0, 0.3f, -0.29f), new Vector3(0.48f, 0.5f, 0.22f), p);
                    Box(b, new Vector3(0, 0.22f, -0.41f), new Vector3(0.36f, 0.2f, 0.04f), s);
                    break;
                case ItemShape.Wings:
                    Box(b, new Vector3(-0.32f, 0.4f, -0.26f), new Vector3(0.6f, 0.5f, 0.03f), p, Quaternion.Euler(0, 25, 20));
                    Box(b, new Vector3(0.32f, 0.4f, -0.26f), new Vector3(0.6f, 0.5f, 0.03f), p, Quaternion.Euler(0, -25, -20));
                    break;
                case ItemShape.Cape:
                    Box(b, new Vector3(0, 0.05f, -0.24f), new Vector3(0.62f, 0.95f, 0.04f), p, Quaternion.Euler(-8, 0, 0));
                    Box(b, new Vector3(0, 0.56f, -0.2f), new Vector3(0.66f, 0.08f, 0.08f), s);
                    break;
                case ItemShape.Jetpack:
                    Cylinder(b, new Vector3(-0.13f, 0.3f, -0.3f), new Vector3(0.2f, 0.25f, 0.2f), p);
                    Cylinder(b, new Vector3(0.13f, 0.3f, -0.3f), new Vector3(0.2f, 0.25f, 0.2f), p);
                    Box(b, new Vector3(-0.13f, -0.02f, -0.3f), new Vector3(0.12f, 0.15f, 0.12f), s);
                    Box(b, new Vector3(0.13f, -0.02f, -0.3f), new Vector3(0.12f, 0.15f, 0.12f), s);
                    break;
                case ItemShape.WauKite:
                    Box(b, new Vector3(0, 0.45f, -0.26f), new Vector3(0.5f, 0.5f, 0.03f), p, Quaternion.Euler(0, 0, 45));
                    Box(b, new Vector3(0, 0.45f, -0.28f), new Vector3(0.75f, 0.12f, 0.03f), s);
                    break;
            }
        }

        // ---------- Pets ----------

        public static Transform BuildPet(ItemShape kind, Color primary, Color secondary, Transform parent)
        {
            var root = new GameObject("Pet").transform;
            root.SetParent(parent, false);
            switch (kind)
            {
                case ItemShape.PetCat:
                    Box(root, new Vector3(0, 0.22f, 0), new Vector3(0.28f, 0.22f, 0.45f), primary);
                    Box(root, new Vector3(0, 0.42f, 0.25f), new Vector3(0.28f, 0.26f, 0.24f), primary);
                    Box(root, new Vector3(-0.09f, 0.6f, 0.25f), new Vector3(0.07f, 0.1f, 0.05f), primary);
                    Box(root, new Vector3(0.09f, 0.6f, 0.25f), new Vector3(0.07f, 0.1f, 0.05f), primary);
                    Box(root, new Vector3(0, 0.36f, -0.3f), new Vector3(0.06f, 0.3f, 0.06f), secondary, Quaternion.Euler(-30, 0, 0));
                    Eyes(root, 0.45f, 0.375f, 0.06f);
                    break;
                case ItemShape.PetTurtle:
                    Sphere(root, new Vector3(0, 0.16f, 0), new Vector3(0.55f, 0.28f, 0.65f), primary);
                    Box(root, new Vector3(0, 0.12f, 0.38f), new Vector3(0.18f, 0.14f, 0.18f), secondary);
                    Eyes(root, 0.15f, 0.475f, 0.05f);
                    break;
                case ItemShape.PetHornbill:
                    Box(root, new Vector3(0, 0.35f, 0), new Vector3(0.26f, 0.3f, 0.36f), primary);
                    Box(root, new Vector3(0, 0.56f, 0.1f), new Vector3(0.2f, 0.2f, 0.2f), primary);
                    Box(root, new Vector3(0, 0.55f, 0.3f), new Vector3(0.08f, 0.08f, 0.26f), secondary);   // beak
                    Box(root, new Vector3(0, 0.63f, 0.25f), new Vector3(0.08f, 0.06f, 0.16f), new Color(0.9f, 0.3f, 0.15f)); // casque
                    Box(root, new Vector3(0, 0.3f, -0.28f), new Vector3(0.18f, 0.06f, 0.3f), Color.white);  // tail
                    Box(root, new Vector3(0, 0.1f, 0), new Vector3(0.12f, 0.2f, 0.06f), secondary);
                    Eyes(root, 0.6f, 0.205f, 0.04f);
                    break;
                case ItemShape.PetTapir:
                    // The Malayan tapir: black front and back, white saddle
                    Box(root, new Vector3(0, 0.3f, 0.2f), new Vector3(0.34f, 0.3f, 0.2f), primary);
                    Box(root, new Vector3(0, 0.3f, 0f), new Vector3(0.36f, 0.32f, 0.22f), secondary);
                    Box(root, new Vector3(0, 0.3f, -0.2f), new Vector3(0.34f, 0.3f, 0.2f), primary);
                    Box(root, new Vector3(0, 0.36f, 0.38f), new Vector3(0.24f, 0.22f, 0.18f), primary);
                    Box(root, new Vector3(0, 0.3f, 0.5f), new Vector3(0.1f, 0.08f, 0.1f), primary);
                    foreach (var x in new[] { -0.12f, 0.12f }) foreach (var z in new[] { -0.2f, 0.2f })
                        Box(root, new Vector3(x, 0.08f, z), new Vector3(0.08f, 0.16f, 0.08f), primary);
                    Eyes(root, 0.42f, 0.475f, 0.04f);
                    break;
                default: // Orangutan
                    Box(root, new Vector3(0, 0.35f, 0), new Vector3(0.34f, 0.34f, 0.26f), primary);
                    Box(root, new Vector3(0, 0.62f, 0.02f), new Vector3(0.28f, 0.24f, 0.24f), primary);
                    Box(root, new Vector3(0, 0.6f, 0.145f), new Vector3(0.18f, 0.14f, 0.02f), secondary);
                    Box(root, new Vector3(-0.24f, 0.25f, 0), new Vector3(0.09f, 0.45f, 0.09f), primary);
                    Box(root, new Vector3(0.24f, 0.25f, 0), new Vector3(0.09f, 0.45f, 0.09f), primary);
                    Eyes(root, 0.66f, 0.16f, 0.05f);
                    break;
            }
            return root;
        }

        static void Eyes(Transform t, float y, float z, float size)
        {
            Box(t, new Vector3(-0.06f, y, z), new Vector3(size, size, 0.02f), EyeColor);
            Box(t, new Vector3(0.06f, y, z), new Vector3(size, size, 0.02f), EyeColor);
        }

        // ---------- Mesh helpers ----------

        static Transform Pivot(string name, Transform parent, Vector3 localPos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            return t;
        }

        public static GameObject Box(Transform parent, Vector3 pos, Vector3 size, Color c, Quaternion? rot = null) =>
            Part(CubeMesh, parent, pos, size, c, rot);

        public static GameObject Sphere(Transform parent, Vector3 pos, Vector3 size, Color c) =>
            Part(SphereMesh, parent, pos, size, c, null);

        // Unity's cylinder is 2 units tall, so Y is halved to keep "size" meaning real size.
        public static GameObject Cylinder(Transform parent, Vector3 pos, Vector3 size, Color c) =>
            Part(CylinderMesh, parent, pos, new Vector3(size.x, size.y * 0.5f, size.z), c, null);

        static GameObject Part(Mesh mesh, Transform parent, Vector3 pos, Vector3 size, Color c, Quaternion? rot)
        {
            var go = new GameObject("Part");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            if (rot.HasValue) go.transform.localRotation = rot.Value;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = MaterialCache.Get(c);
            return go;
        }

        static Mesh PrimitiveMesh(PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            if (Application.isPlaying) Object.Destroy(go); else Object.DestroyImmediate(go);
            return mesh;
        }
    }
}
