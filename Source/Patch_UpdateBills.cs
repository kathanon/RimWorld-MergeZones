using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeZones;
[HarmonyPatch]
public static class Patch_UpdateBills {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Bill_Production), nameof(Bill_Production.ValidateSettings))]
    public static void ValidateSettings(Bill_Production __instance) 
        => Undo.UpdateBillZones(__instance);
}
