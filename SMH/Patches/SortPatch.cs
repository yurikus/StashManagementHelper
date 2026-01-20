using System;
using System.Reflection;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SMH;

public class SortPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InteractionsHandlerClass), "Sort");
    }

    [PatchPrefix]
    private static async void PatchPrefix(CompoundItem sortedItem, InventoryController controller, bool simulate)
    {
        try
        {
            if (!ItemManager.IsItemInStash(sortedItem))
            {
                ItemManager.Log.LogDebug($"Skipping custom sorting in {sortedItem.Template._name} - not in hideout stash container");
                return;
            }

            Settings.Sorting = true;
        }
        catch (Exception e)
        {
            ItemManager.Log.LogError(e.Message);
        }

        await Task.CompletedTask;
    }

    [PatchPostfix]
    private static void PatchPostfix()
    {
        Settings.Sorting = false;
    }
}