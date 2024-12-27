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
        internal static UserInterface invUIF, filterUIF,reipceUIF;
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
            reipceUIF = new();
            reipceUIF.SetState(reipceUI);
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
            if (reipceUIF.IsVisible)
            {
                reipceUIF.Update(gameTime);
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
                       if (invUIF.IsVisible)
                       {
                           invUIF.Draw(Main.spriteBatch, gt);
                       }
                       if (filterUIF.IsVisible)
                       {
                           filterUIF.Draw(Main.spriteBatch, gt);
                       }
                       return true;
                   },
                   InterfaceScaleType.UI)
               );
            }
        }
    }
}
