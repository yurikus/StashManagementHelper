using System;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

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
                await ItemManager.MergeItems(sortedItem, controller, simulate);
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