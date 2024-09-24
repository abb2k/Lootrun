using HarmonyLib;
using Lootrun.hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Lootrun
{
    [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.StartGame))]
    internal class StartGamePatch
    {
        [HarmonyPostfix]
        static void StartGameHook()
        {
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                return;

            //LootrunNetworkHandler.Instance.EventClientRpc("SetLootrunEnabled");

            if (!LootrunBase.isInLootrun) return;

            LootrunBase.CurrentRoundBees.Clear();
            LootrunBase.CurrentRoundSpecials.Clear();
            LootrunBase.LootrunTime = 0;
            LootrunBase.playersThisRound = StartOfRound.Instance.connectedPlayersAmount + 1;
        }
    }
}
