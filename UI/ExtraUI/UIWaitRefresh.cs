using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;

namespace LargerInventory.UI.ExtraUI
{
    public class UIWaitRefresh(string text) : UIText(text)
    {
        public bool hide;
        public override void OnActivate()
        {
            hide = false;
        }
        public override void OnDeactivate()
        {
            hide = false;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (hide)
                return;
            base.Draw(spriteBatch);
        }
    }
}
