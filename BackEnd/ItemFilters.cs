using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LargerInventory.BackEnd
{
    public class InvItemFilter
    {
        Func<Item, bool> _filter;
        public InvItemFilter(Func<Item, bool> predicate)
        {
            _filter = predicate;
        }
        public static InvItemFilter Combine(CombineType combineType, object extra = null, params InvItemFilter[] invItemFilters)
        {
            switch (combineType)
            {
                case CombineType.AllTrue:
                    {
                        return new(item =>
                        {
                            return invItemFilters.All(filter => filter.Check(item));
                        });
                    }
                case CombineType.AllFalse:
                    {
                        return new(item =>
                        {
                            return invItemFilters.All(filter => !filter.Check(item));
                        });
                    }
                case CombineType.AnyTrue:
                    {
                        return new(item =>
                        {
                            return invItemFilters.Any(filter => filter.Check(item));
                        });
                    }
                case CombineType.AnyFalse:
                    {
                        return new(item =>
                        {
                            return invItemFilters.Any(filter => !filter.Check(item));
                        });
                    }
                case CombineType.CountTrue:
                    {
                        if (extra is not int target)
                        {
                            throw new ArgumentException("When CombineType is CountTrue, extra should be of type int", nameof(extra));
                        }
                        return new(item =>
                        {
                            int count = 0;
                            for (int i = 0; i < invItemFilters.Length; i++)
                            {
                                if (invItemFilters[i].Check(item))
                                {
                                    count++;
                                    if (count >= target)
                                    {

                                    }
                                }
                            }
                            return count >= target;
                        });
                    }
                case CombineType.CountFalse:
                    {
                        if (extra is not int target)
                        {
                            throw new ArgumentException("When CombineType is CountFalse, extra should be of type int", nameof(extra));
                        }
                        return new(item =>
                        {
                            int count = 0;
                            for (int i = 0; i < invItemFilters.Length; i++)
                            {
                                if (!invItemFilters[i].Check(item))
                                {
                                    count++;
                                    if (count >= target)
                                    {

                                    }
                                }
                            }
                            return count >= target;
                        });
                    }
                default:
                    {
                        throw new ArgumentException("The CombineType parameter is incorrect");
                    }
            }
        }
        public bool Check(Item item)
        {
            return _filter(item);
        }
        ~InvItemFilter()
        {
            _filter = null;
        }
        public static class FilterPrefab
        {
            public static readonly Func<Item, bool> Default = new(i => true);

            public static readonly InvItemFilter IsWeapon = new(i => i.damage > 0 && i.useStyle != ItemUseStyleID.None);
            static InvItemFilter _isDamageClass_0, _isDamageClass_1, _isDamageClass_2;
            /// <summary>
            /// Get a <see cref="InvItemFilter"/> for given DamageClass
            /// </summary>
            /// <param name="damageClass">given DamageClass</param>
            /// <param name="asType">0 for strictly equal to<br>1 for <see cref="DamageClass.CountsAsClass(DamageClass)"/></br><br>2 for <see cref="DamageClass.GetEffectInheritance(DamageClass)"/></br></param>
            /// <returns></returns>
            public static InvItemFilter IsDamageAs(DamageClass damageClass, int asType)
            {
                return asType switch
                {
                    1 => _isDamageClass_1 ??= new(i => i.DamageType.CountsAsClass(damageClass)),
                    2 => _isDamageClass_2 ??= new(i => i.DamageType.GetEffectInheritance(damageClass)),
                    _ => _isDamageClass_0 ??= new(i => i.DamageType == damageClass)
                };
            }

            public static readonly InvItemFilter IsAxe = new(i => i.axe > 0);
            public static readonly InvItemFilter IsHammer = new(i => i.hammer > 0);
            public static readonly InvItemFilter IsPick = new(i => i.pick > 0);
            static InvItemFilter _isTool;
            public static InvItemFilter IsTool => _isTool ??= Combine(CombineType.AnyTrue, null, IsAxe, IsHammer, IsPick);

            static InvItemFilter _isArmor;
            public static InvItemFilter IsArmor => _isArmor ??= Combine(CombineType.AnyTrue, null,
                IsEquip(EquipType.Head), IsEquip(EquipType.Body), IsEquip(EquipType.Legs));

            private readonly static Dictionary<EquipType, InvItemFilter> _isEquip = [];
            private static InvItemFilter CheckEquip(EquipType equip) => equip switch
            {
                EquipType.Head => new(i => i.headSlot > 0),
                EquipType.Body => new(i => i.bodySlot > 0),
                EquipType.Legs => new(i => i.legSlot > 0),
                EquipType.HandsOn => new(i => i.handOnSlot > 0),
                EquipType.HandsOff => new(i => i.handOffSlot > 0),
                EquipType.Back => new(i => i.backSlot > 0),
                EquipType.Front => new(i => i.frontSlot > 0),
                EquipType.Shoes => new(i => i.shoeSlot > 0),
                EquipType.Waist => new(i => i.waistSlot > 0),
                EquipType.Wings => new(i => i.wingSlot > 0),
                EquipType.Shield => new(i => i.shieldSlot > 0),
                EquipType.Neck => new(i => i.neckSlot > 0),
                EquipType.Face => new(i => i.faceSlot > 0),
                EquipType.Balloon => new(i => i.balloonSlot > 0),
                EquipType.Beard => new(i => i.beardSlot > 0),
                _ => null,
            };
            public readonly static InvItemFilter CanEquip = Combine(CombineType.AnyTrue, null, LoadEquipFilter());
            private static InvItemFilter[] LoadEquipFilter()
            {
                foreach (var equip in Enum.GetValues<EquipType>())
                {
                    _isEquip[equip] = CheckEquip(equip);
                }
                return [.. _isEquip.Values];
            }
            public static InvItemFilter IsEquip(EquipType type) => _isEquip[type];

            public static readonly InvItemFilter IsLightPet = new(i => Main.lightPet[i.buffType]);
            public static readonly InvItemFilter IsVanityPet = new(i => Main.vanityPet[i.buffType]);

            /// <summary>
            /// 这个好像不对
            /// </summary>
            public static readonly InvItemFilter IsProjPet = new(i => Main.projPet[i.buffType]);
            public static readonly InvItemFilter IsHook = new(i => Main.projHook[i.shoot]);
            public static readonly InvItemFilter IsMount = new(i => i.mountType != -1);
            public static readonly InvItemFilter IsMinecrat = Combine(CombineType.AllTrue, null, IsMount, new(i => Mount.mounts[i.mountType].Minecart));
            public static readonly InvItemFilter IsDye = new(i => i.dye > 0);

            public static readonly InvItemFilter IsAccessory = new(i => i.accessory);

            public static readonly InvItemFilter IsPlaceableTile = new(i => i.createTile != -1);
            public static readonly InvItemFilter IsPlaceableWall = new(i => i.createWall != -1);
            public static readonly InvItemFilter IsFurniture = Combine(CombineType.AllTrue, null, IsPlaceableTile, new(i => Main.tileFrameImportant[i.createTile]));
            public static readonly InvItemFilter IsPlaceable = Combine(CombineType.AnyTrue, null, IsPlaceableTile, IsPlaceableWall);

            public static readonly InvItemFilter IsConsumeable = new(i => i.consumable);
            public static readonly InvItemFilter IsHealthOrMana = new(i => i.healLife > 0 || i.healMana > 0);
            public static readonly InvItemFilter IsMedicament = new(i => i.buffType > 0 && i.useStyle == ItemUseStyleID.DrinkLiquid);
            public static readonly InvItemFilter IsFood = new(i => i.buffType > 0 && i.useStyle == ItemUseStyleID.EatFood);
            public static readonly InvItemFilter IsAmmo = new(i => i.ammo > AmmoID.None);



            public static readonly InvItemFilter IsMaterial = new(i => i.material);
            public static readonly InvItemFilter IsCoin = new(i => i.IsCurrency || i.IsACoin);

            static InvItemFilter _exclusionAll;
            public static InvItemFilter ExclusionAll => _exclusionAll ??= Combine(CombineType.AllFalse, null,
                IsWeapon, IsTool, CanEquip, IsAccessory, IsPlaceableTile, IsPlaceableWall, IsConsumeable, IsMaterial, IsCoin);
        }
        public enum CombineType
        {
            AllTrue,
            AllFalse,
            AnyTrue,
            AnyFalse,
            CountTrue,
            CountFalse
        }
    }
}
