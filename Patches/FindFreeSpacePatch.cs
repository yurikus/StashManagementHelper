using Aki.Reflection.Patching;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StashManagementHelper;

public class FindFreeSpacePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(GClass2318), "FindFreeSpace");

    [PatchPrefix]
    private static void PatchPrefix(GClass2318 __instance, List<bool> ___list_0, List<int> ___list_1, List<int> ___list_2)
    {
        var gridHeight = __instance.GridHeight.Value;
        var gridWidth = __instance.GridWidth.Value;
        var skipRows = Math.Max(0, Math.Min(gridHeight - Settings.SkipRows.Value, Settings.SkipRows.Value));

        if (!Settings.Sorting || skipRows == 0 || __instance.ID != "hideout")
            return;
        try
        {
            CalculateHorizontalSpace(__instance, ___list_0, ___list_1, gridHeight, gridWidth, skipRows);
            CalculateVerticalSpace(__instance, ___list_0, ___list_2, gridHeight, gridWidth, skipRows);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    private static void CalculateHorizontalSpace(GClass2318 __instance, IReadOnlyList<bool> list_0, IList<int> list_1, int gridHeight, int gridWidth, int skipRows)
    {
        for (var row = 0; row < gridHeight - skipRows; ++row)
        {
            var num = __instance.CanStretchHorizontally ? -1 : 0;
            for (var col = gridWidth - 1; col >= 0; --col)
            {
                var index = row * gridWidth + col;
                if (row < skipRows)
                    list_1[index] = 0;
                else
                {
                    if (list_0[index])
                        num = 0;
                    else if (num != -1)
                        ++num;
                    list_1[index] = num;
                }
            }
        }
    }

    private static void CalculateVerticalSpace(GClass2318 __instance, IReadOnlyList<bool> list_0, IList<int> list_2, int gridHeight, int gridWidth, int skipRows)
    {
        for (var col = 0; col < gridWidth; ++col)
        {
            var num = __instance.CanStretchVertically ? -1 : 0;
            for (var row = gridHeight - 1 - skipRows; row >= 0; --row)
            {
                var index = row * gridWidth + col;
                if (row < skipRows)
                    list_2[index] = 0;
                else
                {
                    if (list_0[index])
                        num = 0;
                    else if (num != -1)
                        ++num;
                    list_2[index] = num;
                }
            }
        }
    }
}
