using LargerInventory.UI.Inventory;
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
        public Item item ;
        public int Index { get; private set; }
        private bool rightDown;
        private int time;
        uint lastLeftTime, lastRightTime, leftKeepTime, rightKeepTime;
        public UIInvSlot(int type ,int index)
        {
            item = ContentSamples.ItemsByType[type];
            Index = index;
            this.SetSize(52, 52);
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            if (Main.GameUpdateCount - lastLeftTime > 1)
            {
                leftKeepTime = 0;
            }
            if (leftKeepTime > 0)
            {
                return;
            }
            lastLeftTime = Main.GameUpdateCount;
            //当前空位，且鼠标物品非空，将物品放入
            if (Index == -1 && !Main.mouseItem.IsAir)
            {
                Item item = new(ItemID.None, 0);
                (item, Main.mouseItem) = (Main.mouseItem, item);
                Inv.PushItem(item, out bool refresh);
                this.item = ContentSamples.ItemsByType[item.type];
                if (refresh)
                {
                    InvUI.Ins.needRefresh = true;
                }
            }
            //当前非空槽，且鼠标物品非空，交换物品
            else if (Index != -1 && Main.mouseItem.type != item.type)
            {
                //取出物品
                Item item = new(this.item.type);
                Inv.PickItemFromDesignatedIndex(item, Index, item.maxStack);
                //与鼠标交换
                (item, Main.mouseItem) = (Main.mouseItem, item);
                //把物品推入
                Inv.PushItem(item, out bool refresh);
                this.item = ContentSamples.ItemsByType[item.type];
                if (refresh)
                {
                    InvUI.Ins.needRefresh = true;
                }
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            //空位长按无操作
            if (Index == -1)
            {
                return;
            }
            if (Main.GameUpdateCount - lastLeftTime > 1)
            {
                leftKeepTime = 0;
            }
            lastLeftTime = Main.GameUpdateCount;
            leftKeepTime++;
            if (leftKeepTime % 5 != 0)
            {
                return;
            }
            //加速提取
            if (Main.mouseItem.IsAir)
            {
                Main.mouseItem.SetDefaults(item.type);
            }
            //取不出来就是没东西了
            if (Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, (int)Math.Min(leftKeepTime % 5, 36)) == 0)
            {
                //槽位置空
                item = ContentSamples.ItemsByType[ItemID.None];
                Index = -1;
                InvUI.Ins.needRefresh = true;
            }
        }
        //public override void RightMouseDown(UIMouseEvent evt)
        //{
        //    if (item.IsAir)
        //    {
        //        return;
        //    }
        //    //鼠标没东西时
        //    if (Main.mouseItem.IsAir)
        //    {
        //        Main.mouseItem.type = item.type;
        //        Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, 1);
        //    }
        //    else if (Main.mouseItem.type == item.type)
        //    {
        //        Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, 1);
        //    }
        //    rightDown = true;
        //}
        //public override void Update(GameTime gameTime)
        //{
        //    base.Update(gameTime);
        //    if (rightDown)
        //    {
        //        if (!Main.mouseRight|| item.IsAir || item.type != Main.mouseItem.type)
        //        {
        //            time = 0;
        //            rightDown = false;
        //            return;
        //        }
        //        if (time >= 30)
        //        {
        //            int mult = (time - 20) / 5;
        //            int space = (int)Math.Sqrt(20 - Math.Min(mult, 19));
        //            int count = Math.Max(1, mult - 20);
        //            if (time % space == 0)
        //            {
        //                Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, count);
        //            }
        //        }
        //        time++;
        //    }
        //}
        protected override void DrawSelf(SpriteBatch sb)
        {
            var drawItem = Index == -1 ? ContentSamples.ItemsByType[item.type] : (Inv._items[item.type].IndexInRange(Index) ? Inv._items[item.type][Index] : ContentSamples.ItemsByType[item.type]);
            ItemSlot.Draw(sb, ref drawItem, 0, GetDimensions().ToRectangle().TopLeft());
        }
    }
}
