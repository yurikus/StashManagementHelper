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
        var skipRows = Math.Min(Math.Max(0, __instance.GridHeight.Value - Settings.SkipRows.Value), Settings.SkipRows.Value);

        if (!Settings.Sorting || skipRows == 0 || __instance.ID != "hideout")
            return;

        try
        {
            for (var index1 = 0; index1 < __instance.GridHeight.Value - skipRows; ++index1)
            {
                var num = __instance.CanStretchHorizontally ? -1 : 0;
                for (var index2 = __instance.GridWidth.Value - 1; index2 >= 0; --index2)
                {
                    if (index1 < skipRows)
                        ___list_1[index1 * __instance.GridWidth.Value + index2] = 0;
                    else
                    {
                        if (___list_0[index1 * __instance.GridWidth.Value + index2])
                            num = 0;
                        else if (num != -1)
                            ++num;
                        ___list_1[index1 * __instance.GridWidth.Value + index2] = num;
                    }
                }
            }

            for (var index3 = 0; index3 < __instance.GridWidth.Value; ++index3)
            {
                var num = __instance.CanStretchVertically ? -1 : 0;
                for (var index4 = __instance.GridHeight.Value - 1 - skipRows; index4 >= 0; --index4)
                {
                    if (index4 < skipRows)
                        ___list_2[index4 * __instance.GridWidth.Value + index3] = 0;
                    else
                    {
                        if (___list_0[index4 * __instance.GridWidth.Value + index3])
                            num = 0;
                        else if (num != -1)
                            ++num;
                        ___list_2[index4 * __instance.GridWidth.Value + index3] = num;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }
}