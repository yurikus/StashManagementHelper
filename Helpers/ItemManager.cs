using BepInEx.Logging;
using EFT.InventoryLogic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StashManagementHelper;

public static class ItemManager
{
    public static ManualLogSource Logger { get; set; }

    /// <summary>
    /// Folds every weapon in the list to take up less space.
    /// </summary>
    /// <param name="items">The table of items.</param>
    /// <param name="inventoryController">The Inventory controller class.</param>
    /// <param name="simulate">Flag to simulate the operation without actual changes.</param>
    public static async Task FoldItemsAsync(CompoundItem items, InventoryController inventoryController, bool simulate)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (inventoryController == null) throw new ArgumentNullException(nameof(inventoryController));

        try
        {
            foreach (var grid in items.Grids.OrderBy(g => g.GridHeight * g.GridWidth))
            {
                foreach (var item in grid.Items)
                {
                    if (!InteractionsHandlerClass.CanFold(item, out var foldable) || foldable?.Folded == true) continue;

                    Logger.LogDebug($"Folding {item.Name.Localized()}");
                    await inventoryController.TryRunNetworkTransaction(InteractionsHandlerClass.Fold(foldable, true, simulate));
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"Error folding items: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Merge separate stacks of the same item.
    /// </summary>
    /// <param name="items">The table of items.</param>
    public static void MergeItems(CompoundItem items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        try
        {
            foreach (var grid in items.Grids.OrderBy(g => g.GridHeight * g.GridWidth))
            {
                var itemsToMerge = grid.Items.Where(i => i.StackObjectsCount < i.StackMaxSize).OrderByDescending(i => i.StackObjectsCount).ToList();
                foreach (var item in itemsToMerge)
                {
                    if (item.StackObjectsCount <= 0)
                    {
                        Logger.LogDebug($"Removing empty stack of {item.Name.Localized()}");
                        grid.Remove(item, false);
                        continue;
                    }

                    Logger.LogDebug($"Topping up {item.Name.Localized()} - {item.StackObjectsCount} of {item.StackMaxSize}");
                    EFT.UI.ItemUiContext.Instance.TopUpItem(item);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"Error merging items: {e.Message}");
            throw;
        }
    }
}