using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(LungProp), nameof(LungProp.Start))]
    internal class ApparatusStartPatch
    {
        [HarmonyPostfix]
        static void StartHook(LungProp __instance)
        {
            LootrunBase.CurrentRoundScrap.Add(__instance);
        }
    }
}
