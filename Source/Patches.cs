using HarmonyLib;
using I2.Loc;

namespace EigongPrime;

[HarmonyPatch]
public class Patches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
    private static void EigongPrimeNameChange(string Term, ref string __result) {
        if (Term != "Characters/NameTag_YiKong") return;
        __result = "Eigong Prime";
    }
}