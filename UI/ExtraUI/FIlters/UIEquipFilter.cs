using LargerInventory.BackEnd;
using Terraria;

namespace LargerInventory.UI.ExtraUI.FIlters
{
    public class UIEquipFilter(InvItemFilter filter) : UIItemFilter(filter)
    {
        public bool CheckVanity;
        protected override bool Match(Item item) => CheckVanity == item.vanity && base.Match(item);
    }
}
