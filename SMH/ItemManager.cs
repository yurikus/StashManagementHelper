using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT.InventoryLogic;

namespace SMH;

public static class ItemManager
{
    public static ManualLogSource Log { get; set; }

    const string StashItemId = "hideout";
    const string StashTemplateId = "566abbc34bdc2d92178b4576";

    /// <summary>
    /// Checks if an item is located in the player's stash
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns>True if the item is in the player stash, false otherwise</returns>
    public static bool IsItemInStash(Item item)
    {
        if (item is null)
            return false;

        if (item is StashItemClass
            || string.Equals(item.TemplateId, StashTemplateId, StringComparison.Ordinal)
            || string.Equals(item.Owner?.ID, StashItemId, StringComparison.Ordinal))
        {
            return true;
        }

        try
        {
            foreach (var parent in item.GetAllParentItems())
            {
                if (parent is StashItemClass
                    || string.Equals(parent.TemplateId, StashTemplateId, StringComparison.Ordinal)
                    || string.Equals(parent.Owner?.ID, StashItemId, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogError($"Error checking stash parents: {ex.Message}");
        }

        return false;
    }
}