using BepInEx.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StashManagementHelper;

public static class ItemManager
{
    public static ManualLogSource Logger { get; set; }

    /// <summary>
    /// Folds every weapon in the list to take up less space
    /// </summary>
    /// <param name="sortingItem">The table of items</param>
    /// <param name="controller">The Inventory controller class.</param>
    /// <param name="simulate">?</param>
    public static Task FoldSortingItems(LootItemClass sortingItem, InventoryControllerClass controller, bool simulate)
    {
        try
        {
            foreach (var sortingItemGrid in sortingItem.Grids)
            {
                foreach (var item in sortingItemGrid.Items)
                {
                    if (!GClass2585.CanFold(item, out var foldable)) continue;
                    if (foldable is null || foldable.Folded) continue;

                    Logger.LogDebug($"Folding {item.Name.Localized()}");
                    return controller.TryRunNetworkTransaction(GClass2585.Fold(foldable, true, controller.ID, simulate));
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Merge separate stacks of the same item
    /// </summary>
    /// <param name="sortingItem">The table of items</param>
    public static void MergeSortingItems(LootItemClass sortingItem)
    {
        try
        {
            foreach (var sortingItemGrid in sortingItem.Grids)
            {
                foreach (var item in sortingItemGrid.Items.Where(x => x.StackObjectsCount < x.StackMaxSize).OrderByDescending(x => x.StackObjectsCount).ToList())
                {
                    if (item.StackObjectsCount <= 0)
                    {
                        Logger.LogDebug($"Removing empty stack of {item.Name.Localized()}");
                        sortingItemGrid.Remove(item);
                        continue;
                    }

                    Logger.LogDebug($"Topping up {item.Name.Localized()} - {item.StackObjectsCount} of {item.StackMaxSize}");
                    EFT.UI.ItemUiContext.Instance.TopUpItem(item);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }
}