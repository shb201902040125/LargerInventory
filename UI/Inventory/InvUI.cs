using ForOneToolkit.UI.Basic;
using ForOneToolkit.UI.Interface;
using ForOneToolkit.UI.Scroll;
using ForOneToolkit.UI.Sys;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.Inventory;

public partial class InvUI : UIContainer
{
    internal static InvUI Ins { get; private set; }
    public InvUI() => Ins = this;
    private static Dictionary<int, List<Item>> Items => Inv._items;
    private UIMovableView view;
    public override void OnInit()
    {
        UICornerPanel bg = new() { CanDrag = true };
        bg.SetSize(520, 320);
        bg.SetPadding(10);
        bg.SetCenter(0, 0);
        Add(bg);

        float x = 0, y = 0;
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

        UIImage line = new(TextureAssets.MagicPixel.Value, true, DrawTextureStyle.Full);
        line.SetSize(0, 2, 1);
        line.SetPos(0, 60);
        bg.Add(line);

        UIBottom viewBg = [];
        viewBg.SetSize(0, -70, 1, 1);
        viewBg.SetPos(0, 70);
        bg.Add(viewBg);

        view = [];
        view.SetSize(-40, 0, 1, 1);
        viewBg.Add(view);

        UIScrollV scroll = new(view, 62);
        scroll.VerticalFlowLayout(10, 10);
        viewBg.Add(scroll);
        view.AddScroll(scroll);
    }
    public void Refresh(Predicate<Item> condition = null)
    {
        view.Clear();
        foreach (int type in Items.Keys)
        {
            foreach (Item item in Items[type])
            {
                if (condition?.Invoke(item) != false)
                {
                    UIItemSlot slot = new(item);
                    HandleEvent(slot);
                    view.Add(slot);
                }
            }
        }
        view.Calculate();
    }
    private static void HandleEvent(UIItemSlot slot)
    {
        slot.OnLeftJustPress += uie =>
        {
            //左键单机
            UIItemSlot s = uie as UIItemSlot;
            ref Item mouse = ref Main.mouseItem;
            ref Item origin = ref s.Item;
            if (origin.type == 0)
            {
                if (mouse.type == 0)
                    return;
                //栏位是空执行放入，背包同时执行放入
                origin = mouse.Clone();
                Inv.PushItemToEnd(origin);
                mouse.SetDefaults();
            }
            else//栏位不空
            {
                //栏位与鼠标同类型
                if (origin.type == mouse.type)
                {
                    if (origin.stack < origin.maxStack && ItemLoader.CanStack(origin, mouse))
                    {
                        //可堆叠执行堆叠，背包无操作
                        ItemLoader.StackItems(origin, mouse, out int trans);
                    }
                    else
                    {
                        //不可堆叠执行交换，背包同步交换
                        Inv.PopItems(origin);
                        (mouse, origin) = (origin, mouse);
                        Inv.PushItemToEnd(origin);
                    }
                }
                else
                {
                    //不同则交换，背包执行删除和放入
                    Inv.PopItems(origin);
                    (mouse, origin) = (origin, mouse);
                    Inv.PushItemToEnd(origin);
                }
            }
        };
        slot.OnRightJustPress += uie =>
        {
            //右键按下
            UIItemSlot s = uie as UIItemSlot;
            ref Item mouse = ref Main.mouseItem;
            ref Item origin = ref s.Item;
            //栏位是空拿不到任何东西
            if (origin.type == 0)
                return;

            //鼠标没东西时
            if (mouse.type == 0)
            {
                mouse = origin.Clone();
                mouse.stack = 1;
                if (origin.stack <= 1)
                {
                    Inv.PopItems(origin);
                    origin.SetDefaults();
                }
                else
                {
                    origin.stack--;
                }
            }
            else if (mouse.type == origin.type)
            {
                if (origin.stack <= 1)
                {
                    Inv.PopItems(origin);
                    ItemLoader.StackItems(mouse, origin, out _, false, 1);
                    origin.SetDefaults();
                }
                else
                {
                    ItemLoader.StackItems(mouse, origin, out _, false, 1);
                }
            }
        };
        slot.OnRightHolding += uie =>
        {
            UIItemSlot s = uie as UIItemSlot;
            int time = UISystem.Manager.MouseRight.KeepTime;
            ref Item mouse = ref Main.mouseItem;
            ref Item origin = ref s.Item;
            if (origin.type == 0 || origin.type != mouse.type || !ItemLoader.CanStack(origin, mouse))
            {
                return;
            }
            else if (time > 30)
            {
                time--;
                int mult = (time - 20) / 5;
                int space = (int)(Math.Sqrt(20 - Math.Min(mult, 19)));
                int count = Math.Max(1, mult - 20);
                count = Math.Min(count, origin.stack);
                if (time % space == 0)
                {
                    if (origin.stack == count)
                    {
                        Inv.PopItems(origin);
                    }
                    ItemLoader.StackItems(mouse, origin, out _, false, count);
                }
            }
        };
    }
}
