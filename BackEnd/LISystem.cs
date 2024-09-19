using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace LargerInventory.BackEnd
{
    public class LISystem : ModSystem
    {
        internal static InvUI invUI;
        internal static UserInterface uif;
        private GameTime gt;
        public override void Load()
        {
            invUI = new();
            invUI.Activate();
            uif = new();
            uif.SetState(invUI);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.LocalPlayer.controlInv && uif.IsVisible)
            {
                uif.IsVisible = false;
            }
            //invUI.Update(gameTime);
            uif.Update(gameTime);
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
                       uif.Draw(Main.spriteBatch, gt);
                       return true;
                   },
                   InterfaceScaleType.UI)
               );
            }
        }
    }
}
