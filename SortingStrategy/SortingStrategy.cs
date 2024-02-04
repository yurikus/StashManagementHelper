using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StashManagementHelper;

public static class SortingStrategy
{
    // This should be configurable
    public static List<Type> GroupingOrder { get; set; } =
    [
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
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        return Settings.SortingStrategy.Value switch
        {
            SortEnum.Default => items.SortByDefault(),
            SortEnum.Custom => items.SortByCustomOrder(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static List<Item> SortByCustomOrder(this IEnumerable<Item> items)
    {
        var sortFunctions = new List<(Func<Item, object> keySelector, bool enabled, bool descending)>
        {
            (GetContainerSize, Settings.ContainerSize.Value.HasFlag(SortOptions.Enabled), Settings.ContainerSize.Value.HasFlag(SortOptions.Descending)),
            (GetIndexOfItemType, Settings.IndexOfItemType.Value.HasFlag(SortOptions.Enabled), Settings.IndexOfItemType.Value.HasFlag(SortOptions.Descending)),
            (y => y.CalculateCellSize().Length, Settings.CellSize.Value.HasFlag(SortOptions.Enabled), Settings.CellSize.Value.HasFlag(SortOptions.Descending))
        };

        IOrderedEnumerable<Item> orderedItems = null;
        var originalList = items.ToList();

        foreach (var (keySelector, enabled, descending) in sortFunctions)
        {
            if (!enabled) continue;

            orderedItems = orderedItems == null
                ? (descending ? originalList.OrderByDescending(keySelector) : originalList.OrderBy(keySelector))
                : (descending ? orderedItems.ThenByDescending(keySelector) : orderedItems.ThenBy(keySelector));
        }

        return orderedItems?.ToList() ?? originalList;
    }

    private static List<Item> SortByDefault(this IEnumerable<Item> items) => items.ToList();

    private static object GetIndexOfItemType(Item i) => GroupingOrder?.IndexOf(i.GetType()) ?? -1;

    private static object GetContainerSize(Item item)
    {
        return item.Attributes.FirstOrDefault(y => y.Id.Equals(EItemAttributeId.ContainerSize))?.Base.Invoke() ?? -1;
    }
}