using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace LargerInventory.BackEnd
{
    public class LISystem : ModSystem
    {
        internal static InvUI invUI;
        internal static InvFilter filterUI;
        internal static InvReipce reipceUI;
        internal static UserInterface invUIF, filterUIF,recipeUIF;
        private GameTime gt;
        public override void Load()
        {
            invUI = new();
            invUI.Activate();
            filterUI = new();
            filterUI.Activate();
            reipceUI = new();
            reipceUI.Activate();
            invUIF = new();
            invUIF.SetState(invUI);
            filterUIF = new();
            filterUIF.SetState(filterUI);
            recipeUIF = new();
            recipeUIF.SetState(reipceUI);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (invUIF.IsVisible)
            {
                invUIF.Update(gameTime);
            }
            if (filterUIF.IsVisible)
            {
                filterUIF.Update(gameTime);
            }
            if (recipeUIF.IsVisible)
            {
                recipeUIF.Update(gameTime);
            }
            gt = gameTime;
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (MouseTextIndex != -1)
            {
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
                   Mod.Name,
                   delegate
                   {
                       var sb = Main.spriteBatch;
                       if (invUIF.IsVisible)
                       {
                           invUIF.Draw(sb, gt);
                       }
                       if (filterUIF.IsVisible)
                       {
                           filterUIF.Draw(sb, gt);
                       }
                       if (recipeUIF.IsVisible)
                       {
                           recipeUIF.Draw(sb, gt);
                       }
                       return true;
                   },
                   InterfaceScaleType.UI)
               );
            }
        }
    }
}
