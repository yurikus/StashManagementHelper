using SPT.Core.Utils;
using BepInEx.Logging;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace StashManagementHelper;

public static class SortingStrategy
{
    private static readonly string configPath;

    private static List<string> SortOrder { get; set; } = ["ContainerSize", "CellSize", "ItemType"];

    private static List<ItemTypes.ItemType> ItemTypeOrder2 { get; set; } =
    [
        ItemTypes.ItemType.Ammo,
        ItemTypes.ItemType.Grenades,
        ItemTypes.ItemType.Magazines,
        ItemTypes.ItemType.Weapons,
        ItemTypes.ItemType.Equipment,
        ItemTypes.ItemType.Rigs,
        ItemTypes.ItemType.Goggles,
        ItemTypes.ItemType.Melee,
        ItemTypes.ItemType.Meds,
        ItemTypes.ItemType.FoodAndDrink,
        ItemTypes.ItemType.Mods,
        ItemTypes.ItemType.Containers,
        ItemTypes.ItemType.RepairKits,
        ItemTypes.ItemType.BallisticPlates,
        ItemTypes.ItemType.Barter,
        ItemTypes.ItemType.SpecialEquipment,
        ItemTypes.ItemType.Keys,
        ItemTypes.ItemType.Money,
        //ItemTypes.ItemType.Info,
        //ItemTypes.ItemType.Backpacks,
    ];

    //private static List<string> ItemTypeOrder { get; set; } =
    //[
    //nameof(Weapon),
    //nameof(AmmoBox),
    //nameof(BulletClass),
    //nameof(GrenadeClass),
    //nameof(MagazineClass),
    //nameof(MedsClass),
    //nameof(Mod),
    //nameof(FoodClass),
    //nameof(KnifeClass),
    //nameof(ItemContainerClass),
    //nameof(GogglesClass),
    //nameof(MountModClass),
    //nameof(BarrelModClass),
    //nameof(StockItemClass),

    // TODO...
    //nameof(GClass2633), // Ballistic plates
    //nameof(GClass2635), // Face covers
    //nameof(GClass2636), // Headgear
    //nameof(GClass2637), // Body armor 
    //nameof(GClass2639), // Headsets
    //nameof(GClass2641), // Storage containers (case)
    //nameof(GClass2642), // Mods > Auxiliary parts
    //nameof(GClass2644), // Mods > Bipods
    //nameof(GClass2647), // Mods > Gas blocks
    //nameof(GClass2653), // Mods > Muzzle devices > Brakes
    //nameof(GClass2656), // Mods > Muzzle devices > Suppressors
    //nameof(GClass2659), // Sights > Collimators
    //nameof(GClass2661), //
    //nameof(GClass2662), //
    //nameof(GClass2663), //
    //nameof(GClass2672), //
    //nameof(GClass2678), //
    //nameof(GClass2679), //
    //nameof(GClass2684), // Backpacks
    //nameof(GClass2685), // Tactical rigs
    //nameof(GClass2686), // Storage containers (box)
    //nameof(GClass2690), // ? of Weapon
    //nameof(GClass2691), // Assault rifles
    //nameof(GClass2692), // Flares
    //nameof(GClass2694), // Storage containers (box)
    //nameof(GClass2695), // Pistols 
    //nameof(GClass2697), // Shotguns 
    //nameof(GClass2698), // SMGs 
    //nameof(GClass2699), // Bolt-action rifles 
    //nameof(GClass2705), // Barter items > Energy elements
    //nameof(GClass2706), // Barter items > Building materials
    //nameof(GClass2707), // Barter items > Electronics
    //nameof(GClass2708), // Barter items > Household materials
    //nameof(GClass2709), // Barter items > Valuables
    //nameof(GClass2710), // Barter items > Flammable materials
    //nameof(GClass2711), // Barter items > Medical supplies
    //nameof(GClass2712), // Barter items > Tools
    //nameof(GClass2713), // Barter items > Others
    //nameof(GClass2714), // Fuel
    //nameof(GClass2718), // Drinks
    //nameof(GClass2719), // Food
    //nameof(GClass2721), // Keys > Mechanical keys
    //nameof(GClass2726), // Medkits
    //nameof(GClass2727), // Heal Injectors?
    //nameof(GClass2728), // Painkillers
    //nameof(GClass2729), // Injury treatment
    //nameof(GClass2730), // Repair kits
    //nameof(GClass2731), // Special equipments (tool)
    //nameof(GClass2732), // Special equipments (compass)
    //nameof(GClass2737), // Money
    //nameof(GClass2738), // ?
    //];

    static SortingStrategy()
    {
        var dllPath = Assembly.GetExecutingAssembly().Location;
        configPath = Path.Combine(Path.GetDirectoryName(dllPath) ?? string.Empty, "customSortConfig.json");
    }

    public static List<Item> Sort(this IEnumerable<Item> items)
        => Settings.SortingStrategy.Value switch
        {
            SortEnum.Default => items.ToList(),
            SortEnum.Custom => items.SortByCustomOrder(),
            _ => items.ToList()
        };

    private static List<Item> SortByCustomOrder(this IEnumerable<Item> items)
    {
        LoadSortOrder();
        var sortFunctions = SortOrder
            .Select(type => (
                GetSortFunction(type),
                Settings.GetSortOption(type).HasFlag(SortOptions.Enabled),
                Settings.GetSortOption(type).HasFlag(SortOptions.Descending)
            ))
            .Where(sf => sf.Item2)
            .Reverse()
            .ToList();

        var orderedItems = items;

        foreach (var (keySelector, _, descending) in sortFunctions)
        {
            orderedItems = descending
                ? orderedItems.OrderByDescending(keySelector)
                : orderedItems.OrderBy(keySelector);
        }

        return orderedItems.ToList();
    }

    private static Func<Item, object> GetSortFunction(string sortType)
    {
        return sortType switch
        {
            "ContainerSize" => GetContainerSize,
            "ItemType" => GetItemType2,
            "CellSize" => item => item.CalculateCellSize().Length,
            _ => throw new ArgumentException("Invalid sort type")
        };
    }

    //private static object GetItemType(Item i)
    //{
    //    var index = ItemTypeOrder?.IndexOf(i.GetType().Name) ?? 999;
    //    return index;
    //}

    private static object GetItemType2(Item item)
    {
        var itemType = ItemTypes.ItemTypeMap.FirstOrDefault(entry => entry.Value(item)).Key;
        var index = ItemTypeOrder2.IndexOf(itemType);
        return index == -1 ? 9999 : index;
    }

    private static object GetContainerSize(Item item)
    {
        return item.Attributes.FirstOrDefault(y => y.Id.Equals(EItemAttributeId.ContainerSize))?.Base.Invoke() ?? -1;
    }

    private static void LoadSortOrder()
    {
        try
        {
            if (!File.Exists(configPath))
            {
                var defaultConfig = new Dictionary<string, List<string>>
                {
                    { "sortOrder", SortOrder },
                    { "itemTypeOrder", ItemTypeOrder2.Select(itemType => itemType.ToString()).ToList() }
                };
                var defaultJson = JsonConvert.SerializeObject(defaultConfig);
                File.WriteAllText(configPath, defaultJson);
            }
            else
            {
                var json = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                SortOrder = config["sortOrder"];
                ItemTypeOrder2 = config["itemTypeOrder"].Select(itemType => (ItemTypes.ItemType)Enum.Parse(typeof(ItemTypes.ItemType), itemType)).ToList();
            }
        }
        catch (Exception e)
        {
            Logger.CreateLogSource("StashManagementHelper").LogError(e.Message);
            throw;
        }
    }
}