using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.Inventory;

public partial class InvUI : UIState
{
    internal static InvUI Ins { get; private set; }
    public InvUI() => Ins = this;
    private static Dictionary<int, List<Item>> Items => Inv._items;
    private bool dragging;
    private Vector2 oldPos;
    private UIList view;
    private UIPanel bg;
    public override void OnInitialize()
    {
        bg = new()
        {
            Width = new(520, 0),
            Height = new(320, 0),
            VAlign = 0.5f,
            HAlign = 0.5f,
            MarginLeft = 10,
            MarginRight = 10,
            MarginTop = 10,
            MarginBottom = 10,
        };
        bg.OnLeftMouseDown += (UIMouseEvent evt, UIElement listeningElement) =>
        {
            dragging = true;
            oldPos = Main.MouseScreen;
        };
        Append(bg);
        /*const string vp = "Terraria/Images/";
        for (int i = 0; i < 4; i++)
        {
            UICornerPanel bottom = new() { Sensitive = true };
            bottom.SetSize(52, 52);
            bottom.SetPos(x, y);
            bottom.HoverToColor();
            x += 10 + bottom.Width;
            bg.Add(bottom);

            string tex = i switch
            {
                0 => "ChestStack_1",
                1 => "UI/Sort_1",
                _ => string.Empty
            };
            string tip = i switch
            {
                0=>
            }
            if (tex == string.Empty)
                continue;
            UIImage allIN = new(T2D(vp + tex)) { DrawTextureStyle = DrawTextureStyle.FromCenter };
            allIN.SetCenter(0, 0);
            bottom.Add(allIN);}*/

        UIImage line = new(TextureAssets.MagicPixel.Value);
        line.ScaleToFit = true;
        line.SetSize(0, 2, 1);
        line.SetPos(0, 60);
        bg.Append(line);

        UIPanel viewBg = new();
        viewBg.SetSize(0, -70, 1, 1);
        viewBg.SetPos(0, 70);
        bg.Append(viewBg);

        view = [];
        view.SetSize(-40, 0, 1, 1);
        view.ManualSortMethod = list =>
        {
            int x = 0, y = 0;
            int w = view.GetDimensions().ToRectangle().Width;
            foreach (UIElement uie in list)
            {
                uie.SetPos(x, y);
                Rectangle rect = uie.GetDimensions().ToRectangle();
                x += rect.Width + 10;
                if (x + rect.Width > w)
                {
                    x = 0;
                    y += rect.Height;
                }
            }
        };
        //view.autoPos = [5, 5];
        viewBg.Append(view);

        UIScrollbar scroll = new();
        scroll.Height.Set(0, 1);
        scroll.Left.Set(-20, 1);
        view.SetScrollbar(scroll);
        viewBg.Append(scroll);
    }
    public override void Update(GameTime gt)
    {
        base.Update(gt);
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
            }
            oldPos = Main.MouseScreen;
        }
    }
    public void Refresh(Predicate<Item> condition = null)
    {
        view.Clear();
        foreach (int type in Items.Keys)
        {
            Item[] array = [.. Items[type]];
            int count = array.Length;
            for (int index = 0; index < count; index++)
            {
                Item item = Items[type][index];
                if (condition?.Invoke(item) != false)
                {
                    UIItemSlot slot = new(array, index, 0);
                    view.Add(slot);
                }
            }
        }
        view.Recalculate();
    }
}
