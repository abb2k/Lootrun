using GameNetcodeStuff;
using Lootrun.types;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.hooks
{
    public class LootrunNetworkHandler : NetworkBehaviour
    {
        public static LootrunNetworkHandler instance;

        void Awake()
        {
            instance = this;
        }

        [ClientRpc]
        public void SyncInLootrunClientRpc(ulong playerID, bool enable)
        {
            LootrunBase.isInLootrun = enable;

            if (LootrunBase.timerText || !LootrunBase.isInLootrun) return;

            var text = GameObject.Instantiate(LootrunBase.bundle.LoadAsset<GameObject>("RunTimer"), StartOfRound.Instance.allPlayerObjects[playerID].GetComponent<PlayerControllerB>().playerHudUIContainer);
            text.name = "Lootrun time text";
            text.transform.localPosition = new Vector3(325, -210, 0);
            TextMeshProUGUI textComp = text.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            textComp.text = LootrunBase.SecsToTimer(0);

            LootrunBase.timerText = textComp;
        }

        [ClientRpc]
        public void SyncLootrunTimerClientRpc(float time)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                return;
            LootrunBase.LootrunTime = time;
            LootrunBase.timerText.text = LootrunBase.SecsToTimer(LootrunBase.LootrunTime);
        }

        [ClientRpc]
        public void SyncLootrunResultsClientRpc(LootrunSettings s, LootrunResults res)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                return;
            LootrunBase.currentRunSettings = s;
            LootrunBase.currentRunResults = res;
        }

        [ClientRpc]
        public void SyncStatsClientRpc(int moon, int weather, int money)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                return;

            StartOfRound.Instance.currentLevel = StartOfRound.Instance.levels[moon];
            StartOfRound.Instance.currentLevelID = moon;
            TimeOfDay.Instance.currentLevel = StartOfRound.Instance.currentLevel;
            RoundManager.Instance.currentLevel = StartOfRound.Instance.levels[moon];


            StartOfRound.Instance.currentLevel.currentWeather = (LevelWeatherType)weather;

            TimeOfDay timeOfDay = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
            timeOfDay.quotaFulfilled = 0;
            timeOfDay.timesFulfilledQuota = 0;
            timeOfDay.UpdateProfitQuotaCurrentTime();

            Terminal t = GameObject.FindObjectOfType<Terminal>();
            t.groupCredits = money;

            StartOfRound.Instance.ChangePlanet();
            StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

            StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\nNever";

            StartOfRound.Instance.profitQuotaMonitorText.text = "PROFIT QUOTA:\nAll of them";
        }

        [ClientRpc]
        public void ClearInventoryClientRpc()
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

        public static void onClientConnected(ulong playerID)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                LootrunNetworkHandler.instance.SyncInLootrunClientRpc(playerID, LootrunBase.isInLootrun);
                if (LootrunBase.isInLootrun)
                    LootrunNetworkHandler.instance.SyncStatsClientRpc(LootrunBase.currentRunSettings.moon, (int)StartOfRound.Instance.currentLevel.overrideWeatherType, LootrunBase.currentRunSettings.money);
            }
        }
    }
}
