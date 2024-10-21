using LargerInventory.BackEnd;
using LargerInventory.UI.ExtraUI;
using LargerInventory.UI.ExtraUI.FIlters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.Inventory;

public partial class InvUI : UIState
{
    internal static InvUI Ins { get; private set; }
    public InvUI() => Ins = this;
    private bool dragging;
    private Vector2 oldPos;
    private UIView view;
    private UIPanel bg;
    internal bool needRefresh;

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


        UIPanel buttonBg = new();
        buttonBg.SetSize(60, 30);
        buttonBg.SetPos(0, 0);
        bg.Append(buttonBg);

        UIText confirm = new("筛选");
        confirm.Top.Pixels -= 5;
        buttonBg.Append(confirm);
        buttonBg.OnMouseOver += (evt, ls) =>
        {
            confirm.TextColor = Color.Gold;
        };
        buttonBg.OnMouseOut += (evt, ls) =>
        {
            confirm.TextColor = Color.White;
        };
        buttonBg.OnLeftMouseDown += (evt, ls) =>
        {
            dragging = false;
            LISystem.filterUI.OnInitialize();
            LISystem.filterUIF.IsVisible = true;
            LISystem.invUIF.IsVisible = false;
        };

        UIImage line = new(TextureAssets.MagicPixel.Value)
        {
            ScaleToFit = true
        };
        line.SetSize(0, 2, 1);
        line.SetPos(0, 60);
        bg.Append(line);

        UIPanel viewBg = new();
        viewBg.SetSize(0, -70, 1, 1);
        viewBg.SetPos(0, 70);
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
            float x = px, y = py;
            int w = view.GetDimensions().ToRectangle().Width;
            foreach (UIElement uie in list)
            {
                uie.SetPos(x, y);
                Rectangle rect = uie.GetDimensions().ToRectangle();
                x += rect.Width + px;
                h = y + rect.Height;
                if (x + rect.Width > w)
                {
                    x = px;
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
    }
    public override void Update(GameTime gt)
    {
        base.Update(gt);
        if (IsMouseHovering)
        {
            PlayerInput.LockVanillaMouseScroll(GetType().FullName);
            Main.LocalPlayer.mouseInterface = true;
        }
        if (needRefresh)
        {
            CallRefresh();
            needRefresh = false;
        }
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
    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        ref float scale = ref Main.inventoryScale;
        float old = scale;
        scale = 0.75f;
        base.DrawChildren(spriteBatch);
        scale = old;

    }
    internal void Refresh(List<Inv.InfoForUI> items)
    {
        view.Clear();
        int slotCount = 0;
        List<UIInvSlot> temp = [];
        foreach (var info in items)
        {
            UIInvSlot slot = new(info);
            temp.Add(slot);
            slotCount++;
        }
        temp = [.. temp.OrderBy(slot => slot.Info.Item.favorited)
            .ThenBy(slot => slot.Info.Item.type)
            .ThenByDescending(slot => slot.Info.Item.stack)];
        var slotCountPerRow = (view.Width.Pixels - 10) / 62;
        int needCount = (int)(Math.Ceiling(slotCount / slotCountPerRow) * slotCountPerRow);
        if (needCount > slotCount)
        {
            while (slotCount < needCount)
            {
                UIInvSlot Empty = new(new(-1, -1, new()));
                temp.Add(Empty);
                slotCount++;
            }
        }
        temp.ForEach(view.Add);
        view.Recalculate();
    }
    private List<UIItemFilter> CreateFilter()
    {
        List<UIItemFilter> filters = new();
        return filters;
    }
    CancellationToken refreshToken;
    InvItemFilter lastInvItemFilter;
    internal void CallRefresh()
    {
        refreshToken.ThrowIfCancellationRequested();
        refreshToken = new();
        Inv.StartRefreshTask(lastInvItemFilter, refreshToken);
    }
}
