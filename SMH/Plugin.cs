using BepInEx;

namespace StashManagementHelper;

[BepInPlugin("com.yurikus.smh", "yurikus - Stash Management Helper", BuildInfo.ModVersion)]
[BepInDependency("com.SPT.core", "4.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        ItemManager.Log = Logger;

        Settings.BindSettings(Config);

        new FindFreeSpacePatch().Enable();
        new SortPatch().Enable();
        new ItemListSortPatch().Enable();

        ItemManager.Log.LogInfo($"Plugin SMH loaded.");
    }
}