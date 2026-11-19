using System;
using UnityEngine;

namespace KhaleefWorld
{
    public enum BodyPart { SkinTone, Eyes, Mouth, HairStyle, HairColor, Height }
    public enum EquipResult { Equipped, Unequipped, Locked }

    /// <summary>
    /// Customisation logic (no UI): edits a working copy of the avatar, handles mix-and-match slot rules,
    /// coin purchases and the 5 outfit presets. CharacterCreatorScreen is the UI on top of this.
    /// </summary>
    public class CharacterCustomizer : MonoBehaviour
    {
        public Transform previewAnchor;
        public AvatarData Working { get; private set; }
        public event Action Changed;

        AvatarRig rig;
        Transform petPreview;
        GameManager GM => GameManager.I;

        public void Init()
        {
            Working = GM.Save.avatar.Clone();
            Rebuild();
        }

        void Update()
        {
            if (rig != null) rig.Animate(0f, Time.deltaTime);
        }

        public void Rebuild()
        {
            if (previewAnchor == null) previewAnchor = transform;
            rig = AvatarBuilder.Build(Working, previewAnchor);

            if (petPreview != null) Destroy(petPreview.gameObject);
            var cfg = GM.Config;
            var pet = cfg != null ? cfg.FindItem(Working.Get(CosmeticSlot.Pet)) : null;
            if (pet != null)
            {
                petPreview = AvatarBuilder.BuildPet(pet.shape, pet.primary, pet.secondary, previewAnchor);
                petPreview.localPosition = new Vector3(0.75f, 0, 0.2f);
                petPreview.localRotation = Quaternion.Euler(0, -20, 0);
            }
            Changed?.Invoke();
        }

        public int GetValue(BodyPart part)
        {
            switch (part)
            {
                case BodyPart.SkinTone: return Working.skinTone;
                case BodyPart.Eyes: return Working.eyes;
                case BodyPart.Mouth: return Working.mouth;
                case BodyPart.HairStyle: return Working.hairStyle;
                case BodyPart.HairColor: return Working.hairColor;
                default: return Working.heightStep;
            }
        }

        public static int CountOf(BodyPart part)
        {
            switch (part)
            {
                case BodyPart.SkinTone: return AvatarBuilder.SkinTones.Length;
                case BodyPart.Eyes: return AvatarBuilder.EyeStyles;
                case BodyPart.Mouth: return AvatarBuilder.MouthStyles;
                case BodyPart.HairStyle: return AvatarBuilder.HairStyles;
                case BodyPart.HairColor: return AvatarBuilder.HairColors.Length;
                default: return 3;
            }
        }

        public void Step(BodyPart part, int delta)
        {
            int n = CountOf(part);
            int v = ((GetValue(part) + delta) % n + n) % n;
            switch (part)
            {
                case BodyPart.SkinTone: Working.skinTone = v; break;
                case BodyPart.Eyes: Working.eyes = v; break;
                case BodyPart.Mouth: Working.mouth = v; break;
                case BodyPart.HairStyle: Working.hairStyle = v; break;
                case BodyPart.HairColor: Working.hairColor = v; break;
                case BodyPart.Height: Working.heightStep = Mathf.Clamp(GetValue(part) + delta, 0, 2); break;
            }
            Rebuild();
        }

        public bool IsEquipped(CosmeticItem item) => item != null && Working.Get(item.slot) == item.id;

        public bool Owns(CosmeticItem item) => GM.Rewards.OwnsItem(item);

        public EquipResult TryEquip(CosmeticItem item)
        {
            if (IsEquipped(item)) { Unequip(item.slot); return EquipResult.Unequipped; }
            if (!Owns(item)) return EquipResult.Locked;

            Working.Set(item.slot, item.id);
            var cfg = GM.Config;
            // Full outfits (Saree, Cheongsam, Iban attire...) occupy Top + Bottom together.
            if (item.slot == CosmeticSlot.Top && item.fullOutfit) Working.Set(CosmeticSlot.Bottom, null);
            if (item.slot == CosmeticSlot.Bottom && cfg != null)
            {
                var top = cfg.FindItem(Working.Get(CosmeticSlot.Top));
                if (top != null && top.fullOutfit) Working.Set(CosmeticSlot.Top, null);
            }
            Rebuild();
            return EquipResult.Equipped;
        }

        public void Unequip(CosmeticSlot slot)
        {
            Working.Set(slot, null);
            Rebuild();
        }

        public bool Buy(CosmeticItem item) => GM.Rewards.BuyItem(item);

        public void SavePreset(int index)
        {
            var p = GM.Save.presets[index];
            p.used = true;
            p.name = (index + 1).ToString();
            p.avatar = Working.Clone();
            GM.SaveNow();
        }

        public bool LoadPreset(int index)
        {
            var p = GM.Save.presets[index];
            if (!p.used || p.avatar == null) return false;
            Working = p.avatar.Clone();
            Rebuild();
            return true;
        }

        public bool WearingTraditional()
        {
            var cfg = GM.Config;
            if (cfg == null) return false;
            for (int i = 0; i < AvatarData.SlotCount; i++)
            {
                var it = cfg.FindItem(Working.Get((CosmeticSlot)i));
                if (it != null && it.traditional) return true;
            }
            return false;
        }

        /// <summary>Commits the working copy to the save file.</summary>
        public void Confirm()
        {
            GM.Save.avatar = Working.Clone();
            GM.Save.avatarCreated = true;
            if (WearingTraditional()) GM.Quests.Notify(QuestType.WearTraditional, 1);
            GM.SaveNow();
        }

        public void RotatePreview(float degrees)
        {
            if (previewAnchor != null) previewAnchor.Rotate(0f, degrees, 0f, Space.World);
        }
    }
}
