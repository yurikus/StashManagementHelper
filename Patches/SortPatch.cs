using Aki.Reflection.Patching;
using HarmonyLib;
using System;
using System.Reflection;

namespace StashManagementHelper;

public class SortPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(GClass2585), "Sort");

    [PatchPrefix]
    private static async void PatchPrefix(GStruct375<GClass2619> __result, LootItemClass sortingItem, InventoryControllerClass controller, bool simulate)
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
            Console.WriteLine(e);
        }
    }

    [PatchPostfix]
    private static void PatchPostfix()
    {
        Settings.Sorting = false;
    }
}