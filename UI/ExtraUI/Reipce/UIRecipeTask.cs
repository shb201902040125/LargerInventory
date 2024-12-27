using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LargerInventory.UI.ExtraUI.Reipce
{
    public class UIRecipeTask : UIElement
    {
        public readonly int recipeIndex;
        public int mode;
        public int waitCount;
        public int keepCount;
        public bool Notify;
        public bool PutIntoVanilla;
        public bool IgnoreFavorite;
        public UIRecipeTask(UIRecipe ur)
        {
            Height.Pixels = ur.Height.Pixels + 10;
            SetPadding(5);
            recipeIndex = ur.recipeIndex;
            Append(ur);
            int x = (int)ur.Width.Pixels + 5, y = 0;

            Recipe recipe = Main.recipe[recipeIndex];
            UIText label = new(recipe.createItem.Name);
            Append(label);

            UITextButton edit = new(MiscHelper.GTV("UI.Common.Edit"));
            edit.SetPos(-edit.Width.Pixels, -edit.Height.Pixels, 1, 1);
            edit.OnLeftMouseDown += Edit_OnLeftMouseDown;
            Append(edit);
        }

        private void Edit_OnLeftMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            throw new System.NotImplementedException();
        }
    }
}
