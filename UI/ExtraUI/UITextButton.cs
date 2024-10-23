using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;

namespace LargerInventory.UI.ExtraUI
{
    public class UITextButton : UIPanel
    {
        public readonly UIText Text;
        public UITextButton(string text)
        {
            Text = new(text)
            {
                HAlign = 0.5f
            };
            Text.Top.Pixels -= 5;
            Append(Text);
            this.SetSize(Text.MinWidth.Pixels + 20, 30);
            OnMouseOver += (_, _) => Text.TextColor = Color.Gold;
            OnMouseOut += (_, _) => Text.TextColor = Color.White;
        }
    }
}
