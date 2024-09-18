using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Lootrun.hooks;
using Lootrun.types;
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
