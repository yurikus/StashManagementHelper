using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace StashManagementHelper;

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

// For finding GClass

//public class ItemListSortPatch : ModulePatch
//{
//    protected override MethodBase GetTargetMethod()
//    {
//        // Find all types in the assembly that contains Item class
//        var assembly = typeof(Item).Assembly;

//        // Look for any public/internal type that has a Sort method with the correct signature
//        foreach (var type in assembly.GetTypes())
//        {
//            var method = AccessTools.Method(type, "Sort", new[] { typeof(IEnumerable<Item>) });
//            if (method != null)
//            {
//                Logger.LogInfo($"Found Sort method in class: {type.FullName}");
//                return method;
//            }
//        }

//        Logger.LogError("Could not find the Sort method for items. Patch will not be applied.");
//        return null;
//    }

//    [PatchPostfix]
//    private static void PatchPostfix(ref IEnumerable<Item> __result)
//    {
//        __result = __result.Sort();
//    }
//}