using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class SaveGamePatch
    {
        private static bool hasSubscribedToConnectionCallbacks;

        [HarmonyPrefix, HarmonyPatch(nameof(GameNetworkManager.SaveGame))]
        static bool SaveGameHook()
        {
            if (GameNetworkManager.Instance.currentSaveFileName == "Speedloot")
            {
                GameNetworkManager.Instance.currentSaveFileName = "LCSaveFile1";
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("SubscribeToConnectionCallbacks")]
        private static void SubscribeToConnectionCallbacksPatch(GameNetworkManager __instance)
        {
            if (!hasSubscribedToConnectionCallbacks)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += LootrunNetworkHandler.onClientConnected;
                hasSubscribedToConnectionCallbacks = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("Disconnect")]
        private static void DisconnectPatch()
        {
            if (hasSubscribedToConnectionCallbacks)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= LootrunNetworkHandler.onClientConnected;
                hasSubscribedToConnectionCallbacks = false;
            }
        }
    }

}
