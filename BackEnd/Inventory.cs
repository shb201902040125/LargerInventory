using ReLogic.Threading;
using SML.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LargerInventory.BackEnd
{
    public static class Inventory
    {
        private static Dictionary<int, List<Item>> _items = [];
        private static NormalCache _cache = new();
        private static Item _fakeItem;

        private const string CacheKey_CachedType = "cachedType";
        private const string CacheKey_HealLifeData = "healLifeData";
        private const string CacheKey_HealManaData = "healManaData";

        public static int Count => _items.Values.Sum(items => items.Count);

        private static void SplitItem(Item item, List<Item> container)
        {
            while (item.stack > item.maxStack)
            {
                Item copy = item.Clone();
                copy.stack = Math.Min(copy.maxStack, item.stack);
                item.stack -= copy.stack;
                container.Add(copy);
            }
            container.Add(item);
        }
        private static List<Item> CompressItems(List<Item> items)
        {
            List<Item> resultItems = [];
            items = [.. items.OrderByDescending(item => item.maxStack)];
            foreach (Item item in items)
            {
                bool addedToExisting = false;
                foreach (Item resultItem in resultItems)
                {
                    if (ItemLoader.CanStack(resultItem, item))
                    {
                        int spaceAvailable = resultItem.maxStack - resultItem.stack;
                        int toTransfer = Math.Min(spaceAvailable, item.stack);

                        resultItem.stack += toTransfer;
                        item.stack -= toTransfer;

                        if (item.stack <= 0)
                        {
                            addedToExisting = true;
                            break;
                        }
                    }
                }
                if (!addedToExisting && item.stack > 0)
                {
                    resultItems.Add(item);
                }
            }
            List<Item> res = [], splitedItems = [];
            foreach (Item item in resultItems)
            {
                if (item.stack == 0)
                {
                    continue;
                }
                if (item.stack > item.maxStack)
                {
                    splitedItems.Clear();
                    SplitItem(item, splitedItems);
                    res.AddRange(splitedItems);
                }
                else
                {
                    res.Add(item);
                }
            }
            return res;
        }

        private static T GetOrCreateCache<T>(string key) where T : class
        {
            if (!_cache.TryGet(key, out T cacheData))
            {
                _cache.Add(key, cacheData = Activator.CreateInstance<T>());
            }
            return cacheData;
        }
        private static void WriteCache(int type)
        {
            HashSet<int> cachedType = GetOrCreateCache<HashSet<int>>(CacheKey_CachedType);
            if (!cachedType.Add(type))
            {
                return;
            }
            Item item = ContentSamples.ItemsByType[type];
            if (item.potion && item.healLife > 0)
            {
                Dictionary<Item, int> healLife = GetOrCreateCache<Dictionary<Item, int>>(CacheKey_HealLifeData);
                healLife[item] = item.healLife;
            }
            if (item.healMana > 0 && !item.potion)
            {
                Dictionary<Item, int> healMana = GetOrCreateCache<Dictionary<Item, int>>(CacheKey_HealManaData);
                healMana[item] = item.healMana;
            }
        }
        private static void CompressItemList(int type)
        {
            if (_items.TryGetValue(type, out List<Item> container))
            {
                CompressItems(container);
            }
        }
        private static void CompressAllItems()
        {
            foreach (List<Item> items in _items.Values)
            {
                CompressItems(items);
            }
        }
        private static KeyValuePair<Item, int> FindBestMatch(Dictionary<Item, int> data, int target)
        {
            return data.OrderBy(kvp => Math.Abs(kvp.Value - target))
                .ThenBy(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key.value)
                .ThenBy(kvp => kvp.Key.type)
                .First();
        }
        public static void TryHealLife(Player player)
        {
            if (player.potionDelay > 0)
            {
                return;
            }
            float rate = player.statLife / (float)player.statLifeMax2;
            if (rate > LIConfigs.Instance.AutoUseLifePotion)
            {
                return;
            }
            int cure = player.statLifeMax2 - player.statLife;
            KeyValuePair<Item, int> bestMatch = FindBestMatch(GetOrCreateCache<Dictionary<Item, int>>(CacheKey_HealLifeData), cure);
            _fakeItem ??= new();
            _fakeItem.SetDefaults(bestMatch.Key.type);
            PickItem(_fakeItem, 1);
            player.ApplyLifeAndOrMana(_fakeItem);
        }
        public static void TryHealMana(Player player)
        {
            if (player.potionDelay > 0)
            {
                return;
            }
            float rate = player.statMana / (float)player.statManaMax2;
            if (rate > LIConfigs.Instance.AutoUseManaPotion)
            {
                return;
            }
            int cure = player.statManaMax2 - player.statMana;
            KeyValuePair<Item, int> bestMatch = FindBestMatch(GetOrCreateCache<Dictionary<Item, int>>(CacheKey_HealManaData), cure);
            _fakeItem ??= new();
            _fakeItem.SetDefaults(bestMatch.Key.type);
            PickItem(_fakeItem, 1);
            player.ApplyLifeAndOrMana(_fakeItem);
        }

        private static void SureItemType(int type, Item item, bool keepStack = false, bool keepPrefix = false, bool keepFavorited = false, bool keepNewAndShiny = false)
        {
            if (item.type != type)
            {
                var cacheStack = item.stack;
                var cachePrefix = item.prefix;
                var cacheFavorited = item.favorited;
                var cacheNewAndShiny = item.newAndShiny;
                item.SetDefaults(type);
                if (keepStack)
                {
                    item.stack = Math.Min(cacheStack, item.maxStack);
                }
                if (keepPrefix)
                {
                    item.Prefix(cachePrefix);
                }
                if (keepFavorited)
                {
                    item.favorited = cacheFavorited;
                }
                if (keepNewAndShiny)
                {
                    item.newAndShiny = cacheNewAndShiny;
                }
            }
        }
        public static void PushItem(Item item, out bool refresh)
        {
            refresh = false;
            if (!_items.TryGetValue(item.type, out List<Item> container))
            {
                _items[item.type] = container = [];
            }
            int count = container.Count;
            foreach (Item target in container)
            {
                SureItemType(item.type, target);
                if (ItemLoader.CanStack(target, item))
                {
                    int move = Math.Min(target.maxStack - target.stack, item.stack);
                    if (move == 0)
                    {
                        continue;
                    }
                    ItemLoader.OnStack(target, item, move);
                    target.stack += move;
                    item.stack -= move;
                    if (item.stack == 0)
                    {
                        break;
                    }
                }
            }
            if (item.stack > 0)
            {
                List<Item> splitedItems = [];
                SplitItem(item, splitedItems);
                container.AddRange(splitedItems);
            }
            refresh = count != container.Count;
            WriteCache(item.type);
        }
        public static int PushItemToEnd(Item item, bool splitIfOverflow = true)
        {
            if (!_items.TryGetValue(item.type, out List<Item> container))
            {
                _items[item.type] = container = [];
            }
            int result = container.Count;
            if (splitIfOverflow && item.stack > item.maxStack)
            {
                List<Item> splitedItems = [];
                SplitItem(item, splitedItems);
                container.AddRange(splitedItems);
            }
            else
            {
                container.Add(item);
            }
            WriteCache(item.type);
            return result;
        }
        public static int PushItemToFirstEmptySlot(Item item, bool splitIfOverflow = true)
        {
            if (!_items.TryGetValue(item.type, out List<Item> container))
            {
                _items[item.type] = container = [];
            }
            int index = -1;
            for (int i = 0; i < container.Count; i++)
            {
                if(container[i].IsAir)
                {
                    index = i; 
                    break;
                }
            }
            int result;
            if (index == -1)
            {
                result = container.Count;
                if (splitIfOverflow && item.stack > item.maxStack)
                {
                    List<Item> splitedItems = [];
                    SplitItem(item, splitedItems);
                    container.AddRange(splitedItems);
                }
                else
                {
                    container.Add(item);
                }
                WriteCache(item.type);
                return result;
            }
            result = index;
            if (splitIfOverflow && item.stack > item.maxStack)
            {
                List<Item> splitedItems = [];
                SplitItem(item, splitedItems);
                container[index] = splitedItems[0];
                container.AddRange(splitedItems[1..]);
            }
            else
            {
                container[index] = item;
            }
            WriteCache(item.type);
            return result;
        }
        public static int PutItemToDesignatedIndex(Item item, int index)
        {
            if (!_items.TryGetValue(item.type, out List<Item> container) || !container.IndexInRange(index))
            {
                return -1;
            }
            Item target = container[index];
            SureItemType(item.type, target);
            if (!ItemLoader.CanStack(target, item))
            {
                return -1;
            }
            int move = Math.Min(target.maxStack - target.stack, item.stack);
            if (move == 0)
            {
                return 0;
            }
            ItemLoader.OnStack(target, item, move);
            item.stack -= move;
            target.stack += move;
            WriteCache(item.type);
            return move;
        }
        public static int PickItem(Item item, int count)
        {
            if (!_items.TryGetValue(item.type, out List<Item> container))
            {
                return 0;
            }
            count = Math.Min(count, item.maxStack - item.stack);
            int moved = 0;
            foreach (Item target in container)
            {
                SureItemType(item.type, target);
                int move = Math.Min(target.stack, count);
                if (move == 0)
                {
                    continue;
                }
                ItemLoader.OnStack(item, target, move);
                target.stack -= move;
                moved += move;
                count -= move;
                if (count == 0)
                {
                    break;
                }
            }
            return moved;
        }
        public static int PickItemFromDesignatedIndex(Item item, int index, int count)
        {
            if (!_items.TryGetValue(item.type, out List<Item> container) || !container.IndexInRange(index))
            {
                return 0;
            }
            count = Math.Min(count, item.maxStack - item.stack);
            Item target = container[index];
            SureItemType(item.type, target);
            int move = Math.Min(target.stack, count);
            if (move == 0)
            {
                return 0;
            }
            target.stack -= move;
            item.stack += move;
            ItemLoader.OnStack(item, target, move);
            return move;
        }
        public static bool PopItems(int type, int index, [NotNullWhen(true)] out Item item)
        {
            item = null;
            if (!_items.TryGetValue(type, out List<Item> container) || container.IndexInRange(index))
            {
                return false;
            }
            item = container[index];
            container.RemoveAt(index);
            return true;
        }

        public static Dictionary<int, List<InfoForUI>> GetInfoForUIs(params Predicate<Item>[] predicates)
        {
            predicates ??= [DefaultPredicate_GetInfoForUIs];
            Dictionary<int, List<InfoForUI>> result = [];
            foreach(var type in _items.Keys)
            {
                var list = result[type] = [];
                for(int i = 0; i < _items[type].Count;i++)
                {
                    var item = _items[type][i];
                    if (predicates.All(p => p?.Invoke(item) ?? true))
                    {
                        list.Add(new InfoForUI(type, i, item));
                    }
                }
            };
            return result;
        }
        private static bool DefaultPredicate_GetInfoForUIs(Item item)
        {
            return true;
        }
        public class InfoForUI
        {
            public InfoForUI(int type, int index, Item item)
            {
                Type = type;
                Index = index;
                Item = item;
            }
            internal int Type { get; private set; }
            internal int Index { get; private set; }
            internal Item Item { get; private set; }
            internal void Changed(Item newItem)
            {
                if (!_items.TryGetValue(Type, out var list) || !list.IndexInRange(Index))
                {
                    Type = Item.type;
                    Index = PushItemToFirstEmptySlot(newItem);
                    Item = newItem;
                    return;
                }
                if (newItem == Item)
                {
                    if (newItem.IsAir)
                    {
                        if (newItem.type != Type)
                        {
                            _items[Type][Index] = new(Type, 0);
                        }
                        return;
                    }
                    if(newItem.type != Type)
                    {
                        _items[Type][Index] = new(Type, 0);
                        Type = newItem.type;
                        Index = PushItemToFirstEmptySlot(newItem);
                        Item = _items[Type][Index];
                    }
                }
                else
                {
                    if (newItem.IsAir)
                    {
                        _items[Type][Index] = new(Type, 0);
                        return;
                    }
                    if (newItem.type != Type)
                    {
                        _items[Type][Index] = new(Type, 0);
                        Type = newItem.type;
                        Index = PushItemToFirstEmptySlot(newItem);
                        Item = newItem;
                        return;
                    }
                    _items[Type][Index] = newItem;
                }
            }
        }
    }
}