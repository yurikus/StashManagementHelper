using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StashManagementHelper;

public static class SortingStrategy
{
    public static int GetIndexOfItemType(this Item i) => GroupingOrder.IndexOf(i.GetType());

    // This should be configurable
    public static List<Type> GroupingOrder = [
        typeof(Item),
        typeof(Weapon),
        typeof(AmmoBox),
        typeof(BulletClass),
        typeof(GrenadeClass),
        typeof(MagazineClass),
        typeof(MedsClass),
        typeof(Mod)
    ];

    public static List<Item> Sort(this IEnumerable<Item> items)
    {
        return Settings.SortingStrategy.Value switch
        {
            SortEnum.Size => items.SortBySize(),
            SortEnum.Default => items.SortByDefault(),
            SortEnum.Custom => items.SortByCustomOrder(),
            _ => items.SortByDefault()
        };
    }

    private static List<Item> SortBySize(this IEnumerable<Item> items)
    {
        return Settings.LargestItemsFirst.Value
            ? items.OrderByDescending(x => x.CalculateCellSize().Length).ToList()
            : items.OrderBy(x => x.CalculateCellSize().Length).ToList();
    }

    private static List<Item> SortByCustomOrder(this IEnumerable<Item> items)
    {
        if (Settings.LargestItemsFirst.Value)
        {
            return items
                .OrderByDescending(x => x.IsContainer)
                //.ThenBy(x => x.GetIndexOfItemType())
                .ThenByDescending(y => y.CalculateCellSize().Length)
                .ToList();
        }
        else
        {
            return items
                .OrderByDescending(x => x.IsContainer)
                //.ThenBy(x => x.GetIndexOfItemType())
                .ThenBy(y => y.CalculateCellSize().Length)
                .ToList();
        }
    }

    private static List<Item> SortByDefault(this IEnumerable<Item> items) => items.ToList();
}