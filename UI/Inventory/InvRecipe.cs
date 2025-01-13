using LargerInventory.BackEnd;
using LargerInventory.UI.ExtraUI;
using LargerInventory.UI.ExtraUI.Reipce;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;

namespace LargerInventory.UI.Inventory
{
    public class InvRecipe : UIState
    {
        public int hoverRecipe = -1;
        private int pickRecipe = -1;
        private UIView taskView;
        private UIView recipeView;
        private bool loaded;
        public override void OnInitialize()
        {
            RemoveAllChildren();
            #region 基本
            UIPanel panel = new();
            panel.SetSize(800, 600);
            panel.SetPos(-400, -300, 0.5f, 0.5f);
            panel.SetPadding(5);
            Append(panel);

            UIPanel left = new();
            left.SetSize(-5, 0, 0.5f, 1);
            left.SetPadding(5);
            panel.Append(left);

            UIPanel right = new();
            right.SetSize(-5, -40, 0.5f, 1);
            right.SetPos(5, 40, 0.5f);
            right.SetPadding(5);
            panel.Append(right);

            UIView leftView = [];
            leftView.SetSize(-40, 0, 1, 1);
            left.Append(leftView);
            taskView = leftView;

            UIScrollbar scroll = new();
            scroll.Height.Set(-20, 1);
            scroll.SetPos(-20, 10, 1);
            leftView.SetScrollbar(scroll);
            left.Append(scroll);

            UIView rightView = [];
            rightView.SetSize(-40, 0, 1, 1);
            right.Append(rightView);
            recipeView = rightView;

            scroll = new();
            scroll.Height.Set(-20, 1);
            scroll.SetPos(-20, 10, 1);
            rightView.SetScrollbar(scroll);
            right.Append(scroll);
            #endregion

            #region 合成表
            foreach (Recipe recipe in Main.recipe)
            {
                if (recipe.Disabled)
                    continue;
                if (recipe.RecipeIndex < 0)
                    continue;
                UIRecipe r = new(recipe.RecipeIndex);
                rightView.Add(r);
            }
            rightView.Recalculate();

            UIPanel searchBg = new();
            searchBg.SetSize(-5, 30, 0.5f);
            searchBg.SetPos(5, 0, 0.5f);
            searchBg.BackgroundColor = Color.White;
            searchBg.OnMouseOver += (evt, ls) => searchBg.BorderColor = Color.Gold;
            searchBg.OnMouseOut += (evt, ls) => searchBg.BorderColor = Color.Black;
            panel.Append(searchBg);

            UISearchBar search = new(Language.GetText("Mods.LargerInventory.UI.Inventory.Common.Search"), 1f);
            search.SetSize(0, 0, 1, 1);
            search.OnContentsChanged += Search_OnContentsChanged;
            searchBg.Append(search);
            searchBg.OnLeftMouseDown += (evt, ls) => search.ToggleTakingText();
            #endregion
        }

        private void Search_OnContentsChanged(string obj)
        {
            bool empty = string.IsNullOrEmpty(obj);
            foreach (Recipe recipe in Main.recipe)
            {
                if (recipe.Disabled)
                    continue;
                if (recipe.RecipeIndex < 0)
                    continue;
                if (empty || recipe.createItem.Name.Contains(obj))
                {
                    UIRecipe r = new(recipe.RecipeIndex);
                    recipeView.Add(r);
                }
            }
            recipeView.Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            hoverRecipe = -1;
            base.Update(gameTime);
            if (Children.First().ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
                PlayerInput.LockVanillaMouseScroll("InvRecipe");
            }
            if (pickRecipe == -1 && Main.mouseRight)
            {
                pickRecipe = hoverRecipe;
            }
            if (pickRecipe > -1 && Main.mouseRightRelease)
            {
                if (taskView.ContainsPoint(Main.MouseScreen))
                {
                    UIRecipeTask rt = new(pickRecipe);
                    taskView.Add(rt);
                    taskView.Recalculate();
                }
                pickRecipe = -1;
            }
        }
        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            float scale = Main.inventoryScale;
            Main.inventoryScale = 0.75f;
            base.DrawChildren(spriteBatch);
            Main.inventoryScale = scale;
        }
        public void Load()
        {
            if (loaded)
                return;
            loaded = true;
            OnInitialize();
            //TODO: 把遍历目标加进来          ↓↓↓
            /*foreach (RecipeTask rt in Inventory.rts)
            {
                UIRecipeTask task = new(rt);
                taskView.Add(task);
            }
            taskView.RecalculateChildren();*/
        }
    }
}
