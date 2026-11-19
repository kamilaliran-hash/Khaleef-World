using System;
using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>
    /// Parental gate required by the Google Play Families Policy: a multiplication question written in
    /// words, which young children cannot answer by tapping randomly. Guards the dashboard, purchases,
    /// external links and anything that changes privacy settings.
    /// </summary>
    public static class ParentalGate
    {
        static readonly string[] En = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        static readonly string[] Ms = { "kosong", "satu", "dua", "tiga", "empat", "lima", "enam", "tujuh", "lapan", "sembilan" };

        public static void Show(Action onPass, Action onCancel = null)
        {
            var dim = UIFactory.Overlay("ParentalGate", 1000, out var canvas);
            var go = canvas.gameObject;
            var box = UIFactory.Dialog(dim, new Vector2(1100, 960));

            var title = UIFactory.Label(box, Loc.Get("for_grownups"), 54, UIFactory.Purple);
            UIFactory.Anchor(title.rectTransform, 0.05f, 0.87f, 0.85f, 0.97f);

            var question = UIFactory.Label(box, "", 42, UIFactory.Dark);
            UIFactory.Anchor(question.rectTransform, 0.05f, 0.7f, 0.95f, 0.87f);

            var display = UIFactory.Panel(box, Color.white, "Display");
            UIFactory.Anchor(display.rectTransform, 0.3f, 0.6f, 0.7f, 0.69f);
            var displayText = UIFactory.Label(display.transform, "", 52, UIFactory.Dark);
            UIFactory.Stretch(displayText.rectTransform);

            int answer = 0, attempts = 0;
            string typed = "";

            void NewQuestion()
            {
                int a = UnityEngine.Random.Range(3, 10), b = UnityEngine.Random.Range(3, 10);
                answer = a * b;
                question.text = $"What is {En[a]} times {En[b]}?\n<i>Berapakah {Ms[a]} darab {Ms[b]}?</i>";
                typed = "";
                displayText.text = "";
            }

            void Close(bool passed)
            {
                UnityEngine.Object.Destroy(go);
                if (passed) onPass?.Invoke(); else onCancel?.Invoke();
            }

            var close = UIFactory.Button(box, "X", UIFactory.Grey, () => Close(false), 44);
            UIFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(100, 100), new Vector2(-70, -70));

            var pad = UIFactory.CreateRect("Keypad", box);
            UIFactory.Anchor(pad, 0.2f, 0.04f, 0.8f, 0.57f);
            var grid = pad.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(190, 105);
            grid.spacing = new Vector2(18, 16);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "<", "0", "OK" };
            foreach (var k in keys)
            {
                string key = k;
                Color c = key == "OK" ? UIFactory.Green : key == "<" ? UIFactory.Orange : UIFactory.Blue;
                UIFactory.Button(pad, key, c, () =>
                {
                    if (key == "<") { if (typed.Length > 0) typed = typed.Substring(0, typed.Length - 1); }
                    else if (key == "OK")
                    {
                        if (int.TryParse(typed, out int v) && v == answer) { Close(true); return; }
                        attempts++;
                        if (attempts >= 3) { Close(false); return; }
                        NewQuestion();
                        UIFactory.Toast(Loc.Get("try_again"), 1.5f);
                        return;
                    }
                    else if (typed.Length < 3) typed += key;
                    displayText.text = typed;
                }, 48);
            }

            NewQuestion();
        }
    }
}
