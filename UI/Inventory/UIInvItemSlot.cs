using ForOneToolkit.UI.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace LargerInventory.UI.Inventory
{
    internal class UIInvItemSlot : UIItemSlot
    {
        internal int Type;
        internal int Index;
        public UIInvItemSlot(Item item,int type,int index, int slotID = 0, float scale = 1f):base(item,slotID,scale)
        {
            Type = type;
            Index = index;
        }
    }
}
