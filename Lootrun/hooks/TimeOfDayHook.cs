using HarmonyLib;
using Lootrun;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetShipToLeaveOnMidnightClientRpc))]
    internal class SetShipToLeaveOnMidnightClientRpcPatcher
    {
        [HarmonyPrefix]
        static bool SetShipToLeaveOnMidnightClientRpcPatch()
        {
            if (LootrunBase.isInLootrun)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.UpdateProfitQuotaCurrentTime))]
    internal class UpdateProfitQuotaCurrentTimePatch
    {
        [HarmonyPrefix]
        static bool UpdateProfitQuotaCurrentTimeHook()
        {
            if (LootrunBase.isInLootrun) return false;
            return true;
        }
    }

    [HarmonyPatch]
    internal class TimeOfDayUpdatePatch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(TimeOfDay), "Update")]
        static void UpdateHook(TimeOfDay __instance)
        {
            if (__instance.currentDayTimeStarted && LootrunBase.isInLootrun)
            {
                if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                    return;

                LootrunBase.LootrunTime += Time.deltaTime;
                LootrunBase.timerText.text = LootrunBase.SecsToTimer(LootrunBase.LootrunTime);

                LootrunNetworkHandler.instance.SyncLootrunTimerClientRpc(LootrunBase.LootrunTime);

            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeOfDay), "Awake")]
        static void AwakeHook(TimeOfDay __instance)
        {
            if (!LootrunBase.isInLootrun) return;
            if (__instance.quotaVariables != null)
            {
                __instance.quotaVariables.startingCredits = LootrunBase.currentRunSettings.money;
            }
        }
    }
}
