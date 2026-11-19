using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace KhaleefWorld.EditorTools
{
    /// <summary>
    /// Menu: Khaleef World > 1. Create MVP Content
    /// Generates every asset the MVP needs – materials, cosmetics, 16 states, 12 mini-games, the config –
    /// and builds 7 playable scenes (Boot, CharacterCreator, WorldMap, KualaLumpur, Selangor, Kelantan,
    /// Penang) out of low-poly blocks, then registers them in Build Settings. Safe to run again.
    /// </summary>
    public static class KhaleefSetupWizard
    {
        const string Root = "Assets/_KhaleefWorld";
        const string MatDir = Root + "/Materials";
        const string SceneDir = Root + "/Scenes";
        const string ResDir = Root + "/Resources";

        static readonly Dictionary<string, Material> Mats = new Dictionary<string, Material>();
        static Shader litShader;

        [MenuItem("Khaleef World/1. Create MVP Content (data + scenes)", priority = 1)]
        public static void CreateAll()
        {
            if (!EditorUtility.DisplayDialog("Khaleef World",
                "This will create the game data, materials and 7 playable scenes under Assets/_KhaleefWorld.\n\nExisting generated scenes will be overwritten.",
                "Create", "Cancel")) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                EditorUtility.DisplayDialog("Khaleef World",
                    "URP Lit shader not found. Create the project from the 'Universal 3D' (URP) template, then run this again.", "OK");
                return;
            }

            try
            {
                Progress("Creating folders", 0.05f);
                foreach (var f in new[] { Root, KhaleefContent.DataDir, KhaleefContent.DataDir + "/Items", KhaleefContent.DataDir + "/States",
                                          KhaleefContent.DataDir + "/MiniGames", MatDir, MatDir + "/Generated", SceneDir, ResDir })
                    Folder(f);
                Mats.Clear();

                Progress("Creating game data", 0.15f);
                var baseMat = MaterialAsset(MatDir + "/Blocky.mat", Color.white);
                var items = KhaleefContent.CreateCosmetics();
                var games = KhaleefContent.CreateMiniGames();
                var states = KhaleefContent.CreateStates(games);

                var cfg = KhaleefContent.GetOrCreate<GameConfig>(ResDir + "/KhaleefConfig.asset");
                cfg.blockyMaterial = baseMat;
                cfg.cosmetics = items;
                cfg.states = states;
                EditorUtility.SetDirty(cfg);
                AssetDatabase.SaveAssets();

                var scenes = new List<string>();
                Progress("Building Boot scene", 0.3f); scenes.Add(BuildBootScene());
                Progress("Building Character Creator", 0.35f); scenes.Add(BuildCreatorScene());
                Progress("Building World Map", 0.4f); scenes.Add(BuildWorldMapScene());
                float p = 0.45f;
                foreach (var st in states.Where(s => s.playable))
                {
                    Progress("Building " + st.nameEN, p);
                    scenes.Add(BuildStateScene(st));
                    p += 0.12f;
                }

                EditorBuildSettings.scenes = scenes.Select(s => new EditorBuildSettingsScene(s, true)).ToArray();
                AssetDatabase.SaveAssets();
                EditorSceneManager.OpenScene(scenes[0]);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Khaleef World",
                "Done! 7 scenes are in Build Settings.\n\nPress Play in the Boot scene to try the game, or use\nKhaleef World > 3. Build APK to make an Android build.", "Great");
        }

        static void Progress(string msg, float t) => EditorUtility.DisplayProgressBar("Khaleef World", msg, t);

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // ---------------------------------------------------------------- Materials

        static Material MaterialAsset(string path, Color c)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(litShader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = litShader;
            m.color = c;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
            m.enableInstancing = true;
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material Mat(Color c)
        {
            string hex = ColorUtility.ToHtmlStringRGB(c);
            if (Mats.TryGetValue(hex, out var m) && m != null) return m;
            m = MaterialAsset($"{MatDir}/Generated/M_{hex}.mat", c);
            Mats[hex] = m;
            return m;
        }

        static Material Mat(string hex) => Mat(KhaleefContent.C(hex));

        // ---------------------------------------------------------------- Scene helpers

        static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, string hex,
                               Vector3? euler = null, bool collider = true, bool isStatic = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            go.GetComponent<MeshRenderer>().sharedMaterial = Mat(hex);
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            else if (type == PrimitiveType.Sphere && !(Mathf.Approximately(scale.x, scale.y) && Mathf.Approximately(scale.y, scale.z)))
            {
                // A SphereCollider ignores non-uniform scale (it uses the largest axis), so squashed hills
                // and domes get an exact mesh collider instead.
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.AddComponent<MeshCollider>();
            }
            if (isStatic) GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            return go;
        }

        static GameObject Box(string name, Transform p, Vector3 pos, Vector3 scale, string hex, Vector3? euler = null, bool collider = true) =>
            Prim(PrimitiveType.Cube, name, p, pos, scale, hex, euler, collider);

        /// <summary>Cylinder whose `height` is its real height (Unity's cylinder is 2 units tall).</summary>
        static GameObject Cyl(string name, Transform p, Vector3 basePos, float diameter, float height, string hex, bool collider = true) =>
            Prim(PrimitiveType.Cylinder, name, p, basePos + Vector3.up * height * 0.5f, new Vector3(diameter, height * 0.5f, diameter), hex, null, collider);

        static GameObject Ball(string name, Transform p, Vector3 pos, Vector3 scale, string hex, bool collider = true) =>
            Prim(PrimitiveType.Sphere, name, p, pos, scale, hex, null, collider);

        static Transform Group(string name)
        {
            return new GameObject(name).transform;
        }

        static void Sign(Transform parent, string text, Vector3 pos, float yRotation = 0f, float size = 0.25f, string hex = "FFFFFF")
        {
            var board = Box("Sign_" + text, parent, pos, new Vector3(text.Length * size * 1.1f + 1f, size * 4f, 0.2f), "2C3E50", new Vector3(0, yRotation, 0), false);
            var go = new GameObject("Text");
            go.transform.SetParent(board.transform.parent, false);
            go.transform.localPosition = pos + Quaternion.Euler(0, yRotation, 0) * new Vector3(0, 0, -0.12f);
            go.transform.localEulerAngles = new Vector3(0, yRotation, 0);
            var tm = go.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.font = font;
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = size * 0.31f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = KhaleefContent.C(hex);
        }

        static Camera MakeCamera(string skyHex, Vector3 pos, Vector3 euler)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            go.transform.position = pos;
            go.transform.eulerAngles = euler;
            var cam = go.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = KhaleefContent.C(skyHex);
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 200f;
            return cam;
        }

        static void MakeSun()
        {
            var go = new GameObject("Sun");
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.2f;
            l.color = new Color(1f, 0.96f, 0.88f);
            l.shadows = LightShadows.Soft;
            go.transform.eulerAngles = new Vector3(50, -35, 0);
        }

        static Scene NewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.65f, 0.72f);
            RenderSettings.skybox = null;
            return scene;
        }

        static string SaveScene(Scene scene, string name)
        {
            string path = $"{SceneDir}/{name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        // ---------------------------------------------------------------- UI-only scenes

        static string BuildBootScene()
        {
            var s = NewScene();
            MakeCamera("FAD85A", new Vector3(0, 1, -10), Vector3.zero);
            new GameObject("Boot").AddComponent<BootScreen>();
            return SaveScene(s, SceneLoader.Boot);
        }

        static string BuildWorldMapScene()
        {
            var s = NewScene();
            MakeCamera("5AB8EB", new Vector3(0, 1, -10), Vector3.zero);
            new GameObject("WorldMap").AddComponent<WorldMapManager>();
            return SaveScene(s, SceneLoader.WorldMap);
        }

        static string BuildCreatorScene()
        {
            var s = NewScene();
            MakeSun();
            MakeCamera("CDEBF7", new Vector3(1.25f, 1.15f, 3.6f), new Vector3(4, 180, 0));
            var stage = Group("Stage");
            Box("Floor", stage, new Vector3(0, -0.05f, 0), new Vector3(30, 0.1f, 30), "F5E6C8");
            Cyl("Platform", stage, new Vector3(0, 0f, 0), 1.8f, 0.08f, "F28DB2");
            Box("Backdrop", stage, new Vector3(0, 3, -4), new Vector3(30, 8, 0.2f), "BFE3F5");

            var anchor = new GameObject("PreviewAnchor").transform;
            anchor.position = new Vector3(0, 0.08f, 0);
            var creator = new GameObject("CharacterCreator");
            var cust = creator.AddComponent<CharacterCustomizer>();
            cust.previewAnchor = anchor;
            creator.AddComponent<CharacterCreatorScreen>();
            return SaveScene(s, SceneLoader.CharacterCreator);
        }

        // ---------------------------------------------------------------- State scenes

        static string BuildStateScene(StateDefinition st)
        {
            var s = NewScene();
            MakeSun();
            string sky = st.id == "kelantan" ? "8FD3F5" : st.id == "penang" ? "9AD7F0" : "87CEEB";
            MakeCamera(sky, new Vector3(0, 5, -28), new Vector3(15, 0, 0));
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = KhaleefContent.C(sky);
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 160f;

            var env = Group("Environment");
            Box("Ground", env, new Vector3(0, -0.5f, 0), new Vector3(120, 1, 120), "7DC95E");
            Walls(env, 56f);

            var zoneGo = new GameObject("StateZone");
            var zone = zoneGo.AddComponent<StateZone>();
            zone.state = st;
            var spawn = new GameObject("SpawnPoint").transform;
            spawn.SetParent(zoneGo.transform, false);
            spawn.position = new Vector3(0, 0.1f, -20);
            zone.spawnPoint = spawn;

            var portals = Group("MiniGamePortals");
            var npcs = Group("NPCs");
            var landmarks = Group("Landmarks");

            switch (st.id)
            {
                case "kuala_lumpur": KualaLumpur(landmarks, portals, npcs, st); break;
                case "selangor": Selangor(landmarks, portals, npcs, st); break;
                case "kelantan": Kelantan(landmarks, portals, npcs, st); break;
                case "penang": Penang(landmarks, portals, npcs, st); break;
            }

            Walkways(env);
            Trees(env, st.id.GetHashCode(), st.id == "penang" || st.id == "kelantan");
            Coins(Group("Coins"));
            return SaveScene(s, st.sceneName);
        }

        static void Walls(Transform env, float half)
        {
            foreach (var (pos, size) in new[]
            {
                (new Vector3(0, 5, half), new Vector3(half * 2, 10, 1)), (new Vector3(0, 5, -half), new Vector3(half * 2, 10, 1)),
                (new Vector3(half, 5, 0), new Vector3(1, 10, half * 2)), (new Vector3(-half, 5, 0), new Vector3(1, 10, half * 2)),
            })
            {
                var w = new GameObject("InvisibleWall");
                w.transform.SetParent(env, false);
                w.transform.localPosition = pos;
                w.AddComponent<BoxCollider>().size = size;
            }
        }

        static void Walkways(Transform env)
        {
            Box("Path", env, new Vector3(0, 0.01f, -5), new Vector3(5, 0.02f, 40), "E8D9B5", null, false);
            Box("Plaza", env, new Vector3(0, 0.012f, 12), new Vector3(30, 0.02f, 14), "E8D9B5", null, false);
        }

        static void Trees(Transform env, int seed, bool palms)
        {
            var rng = new System.Random(seed);
            var group = new GameObject("Trees").transform;
            group.SetParent(env, false);
            int placed = 0, tries = 0;
            while (placed < 28 && tries++ < 400)
            {
                float x = (float)(rng.NextDouble() * 100 - 50), z = (float)(rng.NextDouble() * 100 - 50);
                if (Mathf.Abs(x) < 44 && z > -32 && z < 48) continue;    // keep the play area clear
                if (Mathf.Abs(x) < 6) continue;                          // keep the entry path clear
                var basePos = new Vector3(x, 0, z);
                if (palms && rng.NextDouble() < 0.5)
                {
                    Cyl("PalmTrunk", group, basePos, 0.5f, 6f, "9C7A54");
                    for (int i = 0; i < 5; i++)
                        Box("Frond", group, basePos + new Vector3(0, 6f, 0), new Vector3(0.5f, 0.15f, 3.2f), "3FA34D", new Vector3(20, i * 72f, 0), false);
                }
                else
                {
                    Cyl("Trunk", group, basePos, 0.6f, 2.5f, "8B5A2B");
                    float r = 2.5f + (float)rng.NextDouble() * 1.5f;
                    Ball("Canopy", group, basePos + new Vector3(0, 3.2f, 0), new Vector3(r, r * 0.85f, r), rng.NextDouble() < 0.5 ? "4CAF50" : "66BB6A", false);
                }
                placed++;
            }
        }

        static void Coins(Transform group)
        {
            void Coin(Vector3 pos)
            {
                if (Mathf.Abs(pos.x) < 4.5f && pos.z > 13f) return;   // keep clear of the Batu Caves steps
                var c = Prim(PrimitiveType.Cylinder, "Coin", group, pos + Vector3.up * 0.9f, new Vector3(0.6f, 0.04f, 0.6f), "F7C948",
                             new Vector3(90, 0, 0), false, false);
                c.AddComponent<CoinPickup>();
            }
            for (int i = 0; i < 8; i++) Coin(new Vector3(0, 0, -16 + i * 2.2f));                       // along the entry path
            for (int i = 0; i < 12; i++)                                                              // ring around the plaza
            {
                float a = i / 12f * Mathf.PI * 2f;
                Coin(new Vector3(Mathf.Cos(a) * 11f, 0, 12 + Mathf.Sin(a) * 6f));
            }
            for (int i = 0; i < 6; i++) Coin(new Vector3(-30 + i * 2f, 0, -12));
            for (int i = 0; i < 6; i++) Coin(new Vector3(20 + i * 2f, 0, -12));
        }

        static void Portal(Transform parent, MiniGameDefinition def, Vector3 pos)
        {
            if (def == null) return;
            var go = new GameObject("Portal_" + def.id);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.AddComponent<MiniGamePortal>().miniGame = def;
        }

        static void Npc(Transform parent, string name, Vector3 pos, float yRot, string ms, string en, int skin, int hair,
                        string head, string top, string bottom, string shoes = "kasut_sekolah")
        {
            var go = new GameObject("NPC_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.eulerAngles = new Vector3(0, yRot, 0);
            var npc = go.AddComponent<NpcGreeter>();
            npc.npcName = name;
            npc.lineMS = ms;
            npc.lineEN = en;
            npc.look = new AvatarData { skinTone = skin, hairStyle = hair, hairColor = 0 };
            npc.look.Set(CosmeticSlot.Head, head);
            npc.look.Set(CosmeticSlot.Top, top);
            npc.look.Set(CosmeticSlot.Bottom, bottom);
            npc.look.Set(CosmeticSlot.Shoes, shoes);
        }

        static void Stall(Transform p, Vector3 pos, string roofHex, string label)
        {
            Box("StallCounter", p, pos + new Vector3(0, 0.55f, 0), new Vector3(3.2f, 1.1f, 1.4f), "F5F5F5");
            Cyl("StallPostL", p, pos + new Vector3(-1.5f, 0, -0.6f), 0.15f, 2.6f, "7F8C8D", false);
            Cyl("StallPostR", p, pos + new Vector3(1.5f, 0, -0.6f), 0.15f, 2.6f, "7F8C8D", false);
            for (int i = 0; i < 4; i++)
                Box("Roof", p, pos + new Vector3(-1.35f + i * 0.9f, 2.7f, 0), new Vector3(0.9f, 0.2f, 2f), i % 2 == 0 ? roofHex : "FFFFFF", new Vector3(-8, 0, 0), false);
            Sign(p, label, pos + new Vector3(0, 3.3f, -0.6f), 0f, 0.18f);
        }

        // ---------------------------------------------------------------- Kuala Lumpur

        static void KualaLumpur(Transform lm, Transform portals, Transform npcs, StateDefinition st)
        {
            // Petronas Twin Towers
            foreach (float x in new[] { -6.5f, 6.5f })
            {
                float y = 0f;
                for (int i = 0; i < 6; i++)
                {
                    float d = 8f - i * 0.95f, h = 7f;
                    Cyl("PetronasTier", lm, new Vector3(x, y, 34), d, h, i % 2 == 0 ? "C9D1D9" : "AEB8C2");
                    y += h;
                }
                Cyl("PetronasSpire", lm, new Vector3(x, y, 34), 0.5f, 8f, "DDE3E8", false);
            }
            Box("Skybridge", lm, new Vector3(0, 17.5f, 34), new Vector3(8f, 1.4f, 2.2f), "8FA3B5");
            Box("SkybridgeLeg", lm, new Vector3(0, 12f, 34), new Vector3(0.6f, 10f, 0.6f), "8FA3B5", new Vector3(0, 0, 0), false);
            Sign(lm, "Menara Berkembar Petronas", new Vector3(0, 3, 26.5f), 0, 0.3f);

            // KL Tower (Menara Kuala Lumpur)
            Cyl("KLTowerShaft", lm, new Vector3(32, 0, 40), 2.6f, 30f, "E5E8EC");
            Ball("KLTowerPod", lm, new Vector3(32, 32, 40), new Vector3(9, 5, 9), "6A8CAF");
            Cyl("KLTowerAntenna", lm, new Vector3(32, 34, 40), 0.4f, 9f, "E5E8EC", false);
            Sign(lm, "Menara KL", new Vector3(32, 3, 35.5f), 0, 0.3f);

            // Merdeka Square + Sultan Abdul Samad Building
            Box("MerdekaField", lm, new Vector3(-30, 0.03f, 12), new Vector3(22, 0.06f, 16), "4CAF50", null, false);
            Cyl("Flagpole", lm, new Vector3(-30, 0, 12), 0.3f, 16f, "DDDDDD");
            for (int i = 0; i < 6; i++)
                Box("FlagStripe", lm, new Vector3(-27.6f, 15.3f - i * 0.4f, 12), new Vector3(4.4f, 0.4f, 0.08f), i % 2 == 0 ? "D62828" : "FFFFFF", null, false);
            Box("FlagCanton", lm, new Vector3(-28.9f, 14.7f, 11.95f), new Vector3(1.8f, 1.2f, 0.1f), "1A3A8F", null, false);
            Ball("FlagMoon", lm, new Vector3(-28.9f, 14.7f, 11.88f), new Vector3(0.6f, 0.6f, 0.05f), "F7C948", false);
            Box("SultanAbdulSamad", lm, new Vector3(-30, 4, 28), new Vector3(24, 8, 6), "B5651D");
            Box("ClockTower", lm, new Vector3(-30, 8, 28), new Vector3(3, 16, 3), "B5651D");
            Ball("CopperDome", lm, new Vector3(-30, 16.5f, 28), new Vector3(3.4f, 3.4f, 3.4f), "4C9A7F");
            Ball("CopperDomeL", lm, new Vector3(-40, 8.5f, 28), new Vector3(2.6f, 2.6f, 2.6f), "4C9A7F");
            Ball("CopperDomeR", lm, new Vector3(-20, 8.5f, 28), new Vector3(2.6f, 2.6f, 2.6f), "4C9A7F");
            Sign(lm, "Dataran Merdeka", new Vector3(-30, 2, 3.5f), 0, 0.3f);

            Stall(lm, new Vector3(12, 0, -6), "E74C3C", "Nasi Lemak");
            Stall(lm, new Vector3(18, 0, -6), "F39C12", "Satay");

            Portal(portals, st.miniGames[0], new Vector3(0, 0, 18));
            Portal(portals, st.miniGames[1], new Vector3(24, 0, 30));
            Portal(portals, st.miniGames[2], new Vector3(-20, 0, 6));

            Npc(npcs, "Abang Faiz", new Vector3(5, 0, -10), -30, "Jom naik Menara Petronas!", "Let's go up the Petronas Towers!", 6, 0, "songkok", "baju_melayu", "samping", "capal");
            Npc(npcs, "Kak Mei", new Vector3(-7, 0, -4), 30, "Dataran Merdeka ada di sebelah sana!", "Merdeka Square is over there!", 2, 5, null, "cheongsam", null);
            Npc(npcs, "Encik Raju", new Vector3(15, 0, -9), 0, "Cuba nasi lemak di sini, sedap!", "Try the nasi lemak here, it's tasty!", 9, 0, null, "kurta", "seluar_hitam");
        }

        // ---------------------------------------------------------------- Selangor

        static void Selangor(Transform lm, Transform portals, Transform npcs, StateDefinition st)
        {
            // Batu Caves limestone hill
            Ball("LimestoneHill", lm, new Vector3(0, 4, 55), new Vector3(70, 40, 40), "9E8B76");
            Ball("HillGreen1", lm, new Vector3(-18, 16, 46), new Vector3(16, 8, 10), "4E8B3A", false);
            Ball("HillGreen2", lm, new Vector3(20, 14, 47), new Vector3(14, 7, 10), "4E8B3A", false);

            // 272 rainbow steps (shortened to 28 walkable steps)
            string[] rainbow = { "E74C3C", "F39C12", "F1C40F", "2ECC71", "3498DB", "9B59B6", "EC407A" };
            for (int i = 0; i < 28; i++)
                Box("Step" + i, lm, new Vector3(0, 0.16f * (i + 1), 14f + i * 0.6f), new Vector3(7f, 0.32f * (i + 1), 0.6f), rainbow[i % rainbow.Length]);
            // (each step is a solid block down to the ground so there are no gaps underneath)
            Box("CavePlatform", lm, new Vector3(0, 4.5f, 32.5f), new Vector3(10, 9f, 4), "B0A08A");
            Box("CaveMouth", lm, new Vector3(0, 12, 34.6f), new Vector3(8, 6, 1), "2B2420", null, false);
            Sign(lm, "Batu Caves", new Vector3(0, 16.5f, 34f), 0, 0.4f);

            // Golden statue (stylised, respectful)
            Box("StatuePlinth", lm, new Vector3(-11, 1, 17), new Vector3(5, 2, 5), "D8CBB0");
            Prim(PrimitiveType.Capsule, "GoldenStatue", lm, new Vector3(-11, 8.5f, 17), new Vector3(3.2f, 6.5f, 3.2f), "E2B33C");

            // Sultan Salahuddin Abdul Aziz Shah Mosque (the Blue Mosque)
            Vector3 m = new Vector3(-34, 0, 6);
            Box("MosqueBase", lm, m + new Vector3(0, 3, 0), new Vector3(14, 6, 14), "F5F5F5");
            Ball("BlueDome", lm, m + new Vector3(0, 8, 0), new Vector3(11, 9, 11), "2D6CDF");
            foreach (var c in new[] { new Vector3(-9, 0, -9), new Vector3(9, 0, -9), new Vector3(-9, 0, 9), new Vector3(9, 0, 9) })
            {
                Cyl("Minaret", lm, m + c, 1f, 18f, "F5F5F5");
                Ball("MinaretTop", lm, m + c + new Vector3(0, 18.4f, 0), new Vector3(1.4f, 1.4f, 1.4f), "2D6CDF", false);
            }
            Sign(lm, "Masjid Biru Shah Alam", m + new Vector3(0, 1.5f, -7.4f), 0, 0.25f);

            // i-City light trees
            string[] lights = { "FF4FA3", "00E5FF", "FFEB3B", "76FF03", "FF9100", "B388FF" };
            for (int i = 0; i < 6; i++)
            {
                Vector3 b = new Vector3(26 + (i % 3) * 5, 0, 6 + (i / 3) * 6);
                Cyl("LightTreeTrunk", lm, b, 0.5f, 4f, "455A64");
                Ball("LightTree", lm, b + new Vector3(0, 5, 0), new Vector3(3.5f, 3.5f, 3.5f), lights[i], false);
            }
            Sign(lm, "i-City", new Vector3(31, 1.5f, 1.5f), 0, 0.3f);

            Portal(portals, st.miniGames[0], new Vector3(6, 0, 11));
            Portal(portals, st.miniGames[1], new Vector3(-16, 0, 24));
            Portal(portals, st.miniGames[2], new Vector3(22, 0, 2));

            Npc(npcs, "Pak Cik Muthu", new Vector3(-5, 0, 9), 20, "Bolehkah kamu kira semua anak tangga?", "Can you count all the steps?", 9, 6, null, "kurta", "seluar_hitam", "capal");
            Npc(npcs, "Adik Aina", new Vector3(10, 0, -5), -20, "Jangan beri monyet makan ya!", "Please don't feed the monkeys!", 4, 1, "tudung_pink", "baju_kurung", null);
            Npc(npcs, "Uncle Lim", new Vector3(24, 0, -4), 0, "Lampu i-City sangat cantik waktu malam!", "i-City's lights are beautiful at night!", 1, 0, "cap_red", "jersey", "jeans");
        }

        // ---------------------------------------------------------------- Kelantan

        static void Kelantan(Transform lm, Transform portals, Transform npcs, StateDefinition st)
        {
            // Wau kite field: tall poles with kites, plus kites "flying" high above
            string[] wauColors = { "2E86DE", "E67E22", "27AE60", "C0392B", "8E44AD" };
            for (int i = 0; i < 5; i++)
            {
                Vector3 b = new Vector3(-12 + i * 6, 0, 30);
                Cyl("WauPole", lm, b, 0.3f, 6f, "8B5A2B");
                Box("Wau", lm, b + new Vector3(0, 7f, 0), new Vector3(2.2f, 2.2f, 0.1f), wauColors[i], new Vector3(0, 0, 45), false);
                Box("WauWings", lm, b + new Vector3(0, 7.6f, -0.05f), new Vector3(4.2f, 0.6f, 0.08f), "F1C40F", null, false);
                Box("WauSky", lm, new Vector3(-20 + i * 10, 22 + (i % 2) * 4, 45), new Vector3(3, 3, 0.1f), wauColors[(i + 2) % 5], new Vector3(0, 0, 45 + i * 6), false);
            }
            Sign(lm, "Padang Wau", new Vector3(0, 1.5f, 27.5f), 0, 0.3f);

            // Pasar Siti Khadijah
            Vector3 mk = new Vector3(26, 0, 20);
            Box("PasarBuilding", lm, mk + new Vector3(0, 4.5f, 0), new Vector3(16, 9, 16), "F2B84B");
            Box("PasarRoof", lm, mk + new Vector3(0, 9.6f, 0), new Vector3(17, 1.2f, 17), "1E8C7E");
            Box("PasarDoor", lm, mk + new Vector3(0, 2, -8.05f), new Vector3(4, 4, 0.2f), "6D4C41", null, false);
            Sign(lm, "Pasar Siti Khadijah", mk + new Vector3(0, 6.5f, -8.3f), 0, 0.3f);

            // Wayang Kulit stage
            Vector3 wk = new Vector3(-24, 0, 20);
            Box("WayangStage", lm, wk + new Vector3(0, 0.6f, 0), new Vector3(10, 1.2f, 6), "8D6E63");
            Box("WayangScreen", lm, wk + new Vector3(0, 3.6f, 0), new Vector3(8, 4.5f, 0.15f), "FFF8E1");
            Box("PuppetA", lm, wk + new Vector3(-1.5f, 3.6f, 0.15f), new Vector3(0.7f, 2.2f, 0.05f), "3E2723", null, false);
            Box("PuppetB", lm, wk + new Vector3(1.6f, 3.4f, 0.15f), new Vector3(1.2f, 1.6f, 0.05f), "3E2723", null, false);
            Box("WayangRoof", lm, wk + new Vector3(0, 6.6f, 0), new Vector3(11, 0.5f, 7), "A1887F");
            Sign(lm, "Wayang Kulit", wk + new Vector3(0, 1.8f, -3.2f), 0, 0.3f);

            // Kampung houses on stilts
            foreach (float x in new[] { -38f, 38f })
            {
                Vector3 h = new Vector3(x, 0, -6);
                foreach (var c in new[] { new Vector3(-2.5f, 0, -2.5f), new Vector3(2.5f, 0, -2.5f), new Vector3(-2.5f, 0, 2.5f), new Vector3(2.5f, 0, 2.5f) })
                    Cyl("Stilt", lm, h + c, 0.4f, 1.8f, "6D4C41");
                Box("House", lm, h + new Vector3(0, 3.3f, 0), new Vector3(6, 3, 6), "A0522D");
                Box("RoofL", lm, h + new Vector3(-1.6f, 5.6f, 0), new Vector3(4, 0.3f, 7), "5D4037", new Vector3(0, 0, 35), false);
                Box("RoofR", lm, h + new Vector3(1.6f, 5.6f, 0), new Vector3(4, 0.3f, 7), "5D4037", new Vector3(0, 0, -35), false);
            }

            Stall(lm, new Vector3(12, 0, -6), "1E8C7E", "Nasi Kerabu");

            Portal(portals, st.miniGames[0], new Vector3(0, 0, 14));
            Portal(portals, st.miniGames[1], new Vector3(18, 0, 8));
            Portal(portals, st.miniGames[2], new Vector3(-16, 0, 9));

            Npc(npcs, "Tok Wan", new Vector3(4, 0, 4), -20, "Wau Bulan ialah kebanggaan Kelantan!", "The Wau Bulan is Kelantan's pride!", 7, 6, "songkok", "baju_melayu", "samping", "capal");
            Npc(npcs, "Mak Cik Kiah", new Vector3(20, 0, 2), 0, "Mari beli di pasar!", "Come and shop at the market!", 5, 0, "tudung_cream", "kebaya", null, "capal");
            Npc(npcs, "Adik Danial", new Vector3(-6, 0, -2), 30, "Jom tengok Wayang Kulit!", "Let's watch the shadow puppets!", 6, 3, "cap_red", "jersey", "seluar_sukan", "sneakers_red");
        }

        // ---------------------------------------------------------------- Penang

        static void Penang(Transform lm, Transform portals, Transform npcs, StateDefinition st)
        {
            // George Town shophouses
            string[] facades = { "F8BBD0", "B2EBF2", "FFF59D", "C5E1A5", "FFCCBC", "D1C4E9", "FFE0B2" };
            for (int i = 0; i < 9; i++)
            {
                float x = -34 + i * 6;
                Vector3 b = new Vector3(x, 0, 32);
                Box("Shophouse", lm, b + new Vector3(0, 3.5f, 0), new Vector3(5.8f, 7, 7), facades[i % facades.Length]);
                Box("ShophouseRoof", lm, b + new Vector3(0, 7.3f, 0), new Vector3(6.2f, 0.6f, 7.4f), "B23A2A");
                Box("Window", lm, b + new Vector3(-1.2f, 5, -3.55f), new Vector3(1.2f, 1.6f, 0.1f), "5D4037", null, false);
                Box("Window", lm, b + new Vector3(1.2f, 5, -3.55f), new Vector3(1.2f, 1.6f, 0.1f), "5D4037", null, false);
                Box("FiveFootWay", lm, b + new Vector3(0, 3, -4.2f), new Vector3(5.8f, 0.3f, 1.4f), "EFEBE9", null, false);
            }
            Sign(lm, "George Town", new Vector3(-10, 8.6f, 28.2f), 0, 0.4f);

            // Street-art wall with a blocky "kids on a bicycle" mural
            Vector3 w = new Vector3(-30, 0, 8);
            Box("MuralWall", lm, w + new Vector3(0, 2.5f, 0), new Vector3(10, 5, 0.4f), "EDE0D4");
            Cyl("MuralWheelA", lm, w + new Vector3(-2, 0.4f, -0.25f), 1.6f, 0.05f, "37474F", false);
            Cyl("MuralWheelB", lm, w + new Vector3(2, 0.4f, -0.25f), 1.6f, 0.05f, "37474F", false);
            Box("MuralKid", lm, w + new Vector3(-0.6f, 2.4f, -0.22f), new Vector3(0.8f, 1.6f, 0.05f), "1565C0", null, false);
            Box("MuralKid2", lm, w + new Vector3(0.6f, 2.2f, -0.22f), new Vector3(0.7f, 1.3f, 0.05f), "C62828", null, false);
            Sign(lm, "Seni Jalanan", w + new Vector3(0, 5.6f, 0), 0, 0.25f);

            // Penang Hill with the funicular
            Ball("PenangHill", lm, new Vector3(36, 0, 58), new Vector3(60, 40, 44), "558B2F");
            Box("FunicularTrack", lm, new Vector3(30, 7, 32), new Vector3(1.6f, 0.4f, 30), "6D4C41", new Vector3(-26, 0, 0), false);
            Box("FunicularCar", lm, new Vector3(30, 5.2f, 27.5f), new Vector3(2.6f, 2.2f, 4), "C62828", new Vector3(-26, 0, 0), false);
            Sign(lm, "Bukit Bendera", new Vector3(30, 1.5f, 16), 0, 0.3f);

            // Hawker centre
            string[] roofs = { "E74C3C", "F39C12", "27AE60", "8E44AD", "2E86DE" };
            string[] dishes = { "Char Kuey Teow", "Asam Laksa", "Nasi Kandar", "Cendol", "Rojak" };
            for (int i = 0; i < 5; i++) Stall(lm, new Vector3(-16 + i * 8, 0, -12), roofs[i], dishes[i]);
            for (int i = 0; i < 4; i++)
            {
                Vector3 t = new Vector3(-12 + i * 8, 0, -6);
                Cyl("Table", lm, t, 1.6f, 0.9f, "ECEFF1");
                for (int k = 0; k < 4; k++)
                    Cyl("Stool", lm, t + Quaternion.Euler(0, k * 90, 0) * new Vector3(1.4f, 0, 0), 0.6f, 0.5f, "D84315", false);
            }

            Portal(portals, st.miniGames[0], new Vector3(0, 0, 2));
            Portal(portals, st.miniGames[1], new Vector3(24, 0, 14));
            Portal(portals, st.miniGames[2], new Vector3(-18, 0, 12));

            Npc(npcs, "Auntie Leong", new Vector3(5, 0, -3), 0, "Char kuey teow saya paling sedap!", "My char kuey teow is the tastiest!", 1, 2, null, "samfu", null);
            Npc(npcs, "Kak Priya", new Vector3(-12, 0, 4), 30, "Cari lukisan dinding di George Town!", "Look for the murals in George Town!", 9, 5, null, "saree", null, "capal");
            Npc(npcs, "Encik Hafiz", new Vector3(16, 0, 6), -20, "Naik kereta api ke Bukit Bendera!", "Take the train up Penang Hill!", 5, 0, "songkok", "baju_melayu", "samping", "capal");
        }
    }
}
