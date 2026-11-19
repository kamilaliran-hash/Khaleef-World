using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>
    /// SAMPLE MINI-GAME 2 – "Hawker Memory" in Penang.
    /// Flip cards to find matching pairs of famous hawker dishes. The same template handles
    /// "word pair" mode (card A and card B differ, e.g. "Kucing" / "Cat", or "31 Ogos 1957" / "Hari Merdeka"),
    /// which powers the language and history matching games in other states.
    ///
    ///   Easy   – 3 pairs, cards shown for 2.5 s at the start
    ///   Medium – 6 pairs, 1.2 s preview
    ///   Hard   – 8 pairs, no preview
    /// Stars are based on how few flips were needed.
    /// </summary>
    public class MemoryMatchGame : MiniGameBase
    {
        class Card
        {
            public Button button;
            public Image image;
            public Text label;
            public RectTransform rect;
            public int pairId;
            public string face;
            public Color color;
            public bool open, matched;
        }

        readonly List<Card> cards = new List<Card>();
        Card first;
        bool busy;
        int pairsTotal, pairsFound, moves;
        Text movesLabel;

        static readonly Color BackColor = new Color(0.98f, 0.55f, 0.25f);

        protected override bool AutoLowerDifficulty => false;

        /// <summary>Memory games naturally have misses, so accuracy is measured against a "good" move count.</summary>
        protected override float Accuracy => moves == 0 ? 1f : Mathf.Clamp01(pairsTotal * 1.5f / moves);

        protected override void OnBegin()
        {
            int wanted = CurrentDifficulty == Difficulty.Easy ? 3 : CurrentDifficulty == Difficulty.Medium ? 6 : 8;
            var pool = new List<CardPair>(Definition.cards);
            Shuffle(pool);
            pairsTotal = Mathf.Min(wanted, pool.Count);
            pairsFound = 0;
            moves = 0;

            var deck = new List<Card>();
            for (int i = 0; i < pairsTotal; i++)
            {
                var p = pool[i];
                deck.Add(new Card { pairId = i, face = p.a, color = p.color });
                deck.Add(new Card { pairId = i, face = string.IsNullOrEmpty(p.b) ? p.a : p.b, color = p.color });
            }
            Shuffle(deck);

            int cols = pairsTotal <= 3 ? 3 : 4;
            int rows = Mathf.CeilToInt(deck.Count / (float)cols);

            var info = UIFactory.Panel(Content, Color.white, "Info");
            UIFactory.Anchor(info.rectTransform, 0.8f, 0.4f, 1f, 0.6f);
            movesLabel = UIFactory.Label(info.transform, "", 40, UIFactory.Dark);
            UIFactory.Stretch(movesLabel.rectTransform, 8);
            UpdateMoves();

            var board = UIFactory.CreateRect("Board", Content);
            UIFactory.Anchor(board, 0f, 0f, 0.78f, 1f);
            Canvas.ForceUpdateCanvases();
            Vector2 size = board.rect.size;
            float spacing = 18f;
            float cell = Mathf.Min((size.x - spacing * (cols + 1)) / cols, (size.y - spacing * (rows + 1)) / rows);
            var grid = board.gameObject.AddComponent<GridLayoutGroup>();
            float maxWidth = (size.x - spacing * (cols + 1)) / cols;
            grid.cellSize = new Vector2(Mathf.Min(maxWidth, cell * 1.3f), cell);
            grid.spacing = new Vector2(spacing, spacing);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;

            foreach (var c in deck)
            {
                var card = c;
                card.button = UIFactory.Button(board, "", BackColor, () => OnCardTapped(card), 34);
                card.image = (Image)card.button.targetGraphic;
                card.label = UIFactory.ButtonText(card.button);
                card.label.color = UIFactory.Dark;
                card.rect = (RectTransform)card.button.transform;
                cards.Add(card);
                ShowFace(card, false);
            }

            float preview = CurrentDifficulty == Difficulty.Easy ? 2.5f : CurrentDifficulty == Difficulty.Medium ? 1.2f : 0f;
            if (preview > 0f) StartCoroutine(Preview(preview));
        }

        IEnumerator Preview(float seconds)
        {
            busy = true;
            foreach (var c in cards) ShowFace(c, true);
            yield return new WaitForSeconds(seconds);
            foreach (var c in cards) yield return Flip(c, false, 0.05f);
            busy = false;
        }

        void OnCardTapped(Card c)
        {
            if (busy || !Running || c.open || c.matched) return;
            c.open = true;   // mark immediately so a fast double-tap can't match a card with itself
            StartCoroutine(Flip(c, true, 0.12f));
            if (first == null) { first = c; return; }

            moves++;
            UpdateMoves();
            busy = true;
            StartCoroutine(Resolve(first, c));
            first = null;
        }

        IEnumerator Resolve(Card a, Card b)
        {
            yield return new WaitForSeconds(0.75f);
            bool match = a.pairId == b.pairId;
            RegisterAnswer(match);
            if (match)
            {
                a.matched = b.matched = true;
                foreach (var c in new[] { a, b })
                {
                    c.image.color = Color.Lerp(c.color, UIFactory.Green, 0.35f);
                    var o = c.button.gameObject.AddComponent<Outline>();
                    o.effectColor = UIFactory.Green;
                    o.effectDistance = new Vector2(6, -6);
                }
                pairsFound++;
                if (pairsFound >= pairsTotal)
                {
                    yield return new WaitForSeconds(0.4f);
                    busy = false;
                    Finish();
                    yield break;
                }
            }
            else
            {
                StartCoroutine(Flip(a, false, 0.12f));
                yield return Flip(b, false, 0.12f);
            }
            busy = false;
        }

        IEnumerator Flip(Card c, bool open, float half)
        {
            GameManager.I.Audio.Play(Sfx.Flip);
            for (float t = 0; t < half; t += Time.deltaTime)
            {
                c.rect.localScale = new Vector3(1f - t / half, 1f, 1f);
                yield return null;
            }
            ShowFace(c, open);
            for (float t = 0; t < half; t += Time.deltaTime)
            {
                c.rect.localScale = new Vector3(t / half, 1f, 1f);
                yield return null;
            }
            c.rect.localScale = Vector3.one;
        }

        void ShowFace(Card c, bool open)
        {
            c.open = open;
            c.image.color = open ? Color.Lerp(c.color, Color.white, 0.55f) : BackColor;
            c.label.text = open ? c.face : "?";
            c.label.color = open ? UIFactory.Dark : Color.white;
            c.label.fontSize = open ? 34 : 70;
            c.label.resizeTextMaxSize = c.label.fontSize;
        }

        void UpdateMoves() => movesLabel.text = $"{Loc.Pick("Langkah", "Moves")}\n{moves}";

        /// <summary>Perfect memory needs one move per pair; allow some slack before dropping a star.</summary>
        protected override int CalculateStars()
        {
            if (moves <= Mathf.CeilToInt(pairsTotal * 1.6f)) return 3;
            if (moves <= Mathf.CeilToInt(pairsTotal * 2.4f)) return 2;
            return 1;
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
