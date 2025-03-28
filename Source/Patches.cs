using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using I2.Loc;
using NineSolsAPI;

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