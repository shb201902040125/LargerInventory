using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.ExtraUI
{
    public class UIInvSlot : UIElement
    {
        Inv.InfoForUI Info;
        bool rightDown;
        int time;
        public UIInvSlot(Inv.InfoForUI info)
        {
            Info = info;
            this.SetSize(52, 52);
            OnLeftMouseDown += UIInvSlot_OnLeftMouseDown;
            OnRightMouseDown += UIInvSlot_OnRightMouseDown;
        }

        private void UIInvSlot_OnRightMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            Item mouse = Main.mouseItem;
            Item source = Info.Item;
            if (mouse.type == ItemID.None)
            {
                if (source.type == 0)
                    return;
                Main.mouseItem = source.Clone();
                source.stack--;
                if (source.stack == 0)
                {
                    Info.Changed(new());
                    return;
                }
                rightDown = true;
            }
            else if (mouse.type == source.type && ItemLoader.CanStack(mouse, source))
            {
                mouse.stack++;
                source.stack--;
                if (source.stack == 0)
                {
                    Info.Changed(new());
                    return;
                }
                rightDown = true;
            }
        }

        private void UIInvSlot_OnLeftMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            Item item = Info.Item;
            (Main.mouseItem, item) = (item, Main.mouseItem);
            Info.Changed(item);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Main.HoverItem = Info.Item;
                Main.hoverItemName = Info.Item.Name;
            }
            if (rightDown)
            {
                if (!Main.mouseRight)
                {
                    time = 0;
                    return;
                }
                if (time >= 30)
                {
                    int mult = (time - 20) / 5;
                    int space = (int)Math.Sqrt(20 - Math.Min(mult, 19));
                    Item source = Info.Item;
                    int count = Math.Clamp(mult - 20, 1, source.stack);
                    if (time % space == 0)
                    {
                        Main.mouseItem.stack += count;
                        source.stack -= count;
                        if (source.stack <= 0)
                        {
                            Info.Changed(new());
                            time = 0;
                            return;
                        }
                        Info.Changed(source);
                    }
                }
                time++;
            }
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            var item = Info.Item;
            ItemSlot.Draw(sb, ref item, 0, GetDimensions().ToRectangle().TopLeft());
        }
    }
}
