using Discord;
using HarmonyLib;
using Lootrun.hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun
{
    [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.StartGame))]
    internal class StartGamePatch
    {
        [HarmonyPostfix]
        static void StartGameHook()
        {
            if (!LootrunBase.isInLootrun) return;

            LootrunBase.CurrentRoundBees.Clear();
            LootrunBase.CurrentRoundSpecials.Clear();
            LootrunBase.LootrunTime = 0;
            LootrunBase.playersThisRound.Clear();
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null) continue;
                LootrunBase.playersThisRound.Add(player.playerUsername);
            }
        }
    }
}
