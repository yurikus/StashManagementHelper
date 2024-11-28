using System;
using System.Collections.Generic;
using EFT.InventoryLogic;

namespace StashManagementHelper;

public static class ItemTypes
{
    public enum ItemType
    {
        Weapons,
        Magazines,
        Ammo,
        Meds,
        FoodAndDrink,
        Melee,
        Mods,
        Grenades,
        Barter,
        Rigs,
        Goggles,
        Containers,
        Equipment,
        Keys,
        RepairKits,
        SpecialEquipment,
        BallisticPlates,
        Money,
        Backpacks,
        Info,
    }

    public static readonly Dictionary<ItemType, Func<Item, bool>> ItemTypeMap = new()
    {
        {ItemType.Weapons, item => item is Weapon},
        {ItemType.Magazines, item => item is MagazineItemClass},
        {ItemType.Ammo, item => item is AmmoItemClass or AmmoBox},
        {ItemType.Meds, item => item is MedsItemClass},
        {ItemType.FoodAndDrink, item => item is FoodItemClass},
        {ItemType.Melee, item => item is KnifeItemClass},
        {ItemType.Mods, item => item is Mod},
        {ItemType.Grenades, item => item is ThrowWeapItemClass},
        {ItemType.Barter, item => item is BarterItemItemClass},
        {ItemType.Rigs, item => item is VestItemClass},
        {ItemType.Goggles, item => item is VisorsItemClass},
        {ItemType.Equipment, item => item is ArmorItemClass},
        {ItemType.Keys, item => item is KeyItemClass},
        {ItemType.Containers, item => item is SearchableItemItemClass },
        {ItemType.Backpacks, item => item is BackpackItemClass },
        {ItemType.RepairKits, item => item is RepairKitsItemClass},
        {ItemType.SpecialEquipment, item => item is SpecialScopeItemClass or SpecialWeaponItemClass},
        {ItemType.BallisticPlates, item => item is ArmorPlateItemClass},
        {ItemType.Money, item => item is MoneyItemClass},
        {ItemType.Info, item => item is InfoItemClass},
    };
}