using GameNetcodeStuff;
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
    [HarmonyPatch(typeof(PlayerControllerB), "Start")]
    internal class PlayerControllerBPatch
    {
        [HarmonyPostfix]
        static void StartHook(PlayerControllerB __instance)
        {
            if (!LootrunBase.isInLootrun) return;
            if (LootrunBase.timerText) return;

            var empty = new GameObject();
            var text = GameObject.Instantiate(empty, __instance.playerHudUIContainer);
            text.name = "Lootrun time text";
            text.transform.localPosition = new Vector3(350, -200, 0);
            TextMeshProUGUI textComp = text.AddComponent<TextMeshProUGUI>();

            textComp.text = LootrunBase.SecsToTimer(0);

            LootrunBase.timerText = textComp;
            GameObject.Destroy(empty);
        }
    }
}
