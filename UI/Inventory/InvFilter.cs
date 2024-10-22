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

namespace LargerInventory.UI.Inventory
{
    public class InvFilter : UIState
    {
        public InvItemFilter currentFilter;
        public bool IsVisable;
        internal static HashSet<int> usedIcon;
        internal static bool load;
        private UIPanel bg;
        private List<UIItemFilter> filters;
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
            bg.SetSize(600, 400);
            bg.SetMargin(10, 10, 10, 10);
            Append(bg);

            UIPanel viewBg = new();
            viewBg.SetSize(-30, -40, 1, 1);
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
            view.SetSize(0, 0, 1, 1);
            viewBg.Append(view);

            UIScrollbar scroll = new();
            scroll.Top.Pixels += 6;
            scroll.Height.Set(-52, 1);
            scroll.Left.Set(-20, 1);
            view.SetScrollbar(scroll);
            bg.Append(scroll);
            #endregion

            #region 确认按钮
            UIPanel buttonBg = new()
            {
                HAlign = 0.3f,
            };
            buttonBg.SetSize(60, 30);
            buttonBg.Top.Set(-30, 1);
            bg.Append(buttonBg);

            UIText confirm = new("确认");
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
            buttonBg.OnLeftMouseDown += ApplyFilters;

            buttonBg = new()
            {
                HAlign = 0.7f,
            };
            buttonBg.SetSize(60, 30);
            buttonBg.Top.Set(-30, 1);
            bg.Append(buttonBg);

            UIText cancel = new("取消");
            cancel.Top.Pixels -= 5;
            buttonBg.Append(cancel);
            buttonBg.OnMouseOver += (evt, ls) =>
            {
                cancel.TextColor = Color.Gold;
            };
            buttonBg.OnMouseOut += (evt, ls) =>
            {
                cancel.TextColor = Color.White;
            };

            #endregion

            #region 要用的东西
            usedIcon = [];
            filters = [];
            Texture2D head = ModContent.Request<Texture2D>("Terraria/Images/UI/Creative/Infinite_Icons", AssetRequestMode.ImmediateLoad).Value;
            float x = 0, y = 0;

            bool TryAppend(UIItemFilter filter)
            {
                if (filter.IconItemID > 0)
                {
                    if (view.GetDimensions().Width - 20 < x + 39)
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
            AppendLeader(IsWeapon, "武器", 0, false, false);

            UICheckBoxText exact = new("完全匹配")
            {
                checkActive = true
            };
            exact.SetPos(x, y);
            view.Add(exact);
            x += exact.Width.Pixels + 10;


            UICheckBoxText countAs = new("计为同类");
            countAs.SetPos(x, y);
            view.Add(countAs);
            x += countAs.Width.Pixels + 10;

            UICheckBoxText effAllow = new("允许效果");
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
                var filter = new UIDamageClassFilter(DamageClassLoader.GetDamageClass(i));
                if (filter.DamageClass != DamageClass.Default)
                {
                    TryAppend(filter);
                }

            }
            #endregion

            #region 工具
            AppendLeader(IsTool, "工具", 6);
            AppendLine();

            TryAppend(new(IsAxe));
            TryAppend(new(IsHammer));
            TryAppend(new(IsPick));
            #endregion

            #region 装备
            AppendLeader(CanEquip, "装备", 2, false);

            UICheckBoxText vanity = new("时装");
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
                TryAppend(new UIEquipFilter(IsEquip(equips[i])));
            }
            #endregion

            #region 饰品
            AppendLeader(IsAccessory, "其他饰品", 9);
            AppendLine();
            TryAppend(new(IsLightPet));
            TryAppend(new(IsVanityPet));
            TryAppend(new(IsProjPet));
            TryAppend(new(IsHook));
            TryAppend(new(IsMount));
            TryAppend(new(IsMinecrat));
            TryAppend(new(IsDye));
            #endregion

            #region 消耗
            AppendLeader(IsConsumeable, "消耗", 3);
            AppendLine();
            TryAppend(new(IsHealthOrMana)
            {
                IconItemID = 3001
            });
            TryAppend(new(IsMedicament)
            {
                IconItemID = ItemID.FlaskofIchor
            });
            TryAppend(new(IsFood)
            {
                IconItemID = ItemID.SeafoodDinner
            });
            TryAppend(new(IsAmmo)
            {
                IconItemID = ItemID.AmmoBox
            });
            #endregion

            #region 放置
            AppendLeader(IsPlaceable, "放置", 4);
            AppendLine();
            TryAppend(new(IsPlaceableTile));
            TryAppend(new(IsPlaceableWall));
            TryAppend(new(IsFurniture));
            #endregion

            #region 其他
            x = 0;
            y += 70;
            UIText another = new("其他");
            another.SetPos(x, y + 10);
            view.Add(another);

            AppendLine();

            TryAppend(new(IsMaterial)
            {
                IconItemID = ItemID.Gel
            });
            TryAppend(new(IsCoin)
            {
                IconItemID = ItemID.PlatinumCoin
            });
            TryAppend(new(ExclusionAll)
            {
                OverrideTex = ModContent.Request<Texture2D>("Terraria/Images/UI/SearchCancel", AssetRequestMode.ImmediateLoad).Value
            });
            #endregion
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (IsMouseHovering)
            {
                PlayerInput.LockVanillaMouseScroll(GetType().FullName);
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        private void ApplyFilters(UIMouseEvent evt, UIElement listeningElement)
        {
            currentFilter = InvItemFilter.Combine(InvItemFilter.CombineType.AllTrue, null, [..from UIItemFilter uiFilter in filters where uiFilter.filterActive select uiFilter.Filter]);
            refreshToken.ThrowIfCancellationRequested();
            refreshToken = new();
            BackEnd.Inventory.StartRefreshTask(currentFilter, refreshToken, InvUI.Ins.Refresh);

            LISystem.filterUIF.IsVisible = false;
            LISystem.invUIF.IsVisible = true;
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
    }
}
