using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SML.Physics;
using System;
using System.ComponentModel;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.ExtraUI
{
    public class UIInvSlot : UIElement
    {
        Inv.InfoForUI Info;
        public UIInvSlot(Inv.InfoForUI info)
        {
            Info = info;
            this.SetSize(52, 52);
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                var item = Info.Item;
                ItemSlot.Handle(ref item, ItemSlot.Context.InventoryItem);
                Info.Changed(item);
            }
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            var item = Info.Item;
            ItemSlot.Draw(sb, ref item, 0, GetDimensions().ToRectangle().TopLeft());
        }
    }
}
