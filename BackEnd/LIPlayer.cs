﻿using LargerInventory.UI.Inventory;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LargerInventory.BackEnd
{
    internal class LIPlayer : ModPlayer
    {
        internal static ModKeybind SwitchInv;
        public override void Load()
        {
            SwitchInv = KeybindLoader.RegisterKeybind(Mod, "SwitchInv", Keys.C);
            On_Player.BuyItem += On_Player_BuyItem;
        }
        public override void Unload()
        {
            On_Player.BuyItem -= On_Player_BuyItem;
        }
        private bool On_Player_BuyItem(On_Player.orig_BuyItem orig, Player self, long price, int customCurrency)
        {
            return Inventory.BuyItem(self, price, customCurrency, true);
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (SwitchInv.JustPressed)
            {
                InvUI.Ins.OnInitialize();
                if (!InvToken.TryGetToken(out InvToken.Token token))
                {
                    return;
                }
                /*if (Inventory.GetCount(token) == 0)
                {
                    for (int i = 1; i < ItemID.Count; i++)
                    {
                        Item item = new(i);
                        item.stack = Main.rand.Next(1, item.maxStack);
                        Inventory.PushItem(token, item, out _);
                    }
                }*/
                InvUI.Ins.CallRefresh();
                if (!LISystem.filterUIF.IsVisible)
                {
                    LISystem.invUIF.IsVisible = !LISystem.invUIF.IsVisible;
                    LISystem.recipeUIF.IsVisible = LISystem.editorUIF.IsVisible = false;
                    if (!LISystem.invUIF.IsVisible)
                    {
                        Inventory.ClearAllEmptyItems(token);
                    }
                }
                token.Return();
            }
        }
        public override void PostUpdate()
        {
            if (InvToken.TryGetToken(out InvToken.Token token))
            {
                Inventory.TryHealLife(token, Player);
                Inventory.TryHealMana(token, Player);
                Inventory.UpdateRecipeTasks(token);
                token.Return();
            }
        }
        public override void SaveData(TagCompound tag)
        {
            Inventory.Save(tag);
        }
        public override void LoadData(TagCompound tag)
        {
            Inventory.Load(tag);
        }
    }
}
