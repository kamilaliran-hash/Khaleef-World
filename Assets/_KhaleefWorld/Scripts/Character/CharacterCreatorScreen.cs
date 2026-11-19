using UnityEngine;
using UnityEngine.UI;

namespace KhaleefWorld
{
    /// <summary>The wardrobe / character creation UI. Lives in the CharacterCreator scene.</summary>
    [RequireComponent(typeof(CharacterCustomizer))]
    public class CharacterCreatorScreen : MonoBehaviour
    {
        /// <summary>Scene to return to when the player taps Done.</summary>
        public static string ReturnScene = SceneLoader.WorldMap;

        CharacterCustomizer cust;
        RectTransform tabContent;
        Text coinsLabel;
        string currentTab = "body";
        readonly System.Collections.Generic.Dictionary<string, Image> tabButtons = new System.Collections.Generic.Dictionary<string, Image>();

        void Start()
        {
            cust = GetComponent<CharacterCustomizer>();
            cust.Init();
            BuildUI();
            ShowTab("body");
        }

        void BuildUI()
        {
            var canvas = UIFactory.CreateCanvas("CreatorUI", 10);
            var root = canvas.transform;

            // Drag on the left half to spin the avatar
            var drag = UIFactory.Panel(root, new Color(1, 1, 1, 0.01f), "RotateArea", false);
            UIFactory.Anchor(drag.rectTransform, 0f, 0.14f, 0.45f, 0.86f);
            drag.gameObject.AddComponent<DragArea>().onDrag = d => cust.RotatePreview(-d.x * 0.4f);

            var title = UIFactory.Label(root, Loc.Get("create_avatar"), 60, UIFactory.Dark);
            UIFactory.Anchor(title.rectTransform, 0.02f, 0.88f, 0.44f, 0.98f);

            var coinPill = UIFactory.Panel(root, UIFactory.Yellow, "Coins");
            UIFactory.Anchor(coinPill.rectTransform, 0.02f, 0.02f, 0.2f, 0.11f);
            coinsLabel = UIFactory.Label(coinPill.transform, "", 40, UIFactory.Dark);
            UIFactory.Stretch(coinsLabel.rectTransform, 6);
            RefreshCoins();

            var done = UIFactory.Button(root, Loc.Get("done"), UIFactory.Green, OnDone, 56);
            UIFactory.Anchor((RectTransform)done.transform, 0.23f, 0.02f, 0.44f, 0.12f);

            // Right panel
            var panel = UIFactory.Panel(root, new Color(1f, 1f, 1f, 0.94f), "Wardrobe");
            UIFactory.Anchor(panel.rectTransform, 0.46f, 0.02f, 0.99f, 0.98f);

            var tabs = UIFactory.CreateRect("Tabs", panel.transform);
            UIFactory.Anchor(tabs, 0.01f, 0.89f, 0.99f, 0.99f);
            UIFactory.HLayout(tabs, 6);
            AddTab(tabs, "body", "tab_body");
            for (int i = 0; i < AvatarData.SlotCount; i++)
            {
                var slot = (CosmeticSlot)i;
                AddTab(tabs, slot.ToString(), "tab_" + slot);
            }
            AddTab(tabs, "presets", "tab_presets");

            tabContent = UIFactory.CreateRect("TabContent", panel.transform);
            UIFactory.Anchor(tabContent, 0.01f, 0.01f, 0.99f, 0.88f);
        }

        void AddTab(Transform parent, string id, string locKey)
        {
            var b = UIFactory.Button(parent, Loc.Get(locKey), UIFactory.Teal, () => ShowTab(id), 26);
            tabButtons[id] = (Image)b.targetGraphic;
        }

        void ShowTab(string id)
        {
            currentTab = id;
            foreach (var kv in tabButtons) kv.Value.color = kv.Key == id ? UIFactory.Orange : UIFactory.Teal;
            UIFactory.Clear(tabContent);

            if (id == "body") BuildBodyTab();
            else if (id == "presets") BuildPresetsTab();
            else BuildItemTab((CosmeticSlot)System.Enum.Parse(typeof(CosmeticSlot), id));
        }

