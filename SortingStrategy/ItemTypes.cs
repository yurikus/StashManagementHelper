using EFT.InventoryLogic;
using System;
using System.Collections.Generic;

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
        //Info,
        //Backpacks,
    }

    public static readonly Dictionary<ItemType, Func<Item, bool>> ItemTypeMap = new()
    {
        {ItemType.Weapons, item => item is Weapon},
        {ItemType.Magazines, item => item is MagazineClass},
        {ItemType.Ammo, item => item is BulletClass or AmmoBox},
        {ItemType.Meds, item => item is MedsClass},
        {ItemType.FoodAndDrink, item => item is FoodClass},
        {ItemType.Melee, item => item is KnifeClass},
        {ItemType.Mods, item => item is Mod},
        {ItemType.Grenades, item => item is GrenadeClass},
        {ItemType.Barter, item => item is GClass2704 or GClass2738},
        {ItemType.Rigs, item => item is GClass2685},
        {ItemType.Goggles, item => item is GogglesClass},
        {ItemType.Equipment, item => item is ArmorClass},
        {ItemType.Keys, item => item is GClass2720},
        {ItemType.Containers, item => item is SearchableItemClass or GClass2686},
        {ItemType.RepairKits, item => item is GClass2730},
        {ItemType.SpecialEquipment, item => item is GClass2731},
        {ItemType.BallisticPlates, item => item is GClass2633},
        {ItemType.Money, item => item is GClass2735},
        //{ItemType.Info, item => item is GClass2738},
        //{ItemType.Backpacks, item => item is GClass2684},
    };
}