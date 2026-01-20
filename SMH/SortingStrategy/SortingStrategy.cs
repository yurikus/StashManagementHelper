using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using Newtonsoft.Json;

namespace SMH;

public static class SortingStrategy
{
    static bool inSync = false;
    static DateTime lastConfigFileWriteTime = DateTime.MinValue;

    static readonly string configPath;

    static List<ItemTypes.ItemType> ItemTypeOrder { get; set; } =
    [
        ItemTypes.ItemType.Money,
        ItemTypes.ItemType.Keys,
        ItemTypes.ItemType.Ammo,
        ItemTypes.ItemType.Grenades,
        ItemTypes.ItemType.Magazines,
        ItemTypes.ItemType.Weapons,
        ItemTypes.ItemType.Headgear,
        ItemTypes.ItemType.HeadgearArmor,
        ItemTypes.ItemType.Facecovers,
        ItemTypes.ItemType.Rigs,
        ItemTypes.ItemType.NightAndThermalVision,
        ItemTypes.ItemType.Eyewear,
        ItemTypes.ItemType.Melee,
        ItemTypes.ItemType.Meds,
        ItemTypes.ItemType.Food,
        ItemTypes.ItemType.Drink,
        ItemTypes.ItemType.Mods,
        ItemTypes.ItemType.RepairKits,
        ItemTypes.ItemType.SpecialEquipment,
        ItemTypes.ItemType.Barter,
        ItemTypes.ItemType.Armor,
        ItemTypes.ItemType.Info,
        ItemTypes.ItemType.Backpacks,
        ItemTypes.ItemType.Headsets,
        ItemTypes.ItemType.Containers,
        ItemTypes.ItemType.BallisticPlates,
        ItemTypes.ItemType.Armband
    ];

    static SortingStrategy()
    {
        var dllPath = Assembly.GetExecutingAssembly().Location;
        configPath = Path.Combine(Path.GetDirectoryName(dllPath) ?? string.Empty, "SMH.CustomSortConfig.json");
    }

    public static List<Item> Sort(this IEnumerable<Item> items)
    {
        LoadSortOrder();
        return [.. items.OrderBy(GetItemType)];
    }

    static object GetItemType(Item item)
    {
        var itemType = ItemTypes.ItemTypeMap.FirstOrDefault(entry => entry.Value(item)).Key;
        var index = ItemTypeOrder.IndexOf(itemType);

        if (index == -1)
        {
            ItemManager.Log.LogInfo($"Unknown item type: {item.GetType().Name}");
            return ItemTypeOrder.Count + 100;
        }

        return index;
    }

    static void SyncItemTypeOrder()
    {
        if (inSync)
            return;

        var mapItemTypes = ItemTypes.ItemTypeMap.Keys.ToList();
        var missingTypes = mapItemTypes.Except(ItemTypeOrder).ToList();
        var extraTypes = ItemTypeOrder.Except(mapItemTypes).ToList();

        if (missingTypes.Any())
        {
            ItemManager.Log.LogInfo($"Found {missingTypes.Count} item types in map but not in order: {string.Join(", ", missingTypes)}");

            foreach (var missingType in missingTypes)
            {
                int enumValue = (int) missingType;

                bool inserted = false;
                for (int i = 0; i < ItemTypeOrder.Count - 1; i++)
                {
                    int currentEnumValue = (int) ItemTypeOrder[i];
                    int nextEnumValue = (int) ItemTypeOrder[i + 1];

                    if (enumValue > currentEnumValue && enumValue < nextEnumValue)
                    {
                        ItemTypeOrder.Insert(i + 1, missingType);
                        inserted = true;
                        ItemManager.Log.LogInfo($"Inserted {missingType} after {ItemTypeOrder[i]}");
                        break;
                    }
                }

                if (!inserted)
                {
                    ItemTypeOrder.Add(missingType);
                    ItemManager.Log.LogInfo($"Added {missingType} to the end of order list");
                }
            }
        }

        if (extraTypes.Any())
        {
            ItemManager.Log.LogInfo($"Found {extraTypes.Count} item types in order but not in map: {string.Join(", ", extraTypes)}");
        }

        inSync = true;
    }

    static void LoadSortOrder()
    {
        try
        {
            if (!File.Exists(configPath))
            {
                SyncItemTypeOrder();

                var defaultConfig = new Dictionary<string, List<string>>
                {
                    { "itemTypeOrder", ItemTypeOrder.Select(itemType => itemType.ToString()).ToList() }
                };

                var defaultJson = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                File.WriteAllText(configPath, defaultJson);
                lastConfigFileWriteTime = File.GetLastWriteTimeUtc(configPath);
            }
            else
            {

                var currentWriteTime = File.GetLastWriteTimeUtc(configPath);
                if (currentWriteTime > lastConfigFileWriteTime)
                {

                    ItemManager.Log.LogInfo($"Config file '{configPath}' has changed. Reloading.");
                    var json = File.ReadAllText(configPath);
                    var config = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

                    if (config.TryGetValue("itemTypeOrder", out var loadedItemTypeOrderStrings))
                    {
                        var newItemTypeOrder = new List<ItemTypes.ItemType>();
                        if (loadedItemTypeOrderStrings != null)
                        {
                            foreach (var typeStr in loadedItemTypeOrderStrings)
                            {
                                if (Enum.TryParse<ItemTypes.ItemType>(typeStr, out var itemType))
                                {
                                    newItemTypeOrder.Add(itemType);
                                }
                                else
                                {
                                    ItemManager.Log.LogWarning($"Skipping invalid item type in config: {typeStr}");
                                }
                            }
                            ItemTypeOrder = newItemTypeOrder;
                        }
                        else
                        {
                            ItemManager.Log.LogWarning("Config file contains null 'itemTypeOrder'. Using previous or default.");
                        }
                    }
                    else
                    {
                        ItemManager.Log.LogWarning("Config file missing 'itemTypeOrder' key. Using previous or default.");
                    }

                    inSync = false;
                    SyncItemTypeOrder();

                    lastConfigFileWriteTime = currentWriteTime;
                }
                else
                {
                    SyncItemTypeOrder();
                }
            }
        }
        catch (Exception e)
        {
            ItemManager.Log.LogError($"Error loading sort configuration: {e.Message}");
            inSync = false;
            SyncItemTypeOrder();
        }
    }
}