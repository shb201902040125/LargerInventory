using Microsoft.Xna.Framework;
using SML.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LargerInventory.BackEnd
{
    public class RecipeTask
    {
        public enum TaskType
        {
            Until,
            Keep,
            Always
        }
        public Recipe Recipe { get; }
        public int Count { get;internal set; }
        public TaskType Type { get; internal set; }
        public bool Notify { get;internal set;}
        public bool PutIntoVanilla {  get; internal set;}
        public bool IgnoreFavorite {  get; internal set;}
        internal Dictionary<int, HashSet<int>> _ignoreInRecipeGroup = [];
        public RecipeTask(Recipe targetRecipe, int targetCount = 1, TaskType taskType = TaskType.Until, bool notify = false, bool putIntoVanilla = false, bool ignoreFavorite = true)
        {
            Recipe = targetRecipe;
            Count = targetCount;
            Type = taskType;
            Notify = notify;
            PutIntoVanilla = putIntoVanilla;
            IgnoreFavorite = ignoreFavorite;
        }
        public bool Update(Dictionary<int, List<Item>> inv, InvToken.Token token)
        {
            if (!token.InValid)
            {
                return false;
            }
            if ((Type == TaskType.Until && Count <= 0) || (Type == TaskType.Keep && Inventory.GetItemCount(token, Recipe.createItem.type) >= Count))
            {
                return false;
            }
            Dictionary<int, Ref<int>> fakeMap = CreateFakeMap();
            HashSet<Ref<int>> checkMap = [.. fakeMap.Values];
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
                    if (IgnoreFavorite && item.favorited)
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
                if (PutIntoVanilla)
                {
                    Main.LocalPlayer.QuickSpawnItem(null, crafted);
                }
                else
                {
                    Inventory.PushItem(token, crafted, out _);
                }
                ItemLoader.OnCreated(Main.mouseItem, new RecipeItemCreationContext(Recipe, [.. consumed.Keys], Main.mouseItem));
                PopupText.NewText(PopupTextContext.ItemCraft, Main.mouseItem, Recipe.createItem.stack, false, false);
                SoundEngine.PlaySound(SoundID.Grab with { Volume = 1, Pitch = 0 }, -Vector2.One);
                if (Type == TaskType.Until)
                {
                    if (Count-- == 0)
                    {
                        return true;
                    }
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
                    if (_ignoreInRecipeGroup.TryGetValue(groupID, out var hashSet) && hashSet.Contains(unit))
                    {
                        continue;
                    }
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
        public bool SetIgnoreInRecipeGroup(int groupID, int type)
        {
            if (RecipeGroup.recipeGroups.TryGetValue(groupID, out var group) && group.ContainsItem(type))
            {
                if (type == group.IconicItemId)
                {
                    return false;
                }
                if (!_ignoreInRecipeGroup.TryGetValue(groupID, out var hashSet))
                {
                    hashSet = _ignoreInRecipeGroup[groupID] = [];
                }
                hashSet.Add(type);
                return true;
            }
            return false;
        }
    }
    public class RecipeTaskTagSerializer : TagSerializer<RecipeTask, TagCompound>
    {
        public override RecipeTask Deserialize(TagCompound tag)
        {
            Recipe targetRecipe = null;
            int count = 0;
            RecipeTask.TaskType taskType = RecipeTask.TaskType.Until;
            bool notify = false;
            bool putIntoVanilla = false;
            bool ignoreFavorite = true;
            RecipeTask res = null;
            try
            {
                Item createItem = tag.Get<Item>(nameof(Recipe.createItem));
                List<Item> requiredItem = tag.Get<List<Item>>(nameof(Recipe.requiredItem));
                count=tag.Get<int>(nameof(RecipeTask.Count));
                notify=tag.Get<bool>(nameof(RecipeTask.Notify));
                putIntoVanilla = tag.Get<bool>(nameof(RecipeTask.PutIntoVanilla));
                ignoreFavorite=tag.Get<bool>(nameof(RecipeTask.IgnoreFavorite));
                taskType = Enum.Parse<RecipeTask.TaskType>(tag.Get<string>(nameof(RecipeTask.Type)));
                List<int> groups = [];
                groups.AddRange(tag.Get<int[]>("trGroups"));
                groups.AddRange(from string name in tag.Get<string[]>("modGroups") select RecipeGroup.recipeGroupIDs[name]);
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
                res = new RecipeTask(targetRecipe, count, taskType, notify, putIntoVanilla, ignoreFavorite);
                List<string> ignoreRecipeGroup = tag.Get<List<string>>("IgnoreRecipeGroup");
                foreach (var content in ignoreRecipeGroup)
                {
                    string[] sub = content.Split(' ');
                    if (sub.Length != 2)
                    {
                        continue;
                    }
                    int groupID = -1;
                    if (int.TryParse(sub[0], out int id) && id < 26)
                    {
                        groupID = id;
                    }
                    else
                    {
                        if (RecipeGroup.recipeGroupIDs.TryGetValue(sub[0], out groupID))
                        {
                            string[] ids = sub[1].Split(",");
                            foreach (var idString in ids)
                            {
                                if(int.TryParse(idString, out int id2))
                                {
                                    res.SetIgnoreInRecipeGroup(groupID, id2);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                targetRecipe = null;
                res = null;
            }
            return res;
        }
        public override TagCompound Serialize(RecipeTask value)
        {
            Recipe recipe = value.Recipe;
            TagCompound tag = new()
            {
                [nameof(Recipe.createItem)] = recipe.createItem,
                [nameof(Recipe.requiredItem)] = recipe.requiredItem,
                [nameof(RecipeTask.Count)] = value.Count,
                [nameof(RecipeTask.Type)] = Enum.GetName(value.Type),
                [nameof(RecipeTask.Notify)] = value.Notify,
                [nameof(RecipeTask.PutIntoVanilla)] = value.PutIntoVanilla,
                [nameof(RecipeTask.IgnoreFavorite)] = value.IgnoreFavorite,
                ["trGroup"] = (from int id in recipe.acceptedGroups where id < 26 select id).ToArray(),
                ["modGroup"] = (from int id in recipe.acceptedGroups where id > 25 select FindGroupName( RecipeGroup.recipeGroups[id])).ToArray(),
                [nameof(Recipe.Conditions)] = (from Condition condition in recipe.Conditions select condition.Description.Key).ToArray()
            };
            List<string> ignoreRecipeGroup = [];
            foreach (var pair in value._ignoreInRecipeGroup)
            {
                StringBuilder sb = new();
                if (pair.Key < 26)
                {
                    sb.Append(pair.Key);
                }
                else
                {
                    sb.Append(FindGroupName(RecipeGroup.recipeGroups[pair.Key]));
                }
                sb.Append(' ');
                sb.Append(string.Join(",", pair.Value));
                ignoreRecipeGroup.Add(sb.ToString());
            }
            tag["IgnoreRecipeGroup"] = ignoreRecipeGroup;
            return tag;
        }
        static string FindGroupName(RecipeGroup recipeGroup)
        {
            string res=string.Empty;
            foreach(var pair in RecipeGroup.recipeGroupIDs)
            {
                if(pair.Value==recipeGroup.RegisteredId)
                {
                    res = pair.Key;
                    break;
                }
            }
            if(res == string.Empty)
            {
                throw new ArgumentException();
            }
            return res;
        }
    }
}
