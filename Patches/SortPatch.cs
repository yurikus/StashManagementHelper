using SPT.Reflection.Patching;
using HarmonyLib;
using System;
using System.Reflection;
using EFT.InventoryLogic;

namespace StashManagementHelper;

public class SortPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(InteractionsHandlerClass), "Sort");

    [PatchPrefix]
    private static async void PatchPrefix(CompoundItem sortedItem, InventoryController controller, bool simulate)
    {
        try
        {
            Settings.Sorting = true;

            // Merge separate stacks of the same item
            if (Settings.MergeItems.Value)
            {
                ItemManager.MergeItems(sortedItem);
            }

            // Fold weapons to take up less space
            if (Settings.FoldItems.Value)
            {
                await ItemManager.FoldItemsAsync(sortedItem, controller, simulate);
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