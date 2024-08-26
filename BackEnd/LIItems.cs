using Terraria;
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
    }
}
