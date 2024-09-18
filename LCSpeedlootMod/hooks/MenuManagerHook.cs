using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerHook
    {
        public static GameObject speedlootMenuContainer;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartHook(ref GameObject ___menuButtons, ref GameObject ___HostSettingsScreen)
        {
            if (___menuButtons.transform.GetChild(1) == null || ___menuButtons == null) return;

            GameObject speedlootButton = GameObject.Instantiate(___menuButtons.transform.GetChild(1).gameObject, ___menuButtons.transform);
            speedlootButton.name = "LootrunButton";
            speedlootButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(speedlootButton.GetComponent<RectTransform>().anchoredPosition.x, 235);

            Button speedlootB = speedlootButton.GetComponent<Button>();
            speedlootB.onClick.RemoveAllListeners();
            
            TextMeshProUGUI speedlootT = speedlootButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            speedlootT.text = "> Lootrun";

            // ---- speedloot menu ----

            GameObject empty = new GameObject();

            speedlootMenuContainer = GameObject.Instantiate(LootrunBase.bundle.LoadAsset<GameObject>("speedlootMenuContainer"), ___HostSettingsScreen.transform.parent);
            speedlootMenuContainer.name = "speedlootMenuContainer";
            speedlootMenuContainer.transform.position = ___HostSettingsScreen.transform.position;
            speedlootMenuContainer.transform.localScale = ___HostSettingsScreen.transform.localScale;

            //buttons

            GameObject speedlootBack = GameObject.Instantiate(speedlootButton, speedlootMenuContainer.transform);
            speedlootBack.name = "speedlootBack";
            speedlootBack.transform.localPosition = new Vector3(0, -75, 0);
            speedlootBack.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 30);
            speedlootBack.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(120, 25);
            speedlootBack.transform.GetChild(0).localPosition = Vector3.zero;
            Button speedlootBackButton = speedlootBack.GetComponent<Button>();
            speedlootBackButton.onClick.AddListener(() =>
            {
                speedlootMenuContainer.SetActive(false);
            });
            TextMeshProUGUI speedlootBacktext = speedlootBack.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            speedlootBacktext.transform.localPosition = Vector3.zero;
            speedlootBacktext.alignment = TextAlignmentOptions.Center;
            speedlootBacktext.text = "[ Back ]";

            GameObject speedlootStart = GameObject.Instantiate(speedlootBack, speedlootMenuContainer.transform);
            speedlootStart.name = "speedlootStart";
            speedlootStart.transform.localPosition = new Vector3(0, -50, 0);
            speedlootStart.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 30);
            speedlootStart.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(130, 25);
            speedlootStart.transform.GetChild(0).localPosition = Vector3.zero;
            Button speedlootStartButton = speedlootStart.GetComponent<Button>();
            speedlootStartButton.onClick.AddListener(() =>
            {
                speedlootMenuContainer.SetActive(false);

                GameNetworkManager.Instance.StartHost();
            });
            TextMeshProUGUI speedlootStartButtontext = speedlootStart.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            speedlootStartButtontext.transform.localPosition = Vector3.zero;
            speedlootStartButtontext.alignment = TextAlignmentOptions.Center;
            speedlootStartButtontext.text = "[ Start ]";

            //options

            


            GameObject.Destroy(empty);
            speedlootB.onClick.AddListener(() =>
            {
                speedlootMenuContainer.SetActive(true);
            });
        }
    }
}

/*
 * TODO:
 * 
 * - add options to the menu
 *   - beehives
 *   - shotguns/knifes
 *   - start money
 *   - bests
 *   - moon select
 *   - whether select
 *   - random seed/seed select
 * 
 *  - start button
 *  - close button
 */