using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
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
                if (Inventory.Count == 0)
                {
                    for (int i = 1; i < ItemID.Count; i++)
                    {
                        Item item = new(i);
                        item.stack = Main.rand.Next(1, item.maxStack);
                        Inventory.PushItem(item, out _);
                    }
                }
                InvUI.Ins.CallRefresh();
                if (!LISystem.filterUIF.IsVisible)
                    LISystem.invUIF.IsVisible = !LISystem.invUIF.IsVisible;
                /*InvUI inv = InvUI.Ins;
                inv.RemoveAllChildren();
                inv.OnInitialize();
                inv.Recalculate();*/
            }
        }
        public override void PostUpdate()
        {
            Inventory.TryHealLife(Player);
            Inventory.TryHealMana(Player);
        }
    }
}
