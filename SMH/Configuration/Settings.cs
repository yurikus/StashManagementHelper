using BepInEx.Configuration;

namespace StashManagementHelper;

public static class Settings
{
    public static bool Sorting = false;

    public static ConfigEntry<bool> FoldItems { get; set; }
    public static ConfigEntry<bool> MergeItems { get; set; }
    public static ConfigEntry<bool> RotateItems { get; set; }
    public static ConfigEntry<bool> FlipSortDirection { get; set; }
    public static ConfigEntry<bool> SortOtherContainers { get; set; }
    public static ConfigEntry<int> SkipRows { get; set; }

    public static ConfigEntry<SortOptions> ItemType { get; set; }

    public static void BindSettings(ConfigFile config)
    {
        string SortingSection = "(1) Options";
        string SortingStrategySection = "(2) Custom sorting strategy";

        // Overall
        SortOtherContainers = config.Bind(SortingSection, "Sort other containers", false,
                new ConfigDescription(
                    "Apply sorting strategy to containers other than Stash.",
                    null,
                    new ConfigurationManagerAttributes { Order = 98 }));

        FoldItems = config.Bind(SortingSection, "Fold items", false,
            new ConfigDescription(
                "Fold items to save space.",
                null,
                new ConfigurationManagerAttributes { Order = 97 }));

        MergeItems = config.Bind(SortingSection, "Merge items", false,
            new ConfigDescription(
                "Merge stacking items to save space.",
                null,
                new ConfigurationManagerAttributes { Order = 96 }));

        RotateItems = config.Bind(SortingSection, "Rotate items", false,
            new ConfigDescription(
                "Rotate items for best fit.",
                null,
                new ConfigurationManagerAttributes { Order = 95 }));

        FlipSortDirection = config.Bind(SortingSection, "Flip sort direction", false,
            new ConfigDescription(
                "Start sorting from bottom up.",
                null,
                new ConfigurationManagerAttributes { Order = 94 }));

        SkipRows = config.Bind(SortingSection, "Skip rows", 12,
            new ConfigDescription(
                "Skips the first # rows in stash.",
                new AcceptableValueRange<int>(0, 14),
                new ConfigurationManagerAttributes { Order = 93 }));

        // Sorting strategy
        ItemType = config.Bind(SortingStrategySection, "Sort by item type", SortOptions.Enabled,
            new ConfigDescription(
                "Sort by item type",
                null,
                new ConfigurationManagerAttributes { Order = 48 }));
    }

    public static SortOptions GetSortOption() => ItemType.Value;
}