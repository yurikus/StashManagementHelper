using Aki.Reflection.Patching;
using HarmonyLib;
using System;
using System.Reflection;

namespace StashManagementHelper;

public class SortPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(InteractionsHandlerClass), "Sort");

    [PatchPrefix]
    private static async void PatchPrefix(LootItemClass sortingItem, InventoryControllerClass controller, bool simulate)
    {
        try
        {
            Settings.Sorting = true;

            // Merge separate stacks of the same item
            if (Settings.MergeItems.Value)
            {
                ItemManager.MergeSortingItems(sortingItem);
            }

            // Fold weapons to take up less space
            if (Settings.FoldItems.Value)
            {
                await ItemManager.FoldSortingItems(sortingItem, controller, simulate);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
        }
    }

    [PatchPostfix]
    private static void PatchPostfix()
    {
        Settings.Sorting = false;
    }
}