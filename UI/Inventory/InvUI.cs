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
using Terraria.Localization;
using Terraria.UI;
using static LargerInventory.MiscHelper;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.Inventory;

public class InvUI : UIState
{
    internal static InvUI Ins { get; private set; }
    public InvUI() => Ins = this;
    private bool dragging;
    private Vector2 oldPos;
    private UIView view;
    private UIPanel bg;
    private UISearchBar input;
    private UIWaitRefresh waitText;
    private const string UIKey = "UI.";
    private bool load;
    private List<UIInvSlot> originSlots;
    internal bool needRefresh;

    public override void OnInitialize()
    {
        if (Main.gameMenu)
        {
            return;
        }

        if (load)
        {
            return;
        }

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
            dragging = true;
            oldPos = Main.MouseScreen;
        };
        Append(bg);


        UITextButton filter = new(InvGTV("Common.OpenFilter"));
        filter.SetPos(0, 0);
        filter.OnLeftMouseDown += (evt, ls) =>
        {
            LISystem.filterUI.OnInitialize();
            InvFilter.ChangeVisible(true/*, this*/);
        };
        bg.Append(filter);

        UITextButton clear = new(InvGTV("Common.ClearFilters"));
        clear.SetPos(70, 0);
        clear.OnLeftMouseDown += (_, _) => LISystem.filterUI.ClearFilters();
        bg.Append(clear);

        UITextButton recipe = new(InvGTV("Common.Recipe"));
        recipe.SetPos(70 + clear.Width.Pixels + 10, 0);
        recipe.OnLeftMouseDown += (_, _) =>
        {
            LISystem.recipeUI.Load();
            LISystem.recipeUIF.IsVisible = true;
            LISystem.invUIF.IsVisible = false;
        };
        bg.Append(recipe);

        UIPanel searchBg = new();
        searchBg.SetSize(100, 30);
        searchBg.SetPos(-100, 0, 1);
        searchBg.BackgroundColor = Color.White;
        searchBg.OnMouseOver += (evt, ls) => searchBg.BorderColor = Color.Gold;
        searchBg.OnMouseOut += (evt, ls) => searchBg.BorderColor = Color.Black;
        bg.Append(searchBg);

        input = new(Language.GetText("Mods.LargerInventory.UI.Inventory.Common.Search"), 1f);
        input.SetSize(0, 0, 1, 1);
        input.OnContentsChanged += SearchItem;
        searchBg.Append(input);
        searchBg.OnLeftMouseDown += (evt, ls) => input.ToggleTakingText();

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
    }

    private void SearchItem(string text)
    {
        LISystem.filterUI.currentFilter = text == string.Empty ? x => true : x => x.Name.Contains(text);
        CallRefresh();
    }

    public override void Update(GameTime gt)
    {
        base.Update(gt);
        if (bg.IsMouseHovering)
        {
            PlayerInput.LockVanillaMouseScroll(GetType().FullName);
            Main.LocalPlayer.mouseInterface = true;
            if (Main.mouseLeft || Main.mouseRight)
            {
                if (input.IsWritingText && !input.Parent.ContainsPoint(Main.MouseScreen))
                {
                    input.ToggleTakingText();
                }
            }
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
            List<Inv.InfoForUI> items = task.Result;
            waitText.SetText(InvGTV("Common.WaitRefresh"));
            view.Clear();
            int slotCount = 0;
            originSlots = [];
            foreach (Inv.InfoForUI info in items)
            {
                UIInvSlot slot = new(info);
                originSlots.Add(slot);
                slotCount++;
            }
            originSlots = [.. originSlots.OrderBy(slot => slot.Info.Item.favorited)
            .ThenBy(slot => slot.Info.Item.type)
            .ThenByDescending(slot => slot.Info.Item.stack)];
            float slotCountPerRow = (view.Width.Pixels - 10) / 62;
            int needCount = (int)(Math.Ceiling(slotCount / slotCountPerRow) * slotCountPerRow);
            if (needCount > slotCount)
            {
                while (slotCount < needCount)
                {
                    UIInvSlot Empty = new(new(-1, -1, new()));
                    originSlots.Add(Empty);
                    slotCount++;
                }
            }
            originSlots.ForEach(view.Add);

            view.Recalculate();
            waitText.hide = true;
            return;
            //view.Recalculate();
        }
        waitText.SetText(InvGTV("Common.RefreshFailed"));
        //TODO 补充刷新任务失败的显示
    }
    private List<UIItemFilter> CreateFilter()
    {
        List<UIItemFilter> filters = [];
        return filters;
    }

    private CancellationToken refreshToken;
    internal void CallRefresh()
    {
        refreshToken.ThrowIfCancellationRequested();
        refreshToken = new();
        if (InvToken.TryGetToken(new TimeSpan(0, 0, 1), out var token))
        {
            Inv.StartRefreshTask(token, LISystem.filterUI.currentFilter, refreshToken, Refresh);
        }
        //TODO 需要显示等待结果界面
    }
    private void SubmitSearch(string text)
    {
        view.Clear();
        originSlots.Where(x => x.Info.Item.Name.Contains(text)).ToList().ForEach(view.Add);
    }
    private void CancelSearch() { }
    private static string InvGTV(string key) => GTV(UIKey + key);
}
