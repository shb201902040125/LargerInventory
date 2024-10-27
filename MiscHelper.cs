using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace LargerInventory
{
    public static class MiscHelper
    {
        public const string LocalKey = "Mods.LargerInventory.";
        public static void ApplyLifeAndOrMana(this Player player, Item item)
        {
            int num = player.GetHealLife(item, true);
            int healMana = player.GetHealMana(item, true);
            if (item.type == ItemID.StrangeBrew)
            {
                int healLife = item.healLife;
                int num2 = 120;
                num = Main.rand.Next(healLife, num2 + 1);
                if (Main.myPlayer == player.whoAmI)
                {
                    float num3 = Main.rand.NextFloat();
                    int num4 = 0;
                    if (num3 <= 0.1f)
                    {
                        num4 = 240;
                    }
                    else if (num3 <= 0.3f)
                    {
                        num4 = 120;
                    }
                    else if (num3 <= 0.6f)
                    {
                        num4 = 60;
                    }

                    if (num4 > 0)
                    {
                        player.SetImmuneTimeForAllTypes(num4);
                    }
                }
            }

            player.statLife += num;
            player.statMana += healMana;
            if (player.statLife > player.statLifeMax2)
            {
                player.statLife = player.statLifeMax2;
            }

            if (player.statMana > player.statManaMax2)
            {
                player.statMana = player.statManaMax2;
            }

            if (num > 0 && Main.myPlayer == player.whoAmI)
            {
                player.HealEffect(num);
            }

            if (healMana > 0)
            {
                player.AddBuff(94, Player.manaSickTime);
                if (Main.myPlayer == player.whoAmI)
                {
                    player.ManaEffect(healMana);
                }
            }
        }
        public static string GTV(string key, params object[] args) => Language.GetTextValue(LocalKey + key, args);
    }
}
