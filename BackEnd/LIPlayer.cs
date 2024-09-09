using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework.Input;
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
                InvUI inv = InvUI.Ins;
                inv.RemoveAll();
                inv.OnInitialization();
            }
        }
        public override void PostUpdate()
        {
            Inventory.TryHealLife(Player);
            Inventory.TryHealMana(Player);
        }
    }
}
