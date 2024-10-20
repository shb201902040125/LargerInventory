using ReLogic.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace LargerInventory.UI.ExtraUI
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
                                        break;
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
                            throw new ArgumentException("When CombineType is CountTrue, extra should be of type int", nameof(extra));
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
                                        break;
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
        public static class Prefab
        {
            public static readonly InvItemFilter Default = new(i => true);

            public static readonly InvItemFilter IsWeapon = new(i => i.damage > 0);
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

            public static readonly InvItemFilter IsHead = new(i => i.headSlot > 0);
            public static readonly InvItemFilter IsBody = new(i => i.bodySlot > 0);
            public static readonly InvItemFilter IsLeg = new(i => i.legSlot > 0);
            static InvItemFilter _isArmor;
            public static InvItemFilter IsArmor => _isArmor ??= Combine(CombineType.AnyTrue, null, IsHead, IsBody, IsLeg);

            static Dictionary<EquipType, InvItemFilter> _isEquip = [];
            public static InvItemFilter IsEquip(EquipType type)
            {
                if (_isEquip.TryGetValue(type, out InvItemFilter filter))
                {
                    return filter;
                }
                return _isEquip[type] = new(i =>
                {
                    var att = i.ModItem?.GetType().GetAttribute<AutoloadEquip>() ?? null;
                    if ((att?.equipTypes ?? null) == null)
                    {
                        return false;
                    }
                    foreach (var attType in att.equipTypes)
                    {
                        if (type == attType)
                        {
                            return true;
                        }
                    }
                    return false;
                });
            }

            public static readonly InvItemFilter IsLightPet = new(i => Main.lightPet[i.buffType]);
            public static readonly InvItemFilter IsVanityPet = new(i => Main.vanityPet[i.buffType]);
            public static readonly InvItemFilter IsProjPet = new(i => Main.projPet[i.buffType]);
            public static readonly InvItemFilter IsHook = new(i => Main.projHook[i.shoot]);
            public static readonly InvItemFilter IsMount = new(i => i.mountType != -1);
            public static readonly InvItemFilter IsMinecraft = Combine(CombineType.AllTrue, null, IsMount, new(i => Mount.mounts[i.mountType].Minecart));
            public static readonly InvItemFilter IsDye = new(i => i.dye > 0);

            public static readonly InvItemFilter IsAccessory = new(i => i.accessory);

            public static readonly InvItemFilter IsPlaceableTile = new(i => i.createTile != -1);
            public static readonly InvItemFilter IsPlaceableWall = new(i => i.createWall != -1);

            public static readonly InvItemFilter IsConsumeable = new(i => i.consumable);
            public static readonly InvItemFilter IsMedicament = new(i => i.buffType > 0 && i.useStyle == ItemUseStyleID.DrinkLiquid);
            public static readonly InvItemFilter IsAmmo = new(i => i.ammo != AmmoID.None);

            public static readonly InvItemFilter IsFurniture = Combine(CombineType.AllTrue, null, IsPlaceableTile, new(i => Main.tileFrameImportant[i.createTile]));

            public static readonly InvItemFilter IsMaterial = new(i => i.material);

            static InvItemFilter _exclusionAll;
            public static InvItemFilter ExclusionAll
            {
                get
                {
                    if(_exclusionAll == null)
                    {
                        _exclusionAll = Combine(CombineType.AllFalse, null,
                            IsWeapon,
                            IsTool,
                            IsArmor,
                            IsAccessory,
                            IsPlaceableTile,
                            IsPlaceableWall,
                            IsConsumeable,
                            IsMaterial);
                    }
                    return _exclusionAll;
                }
            }
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
