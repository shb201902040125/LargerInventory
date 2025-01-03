﻿using Microsoft.Xna.Framework;
using SML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LargerInventory.BackEnd
{
    internal class RecipeTask
    {
        public Recipe Recipe { get; }
        public RecipeTask(Recipe targetRecipe)
        {
            Recipe = targetRecipe;
        }
        public bool Update(Dictionary<int, List<Item>> inv)
        {
            if (Main.mouseItem.stack > 0 && !ItemLoader.CanStack(Main.mouseItem, Recipe.createItem))
            {
                return false;
            }
            Dictionary<int, Ref<int>> fakeMap = CreateFakeMap();
            HashSet<Ref<int>> checkMap = fakeMap.Values.ToHashSet();
            Dictionary<Item, int> consumed = [];
            foreach (int type in fakeMap.Keys)
            {
                if (fakeMap[type].Value == 0 || !inv.TryGetValue(type, out List<Item> container))
                {
                    continue;
                }
                for (int index = container.Count - 1; index >= 0; index--)
                {
                    Item item = container[index];
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
                    ItemLoader.OnConsumeItem(item, Main.LocalPlayer);
                }
                Item crafted = Recipe.createItem.Clone();
                crafted.Prefix(-1);
                AchievementsHelper.NotifyItemCraft(Recipe);
                AchievementsHelper.NotifyItemPickup(Main.player[Main.myPlayer], Recipe.createItem);
                if (Main.mouseItem.stack > 0)
                {
                    ItemLoader.StackItems(Main.mouseItem, crafted, out int num, false, null);
                }
                else
                {
                    Main.mouseItem = crafted;
                }
                ItemLoader.OnCreated(Main.mouseItem, new RecipeItemCreationContext(Recipe, [.. consumed.Keys], Main.mouseItem));
                Main.mouseItem.Center = Main.LocalPlayer.Center;
                PopupText.NewText(PopupTextContext.ItemCraft, Main.mouseItem, Recipe.createItem.stack, false, false);
                if (Main.mouseItem.type > ItemID.None || Recipe.createItem.type > ItemID.None)
                {
                    SoundEngine.PlaySound(SoundID.Grab with { Volume = 1, Pitch = 0 }, -Vector2.One);
                }
            }
            return false;
        }
        private Dictionary<int, Ref<int>> CreateFakeMap()
        {
            Dictionary<int, Ref<int>> fakeMap = [];
            foreach (Item item in Recipe.requiredItem)
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
            foreach (int groupID in Recipe.acceptedGroups)
            {
                RecipeGroup group = RecipeGroup.recipeGroups[groupID];
                int targetType = group.IconicItemId;
                groupItem.Add(groupID);
                foreach (int unit in group.ValidItems)
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
    internal class RecipeTaskTagSerializer : TagSerializer<RecipeTask, TagCompound>
    {
        public override RecipeTask Deserialize(TagCompound tag)
        {
            Recipe targetRecipe = null;
            try
            {
                Item createItem = tag.Get<Item>(nameof(Recipe.createItem));
                List<Item> requiredItem = tag.Get<List<Item>>(nameof(Recipe.requiredItem));
                List<int> groups = [];
                groups.AddRange(tag.Get<int[]>("trGroups"));
                groups.AddRange(from string gettext in tag.Get<string[]>("modGroups") select RecipeGroup.recipeGroupIDs[gettext]);
                string[] conditions = tag.Get<string[]>(nameof(Recipe.Conditions));
                HashSet<int> hashedGroupIDs = new(groups);
                HashSet<string> hashedConditions = new(conditions);
                for (int i = 0; i < Recipe.maxRecipes; i++)
                {
                    Recipe recipe = Main.recipe[i];
                    if (recipe is null)
                    {
                        goto Next;
                    }
                    if (recipe.createItem.type != createItem.type || recipe.createItem.stack != createItem.stack)
                    {
                        goto Next;
                    }
                    if (recipe.requiredItem.Count != requiredItem.Count)
                    {
                        goto Next;
                    }
                    for (int j = 0; j < recipe.requiredItem.Count; j++)
                    {
                        if (recipe.requiredItem[j].type != requiredItem[j].type || recipe.requiredItem[j].stack != recipe.requiredItem[j].stack)
                        {
                            goto Next;
                        }
                    }
                    if (recipe.acceptedGroups.Count != hashedGroupIDs.Count || recipe.acceptedGroups.Any(id => !hashedGroupIDs.Contains(id)))
                    {
                        continue;
                    }
                    if (recipe.Conditions.Count != conditions.Length || recipe.Conditions.Any(condition => !hashedConditions.Contains(condition.Description.Key)))
                    {
                        goto Next;
                    }
                    targetRecipe = recipe;
                    break;
                Next:;
                }
            }
            catch
            {
                targetRecipe = null;
            }
            return new RecipeTask(targetRecipe);
        }
        public override TagCompound Serialize(RecipeTask value)
        {
            Recipe recipe = value.Recipe;
            TagCompound tag = new()
            {
                [nameof(Recipe.createItem)] = recipe.createItem,
                [nameof(Recipe.requiredItem)] = recipe.requiredItem,
                ["trGroup"] = (from int id in recipe.acceptedGroups where id < 26 select id).ToArray(),
                ["modGroup"] = (from int id in recipe.acceptedGroups where id > 25 select RecipeGroup.recipeGroups[id].GetText).ToArray(),
                [nameof(Recipe.Conditions)] = (from Condition condition in recipe.Conditions select condition.Description.Key).ToArray()
            };
            return tag;
        }
    }
}
