using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace LargerInventory.UI.ExtraUI
{
    public abstract class UIItemFilter : UIElement
    {
        public readonly Texture2D Tex;
        public virtual string Texture => "Assets/" + GetType().Name;
        public abstract Predicate<Item> Match { get; }
        public UIItemFilter()
        {
            Mod mod = LargerInventory.Ins;
            if (mod.HasAsset(Texture))
            {
                Tex = mod.Assets.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            }
        }
    }
}
