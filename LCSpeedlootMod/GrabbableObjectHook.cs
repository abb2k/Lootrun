using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lootrun
{
    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Start))]
    internal class GrabbableObjectStartPatch
    {
        [HarmonyPostfix]
        static void StartHook(GrabbableObject __instance)
        {
            if (!LootrunBase.isInLootrun) return;

            KnifeItem knife = (KnifeItem)__instance;
            if (knife != null)
            {
                LootrunBase.CurrentRoundSpecials.Add(knife);
            }
        }
    }
}
