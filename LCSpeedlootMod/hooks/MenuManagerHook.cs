﻿using HarmonyLib;
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

        public static TMP_Dropdown moonsDropdown;
        public static TMP_Dropdown weatherDropdown;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartHook(ref GameObject ___menuButtons, ref GameObject ___HostSettingsScreen)
        {
            if (___menuButtons == null) return;
            if (___menuButtons.transform.GetChild(1) == null) return;

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

            moonsDropdown = speedlootMenuContainer.transform.GetChild(2).GetComponent<TMP_Dropdown>();
            weatherDropdown = speedlootMenuContainer.transform.GetChild(3).GetComponent<TMP_Dropdown>();

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

            moonsDropdown.AddOptions(options);

            List<TMP_Dropdown.OptionData> weatherOptions = new List<TMP_Dropdown.OptionData>();

            var weatherOptionsViable = LootrunBase.MoonAvalableWeathers(LootrunBase.MoonNameToID(moonsDropdown.options[moonsDropdown.value].text));

            weatherOptions.Add(new TMP_Dropdown.OptionData("Random"));
            for (int i = 0; i < weatherOptionsViable.Count; i++)
            {
                weatherOptions.Add(new TMP_Dropdown.OptionData(weatherOptionsViable[i].ToString()));
            }

            weatherDropdown.AddOptions(weatherOptions);

            moonsDropdown.onValueChanged.AddListener((int newVal) =>
            {
                weatherDropdown.ClearOptions();

                List<TMP_Dropdown.OptionData> _weatherOptions = new List<TMP_Dropdown.OptionData>();

                var _weatherOptionsViable = LootrunBase.MoonAvalableWeathers(LootrunBase.MoonNameToID(moonsDropdown.options[moonsDropdown.value].text));

                _weatherOptions.Add(new TMP_Dropdown.OptionData("Random"));
                for (int i = 0; i < _weatherOptionsViable.Count; i++)
                {
                    _weatherOptions.Add(new TMP_Dropdown.OptionData(_weatherOptionsViable[i].ToString()));
                }

                weatherDropdown.AddOptions(_weatherOptions);
            });

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

                s.moon = LootrunBase.MoonNameToID(moonsDropdown.options[moonsDropdown.value].text);

                if (weatherDropdown.options[weatherDropdown.value].text == "Random")
                {
                    s.weather = -2;
                }
                else
                {
                    s.weather = (int)LootrunBase.weatherNameToType(weatherDropdown.options[weatherDropdown.value].text);
                    LootrunBase.mls.LogInfo("weather " + s.weather);
                }
                
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