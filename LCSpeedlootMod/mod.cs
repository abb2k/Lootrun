using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Lootrun.hooks;
using Lootrun.types;
using System.Collections.Generic;
using System.Security.Policy;
using TMPro;
using UnityEngine;

namespace Lootrun
{
    [BepInPlugin(GUID, modName, modVersion)]
    public class LootrunBase : BaseUnityPlugin
    {
        public const string GUID = "abb2k.Lootrun";
        public const string modName = "Lootrun";
        public const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(GUID);

        private static LootrunBase Instance;

        public static ManualLogSource mls;

        public static AssetBundle bundle;

        public static Sprite DialogueBox1Frame_5, BloodStain2, DialogueBoxSimple_1, DialogueBoxSimple, DropdownArrow;

        public static LootrunSettings currentRunSettings = new LootrunSettings();
        public static LootrunResults currentRunResults = new LootrunResults();

        public static bool isInLootrun;

        public static float LootrunTime;

        public static int playersThisRound;

        public static TextMeshProUGUI timerText;

        public static List<GrabbableObject> CurrentRoundScrap = new List<GrabbableObject>();
        public static List<GrabbableObject> CurrentRoundBees = new List<GrabbableObject>();
        public static List<GrabbableObject> CurrentRoundSpecials = new List<GrabbableObject>();

        public static Dictionary<LootrunSettings, LootrunResults> allLootruns = new Dictionary<LootrunSettings, LootrunResults>();

        void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }

            mls = Logger;

            mls.LogInfo("Lootrun has loaded :D");

            harmony.PatchAll(typeof(NetworkObjectManagerPatch));
            harmony.PatchAll(typeof(LootrunBase));
            harmony.PatchAll(typeof(MenuManagerHook));
            harmony.PatchAll(typeof(StartOfRoundHook));
            harmony.PatchAll(typeof(SaveGamePatch));
            harmony.PatchAll(typeof(TimeOfDayEventsTranspiler));
            harmony.PatchAll(typeof(UpdateProfitQuotaCurrentTimePatch));
            harmony.PatchAll(typeof(PassTimeToNextDayPatch));
            harmony.PatchAll(typeof(waitForScrapToSpawnToSyncPatch));
            harmony.PatchAll(typeof(StartGamePatch));
            harmony.PatchAll(typeof(ShipHasLeftPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(TimeOfDayUpdatePatch));
            harmony.PatchAll(typeof(FillEndGameStatsPatch));
            harmony.PatchAll(typeof(ApparatusStartPatch));
            harmony.PatchAll(typeof(RedLocustBeesStartPatch));
            harmony.PatchAll(typeof(NutcrackerEnemyAIGrabGunPatch));
            harmony.PatchAll(typeof(KnifePatch));
            harmony.PatchAll(typeof(AutoSaveShipDataPatch));
            harmony.PatchAll(typeof(LoadShipGrabbableItemsPatch));

            allLootruns = ES3.Load("allLootruns", Application.persistentDataPath + "/LootrunSave", new Dictionary<LootrunSettings, LootrunResults>());

            string location = Instance.Info.Location;
            location = location.TrimEnd("Lootrun.dll".ToCharArray());
            bundle = AssetBundle.LoadFromFile(location + "modassets");

            if (bundle != null)
            {
                DialogueBox1Frame_5 = LootrunBase.bundle.LoadAsset<Sprite>("DialogueBox1Frame 5");
                BloodStain2 = LootrunBase.bundle.LoadAsset<Sprite>("BloodStain2");
                DialogueBoxSimple_1 = LootrunBase.bundle.LoadAsset<Sprite>("DialogueBoxSimple 1");
                DialogueBoxSimple = LootrunBase.bundle.LoadAsset<Sprite>("DialogueBoxSimple");
                DropdownArrow = LootrunBase.bundle.LoadAsset<Sprite>("DropdownArrow");
            }
        }

        public static string SecsToTimer(float secondsTimer)
        {
            string toReturn = string.Empty;

            int minutes = 0;
            int hours = 0;

            while (secondsTimer >= 60)
            {
                secondsTimer -= 60;
                minutes++;
            }

            while (minutes >= 60)
            {
                minutes -= 60;
                hours++;
            }

            int seconds = (int)secondsTimer;
            int milliseconds = (int)((secondsTimer % 1) * 100);


            toReturn = string.Format("{0:0}:{1:00}:{2:00}:{3:00}", hours, minutes, seconds, milliseconds);

            return toReturn;
        }

        public static int MoonNameToID(string moonName)
        {
            switch (moonName)
            {
                case "41-Experimentation":
                    return 0;

                case "220-Assurance":
                    return 1;

                case "56-Vow":
                    return 2;

                case "21-Offense":
                    return 8;

                case "61-March":
                    return 4;

                case "20-Adamance":
                    return 5;

                case "85-Rend":
                    return 6;

                case "7-Dine":
                    return 7;

                case "8-Titan":
                    return 9;

                case "68-Artifice":
                    return 10;

                case "5-Embrion":
                    return 12;

                default:
                    return 0;
            }
        }

        public static List<LevelWeatherType> MoonAvalableWeathers(int moonID)
        {
            List<LevelWeatherType> toReturn = new List<LevelWeatherType>
            {
                LevelWeatherType.None
            };

            switch (moonID)
            {
                case 0:
                    toReturn.Add(LevelWeatherType.Rainy);
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 1:
                    toReturn.Add(LevelWeatherType.Rainy);
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 2:
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 8:
                    toReturn.Add(LevelWeatherType.Rainy);
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 4:
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 5:
                    toReturn.Add(LevelWeatherType.Rainy);
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 6:
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 7:
                    toReturn.Add(LevelWeatherType.Rainy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 9:
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 10:
                    toReturn.Add(LevelWeatherType.Rainy);
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                case 12:
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;

                default:
                    toReturn.Add(LevelWeatherType.Rainy);
                    toReturn.Add(LevelWeatherType.Stormy);
                    toReturn.Add(LevelWeatherType.Foggy);
                    toReturn.Add(LevelWeatherType.Flooded);
                    toReturn.Add(LevelWeatherType.Eclipsed);
                    break;
            }

            return toReturn;
        }

        public static LevelWeatherType weatherNameToType(string name)
        {
            switch (name)
            {
                case "None":
                    return LevelWeatherType.None;
                case "DustClouds":
                    return LevelWeatherType.DustClouds;
                case "Rainy":
                    return LevelWeatherType.Rainy;
                case "Stormy":
                    return LevelWeatherType.Stormy;
                case "Foggy":
                    return LevelWeatherType.Foggy;
                case "Flooded":
                    return LevelWeatherType.Flooded;
                case "Eclipsed":
                    return LevelWeatherType.Eclipsed;

                default:
                    return LevelWeatherType.None;
            }
        }
    }
}
