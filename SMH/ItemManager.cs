using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT.InventoryLogic;

namespace StashManagementHelper;

public static class ItemManager
{
    public static ManualLogSource Log { get; set; }

    /// <summary>
    /// Folds every weapon in the list to take up less space.
    /// </summary>
    /// <param name="items">The table of items.</param>
    /// <param name="inventoryController">The Inventory controller class.</param>
    /// <param name="simulate">Flag to simulate the operation without actual changes.</param>
    public static async Task FoldItemsAsync(CompoundItem items, InventoryController inventoryController, bool simulate)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (inventoryController == null)
            throw new ArgumentNullException(nameof(inventoryController));

        try
        {
            foreach (var grid in items.Grids.OrderBy(g => g.GridHeight * g.GridWidth))
            {
                foreach (var item in grid.Items)
                {
                    if (!InteractionsHandlerClass.CanFold(item, out var foldable) || foldable?.Folded == true)
                        continue;

                    Log.LogDebug($"Folding {item.Name.Localized()}");

                    await inventoryController.TryRunNetworkTransaction(InteractionsHandlerClass.Fold(foldable, true, simulate));
                }
            }
        }
        catch (Exception e)
        {
            Log.LogError($"Error folding items: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Merge separate stacks of the same item.
    /// </summary>
    /// <param name="items">The table of items.</param>
    public static async Task MergeItems(CompoundItem items, InventoryController inventoryController, bool simulate)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (inventoryController == null)
            throw new ArgumentNullException(nameof(inventoryController));

        try
        {
            foreach (var grid in items.Grids)
            {
                var stackableGroups = StackUtils.GetStackableGroups(grid);

                foreach (var group in stackableGroups)
                {
                    bool mergesMade;

                    do
                    {
                        mergesMade = false;

                        var stacks = StackUtils.GetSortedStacks(group);

                        if (stacks.Count <= 1)
                            break;

                        var target = stacks.FirstOrDefault(s => s.Owner != null && s.StackObjectsCount < s.StackMaxSize);
                        if (target == null)
                            break;

                        var src = stacks.Last();
                        if (src == target)
                            break;

                        Log.LogDebug($"Merging {src.Name.Localized()} ({src.StackObjectsCount}) into {target.Name.Localized()} ({target.StackObjectsCount})");

                        await inventoryController.TryRunNetworkTransaction(
                            InteractionsHandlerClass.TransferOrMerge(src, target, inventoryController, simulate));

                        mergesMade = true;

                    } while (mergesMade);
                }
            }
        }
        catch (Exception e)
        {
            Log.LogError($"Error merging items: {e.Message}");
            throw;
        }
    }

    const string StashItemId = "hideout";
    const string StashTemplateId = "566abbc34bdc2d92178b4576";

    /// <summary>
    /// Checks if an item is located in the player's stash
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns>True if the item is in the player stash, false otherwise</returns>
    public static bool IsItemInStash(Item item)
    {
        if (item is null)
            return false;

        if (item is StashItemClass
            || string.Equals(item.TemplateId, StashTemplateId, StringComparison.Ordinal)
            || string.Equals(item.Owner?.ID, StashItemId, StringComparison.Ordinal))
        {
            return true;
        }

        try
        {
            foreach (var parent in item.GetAllParentItems())
            {
                if (parent is StashItemClass
                    || string.Equals(parent.TemplateId, StashTemplateId, StringComparison.Ordinal)
                    || string.Equals(parent.Owner?.ID, StashItemId, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogError($"Error checking stash parents: {ex.Message}");
        }

        return false;
    }
}