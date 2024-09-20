using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LargerInventory.BackEnd
{
    internal class LIPlayer : ModPlayer
    {
        internal static ModKeybind SwitchInv;
        public override void Load()
        {
            SwitchInv = KeybindLoader.RegisterKeybind(Mod, "SwitchInv", Keys.C);
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (SwitchInv.JustPressed)
            {
                if (Inventory._items.Count == 0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Inventory.PushItem(new Item(Main.rand.Next(ItemID.Count), 1), out _);
                    }
                }
                InvUI.Ins.Refresh();
                LISystem.uif.IsVisible =! LISystem.uif.IsVisible;
                /*InvUI inv = InvUI.Ins;
                inv.RemoveAllChildren();
                inv.OnInitialize();
                inv.Recalculate();*/
            }
        }
        public override void PostUpdate()
        {
            Main.chatMonitor.Clear();
            foreach(var pair in Inventory._items)
            {
                Main.NewText(Lang.GetItemName(pair.Key) + ":[" + string.Join(", ", pair.Value.ConvertAll(item => item.stack)) + "]");
            }
            Inventory.TryHealLife(Player);
            Inventory.TryHealMana(Player);
        }
    }
}
