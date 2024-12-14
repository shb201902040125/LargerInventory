using LargerInventory.UI.ExtraUI.FIlters;
using System.Collections.Generic;
using System.Threading;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria;
using Terraria.UI;

namespace LargerInventory.UI.Inventory
{
    public class InvReipce : UIState
    {
        public bool IsVisable;
        internal static bool load;
        private UIPanel bg;
        private const string UIKey = "UI.Recipe.";
        public override void OnInitialize()
        {
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
        }
    }
}