using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using HarmonyLib.Tools;
using Lootrun.hooks;
using Lootrun.types;
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

        public static bool isInLootrun;

        public static float LootrunTime;

        public static TextMeshProUGUI timerText;

        public static List<GrabbableObject> CurrentRoundScrap = new List<GrabbableObject>();

        void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }

            mls = Logger;

            mls.LogInfo("Lootrun has loaded :D");

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
            harmony.PatchAll(typeof(CollectNewScrapForThisRoundPatch));


            

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
    }
}
