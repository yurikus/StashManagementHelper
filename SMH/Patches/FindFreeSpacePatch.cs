using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace StashManagementHelper;

public class FindFreeSpacePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(StashGridClass), nameof(StashGridClass.FindFreeSpace));
    }

    public static StashGridClass Instance { get; set; }
    public static List<bool> List0 { get; set; }
    public static List<int> HorizontalSpaceList { get; set; }
    public static List<int> VerticalSpaceList { get; set; }

    /// <summary>
    /// Path for FindFreeSpace method of <see cref="StashGridClass"/>
    /// </summary>
    /// <param name="__instance">Instance of <see cref="StashGridClass"/></param>
    /// <param name="__result">The <see cref="LocationInGrid"/></param>
    /// <param name="item"><see cref="Item"/> to sort</param>
    /// <param name="___List_0">Indicates of which square in grid is occupied by index</param>
    /// <param name="___List_1">Number of free space horizontally from current square by index</param>
    /// <param name="___List_2">Number of free space vertically from current square by index</param>
    [PatchPrefix]
    private static bool PatchPrefix(StashGridClass __instance, ref LocationInGrid __result, Item item, List<bool> ___List_0, List<int> ___List_1, List<int> ___List_2)
    {
        Instance = __instance;
        List0 = ___List_0;
        HorizontalSpaceList = ___List_1;
        VerticalSpaceList = ___List_2;

        var skipRows = Math.Max(0, Math.Min(
            Instance.GridHeight - Settings.SkipRows.Value,
            Settings.SkipRows.Value));

        if (!Settings.Sorting || Instance.ID != "hideout")
            return true;

        // Reject item if it cannot be accepted by the instance
        if (!__instance.CanAccept(item))
        {
            __result = null;
            return false;
        }

        // Attempt to find an existing location for the item
        var locationInGrid = __instance.GetItemLocation(item);

        // If found, remove the item temporarily to update the grid
        if (locationInGrid is not null)
        {
            __instance.ItemCollection.Remove(item, __instance);
            __instance.Add(item, locationInGrid, false);
            UpdateGridSpaces();
        }

        // Calculate the optimal placement for the item considering the cell size and skipped rows
        var cellSize = item.CalculateCellSize();
        var freeSpace = DetermineBestPlacement(cellSize, skipRows);

        // If no existing location, set the new location
        if (locationInGrid is null)
        {
            __result = freeSpace;
            return false;
        }

        // Add the item back to the collection at the new or existing location
        __instance.ItemCollection.Add(item, __instance, locationInGrid);
        __instance.Add(item, locationInGrid, true);

        UpdateGridSpaces();

        __result = freeSpace;

        ItemManager.Log.LogInfo($"{item.Name.Localized()} location: {__result}");

        return false;
    }

    /// <summary>
    /// Determines the best placement for an item based on its size and the number of rows to skip.
    /// </summary>
    private static LocationInGrid DetermineBestPlacement(XYCellSizeStruct cellSize, int skipRows)
    {
        // Evaluate both horizontal and vertical placements and choose the best one
        var freeSpaceHorizontal = FindOptimalItemPlacement(cellSize.X, cellSize.Y, ItemRotation.Horizontal, skipRows);
        var freeSpaceVertical = Settings.RotateItems.Value || freeSpaceHorizontal == null
            ? FindOptimalItemPlacement(cellSize.Y, cellSize.X, ItemRotation.Vertical, skipRows)
            : null;

        // Compare placements based on settings and return the most suitable one
        return freeSpaceHorizontal != null && (freeSpaceVertical == null || (Settings.FlipSortDirection.Value
            ? freeSpaceHorizontal.y >= freeSpaceVertical.y : freeSpaceHorizontal.y <= freeSpaceVertical.y))
            ? freeSpaceHorizontal : freeSpaceVertical;
    }

    /// <summary>
    /// Finds the optimal item placement considering item dimensions, rotation, and grid constraints.
    /// </summary>
    protected static LocationInGrid FindOptimalItemPlacement(int itemWidth, int itemHeight, ItemRotation rotation, int skipRows)
    {
        // Determine the primary and secondary dimensions based on the ability to stretch horizontally
        var useInvertedDimensions = Instance.CanStretchHorizontally && Instance.GridHeight >= Instance.GridWidth + itemWidth;
        var primaryWidth = useInvertedDimensions ? itemWidth : itemHeight;
        var primaryHeight = useInvertedDimensions ? itemHeight : itemWidth;
        var primaryGridWidth = useInvertedDimensions ? Instance.GridWidth : Instance.GridHeight;
        var primaryGridHeight = useInvertedDimensions ? Instance.GridHeight : Instance.GridWidth;
        var primaryList = useInvertedDimensions ? HorizontalSpaceList : VerticalSpaceList;
        var secondaryList = useInvertedDimensions ? VerticalSpaceList : HorizontalSpaceList;

        // Attempt to find a suitable location with the current orientation
        var locationInGrid = FindSuitableLocation(
            primaryWidth,
            primaryHeight,
            rotation,
            primaryGridWidth,
            primaryGridHeight,
            primaryList,
            secondaryList,
            skipRows,
            useInvertedDimensions);

        if (locationInGrid != null)
            return locationInGrid;

        // Check if stretching is possible and return a new location accordingly
        if (Instance.CanStretchHorizontally && Instance.GridHeight >= Instance.GridWidth + itemWidth)
            return new LocationInGrid(Instance.GridWidth, 0, rotation);

        if (Instance.CanStretchVertically && (Instance.CanStretchHorizontally || itemWidth <= Instance.GridWidth))
            return new LocationInGrid(0, Instance.GridHeight, rotation);

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
        // Determine starting and ending indices for the main dimension based on sorting direction
        var mainStartIndex = Settings.FlipSortDirection.Value ? gridMainDimensionSize - itemMainDimensionSize - skipRows : skipRows;
        var mainEndIndex = Settings.FlipSortDirection.Value ? 0 : gridMainDimensionSize - itemMainDimensionSize;
        var step = Settings.FlipSortDirection.Value ? -1 : 1;

        // Iterate over possible positions in the grid to find a suitable location for the item
        for (var mainIndex = mainStartIndex; Settings.FlipSortDirection.Value ? mainIndex >= mainEndIndex : mainIndex <= mainEndIndex; mainIndex += step)
        {
            var secondaryStart = Settings.FlipSortDirection.Value ? gridSecondaryDimensionSize - itemSecondaryDimensionSize : 0;
            var secondaryEnd = Settings.FlipSortDirection.Value ? 0 : gridSecondaryDimensionSize - itemSecondaryDimensionSize;
            var secondaryStep = Settings.FlipSortDirection.Value ? -1 : 1;

            for (var secondaryIndex = secondaryStart;
                Settings.FlipSortDirection.Value ? secondaryIndex >= secondaryEnd : secondaryIndex <= secondaryEnd; 
                secondaryIndex += secondaryStep)
            {
                // Check if the current position has enough space for the item
                if (IsSpaceAvailable(
                    mainIndex,
                    secondaryIndex,
                    itemMainDimensionSize,
                    itemSecondaryDimensionSize,
                    gridMainDimensionSize,
                    gridSecondaryDimensionSize,
                    mainDimensionSpaces,
                    secondaryDimensionSpaces,
                    invertDimensions))
                {
                    // Return the location if space is available
                    return new LocationInGrid(
                        invertDimensions ? mainIndex : secondaryIndex,
                        invertDimensions ? secondaryIndex : mainIndex, rotation);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if the specified space in the grid is free for the item placement.
    /// </summary>
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
        // Check if the secondary dimension has enough space for the item
        var availableSecondarySpace = invertDimensions 
            ? secondaryDimensionSpaces[secondaryIndex * gridMainDimensionSize + mainIndex] 
            : secondaryDimensionSpaces[mainIndex * gridSecondaryDimensionSize + secondaryIndex];

        if (availableSecondarySpace < itemSecondaryDimensionSize && availableSecondarySpace != -1)
            return false;

        // Check each cell in the main dimension to ensure all have enough space for the item
        for (var index = secondaryIndex; index < secondaryIndex + itemSecondaryDimensionSize; ++index)
        {
            var availableMainSpace = invertDimensions 
                ? mainDimensionSpaces[index * gridMainDimensionSize + mainIndex] 
                : mainDimensionSpaces[mainIndex * gridSecondaryDimensionSize + index];

            if (availableMainSpace < itemMainDimensionSize && availableMainSpace != -1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Updates the grid spaces to reflect the current state of item placements.
    /// </summary>
    protected static void UpdateGridSpaces()
    {
        var skipRows = Math.Max(0, Math.Min(Instance.GridHeight - Settings.SkipRows.Value, Settings.SkipRows.Value));

        if (!Settings.Sorting || skipRows == 0 || Instance.ID != "hideout")
            return;

        try
        {
            // Update horizontal list
            CalculateAndUpdateSpace(isHorizontal: true);

            // Update vertical list
            CalculateAndUpdateSpace(isHorizontal: false);
        }
        catch (Exception e)
        {
            ItemManager.Log.LogError($"Error updating grid spaces: {e}");
        }
    }

    /// <summary>
    /// Calculates the available space in either horizontal or vertical direction.
    /// </summary>
    /// <param name="isHorizontal">Flag to calculate horizontally or vertically</param>
    private static void CalculateAndUpdateSpace(bool isHorizontal)
    {
        var list = isHorizontal ? HorizontalSpaceList : VerticalSpaceList;
        var gridHeight = Instance.GridHeight;
        var gridWidth = Instance.GridWidth;

        var skipRows = Math.Max(0, Math.Min(
            gridHeight - Settings.SkipRows.Value,
            Settings.SkipRows.Value));

        var outerLimit = isHorizontal ? gridHeight : gridWidth;
        var innerLimit = isHorizontal ? gridWidth : gridHeight;

        for (var outer = 0; outer < outerLimit - skipRows; ++outer)
        {
            var num = (isHorizontal 
                ? Instance.CanStretchHorizontally 
                : Instance.CanStretchVertically) 
                    ? -1 : 0;

            for (var inner = innerLimit - 1; inner >= 0; --inner)
            {
                var index = isHorizontal 
                    ? outer * gridWidth + inner 
                    : inner * gridWidth + outer;

                if (outer < skipRows)
                {
                    list[index] = 0;
                }
                else
                {
                    if (List0[index])
                        num = 0;
                    else if (num != -1)
                        ++num;

                    list[index] = num;
                }
            }
        }
    }
}
