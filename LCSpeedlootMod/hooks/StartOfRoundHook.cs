using GameNetcodeStuff;
using HarmonyLib;
using Lootrun.types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace Lootrun.hooks
{
    [HarmonyPatch]
    internal class StartOfRoundHook
    {
        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Start")]
        static void StartHook(StartOfRound __instance)
        {
            LootrunNetworkHandler.LevelEvent += ReceivedEventFromServer;
            LootrunNetworkHandler.LootrunResEvent += ReceivedLootrunResFromServer;
            if (!LootrunBase.isInLootrun) return;

            __instance.currentLevel = __instance.levels[LootrunBase.currentRunSettings.moon];
            __instance.currentLevelID = LootrunBase.currentRunSettings.moon;
            TimeOfDay.Instance.currentLevel = __instance.currentLevel;
            RoundManager.Instance.currentLevel = __instance.levels[LootrunBase.currentRunSettings.moon];

            if (LootrunBase.currentRunSettings.weather == -2)
            {
                __instance.currentLevel.overrideWeather = true;
                __instance.currentLevel.overrideWeatherType = __instance.currentLevel.randomWeathers[UnityEngine.Random.Range(0, __instance.currentLevel.randomWeathers.Length)].weatherType;

                LootrunBase.mls.LogInfo("w - " + __instance.currentLevel.overrideWeatherType.ToString());
            }
            else
            {
                __instance.currentLevel.overrideWeather = true;
                __instance.currentLevel.overrideWeatherType = (LevelWeatherType)LootrunBase.currentRunSettings.weather;
            }

            TimeOfDay timeOfDay = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
            timeOfDay.quotaFulfilled = 0;
            timeOfDay.timesFulfilledQuota = 0;
            timeOfDay.UpdateProfitQuotaCurrentTime();

            __instance.ChangePlanet();
            __instance.SetPlanetsWeather();
            __instance.SetMapScreenInfoToCurrentLevel();

            __instance.overrideRandomSeed = !LootrunBase.currentRunSettings.randomseed;
            __instance.overrideSeedNumber = LootrunBase.currentRunSettings.seed;

            StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\nNever";

            StartOfRound.Instance.profitQuotaMonitorText.text = "PROFIT QUOTA:\nAll of them";
        }

        static void ReceivedEventFromServer(string eventName)
        {
            if (eventName == "SetLootrunEnabled")
            {
                LootrunBase.isInLootrun = true;
                return;
            }
            if (eventName == "ClearInventory")
            {
                for (int i = 0; i < StartOfRound.Instance.localPlayerController.ItemSlots.Length; i++)
                {
                    StartOfRound.Instance.localPlayerController.ItemSlots[i] = null;
                }

                for (int i = 0; i < HUDManager.Instance.itemSlotIcons.Length; i++)
                {
                    HUDManager.Instance.itemSlotIcons[i].enabled = false;
                }
            }
        }

        static void ReceivedLootrunResFromServer(LootrunSettings s, LootrunResults res)
        {
            LootrunBase.currentRunSettings = s;
            LootrunBase.currentRunResults = res;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "LoadShipGrabbableItems")]
    internal class LoadShipGrabbableItemsPatch
    {
        [HarmonyPostfix]
        static void LoadShipGrabbableItemsHook(StartOfRound __instance)
        {
            GameObject jetpackPrefab = null;
            GameObject weedkillerPrefab = null;

            for (int i = 0; i < __instance.allItemsList.itemsList.Count; i++)
            {
                LootrunBase.mls.LogInfo(__instance.allItemsList.itemsList[i].itemName);
                if (__instance.allItemsList.itemsList[i].itemName == "Jetpack")
                {
                    jetpackPrefab = __instance.allItemsList.itemsList[i].spawnPrefab;
                }

                if (__instance.allItemsList.itemsList[i].itemName == "Weed killer")
                {
                    weedkillerPrefab = __instance.allItemsList.itemsList[i].spawnPrefab;
                }
            }

            if (LootrunBase.currentRunSettings.startJetpack)
            {
                for (int i = 0; i < 4; i++)
                {
                    GrabbableObject component = UnityEngine.Object.Instantiate(jetpackPrefab, new Vector3(-3.5f, 1, -14.5f), Quaternion.identity, __instance.elevatorTransform).GetComponent<GrabbableObject>();
                    component.fallTime = 1f;
                    component.hasHitGround = true;
                    component.scrapPersistedThroughRounds = true;
                    component.isInElevator = true;
                    component.isInShipRoom = true;
                    component.NetworkObject.Spawn();
                }
            }

            GameObject crusierPrefab = null;

            for (int i = 0; i < __instance.VehiclesList.Length; i++)
            {
                if (__instance.VehiclesList[i].name == "CompanyCruiser")
                    crusierPrefab = __instance.VehiclesList[i];
            }

            if (LootrunBase.currentRunSettings.startCrusier)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(crusierPrefab, __instance.magnetPoint.position + __instance.magnetPoint.forward * 5f, Quaternion.identity, RoundManager.Instance.VehiclesContainer);
                __instance.attachedVehicle = gameObject.GetComponent<VehicleController>();
                __instance.isObjectAttachedToMagnet = true;
                __instance.attachedVehicle.NetworkObject.Spawn();
                __instance.magnetOn = true;
                __instance.magnetLever.initialBoolState = true;
                __instance.magnetLever.setInitialState = true;
                __instance.magnetLever.SetInitialState();

                if (weedkillerPrefab)
                    for (int i = 0; i < 2; i++)
                    {
                        GrabbableObject component = UnityEngine.Object.Instantiate(weedkillerPrefab, new Vector3(10, 1.5f, -13), Quaternion.identity, __instance.elevatorTransform).GetComponent<GrabbableObject>();
                        component.fallTime = 1f;
                        component.hasHitGround = true;
                        component.scrapPersistedThroughRounds = true;
                        component.isInElevator = true;
                        component.isInShipRoom = true;
                        component.NetworkObject.Spawn();
                    }
            }
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "PassTimeToNextDay")]
    internal class PassTimeToNextDayPatch
    {
        [HarmonyPrefix]
        static bool PassTimeToNextDayHook()
        {
            LootrunBase.LootrunTime = 0;
            if (LootrunBase.isInLootrun) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ShipHasLeft))]
    internal class ShipHasLeftPatch
    {
        [HarmonyPrefix]
        static void ShipHasLeftHook()
        {
            if (!LootrunBase.isInLootrun) return;

            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                return;

            TimeOfDay.Instance.currentDayTimeStarted = false;

            LootrunResults res = new LootrunResults();

            int validScrapCount = 0;

            for (int i = 0; i < RoundManager.Instance.scrapCollectedThisRound.Count; i++)
            {
                if (LootrunBase.CurrentRoundScrap.Contains(RoundManager.Instance.scrapCollectedThisRound[i]))
                    validScrapCount++;

                if (LootrunBase.currentRunSettings.bees && LootrunBase.CurrentRoundBees.Contains(RoundManager.Instance.scrapCollectedThisRound[i]))
                    validScrapCount++;

                if (LootrunBase.currentRunSettings.spacials && LootrunBase.CurrentRoundSpecials.Contains(RoundManager.Instance.scrapCollectedThisRound[i]))
                    validScrapCount++;
            }

            int scrapCount = LootrunBase.CurrentRoundScrap.Count;
            if (LootrunBase.currentRunSettings.bees)
                scrapCount += LootrunBase.CurrentRoundBees.Count;
            if (LootrunBase.currentRunSettings.spacials)
                scrapCount += LootrunBase.CurrentRoundSpecials.Count;

            res.players = LootrunBase.playersThisRound;
            res.time = LootrunBase.LootrunTime;
            res.scrapCollectedOutOf = new Vector2Int(validScrapCount, scrapCount);

            LootrunBase.currentRunResults = res;

            //LootrunNetworkHandler.Instance.LootrunResultsClientRpc(LootrunBase.currentRunSettings, LootrunBase.currentRunResults);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "AutoSaveShipData")]
    internal class AutoSaveShipDataPatch
    {
        [HarmonyPrefix]
        static bool AutoSaveShipDataHook(StartOfRound __instance)
        {
            if (LootrunBase.isInLootrun)
            {
                HUDManager.Instance.saveDataIconAnimatorB.SetTrigger("save");

                if (!LootrunBase.allLootruns.ContainsKey(LootrunBase.currentRunSettings))
                    LootrunBase.allLootruns.Add(LootrunBase.currentRunSettings, LootrunBase.currentRunResults);
                else
                {
                    if (LootrunBase.allLootruns[LootrunBase.currentRunSettings].scrapCollectedOutOf.x / LootrunBase.allLootruns[LootrunBase.currentRunSettings].scrapCollectedOutOf.y < LootrunBase.currentRunResults.scrapCollectedOutOf.x / LootrunBase.currentRunResults.scrapCollectedOutOf.y)
                    {
                        LootrunBase.allLootruns.Remove(LootrunBase.currentRunSettings);
                        LootrunBase.allLootruns.Add(LootrunBase.currentRunSettings, LootrunBase.currentRunResults);
                    }
                    else if (LootrunBase.allLootruns[LootrunBase.currentRunSettings].scrapCollectedOutOf.x / LootrunBase.allLootruns[LootrunBase.currentRunSettings].scrapCollectedOutOf.y == LootrunBase.currentRunResults.scrapCollectedOutOf.x / LootrunBase.currentRunResults.scrapCollectedOutOf.y)
                    {
                        if (LootrunBase.allLootruns[LootrunBase.currentRunSettings].time < LootrunBase.currentRunResults.time)
                        {
                            LootrunBase.allLootruns.Remove(LootrunBase.currentRunSettings);
                            LootrunBase.allLootruns.Add(LootrunBase.currentRunSettings, LootrunBase.currentRunResults);
                        }
                    }
                    
                }
                ES3.Save("allLootruns", LootrunBase.allLootruns, Application.persistentDataPath + "/LootrunSave");

                //reset everything back to normal

                List<GrabbableObject> items = GameObject.FindObjectsOfType<GrabbableObject>().ToList();
                foreach (GrabbableObject item in items)
                {
                    if (item.TryGetComponent(out NetworkObject netObj))
                        netObj.Despawn();
                    if (item.gameObject)
                        GameObject.Destroy(item.gameObject);
                }

                if (StartOfRound.Instance.localPlayerController.twoHanded)
                {
                    HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, 1.5f, 1f, 0.13f);
                    HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                }

                for (int i = 0; i < StartOfRound.Instance.localPlayerController.ItemSlots.Length; i++)
                {
                    StartOfRound.Instance.localPlayerController.ItemSlots[i] = null;
                }

                for (int i = 0; i < HUDManager.Instance.itemSlotIcons.Length; i++)
                {
                    HUDManager.Instance.itemSlotIcons[i].enabled = false;
                }

                //LootrunNetworkHandler.Instance.EventClientRpc("ClearInventory");

                if (__instance.attachedVehicle != null)
                {
                    __instance.attachedVehicle.NetworkObject.Despawn();

                    if (__instance.attachedVehicle.gameObject != null)
                        GameObject.Destroy(__instance.attachedVehicle.gameObject);
                }

                GameObject jetpackPrefab = null;
                GameObject weedkillerPrefab = null;

                for (int i = 0; i < __instance.allItemsList.itemsList.Count; i++)
                {
                    LootrunBase.mls.LogInfo(__instance.allItemsList.itemsList[i].itemName);
                    if (__instance.allItemsList.itemsList[i].itemName == "Jetpack")
                    {
                        jetpackPrefab = __instance.allItemsList.itemsList[i].spawnPrefab;
                    }

                    if (__instance.allItemsList.itemsList[i].itemName == "Weed killer")
                    {
                        weedkillerPrefab = __instance.allItemsList.itemsList[i].spawnPrefab;
                    }
                }

                if (LootrunBase.currentRunSettings.startJetpack)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        GrabbableObject component = UnityEngine.Object.Instantiate(jetpackPrefab, new Vector3(-3.5f, 1, -14.5f), Quaternion.identity, __instance.elevatorTransform).GetComponent<GrabbableObject>();
                        component.fallTime = 1f;
                        component.hasHitGround = true;
                        component.scrapPersistedThroughRounds = true;
                        component.isInElevator = true;
                        component.isInShipRoom = true;
                        component.NetworkObject.Spawn();
                    }
                }

                GameObject crusierPrefab = null;

                for (int i = 0; i < __instance.VehiclesList.Length; i++)
                {
                    if (__instance.VehiclesList[i].name == "CompanyCruiser")
                        crusierPrefab = __instance.VehiclesList[i];
                }

                if (LootrunBase.currentRunSettings.startCrusier)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(crusierPrefab, __instance.magnetPoint.position + __instance.magnetPoint.forward * 5f, Quaternion.identity, RoundManager.Instance.VehiclesContainer);
                    __instance.attachedVehicle = gameObject.GetComponent<VehicleController>();
                    __instance.isObjectAttachedToMagnet = true;
                    __instance.attachedVehicle.NetworkObject.Spawn();
                    __instance.magnetOn = true;
                    __instance.magnetLever.initialBoolState = true;
                    __instance.magnetLever.setInitialState = true;
                    __instance.magnetLever.SetInitialState();

                    if (weedkillerPrefab)
                        for (int i = 0; i < 2; i++)
                        {
                            GrabbableObject component = UnityEngine.Object.Instantiate(weedkillerPrefab, new Vector3(10, 1.5f, -13), Quaternion.identity, __instance.elevatorTransform).GetComponent<GrabbableObject>();
                            component.fallTime = 1f;
                            component.hasHitGround = true;
                            component.scrapPersistedThroughRounds = true;
                            component.isInElevator = true;
                            component.isInShipRoom = true;
                            component.NetworkObject.Spawn();
                        }
                }

                Terminal t = GameObject.FindObjectOfType<Terminal>();
                t.groupCredits = LootrunBase.currentRunSettings.money;

                if (LootrunBase.currentRunSettings.weather == -2)
                {
                    __instance.currentLevel.overrideWeather = true;
                    __instance.currentLevel.overrideWeatherType = __instance.currentLevel.randomWeathers[UnityEngine.Random.Range(0, __instance.currentLevel.randomWeathers.Length)].weatherType;

                    LootrunBase.mls.LogInfo("w - " + __instance.currentLevel.overrideWeatherType.ToString());
                }
                else
                {
                    __instance.currentLevel.overrideWeather = true;
                    __instance.currentLevel.overrideWeatherType = (LevelWeatherType)LootrunBase.currentRunSettings.weather;
                }

                __instance.ChangePlanet();
                __instance.SetPlanetsWeather();
                __instance.SetMapScreenInfoToCurrentLevel();

                return false;
            }

            return true;
        }
    }
}
