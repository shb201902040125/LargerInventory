using RUIModule.RUIElements;
using Terraria;

namespace LargerInventory.UI.Inventory
{
    internal class UIInvItemSlot : UIItemSlot
    {
        internal int Type;
        internal int Index;
        public UIInvItemSlot(Item item, int type, int index, int slotID = 0, float scale = 1f) : base(item)
        {
            Slot.slotID = slotID;
            Type = type;
            Index = index;
        }
    }
}
