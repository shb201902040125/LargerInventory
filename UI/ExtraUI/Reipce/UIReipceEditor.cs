using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace LargerInventory.UI.ExtraUI.Reipce
{
    public class UIReipceEditor : UIState
    {
        public bool visiable;
        public UIRecipeTask EditingRT { get; private set; }
        private List<UICheckBoxText> group;
        private UIView accepts;
        private bool loaded;
        public override void OnInitialize()
        {
            UIPanel bg = new() { VAlign = 0.5f };
            bg.SetSize(300, 400);
            bg.SetPos(200, 0, 0);
            bg.SetPadding(5);
            Append(bg);

            int x = 0, y = 0;
            UICheckBoxText until = new(UIGTV("Until"));
            until.OnLeftMouseDown += (_, _) =>
            {
                until.checkActive = !until.checkActive;
                EditingRT.mode = 0;
                group[1].checkActive = false;
                group[2].checkActive = false;
            };
            until.SetPos(x, y);
            bg.Append(until);
            y += 30;

            UICheckBoxText keep = new(UIGTV("Keep"));
            keep.OnLeftMouseDown += (_, _) =>
            {
                keep.checkActive = !keep.checkActive;
                EditingRT.mode = 0;
                group[0].checkActive = false;
                group[2].checkActive = false;
            };
            keep.SetPos(x, y);
            bg.Append(keep);
            y += 30;

            UICheckBoxText always = new(UIGTV("Always"));
            always.OnLeftMouseDown += (_, _) =>
            {
                always.checkActive = !always.checkActive;
                EditingRT.mode = 0;
                group[0].checkActive = false;
                group[1].checkActive = false;
            };
            always.SetPos(x, y);
            bg.Append(always);
            y += 30;


            UICheckBoxText notify = new(UIGTV("Notify"));
            notify.OnLeftMouseDown += (_, _) =>
            {
                EditingRT.Notify = notify.checkActive = !notify.checkActive;
            };
            notify.SetPos(x, y);
            bg.Append(notify);
            y += 30;

            UICheckBoxText putvanilla = new(UIGTV("PutIntoVanilla"));
            putvanilla.OnLeftMouseDown += (_, _) =>
            {
                EditingRT.PutIntoVanilla = putvanilla.checkActive = !putvanilla.checkActive;
            };
            putvanilla.SetPos(x, y);
            bg.Append(putvanilla);
            y += 30;

            UICheckBoxText ignore = new(UIGTV("IgnoreFavorite"));
            ignore.OnLeftMouseDown += (_, _) =>
            {
                EditingRT.IgnoreFavorite = ignore.checkActive = !ignore.checkActive;
            };
            ignore.SetPos(x, y);
            bg.Append(ignore);
            y += 40;

            UIText text = new(UIGTV("RecipeGroup"));
            text.SetPos(x, y);
            bg.Append(text);
            y += 30;

            UIPanel rgbg = new();
            rgbg.SetPos(0, y);
            rgbg.SetSize(0, -y, 1, 1);
            rgbg.SetPadding(5);
            bg.Append(rgbg);

            UIView view = [];
            view.SetSize(-40, 0, 1, 1);
            view.SetPadding(5);
            view.needRepos = false;
            rgbg.Append(view);
            accepts = view;

            UIScrollbar scroll = new();
            scroll.SetSize(20,-20, 0, 1);
            scroll.SetPos(-20,  10, 1);
            rgbg.Append(scroll);
            view.SetScrollbar(scroll);

            group = [until, keep, always, notify, putvanilla, ignore];
        }
        private static string UIGTV(string key) => MiscHelper.GTV("UI.Recipe." + key);
        public void OpenEditor(UIRecipeTask rt)
        {
            Load();
            EditingRT = rt;
            for (int i = 0; i < 3; i++)
            {
                group[i].checkActive = rt.mode == i;
            }
            group[3].checkActive = rt.Notify;
            group[4].checkActive = rt.PutIntoVanilla;
            group[5].checkActive = rt.IgnoreFavorite;
            accepts.Clear();
            int x = 0, y = 0, w = (int)accepts.GetInnerDimensions().Width;
            bool first = true, needSpaceing = false;
            foreach (var (index, rg) in rt.recipeGroups)
            {
                var localIndex = index;
                UIText name = new(RecipeGroup.recipeGroups[index].GetText.Invoke());
                if (first)
                {
                    first = false;
                }
                else if (needSpaceing)
                {
                    x = 0;
                    y += 72;
                }
                name.SetPos(x, y);
                y += 30;
                accepts.Add(name);

                foreach (var (id, locked) in rg)
                {
                    UIRGSlot slot = new(ContentSamples.ItemsByType[id], locked);
                    int type = id;
                    slot.OnLeftMouseDown += (_, _) =>
                    {
                        EditingRT.recipeGroups[localIndex][type] = slot.locked = !slot.locked;
                    };
                    slot.SetPos(x, y);
                    accepts.Add(slot);
                    x += 57;
                    needSpaceing = true;
                    if (x + 52 > w)
                    {
                        x = 0;
                        y += 57;
                        needSpaceing = false;
                    }
                }
            }
            accepts.RecalculateChildren();
        }

        private void Load()
        {
            if (loaded)
                return;
            loaded = true;
            RemoveAllChildren();
            OnInitialize();
        }
    }
}
