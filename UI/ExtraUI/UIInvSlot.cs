using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.ExtraUI
{
    public class UIInvSlot : UIElement
    {
        public Item item ;
        public readonly int Index ;
        private bool rightDown;
        private int time;
        public UIInvSlot(Item item ,int index)
        {
            this.item = item;
            Index = index;
            this.SetSize(52, 52);
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (item.IsAir)
            {
                if (Main.mouseItem.IsAir)
                {
                    return;
                }
                //栏位是空执行放入，背包同时执行放入
                Inv.PutItemToDesignatedIndex(Main.mouseItem, Index);
                if (Main.mouseItem.IsAir)
                {
                    Main.mouseItem.SetDefaults(ItemID.None);
                }
            }
            else//栏位不空
            {
                //栏位与鼠标同类型
                if (item.type == Main.mouseItem.type)
                {
                    if (!Inv.PutItemToDesignatedIndex(Main.mouseItem, Index))
                    {
                        Inv.ExchangeItems(ref Main.mouseItem, Index);
                    }
                }
                else
                {
                    //不同则交换，背包执行删除和放入
                    if (Inv.PopItems(item.type, Index, out Item i))
                    {
                        (i, Main.mouseItem) = (Main.mouseItem, i);
                        Inv.PushItemToEnd(i);
                        //TODO
                        //需要刷新UI列表
                        //s.Type = item.type;
                        //s.Index = Items[s.Type].Count - 1;
                    }
                }
            }
        }
        public override void RightMouseDown(UIMouseEvent evt)
        {
            if (item.IsAir)
            {
                return;
            }
            //鼠标没东西时
            if (Main.mouseItem.IsAir)
            {
                Main.mouseItem.type = item.type;
                Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, 1);
            }
            else if (Main.mouseItem.type == item.type)
            {
                Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, 1);
            }
            rightDown = true;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (rightDown)
            {
                if (!Main.mouseRight|| item.IsAir || item.type != Main.mouseItem.type)
                {
                    time = 0;
                    rightDown = false;
                    return;
                }
                if (time >= 30)
                {
                    int mult = (time - 20) / 5;
                    int space = (int)Math.Sqrt(20 - Math.Min(mult, 19));
                    int count = Math.Max(1, mult - 20);
                    if (time % space == 0)
                    {
                        Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, count);
                    }
                }
                time++;
            }
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            ItemSlot.Draw(sb, ref item, 0, GetDimensions().ToRectangle().TopLeft());
        }
    }
}
