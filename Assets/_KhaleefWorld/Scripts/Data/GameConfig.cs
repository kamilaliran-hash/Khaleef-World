using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>Global config, loaded from Resources/KhaleefConfig.</summary>
    [CreateAssetMenu(menuName = "Khaleef World/Game Config", fileName = "KhaleefConfig")]
    public class GameConfig : ScriptableObject
    {
        public Material blockyMaterial;
        public List<StateDefinition> states = new List<StateDefinition>();
        public List<CosmeticItem> cosmetics = new List<CosmeticItem>();

        static GameConfig _instance;

        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameConfig>("KhaleefConfig");
                    if (_instance == null)
                        Debug.LogError("[Khaleef World] KhaleefConfig not found. In Unity run menu: Khaleef World > 1. Create MVP Content.");
                }
                return _instance;
            }
        }

        public CosmeticItem FindItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return cosmetics.Find(c => c != null && c.id == id);
        }

        public StateDefinition FindState(string id) => states.Find(s => s != null && s.id == id);

        public MiniGameDefinition FindMiniGame(string id)
        {
            foreach (var s in states)
            {
                if (s == null) continue;
                foreach (var m in s.miniGames) if (m != null && m.id == id) return m;
            }
            return null;
        }

        public List<CosmeticItem> ItemsFor(CosmeticSlot slot) => cosmetics.FindAll(c => c != null && c.slot == slot);
    }
}
