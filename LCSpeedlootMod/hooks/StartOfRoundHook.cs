using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundHook
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartHook(ref StartOfRound ___this)
        {
            if (!LootrunBase.isInLootrun) return;

            ___this.SetPlanetsWeather();
            ___this.currentLevel = ___this.levels[LootrunBase.currentRunSettings.moon];
            ___this.currentLevelID = LootrunBase.currentRunSettings.moon;
            TimeOfDay.Instance.currentLevel = ___this.currentLevel;
            RoundManager.Instance.currentLevel = ___this.levels[LootrunBase.currentRunSettings.moon];

            TimeOfDay timeOfDay = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
            timeOfDay.quotaFulfilled = 0;
            timeOfDay.timesFulfilledQuota = 0;

            ___this.ChangePlanet();
        }

        [HarmonyPatch("Start")]
        static void AutoSaveShipDataHook()
        {

        }
    }
}
