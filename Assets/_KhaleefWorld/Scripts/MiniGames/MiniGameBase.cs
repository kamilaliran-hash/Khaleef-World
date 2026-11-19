using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    public struct MiniGameResult
    {
        public int stars;
        public int coins;
        public bool quit;
        public bool playAgain;
        public bool stampEarned;
    }

    /// <summary>
    /// Base class for every mini-game. Handles the shared lifecycle so each game only writes its own
    /// gameplay:
    ///   intro card (with level picker) -> OnBegin() -> RegisterAnswer()... -> Finish()
    ///   -> stars / coins / passport stamp -> results card with a fun fact.
    /// Also implements the in-game safety net: three misses in a row drops one difficulty level.
    /// </summary>
    public abstract class MiniGameBase : MonoBehaviour
    {
        public MiniGameDefinition Definition { get; private set; }
        public StateDefinition State { get; private set; }
        public Difficulty CurrentDifficulty { get; protected set; }
        public event Action<MiniGameResult> Completed;

        /// <summary>Area below the header where the game builds its UI.</summary>
        protected RectTransform Content { get; private set; }
        protected RectTransform Root { get; private set; }
        protected bool Running { get; private set; }
        protected int Correct { get; private set; }
        protected int Attempts { get; private set; }
        protected float ElapsedTime => Running || finished ? Time.time - startTime : 0f;
        protected virtual float Accuracy => Attempts == 0 ? 1f : Correct / (float)Attempts;

        /// <summary>Override to false for games where misses are expected (e.g. memory cards).</summary>
        protected virtual bool AutoLowerDifficulty => true;

        int missStreak;
        float startTime;
        bool finished;
        Text instructionLabel;
        RectTransform difficultyBadge;
        GameManager GM => GameManager.I;

        // ---------- Lifecycle ----------

        public void Begin(MiniGameDefinition def, StateDefinition state, RectTransform canvasRoot)
        {
            Definition = def;
            State = state;
            CurrentDifficulty = GM.Difficulty.Get(def.subject);

            Color subjectColor = UIFactory.SubjectColor(def.subject);
            var bg = UIFactory.Panel(canvasRoot, Color.Lerp(subjectColor, Color.white, 0.75f), "MiniGameBg", false);
            UIFactory.Stretch(bg.rectTransform);
            Root = bg.rectTransform;

            var header = UIFactory.Panel(Root, subjectColor, "Header", false);
            UIFactory.Anchor(header.rectTransform, 0f, 0.87f, 1f, 1f);
            var title = UIFactory.Label(header.transform, def.Title, 52, Color.white, TextAnchor.MiddleLeft);
            UIFactory.Anchor(title.rectTransform, 0.03f, 0f, 0.6f, 1f);
            var subj = UIFactory.Label(header.transform, Loc.SubjectName(def.subject), 32, Color.white, TextAnchor.MiddleRight);
            UIFactory.Anchor(subj.rectTransform, 0.55f, 0f, 0.76f, 1f);

            difficultyBadge = UIFactory.CreateRect("Difficulty", header.transform);
            UIFactory.Anchor(difficultyBadge, 0.77f, 0.1f, 0.9f, 0.9f);
            UpdateDifficultyBadge();

            var close = UIFactory.Button(header.transform, "X", UIFactory.Red, Quit, 44);
            UIFactory.Place((RectTransform)close.transform, new Vector2(1, 0.5f), new Vector2(110, 100), new Vector2(-75, 0));

            instructionLabel = UIFactory.Label(Root, Loc.Pick(def.instructionMS, def.instructionEN), 36, UIFactory.Dark);
            UIFactory.Anchor(instructionLabel.rectTransform, 0.03f, 0.79f, 0.97f, 0.87f);

            Content = UIFactory.CreateRect("Content", Root);
            UIFactory.Anchor(Content, 0.02f, 0.02f, 0.98f, 0.78f);

            ShowIntro();
        }

        /// <summary>Build gameplay UI inside Content and start playing.</summary>
        protected abstract void OnBegin();

        protected virtual void OnTick(float dt) { }

        /// <summary>Called when the safety net lowers difficulty mid-game.</summary>
        protected virtual void OnDifficultyLowered() { }

        void Update()
        {
            if (Running) OnTick(Time.deltaTime);
        }

        void ShowIntro()
        {
            var dim = UIFactory.Panel(Root, UIFactory.Dim, "IntroDim", false);
            UIFactory.Stretch(dim.rectTransform);
            var card = UIFactory.Dialog(dim.transform, new Vector2(1300, 780));

            var t = UIFactory.Label(card, Definition.Title, 70, UIFactory.SubjectColor(Definition.subject));
            UIFactory.Anchor(t.rectTransform, 0.05f, 0.78f, 0.95f, 0.97f);
            var ins = UIFactory.Label(card, Loc.Pick(Definition.instructionMS, Definition.instructionEN), 40, UIFactory.Dark);
            UIFactory.Anchor(ins.rectTransform, 0.06f, 0.5f, 0.94f, 0.78f);

            var lvlLabel = UIFactory.Label(card, Loc.Get("choose_level"), 34, UIFactory.Grey);
            UIFactory.Anchor(lvlLabel.rectTransform, 0.1f, 0.4f, 0.9f, 0.5f);

            var row = UIFactory.CreateRect("Levels", card);
            UIFactory.Anchor(row, 0.08f, 0.24f, 0.92f, 0.4f);
            UIFactory.HLayout(row, 24);
            var buttons = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var d = (Difficulty)i;
                var b = UIFactory.Button(row, Loc.DifficultyName(d), UIFactory.Grey, null, 38);
                buttons[i] = (Image)b.targetGraphic;
                b.onClick.AddListener(() =>
                {
                    CurrentDifficulty = d;
                    for (int j = 0; j < 3; j++) buttons[j].color = j == (int)d ? UIFactory.Orange : UIFactory.Grey;
                    UpdateDifficultyBadge();
                });
            }
            buttons[(int)CurrentDifficulty].color = UIFactory.Orange;

            var start = UIFactory.Button(card, Loc.Get("start"), UIFactory.Green, () =>
            {
                Destroy(dim.gameObject);
                startTime = Time.time;
                Running = true;
                OnBegin();
            }, 60);
            UIFactory.Anchor((RectTransform)start.transform, 0.3f, 0.04f, 0.7f, 0.2f);
        }

        // ---------- Scoring helpers for subclasses ----------

        protected void RegisterAnswer(bool correct)
        {
            Attempts++;
            if (correct)
            {
                Correct++;
                missStreak = 0;
                GM.Audio.Play(Sfx.Correct);
                return;
            }

            missStreak++;
            GM.Audio.Play(Sfx.Wrong);
            if (AutoLowerDifficulty && missStreak >= 3 && CurrentDifficulty != Difficulty.Easy)
            {
                CurrentDifficulty = DifficultyManager.Lower(CurrentDifficulty);
                missStreak = 0;
                UpdateDifficultyBadge();
                ShowHint(Loc.Get("easier_now"));
                OnDifficultyLowered();
            }
        }

        protected void SetInstruction(string ms, string en)
        {
            if (instructionLabel != null) instructionLabel.text = Loc.Pick(ms, en);
        }

        /// <summary>Default: 90%+ accuracy = 3 stars, 70%+ = 2 stars, otherwise 1 (effort always rewarded).</summary>
        protected virtual int CalculateStars()
        {
            float a = Accuracy;
            return a >= 0.9f ? 3 : a >= 0.7f ? 2 : 1;
        }

        protected void ShowHint(string message, float seconds = 2.2f)
        {
            var bubble = UIFactory.Panel(Root, new Color(0.13f, 0.15f, 0.22f, 0.9f), "Hint");
            UIFactory.Place(bubble.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1000, 150), Vector2.zero);
            bubble.raycastTarget = false;
            var t = UIFactory.Label(bubble.transform, message, 42, Color.white);
            UIFactory.Stretch(t.rectTransform, 14);
            bubble.gameObject.AddComponent<AutoDestroy>().seconds = seconds;
        }

        protected void Finish()
        {
            if (!Running) return;
            Running = false;
            finished = true;

            int stars = Mathf.Clamp(CalculateStars(), 1, 3);
            float speedBonus = Accuracy >= 0.9f && ElapsedTime < 60f ? 0.02f : 0f;
            GM.Difficulty.Report(Definition.subject, Accuracy, speedBonus);
            int coins = GM.Rewards.RecordMiniGame(Definition, stars, State, out bool stamp);
            GM.Audio.Play(Sfx.Fanfare);
            StartCoroutine(ShowResultsAfter(0.4f, stars, coins, stamp));
        }

        IEnumerator ShowResultsAfter(float delay, int stars, int coins, bool stamp)
        {
            yield return new WaitForSeconds(delay);
            ShowResults(new MiniGameResult { stars = stars, coins = coins, stampEarned = stamp });
        }

        void ShowResults(MiniGameResult result)
        {
            var dim = UIFactory.Panel(Root, UIFactory.Dim, "ResultsDim", false);
            UIFactory.Stretch(dim.rectTransform);
            var card = UIFactory.Dialog(dim.transform, new Vector2(1400, 900));

            var t = UIFactory.Label(card, Loc.Get("well_done"), 80, UIFactory.Purple);
            UIFactory.Anchor(t.rectTransform, 0.05f, 0.84f, 0.95f, 0.98f);

            var starRow = UIFactory.StarRow(card, result.stars, 90);
            UIFactory.Anchor(starRow, 0.3f, 0.68f, 0.7f, 0.84f);

            var coinsText = UIFactory.Label(card, "+" + result.coins + " " + Loc.Get("coins"), 50, UIFactory.Orange);
            UIFactory.Anchor(coinsText.rectTransform, 0.2f, 0.58f, 0.8f, 0.68f);

            var factBox = UIFactory.Panel(card, new Color(0.9f, 0.96f, 1f), "FunFact");
            UIFactory.Anchor(factBox.rectTransform, 0.05f, 0.22f, 0.95f, 0.56f);
            var fh = UIFactory.Label(factBox.transform, Loc.Get("did_you_know"), 40, UIFactory.Blue);
            UIFactory.Anchor(fh.rectTransform, 0.03f, 0.72f, 0.97f, 0.97f);
            var ft = UIFactory.Label(factBox.transform, Loc.Pick(Definition.funFactMS, Definition.funFactEN), 36, UIFactory.Dark);
            UIFactory.Anchor(ft.rectTransform, 0.04f, 0.05f, 0.96f, 0.72f);

            if (result.stampEarned)
            {
                var stamp = UIFactory.Panel(card, State != null ? State.color : UIFactory.Yellow, "Stamp");
                UIFactory.Place(stamp.rectTransform, new Vector2(1, 1), new Vector2(380, 120), new Vector2(-170, -40));
                stamp.rectTransform.localRotation = Quaternion.Euler(0, 0, -12);
                var st = UIFactory.Label(stamp.transform, Loc.Get("stamp_earned"), 30, Color.white);
                UIFactory.Stretch(st.rectTransform, 8);
            }

            var again = UIFactory.Button(card, Loc.Get("play_again"), UIFactory.Blue, () =>
            {
                result.playAgain = true;
                Completed?.Invoke(result);
            }, 44);
            UIFactory.Anchor((RectTransform)again.transform, 0.08f, 0.04f, 0.46f, 0.18f);
            var cont = UIFactory.Button(card, Loc.Get("continue"), UIFactory.Green, () => Completed?.Invoke(result), 48);
            UIFactory.Anchor((RectTransform)cont.transform, 0.54f, 0.04f, 0.92f, 0.18f);
        }

        public void Quit()
        {
            Running = false;
            Completed?.Invoke(new MiniGameResult { quit = true });
        }

        void UpdateDifficultyBadge()
        {
            if (difficultyBadge == null) return;
            UIFactory.Clear(difficultyBadge);
            var row = UIFactory.StarRow(difficultyBadge, (int)CurrentDifficulty + 1, 34);
            UIFactory.Stretch(row);
        }
    }
}
