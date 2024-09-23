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

            LootrunBase.isInLootrun = false;
            LootrunBase.LootrunTime = 0;

            // ---- speedloot menu ----

            GameObject empty = new GameObject();

            speedlootMenuContainer = GameObject.Instantiate(LootrunBase.bundle.LoadAsset<GameObject>("speedlootMenuContainer"), ___HostSettingsScreen.transform.parent);
            speedlootMenuContainer.name = "speedlootMenuContainer";
            speedlootMenuContainer.transform.position = ___HostSettingsScreen.transform.position;
            speedlootMenuContainer.transform.localScale = ___HostSettingsScreen.transform.localScale;
            speedlootMenuContainer.SetActive(false);

            TMP_Dropdown moonsD = speedlootMenuContainer.transform.GetChild(2).GetComponent<TMP_Dropdown>();

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("41-Experimentation"),
                new TMP_Dropdown.OptionData("220-Assurance"),
                new TMP_Dropdown.OptionData("56-Vow"),
                new TMP_Dropdown.OptionData("21-Offense"),
                new TMP_Dropdown.OptionData("61-March"),
                new TMP_Dropdown.OptionData("20-Adamance"),
                new TMP_Dropdown.OptionData("85-Rend"),
                new TMP_Dropdown.OptionData("7-Dine"),
                new TMP_Dropdown.OptionData("8-Titan"),
                new TMP_Dropdown.OptionData("68-Artifice"),
                new TMP_Dropdown.OptionData("5-Embrion")
            };

            moonsD.AddOptions(options);

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
                LootrunBase.isInLootrun = true;
                GameNetworkManager.Instance.currentSaveFileName = "Speedloot";
                LootrunSettings s = new LootrunSettings();
                for (int i = 0; i < speedlootMenuContainer.transform.childCount; i++)
                {
                    Transform child = speedlootMenuContainer.transform.GetChild(i);

                    if (child.name == "Shotguns/Knifes")
                    {
                        s.spacials = child.GetComponent<Toggle>().isOn;
                    }

                    if (child.name == "beesToggle")
                    {
                        s.bees = child.GetComponent<Toggle>().isOn;
                    }

                    if (child.name == "seedToggle")
                    {
                        s.randomseed = child.GetComponent<Toggle>().isOn;
                    }

                    if (child.name == "moonsDropdown")
                    {
                        switch (child.GetComponent<TMP_Dropdown>().options[child.GetComponent<TMP_Dropdown>().value].text)
                        {
                            case "41-Experimentation":
                                s.moon = 0;
                                break;

                            case "220-Assurance":
                                s.moon = 1;
                                break;

                            case "56-Vow":
                                s.moon = 2;
                                break;

                            case "21-Offense":
                                s.moon = 8;
                                break;

                            case "61-March":
                                s.moon = 4;
                                break;

                            case "20-Adamance":
                                s.moon = 5;
                                break;

                            case "85-Rend":
                                s.moon = 6;
                                break;

                            case "7-Dine":
                                s.moon = 7;
                                break;

                            case "8-Titan":
                                s.moon = 9;
                                break;

                            case "68-Artifice":
                                s.moon = 10;
                                break;

                            case "5-Embrion":
                                s.moon = 12;
                                break;
                        }
                        
                    }

                    if (child.name == "whetherDropdown")
                    {
                        s.weather = child.GetComponent<TMP_Dropdown>().value;
                    }

                    if (child.name == "seedInput")
                    {
                        if (int.TryParse(child.GetComponent<TMP_InputField>().text, out int res))
                            s.seed = res;
                        else
                            s.seed = 0;
                    }

                    if (child.name == "money")
                    {
                        if (int.TryParse(child.GetChild(1).GetComponent<TMP_InputField>().text, out int res))
                            s.money = res;
                        else
                            s.money = 0;
                    }
                }
                LootrunBase.currentRunSettings = s;

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