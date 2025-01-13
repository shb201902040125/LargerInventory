using LargerInventory.BackEnd;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace LargerInventory.UI.ExtraUI.Reipce
{
    public class UIRecipeTask : UIPanel
    {
        public readonly RecipeTask rt;
        public int Mode { get => (int)rt.Type; internal set => rt.Type = (RecipeTask.TaskType)value; }
        public int Count { get => rt.TargetCount; internal set => rt.TargetCount = value; }
        public bool Notify { get => rt.Notify; internal set => rt.Notify = value; }
        public bool PutIntoVanilla { get => rt.PutIntoVanilla; internal set => rt.PutIntoVanilla = value; }
        public bool IgnoreFavorite { get => rt.IgnoreFavorite; internal set => rt.IgnoreFavorite = value; }
        public Dictionary<int, Dictionary<int, bool>> RecipeGroups => rt.RecipeGroups;
        private bool editing;
        private readonly UISearchBar input;
        public UIRecipeTask(int recipeIndex) : this(new RecipeTask(Main.recipe[recipeIndex], 0, RecipeTask.TaskType.Timer))
        {

        }
        public UIRecipeTask(RecipeTask rt)
        {
            this.rt = rt;
            this.SetSize(0, 62, 1);
            SetPadding(5);
            int recipeIndex = rt.Recipe.RecipeIndex;
            UIRecipe r = new(recipeIndex)
            {
                VAlign = 0.5f
            };
            Append(r);

            Recipe recipe = Main.recipe[recipeIndex];
            UIText label = new(recipe.createItem.Name);
            label.SetPos(62, 5);
            Append(label);

            UITextButton edit = new(MiscHelper.GTV("UI.Common.Edit"));
            edit.SetPos(-edit.Width.Pixels, -edit.Height.Pixels, 1, 1);
            edit.OnLeftMouseDown += Edit_OnLeftMouseDown;
            Append(edit);

            foreach (var accepts in recipe.acceptedGroups)
            {
                var rg = RecipeGroup.recipeGroups[accepts];
                RecipeGroups[accepts] = rg.ValidItems.ToDictionary(x => x, _ => true);
            }

            UIPanel searchBg = new();
            searchBg.SetSize(-62 - edit.Width.Pixels - 10, 30, 1);
            searchBg.SetPos(62, -30, 0, 1);
            searchBg.BackgroundColor = Color.White;
            searchBg.OnMouseOver += (evt, ls) => searchBg.BorderColor = Color.Gold;
            searchBg.OnMouseOut += (evt, ls) => searchBg.BorderColor = Color.Black;
            Append(searchBg);

            input = new(Language.GetText("Mods.LargerInventory.UI.Inventory.Recipe.Count"), 1f);
            input.SetSize(0, 0, 1, 1);
            input.OnContentsChanged += Input_OnContentsChanged;
            searchBg.Append(input);
            searchBg.OnLeftMouseDown += (evt, ls) => input.ToggleTakingText();
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (editing && LISystem.editorUI.EditingRT != this)
            {
                editing = false;
                BorderColor = Color.Black;
            }
            base.DrawSelf(spriteBatch);
        }

        private void Input_OnContentsChanged(string obj)
        {
            if (int.TryParse(obj, out var c))
            {
                Count = c;
            }
            else
                input.SetContents(Count.ToString());
        }

        private void Edit_OnLeftMouseDown(UIMouseEvent evt, UIElement listeningElement)
        {
            LISystem.editorUIF.IsVisible = true;
            LISystem.editorUI.OpenEditor(this);
            editing = true;
            BorderColor = Color.Gold;
        }
    }
}
