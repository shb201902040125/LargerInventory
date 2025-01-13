using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace LargerInventory.UI.ExtraUI
{
    public class UICheckBoxText : UIElement
    {
        private static Texture2D buttonOn, buttonOff;
        public string Info;
        public bool checkActive;
        public UICheckBoxText(string info)
        {
            Info = info;
            buttonOn ??= ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonFavoriteActive", AssetRequestMode.ImmediateLoad).Value;
            buttonOff ??= ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonFavoriteInactive", AssetRequestMode.ImmediateLoad).Value;
            Vector2 size = FontAssets.MouseText.Value.MeasureString(Info);
            this.SetSize(size.X + 30, size.Y);
        }
        protected override void DrawSelf(SpriteBatch sb)
        {
            Texture2D button = checkActive ? buttonOn : buttonOff;
            Vector2 topLeft = GetDimensions().ToRectangle().TopLeft();
            sb.Draw(button, topLeft + new Vector2(15f), null, Color.White, 0, button.Size() / 2f, 1f, 0, 0);
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, Info,
                topLeft + new Vector2(30, 5), IsMouseHovering ? Color.Gold : Color.White, 0, Vector2.Zero, Vector2.One);
        }
    }
}
