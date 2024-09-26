using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.ExtraUI
{
    public class UIInvSlot : UIElement
    {
        public Item item;
        public int Index { get; private set; }
        public bool InLock { get; private set; }
        uint lastLeftTime, lastRightTime;
        int leftKeepTime, rightKeepTime;
        public UIInvSlot(int type, int index)
        {
            item = ContentSamples.ItemsByType[type];
            Index = index;
            this.SetSize(52, 52);
        }
        bool CheckThisSlotAccessibleInInv(out Item itemInInv)
        {
            if (Inv._items.TryGetValue(item.type, out var container) && container.IndexInRange(Index))
            {
                itemInInv = container[Index];
                return true;
            }
            itemInInv = null;
            return false;
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (Main.GameUpdateCount - lastLeftTime > 1)
            {
                leftKeepTime = 0;
            }
            lastLeftTime = Main.GameUpdateCount;
            leftKeepTime++;
            if (leftKeepTime < 15)
            {
                return;
            }
            if (InLock)
            {
                return;
            }
            //左键无持续操作
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (leftKeepTime > 15)
            {
                return;
            }
            if (InLock)
            {
                return;
            }
            Item temp;
            bool refresh;
            //空槽位，放入物品
            if (Index == -1)
            {
                temp = new();
                (temp, Main.mouseItem) = (Main.mouseItem, item);
                Inv.PushItem(temp, out refresh);
                item = ContentSamples.ItemsByType[temp.type];
                if (refresh)
                {
                    InvUI.Ins.needRefresh = true;
                    InLock = true;
                }
                return;
            }
            //放入全部
            if (Main.mouseItem.type == item.type)
            {
                Inv.PutItemToDesignatedIndex(Main.mouseItem, Index);
                return;
            }
            //交换
            temp = new(item.type);
            Inv.PickItemFromDesignatedIndex(temp, Index, temp.maxStack);
            (temp, Main.mouseItem) = (Main.mouseItem, temp);
            Inv.PushItem(temp, out refresh);
            item = ContentSamples.ItemsByType[temp.type];
            if (refresh)
            {
                InvUI.Ins.needRefresh = true;
                InLock = true;
            }
        }
        public override void RightMouseDown(UIMouseEvent evt)
        {
            if (Main.GameUpdateCount - lastRightTime > 1)
            {
                lastRightTime = 0;
            }
            lastRightTime = Main.GameUpdateCount;
            rightKeepTime++;
            if (rightKeepTime < 15)
            {
                return;
            }
            if (InLock)
            {
                return;
            }
            //空槽位无右键持续操作
            if (Index == -1)
            {
                return;
            }
            //鼠标物品非空且类型不同无右键持续操作
            if (!Main.mouseItem.IsAir && Main.mouseItem.type != item.type)
            {
                return;
            }
            //加速取出
            int time = rightKeepTime - 15;
            if (Main.mouseItem.IsAir)
            {
                Main.mouseItem.SetDefaults(item.type);
            }
            Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, Math.Min(time / 15, 36));
            //取完了将该位置置空
            if (CheckThisSlotAccessibleInInv(out var itemInInv) && itemInInv.IsAir)
            {
                Index = -1;
            }
        }
        public override void RightMouseUp(UIMouseEvent evt)
        {
            if (rightKeepTime > 15)
            {
                return;
            }
            if (InLock)
            {
                return;
            }
            //空槽位无右键单机操作
            if (Index == -1)
            {
                return;
            }
            if (!Main.mouseItem.IsAir && Main.mouseItem.type != item.type)
            {
                return;
            }
            //右键取出一半
            if (Main.mouseItem.IsAir)
            {
                Main.mouseItem.SetDefaults(item.type);
            }
            if (CheckThisSlotAccessibleInInv(out var itemInInv))
            {
                Inv.PickItemFromDesignatedIndex(Main.mouseItem, Index, itemInInv.stack / 2);
            }
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            CheckThisSlotAccessibleInInv(out var drawItem);
            drawItem ??= ContentSamples.ItemsByType[item.type];
            ItemSlot.Draw(sb, ref drawItem, 0, GetDimensions().ToRectangle().TopLeft());
        }
    }
}
