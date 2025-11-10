using HarmonyLib;
using Lootrun;
using Lootrun.types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LootrunRunCell : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playersLabel;
    [SerializeField] private TextMeshProUGUI scrapCollectedLabel;
    [SerializeField] private TextMeshProUGUI timeSpentLabel;

    [Space]

    [SerializeField] private TextMeshProUGUI moonLabel;
    [SerializeField] private TextMeshProUGUI weatherLabel;

    [Space]

    [SerializeField] private TextMeshProUGUI beesLabel;
    [SerializeField] private TextMeshProUGUI specialsLabel;
    [SerializeField] private TextMeshProUGUI isEndlessLabel;
    [SerializeField] private TextMeshProUGUI cruiserLabel;

    [Space]

    [SerializeField] private TextMeshProUGUI moneyLabel;
    [SerializeField] private TextMeshProUGUI seedLabel;

    [Space]

    [SerializeField] private Transform itemsContent;
    [SerializeField] private GameObject itemPrefab;

    private LootrunResults myResults = null;
    private LootrunSettingsMenu settingsMenu = null;

    public event UnityAction<LootrunPreset> OnUseCallback;

    public void SetSettingsMenuRef(LootrunSettingsMenu menu)
    {
        settingsMenu = menu;
    }

    public void Setup(LootrunResults lootrunResults)
    {
        playersLabel.text = $"Players: {string.Join(", ", lootrunResults.players)}";
        float precentOfScrapCollected = ((float)lootrunResults.scrapCollectedOutOf.x) / lootrunResults.scrapCollectedOutOf.y * 100;
        scrapCollectedLabel.text = $"Scrap Collected: {lootrunResults.scrapCollectedOutOf.x}/{lootrunResults.scrapCollectedOutOf.y} ({precentOfScrapCollected}%)";
        timeSpentLabel.text = $"Time Spent: {LootrunBase.SecsToTimer(lootrunResults.time)}";

        moonLabel.text = $"Moon: {LootrunBase.MoonIDToName(lootrunResults.presetUsed.moon)}";
        weatherLabel.text = $"Weather: {(lootrunResults.presetUsed.weatherType == -2 ? "Random" : Enum.GetName(typeof(LevelWeatherType), (LevelWeatherType)lootrunResults.presetUsed.weatherType))}";

        beesLabel.color = lootrunResults.presetUsed.countBees ? Color.green : Color.gray;
        specialsLabel.color = lootrunResults.presetUsed.countSpecials ? Color.green : Color.gray;
        isEndlessLabel.color = lootrunResults.presetUsed.isEndless ? Color.green : Color.gray;
        cruiserLabel.color = lootrunResults.presetUsed.cruiserOnStart ? Color.green : Color.gray;

        moneyLabel.text = $"Money: {lootrunResults.presetUsed.money}";
        seedLabel.text = $"Seed: {(lootrunResults.presetUsed.seed == -1 ? "Random" : lootrunResults.presetUsed.seed.ToString())}";

        foreach (Transform child in itemsContent)
            Destroy(child.gameObject);

        if (settingsMenu != null)
        {
            foreach (var item in lootrunResults.presetUsed.items)
            {
                if (!settingsMenu.allItems.ContainsKey(item.itemName)) continue;

                var itemSelect = Instantiate(itemPrefab, itemsContent);
                var cellScript = itemSelect.GetComponent<LootrunItemCell>();

                cellScript.Setup(settingsMenu.allItems[item.itemName], item.quantity, true);
                cellScript.DisableAll();
            }
        }

        myResults = lootrunResults;
    }

    public void OnUse()
    {
        if (myResults == null) return;

        OnUseCallback?.Invoke(myResults.presetUsed);
    }
}
