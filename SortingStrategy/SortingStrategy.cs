using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StashManagementHelper;

public static class SortingStrategy
{
    // The order of these types should be configurable
    public static List<Type> GroupingOrder { get; set; } =
    [
        typeof(Weapon),
        typeof(AmmoBox),
        typeof(BulletClass),
        typeof(GrenadeClass),
        typeof(MagazineClass),
        typeof(MedsClass),
        typeof(Mod),
        typeof(FoodClass),
        typeof(KnifeClass),
        typeof(ItemContainerClass),  // Secure container?
        typeof(GClass2453),
        typeof(GClass2537), // of MedClass
        typeof(GClass2548), // money?
        typeof(GClass2499),
        typeof(GClass2498),
        typeof(GClass2497),
        typeof(GClass2448),
        typeof(GClass2444),
        typeof(GClass2500),
    ];

    public static List<Item> Sort(this IEnumerable<Item> items)
    {
        return Settings.SortingStrategy.Value switch
        {
            SortEnum.Default => items.ToList(),
            SortEnum.Custom => items.SortByCustomOrder(),
            _ => items.ToList()
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
                ? descending ? originalList.OrderByDescending(keySelector) : originalList.OrderBy(keySelector)
                : descending ? orderedItems.ThenByDescending(keySelector) : orderedItems.ThenBy(keySelector);
        }

        return orderedItems?.ToList() ?? originalList;
    }

    private static object GetIndexOfItemType(Item i) => GroupingOrder?.IndexOf(i.GetType()) ?? -1;

    private static object GetContainerSize(Item item)
    {
        return item.Attributes.FirstOrDefault(y => y.Id.Equals(EItemAttributeId.ContainerSize))?.Base.Invoke() ?? -1;
    }
}