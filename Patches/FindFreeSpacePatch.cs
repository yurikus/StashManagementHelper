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

        var skipRows = Math.Max(0, Math.Min(Instance.GridHeight.Value - Settings.SkipRows.Value, Settings.SkipRows.Value));
        if (!Settings.Sorting || Instance.ID != "hideout")
        {
            skipRows = 0;
        }

        if (!__instance.Filters.CheckItemFilter(item))
        {
            __result = null;
            return false;
        }

        var itemExists = __instance.Contains(item);
        var locationInGrid = itemExists ? __instance.ItemCollection[item] : null;

        if (itemExists)
        {
            __instance.ItemCollection.Remove(item, __instance);
            __instance.SetLayout(item, locationInGrid, false);
            UpdateGridSpaces();
        }

        var cellSize = item.CalculateCellSize();
        var freeSpace = DetermineBestPlacement(cellSize, skipRows);

        if (!itemExists)
        {
            __result = freeSpace;
            return false;
        }

        __instance.ItemCollection.Add(item, __instance, locationInGrid);
        __instance.SetLayout(item, locationInGrid, true);
        UpdateGridSpaces();
        __result = freeSpace;
        Logger.LogInfo($"{item.Name.Localized()} location: {__result}");
        return false;
    }

    private static LocationInGrid DetermineBestPlacement(GStruct24 cellSize, int skipRows)
    {
        var freeSpaceHorizontal = FindOptimalItemPlacement(cellSize.X, cellSize.Y, ItemRotation.Horizontal, skipRows);
        var freeSpaceVertical = Settings.RotateItems.Value || freeSpaceHorizontal == null
            ? FindOptimalItemPlacement(cellSize.Y, cellSize.X, ItemRotation.Vertical, skipRows)
            : null;

        return freeSpaceHorizontal != null && (freeSpaceVertical == null || freeSpaceHorizontal.y <= freeSpaceVertical.y)
            ? freeSpaceHorizontal
            : freeSpaceVertical;
    }

    protected static LocationInGrid FindOptimalItemPlacement(int itemWidth, int itemHeight, ItemRotation rotation, int skipRows)
    {
        // Determine the primary and secondary dimensions based on the ability to stretch horizontally
        var useInvertedDimensions = Instance.CanStretchHorizontally && Instance.GridHeight.Value >= Instance.GridWidth.Value + itemWidth;
        var primaryWidth = useInvertedDimensions ? itemWidth : itemHeight;
        var primaryHeight = useInvertedDimensions ? itemHeight : itemWidth;
        var primaryGridWidth = useInvertedDimensions ? Instance.GridWidth.Value : Instance.GridHeight.Value;
        var primaryGridHeight = useInvertedDimensions ? Instance.GridHeight.Value : Instance.GridWidth.Value;
        var primaryList = useInvertedDimensions ? List1 : List2;
        var secondaryList = useInvertedDimensions ? List2 : List1;

        // Attempt to find a suitable location with the current orientation
        var locationInGrid = FindSuitableLocation(primaryWidth, primaryHeight, rotation, primaryGridWidth, primaryGridHeight, primaryList, secondaryList, skipRows, useInvertedDimensions);
        if (locationInGrid != null)
            return locationInGrid;

        // Check if stretching is possible and return a new location accordingly
        if (Instance.CanStretchHorizontally && Instance.GridHeight.Value >= Instance.GridWidth.Value + itemWidth)
            return new LocationInGrid(Instance.GridWidth.Value, 0, rotation);

        if (Instance.CanStretchVertically && (Instance.CanStretchHorizontally || itemWidth <= Instance.GridWidth.Value))
            return new LocationInGrid(0, Instance.GridHeight.Value, rotation);

        return null;
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
        var mainStartIndex = Settings.FlipSortDirection.Value ? gridMainDimensionSize - skipRows - itemMainDimensionSize : skipRows;

        var mainEndIndex = Settings.FlipSortDirection.Value ? skipRows : gridMainDimensionSize - itemMainDimensionSize;
        var step = Settings.FlipSortDirection.Value ? -1 : 1;

        for (var mainIndex = mainStartIndex; Settings.FlipSortDirection.Value ? mainIndex >= mainEndIndex : mainIndex <= mainEndIndex; mainIndex += step)
        {
            for (var secondaryIndex = 0; secondaryIndex + itemSecondaryDimensionSize <= gridSecondaryDimensionSize; ++secondaryIndex)
            {
                if (IsSpaceAvailable(mainIndex, secondaryIndex, itemMainDimensionSize, itemSecondaryDimensionSize, gridMainDimensionSize, gridSecondaryDimensionSize, mainDimensionSpaces, secondaryDimensionSpaces, invertDimensions))
                {
                    return new LocationInGrid(invertDimensions ? mainIndex : secondaryIndex, invertDimensions ? secondaryIndex : mainIndex, rotation);
                }
            }
        }
        return null;
    }

    private static bool IsSpaceAvailable(
        int mainIndex,
        int secondaryIndex,
        int itemMainDimensionSize,
        int itemSecondaryDimensionSize,
        int gridMainDimensionSize,
        int gridSecondaryDimensionSize,
        IReadOnlyList<int> mainDimensionSpaces,
        IReadOnlyList<int> secondaryDimensionSpaces,
        bool invertDimensions)
    {
        var availableSecondarySpace = invertDimensions ? secondaryDimensionSpaces[secondaryIndex * gridMainDimensionSize + mainIndex] : secondaryDimensionSpaces[mainIndex * gridSecondaryDimensionSize + secondaryIndex];
        if (availableSecondarySpace < itemSecondaryDimensionSize && availableSecondarySpace != -1)
        {
            return false;
        }

        for (var index = secondaryIndex; index < secondaryIndex + itemSecondaryDimensionSize; ++index)
        {
            var availableMainSpace = invertDimensions ? mainDimensionSpaces[index * gridMainDimensionSize + mainIndex] : mainDimensionSpaces[mainIndex * gridSecondaryDimensionSize + index];
            if (availableMainSpace < itemMainDimensionSize && availableMainSpace != -1)
            {
                return false;
            }
        }
        return true;
    }

    protected static void UpdateGridSpaces()
    {
        var gridHeight = Instance.GridHeight.Value;
        var gridWidth = Instance.GridWidth.Value;
        var skipRows = Math.Max(0, Math.Min(gridHeight - Settings.SkipRows.Value, Settings.SkipRows.Value));

        if (!Settings.Sorting || skipRows == 0 || Instance.ID != "hideout")
        {
            return;
        }

        try
        {
            CalculateHorizontalSpace(Instance, List0, List1, gridHeight, gridWidth, skipRows);
            CalculateVerticalSpace(Instance, List0, List2, gridHeight, gridWidth, skipRows);
        }
        catch (Exception e)
        {
            Logger.LogError($"Error updating grid spaces: {e}");
        }
    }

    private static void CalculateSpace(StashGridClass __instance, IReadOnlyList<bool> list_0, IList<int> list_1, int gridHeight, int gridWidth, int skipRows, bool isHorizontal)
    {
        var outerLimit = isHorizontal ? gridHeight : gridWidth;
        var innerLimit = isHorizontal ? gridWidth : gridHeight;

        for (var outer = 0; outer < outerLimit - skipRows; ++outer)
        {
            var num = (isHorizontal ? __instance.CanStretchHorizontally : __instance.CanStretchVertically) ? -1 : 0;
            for (var inner = innerLimit - 1; inner >= 0; --inner)
            {
                var index = isHorizontal ? outer * gridWidth + inner : inner * gridWidth + outer;
                if (outer < skipRows)
                {
                    list_1[index] = 0;
                }
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

    private static void CalculateHorizontalSpace(StashGridClass __instance, IReadOnlyList<bool> list_0, IList<int> list_1, int gridHeight, int gridWidth, int skipRows)
    {
        CalculateSpace(__instance, list_0, list_1, gridHeight, gridWidth, skipRows, true);
    }

    private static void CalculateVerticalSpace(StashGridClass __instance, IReadOnlyList<bool> list_0, IList<int> list_2, int gridHeight, int gridWidth, int skipRows)
    {
        CalculateSpace(__instance, list_0, list_2, gridHeight, gridWidth, skipRows, false);
    }
}
