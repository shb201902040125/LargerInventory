﻿using LargerInventory.UI.Inventory;
using Terraria;
using Terraria.ModLoader;

namespace LargerInventory.BackEnd
{
    internal class LIItems : GlobalItem
    {
        public override bool InstancePerEntity => true;
        private bool ignoreSelfInfluence;
        public override bool ConsumeItem(Item item, Player player)
        {
            if (!ignoreSelfInfluence && LIConfigs.Instance.ReplenishStockBeforeUse)
            {
                Inventory.PickItem(item, item.maxStack - item.stack);
            }
            return base.ConsumeItem(item, player);
        }
        public override bool CanPickup(Item item, Player player)
        {
            return !ignoreSelfInfluence || base.CanPickup(item, player);
        }
        public override bool OnPickup(Item item, Player player)
        {
            //Inventory.PushItem(item, out bool refresh);
            //if (refresh)
            //{
            //    InvUI.Ins.Refresh();
            //}
            //return false;
            if (!ignoreSelfInfluence)
            {
                ignoreSelfInfluence = true;
                Player.ItemSpaceStatus status = player.ItemSpace(item);
                ignoreSelfInfluence = false;
                if (!status.CanTakeItem)
                {
                    Inventory.PushItem(item, out bool refresh);
                    if (refresh)
                    {
                        InvUI.Ins.Refresh();
                    }
                    return false;
                }
            }
            return base.OnPickup(item, player);
        }
        public override bool ItemSpace(Item item, Player player)
        {
            return !ignoreSelfInfluence || base.ItemSpace(item, player);
        }
    }
}
