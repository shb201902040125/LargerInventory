using LargerInventory.BackEnd;
using LargerInventory.UI.ExtraUI;
using LargerInventory.UI.ExtraUI.FIlters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static LargerInventory.BackEnd.InvItemFilter.FilterPrefab;
using static LargerInventory.MiscHelper;

namespace LargerInventory.UI.Inventory
{
    public class InvFilter : UIState
    {
        public Func<Item, bool> currentFilter;
        public bool IsVisable;
        internal static HashSet<int> usedIcon;
        internal static bool load;
        private UIPanel bg;
        private List<UIItemFilter> filters;
        private const string UIKey = "UI.ItemFilter.";
        CancellationToken refreshToken;
        public override void OnInitialize()
        {
            #region 基本设定
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
            Append(bg);

            UIPanel viewBg = new();
            bg.Append(viewBg);

            UIView view = [];
            view.ManualSortMethod = new(_ =>
            {
                return;
            });
            view.ManualRePosMethod = new((list, px, py) =>
            {
                if (list.Count == 0)
                    return 0;
                UIElement uie = list.Last();
                return uie.Top.Pixels + uie.Height.Pixels;
            });
            view.SetSize(-40, 0, 1, 1);
            viewBg.Append(view);

            UIScrollbar scroll = new();
            scroll.Height.Set(0, 1);
            scroll.Left.Set(-20, 1);
            view.SetScrollbar(scroll);
            viewBg.Append(scroll);
            #endregion

            #region 确认按钮
            List<UITextButton> buttons = [];
            UITextButton confirm = new(FilterGTV("Common.Verify"));//确认
            confirm.OnLeftMouseDown += ConfirmFilter;
            bg.Append(confirm);
            buttons.Add(confirm);

            UITextButton reverse = new(FilterGTV("Common.Reversal"));//反转
            reverse.OnLeftMouseDown += (_, _) => filters.ForEach(f =>
            {
                if (f.filterActive)
                {
                    f.Reverse = !f.Reverse;
                }
            });
            bg.Append(reverse);
            buttons.Add(reverse);

            UITextButton clear = new(FilterGTV("Common.Clear"));//清除
            clear.OnLeftMouseDown += (_, _) => filters.ForEach(f => f.Reverse = f.filterActive = false);
            bg.Append(clear);
            buttons.Add(clear);

            UITextButton cancel = new(FilterGTV("Common.Cancel"));//取消
            cancel.OnLeftMouseDown += (_, _) => ChangeVisible(false);
            bg.Append(cancel);
            buttons.Add(cancel);

            float w = buttons.Max(x => x.Width.Pixels), y = 0;
            viewBg.SetSize(-w - 10, 0, 1, 1);
            viewBg.Recalculate();
            foreach (var button in buttons)
            {
                button.SetPos(-w, y, 1);
                y += 40;
            }
            #endregion

            #region 要用的东西
            float x = 0;
            y = 0;
            usedIcon = [];
            filters = [];
            Texture2D head = ModContent.Request<Texture2D>("Terraria/Images/UI/Creative/Infinite_Icons", AssetRequestMode.ImmediateLoad).Value;

            bool TryAppend(UIItemFilter filter)
            {
                if (filter.IconItemID > 0)
                {
                    if (view.GetInnerDimensions().Width < x + 52)
                    {
                        x = 0;
                        y += filter.Height.Pixels + 5;
                    }
                    filter.SetPos(x, y);
                    view.Add(filter);
                    filters.Add(filter);
                    x += filter.Width.Pixels + 5;
                    return true;
                }
                return false;
            }

            void AppendLeader(InvItemFilter filter, string info, int index, bool noExtra = true, bool moveY = true)
            {
                x = 0;
                if (moveY)
                    y += 70;
                UIItemFilter leader = new(filter)
                {
                    OverrideTex = head,
                    sourceRect = new Rectangle(30 * index, 0, 30, 30)
                };
                leader.SetPos(x, y);
                view.Add(leader);
                filters.Add(leader);
                x += leader.Width.Pixels + 5;

                UIText text = new(info);
                text.SetPos(x, y + (noExtra ? 30 : 5));
                view.Add(text);

                y += 25;
            }

            void AppendLine()
            {
                x = 0;
                y += 27 + 5;

                UIImage line = new(TextureAssets.MagicPixel)
                {
                    ScaleToFit = true
                };
                line.SetSize(0, 2, 1);
                line.SetPos(x, y);
                view.Add(line);
                y += 7;
            }
            #endregion

            #region 武器
            AppendLeader(IsWeapon, FilterGTV("Filters.IsWeapon.Label"), 0, false, false);

            UICheckBoxText exact = new(FilterGTV("Filters.IsWeapon.Perfect"))
            {
                checkActive = true
            };
            exact.SetPos(x, y);
            view.Add(exact);
            x += exact.Width.Pixels + 10;


            UICheckBoxText countAs = new(FilterGTV("Filters.IsWeapon.CountAs"));
            countAs.SetPos(x, y);
            view.Add(countAs);
            x += countAs.Width.Pixels + 10;

            UICheckBoxText effAllow = new(FilterGTV("Filters.IsWeapon.GetEffect"));
            effAllow.SetPos(x, y);
            view.Add(effAllow);

            AppendLine();

            exact.OnLeftMouseDown += (evt, le) =>
            {
                if (!exact.checkActive)
                {
                    exact.checkActive = true;
                    countAs.checkActive = false;
                    effAllow.checkActive = false;
                    ChangeDamageClassMatchType(0);
                }
            };
            countAs.OnLeftMouseDown += (evt, le) =>
            {
                if (!countAs.checkActive)
                {
                    countAs.checkActive = true;
                    exact.checkActive = false;
                    effAllow.checkActive = false;
                    ChangeDamageClassMatchType(1);
                }
            };
            effAllow.OnLeftMouseDown += (evt, le) =>
            {
                if (!effAllow.checkActive)
                {
                    effAllow.checkActive = true;
                    exact.checkActive = false;
                    countAs.checkActive = false;
                    ChangeDamageClassMatchType(2);
                }
            };

            int count = DamageClassLoader.DamageClassCount;
            for (int i = 0; i < count; i++)
            {
                var dc = DamageClassLoader.GetDamageClass(i);
                var filter = new UIDamageClassFilter(dc)
                {
                    Label = dc.DisplayName.Value.ToString().Replace(" ", "") + "\n" + dc.PrettyPrintName()
                };
                if (filter.DamageClass != DamageClass.Default)
                {
                    TryAppend(filter);
                }
            }
            #endregion

            #region 工具
            AppendLeader(IsTool, FilterGTV("Filters.IsTool.Label"), 6);
            AppendLine();

            TryAppend(new(IsAxe) { Label = FilterGTV("Filters.IsTool.IsAxe") });
            TryAppend(new(IsHammer) { Label = FilterGTV("Filters.IsTool.IsHammer") });
            TryAppend(new(IsPick) { Label = FilterGTV("Filters.IsTool.IsPick") });
            #endregion

            #region 装备
            AppendLeader(CanEquip, FilterGTV("Filters.IsEquip.Label"), 2, false);

            UICheckBoxText vanity = new(FilterGTV("Filters.IsEquip.IsVanity"));
            vanity.OnLeftClick += (evt, le) =>
            {
                foreach (var filter in filters)
                {
                    if (filter is UIEquipFilter equip)
                    {
                        equip.CheckVanity = !equip.CheckVanity;
                    }
                }
            };
            vanity.SetPos(x, y);
            view.Add(vanity);

            AppendLine();
            var equips = Enum.GetValues<EquipType>().ToArray();
            count = equips.Length;
            for (int i = 0; i < count; i++)
            {
                TryAppend(new UIEquipFilter(IsEquip(equips[i])) { Label = FilterGTV($"Filters.IsEquip.Is{equips[i]}") });
            }
            #endregion

            #region 其他饰品
            AppendLeader(IsAccessory, FilterGTV("Filters.IsMiscAccessory.Label"), 9);
            AppendLine();
            TryAppend(new(IsLightPet) { Label = FilterGTV("Filters.IsMiscAccessory.IsLightPet") });
            TryAppend(new(IsVanityPet) { Label = FilterGTV("Filters.IsMiscAccessory.IsVanityPet") });
            TryAppend(new(IsHook) { Label = FilterGTV("Filters.IsMiscAccessory.IsHook") });
            TryAppend(new(IsMount) { Label = FilterGTV("Filters.IsMiscAccessory.IsMount") });
            TryAppend(new(IsMinecart) { Label = FilterGTV("Filters.IsMiscAccessory.IsMinecart") });
            #endregion

            #region 消耗
            AppendLeader(IsConsumeable, FilterGTV("Filters.IsConsumeable.Label"), 3);
            AppendLine();
            TryAppend(new(IsHealthOrMana)
            {
                Label = FilterGTV("Filters.IsConsumeable.IsHealthOrMana"),
                IconItemID = 3001
            });
            TryAppend(new(IsBuffPotion)
            {
                Label = FilterGTV("Filters.IsConsumeable.IsBuffPotion"),
                IconItemID = ItemID.FlaskofIchor
            });
            TryAppend(new(IsFood)
            {
                Label = FilterGTV("Filters.IsConsumeable.IsFood"),
                IconItemID = ItemID.SeafoodDinner
            });
            TryAppend(new(IsAmmo)
            {
                Label = FilterGTV("Filters.IsConsumeable.IsAmmo"),
                IconItemID = ItemID.AmmoBox
            });
            #endregion

            #region 放置
            AppendLeader(IsPlaceable, FilterGTV("Filters.IsPlaceable.Label"), 4);
            AppendLine();
            TryAppend(new(IsPlaceableTile) { Label = FilterGTV("Filters.IsPlaceable.IsPlaceableTile") });
            TryAppend(new(IsPlaceableWall) { Label = FilterGTV("Filters.IsPlaceable.IsPlaceableWall") });
            TryAppend(new(IsFurniture) { Label = FilterGTV("Filters.IsPlaceable.IsFurniture") });
            #endregion

            #region 其他
            x = 0;
            y += 70;
            UIText another = new(FilterGTV("Filters.IsOther.Label"));
            another.SetPos(x, y + 10);
            view.Add(another);

            AppendLine();

            TryAppend(new(IsMaterial)
            {
                Label = FilterGTV("Filters.IsOther.IsMaterial"),
                IconItemID = ItemID.Gel
            });
            TryAppend(new(IsDye) { Label = FilterGTV("Filters.IsOther.IsDye") });
            TryAppend(new(IsCurrency)
            {
                Label = FilterGTV("Filters.IsOther.IsCurrency"),
                IconItemID = ItemID.PlatinumCoin
            });
            TryAppend(new(ExclusionAll)
            {
                Label = FilterGTV("Filters.IsOther.ExclusionAll"),
                OverrideTex = ModContent.Request<Texture2D>("Terraria/Images/UI/SearchCancel", AssetRequestMode.ImmediateLoad).Value
            });
            #endregion
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (bg.IsMouseHovering)
            {
                PlayerInput.LockVanillaMouseScroll(GetType().FullName);
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        public static void ChangeVisible(bool openFilter)
        {
            LISystem.filterUIF.IsVisible = openFilter;
            LISystem.invUIF.IsVisible = !openFilter;
        }
        private void ConfirmFilter(UIMouseEvent evt, UIElement listeningElement)
        {
            ApplyFilters();
            ChangeVisible(false);
        }

        private void ChangeDamageClassMatchType(int type)
        {
            foreach (var filter in filters)
            {
                if (filter is UIDamageClassFilter dcF)
                {
                    dcF.FilterType = type;
                }
            }
        }

        private void ApplyFilters()
        {
            currentFilter = new(i => filters?.All(f => !f.filterActive || f.MatchItem(i)) != false);
            refreshToken.ThrowIfCancellationRequested();
            refreshToken = new();
            BackEnd.Inventory.StartRefreshTask(currentFilter, refreshToken, InvUI.Ins.Refresh);
        }

        public void ClearFilters()
        {
            filters?.ForEach(f => f.filterActive = f.Reverse = false);
            ApplyFilters();
        }

        private static string FilterGTV(string key) => GTV(UIKey + key);
    }
}
