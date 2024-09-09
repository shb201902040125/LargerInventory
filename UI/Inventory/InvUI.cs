using Microsoft.Xna.Framework;
using RUIModule.RUIElements;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Inv = LargerInventory.BackEnd.Inventory;

namespace LargerInventory.UI.Inventory;

public partial class InvUI : ContainerElement
{
    internal static InvUI Ins { get; private set; }
    public InvUI() => Ins = this;
    private static Dictionary<int, List<Item>> Items => Inv._items;
    private static bool invRightDown;
    private static int invRightTime;
    private static UIInvItemSlot currentSlot;
    private UIContainerPanel view;
    public override void OnInitialization()
    {
        base.OnInitialization();
        UICornerPanel bg = new(520, 320, null);
        bg.SetSize(520, 320);
        bg.SetMargin(10);
        bg.SetCenter(0, 0, 0.5f, 0.5f);
        Register(bg);
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
        line.SetSize(0, 2, 1);
        line.SetPos(0, 60);
        bg.Register(line);

        UIBottom viewBg = new(0, -70, 1, 1);
        viewBg.SetPos(0, 70);
        bg.Register(viewBg);

        view = new();
        view.SetSize(-40, 0, 1, 1);
        view.autoPos = [5, 5];
        viewBg.Register(view);

        VerticalScrollbar scroll = new(62);
        view.SetVerticalScrollbar(scroll);
    }
    public override void Update(GameTime gt)
    {
        base.Update(gt);
        if (invRightDown)
        {
            if (!Main.mouseRight)
            {
                invRightDown = false;
                invRightTime = 0;
                return;
            }
            InvRightHolding(currentSlot);
            invRightTime++;
        }
    }
    public void Refresh(Predicate<Item> condition = null)
    {
        view.ClearAllElements();
        foreach (int type in Items.Keys)
        {
            for (int index = 0; index < Items[type].Count; index++)
            {
                Item item = Items[type][index];
                if (condition?.Invoke(item) != false)
                {
                    UIInvItemSlot slot = new(item, type, index);
                    HandleEvent(slot);
                    view.Register(slot);
                }
            }
        }
        view.Calculation();
    }
    private static void HandleEvent(UIItemSlot slot)
    {
        slot.Events.OnLeftDown += uie =>
        {
            //左键单机
            if (uie is not UIInvItemSlot s)
            {
                return;
            }
            if (s.item.IsAir)
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
                if (s.item.type == Main.mouseItem.type)
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
        slot.Events.OnRightDown += uie =>
        {
            //右键按下
            //栏位是空拿不到任何东西
            if (uie is not UIInvItemSlot s || s.item.IsAir)
            {
                return;
            }
            currentSlot = s;
            //鼠标没东西时
            if (Main.mouseItem.IsAir)
            {
                Main.mouseItem.type = s.item.type;
                Inv.PickItemFromDesignatedIndex(Main.mouseItem, s.Index, 1);
            }
            else if (Main.mouseItem.type == s.Type)
            {
                Inv.PickItemFromDesignatedIndex(Main.mouseItem, s.Index, 1);
            }
            invRightDown = true;
        };
    }
    private static void InvRightHolding(UIInvItemSlot s)
    {
        int time = invRightTime;

        if (s.item.IsAir || s.Type != Main.mouseItem.type)
        {
            return;
        }
        else if (time > 30)
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
    }
}
