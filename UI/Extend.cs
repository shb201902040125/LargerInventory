using Terraria.UI;

namespace LargerInventory.UI
{
    public static class Extend
    {
        public static UIElement SetSize(this UIElement uie, float w, float h, float wp = 0, float hp = 0)
        {
            uie.Width.Set(w, wp);
            uie.Height.Set(h, hp);
            return uie;
        }
        public static UIElement SetPos(this UIElement uie, float x, float y, float xp = 0, float yp = 0)
        {
            uie.Left.Set(x, xp);
            uie.Top.Set(y, yp);
            return uie;
        }
        public static UIElement SetMargin(this UIElement uie, float l = 0, float t = 0, float r = 0, float b = 0)
        {
            uie.MarginLeft = l;
            uie.MarginTop = t;
            uie.MarginRight = r;
            uie.MarginBottom = b;
            return uie;
        }
        public static UIElement SetPadding(this UIElement uie, float p)
        {
            uie.MarginLeft = p;
            uie.MarginTop = p;
            uie.MarginRight = p;
            uie.MarginBottom = p;
            return uie;
        }
    }
}
