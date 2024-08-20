using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace LargerInventory.BackEnd
{
    internal class LIPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            Inventory.TryHealLife(Player);
            Inventory.TryHealMana(Player);
        }
        public override bool OnPickup(Item item)
        {
            var status = Player.ItemSpace(item);
            if(!status.CanTakeItem)
            {
                Inventory.PushItem(item);
                return false;
            }
            return base.OnPickup(item);
        }
    }
}
