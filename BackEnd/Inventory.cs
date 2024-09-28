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
    internal static class Inventory
    {
        private static Dictionary<int, List<Item>> _items = [];
        private static NormalCache _cache = new();
        private static Item _fakeItem;
        private static bool _uiLock = false;
        private static object _lock = new();

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
            if (_uiLock)
            {
                return;
            }
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
            if (_uiLock)
            {
                return;
            }
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
            if(_uiLock)
            {
                refresh = false;
                return;
            }
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
        public static void PushItemToEnd(Item item)
        {
            if (_uiLock)
            {
                return;
            }
            if (!_items.TryGetValue(item.type, out List<Item> container))
            {
                _items[item.type] = container = [];
            }
            List<Item> splitedItems = [];
            SplitItem(item, splitedItems);
            container.AddRange(splitedItems);
            WriteCache(item.type);
        }
        public static int PutItemToDesignatedIndex(Item item, int index)
        {
            if (_uiLock)
            {
                return 0;
            }
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
            if (_uiLock)
            {
                return 0;
            }
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
            if (_uiLock)
            {
                return 0;
            }
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
            if (_uiLock)
            {
                return false;
            }
            if (!_items.TryGetValue(type, out List<Item> container) || container.IndexInRange(index))
            {
                return false;
            }
            item = container[index];
            container.RemoveAt(index);
            return true;
        }

        public static DisposeWapper<UICallbackInfo> GetItemsForUI(params Predicate<Item>[] filters)
        {
            if (_uiLock)
            {
                throw new InvalidOperationException("The last UI lock was not unlocked.");
            }
            lock (_lock)
            {
                _uiLock = true;
            }
            List<int> types = [.. _items.Keys];
            types.Sort();
            Dictionary<int, List<Item>> result = [];
            Parallel.ForEach(_items.Keys, type =>
            {
                var selected = from Item item in _items[type] where filters.All(filter => filter(item)) select item;
                if (selected.Any())
                {
                    result.Add(type, selected.ToList());
                }
            });
            DisposeWapper<UICallbackInfo> disposeWapper = new(new(result, true), AfterUIReleasesControl);
            return disposeWapper;
        }
        private static void AfterUIReleasesControl(UICallbackInfo info)
        {
            HashSet<int> referredTypes = [.. info.Items.Keys];
            foreach (var type in info.Items.Keys)
            {
                for(int i = 0; i < _items[type].Count;i++)
                {
                    var item = _items[type][i];
                    if (item.IsAir || item.type == type)
                    {
                        continue;
                    }
                    referredTypes.Add(item.type);
                    if (!_items.TryGetValue(item.type, out var container))
                    {
                        container = _items[type] = [];
                    }
                    container.Add(item);
                    _items[type][i] = new();
                }
            }
            if (info.Compress)
            {
                foreach (var type in referredTypes)
                {
                    CompressItemList(type);
                }
            }
            lock (_lock)
            {
                _uiLock = false;
            }
        }
        public class UICallbackInfo
        {
            public readonly Dictionary<int, List<Item>> Items;
            public bool Compress;
            internal UICallbackInfo(Dictionary<int, List<Item>> items, bool compress = false)
            {
                Items = items;
                Compress = compress;
            }
        }
    }
}