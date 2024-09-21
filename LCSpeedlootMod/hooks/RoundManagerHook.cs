using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
    internal class waitForScrapToSpawnToSyncPatch
    {
        [HarmonyPrefix]
        static void waitForScrapToSpawnToSyncHook(object[] __args)
        {
            List<NetworkObjectReference> scrap = ((NetworkObjectReference[])__args[0]).ToList();

            LootrunBase.CurrentRoundScrap.Clear();
            for (int i = 0; i < scrap.Count; i++)
            {
                if (scrap[i].TryGet(out NetworkObject networkObject))
                {
                    if (networkObject.gameObject.TryGetComponent(out GrabbableObject item)){
                        LootrunBase.CurrentRoundScrap.Add(item);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.CollectNewScrapForThisRound))]
    internal class CollectNewScrapForThisRoundPatch
    {
        [HarmonyPostfix]
        static void CollectNewScrapForThisRoundHook()
        {
            bool foundAllItems = true;

            for (int i = 0; i < LootrunBase.CurrentRoundScrap.Count; i++)
            {
                if (!RoundManager.Instance.scrapCollectedThisRound.Contains(LootrunBase.CurrentRoundScrap[i]))
                    foundAllItems = false;

            }

            if (foundAllItems)
                GameObject.FindObjectOfType<StartMatchLever>().EndGame();
        }
    }
}
