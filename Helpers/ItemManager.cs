using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT.InventoryLogic;

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
        try
        {
            foreach (var grid in items.Grids.OrderBy(g => g.GridHeight * g.GridWidth))
            {
                var stackableGroups = grid.Items
                    .Where(i => i.StackObjectsCount < i.StackMaxSize)
                    .GroupBy(i => new { i.TemplateId, i.SpawnedInSession })
                    .ToList();

                foreach (var group in stackableGroups)
                {
                    var stackables = group.OrderByDescending(i => i.StackObjectsCount).ToList();

                    for (int i = 0; i < stackables.Count - 1; i++)
                    {
                        var targetItem = stackables[i];
                        if (targetItem.StackObjectsCount >= targetItem.StackMaxSize) continue;

                        for (int j = stackables.Count - 1; j > i; j--)
                        {
                            var sourceItem = stackables[j];
                            var spaceAvailable = targetItem.StackMaxSize - targetItem.StackObjectsCount;
                            var amountToMove = Math.Min(spaceAvailable, sourceItem.StackObjectsCount);

                            Logger.LogDebug($"Merging {sourceItem.Name.Localized()} ({amountToMove}) into {targetItem.Name.Localized()} ({spaceAvailable})");

                            targetItem.StackObjectsCount += amountToMove;
                            sourceItem.StackObjectsCount -= amountToMove;

                            if (sourceItem.StackObjectsCount <= 0)
                            {
                                Logger.LogDebug($"Removing empty stack of {sourceItem.Name.Localized()}");
                                grid.Remove(sourceItem, false);
                                stackables.RemoveAt(j);
                            }

                            if (targetItem.StackObjectsCount >= targetItem.StackMaxSize) break;
                        }
                    }
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