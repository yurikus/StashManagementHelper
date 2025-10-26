using System;
using System.Collections.Generic;
using EFT.InventoryLogic;

namespace StashManagementHelper;

public static class StackUtils
{
    [ThreadStatic]
    private static List<Item> _buffer;

    public static List<Item> GetSortedStacks(IEnumerable<Item> group)
    {
        var stacks = _buffer ??= [];

        stacks.Clear();

        foreach (var i in group)
        {
            if (i.StackObjectsCount > 0)
                stacks.Add(i);
        }

        // Sort descending
        stacks.Sort((a, b) => b.StackObjectsCount.CompareTo(a.StackObjectsCount));

        var result = new List<Item>(stacks);

        // Leave buffer for reuse next call (avoid GC)
        return result;
    }

    public static List<List<Item>> GetStackableGroups(StashGridClass grid)
    {
        var result = new List<List<Item>>();
        var groups = new Dictionary<(string templateId, bool spawnedInSession), List<Item>>();

        foreach (var itm in grid.Items)
        {
            // Filter early to minimize work
            if (itm.StackObjectsCount >= itm.StackMaxSize || itm.Owner == null)
                continue;

            var key = (itm.TemplateId, itm.SpawnedInSession);

            if (!groups.TryGetValue(key, out var list))
            {
                list = [];
                groups[key] = list;
            }

            list.Add(itm);
        }

        // Collect only groups with multiple entries
        foreach (var kvp in groups)
        {
            if (kvp.Value.Count > 1)
                result.Add(kvp.Value);
        }

        return result;
    }
}
