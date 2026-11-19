using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhaleefWorld
{
    [Serializable]
    public class CardPair
    {
        public string a;
        [Tooltip("Leave empty for identical pairs (classic memory).")]
        public string b;
        public Color color = Color.white;
    }

    /// <summary>Data for a single mini-game: which template it uses and its content.</summary>
    [CreateAssetMenu(menuName = "Khaleef World/Mini-game", fileName = "MG_")]
    public class MiniGameDefinition : ScriptableObject
    {
        public string id;
        public string titleMS;
        public string titleEN;
        [TextArea] public string instructionMS;
        [TextArea] public string instructionEN;
        [TextArea] public string funFactMS;
        [TextArea] public string funFactEN;
        public Subject subject;
        public MiniGameTemplate template;
        public int baseCoins = 20;

        [Header("Maths Climb")]
        public MathMode mathMode;
        public int climbTarget = 272;
        public string climbUnitMS = "anak tangga";
        public string climbUnitEN = "steps";

        [Header("Memory Match")]
        public List<CardPair> cards = new List<CardPair>();

        public string Title => Loc.Pick(titleMS, titleEN);
    }
}
