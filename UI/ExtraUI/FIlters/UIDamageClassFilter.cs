using LargerInventory.BackEnd;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static LargerInventory.BackEnd.InvItemFilter;

namespace LargerInventory.UI.ExtraUI.FIlters
{
    public class UIDamageClassFilter(DamageClass dc) : UIItemFilter(new(i => i.DamageType == dc), FilterPrefab.IsWeapon)
    {
        public int FilterType;
        public DamageClass DamageClass = dc;
        private readonly InvItemFilter countAs = new(i => i.DamageType.CountsAsClass(dc));
        private readonly InvItemFilter effAllow = new(i => i.DamageType.GetEffectInheritance(dc));
        protected override bool Match(Item item)
        {
            return FilterType switch
            {
                1 => countAs.Check(item),
                2 => effAllow.Check(item),
                _ => base.Match(item),
            };
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);
            if (IsMouseHovering)
            {
                Main.hoverItemName += DamageClass.DisplayName + "\n" + DamageClass.PrettyPrintName();
            }
        }
    }
}
