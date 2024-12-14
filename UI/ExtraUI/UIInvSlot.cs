using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.ExtraUI
{
    public class UIInvSlot : UIElement
    {
        internal Inv.InfoForUI Info;
        private bool rightDown;
        private int time;
        public UIInvSlot(Inv.InfoForUI info)
        {
            Info = info;
            this.SetSize(52, 52);
            OnLeftMouseDown += UIInvSlot_OnLeftMouseDown;
            OnRightMouseDown += UIInvSlot_OnRightMouseDown;
        }
        private void CheckItem(ref Item item)
        {
            if (item.stack <= 0)
            {
                Item temp = new();
                Info.Changed(InvUI.Ins.Token, ref temp);
            }
            else
            {
                Info.Changed(InvUI.Ins.Token, ref item, false);
            }
        }
        private void UIInvSlot_OnLeftMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            Item item = Info.Item;
            if (Main.mouseItem.IsAir && item.IsAir)
            {
                return;
            }
            Main.playerInventory = true;
            if (Main.mouseItem.type != item.type)
            {
                (Main.mouseItem, item) = (item, Main.mouseItem);
                CheckItem(ref item);
            }
            else
            {
                if (ItemLoader.CanStack(item, Main.mouseItem))
                {
                    ItemLoader.StackItems(item, Main.mouseItem, out _);
                    CheckItem(ref item);
                }
                else
                {
                    (Main.mouseItem, item) = (item, Main.mouseItem);
                    CheckItem(ref item);
                }
            }
        }

        private void UIInvSlot_OnRightMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            Item mouse = Main.mouseItem;
            Item source = Info.Item;
            if (mouse.type == ItemID.None)
            {
                if (source.IsAir)
                {
                    return;
                }

                Main.playerInventory = true;
                Main.mouseItem = source.Clone();
                Main.mouseItem.stack = 1;
                source.stack--;
                if (source.stack == 0)
                {
                    Item temp = new();
                    Info.Changed(InvUI.Ins.Token, ref temp);
                    return;
                }
                rightDown = true;
            }
            else if (mouse.type == source.type)
            {
                if (source.stack <= 0 || !ItemLoader.CanStack(mouse, source))
                {
                    return;
                }

                Main.playerInventory = true;
                mouse.stack++;
                source.stack--;
                if (source.stack == 0)
                {
                    Item temp = new();
                    Info.Changed(InvUI.Ins.Token, ref temp);
                    return;
                }
                rightDown = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
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
                    if (source.stack <= 0)
                    {
                        Item temp = new();
                        Info.Changed(InvUI.Ins.Token, ref temp);
                        time = 0;
                        return;
                    }
                    int count = Math.Clamp(mult - 20, 1, source.stack);
                    if (time % space == 0)
                    {
                        Main.mouseItem.stack += count;
                        source.stack -= count;
                        if (source.stack <= 0)
                        {
                            Item temp = new();
                            Info.Changed(InvUI.Ins.Token, ref temp);
                            time = 0;
                            return;
                        }
                        Info.Changed(InvUI.Ins.Token, ref source, false);
                    }
                }
                time++;
            }
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            Item item = Info.Item;
            Rectangle rect = GetDimensions().ToRectangle();
            sb.Draw(!item.IsAir && item.favorited ? TextureAssets.InventoryBack10.Value : TextureAssets.InventoryBack.Value, rect, Color.White);
            if (item.stack > 0)
            {
                if (IsMouseHovering)
                {
                    Main.hoverItemName = item.Name;
                    Main.HoverItem = item;
                    OverrideCurosr();
                }
                ItemSlot.DrawItemIcon(item, 0, sb, rect.Center.ToVector2(), Main.inventoryScale, 52 * Main.inventoryScale, Color.White);
                if (item.stack == 1)
                {
                    return;
                }

                ReLogic.Graphics.DynamicSpriteFont font = FontAssets.ItemStack.Value;
                string stack = item.stack.ToString();
                Vector2 size = font.MeasureString(stack);
                Vector2 offset = new Vector2(8, -24) * 0.8f;
                ChatManager.DrawColorCodedStringWithShadow(sb, font, stack,
                    rect.BottomLeft() + offset, Color.White, 0, Vector2.Zero, Vector2.One * 0.8f);
            }
            //ItemSlot.Draw(sb, ref item, 0, GetDimensions().ToRectangle().TopLeft());
        }
        private bool HandleLeftClick()
        {
            KeyboardState state = Keyboard.GetState();
            Player player = Main.LocalPlayer;
            if (state.IsKeyDown(Keys.LeftShift))
            {
                Item item = player.GetItem(Main.myPlayer, Info.Item, new());
                Info.Changed(InvUI.Ins.Token, ref item);
                return true;
            }
            if (state.IsKeyDown(Keys.LeftControl))
            {
                if (Main.npcShop > 0)
                {
                    if (player.SellItem(Info.Item))
                    {
                        Item temp = new();
                        Info.Changed(InvUI.Ins.Token, ref temp);
                        return true;
                    }
                }
                else
                {
                    player.trashItem = Info.Item;
                    Item temp = new();
                    Info.Changed(InvUI.Ins.Token, ref temp);
                    return true;
                }
            }
            if (state.IsKeyDown(Keys.LeftAlt))
            {
                ref bool f = ref Info.Item.favorited;
                f = !f;
                return true;
            }
            return false;
        }
        private void OverrideCurosr()
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.LeftShift))
            {
                Main.cursorOverride = CursorOverrideID.BackInventory;
                return;
            }
            if (state.IsKeyDown(Keys.LeftControl))
            {
                if (Main.npcShop > 0)
                {
                    Main.cursorOverride = CursorOverrideID.QuickSell;
                    return;
                }
                Main.cursorOverride = CursorOverrideID.TrashCan;
                return;
            }
            if (state.IsKeyDown(Keys.LeftAlt))
            {
                Main.cursorOverride = CursorOverrideID.FavoriteStar;
                return;
            }
        }
    }
}