        // ---------- Body ----------

        void BuildBodyTab()
        {
            var list = UIFactory.CreateRect("BodyList", tabContent);
            UIFactory.Stretch(list);
            UIFactory.VLayout(list, 14, 20);

            BodyRow(list, "skin", BodyPart.SkinTone);
            BodyRow(list, "eyes", BodyPart.Eyes);
            BodyRow(list, "mouth", BodyPart.Mouth);
            BodyRow(list, "hair_style", BodyPart.HairStyle);
            BodyRow(list, "hair_color", BodyPart.HairColor);
            BodyRow(list, "height", BodyPart.Height);
        }

        void BodyRow(Transform parent, string locKey, BodyPart part)
        {
            var row = UIFactory.CreateRect("Row_" + part, parent);
            UIFactory.Size(row, -1, 110);

            var label = UIFactory.Label(row, Loc.Get(locKey), 38, UIFactory.Dark, TextAnchor.MiddleLeft);
            UIFactory.Anchor(label.rectTransform, 0.02f, 0f, 0.38f, 1f);

            var prev = UIFactory.Button(row, "<", UIFactory.Blue, () => { cust.Step(part, -1); ShowTab("body"); }, 52);
            UIFactory.Anchor((RectTransform)prev.transform, 0.4f, 0.08f, 0.52f, 0.92f);

            var valueBox = UIFactory.Panel(row, new Color(0.93f, 0.93f, 0.96f), "Value");
            UIFactory.Anchor(valueBox.rectTransform, 0.54f, 0.08f, 0.84f, 0.92f);
            int v = cust.GetValue(part);

            if (part == BodyPart.SkinTone || part == BodyPart.HairColor)
            {
                var sw = UIFactory.Panel(valueBox.transform, part == BodyPart.SkinTone ? AvatarBuilder.SkinTones[v] : AvatarBuilder.HairColors[v], "Swatch");
                UIFactory.Stretch(sw.rectTransform, 10);
            }
            else
            {
                string text;
                if (part == BodyPart.HairStyle) text = Loc.Pick(AvatarBuilder.HairStyleNamesMS[v], AvatarBuilder.HairStyleNamesEN[v]);
                else if (part == BodyPart.Height) text = Loc.Get(v == 0 ? "short" : v == 2 ? "tall" : "medium");
                else text = (v + 1) + " / " + CharacterCustomizer.CountOf(part);
                var t = UIFactory.Label(valueBox.transform, text, 36, UIFactory.Dark);
                UIFactory.Stretch(t.rectTransform, 6);
            }

            var next = UIFactory.Button(row, ">", UIFactory.Blue, () => { cust.Step(part, 1); ShowTab("body"); }, 52);
            UIFactory.Anchor((RectTransform)next.transform, 0.86f, 0.08f, 0.98f, 0.92f);
        }

        // ---------- Items ----------

        void BuildItemTab(CosmeticSlot slot)
        {
            UIFactory.ScrollGrid(tabContent, new Vector2(300, 250), 16, out var content);
            UIFactory.Stretch((RectTransform)content.parent.parent);

            var cfg = GameManager.I.Config;
            if (cfg == null) return;

            // "None" card (pets, back items and accessories are optional)
            ItemCard(content, null, slot);
            foreach (var item in cfg.ItemsFor(slot)) ItemCard(content, item, slot);
        }

