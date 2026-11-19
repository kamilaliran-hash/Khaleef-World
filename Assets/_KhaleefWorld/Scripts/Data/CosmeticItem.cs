using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhaleefWorld
{
    /// <summary>One wearable item. Built procedurally from blocks, or from prefabOverride once real art exists.</summary>
    [CreateAssetMenu(menuName = "Khaleef World/Cosmetic Item", fileName = "Item_")]
    public class CosmeticItem : ScriptableObject
    {
        public string id;
        public string nameMS;
        public string nameEN;
        public CosmeticSlot slot;
        public ItemShape shape;

        [Header("Colours")]
        public Color primary = Color.white;
        public Color secondary = Color.white;
        [Tooltip("Leg colour used when this Top is a full outfit.")]
        public Color bottomColor = new Color(0.15f, 0.15f, 0.2f);

        [Header("Full outfits (Top slot)")]
        public bool fullOutfit;
        public bool longSkirt;

        [Header("Unlock")]
        public UnlockType unlock = UnlockType.Free;
        public int price;
        public string requiredBadge;

        [Header("Culture")]
        public bool traditional;
        public string cultureTag;

        [Tooltip("Optional: real 3D art. When set, it is attached instead of the procedural blocks.")]
        public GameObject prefabOverride;

        public string DisplayName => Loc.Pick(nameMS, nameEN);
    }
}
