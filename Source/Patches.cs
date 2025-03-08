using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using I2.Loc;

namespace EigongPrime;

[HarmonyPatch]
public class Patches {
    [HarmonyPatch(typeof(Player), nameof(Player.SetStoryWalk))]
    [HarmonyPrefix]
    private static bool PatchStoryWalk(ref float walkModifier) {
        walkModifier = 1.0f;

        return true; // the original method should be executed
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
    private static void GetInnerEigongTranslation(string Term, ref string __result) {
        if (Term != "Characters/NameTag_YiKong") return;
        __result = "Eigong Prime";
    }
}