using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>
    /// SAMPLE MINI-GAME 1 – "272 Steps" at Batu Caves (Selangor).
    /// Answer maths questions to climb the rainbow staircase to the cave. One reusable template covers
    /// counting, skip-counting, addition, subtraction, multiplication and Ringgit & sen money questions,
    /// so the same class also powers KL's Tower Count, Penang Hill train maths and Kelantan's market game.
    ///
    ///   Easy   – small numbers, 3 picture-sized answer buttons, auto-hint after a miss
    ///   Medium – bigger numbers, 4 answers
    ///   Hard   – largest numbers; type the answer on a keypad
    /// </summary>
    public class MathsClimbGame : MiniGameBase
    {
        struct Question
        {
            public string prompt;
            public string answer;
            public string hint;
            public List<string> options;
            public bool numeric;
        }

        int rounds, round;
        bool locked;
        Question current;
        string typed = "";

        RectTransform track, climber, answersRoot;
        Text questionLabel, stepLabel, typedLabel;
        readonly List<RectTransform> steps = new List<RectTransform>();

        static readonly Color[] Rainbow =
        {
            new Color(0.95f, 0.3f, 0.3f), new Color(1f, 0.6f, 0.2f), new Color(1f, 0.85f, 0.2f),
            new Color(0.4f, 0.8f, 0.35f), new Color(0.25f, 0.6f, 0.95f), new Color(0.55f, 0.4f, 0.9f),
            new Color(0.95f, 0.45f, 0.7f),
        };

        static int R(int min, int maxInclusive) => Random.Range(min, maxInclusive + 1);

        protected override void OnBegin()
        {
            rounds = CurrentDifficulty == Difficulty.Easy ? 6 : CurrentDifficulty == Difficulty.Medium ? 8 : 10;
            round = 0;
            BuildStaircase();
            BuildQuestionArea();
            NextQuestion();
        }

        // ---------- Layout ----------

        void BuildStaircase()
        {
            var panel = UIFactory.Panel(Content, new Color(0.82f, 0.74f, 0.62f), "Hill");
            UIFactory.Anchor(panel.rectTransform, 0f, 0f, 0.34f, 1f);

            var cave = UIFactory.Panel(panel.transform, new Color(0.25f, 0.2f, 0.18f), "Cave");
            UIFactory.Anchor(cave.rectTransform, 0.3f, 0.86f, 0.7f, 0.99f);
            var caveLabel = UIFactory.Label(cave.transform, Loc.Pick("Puncak!", "The top!"), 30, Color.white);
            UIFactory.Stretch(caveLabel.rectTransform);

            track = UIFactory.CreateRect("Track", panel.transform);
            UIFactory.Anchor(track, 0.08f, 0.1f, 0.92f, 0.85f);
            steps.Clear();
            for (int i = 0; i < rounds; i++)
            {
                var s = UIFactory.Panel(track, Rainbow[i % Rainbow.Length], "Step" + i, false);
                float y0 = i / (float)rounds, y1 = (i + 0.82f) / (float)rounds;
                float inset = 0.12f + 0.25f * (i / (float)rounds);      // narrows toward the top
                UIFactory.Anchor(s.rectTransform, inset, y0, 1f - inset, y1);
                steps.Add(s.rectTransform);
            }

            // Little blocky climber
            climber = UIFactory.CreateRect("Climber", panel.transform);
            climber.sizeDelta = new Vector2(70, 110);
            var save = GameManager.I.Save.avatar;
            var cfg = GameManager.I.Config;
            var top = cfg != null ? cfg.FindItem(save.Get(CosmeticSlot.Top)) : null;
            var body = UIFactory.Panel(climber, top != null ? top.primary : Color.white, "Body", false);
            UIFactory.Anchor(body.rectTransform, 0.1f, 0f, 0.9f, 0.55f);
            var head = UIFactory.Panel(climber, AvatarBuilder.SkinTones[Mathf.Clamp(save.skinTone, 0, AvatarBuilder.SkinTones.Length - 1)], "Head", false);
            UIFactory.Anchor(head.rectTransform, 0.15f, 0.55f, 0.85f, 1f);
            MoveClimberInstant(0);

            var stepBox = UIFactory.Panel(panel.transform, new Color(1, 1, 1, 0.85f), "StepCount");
            UIFactory.Anchor(stepBox.rectTransform, 0.05f, 0.01f, 0.95f, 0.09f);
            stepLabel = UIFactory.Label(stepBox.transform, "", 32, UIFactory.Dark);
            UIFactory.Stretch(stepLabel.rectTransform, 4);
            UpdateStepLabel();
        }

        void BuildQuestionArea()
        {
            var qBox = UIFactory.Panel(Content, Color.white, "Question");
            UIFactory.Anchor(qBox.rectTransform, 0.37f, 0.6f, 1f, 1f);
            questionLabel = UIFactory.Label(qBox.transform, "", 72, UIFactory.Dark);
            UIFactory.Stretch(questionLabel.rectTransform, 20);

            answersRoot = UIFactory.CreateRect("Answers", Content);
            UIFactory.Anchor(answersRoot, 0.37f, 0f, 1f, 0.57f);
        }

        Vector2 StepPosition(int index)
        {
            Canvas.ForceUpdateCanvases();
            var parent = (RectTransform)climber.parent;
            if (index <= 0 || steps.Count == 0)
                return new Vector2(0, -parent.rect.height * 0.5f + parent.rect.height * 0.1f + 55f);
            var s = steps[Mathf.Clamp(index - 1, 0, steps.Count - 1)];
            Vector3 world = s.TransformPoint(new Vector3(s.rect.center.x, s.rect.yMax, 0));
            Vector3 local = parent.InverseTransformPoint(world);
            return new Vector2(0, local.y + 55f);
        }

        void MoveClimberInstant(int index)
        {
            climber.anchorMin = climber.anchorMax = new Vector2(0.5f, 0.5f);
            climber.anchoredPosition = StepPosition(index);
        }

        IEnumerator MoveClimber(int index)
        {
            Vector2 from = climber.anchoredPosition, to = StepPosition(index);
            for (float t = 0; t < 1f; t += Time.deltaTime * 3f)
            {
                climber.anchoredPosition = Vector2.Lerp(from, to, t) + Vector2.up * Mathf.Sin(t * Mathf.PI) * 40f;
                yield return null;
            }
            climber.anchoredPosition = to;
        }

        void UpdateStepLabel()
        {
            int value = Mathf.RoundToInt(Definition.climbTarget * (round / (float)Mathf.Max(1, rounds)));
            stepLabel.text = $"{value} / {Definition.climbTarget} {Loc.Pick(Definition.climbUnitMS, Definition.climbUnitEN)}";
        }

        // ---------- Questions ----------

        void NextQuestion()
        {
            current = Generate(Definition.mathMode, CurrentDifficulty);
            questionLabel.text = current.prompt;
            typed = "";
            UIFactory.Clear(answersRoot);
            if (CurrentDifficulty == Difficulty.Hard && current.numeric) BuildKeypad();
            else BuildOptions();
            locked = false;
        }

        void BuildOptions()
        {
            var grid = UIFactory.CreateRect("Options", answersRoot);
            UIFactory.Stretch(grid);
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            int n = current.options.Count;
            g.cellSize = n == 3 ? new Vector2(330, 260) : new Vector2(520, 170);
            g.spacing = new Vector2(30, 30);
            g.childAlignment = TextAnchor.MiddleCenter;
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = n == 3 ? 3 : 2;

            foreach (var opt in current.options)
            {
                string value = opt;
                Button b = null;
                b = UIFactory.Button(grid, value, UIFactory.Blue, () => OnAnswer(value, b), 64);
            }
        }

        void BuildKeypad()
        {
            var display = UIFactory.Panel(answersRoot, Color.white, "Typed");
            UIFactory.Anchor(display.rectTransform, 0.25f, 0.8f, 0.75f, 1f);
            typedLabel = UIFactory.Label(display.transform, "", 60, UIFactory.Dark);
            UIFactory.Stretch(typedLabel.rectTransform);

            var pad = UIFactory.CreateRect("Keypad", answersRoot);
            UIFactory.Anchor(pad, 0.05f, 0f, 0.95f, 0.76f);
            var g = pad.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(170, 92);
            g.spacing = new Vector2(14, 12);
            g.childAlignment = TextAnchor.MiddleCenter;
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 6;
            string[] keys = { "1", "2", "3", "4", "5", "<", "6", "7", "8", "9", "0", "OK" };
            foreach (var k in keys)
            {
                string key = k;
                Color c = key == "OK" ? UIFactory.Green : key == "<" ? UIFactory.Orange : UIFactory.Blue;
                UIFactory.Button(pad, key, c, () =>
                {
                    if (locked) return;
                    if (key == "<") { if (typed.Length > 0) typed = typed.Substring(0, typed.Length - 1); }
                    else if (key == "OK") { if (typed.Length > 0) OnAnswer(typed, null); return; }
                    else if (typed.Length < 4) typed += key;
                    typedLabel.text = typed;
                }, 48);
            }
        }

        void OnAnswer(string chosen, Button button)
        {
            if (locked || !Running) return;
            bool ok = chosen == current.answer;
            RegisterAnswer(ok);
            if (ok)
            {
                locked = true;
                if (button != null) ((Image)button.targetGraphic).color = UIFactory.Green;
                round++;
                UpdateStepLabel();
                StartCoroutine(MoveClimber(round));
                StartCoroutine(AfterCorrect());
            }
            else
            {
                if (button != null) { ((Image)button.targetGraphic).color = UIFactory.Grey; button.interactable = false; }
                typed = "";
                if (typedLabel != null) typedLabel.text = "";
                if (CurrentDifficulty == Difficulty.Easy || Attempts % 2 == 0) ShowHint(current.hint);
                else ShowHint(Loc.Get("try_again"), 1.2f);
            }
        }

        IEnumerator AfterCorrect()
        {
            yield return new WaitForSeconds(0.7f);
            if (round >= rounds) Finish();
            else NextQuestion();
        }

        protected override void OnDifficultyLowered()
        {
            if (!locked) NextQuestion();
        }

        Question Generate(MathMode mode, Difficulty d)
        {
            var q = new Question { numeric = true };
            int ans;
            switch (mode)
            {
                case MathMode.NextNumber:
                {
                    int start = d == Difficulty.Easy ? R(1, 15) : d == Difficulty.Medium ? R(10, 90) : R(100, 265);
                    ans = start + 3;
                    q.prompt = $"{start}, {start + 1}, {start + 2}, ?";
                    q.hint = Loc.Pick($"Kira terus dari {start + 2}...", $"Count on from {start + 2}...");
                    q.options = NumberOptions(ans, 1, d);
                    break;
                }
                case MathMode.SkipCount:
                {
                    int[] stepsByLevel = d == Difficulty.Easy ? new[] { 2 } : d == Difficulty.Medium ? new[] { 5, 10 } : new[] { 3, 4, 25 };
                    int step = stepsByLevel[R(0, stepsByLevel.Length - 1)];
                    int start = step * R(1, d == Difficulty.Hard ? 9 : 6);
                    ans = start + step * 3;
                    q.prompt = $"{start}, {start + step}, {start + step * 2}, ?";
                    q.hint = Loc.Pick($"Tambah {step} setiap kali!", $"Add {step} each time!");
                    q.options = NumberOptions(ans, step, d);
                    break;
                }
                case MathMode.Addition:
                {
                    int a = d == Difficulty.Easy ? R(1, 9) : d == Difficulty.Medium ? R(10, 50) : R(20, 150);
                    int b = d == Difficulty.Easy ? R(1, 9) : d == Difficulty.Medium ? R(5, 40) : R(20, 120);
                    ans = a + b;
                    q.prompt = $"{a} + {b} = ?";
                    q.hint = Loc.Pick($"Mula dengan {a}, kemudian kira {b} lagi.", $"Start at {a}, then count {b} more.");
                    q.options = NumberOptions(ans, d == Difficulty.Easy ? 1 : 10, d);
                    break;
                }
                case MathMode.Subtraction:
                {
                    int a = d == Difficulty.Easy ? R(5, 15) : d == Difficulty.Medium ? R(20, 99) : R(100, 272);
                    int b = R(1, d == Difficulty.Easy ? a - 1 : a / 2);
                    ans = a - b;
                    q.prompt = $"{a} - {b} = ?";
                    q.hint = Loc.Pick($"Kira ke belakang {b} dari {a}.", $"Count back {b} from {a}.");
                    q.options = NumberOptions(ans, d == Difficulty.Easy ? 1 : 10, d);
                    break;
                }
                case MathMode.Multiplication:
                {
                    int[] tables = d == Difficulty.Easy ? new[] { 2, 5, 10 } : d == Difficulty.Medium ? new[] { 2, 3, 4, 5 } : new[] { 6, 7, 8, 9 };
                    int a = tables[R(0, tables.Length - 1)];
                    int b = d == Difficulty.Easy ? R(1, 5) : d == Difficulty.Medium ? R(2, 10) : R(2, 12);
                    ans = a * b;
                    q.prompt = $"{a} x {b} = ?";
                    q.hint = Loc.Pick($"{b} kumpulan, setiap satu ada {a}.", $"{b} groups of {a}.");
                    q.options = NumberOptions(ans, a, d);
                    break;
                }
                default: // Money: Ringgit & sen
                    return MoneyQuestion(d);
            }
            q.answer = ans.ToString();
            return q;
        }

        Question MoneyQuestion(Difficulty d)
        {
            var q = new Question { numeric = false };
            int priceSen, answerSen;
            if (d == Difficulty.Easy)
            {
                int a = R(1, 5), b = R(1, 4);
                answerSen = (a + b) * 100;
                q.prompt = $"RM{a} + RM{b} = ?";
                q.hint = Loc.Pick("Tambah ringgit dengan ringgit.", "Add the ringgit together.");
            }
            else if (d == Difficulty.Medium)
            {
                int a = R(1, 6);
                int[] sens = { 10, 20, 50 };
                int s = sens[R(0, 2)] * R(1, 2);
                answerSen = a * 100 + s;
                q.prompt = Loc.Pick($"RM{a} dan {s} sen = ?", $"RM{a} and {s} sen = ?");
                q.hint = Loc.Pick("100 sen = RM1", "100 sen = RM1");
            }
            else
            {
                priceSen = R(3, 18) * 50;          // RM1.50 .. RM9.00
                int paid = priceSen <= 500 ? 500 : 1000;
                answerSen = paid - priceSen;
                q.prompt = Loc.Pick($"Harga {Rm(priceSen)}. Kamu bayar {Rm(paid)}.\nBerapa baki?",
                                    $"It costs {Rm(priceSen)}. You pay {Rm(paid)}.\nHow much change?");
                q.hint = Loc.Pick("Tolak harga daripada wang yang dibayar.", "Take the price away from what you paid.");
            }
            q.answer = Rm(answerSen);

            var set = new List<int> { answerSen };
            int[] deltas = { 50, -50, 100, -100, 20, -20, 10, -10 };
            int tries = 0;
            while (set.Count < (d == Difficulty.Easy ? 3 : 4) && tries++ < 40)
            {
                int v = answerSen + deltas[R(0, deltas.Length - 1)] * R(1, 2);
                if (v > 0 && !set.Contains(v)) set.Add(v);
            }
            q.options = new List<string>();
            foreach (var v in set) q.options.Add(Rm(v));
            Shuffle(q.options);
            return q;
        }

        static string Rm(int sen) => sen % 100 == 0 ? $"RM{sen / 100}" : $"RM{sen / 100}.{sen % 100:00}";

        static List<string> NumberOptions(int answer, int spread, Difficulty d)
        {
            int count = d == Difficulty.Easy ? 3 : 4;
            var set = new List<int> { answer };
            int tries = 0;
            while (set.Count < count && tries++ < 50)
            {
                int offset = R(1, 3) * Mathf.Max(1, spread) * (Random.value < 0.5f ? -1 : 1);
                int v = answer + offset;
                if (v >= 0 && !set.Contains(v)) set.Add(v);
            }
            while (set.Count < count) set.Add(answer + set.Count * 7);
            var list = new List<string>();
            foreach (var v in set) list.Add(v.ToString());
            Shuffle(list);
            return list;
        }

        static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
