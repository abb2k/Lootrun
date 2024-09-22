using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(RedLocustBees), nameof(RedLocustBees.Start))]
    internal class RedLocustBeesStartPatch
    {
        [HarmonyPostfix]
        static void StartHook(RedLocustBees __instance)
        {
            if (__instance.hive)
                LootrunBase.CurrentRoundBees.Add(__instance.hive);
        }
    }
}
