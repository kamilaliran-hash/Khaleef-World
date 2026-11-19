using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KhaleefWorld.EditorTools
{
    /// <summary>
    /// All MVP game data in one place: cosmetics, the 16 locations and the 12 playable mini-games.
    /// Edit text here (or later directly on the generated assets) and re-run the setup menu.
    /// </summary>
    public static class KhaleefContent
    {
        public const string DataDir = "Assets/_KhaleefWorld/Data";

        public static Color C(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        public static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        // ------------------------------------------------------------------ Cosmetics

        static CosmeticItem Item(List<CosmeticItem> list, string id, string ms, string en, CosmeticSlot slot, ItemShape shape,
            string primary, string secondary, UnlockType unlock = UnlockType.Free, int price = 0, bool traditional = false,
            bool full = false, bool longSkirt = false, string bottom = null, string badge = null, string culture = null)
        {
            var it = GetOrCreate<CosmeticItem>($"{DataDir}/Items/{id}.asset");
            it.id = id; it.nameMS = ms; it.nameEN = en; it.slot = slot; it.shape = shape;
            it.primary = C(primary); it.secondary = C(secondary);
            it.bottomColor = bottom != null ? C(bottom) : C(primary);
            it.unlock = unlock; it.price = price; it.traditional = traditional;
            it.fullOutfit = full; it.longSkirt = longSkirt; it.requiredBadge = badge; it.cultureTag = culture;
            EditorUtility.SetDirty(it);
            list.Add(it);
            return it;
        }

        public static List<CosmeticItem> CreateCosmetics()
        {
            var L = new List<CosmeticItem>();
            const CosmeticSlot H = CosmeticSlot.Head, T = CosmeticSlot.Top, B = CosmeticSlot.Bottom, S = CosmeticSlot.Shoes,
                               A = CosmeticSlot.Accessory, K = CosmeticSlot.Back, P = CosmeticSlot.Pet;
            const UnlockType Coins = UnlockType.Coins, Badge = UnlockType.Badge;

            // Head
            Item(L, "songkok", "Songkok", "Songkok", H, ItemShape.Songkok, "1A1A1A", "1A1A1A", traditional: true, culture: "Malay");
            Item(L, "tudung_pink", "Tudung Merah Jambu", "Pink Tudung", H, ItemShape.Tudung, "F28DB2", "FFFFFF");
            Item(L, "tudung_blue", "Tudung Biru", "Blue Tudung", H, ItemShape.Tudung, "4F86E8", "FFFFFF");
            Item(L, "tudung_cream", "Tudung Krim", "Cream Tudung", H, ItemShape.Tudung, "F3E5C8", "C9A66B");
            Item(L, "tengkolok", "Tengkolok", "Tengkolok", H, ItemShape.Tengkolok, "D4A017", "B8860B", Coins, 60, true, culture: "Malay");
            Item(L, "cap_red", "Topi Merah", "Red Cap", H, ItemShape.Cap, "E53935", "FFFFFF");
            Item(L, "pilot_cap", "Topi Juruterbang", "Pilot Cap", H, ItemShape.Cap, "1F3A68", "F5C518", Coins, 40);
            Item(L, "chef_hat", "Topi Cef", "Chef Hat", H, ItemShape.ChefHat, "FFFFFF", "FFFFFF", Coins, 40);
            Item(L, "hawker_hat", "Topi Cef Emas", "Golden Hawker Hat", H, ItemShape.ChefHat, "F1C40F", "F1C40F", Badge, badge: "badge_penang");
            Item(L, "fire_helmet", "Topi Bomba", "Firefighter Helmet", H, ItemShape.Helmet, "E53935", "2B2B2B", Coins, 80);
            Item(L, "astro_helmet", "Topi Angkasawan", "Astronaut Helmet", H, ItemShape.Helmet, "F2F2F2", "2A4D8F", Coins, 120);

            // Tops & full outfits
            Item(L, "baju_melayu", "Baju Melayu", "Baju Melayu", T, ItemShape.None, "2E9E6A", "F2C94C", traditional: true, culture: "Malay");
            Item(L, "baju_kurung", "Baju Kurung", "Baju Kurung", T, ItemShape.None, "9C6ADE", "F4D35E", traditional: true, full: true, longSkirt: true, bottom: "9C6ADE", culture: "Malay");
            Item(L, "kebaya", "Baju Kebaya", "Kebaya", T, ItemShape.None, "FFFFFF", "E85D75", traditional: true, full: true, longSkirt: true, bottom: "8E3B46", culture: "Malay / Peranakan");
            Item(L, "cheongsam", "Cheongsam", "Cheongsam", T, ItemShape.None, "D62828", "F2C94C", traditional: true, full: true, longSkirt: true, bottom: "D62828", culture: "Chinese");
            Item(L, "samfu", "Samfu", "Samfu", T, ItemShape.None, "4A90D9", "FFFFFF", traditional: true, full: true, bottom: "4A90D9", culture: "Chinese");
            Item(L, "saree", "Sari", "Saree", T, ItemShape.None, "C2185B", "F2C94C", traditional: true, full: true, longSkirt: true, bottom: "C2185B", culture: "Indian");
            Item(L, "kurta", "Kurta", "Kurta", T, ItemShape.None, "F39C12", "FFFFFF", traditional: true, culture: "Indian");
            Item(L, "punjabi_suit", "Sut Punjabi", "Punjabi Suit", T, ItemShape.None, "16A085", "F1C40F", traditional: true, full: true, bottom: "16A085", culture: "Punjabi");
            Item(L, "kadazan", "Pakaian Kadazan-Dusun", "Kadazan-Dusun Attire", T, ItemShape.None, "1C1C1C", "D4A017", traditional: true, full: true, longSkirt: true, bottom: "1C1C1C", culture: "Sabah");
            Item(L, "iban", "Pakaian Iban", "Iban Attire", T, ItemShape.None, "2B2B2B", "C0392B", traditional: true, full: true, longSkirt: true, bottom: "7B241C", culture: "Sarawak");
            Item(L, "bidayuh", "Pakaian Bidayuh", "Bidayuh Attire", T, ItemShape.None, "1C1C1C", "E74C3C", traditional: true, full: true, longSkirt: true, bottom: "1C1C1C", culture: "Sarawak");
            Item(L, "orang_ulu", "Pakaian Orang Ulu", "Orang Ulu Attire", T, ItemShape.None, "2C2C2C", "F1C40F", traditional: true, full: true, longSkirt: true, bottom: "2C2C2C", culture: "Sarawak");
            Item(L, "school_uniform", "Baju Sekolah", "School Uniform", T, ItemShape.None, "FFFFFF", "1F3A93");
            Item(L, "jersey", "Jersi Sukan", "Sports Jersey", T, ItemShape.None, "F1C40F", "1F3A93");
            Item(L, "superhero", "Baju Adiwira", "Superhero Suit", T, ItemShape.None, "2E86DE", "E74C3C", Coins, 80);
            Item(L, "doctor", "Kot Doktor", "Doctor's Coat", T, ItemShape.None, "FFFFFF", "5DADE2", Coins, 50);
            Item(L, "chef", "Jaket Cef", "Chef Jacket", T, ItemShape.None, "FFFFFF", "2B2B2B", Coins, 50);
            Item(L, "pilot", "Baju Juruterbang", "Pilot Shirt", T, ItemShape.None, "FFFFFF", "1F3A68", Coins, 50);
            Item(L, "firefighter", "Baju Bomba", "Firefighter Suit", T, ItemShape.None, "2C3E50", "F1C40F", Coins, 80, full: true, bottom: "2C3E50");
            Item(L, "astronaut", "Sut Angkasawan", "Astronaut Suit", T, ItemShape.None, "F2F2F2", "E67E22", Coins, 120, full: true, bottom: "F2F2F2");

            // Bottoms
            Item(L, "seluar_sekolah", "Seluar Sekolah", "School Trousers", B, ItemShape.None, "1F3A68", "1F3A68");
            Item(L, "skirt_sekolah", "Skirt Sekolah", "School Skirt", B, ItemShape.LongSkirt, "1F3A68", "1F3A68");
            Item(L, "samping", "Seluar & Samping", "Trousers & Samping", B, ItemShape.Samping, "2E9E6A", "B8860B", traditional: true, culture: "Malay");
            Item(L, "kain_batik", "Kain Batik", "Batik Sarong", B, ItemShape.LongSkirt, "8E44AD", "F39C12", traditional: true, culture: "Malaysia");
            Item(L, "seluar_hitam", "Seluar Hitam", "Black Trousers", B, ItemShape.None, "2B2B2B", "2B2B2B");
            Item(L, "jeans", "Seluar Jean", "Jeans", B, ItemShape.None, "3D5A80", "3D5A80");
            Item(L, "seluar_sukan", "Seluar Sukan", "Sports Shorts", B, ItemShape.None, "1F3A93", "1F3A93");

            // Shoes
            Item(L, "kasut_sekolah", "Kasut Sekolah", "School Shoes", S, ItemShape.None, "FFFFFF", "FFFFFF");
            Item(L, "capal", "Capal", "Capal Sandals", S, ItemShape.None, "8B5A2B", "8B5A2B", traditional: true, culture: "Malay");
            Item(L, "sneakers_red", "Kasut Merah", "Red Sneakers", S, ItemShape.None, "E53935", "FFFFFF");
            Item(L, "boots", "But", "Boots", S, ItemShape.None, "2B2B2B", "2B2B2B", Coins, 30);

            // Accessories
            Item(L, "glasses", "Cermin Mata", "Glasses", A, ItemShape.Glasses, "2B2B2B", "2B2B2B");
            Item(L, "beads", "Manik Orang Ulu", "Orang Ulu Beads", A, ItemShape.Necklace, "F1C40F", "E74C3C", traditional: true, culture: "Sarawak");
            Item(L, "wristband", "Gelang Tangan", "Wristbands", A, ItemShape.Wristband, "E74C3C", "2E86DE");

            // Back
            Item(L, "backpack", "Beg Sekolah", "School Backpack", K, ItemShape.Backpack, "2E86DE", "F1C40F");
            Item(L, "cape_red", "Jubah Adiwira", "Hero Cape", K, ItemShape.Cape, "E74C3C", "F1C40F", Coins, 60);
            Item(L, "wings", "Sayap", "Wings", K, ItemShape.Wings, "FFFFFF", "FFFFFF", Coins, 100);
            Item(L, "golden_wings", "Sayap Emas", "Golden Wings", K, ItemShape.Wings, "F1C40F", "F1C40F", Badge, badge: "badge_selangor");
            Item(L, "jetpack", "Beg Roket", "Jetpack", K, ItemShape.Jetpack, "95A5A6", "E67E22", Badge, badge: "badge_kl");
            Item(L, "wau_back", "Wau Bulan", "Wau Bulan Kite", K, ItemShape.WauKite, "2E86DE", "F1C40F", Badge, badge: "badge_kelantan", traditional: true);

            // Pets
            Item(L, "pet_cat", "Anak Kucing", "Kitten", P, ItemShape.PetCat, "F5B041", "FFFFFF", Coins, 60);
            Item(L, "pet_tapir", "Tapir Malaya", "Malayan Tapir", P, ItemShape.PetTapir, "1C1C1C", "F2F2F2", Coins, 100);
            Item(L, "pet_turtle", "Penyu", "Sea Turtle", P, ItemShape.PetTurtle, "27AE60", "F5CBA7", Badge, badge: "badge_terengganu");
            Item(L, "pet_hornbill", "Burung Enggang", "Hornbill", P, ItemShape.PetHornbill, "1C1C1C", "F1C40F", Badge, badge: "badge_sarawak");
            Item(L, "pet_orangutan", "Orang Utan", "Orangutan", P, ItemShape.PetOrangutan, "D35400", "F5CBA7", Badge, badge: "badge_sabah");
            return L;
        }

        // ------------------------------------------------------------------ Mini-games

        static MiniGameDefinition Game(string id, string titleMS, string titleEN, Subject subject, MiniGameTemplate template,
            string insMS, string insEN, string factMS, string factEN)
        {
            var g = GetOrCreate<MiniGameDefinition>($"{DataDir}/MiniGames/{id}.asset");
            g.id = id; g.titleMS = titleMS; g.titleEN = titleEN; g.subject = subject; g.template = template;
            g.instructionMS = insMS; g.instructionEN = insEN; g.funFactMS = factMS; g.funFactEN = factEN;
            g.cards = new List<CardPair>();
            EditorUtility.SetDirty(g);
            return g;
        }

        static MiniGameDefinition Climb(MiniGameDefinition g, MathMode mode, int target, string unitMS, string unitEN)
        {
            g.mathMode = mode; g.climbTarget = target; g.climbUnitMS = unitMS; g.climbUnitEN = unitEN;
            return g;
        }

        static MiniGameDefinition Cards(MiniGameDefinition g, params string[] faces)
        {
            // faces: a1, b1, a2, b2 ... (b may be "" for identical pairs)
            string[] palette = { "F5B041", "58D68D", "5DADE2", "EC7063", "AF7AC5", "F7DC6F", "48C9B0", "F0B27A", "85C1E9", "F1948A" };
            for (int i = 0; i + 1 < faces.Length; i += 2)
                g.cards.Add(new CardPair { a = faces[i], b = faces[i + 1], color = C(palette[(i / 2) % palette.Length]) });
            return g;
        }

        public static Dictionary<string, List<MiniGameDefinition>> CreateMiniGames()
        {
            var d = new Dictionary<string, List<MiniGameDefinition>>();

            // ---- Kuala Lumpur ----
            d["kuala_lumpur"] = new List<MiniGameDefinition>
            {
                Climb(Game("kl_tower_count", "Kira Tingkat Menara", "Tower Count", Subject.Maths, MiniGameTemplate.MathsClimb,
                    "Jawab soalan tambah untuk naik ke jambatan langit!", "Answer adding questions to ride up to the skybridge!",
                    "Menara Berkembar Petronas mempunyai 88 tingkat dan pernah menjadi bangunan tertinggi di dunia dari 1998 hingga 2004.",
                    "The Petronas Twin Towers have 88 floors and were the tallest buildings in the world from 1998 to 2004."),
                    MathMode.Addition, 88, "tingkat", "floors"),
                Climb(Game("kl_tower_lift", "Lif Menara KL", "KL Tower Lift", Subject.Maths, MiniGameTemplate.MathsClimb,
                    "Selesaikan soalan darab untuk menghidupkan lif!", "Solve times-table questions to power the lift!",
                    "Menara Kuala Lumpur setinggi 421 meter. Dari atasnya kamu boleh melihat seluruh bandar raya!",
                    "Menara Kuala Lumpur is 421 metres tall. From the top you can see the whole city!"),
                    MathMode.Multiplication, 421, "meter", "metres"),
                Cards(Game("kl_merdeka_memory", "Detik Merdeka", "Merdeka Moments", Subject.History, MiniGameTemplate.MemoryMatch,
                    "Padankan setiap kad dengan maksudnya.", "Match each card with its meaning.",
                    "Pada tengah malam 31 Ogos 1957, bendera Persekutuan Tanah Melayu dinaikkan di Dataran Merdeka.",
                    "At midnight on 31 August 1957, the flag of the Federation of Malaya was raised at Merdeka Square."),
                    "31 Ogos 1957", "Hari Merdeka",
                    "16 September 1963", "Hari Malaysia",
                    "Jalur Gemilang", "Bendera Malaysia",
                    "Negaraku", "Lagu Kebangsaan",
                    "Tunku Abdul Rahman", "Bapa Kemerdekaan",
                    "Kuala Lumpur", "Ibu Negara",
                    "Bunga Raya", "Bunga Kebangsaan",
                    "Putrajaya", "Pusat Pentadbiran"),
            };

            // ---- Selangor ----
            d["selangor"] = new List<MiniGameDefinition>
            {
                Climb(Game("sel_272_steps", "272 Anak Tangga", "272 Steps", Subject.Maths, MiniGameTemplate.MathsClimb,
                    "Teruskan corak nombor untuk mendaki tangga pelangi ke gua!", "Continue the number pattern to climb the rainbow steps to the cave!",
                    "Batu Caves mempunyai 272 anak tangga yang dicat dengan warna pelangi. Gua batu kapurnya berusia kira-kira 400 juta tahun!",
                    "Batu Caves has 272 steps painted in rainbow colours. Its limestone caves are around 400 million years old!"),
                    MathMode.SkipCount, 272, "anak tangga", "steps"),
                Cards(Game("sel_animal_words", "Kawan Haiwan", "Animal Friends", Subject.Language, MiniGameTemplate.MemoryMatch,
                    "Padankan nama haiwan dalam Bahasa Melayu dengan Bahasa Inggeris.", "Match each animal's Malay name with its English name.",
                    "Monyet kera ekor panjang tinggal di Batu Caves. Jangan beri mereka makan - makanan manusia tidak baik untuk mereka!",
                    "Long-tailed macaques live at Batu Caves. Please don't feed them - human food is bad for them!"),
                    "Monyet", "Monkey", "Kucing", "Cat", "Gajah", "Elephant", "Harimau", "Tiger",
                    "Burung", "Bird", "Ikan", "Fish", "Kelawar", "Bat", "Penyu", "Sea turtle"),
                Climb(Game("sel_icity_lights", "Lampu i-City", "i-City Lights", Subject.Maths, MiniGameTemplate.MathsClimb,
                    "Tambah nombor untuk menyalakan semua lampu!", "Add the numbers to switch on all the lights!",
                    "i-City di Shah Alam terkenal dengan jutaan lampu LED berwarna-warni yang menyala pada waktu malam.",
                    "i-City in Shah Alam is famous for millions of colourful LED lights that glow at night."),
                    MathMode.Addition, 100, "lampu", "lights"),
            };

            // ---- Kelantan ----
            d["kelantan"] = new List<MiniGameDefinition>
            {
                Game("kel_wau", "Pembuat Wau", "Wau Maker", Subject.Science, MiniGameTemplate.WauKite,
                    "Bina wau, kemudian terbangkannya dalam zon emas!", "Build a wau, then fly it inside the golden zone!",
                    "Wau Bulan berbentuk seperti bulan sabit dan menjadi inspirasi logo Malaysia Airlines.",
                    "The Wau Bulan is shaped like a crescent moon and inspired the Malaysia Airlines logo."),
                Climb(Game("kel_pasar", "Pasar Siti Khadijah", "Siti Khadijah Market", Subject.Maths, MiniGameTemplate.MathsClimb,
                    "Kira wang dalam Ringgit dan sen untuk membeli barang!", "Count money in Ringgit and sen to buy things!",
                    "Pasar Siti Khadijah di Kota Bharu terkenal kerana kebanyakan peniaganya ialah wanita.",
                    "Siti Khadijah Market in Kota Bharu is famous because most of its traders are women."),
                    MathMode.Money, 10, "barang", "items"),
                Cards(Game("kel_wayang_words", "Kata Wayang Kulit", "Shadow Play Words", Subject.Language, MiniGameTemplate.MemoryMatch,
                    "Padankan perkataan budaya Kelantan dengan maksudnya dalam Bahasa Inggeris.", "Match Kelantan culture words with their English meaning.",
                    "Patung Wayang Kulit diperbuat daripada kulit lembu, dan orang yang memainkannya dipanggil Tok Dalang.",
                    "Wayang Kulit puppets are made from cowhide, and the puppeteer is called the Tok Dalang."),
                    "Wayang Kulit", "Shadow puppet", "Gendang", "Drum", "Rebab", "Bowed fiddle", "Wau", "Kite",
                    "Gasing", "Spinning top", "Nasi kerabu", "Blue rice", "Pasar", "Market", "Songket", "Gold-thread cloth"),
            };

            // ---- Penang ----
            d["penang"] = new List<MiniGameDefinition>
            {
                Cards(Game("pen_hawker_memory", "Memori Makanan Gerai", "Hawker Memory", Subject.Logic, MiniGameTemplate.MemoryMatch,
                    "Terbalikkan kad dan cari pasangan makanan yang sama!", "Flip the cards and find the matching hawker dishes!",
                    "George Town di Pulau Pinang ialah Tapak Warisan Dunia UNESCO dan terkenal dengan makanan jalanannya.",
                    "George Town in Penang is a UNESCO World Heritage Site and is famous for its street food."),
                    "Char Kuey Teow", "", "Asam Laksa", "", "Nasi Kandar", "", "Cendol", "",
                    "Rojak", "", "Pasembur", "", "Hokkien Mee", "", "Apom Balik", ""),
                Climb(Game("pen_hill_train", "Kereta Api Bukit Bendera", "Penang Hill Train", Subject.Maths, MiniGameTemplate.MathsClimb,
                    "Jawab soalan tolak untuk naik ke puncak bukit!", "Answer take-away questions to ride to the top of the hill!",
                    "Kereta api funikular Bukit Bendera mula beroperasi pada tahun 1923.",
                    "The Penang Hill funicular railway first opened in 1923."),
                    MathMode.Subtraction, 833, "meter", "metres"),
                Cards(Game("pen_street_words", "Kata Jalanan", "Street Words", Subject.Language, MiniGameTemplate.MemoryMatch,
                    "Padankan perkataan yang kamu jumpa di George Town.", "Match the words you find around George Town.",
                    "Mural 'Kanak-kanak Menunggang Basikal' di George Town dilukis oleh Ernest Zacharevic pada tahun 2012.",
                    "The 'Children on a Bicycle' mural in George Town was painted by Ernest Zacharevic in 2012."),
                    "Basikal", "Bicycle", "Lukisan dinding", "Mural", "Kedai kopi", "Coffee shop", "Jambatan", "Bridge",
                    "Bukit", "Hill", "Pulau", "Island", "Feri", "Ferry", "Jeti", "Jetty"),
            };
            return d;
        }

        // ------------------------------------------------------------------ States

        struct StateRow
        {
            public string id, ms, en, code, scene, lmMS, lmEN, color, greetMS, greetEN;
            public float x, y; public TravelMode travel; public int req; public bool borneo;
        }

        static readonly StateRow[] States =
        {
            new StateRow { id = "kuala_lumpur", ms = "Kuala Lumpur", en = "Kuala Lumpur", code = "KL", scene = "KualaLumpur", lmMS = "Menara Berkembar Petronas", lmEN = "Petronas Twin Towers", color = "2E86DE", x = 0.20f, y = 0.44f, travel = TravelMode.Car, req = 0,
                greetMS = "Selamat datang ke ibu negara Malaysia!", greetEN = "Welcome to Malaysia's capital city!" },
            new StateRow { id = "putrajaya", ms = "Putrajaya", en = "Putrajaya", code = "PJY", scene = "Putrajaya", lmMS = "Masjid Putra", lmEN = "Putra Mosque", color = "E67E22", x = 0.12f, y = 0.37f, travel = TravelMode.Car, req = 0,
                greetMS = "Selamat datang ke Putrajaya!", greetEN = "Welcome to Putrajaya!" },
            new StateRow { id = "labuan", ms = "Labuan", en = "Labuan", code = "LBN", scene = "Labuan", lmMS = "Pelabuhan Labuan", lmEN = "Labuan Port", color = "16A085", x = 0.74f, y = 0.68f, travel = TravelMode.Plane, req = 8, borneo = true,
                greetMS = "Selamat datang ke Pulau Labuan!", greetEN = "Welcome to Labuan Island!" },
            new StateRow { id = "selangor", ms = "Selangor", en = "Selangor", code = "SGR", scene = "Selangor", lmMS = "Batu Caves", lmEN = "Batu Caves", color = "E74C3C", x = 0.13f, y = 0.50f, travel = TravelMode.Car, req = 0,
                greetMS = "Jom panjat 272 anak tangga Batu Caves!", greetEN = "Let's climb the 272 steps of Batu Caves!" },
            new StateRow { id = "kelantan", ms = "Kelantan", en = "Kelantan", code = "KTN", scene = "Kelantan", lmMS = "Padang Wau", lmEN = "Wau Kite Field", color = "9B59B6", x = 0.28f, y = 0.80f, travel = TravelMode.Plane, req = 1,
                greetMS = "Selamat datang ke Negeri Cik Siti Wan Kembang!", greetEN = "Welcome to Kelantan, land of the wau kites!" },
            new StateRow { id = "terengganu", ms = "Terengganu", en = "Terengganu", code = "TRG", scene = "Terengganu", lmMS = "Masjid Kristal", lmEN = "Crystal Mosque", color = "1ABC9C", x = 0.33f, y = 0.66f, travel = TravelMode.Plane, req = 3,
                greetMS = "Selamat datang ke Terengganu!", greetEN = "Welcome to Terengganu!" },
            new StateRow { id = "pahang", ms = "Pahang", en = "Pahang", code = "PHG", scene = "Pahang", lmMS = "Ladang Teh Cameron Highlands", lmEN = "Cameron Highlands Tea Farms", color = "27AE60", x = 0.27f, y = 0.52f, travel = TravelMode.Car, req = 3,
                greetMS = "Selamat datang ke Pahang!", greetEN = "Welcome to Pahang!" },
            new StateRow { id = "johor", ms = "Johor", en = "Johor", code = "JHR", scene = "Johor", lmMS = "Bandaraya Johor Bahru", lmEN = "Johor Bahru City", color = "34495E", x = 0.30f, y = 0.15f, travel = TravelMode.Car, req = 5,
                greetMS = "Selamat datang ke Johor!", greetEN = "Welcome to Johor!" },
            new StateRow { id = "melaka", ms = "Melaka", en = "Melaka", code = "MLK", scene = "Melaka", lmMS = "A Famosa", lmEN = "A Famosa", color = "D35400", x = 0.15f, y = 0.22f, travel = TravelMode.Car, req = 5,
                greetMS = "Selamat datang ke Melaka!", greetEN = "Welcome to Melaka!" },
            new StateRow { id = "negeri_sembilan", ms = "Negeri Sembilan", en = "Negeri Sembilan", code = "NSN", scene = "NegeriSembilan", lmMS = "Rumah Adat Minangkabau", lmEN = "Minangkabau House", color = "F1C40F", x = 0.23f, y = 0.32f, travel = TravelMode.Train, req = 5,
                greetMS = "Selamat datang ke Negeri Sembilan!", greetEN = "Welcome to Negeri Sembilan!" },
            new StateRow { id = "perak", ms = "Perak", en = "Perak", code = "PRK", scene = "Perak", lmMS = "Bandar Lama Ipoh", lmEN = "Ipoh Old Town", color = "F39C12", x = 0.17f, y = 0.66f, travel = TravelMode.Train, req = 3,
                greetMS = "Selamat datang ke Perak!", greetEN = "Welcome to Perak!" },
            new StateRow { id = "penang", ms = "Pulau Pinang", en = "Penang", code = "PNG", scene = "Penang", lmMS = "Seni Jalanan George Town", lmEN = "George Town Street Art", color = "3498DB", x = 0.09f, y = 0.75f, travel = TravelMode.Ferry, req = 2,
                greetMS = "Selamat datang ke Pulau Mutiara!", greetEN = "Welcome to the Pearl of the Orient!" },
            new StateRow { id = "kedah", ms = "Kedah", en = "Kedah", code = "KDH", scene = "Kedah", lmMS = "Sawah Padi & Langkawi", lmEN = "Paddy Fields & Langkawi", color = "2ECC71", x = 0.17f, y = 0.82f, travel = TravelMode.Train, req = 5,
                greetMS = "Selamat datang ke Jelapang Padi Malaysia!", greetEN = "Welcome to Malaysia's rice bowl!" },
            new StateRow { id = "perlis", ms = "Perlis", en = "Perlis", code = "PLS", scene = "Perlis", lmMS = "Ladang Mangga Harumanis", lmEN = "Harumanis Mango Farms", color = "E91E63", x = 0.10f, y = 0.89f, travel = TravelMode.Train, req = 5,
                greetMS = "Selamat datang ke Perlis!", greetEN = "Welcome to Perlis!" },
            new StateRow { id = "sabah", ms = "Sabah", en = "Sabah", code = "SBH", scene = "Sabah", lmMS = "Gunung Kinabalu", lmEN = "Mount Kinabalu", color = "00BCD4", x = 0.86f, y = 0.66f, travel = TravelMode.Plane, req = 8, borneo = true,
                greetMS = "Kopivosian! Selamat datang ke Negeri di Bawah Bayu!", greetEN = "Kopivosian! Welcome to the Land Below the Wind!" },
            new StateRow { id = "sarawak", ms = "Sarawak", en = "Sarawak", code = "SWK", scene = "Sarawak", lmMS = "Rumah Panjang", lmEN = "Longhouse", color = "C0392B", x = 0.60f, y = 0.48f, travel = TravelMode.Plane, req = 8, borneo = true,
                greetMS = "Selamat datang ke Bumi Kenyalang!", greetEN = "Welcome to the Land of the Hornbills!" },
        };

        public static List<StateDefinition> CreateStates(Dictionary<string, List<MiniGameDefinition>> games)
        {
            var list = new List<StateDefinition>();
            foreach (var r in States)
            {
                var st = GetOrCreate<StateDefinition>($"{DataDir}/States/{r.id}.asset");
                st.id = r.id; st.nameMS = r.ms; st.nameEN = r.en; st.shortCode = r.code; st.sceneName = r.scene;
                st.landmarkMS = r.lmMS; st.landmarkEN = r.lmEN; st.mapPosition = new Vector2(r.x, r.y); st.color = C(r.color);
                st.travelMode = r.travel; st.requiredStamps = r.req; st.borneo = r.borneo; st.badgeId = "badge_" + r.id.Replace("kuala_lumpur", "kl");
                st.greetingMS = r.greetMS; st.greetingEN = r.greetEN;
                st.miniGames = games.TryGetValue(r.id, out var g) ? g : new List<MiniGameDefinition>();
                st.playable = st.miniGames.Count > 0;
                EditorUtility.SetDirty(st);
                list.Add(st);
            }
            return list;
        }
    }
}
