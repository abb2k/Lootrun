using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Lootrun
{
    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.FillEndGameStats))]
    internal class FillEndGameStatsPatch
    {
        private static GameObject scrapTextObject;

        [HarmonyPostfix]
        static void FillEndGameStatsHook(HUDManager __instance)
        {
            if (!LootrunBase.isInLootrun) return;

            if (scrapTextObject != null) GameObject.Destroy(scrapTextObject);

            scrapTextObject = UnityEngine.Object.Instantiate<GameObject>(__instance.statsUIElements.quotaNumerator.gameObject, __instance.statsUIElements.quotaNumerator.transform.parent);
            scrapTextObject.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            scrapTextObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(52.5f, -165);
            scrapTextObject.name = "Scrap Collected Text";

            float precentOfScrapCollected = ((float)LootrunBase.currentRunResults.scrapCollectedOutOf.x) / LootrunBase.currentRunResults.scrapCollectedOutOf.y * 100;
            __instance.statsUIElements.quotaNumerator.text = LootrunBase.SecsToTimer(LootrunBase.LootrunTime);
            __instance.statsUIElements.quotaDenominator.text = string.Format("{0}/{1}({2}%)", LootrunBase.currentRunResults.scrapCollectedOutOf.x, LootrunBase.currentRunResults.scrapCollectedOutOf.y, (int)precentOfScrapCollected);
            RectTransform bettomT = __instance.statsUIElements.quotaDenominator.GetComponent<RectTransform>();
            bettomT.sizeDelta = new Vector2(bettomT.sizeDelta.x + 500, bettomT.sizeDelta.y);
        }

    }
}
