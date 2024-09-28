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
        public Item item;
        public bool InLock { get; private set; }
        uint lastLeftTime, lastRightTime;
        int leftKeepTime, rightKeepTime;
        public UIInvSlot(Item item)
        {
            this.SetSize(52, 52);
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            ItemSlot.Draw(sb, ref item, 0, GetDimensions().ToRectangle().TopLeft());
        }
    }
}
