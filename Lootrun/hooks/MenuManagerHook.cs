using HarmonyLib;
using Lootrun.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// TODO:
// update UI in unity assetBundle --Y
// make UI actually live update currentRunSettings --Y
// fix host bug (check how hosting is done in vanilla) --X
// make items actually spawn correctly spawn (maybe create set spawn locations would be cool) --X
// make saving to files work --X
// make old run view list --X

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerHook
    {
        public static GameObject speedlootMenuContainer;

        public static GameObject speedlootButton;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartHook(ref GameObject ___menuButtons, ref GameObject ___HostSettingsScreen)
        {
            if (___menuButtons == null) return;
            if (___menuButtons.transform.GetChild(1) == null) return;

            speedlootButton = GameObject.Instantiate(___menuButtons.transform.GetChild(1).gameObject, ___menuButtons.transform);
            speedlootButton.name = "LootrunButton";
            speedlootButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(speedlootButton.GetComponent<RectTransform>().anchoredPosition.x, 235);

            Button speedlootB = speedlootButton.GetComponent<Button>();
            speedlootB.onClick.RemoveAllListeners();

            TextMeshProUGUI speedlootT = speedlootButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            speedlootT.text = "> Lootrun";

            LootrunBase.isInLootrun = false;
            LootrunBase.LootrunTime = 0;

            speedlootMenuContainer = GameObject.Instantiate(LootrunBase.bundle.LoadAsset<GameObject>("speedlootMenuContainer"), ___HostSettingsScreen.transform.parent);
            speedlootMenuContainer.name = "speedlootMenuContainer";
            speedlootMenuContainer.transform.position = ___HostSettingsScreen.transform.position;
            speedlootMenuContainer.transform.localScale = ___HostSettingsScreen.transform.localScale;
            speedlootMenuContainer.SetActive(false);
            var settingsMenu = speedlootMenuContainer.GetComponent<LootrunSettingsMenu>();
            settingsMenu.SetToPreset(LootrunBase.currentRunSettings);
            settingsMenu.onPresetChanged += (LootrunPreset preset) => { LootrunBase.currentRunSettings = preset; };

            var speedlootBack = CopyCoolButton("speedlootBack", speedlootMenuContainer.transform, new Vector2(120, 30), "[ Back ]", () =>
            {
                speedlootMenuContainer.SetActive(false);
            });
            speedlootBack.transform.localPosition = new Vector3(175, -115, 0);

            var speedlootOldRuns = CopyCoolButton("speedlootOldRuns", speedlootMenuContainer.transform, new Vector2(130, 30), "[ Runs ]", () =>
            {
                
            });
            speedlootOldRuns.transform.localPosition = new Vector3(175, -70, 0);

            var speedlootStart = CopyCoolButton("speedlootStart", speedlootMenuContainer.transform, new Vector2(130, 30), "[ Start ]", () =>
            {
                speedlootMenuContainer.SetActive(false);
                LootrunBase.isInLootrun = true;
                GameNetworkManager.Instance.currentSaveFileName = "Speedloot";

                GameNetworkManager.Instance.lobbyHostSettings = new HostSettings("LootrunLobby", false, "");
                GameNetworkManager.Instance.StartHost();
            });
            speedlootStart.transform.localPosition = new Vector3(175, -22, 0);

            speedlootB.onClick.AddListener(() =>
            {
                speedlootMenuContainer.SetActive(true);
            });
        }

        static GameObject CopyCoolButton(string objectName, Transform parent, Vector2 size, string btnText, UnityAction callback)
        {
            GameObject btn = GameObject.Instantiate(speedlootButton, parent);
            btn.name = objectName;
            btn.GetComponent<RectTransform>().sizeDelta = size;
            btn.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = size - Vector2.up * 5;
            btn.transform.GetChild(0).localPosition = Vector3.zero;
            Button btnButtonScr = btn.GetComponent<Button>();
            btnButtonScr.onClick.RemoveAllListeners();
            btnButtonScr.onClick.AddListener(callback);
            TextMeshProUGUI btnTextScr = btn.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            btnTextScr.transform.localPosition = Vector3.zero;
            btnTextScr.alignment = TextAlignmentOptions.Center;
            btnTextScr.text = btnText;

            return btn;
        }
    }
}