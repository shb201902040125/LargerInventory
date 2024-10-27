using LargerInventory.BackEnd;
using LargerInventory.UI.ExtraUI;
using LargerInventory.UI.ExtraUI.FIlters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;
using static LargerInventory.MiscHelper;
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
    private UIWaitRefresh waitText;
    private bool waiting;
    private const string UIKey = "UI.Inventory.";
    private bool load;
    internal bool needRefresh;

    public override void OnInitialize()
    {
        if (Main.gameMenu)
            return;
        if (load)
            return;
        load = true;
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
            if (bg.GetElementAt(Main.MouseScreen) == bg)
            {
                dragging = true;
                oldPos = Main.MouseScreen;
            }
        };
        Append(bg);


        UITextButton filter = new(InvGTV("Common.OpenFilter"));
        filter.SetPos(0, 0);
        filter.OnLeftMouseDown += (evt, ls) =>
        {
            LISystem.filterUI.OnInitialize();
            InvFilter.ChangeVisible(true);
        };
        bg.Append(filter);

        UITextButton clear = new(InvGTV("Common.ClearFilters"));
        clear.SetPos(70, 0);
        clear.OnLeftMouseDown += (_, _) => LISystem.filterUI.ClearFilters();
        bg.Append(clear);

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

        waitText = new(InvGTV("Common.WaitRefresh"))
        {
            HAlign = VAlign = 0.5f,
            hide = true
        };
        viewBg.Append(waitText);

        view = new()
        {
            ListPaddingX = 10,
            ListPaddingY = 10,
        };
        view.SetSize(-40, 0, 1, 1);
        view.ManualRePosMethod = (list, px, py) =>
        {
            if (waiting)
            {
                return 0;
            }
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
        if (!waiting)
        {
            base.Update(gt);
        }
        if (bg.IsMouseHovering)
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
    internal void Refresh(Task<List<Inv.InfoForUI>> task)
    {
        waitText.hide = false;
        if (task.IsCompletedSuccessfully)
        {
            waiting = true;
            var items = task.Result;
            waitText.SetText(InvGTV("Common.WaitRefresh"));
            view.Clear();
            view.Deactivate();
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
            view.Activate();
            waiting = false;
            waitText.hide = true;
            return;
            //view.Recalculate();
        }
        waitText.SetText(InvGTV("Common.RefreshFailed"));
        //TODO 补充刷新任务失败的显示
    }
    private List<UIItemFilter> CreateFilter()
    {
        List<UIItemFilter> filters = new();
        return filters;
    }
    CancellationToken refreshToken;
    internal void CallRefresh()
    {
        refreshToken.ThrowIfCancellationRequested();
        refreshToken = new();
        Inv.StartRefreshTask(LISystem.filterUI.currentFilter, refreshToken, Refresh);
        //TODO 需要显示等待结果界面
    }
    private static string InvGTV(string key) => GTV(UIKey + key);
}
