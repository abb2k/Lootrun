using GameNetcodeStuff;
using HarmonyLib;
using Lootrun.types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.hooks
{
    [HarmonyPatch]
    internal class StartOfRoundHook
    {
        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Start")]
        static void StartHook(StartOfRound __instance)
        {
            if (!LootrunBase.isInLootrun) return;

            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                return;

            __instance.ChangeLevel(LootrunBase.currentRunSettings.moon);

            if (LootrunBase.currentRunSettings.weatherType == -2)
            {
                int watherIndex = UnityEngine.Random.Range(0, __instance.currentLevel.randomWeathers.Length);

                __instance.currentLevel.currentWeather = __instance.currentLevel.randomWeathers[watherIndex].weatherType;
            }
            else
            {
                __instance.currentLevel.currentWeather = (LevelWeatherType)LootrunBase.currentRunSettings.weatherType;
            }

            TimeOfDay timeOfDay = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
            timeOfDay.quotaFulfilled = 0;
            timeOfDay.timesFulfilledQuota = 0;
            timeOfDay.UpdateProfitQuotaCurrentTime();

            __instance.ChangePlanet();
            __instance.SetMapScreenInfoToCurrentLevel();

            __instance.overrideRandomSeed = LootrunBase.currentRunSettings.seed != -1;
            __instance.overrideSeedNumber = LootrunBase.currentRunSettings.seed;

            foreach (var item in __instance.levels)
            {
                LootrunBase.mls.LogInfo($"{item.PlanetName} - {item.levelID}");
            }

            StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\nNever";

            StartOfRound.Instance.profitQuotaMonitorText.text = "PROFIT QUOTA:\nAll of them";

            LootrunNetworkHandler.instance.SyncStatsClientRpc(LootrunBase.currentRunSettings.moon, (int)__instance.currentLevel.currentWeather, LootrunBase.currentRunSettings.money);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "LoadShipGrabbableItems")]
    internal class LoadShipGrabbableItemsPatch
    {
        [HarmonyPostfix]
        static void LoadShipGrabbableItemsHook(StartOfRound __instance)
        {
            if (!LootrunBase.isInLootrun) return;
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                return;

            SpawnStartItems(__instance);
        }
        public static void SpawnStartItems(StartOfRound SOR)
        {
            List<(GameObject, int)> itemPrefabs = new List<(GameObject, int)>();

            for (int i = 0; i < LootrunBase.currentRunSettings.items.Count; i++)
            {
                var foundItemIndex = SOR.allItemsList.itemsList.FindIndex(x => x.itemName.Equals(LootrunBase.currentRunSettings.items[i].itemName));
                if (foundItemIndex != -1)
                {
                    itemPrefabs.Add((SOR.allItemsList.itemsList[foundItemIndex].spawnPrefab, LootrunBase.currentRunSettings.items[i].quantity));
                }
            }

            foreach (var item in itemPrefabs)
            {
                for (int i = 0; i < item.Item2; i++)
                {
                    GrabbableObject component = UnityEngine.Object.Instantiate(item.Item1, new Vector3(-3.5f, 1, -14.5f), Quaternion.identity, SOR.elevatorTransform).GetComponent<GrabbableObject>();
                    component.fallTime = 1f;
                    component.hasHitGround = true;
                    component.scrapPersistedThroughRounds = true;
                    component.isInElevator = true;
                    component.isInShipRoom = true;
                    component.NetworkObject.Spawn();
                }
            }

            GameObject crusierPrefab = null;

            for (int i = 0; i < SOR.VehiclesList.Length; i++)
            {
                if (SOR.VehiclesList[i].name == "CompanyCruiser")
                    crusierPrefab = SOR.VehiclesList[i];
            }

            if (LootrunBase.currentRunSettings.cruiserOnStart)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(crusierPrefab, SOR.magnetPoint.position + SOR.magnetPoint.forward * 5f, Quaternion.identity, RoundManager.Instance.VehiclesContainer);
                SOR.attachedVehicle = gameObject.GetComponent<VehicleController>();
                SOR.isObjectAttachedToMagnet = true;
                SOR.attachedVehicle.NetworkObject.Spawn();
                SOR.magnetOn = true;
                SOR.magnetLever.initialBoolState = true;
                SOR.magnetLever.setInitialState = true;
                SOR.magnetLever.SetInitialState();

                var weedKillerIndex = SOR.allItemsList.itemsList.FindIndex(x => x.itemName.Equals("Weed killer"));

                if (weedKillerIndex != -1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        GrabbableObject component = UnityEngine.Object.Instantiate(
                            SOR.allItemsList.itemsList[weedKillerIndex].spawnPrefab,
                            new Vector3(10, 1.5f, -13),
                            Quaternion.identity,
                            SOR.elevatorTransform
                        ).GetComponent<GrabbableObject>();

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

                if (LootrunBase.currentRunSettings.countBees && LootrunBase.CurrentRoundBees.Contains(RoundManager.Instance.scrapCollectedThisRound[i]))
                    validScrapCount++;

                if (LootrunBase.currentRunSettings.countSpecials && LootrunBase.CurrentRoundSpecials.Contains(RoundManager.Instance.scrapCollectedThisRound[i]))
                    validScrapCount++;
            }

            int scrapCount = LootrunBase.CurrentRoundScrap.Count;
            if (LootrunBase.currentRunSettings.countBees)
                scrapCount += LootrunBase.CurrentRoundBees.Count;
            if (LootrunBase.currentRunSettings.countSpecials)
                scrapCount += LootrunBase.CurrentRoundSpecials.Count;

            res.players = LootrunBase.playersThisRound;
            res.presetUsed = LootrunBase.currentRunSettings;
            res.time = LootrunBase.LootrunTime;
            res.scrapCollectedOutOf = new Vector2Int(validScrapCount, scrapCount);

            LootrunBase.currentRunResult = res;

            LootrunNetworkHandler.instance.SyncLootrunResultsClientRpc(LootrunBase.currentRunSettings, res);
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

                LootrunBase.SaveResults(LootrunBase.currentRunResult);

                //reset everything back to normal

                if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                    return false;

                LootrunNetworkHandler.instance.ClearInventoryClientRpc();

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

                if (__instance.attachedVehicle != null)
                {
                    __instance.attachedVehicle.NetworkObject.Despawn();

                    if (__instance.attachedVehicle.gameObject != null)
                        GameObject.Destroy(__instance.attachedVehicle.gameObject);
                }

                LoadShipGrabbableItemsPatch.SpawnStartItems(__instance);

                Terminal t = GameObject.FindObjectOfType<Terminal>();
                t.groupCredits = LootrunBase.currentRunSettings.money;

                if (LootrunBase.currentRunSettings.weatherType == -2)
                {
                    __instance.currentLevel.currentWeather = __instance.currentLevel.randomWeathers[UnityEngine.Random.Range(0, __instance.currentLevel.randomWeathers.Length)].weatherType;
                }
                else
                {
                    __instance.currentLevel.currentWeather = (LevelWeatherType)LootrunBase.currentRunSettings.weatherType;
                }

                __instance.ChangePlanet();
                __instance.SetMapScreenInfoToCurrentLevel();

                LootrunNetworkHandler.instance.SyncStatsClientRpc(LootrunBase.currentRunSettings.moon, (int)__instance.currentLevel.currentWeather, LootrunBase.currentRunSettings.money);

                return false;
            }

            return true;
        }
    }
}
