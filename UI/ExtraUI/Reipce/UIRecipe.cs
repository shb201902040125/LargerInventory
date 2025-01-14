using LargerInventory.BackEnd;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace LargerInventory.UI.ExtraUI.Reipce
{
    public class UIRecipe : UIElement
    {
        public readonly int recipeIndex;
        public UIRecipe(int recipeIndex)
        {
            this.recipeIndex = recipeIndex;
            this.SetSize(52, 52);
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            bool indexInRange = Main.recipe.IndexInRange(recipeIndex);
            if (!indexInRange)
            {
                Main.NewText(recipeIndex);
                return;
            }
            Item item = Main.recipe[recipeIndex].createItem;
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
                Vector2 offset = new Vector2(8, -24) * 0.8f;
                ChatManager.DrawColorCodedStringWithShadow(sb, font, stack,
                    rect.BottomLeft() + offset, Color.White, 0, Vector2.Zero, Vector2.One * 0.8f);
            }
        }
        private static void OverrideCurosr()
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.LeftShift))
            {
                Main.cursorOverride = CursorOverrideID.BackInventory;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (IsMouseHovering)
            {
                LISystem.recipeUI.hoverRecipe = recipeIndex;
            }
        }
    }
}
