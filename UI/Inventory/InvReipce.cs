using LargerInventory.BackEnd;
using LargerInventory.UI.ExtraUI;
using LargerInventory.UI.ExtraUI.Reipce;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static LargerInventory.MiscHelper;

namespace LargerInventory.UI.Inventory
{
    public class InvReipce : UIState
    {
        internal static bool IsVisable;
        internal static bool load;
        private UIPanel bg;
        private const string UIKey = "UI.";
        private bool dragging;
        private Vector2 oldPos;
        private UIView view;
        private UIReipceEditor editor;
        public override void OnInitialize()
        {
            if (Main.gameMenu)
                return;
            if (load)
                return;
            load = true;
            bg = new()
            {
                VAlign = 0.5f,
                HAlign = 0.5f
            };
            bg.SetSize(650, 400);
            bg.SetMargin(10, 10, 10, 10);
            bg.OnLeftMouseDown += (_, _) =>
            {
                dragging = true;
                oldPos = Main.MouseScreen;
            };
            Append(bg);

            UITextButton filter = new(InvGTV("Common.OpenFilter"));
            filter.SetPos(0, 0);
            filter.OnLeftMouseDown += (evt, ls) =>
            {
                LISystem.filterUI.OnInitialize();
                InvFilter.ChangeVisible(true, this);
            };
            bg.Append(filter);

            UIPanel viewBg = new();
            viewBg.SetSize(0, -40, 1, 1);
            viewBg.SetPos(0, 40);
            bg.Append(viewBg);

            view = new()
            {
                ListPaddingX = 10,
                ListPaddingY = 10,
            };
            view.SetSize(-40, 0, 1, 1);
            view.ManualRePosMethod = (list, px, py) =>
            {
                float h = 0;
                float x = 0, y = 0;
                int w = view.GetDimensions().ToRectangle().Width;
                foreach (UIElement uie in list)
                {
                    uie.SetPos(x, y);
                    Rectangle rect = uie.GetDimensions().ToRectangle();
                    x += rect.Width + px;
                    h = y + rect.Height;
                    if (x + rect.Width > w)
                    {
                        x = 0;
                        y += rect.Height + py;
                    }
                }
                return h;
            };
            viewBg.Append(view);

            UIScrollbar scroll = new();
            scroll.Height.Set(0, 1);
            scroll.Left.Set(-20, 1);
            view.SetScrollbar(scroll);
            viewBg.Append(scroll);

            editor = new();
            editor.SetSize(200, 400);
            bg.Append(editor);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (dragging)
            {
                if (!Main.mouseLeft)
                {
                    dragging = false;
                    return;
                }
                Vector2 offset = Main.MouseScreen - oldPos;
                if (offset != Vector2.Zero)
                {
                    bg.Left.Pixels += offset.X;
                    bg.Top.Pixels += offset.Y;
                    bg.Recalculate();
                    var rect = bg.GetDimensions().ToRectangle();
                    editor.Left.Pixels += rect.Left - editor.Width.Pixels - 10;
                    editor.Top.Pixels += rect.Top + rect.Height / 2f - editor.Top.Pixels / 2;
                    editor.Recalculate();
                }
                oldPos = Main.MouseScreen;
            }
        }
        private static string InvGTV(string key) => GTV(UIKey + key);
        public void OpenEditor(UIRecipeTask rt)
        {

        }
    }
}