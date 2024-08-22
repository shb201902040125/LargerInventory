using Humanizer;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace LargerInventory.BackEnd
{
    internal class LIItems : GlobalItem
    {
        public override bool ConsumeItem(Item item, Player player)
        {
            if (LIConfigs.Instance.ReplenishStockBeforeUse)
            {
                Inventory.PickItem(item, item.maxStack - item.stack);
            }
            return base.ConsumeItem(item, player);
        }
        public override bool CanPickup(Item item, Player player)
        {
            return true;
        }
        public override bool OnPickup(Item item, Player player)
        {
            var status = player.ItemSpace(item);
            if (!status.CanTakeItem)
            {
                Inventory.PushItem(item);
                return false;
            }
            return base.OnPickup(item, player);
        }
    }
}
