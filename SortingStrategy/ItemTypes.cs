using System;
using System.Collections.Generic;
using System.Linq;
using EFT.InventoryLogic;

namespace StashManagementHelper;

public static class ItemTypes
{
    public enum ItemType
    {
        Weapons,
        Armor,
        Magazines,
        Ammo,
        Meds,
        Food,
        Drink,
        Melee,
        Mods,
        Grenades,
        Barter,
        Rigs,
        Eyewear,
        Containers,
        Headgear,
        Facecovers,
        Headsets,
        Keys,
        RepairKits,
        SpecialEquipment,
        BallisticPlates,
        Money,
        Backpacks,
        Info,
        HeadgearArmor,
        Unknown
    }

    // Optimized type checking with refined order based on likely inheritance
    public static ItemType GetItemTypeEnum(Item item) => item switch
    {
        // Most specific subtypes first
        ArmoredEquipmentItemClass => ItemType.HeadgearArmor,
        BackpackItemClass => ItemType.Backpacks,
        VestItemClass => ItemType.Rigs,
        SpecialScopeItemClass => ItemType.SpecialEquipment,
        SpecialWeaponItemClass => ItemType.SpecialEquipment,
        VisorsItemClass => ItemType.Eyewear,
        HeadwearItemClass => ItemType.Headgear,
        FaceCoverItemClass => ItemType.Facecovers,
        HeadphonesItemClass => ItemType.Headsets,
        MagazineItemClass => ItemType.Magazines,
        AmmoBox => ItemType.Ammo,
        MedsItemClass => ItemType.Meds,
        FoodItemClass => ItemType.Food,
        DrinkItemClass => ItemType.Drink,
        KnifeItemClass => ItemType.Melee,
        ThrowWeapItemClass => ItemType.Grenades,
        KeyItemClass => ItemType.Keys,
        RepairKitsItemClass => ItemType.RepairKits,
        ArmorPlateItemClass => ItemType.BallisticPlates,
        MoneyItemClass => ItemType.Money,
        InfoItemClass => ItemType.Info,
        BarterItemItemClass => ItemType.Barter,

        // More general types later
        Weapon => ItemType.Weapons,
        ArmorItemClass => ItemType.Armor,
        Mod => ItemType.Mods,
        AmmoItemClass => ItemType.Ammo,
        SimpleContainerItemClass => ItemType.Containers,

        _ => ItemType.Unknown
    };

    public static readonly List<ItemType> AllItemTypes = Enum.GetValues(typeof(ItemType))
                                                            .Cast<ItemType>()
                                                            .Where(it => it != ItemType.Unknown)
                                                            .ToList();
}