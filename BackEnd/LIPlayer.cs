using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
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
                var inv = InvUI.Ins;
                inv.Clear();
                inv.Initialize();
            }
        }
        public override void PostUpdate()
        {
            Inventory.TryHealLife(Player);
            Inventory.TryHealMana(Player);
        }
        public override bool OnPickup(Item item)
        {
            var status = Player.ItemSpace(item);
            //if (!status.CanTakeItem)
            {
                Inventory.PushItem(item, out bool refresh);
                if (refresh)
                {
                    InvUI.Ins.Refresh();
                }
                return false;
            }
            return base.OnPickup(item);
        }
    }
}
