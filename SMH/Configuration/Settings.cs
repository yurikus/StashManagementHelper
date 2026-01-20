using BepInEx.Configuration;

namespace SMH;

public static class Settings
{
    public static bool Sorting = false;

    public static ConfigEntry<int> SkipRows { get; set; }

    public static void BindSettings(ConfigFile config)
    {
        string SortingSection = "(1) Options";

        // Overall
        SkipRows = config.Bind(SortingSection, "Skip rows", 10,
            new ConfigDescription(
                "Skips the first # rows in stash.",
                new AcceptableValueRange<int>(0, 14),
                new ConfigurationManagerAttributes { Order = 93 }));
    }
}