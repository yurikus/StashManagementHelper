using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT.InventoryLogic;

namespace StashManagementHelper;

public static class ItemManager
{
    private const string StashItemId = "hideout";
    private const string StashTemplateId = "566abbc34bdc2d92178b4576";

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
    public static async Task MergeItems(CompoundItem items, InventoryController inventoryController, bool simulate)
    {
        try
        {
            foreach (var grid in items.Grids)
            {
                var stackableGroups = grid.Items
                    .Where(i => i.StackObjectsCount < i.StackMaxSize)
                    .Where(i => i.Owner != null)
                    .GroupBy(i => new { i.TemplateId, i.SpawnedInSession })
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var group in stackableGroups)
                {
                    bool mergesMade;
                    do
                    {
                        mergesMade = false;

                        var stacks = group.OrderByDescending(i => i.StackObjectsCount)
                            .Where(i => i.StackObjectsCount > 0)
                            .ToList();

                        if (stacks.Count <= 1) break;

                        var targetStack = stacks.FirstOrDefault(s => s.StackObjectsCount < s.StackMaxSize);
                        if (targetStack == null) break;

                        var sourceStack = stacks.Last();
                        if (sourceStack == targetStack) break;

                        Logger.LogDebug($"Merging {sourceStack.Name.Localized()} ({sourceStack.StackObjectsCount}) into {targetStack.Name.Localized()} ({targetStack.StackObjectsCount})");
                        await inventoryController.TryRunNetworkTransaction(InteractionsHandlerClass.TransferOrMerge(sourceStack, targetStack, inventoryController, simulate));

                        mergesMade = true;
                    } while (mergesMade);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"Error merging items: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Checks if an item is located in the player's stash
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns>True if the item is in the player stash, false otherwise</returns>
    public static bool IsItemInStash(Item item)
    {
        if (item == null) return false;
        if (item is StashItemClass) return true;
        if (item.TemplateId == StashTemplateId) return true;
        if (item.Owner?.ID == StashItemId) return true;

        try
        {
            foreach (var parentItem in item.GetAllParentItems())
            {
                if (parentItem.TemplateId == StashTemplateId || parentItem is StashItemClass) return true;
                if (parentItem.Owner?.ID == StashItemId) return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error checking stash parents: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Checks if item is in trader window
    /// </summary>
    /// <param name="item"></param>
    public static bool IsItemInTrader(Item item)
    {
        if (item?.Owner == null) return false;
        return item.Owner.OwnerType == EOwnerType.Trader;
    }
}