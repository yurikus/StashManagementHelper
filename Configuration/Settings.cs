using BepInEx.Configuration;

namespace StashManagementHelper;

public static class Settings
{
    private const string SortingSection = "Sorting";

    public static bool Sorting = false;

    public static ConfigEntry<SortEnum> SortingStrategy { get; set; }

    public static ConfigEntry<bool> LargestItemsFirst { get; set; }

    public static ConfigEntry<bool> FoldItems { get; set; }

    public static ConfigEntry<bool> MergeItems { get; set; }

    public static ConfigEntry<int> SkipRows { get; set; }

    public static void BindSettings(ConfigFile config)
    {
        SortingStrategy = config.Bind(SortingSection, "Sorting strategy", SortEnum.Size, new ConfigDescription("Changes how items are ordered during sorting.", null, new ConfigurationManagerAttributes { Order = 100 }));
        LargestItemsFirst = config.Bind(SortingSection, "Largest items first", true, new ConfigDescription("Sort the largest items first. Does not affect the default sorting strategy.", null, new ConfigurationManagerAttributes { Order = 90 }));
        FoldItems = config.Bind(SortingSection, "Fold items", true, new ConfigDescription("Fold items to save space.", null, new ConfigurationManagerAttributes { Order = 80 }));
        MergeItems = config.Bind(SortingSection, "Merge items", true, new ConfigDescription("Merge stacking items to save space.", null, new ConfigurationManagerAttributes { Order = 70 }));
        SkipRows = config.Bind(SortingSection, "Skip rows", 0, new ConfigDescription("Skips the first rows.", new AcceptableValueRange<int>(0, 10), new ConfigurationManagerAttributes { Order = 60 }));
    }
}