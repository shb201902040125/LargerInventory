using SML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace LargerInventory.BackEnd
{
    internal class RecipeTask : GameEvent<RecipeTask, Item>
    {
        public Recipe Target { get; private set; }
        private Dictionary<int, int> quickMap = [];
        public RecipeTask(Recipe recipe)
        {
            Target = recipe;
            onComplete += delegate (RecipeTask recipeTask)
            {
                recipeTask.SetTarget(recipeTask.Target);
            };
        }
        public override bool Update(Item item)
        {
            int type = item.type;
            foreach (var group in Target.acceptedGroups)
            {
                var recipeGroup = RecipeGroup.recipeGroups[type];
                if (recipeGroup.ContainsItem(type))
                {
                    type = recipeGroup.IconicItemId;
                }
            }
            if (quickMap.TryGetValue(type, out var stack) && stack > 0)
            {
                int move = Math.Min(stack, item.stack);
                quickMap[type] = stack - move;
                item.stack -= move;
                ItemLoader.OnConsumeItem(item, Main.LocalPlayer);
                onUpdate?.Invoke(this, item);
                if (quickMap.All(pair => pair.value == 0))
                {
                    IsCompleted = true;
                    onComplete?.Invoke(this);
                }
                return true;
            }
            return false;
        }
        public void SetTarget(Recipe target)
        {
            Target = target;
            quickMap.Clear();
            foreach(var required in target.requiredItem)
            {
                if (quickMap.ContainsKey(required.type))
                {
                    quickMap[required.type] += required.stack;
                }
                else
                {
                    quickMap[required.type] = required.stack;
                }
            }
            IsCompleted = false;
        }
        public override void BindHandler(GameEventHandler<RecipeTask, Item> handler)
        {
            onComplete += delegate (RecipeTask recipeTask)
            {
                handler.Update(recipeTask);
            };
        }
    }
    internal class RecipeTaskFinish : GameEventHandler<RecipeTask, Item>
    {
        public override bool Handle(RecipeTask @event)
        {
            if(!@event.IsCompleted)
            {
                return false;
            }
            Main.LocalPlayer.QuickSpawnItem(default, @event.Target.createItem.type, @event.Target.createItem.stack);
            return true;
        }
    }
}
