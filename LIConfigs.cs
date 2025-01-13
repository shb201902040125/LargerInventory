using System.ComponentModel;
using Terraria.GameContent.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace LargerInventory
{
    internal class LIConfigs : ModConfig
    {
        public static LIConfigs Instance => ModContent.GetInstance<LIConfigs>();
        public override ConfigScope Mode => ConfigScope.ClientSide;

        /// <summary>
        /// 在使用前补充库存，并不能避免一次性耗尽
        /// </summary>
        [DefaultValue(false)]
        public bool ReplenishStockBeforeUse;

        /// <summary>
        /// 自动使用生命药水的百分比，血量百分比低于该数值时自动使用生命药水，优先使用不大于损失血量的药水，其次选择溢出最少的药水
        /// </summary>
        [DefaultValue(0)]
        public float AutoUseLifePotion;

        /// <summary>
        /// 自动使用魔力药水的百分比，魔力百分比低于该数值时自动使用魔力药水，优先使用不大于损失魔力的药水，其次选择溢出最少的药水
        /// </summary>
        [DefaultValue(0)]
        public float AutoUseManaPotion;

        /// <summary>
        /// 使用LI的货币储存系统
        /// <br>优先使用LI的货币储存系统，不足时可以额外使用原版背包内的货币</br>
        /// </summary>
        [DefaultValue(false)]
        public bool PayFromLargerInventory;

        [DefaultValue(15)]
        public int UpdateRecipeTaskInterval;
    }
}
