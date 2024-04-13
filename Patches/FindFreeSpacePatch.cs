using Aki.Reflection.Patching;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StashManagementHelper;

public class FindFreeSpacePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(StashGridClass), "FindFreeSpace");

    public static StashGridClass Instance { get; set; }
    public static List<bool> List0 { get; set; }
    public static List<int> List1 { get; set; }
    public static List<int> List2 { get; set; }

    [PatchPrefix]
    private static bool PatchPrefix(StashGridClass __instance, ref LocationInGrid __result, Item item, List<bool> ___list_0, List<int> ___list_1, List<int> ___list_2)
    {
        Instance = __instance;
        List0 = ___list_0;
        List1 = ___list_1;
        List2 = ___list_2;

        var gridHeight = Instance.GridHeight.Value;
        var skipRows = Math.Max(0, Math.Min(gridHeight - Settings.SkipRows.Value, Settings.SkipRows.Value));
        if (!Settings.Sorting || Instance.ID != "hideout")
        {
            skipRows = 0;
        }

        if (!__instance.Filters.CheckItemFilter(item))
        {
            __result = null;
            return false;
        }

        var locationInGrid = (LocationInGrid)null;
        var num = __instance.Contains(item) ? 1 : 0;

        if (num != 0)
        {
            locationInGrid = __instance.ItemCollection[item];
            __instance.ItemCollection.Remove(item, __instance);
            __instance.SetLayout(item, locationInGrid, false);
            method_12();
        }

        var cellSize = item.CalculateCellSize();
        var freeSpaceHorizontal = method_10(cellSize.X, cellSize.Y, ItemRotation.Horizontal, skipRows);
        LocationInGrid freeSpaceVertical = null;

        if (Settings.RotateItems.Value || freeSpaceHorizontal is null)
        {
            freeSpaceVertical = method_10(cellSize.Y, cellSize.X, ItemRotation.Vertical, skipRows);
        }

        var freeSpace = freeSpaceHorizontal?.y <= freeSpaceVertical?.y ? freeSpaceHorizontal : freeSpaceVertical ?? freeSpaceHorizontal;

        if (num == 0)
        {
            __result = freeSpace;
            return false;
        }

        __instance.ItemCollection.Add(item, __instance, locationInGrid);
        __instance.SetLayout(item, locationInGrid, true);
        method_12();
        __result = freeSpace;
        Logger.LogInfo($"{item.Name.Localized()} location: {__result}");
        return false;
    }

    protected static LocationInGrid method_10(int itemWidth, int itemHeight, ItemRotation rotation, int skipRows)
    {
        var locationInGrid = !Instance.CanStretchHorizontally || Instance.GridHeight.Value < Instance.GridWidth.Value + itemWidth ?
            FindSuitableLocation(itemHeight, itemWidth, rotation, Instance.GridHeight.Value, Instance.GridWidth.Value, List2, List1, skipRows) :
            FindSuitableLocation(itemWidth, itemHeight, rotation, Instance.GridWidth.Value, Instance.GridHeight.Value, List1, List2, skipRows, true);

        if (locationInGrid != null)
            return locationInGrid;

        if (Instance.CanStretchHorizontally && Instance.GridHeight.Value >= Instance.GridWidth.Value + itemWidth)
            return new LocationInGrid(Instance.GridWidth.Value, 0, rotation);

        return Instance.CanStretchVertically && (Instance.CanStretchHorizontally || itemWidth <= Instance.GridWidth.Value) ? new LocationInGrid(0, Instance.GridHeight.Value, rotation) : (LocationInGrid)null;
    }

    private static LocationInGrid FindSuitableLocation(
        int itemMainDimensionSize,
        int itemSecondaryDimensionSize,
        ItemRotation rotation,
        int gridMainDimensionSize,
        int gridSecondaryDimensionSize,
        IReadOnlyList<int> mainDimensionSpaces,
        IReadOnlyList<int> secondaryDimensionSpaces,
        int skipRows,
        bool invertDimensions = false)
    {
        var mainOffset = invertDimensions ? 0 : 0 + skipRows;
        var secondaryOffset = invertDimensions ? 0 + skipRows : 0;

        for (var mainIndex = mainOffset; mainIndex < gridMainDimensionSize; ++mainIndex)
        {
            for (var secondaryIndex = secondaryOffset; secondaryIndex < gridSecondaryDimensionSize && secondaryIndex + itemSecondaryDimensionSize <= gridSecondaryDimensionSize; ++secondaryIndex)
            {
                var availableSecondarySpace = invertDimensions ? secondaryDimensionSpaces[secondaryIndex * gridMainDimensionSize + mainIndex] : secondaryDimensionSpaces[mainIndex * gridSecondaryDimensionSize + secondaryIndex];
                if (availableSecondarySpace >= itemSecondaryDimensionSize || availableSecondarySpace == -1)
                {
                    var isSpaceSuitable = true;
                    for (var index = secondaryIndex; isSpaceSuitable && index < secondaryIndex + itemSecondaryDimensionSize; ++index)
                    {
                        var availableMainSpace = invertDimensions ? mainDimensionSpaces[index * gridMainDimensionSize + mainIndex] : mainDimensionSpaces[mainIndex * gridSecondaryDimensionSize + index];
                        isSpaceSuitable = availableMainSpace >= itemMainDimensionSize || availableMainSpace == -1;
                    }
                    if (isSpaceSuitable)
                        return !invertDimensions ? new LocationInGrid(secondaryIndex, mainIndex, rotation) : new LocationInGrid(mainIndex, secondaryIndex, rotation);
                }
            }
        }
        return null;
    }

    protected static void method_12()
    {
        var gridHeight = Instance.GridHeight.Value;
        var gridWidth = Instance.GridWidth.Value;
        var skipRows = Math.Max(0, Math.Min(gridHeight - Settings.SkipRows.Value, Settings.SkipRows.Value));

        if (!Settings.Sorting || skipRows == 0 || Instance.ID != "hideout")
            return;
        try
        {
            CalculateHorizontalSpace(Instance, List0, List1, gridHeight, gridWidth, skipRows);
            CalculateVerticalSpace(Instance, List0, List2, gridHeight, gridWidth, skipRows);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    private static void CalculateHorizontalSpace(StashGridClass __instance, IReadOnlyList<bool> list_0, IList<int> list_1, int gridHeight, int gridWidth, int skipRows)
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

    private static void CalculateVerticalSpace(StashGridClass __instance, IReadOnlyList<bool> list_0, IList<int> list_2, int gridHeight, int gridWidth, int skipRows)
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
