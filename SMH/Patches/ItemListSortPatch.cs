using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SMH;

public class ItemListSortPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GClass3381), nameof(GClass3381.Sort), [typeof(IEnumerable<Item>)]);
    }

    [PatchPostfix]
    private static void PatchPostfix(ref IEnumerable<Item> __result)
    {
        if (Settings.Sorting)
        {
            __result = __result.Sort();
        }
    }
}