        void ItemCard(Transform parent, CosmeticItem item, CosmeticSlot slot)
        {
            bool equipped = item == null ? string.IsNullOrEmpty(cust.Working.Get(slot)) : cust.IsEquipped(item);
            bool owned = item == null || cust.Owns(item);

            var card = UIFactory.Button(parent, "", equipped ? UIFactory.Yellow : Color.white, () => OnItemTapped(item, slot), 20);
            var swatch = UIFactory.Panel(card.transform, item != null ? item.primary : new Color(0.85f, 0.85f, 0.85f), "Swatch");
            UIFactory.Anchor(swatch.rectTransform, 0.2f, 0.42f, 0.8f, 0.92f);
            if (item != null && item.secondary != item.primary)
            {
                var stripe = UIFactory.Panel(swatch.transform, item.secondary, "Trim", false);
                UIFactory.Anchor(stripe.rectTransform, 0.42f, 0f, 0.58f, 1f);
            }
            if (!owned)
            {
                var lockTint = UIFactory.Panel(swatch.transform, new Color(0, 0, 0, 0.45f), "Lock");
                UIFactory.Stretch(lockTint.rectTransform);
            }

            var name = UIFactory.Label(card.transform, item != null ? item.DisplayName : Loc.Get("none"), 28, UIFactory.Dark);
            UIFactory.Anchor(name.rectTransform, 0.03f, 0.18f, 0.97f, 0.42f);

            string status;
            if (equipped) status = Loc.Get("wearing");
            else if (owned) status = item != null && item.unlock == UnlockType.Free ? Loc.Get("free") : "";
            else if (item.unlock == UnlockType.Coins) status = item.price + " " + Loc.Get("coins");
            else status = Loc.Get("locked");
            var st = UIFactory.Label(card.transform, status, 24, equipped ? UIFactory.Dark : UIFactory.Teal);
            UIFactory.Anchor(st.rectTransform, 0.03f, 0.02f, 0.97f, 0.18f);
        }

        void OnItemTapped(CosmeticItem item, CosmeticSlot slot)
        {
            if (item == null) { cust.Unequip(slot); ShowTab(currentTab); return; }

            var result = cust.TryEquip(item);
            if (result == EquipResult.Locked)
            {
                if (item.unlock == UnlockType.Coins)
                {
                    if (GameManager.I.Rewards.Coins < item.price) { UIFactory.Toast(Loc.Get("not_enough")); return; }
                    UIFactory.Confirm(Loc.Format("buy_confirm", item.DisplayName, item.price), () =>
                    {
                        if (cust.Buy(item))
                        {
                            GameManager.I.Audio.Play(Sfx.Coin);
                            cust.TryEquip(item);
                            RefreshCoins();
                            ShowTab(currentTab);
                        }
                    });
                }
                else UIFactory.Toast(Loc.Get("badge_needed"));
                return;
            }
            ShowTab(currentTab);
        }

        // ---------- Presets ----------

        void BuildPresetsTab()
        {
            var list = UIFactory.CreateRect("Presets", tabContent);
            UIFactory.Stretch(list);
            UIFactory.VLayout(list, 14, 20);
            var presets = GameManager.I.Save.presets;
            for (int i = 0; i < presets.Length; i++)
            {
                int index = i;
                var row = UIFactory.CreateRect("Preset" + i, list);
                UIFactory.Size(row, -1, 110);
                var label = UIFactory.Label(row, "Set " + (i + 1) + (presets[i].used ? "" : "  (" + Loc.Get("empty") + ")"), 38, UIFactory.Dark, TextAnchor.MiddleLeft);
                UIFactory.Anchor(label.rectTransform, 0.03f, 0f, 0.45f, 1f);

                var use = UIFactory.Button(row, Loc.Get("load"), presets[i].used ? UIFactory.Blue : UIFactory.Grey, () =>
                {
                    if (cust.LoadPreset(index)) ShowTab("presets");
                }, 34);
                UIFactory.Anchor((RectTransform)use.transform, 0.47f, 0.1f, 0.71f, 0.9f);
                use.interactable = presets[i].used;

                var save = UIFactory.Button(row, Loc.Get("save"), UIFactory.Green, () =>
                {
                    cust.SavePreset(index);
                    UIFactory.Toast(Loc.Get("saved"), 1.5f);
                    ShowTab("presets");
                }, 34);
                UIFactory.Anchor((RectTransform)save.transform, 0.74f, 0.1f, 0.98f, 0.9f);
            }
        }

        void RefreshCoins() => coinsLabel.text = GameManager.I.Rewards.Coins + " " + Loc.Get("coins");

        void OnDone()
        {
            cust.Confirm();
            string target = SceneLoader.Exists(ReturnScene) ? ReturnScene : SceneLoader.WorldMap;
            ReturnScene = SceneLoader.WorldMap;
            SceneLoader.Load(target);
        }
    }
}
