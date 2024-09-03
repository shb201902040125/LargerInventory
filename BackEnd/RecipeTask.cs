using SML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace LargerInventory.BackEnd
{
    internal class RecipeTask : GameEvent<RecipeTask, Dictionary<int, List<Item>>>
    {
        Recipe _targetRecipe;
        public override bool Update(Dictionary<int, List<Item>> inv)
        {
            if (Main.mouseItem.stack > 0 && !ItemLoader.CanStack(Main.mouseItem, _targetRecipe.createItem))
            {
                return false;
            }
            var fakeMap = CreateFakeMap();
            var checkMap = fakeMap.Values.ToHashSet();
            Dictionary<Item, int> consumed = [];
            foreach (var type in fakeMap.Keys)
            {
                if (fakeMap[type].Value == 0 || !inv.TryGetValue(type, out var container))
                {
                    continue;
                }
                for (int index = container.Count - 1; index >= 0; index--)
                {
                    var item = container[index];
                    if (item.favorited)
                    {
                        continue;
                    }
                    int move = Math.Min(item.stack, fakeMap[type].Value);
                    if (move > 0)
                    {
                        consumed[item] = move;
                        fakeMap[type].Value -= move;
                        if (fakeMap[type].Value == 0)
                        {
                            break;
                        }
                    }
                }
            }
            if (checkMap.All(check => check.Value == 0))
            {
                foreach ((Item item, int consumedCount) in consumed)
                {
                    item.stack -= consumedCount;
                }
                Item crafted = _targetRecipe.createItem.Clone();
                crafted.Prefix(-1);
                AchievementsHelper.NotifyItemCraft(_targetRecipe);
                AchievementsHelper.NotifyItemPickup(Main.player[Main.myPlayer], _targetRecipe.createItem);
                if (Main.mouseItem.stack > 0)
                {
                    ItemLoader.StackItems(Main.mouseItem, crafted, out int num, false, null);
                }
                else
                {
                    Main.mouseItem = crafted;
                }
                Main.mouseItem.Center = Main.LocalPlayer.Center;
                PopupText.NewText(PopupTextContext.ItemCraft, Main.mouseItem, _targetRecipe.createItem.stack, false, false);
                if (Main.mouseItem.type > ItemID.None || _targetRecipe.createItem.type > ItemID.None)
                {
                    SoundEngine.PlaySound(SoundID.Grab with { Volume = 1, Pitch = 0 }, -Vector2.One);
                }
            }
            return false;
        }
        private Dictionary<int, Ref<int>> CreateFakeMap()
        {
            Dictionary<int, Ref<int>> fakeMap = [];
            foreach (var item in _targetRecipe.requiredItem)
            {
                if (fakeMap.TryGetValue(item.type, out Ref<int> required))
                {
                    required.Value += item.stack;
                }
                else
                {
                    fakeMap[item.type] = new Ref<int>(item.stack);
                }
            }

            HashSet<int> groupItem = [];
            foreach (var groupID in _targetRecipe.acceptedGroups)
            {
                var group = RecipeGroup.recipeGroups[groupID];
                var targetType = group.IconicItemId;
                groupItem.Add(groupID);
                foreach (var unit in group.ValidItems)
                {
                    if (!fakeMap.ContainsKey(unit))
                    {
                        fakeMap[unit] = fakeMap[targetType];
                    }
                }
            }
            return fakeMap.OrderBy(kvp => !groupItem.Contains(kvp.Key))
                         .ThenBy(kvp => kvp.Key)
                         .ToDictionary();
        }
    }
}
