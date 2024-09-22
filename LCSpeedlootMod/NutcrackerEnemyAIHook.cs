using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lootrun
{
    [HarmonyPatch(typeof(NutcrackerEnemyAI), "GrabGun")]
    internal class NutcrackerEnemyAIGrabGunPatch
    {
        [HarmonyPrefix]
        static void GrabGun(object[] __args)
        {
            if (!LootrunBase.isInLootrun) return;
            LootrunBase.CurrentRoundSpecials.Add(((GameObject)__args[0]).GetComponent<ShotgunItem>());
        }
    }
}
