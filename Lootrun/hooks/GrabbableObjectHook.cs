using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lootrun
{
    [HarmonyPatch(typeof(KnifeItem), "__initializeVariables")]
    internal class KnifePatch
    {
        [HarmonyPostfix]
        static void Hook(KnifeItem __instance)
        {
            if (!LootrunBase.isInLootrun) return;

            LootrunBase.CurrentRoundSpecials.Add(__instance);
        }
    }
}
