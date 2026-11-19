# Khaleef World – Unity project (MVP)

A blocky 3D sandbox where kids explore Malaysia and learn through mini-games.
This package has the full **MVP code** plus an editor wizard that **generates all the data and the
playable scenes**, so you can go from an empty Unity project to an Android APK without making any art first.

**What's playable in this MVP**

| Location | Mini-games |
|---|---|
| Kuala Lumpur | Tower Count (addition) · KL Tower Lift (times tables) · Merdeka Moments (history matching) |
| Selangor | **272 Steps at Batu Caves** (skip-counting climb) · Animal Friends (BM ↔ EN) · i-City Lights (addition) |
| Kelantan | **Wau Maker** (build & fly a kite, wind physics) · Siti Khadijah Market (Ringgit & sen) · Shadow Play Words |
| Penang (bonus) | **Hawker Memory** (memory cards) · Penang Hill Train (subtraction) · Street Words |

Also included: character creator with 60+ cosmetics (traditional Malaysian clothing, careers, pets),
the World Map with all 16 locations (12 show "Coming soon"), passport stamps, badges, daily quests,
achievements, adaptive difficulty, a parental gate, a parent dashboard and screen-time limits.

---

## 1. Requirements

- **Unity 6 LTS (6000.0.x)** installed through Unity Hub, with the **Android Build Support** module
  (tick **Android SDK & NDK Tools** and **OpenJDK** when adding it). Unity 2022.3 LTS should also work.
- An Android phone (Android 8.0 or newer) with **Developer options → USB debugging** turned on, for testing.

## 2. Create the project

1. Unity Hub → **New project** → template **Universal 3D** (URP) → name it `KhaleefWorld`.
2. Close Unity. Copy this package's `Assets/_KhaleefWorld` folder into the new project's `Assets/` folder.
3. Open the project again. Wait for scripts to compile (there should be no red errors in the Console).

## 3. Generate the game content

Menu **Khaleef World → 1. Create MVP Content (data + scenes)**.

This creates, under `Assets/_KhaleefWorld/`:
- `Data/Items` (cosmetics), `Data/States` (16 locations), `Data/MiniGames` (12 games)
- `Resources/KhaleefConfig.asset` (the global config the game loads)
- `Materials/` (URP Lit colour materials)
- `Scenes/` – Boot, CharacterCreator, WorldMap, KualaLumpur, Selangor, Kelantan, Penang (added to Build Settings)

Press **Play** in the `Boot` scene to try it in the Editor (WASD/arrows to move, Space to jump,
E to use, mouse drag on the right half to look around).

## 4. Build the APK

1. **Khaleef World → 3. Build APK (test on a phone)**.
   It switches the platform to Android, applies the player settings (landscape, Android 8.0+, IL2CPP,
   ARM64 + ARMv7) and writes `Builds/Android/KhaleefWorld.apk`.
2. Install it: copy the APK to the phone and open it, or run
   `adb install -r Builds/Android/KhaleefWorld.apk`.

For Google Play, use **4. Build AAB**. Before publishing:
- Change `BundleId` in `Editor/KhaleefBuildScript.cs` to your own (e.g. `com.yourstudio.khaleefworld`).
- Create an upload keystore: **Project Settings → Player → Android → Publishing Settings → Keystore Manager**.
- Complete the Play Console **Target audience and content** form (ages 5–8 and 9–12) and the
  **Families Policy** declarations; the game collects no personal data and has no ads.

Command-line / CI build:
```
Unity -batchmode -quit -projectPath . -executeMethod KhaleefWorld.EditorTools.KhaleefBuildScript.BuildApkCommandLine
```

## 5. Project structure

```
Assets/_KhaleefWorld/
├── Editor/                      Editor-only tools (not in the build)
│   ├── KhaleefSetupWizard.cs    Generates materials, data and the blocky scenes
│   ├── KhaleefContent.cs        All MVP text/data: cosmetics, 16 states, 12 mini-games
│   └── KhaleefBuildScript.cs    Android player settings + APK/AAB builds
├── Scripts/
│   ├── Core/        GameManager, SceneLoader, Loc (BM/EN), UIFactory, AudioManager, MaterialCache …
│   ├── Data/        SaveData, SaveSystem, GameConfig, StateDefinition, MiniGameDefinition, CosmeticItem
│   ├── Player/      PlayerController, ThirdPersonCamera, VirtualJoystick, PlayerHUD, Interactable
│   ├── Character/   CharacterCustomizer, CharacterCreatorScreen, AvatarBuilder, AvatarRig, PetFollower
│   ├── World/       WorldMapManager, StateZone, MiniGamePortal, NpcGreeter, CoinPickup, BootScreen, Screens
│   ├── MiniGames/   MiniGameBase (abstract), MiniGameLauncher, MathsClimbGame, MemoryMatchGame, WauKiteGame
│   ├── Progression/ RewardManager, DifficultyManager, QuestManager
│   └── Parental/    ParentalControlManager, ParentalGate, ParentDashboardScreen
├── Data/            (generated) ScriptableObject assets
├── Materials/       (generated)
├── Resources/       (generated) KhaleefConfig.asset
└── Scenes/          (generated) 7 scenes
```

Recommended folders as the project grows (add when you bring in real art and audio):
```
Art/Characters  Art/Environments/<State>  Art/UI  Audio/Music  Audio/SFX  Audio/VO/{ms,en}
Prefabs/  AddressableGroups/  Localization/  Tests/EditMode  Tests/PlayMode
```

## 6. How the code fits together

- **GameManager** is created automatically before the first scene loads, so any scene can be played on
  its own. It owns the save file and the services (rewards, difficulty, quests, parental controls, audio).
- **MiniGameBase** handles the shared lifecycle: intro card with level picker → `OnBegin()` →
  `RegisterAnswer(bool)` → `Finish()` → stars, coins, passport stamp, results card with a fun fact.
  It also drops one level after three misses in a row. A new mini-game only overrides `OnBegin`,
  and optionally `OnTick`, `CalculateStars` and `Accuracy`.
- **Adding a mini-game**: create a class deriving from `MiniGameBase`, add a value to
  `MiniGameTemplate`, add one `case` in `MiniGameLauncher`, then create a `MiniGameDefinition` asset and
  place a `MiniGamePortal` in a state scene.
- **Adding a state**: set `playable` content in `KhaleefContent.cs` (mini-games list), add a builder
  method in `KhaleefSetupWizard.cs`, re-run the menu. Or build the scene by hand: it only needs a
  `StateZone` (with a spawn point) and some `MiniGamePortal`s.
- **Real art**: set `prefabOverride` on a `CosmeticItem` to use a modelled item instead of the procedural
  blocks; replace landmark blocks in the scenes with modelled prefabs.
- **Addressables / state packs**: install the Addressables package, mark each state scene addressable
  (one group per state, remote load path), and add `KHALEEF_ADDRESSABLES` to *Scripting Define Symbols*.
  `SceneLoader` then streams states instead of loading built-in scenes.

## 7. Not yet in this MVP (next milestones)

- Online multiplayer (Photon Fusion) with preset phrases and parent-approved friend codes
- Firebase anonymous auth + cloud save (the save file is already a single JSON string, ready to sync)
- Voice-over clips, recorded music (kompang, gamelan, sape, erhu) – `StateDefinition.music` is ready
- Mandarin, Tamil and Jawi (needs a TextMeshPro font with those glyphs)
- The other 12 locations and their 36 mini-games
- In-app purchases (cosmetic packs behind the parental gate)
