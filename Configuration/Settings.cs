using BepInEx.Configuration;

namespace StashManagementHelper;

public static class Settings
{
    private const string SortingSection = "(1) Options";

    private const string SortingStrategySection = "(2) Custom sorting strategy";

    public static bool Sorting = false;

    public static ConfigEntry<SortEnum> SortingStrategy { get; set; }

    public static ConfigEntry<bool> FoldItems { get; set; }

    public static ConfigEntry<bool> MergeItems { get; set; }

    public static ConfigEntry<int> SkipRows { get; set; }

    public static ConfigEntry<SortOptions> ContainerSize { get; set; }
    public static ConfigEntry<SortOptions> IndexOfItemType { get; set; }
    public static ConfigEntry<SortOptions> CellSize { get; set; }

    public static void BindSettings(ConfigFile config)
    {
        SortingStrategy = config.Bind(SortingSection, "Sorting strategy", SortEnum.Custom,
            new ConfigDescription("Changes how items are ordered during sorting.", null, new ConfigurationManagerAttributes { Order = 100 }));

        FoldItems = config.Bind(SortingSection, "Fold items", true,
            new ConfigDescription("Fold items to save space.", null, new ConfigurationManagerAttributes { Order = 98 }));

        MergeItems = config.Bind(SortingSection, "Merge items", true,
            new ConfigDescription("Merge stacking items to save space.", null, new ConfigurationManagerAttributes { Order = 97 }));

        SkipRows = config.Bind(SortingSection, "Skip rows", 0,
            new ConfigDescription("Skips the first rows.", new AcceptableValueRange<int>(0, 10), new ConfigurationManagerAttributes { Order = 96 }));


        ContainerSize = config.Bind(SortingStrategySection, "Sort by container size", SortOptions.Enabled | SortOptions.Descending,
            new ConfigDescription("Sort by container size", null, new ConfigurationManagerAttributes { Order = 50 }));

        IndexOfItemType = config.Bind(SortingStrategySection, "Sort by item type", SortOptions.Enabled | SortOptions.Descending,
            new ConfigDescription("Sort by item type", null, new ConfigurationManagerAttributes { Order = 49 }));

        CellSize = config.Bind(SortingStrategySection, "Sort by item size", SortOptions.Enabled | SortOptions.Descending,
            new ConfigDescription("Sort by item size", null, new ConfigurationManagerAttributes { Order = 48 }));
    }
}