using ForOneToolkit.UI.Basic;
using ForOneToolkit.UI.Interface;
using ForOneToolkit.UI.Scroll;
using ForOneToolkit.UI.Sys;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
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
            for(int index=0;index< Items[type].Count;index++)
            {
                Item item = Items[type][index];
                if (condition?.Invoke(item) != false)
                {
                    UIInvItemSlot slot = new(item, type, index);
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
            if (uie is not UIInvItemSlot s)
            {
                return;
            }
            if (s.Item.IsAir)
            {
                if (Main.mouseItem.IsAir)
                {
                    return;
                }
                //栏位是空执行放入，背包同时执行放入
                Inv.PutItemToDesignatedIndex(Main.mouseItem, s.Index);
                if (Main.mouseItem.IsAir)
                {
                    Main.mouseItem.SetDefaults(ItemID.None);
                }
            }
            else//栏位不空
            {
                //栏位与鼠标同类型
                if (s.Item.type == Main.mouseItem.type)
                {
                    if (!Inv.PutItemToDesignatedIndex(Main.mouseItem, s.Index))
                    {
                        Inv.ExchangeItems(ref Main.mouseItem, s.Index);
                    }
                }
                else
                {
                    //不同则交换，背包执行删除和放入
                    if (Inv.PopItems(s.Type, s.Index, out Item item))
                    {
                        (item, Main.mouseItem) = (Main.mouseItem, item);
                        Inv.PushItemToEnd(item);
                        //TODO
                        //需要刷新UI列表
                    }
                }
            }
        };
        slot.OnRightJustPress += uie =>
        {
            //右键按下
            //栏位是空拿不到任何东西
            if (uie is not UIInvItemSlot s || s.Item.IsAir)
            {
                return;
            }

            //鼠标没东西时
            if (Main.mouseItem.IsAir)
            {
                Main.mouseItem.type = s.Item.type;
                Inv.PickItemFromDesignatedIndex(Main.mouseItem, s.Index, 1);
            }
            else if (Main.mouseItem.type == s.Type)
            {
                Inv.PickItemFromDesignatedIndex(Main.mouseItem, s.Index, 1);
            }
        };
        slot.OnRightHolding += uie =>
        {
            if (uie is not UIInvItemSlot s)
            {
                return;
            }
            int time = UISystem.Manager.MouseRight.KeepTime;

            if (s.Item.IsAir || s.Type != Main.mouseItem.type)
            {
                return;
            }
            else if (time > 300)
            {
                time--;
                int mult = (time - 20) / 5;
                int space = (int)Math.Sqrt(20 - Math.Min(mult, 19));
                int count = Math.Max(1, mult - 20);
                if (time % space == 0)
                {
                    Inv.PickItemFromDesignatedIndex(Main.mouseItem, s.Index, count);
                }
            }
        };
    }
}
