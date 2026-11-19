using System.Collections.Generic;

namespace KhaleefWorld
{
    /// <summary>
    /// Tiny bilingual string table (Bahasa Melayu / English).
    /// Swap for the Unity Localization package when Mandarin and Tamil are added.
    /// </summary>
    public static class Loc
    {
        public static bool IsMalay => GameManager.I == null || GameManager.I.Save == null || GameManager.I.Save.language == "ms";

        static readonly Dictionary<string, string[]> T = new Dictionary<string, string[]>
        {
            // General
            {"play", new[]{"Main!", "Play!"}},
            {"start", new[]{"Mula!", "Start!"}},
            {"continue", new[]{"Teruskan", "Continue"}},
            {"play_again", new[]{"Main lagi", "Play again"}},
            {"close", new[]{"Tutup", "Close"}},
            {"done", new[]{"Siap!", "Done!"}},
            {"yes", new[]{"Ya", "Yes"}},
            {"no", new[]{"Tidak", "No"}},
            {"ok", new[]{"OK", "OK"}},
            {"coins", new[]{"Syiling", "Coins"}},
            {"stars", new[]{"Bintang", "Stars"}},
            {"map", new[]{"Peta", "Map"}},
            {"wardrobe", new[]{"Almari", "Wardrobe"}},
            {"passport", new[]{"Pasport", "Passport"}},
            {"quests", new[]{"Misi", "Quests"}},
            {"parents", new[]{"Ibu Bapa", "Parents"}},
            {"jump", new[]{"Lompat", "Jump"}},
            {"go", new[]{"Pergi!", "Go!"}},
            {"locked", new[]{"Berkunci", "Locked"}},
            {"coming_soon", new[]{"Akan datang!", "Coming soon!"}},
            {"well_done", new[]{"Syabas!", "Well done!"}},
            {"did_you_know", new[]{"Tahukah anda?", "Did you know?"}},
            {"stamp_earned", new[]{"Cop pasport baharu!", "New passport stamp!"}},
            {"try_again", new[]{"Cuba lagi!", "Try again!"}},
            {"great", new[]{"Hebat!", "Great!"}},
            {"easier_now", new[]{"Jom cuba yang lebih mudah!", "Let's try an easier one!"}},
            {"easy", new[]{"Mudah", "Easy"}},
            {"medium", new[]{"Sederhana", "Medium"}},
            {"hard", new[]{"Sukar", "Hard"}},
            {"choose_level", new[]{"Pilih tahap", "Choose a level"}},
            {"achievement", new[]{"Pencapaian", "Achievement"}},
            {"need_stamps", new[]{"Perlukan {0} cop pasport", "Needs {0} passport stamps"}},
            {"welcome_to", new[]{"Selamat datang ke {0}!", "Welcome to {0}!"}},
            {"travel_plane", new[]{"Terbang dengan kapal terbang...", "Flying by plane..."}},
            {"travel_train", new[]{"Menaiki kereta api ETS...", "Riding the ETS train..."}},
            {"travel_ferry", new[]{"Belayar dengan feri...", "Sailing on the ferry..."}},
            {"travel_car", new[]{"Memandu kereta...", "Driving by car..."}},
            {"five_min_left", new[]{"Tinggal 5 minit lagi untuk bermain!", "5 minutes of play left!"}},
            {"time_to_rest", new[]{"Masa untuk berehat!", "Time to rest!"}},
            {"rest_body", new[]{"Kamu sudah bermain dengan hebat hari ini. Jumpa lagi esok!", "You played brilliantly today. See you tomorrow!"}},
            {"grownup_more_time", new[]{"Orang dewasa: +15 minit", "Grown-up: +15 minutes"}},
            {"for_grownups", new[]{"Untuk orang dewasa", "For grown-ups"}},
            {"wearing", new[]{"Dipakai", "Wearing"}},
            {"free", new[]{"Percuma", "Free"}},
            {"none", new[]{"Tiada", "None"}},
            {"buy_confirm", new[]{"Beli {0} dengan {1} syiling?", "Buy {0} for {1} coins?"}},
            {"not_enough", new[]{"Syiling tidak cukup. Main lagi untuk dapatkan syiling!", "Not enough coins. Play more games to earn coins!"}},
            {"badge_needed", new[]{"Kumpul cop pasport untuk membuka item ini!", "Collect passport stamps to unlock this item!"}},
            {"save", new[]{"Simpan", "Save"}},
            {"load", new[]{"Guna", "Use"}},
            {"empty", new[]{"Kosong", "Empty"}},
            {"saved", new[]{"Disimpan!", "Saved!"}},
            {"talk", new[]{"Bercakap", "Talk"}},
            {"read", new[]{"Baca", "Read"}},
            {"quest_done", new[]{"Misi selesai!", "Quest complete!"}},
            {"step", new[]{"Langkah", "Step"}},

            // Tabs
            {"tab_body", new[]{"Badan", "Body"}},
            {"tab_Head", new[]{"Kepala", "Head"}},
            {"tab_Top", new[]{"Baju", "Top"}},
            {"tab_Bottom", new[]{"Bawah", "Bottom"}},
            {"tab_Shoes", new[]{"Kasut", "Shoes"}},
            {"tab_Accessory", new[]{"Aksesori", "Extras"}},
            {"tab_Back", new[]{"Belakang", "Back"}},
            {"tab_Pet", new[]{"Haiwan", "Pet"}},
            {"tab_presets", new[]{"Set", "Saved"}},
            {"skin", new[]{"Warna kulit", "Skin tone"}},
            {"eyes", new[]{"Mata", "Eyes"}},
            {"mouth", new[]{"Mulut", "Mouth"}},
            {"hair_style", new[]{"Gaya rambut", "Hair style"}},
            {"hair_color", new[]{"Warna rambut", "Hair colour"}},
            {"height", new[]{"Ketinggian", "Height"}},
            {"short", new[]{"Rendah", "Short"}},
            {"tall", new[]{"Tinggi", "Tall"}},
            {"create_avatar", new[]{"Cipta Watak Kamu", "Create Your Character"}},

            // Subjects
            {"subj_Maths", new[]{"Matematik", "Maths"}},
            {"subj_Language", new[]{"Bahasa", "Language"}},
            {"subj_Science", new[]{"Sains", "Science"}},
            {"subj_History", new[]{"Sejarah & Budaya", "History & Culture"}},
            {"subj_Logic", new[]{"Logik", "Logic"}},

            // Achievements
            {"ach_first_game", new[]{"Pengembara Pertama", "First Adventure"}},
            {"ach_stars_25", new[]{"Pemburu Bintang", "Star Hunter"}},
            {"ach_maths_master", new[]{"Pakar Matematik", "Maths Master"}},
            {"ach_borneo", new[]{"Penjelajah Borneo", "Explored all of Borneo"}},
            {"ach_passport_4", new[]{"Pemegang Pasport", "Passport Holder"}},

            // Parent dashboard
            {"dashboard", new[]{"Papan Pemuka Ibu Bapa", "Parent Dashboard"}},
            {"played_today", new[]{"Main hari ini: {0} minit", "Played today: {0} min"}},
            {"daily_limit", new[]{"Had harian", "Daily limit"}},
            {"unlimited", new[]{"Tiada had", "No limit"}},
            {"language", new[]{"Bahasa", "Language"}},
            {"voice", new[]{"Suara", "Voice-over"}},
            {"sound", new[]{"Bunyi", "Sound"}},
            {"unlock_all", new[]{"Buka semua negeri", "Unlock all states"}},
            {"multiplayer", new[]{"Main dalam talian", "Online play"}},
            {"online_later", new[]{"Mati (akan datang)", "Off (coming later)"}},
            {"on", new[]{"Hidup", "On"}},
            {"off", new[]{"Mati", "Off"}},
            {"report", new[]{"Laporan Pembelajaran", "Learning Report"}},
            {"delete_data", new[]{"Padam semua data", "Delete all data"}},
            {"delete_confirm", new[]{"Padam semua kemajuan? Tindakan ini tidak boleh dibatalkan.", "Delete all progress? This cannot be undone."}},
            {"privacy_note", new[]{"Tiada data peribadi dikumpul. Kemajuan disimpan dalam peranti ini.", "No personal data is collected. Progress is stored on this device."}},
            {"age_band", new[]{"Umur anak", "Child's age"}},
            {"choose_language", new[]{"Pilih bahasa", "Choose a language"}},
        };

        public static string Get(string key)
        {
            if (key != null && T.TryGetValue(key, out var v)) return IsMalay ? v[0] : v[1];
            return key;
        }

        public static string Format(string key, params object[] args) => string.Format(Get(key), args);

        public static string Pick(string ms, string en)
        {
            if (IsMalay) return string.IsNullOrEmpty(ms) ? en : ms;
            return string.IsNullOrEmpty(en) ? ms : en;
        }

        public static string SubjectName(Subject s) => Get("subj_" + s);

        public static string DifficultyName(Difficulty d) => Get(d == Difficulty.Easy ? "easy" : d == Difficulty.Medium ? "medium" : "hard");
    }
}
