using LargerInventory.BackEnd;
using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace LargerInventory.UI.ExtraUI.FIlters
{
    public class UIItemFilter : UIElement
    {
        public readonly InvItemFilter Filter, ParentFilter;
        public int IconItemID { get; init; }
        public Texture2D OverrideTex;
        public Rectangle? sourceRect;
        public bool filterActive;
        public bool Reversal;
        private static Texture2D Gold;
        private static List<Item> vnl, mods;
        protected virtual bool Match(Item item) => ParentFilter?.Check(item) != false && Filter.Check(item);
        public bool MatchItem(Item item) => Reversal ^ Match(item);
        public UIItemFilter(InvItemFilter filter, InvItemFilter parent = null)
        {
            Gold ??= LargerInventory.Ins.Assets.Request<Texture2D>("UI/Assets/Inventory_Gold", AssetRequestMode.ImmediateLoad).Value;
            this.SetSize(52, 52);
            Filter = filter;
            ParentFilter = parent;
            IconItemID = FindItemIcon();
            OnLeftMouseDown += (evt, ls) =>
            {
                if (filterActive)
                    filterActive = Reversal = false;
                else
                    filterActive = true;
            };
            OnRightMouseDown += (evt, ls) =>
            {
                if (!filterActive)
                    filterActive = true;
                Reversal = !Reversal;
            };
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            var local = GetDimensions();
            var rect = local.ToRectangle();
            sb.Draw(TextureAssets.InventoryBack16.Value, rect,
                (filterActive ? Reversal ? Color.Coral : Color.LightGreen : Color.White) * 0.75f);
            if (IsMouseHovering)
            {
                sb.Draw(Gold, rect, Color.White);
                if (IconItemID > 0)
                {
                    Main.hoverItemName += ContentSamples.ItemsByType[IconItemID].Name + "\n";
                }
            }
            var center = local.Center();
            if (OverrideTex != null)
            {
                sb.Draw(OverrideTex, center, sourceRect, Color.White, 0,
                    sourceRect?.Size() / 2f ?? OverrideTex.Size() / 2f, 1f, 0, 0);
                return;
            }
            if (IconItemID <= 0)
                return;
            float old = Main.inventoryScale;
            Main.inventoryScale = 0.75f;
            ItemSlot.DrawItemIcon(ContentSamples.ItemsByType[IconItemID], 0, sb,
               center, Main.inventoryScale, 52 * Main.inventoryScale, Color.White);
            Main.inventoryScale = old;
        }
        protected virtual int FindItemIcon()
        {
            if (vnl == null)
            {
                var items = ContentSamples.ItemsByType.Values.ToList();
                items.Sort((x, y) => y.damage.CompareTo(x.damage));
                vnl = [];
                mods = [];
                foreach (var item in items)
                {
                    (item.type < ItemID.Count ? vnl : mods).Add(item);
                }
            }
            int vanilla = vnl.FirstOrDefault(i => !InvFilter.usedIcon.Contains(i.type) && Match(i))?.type ?? 0;
            if (vanilla > 0)
            {
                return vanilla;
            }
            return mods.FirstOrDefault(i => !InvFilter.usedIcon.Contains(i.type) && Match(i))?.type ?? 0;
        }
    }
}
