using Aki.Reflection.Patching;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Reflection;

namespace StashManagementHelper
{
    public class TestPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(Item), "CalculateExtraSize");

        [PatchPrefix]
        private static void PatchPrefix(ref Item __instance)
        {
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Item __instance)
        {
        }
    }


}
