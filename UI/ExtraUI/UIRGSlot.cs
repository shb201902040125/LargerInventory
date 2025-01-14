using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace LargerInventory.UI.ExtraUI
{
    public class UIRGSlot : UIElement
    {
        public readonly Item item;
        public bool locked;
        public UIRGSlot(Item item, bool locked)
        {
            this.item = item;
            this.locked = locked;
            this.SetSize(52, 52);
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            var rect = GetDimensions().ToRectangle();
            if (IsMouseHovering)
            {
                Main.hoverItemName = item.Name;
                Main.HoverItem = item;
            }
            sb.Draw((locked ? TextureAssets.InventoryBack : TextureAssets.InventoryBack2).Value, rect.TopLeft(), Color.White);
            ItemSlot.DrawItemIcon(item, 0, sb, rect.Center.ToVector2(), Main.inventoryScale, 52 * Main.inventoryScale, Color.White);
        }
    }
}
