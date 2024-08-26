﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using SML.Common;
using Terraria.ID;

namespace LargerInventory.BackEnd
{
    internal static class Inventory
    {
        internal static Dictionary<int, List<Item>> _items = [];
        private static NormalCache _cache = new();
        private static Item _fakeItem;

        private const string CacheKey_CachedType = "cachedType";
        private const string CacheKey_HealLifeData = "healLifeData";
        private const string CacheKey_HealManaData = "healManaData";

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
            foreach (var item in items)
            {
                bool addedToExisting = false;
                foreach (var resultItem in resultItems)
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
            foreach (var item in resultItems)
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
            var cachedType = GetOrCreateCache<HashSet<int>>(CacheKey_CachedType);
            if (!cachedType.Add(type))
            {
                return;
            }
            Item item = ContentSamples.ItemsByType[type];
            if (item.potion && item.healLife > 0)
            {
                var healLife = GetOrCreateCache<Dictionary<Item, int>>(CacheKey_HealLifeData);
                healLife[item] = item.healLife;
            }
            if (item.healMana > 0 && !item.potion)
            {
                var healMana = GetOrCreateCache<Dictionary<Item, int>>(CacheKey_HealManaData);
                healMana[item] = item.healMana;
            }
        }
        public static void PushItem(Item item)
        {
            if (!_items.TryGetValue(item.type, out var container))
            {
                _items[item.type] = container = [];
            }
            foreach (var target in container)
            {
                if (ItemLoader.CanStack(target, item))
                {
                    int move = Math.Min(target.maxStack - target.stack, item.stack);
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
            WriteCache(item.type);
        }
        public static void PushItemToEnd(Item item)
        {
            if (!_items.TryGetValue(item.type, out var container))
            {
                _items[item.type] = container = [];
            }
            List<Item> splitedItems = [];
            SplitItem(item, splitedItems);
            container.AddRange(splitedItems);
            WriteCache(item.type);
        }
        public static bool PutItemToDesignatedIndex(Item item, int index)
        {
            if (!_items.TryGetValue(item.type, out var container) || !container.IndexInRange(index))
            {
                return false;
            }
            Item target = container[index];
            if (!ItemLoader.CanStack(target, item))
            {
                return false;
            }
            int move = Math.Min(target.maxStack - target.stack, item.stack);
            item.stack -= move;
            target.stack += move;
            ItemLoader.OnStack(target, item, move);
            WriteCache(item.type);
            return true;
        }
        public static int PickItem(Item item, int count)
        {
            if (!_items.TryGetValue(item.type, out var container))
            {
                return 0;
            }
            count = Math.Min(count, item.maxStack - item.stack);
            int moved = 0;
            foreach (var target in container)
            {
                int move = Math.Min(target.stack, count);
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
            if (!_items.TryGetValue(item.type, out var container) || !container.IndexInRange(index))
            {
                return 0;
            }
            count = Math.Min(count, item.maxStack - item.stack);
            Item target = container[index];
            int move = Math.Min(target.stack, count);
            target.stack -= move;
            item.stack += move;
            return move;
        }
        public static bool ExchangeItems(ref Item item, int index)
        {
            if (!_items.TryGetValue(item.type, out var container) || !container.IndexInRange(index))
            {
                return false;
            }
            (item, container[index]) = (container[index], item);
            return true;
        }
        public static void CompressItemList(int type)
        {
            if (_items.TryGetValue(type, out var container))
            {
                CompressItems(container);
            }
        }
        public static void CompressAllItems()
        {
            foreach (var items in _items.Values)
            {
                CompressItems(items);
            }
        }
        private static KeyValuePair<Item, int> FindBestMatch(Dictionary<Item, int> data, int target)
        {
            return data.OrderBy(kvp=>Math.Abs(kvp.Value-target))
                .ThenBy(kvp=>kvp.Value)
                .ThenBy(kvp=>kvp.Key.value)
                .ThenBy(kvp=>kvp.Key.type)
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
            var bestMatch = FindBestMatch(GetOrCreateCache<Dictionary<Item, int>>(CacheKey_HealLifeData), cure);
            _fakeItem ??= new();
            _fakeItem.SetDefaults(bestMatch.Key.type);
            _fakeItem.stack = 0;
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
            var beatMatch = FindBestMatch(GetOrCreateCache<Dictionary<Item, int>>(CacheKey_HealManaData), cure);
            _fakeItem ??= new();
            _fakeItem.SetDefaults(beatMatch.Key.type);
            _fakeItem.stack = 0;
            PickItem(_fakeItem, 1);
            player.ApplyLifeAndOrMana(_fakeItem);
        }
    }
}