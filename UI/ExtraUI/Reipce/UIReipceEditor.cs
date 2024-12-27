using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;

namespace LargerInventory.UI.ExtraUI.Reipce
{
    public class UIReipceEditor : UIPanel
    {
        public bool visiable;
        public UIRecipeTask rt;
        private List<UICheckBoxText> group;
        public UIReipceEditor()
        {
            int x = 0, y = 0;
            UICheckBoxText until = new(UIGTV("Until"));
            until.SetPos(x, y);
            Append(until);
            y += 40;

            UICheckBoxText keep = new(UIGTV("Keep"));
            keep.SetPos(x, y);
            Append(keep);
            y += 40;

            UICheckBoxText always = new(UIGTV("Always"));
            always.SetPos(x, y);
            Append(always);

            group = [until, keep, always];

            UICheckBoxText notify = new(UIGTV("Notify"));
            notify.SetPos(x, y);
            Append(notify);

            UICheckBoxText putvanilla = new(UIGTV("PutIntoVanilla"));
            putvanilla.SetPos(x, y);
            Append(putvanilla);

            UICheckBoxText ignore = new(UIGTV("IgnoreFavorite"));
            ignore.SetPos(x, y);
            Append(ignore);

            UIView view = new()
            {
                Top = new(y, 0),
                Width = new(0, 1),
                Height = new(-y, 1)
            };
            Append(view);
        }
        private static string UIGTV(string key) => MiscHelper.GTV("UI.Recipe." + key);
    }
}
