using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>A Malaysian state or Federal Territory on the World Map.</summary>
    [CreateAssetMenu(menuName = "Khaleef World/State", fileName = "State_")]
    public class StateDefinition : ScriptableObject
    {
        public string id;
        public string nameMS;
        public string nameEN;
        public string shortCode;
        public string sceneName;
        public string landmarkMS;
        public string landmarkEN;
        public Vector2 mapPosition;              // 0..1 across the World Map screen
        public Color color = Color.white;
        public TravelMode travelMode;
        public int requiredStamps;
        public bool playable;
        public bool borneo;
        public string badgeId;
        public AudioClip music;
        public List<MiniGameDefinition> miniGames = new List<MiniGameDefinition>();
        [TextArea] public string greetingMS;
        [TextArea] public string greetingEN;

        public string DisplayName => Loc.Pick(nameMS, nameEN);
        public string Landmark => Loc.Pick(landmarkMS, landmarkEN);
    }
}
