using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent.UI.Elements;
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
                LISystem.uif.IsVisible =! LISystem.uif.IsVisible;
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
