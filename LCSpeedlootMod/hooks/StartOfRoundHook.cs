using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    internal class StartOfRoundHook
    {
        [HarmonyPostfix]
        static void StartHook(StartOfRound __instance)
        {
            if (!LootrunBase.isInLootrun) return;

            __instance.currentLevel = __instance.levels[LootrunBase.currentRunSettings.moon];
            __instance.currentLevelID = LootrunBase.currentRunSettings.moon;
            TimeOfDay.Instance.currentLevel = __instance.currentLevel;
            RoundManager.Instance.currentLevel = __instance.levels[LootrunBase.currentRunSettings.moon];

            if (LootrunBase.currentRunSettings.weather == 0)
            {
                __instance.currentLevel.overrideWeather = true;
                __instance.currentLevel.overrideWeatherType = __instance.currentLevel.randomWeathers[UnityEngine.Random.Range(0, __instance.currentLevel.randomWeathers.Length)].weatherType;

                for (int i = 0; i < __instance.currentLevel.randomWeathers.Length; i++)
                {
                    Debug.Log(__instance.currentLevel.randomWeathers[i].weatherType.ToString());
                }
            }
            else
            {
                __instance.currentLevel.overrideWeather = true;
                __instance.currentLevel.overrideWeatherType = (LevelWeatherType)LootrunBase.currentRunSettings.weather;
            }

            TimeOfDay timeOfDay = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
            timeOfDay.quotaFulfilled = 0;
            timeOfDay.timesFulfilledQuota = 0;
            timeOfDay.UpdateProfitQuotaCurrentTime();

            __instance.ChangePlanet();

            __instance.overrideRandomSeed = !LootrunBase.currentRunSettings.randomseed;
            __instance.overrideSeedNumber = LootrunBase.currentRunSettings.seed;
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            terminal.groupCredits = LootrunBase.currentRunSettings.money;

            
            StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\nNever";

            StartOfRound.Instance.profitQuotaMonitorText.text = "PROFIT QUOTA:\nAll of them"; 
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "PassTimeToNextDay")]
    internal class PassTimeToNextDayPatch
    {
        [HarmonyPrefix]
        static bool PassTimeToNextDayHook()
        {
            LootrunBase.LootrunTime = 0;
            if (LootrunBase.isInLootrun) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ShipHasLeft))]
    internal class ShipHasLeftPatch
    {
        [HarmonyPrefix]
        static void ShipHasLeftHook()
        {
            TimeOfDay.Instance.currentDayTimeStarted = false;
            
        }
    }
}
