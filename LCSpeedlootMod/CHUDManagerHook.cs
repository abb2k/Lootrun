using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lootrun
{
    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.FillEndGameStats))]
    internal class FillEndGameStatsPatch
    {
        [HarmonyPostfix]
        static void FillEndGameStatsHook(HUDManager __instance)
        {
            if (!LootrunBase.isInLootrun) return;

            float precentOfScrapCollected = ((float)LootrunBase.currentRunResults.scrapCollectedOutOf.x) / LootrunBase.currentRunResults.scrapCollectedOutOf.y * 100;
            __instance.statsUIElements.quotaNumerator.text = ((int)LootrunBase.LootrunTime).ToString();
            __instance.statsUIElements.quotaDenominator.text = string.Format("{0}/{1}({2}%)", LootrunBase.currentRunResults.scrapCollectedOutOf.x, LootrunBase.currentRunResults.scrapCollectedOutOf.y, (int)precentOfScrapCollected);
            RectTransform bettomT = __instance.statsUIElements.quotaDenominator.GetComponent<RectTransform>();
            bettomT.sizeDelta = new Vector2(bettomT.sizeDelta.x + 500, bettomT.sizeDelta.y);
        }

    }
}
