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

        public static TMP_Dropdown moonsDropdown;
        public static TMP_Dropdown weatherDropdown;

        public static Toggle beesToggle;
        public static Toggle specialsToggle;
        public static Toggle SJetpacksToggle;
        public static Toggle SCrusierToggle;
        public static Toggle randomSeedToggle;

        public static TMP_InputField seedInput;
        public static TMP_InputField moneyAmountInput;

        public static TextMeshProUGUI bestRunText;

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

            //create menu

            speedlootMenuContainer = GameObject.Instantiate(LootrunBase.bundle.LoadAsset<GameObject>("speedlootMenuContainer"), ___HostSettingsScreen.transform.parent);
            speedlootMenuContainer.name = "speedlootMenuContainer";
            speedlootMenuContainer.transform.position = ___HostSettingsScreen.transform.position;
            speedlootMenuContainer.transform.localScale = ___HostSettingsScreen.transform.localScale;
            speedlootMenuContainer.SetActive(false);

            //setup ui

            moonsDropdown = speedlootMenuContainer.transform.GetChild(2).GetComponent<TMP_Dropdown>();
            weatherDropdown = speedlootMenuContainer.transform.GetChild(3).GetComponent<TMP_Dropdown>();

            beesToggle = speedlootMenuContainer.transform.GetChild(4).GetComponent<Toggle>();
            specialsToggle = speedlootMenuContainer.transform.GetChild(5).GetComponent<Toggle>();
            SJetpacksToggle = speedlootMenuContainer.transform.GetChild(6).GetComponent<Toggle>();
            SCrusierToggle = speedlootMenuContainer.transform.GetChild(7).GetComponent<Toggle>();
            randomSeedToggle = speedlootMenuContainer.transform.GetChild(9).GetComponent<Toggle>();

            moneyAmountInput = speedlootMenuContainer.transform.GetChild(8).GetChild(1).GetComponent<TMP_InputField>();
            seedInput = speedlootMenuContainer.transform.GetChild(10).GetComponent<TMP_InputField>();

            moonsDropdown.onValueChanged.AddListener(UpdateBest);
            weatherDropdown.onValueChanged.AddListener(UpdateBest);

            beesToggle.onValueChanged.AddListener(UpdateBest);
            specialsToggle.onValueChanged.AddListener(UpdateBest);
            SJetpacksToggle.onValueChanged.AddListener(UpdateBest);
            SCrusierToggle.onValueChanged.AddListener(UpdateBest);

            randomSeedToggle.onValueChanged.AddListener(toggleSeedInput);
            randomSeedToggle.onValueChanged.AddListener(UpdateBest);
            

            moneyAmountInput.onValueChanged.AddListener(UpdateBest);
            seedInput.onValueChanged.AddListener(UpdateBest);

            bestRunText = speedlootMenuContainer.transform.GetChild(12).GetComponent<TextMeshProUGUI>();

            //setup ui variables

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

            string currentMoonName = LootrunBase.MoonIDToName(LootrunBase.currentRunSettings.moon);
            for (int i = 0; i < options.Count; i++)
                if (options[i].text == currentMoonName)
                    moonsDropdown.value = i;

            moonsDropdown.RefreshShownValue();

            List<TMP_Dropdown.OptionData> weatherOptions = new List<TMP_Dropdown.OptionData>();

            var weatherOptionsViable = LootrunBase.MoonAvalableWeathers(LootrunBase.MoonNameToID(moonsDropdown.options[moonsDropdown.value].text));

            weatherOptions.Add(new TMP_Dropdown.OptionData("Random"));
            for (int i = 0; i < weatherOptionsViable.Count; i++)
            {
                weatherOptions.Add(new TMP_Dropdown.OptionData(weatherOptionsViable[i].ToString()));
            }

            weatherDropdown.AddOptions(weatherOptions);

            if (LootrunBase.currentRunSettings.weather == -2)
                weatherDropdown.value = 0;
            else
            {
                LevelWeatherType w = (LevelWeatherType)LootrunBase.currentRunSettings.weather;

                for (int i = 0; i < weatherDropdown.options.Count; i++)
                {
                    if (weatherDropdown.options[i].text == w.ToString())
                        weatherDropdown.value = i;
                }
            }

            weatherDropdown.RefreshShownValue();

            moonsDropdown.onValueChanged.AddListener((int newVal) =>
            {
                weatherDropdown.ClearOptions();
                weatherDropdown.value = 0;

                List<TMP_Dropdown.OptionData> _weatherOptions = new List<TMP_Dropdown.OptionData>();

                var _weatherOptionsViable = LootrunBase.MoonAvalableWeathers(LootrunBase.MoonNameToID(moonsDropdown.options[moonsDropdown.value].text));

                _weatherOptions.Add(new TMP_Dropdown.OptionData("Random"));
                for (int i = 0; i < _weatherOptionsViable.Count; i++)
                {
                    _weatherOptions.Add(new TMP_Dropdown.OptionData(_weatherOptionsViable[i].ToString()));
                }

                weatherDropdown.AddOptions(_weatherOptions);
            });

            beesToggle.isOn = LootrunBase.currentRunSettings.bees;
            randomSeedToggle.isOn = LootrunBase.currentRunSettings.randomseed;
            SCrusierToggle.isOn = LootrunBase.currentRunSettings.startCrusier;
            SJetpacksToggle.isOn = LootrunBase.currentRunSettings.startJetpack;
            specialsToggle.isOn = LootrunBase.currentRunSettings.spacials;

            if (LootrunBase.currentRunSettings.money != 0)
                moneyAmountInput.text = LootrunBase.currentRunSettings.money.ToString();

            if (LootrunBase.currentRunSettings.seed != 0)
                seedInput.text = LootrunBase.currentRunSettings.seed.ToString();

            //buttons

            moneyAmountInput.onValueChanged.AddListener((string newVal) =>
            {
                if (int.TryParse(newVal, out int resSeed))
                {
                    if (resSeed < 0)
                    {
                        moneyAmountInput.text = "";
                    }
                }
                else
                    moneyAmountInput.text = "";
            });

            seedInput.onValueChanged.AddListener((string newVal) =>
            {
                if (int.TryParse(newVal, out int resSeed))
                {
                    if (resSeed < 0)
                    {
                        seedInput.text = "";
                    }
                }
                else
                    seedInput.text = "";
            });

            GameObject speedlootBack = GameObject.Instantiate(speedlootButton, speedlootMenuContainer.transform);
            speedlootBack.name = "speedlootBack";
            speedlootBack.transform.localPosition = new Vector3(0, -90, 0);
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
            speedlootStart.transform.localPosition = new Vector3(0, -65, 0);
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

                s.bees = beesToggle.isOn;
                s.spacials = specialsToggle.isOn;
                s.startJetpack = SJetpacksToggle.isOn;
                s.startCrusier = SCrusierToggle.isOn;
                s.randomseed = randomSeedToggle.isOn;

                if (int.TryParse(seedInput.text, out int resSeed))
                    s.seed = resSeed;
                else
                    s.seed = 0;

                if (int.TryParse(moneyAmountInput.text, out int resMoney))
                    s.money = resMoney;
                else
                    s.money = 0;

                LootrunBase.currentRunSettings = s;

                GameNetworkManager.Instance.StartHost();
            });
            TextMeshProUGUI speedlootStartButtontext = speedlootStart.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            speedlootStartButtontext.transform.localPosition = Vector3.zero;
            speedlootStartButtontext.alignment = TextAlignmentOptions.Center;
            speedlootStartButtontext.text = "[ Start ]";

            //options

            speedlootB.onClick.AddListener(() =>
            {
                speedlootMenuContainer.SetActive(true);
            });

            UpdateBest(0);
            toggleSeedInput(randomSeedToggle.isOn);
        }

        public static void toggleSeedInput(bool val)
        {
            if (val)
            {
                seedInput.text = "";
                seedInput.interactable = false;
            }
            else
                seedInput.interactable = true;
        }

        public static void UpdateBest<T>(T type)
        {
            if (weatherDropdown.options.Count == 0) return;
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

            s.bees = beesToggle.isOn;
            s.spacials = specialsToggle.isOn;
            s.startJetpack = SJetpacksToggle.isOn;
            s.startCrusier = SCrusierToggle.isOn;
            s.randomseed = randomSeedToggle.isOn;

            if (int.TryParse(seedInput.text, out int resSeed))
                s.seed = resSeed;
            else
                s.seed = 0;

            if (int.TryParse(moneyAmountInput.text, out int resMoney))
                s.money = resMoney;
            else
                s.money = 0;

            LootrunBase.currentRunSettings = s;

            float bestPrecent = 0;
            float bestTime = -1;
            for (int i = 0; i < LootrunBase.allLootruns.Count; i++)
            {
                if (LootrunBase.currentRunSettings.compare(LootrunBase.allLootruns[i].First))
                {
                    float currSPrecent = ((float)LootrunBase.allLootruns[i].Second.scrapCollectedOutOf.x) / LootrunBase.allLootruns[i].Second.scrapCollectedOutOf.y;
                    if (bestTime == -1)
                    {
                        bestTime = LootrunBase.allLootruns[i].Second.time;
                        bestPrecent = currSPrecent;
                        continue;
                    }

                    if (currSPrecent > bestPrecent)
                    {
                        bestTime = LootrunBase.allLootruns[i].Second.time;
                        bestPrecent = currSPrecent;
                    }
                    else if (currSPrecent == bestPrecent && LootrunBase.allLootruns[i].Second.time < bestTime)
                    {
                        bestTime = LootrunBase.allLootruns[i].Second.time;
                        bestPrecent = currSPrecent;
                    }
                }
            }


            string currentBestInfo = string.Empty;

            if (bestTime == -1)
                currentBestInfo = "No run found!";
            else
                currentBestInfo = string.Format("{0:0.0}% - {1}", bestPrecent * 100, LootrunBase.SecsToTimer(bestTime));

            bestRunText.text = currentBestInfo;
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