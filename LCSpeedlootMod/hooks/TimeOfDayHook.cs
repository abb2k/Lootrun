using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.hooks
{

    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.TimeOfDayEvents))]
    internal class TimeOfDayEventsTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            MethodInfo targetMethod = AccessTools.Method(typeof(TimeOfDay), nameof(TimeOfDay.SetShipToLeaveOnMidnightClientRpc));
            Label label = generator.DefineLabel();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(targetMethod))
                {
                    codes.Insert(i - 1, new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(LootrunBase), nameof(LootrunBase.isInLootrun))));
                    codes.Insert(i, new CodeInstruction(OpCodes.Brtrue, label));
                    codes[i + 2].labels.Add(label);
                    break;
                }
            }
            
            return codes.AsEnumerable();
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

                //LootrunNetworkHandler.Instance.UpdateTimeClientRpc(LootrunBase.LootrunTime);

            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeOfDay), "Awake")]
        static void AwakeHook(TimeOfDay __instance)
        {
            if (!LootrunBase.isInLootrun) return;
            LootrunNetworkHandler.TimeEvent += ReceivedTimeFromServer;
            if (__instance.quotaVariables != null)
            {
                __instance.quotaVariables.startingCredits = LootrunBase.currentRunSettings.money;
            }
        }

        static void ReceivedTimeFromServer(float time)
        {
            LootrunBase.LootrunTime = time;
            LootrunBase.timerText.text = LootrunBase.SecsToTimer(time);
        }
    }
}
