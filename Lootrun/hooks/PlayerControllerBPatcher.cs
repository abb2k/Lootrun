using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        private static void ConnectClientToPlayerObjectPatch(PlayerControllerB __instance)
        {
            if (GameNetworkManager.Instance.localPlayerController != __instance) return;

            if (StartOfRound.Instance.localPlayerController.IsHost || StartOfRound.Instance.localPlayerController.IsServer)
            {
                LootrunNetworkHandler.instance.SyncInLootrunClientRpc(StartOfRound.Instance.localPlayerController.playerClientId, LootrunBase.isInLootrun);
            }
        }
    }
}
