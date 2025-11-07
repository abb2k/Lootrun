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

public class LootrunSettingsMenu : MonoBehaviour
{
    private LootrunPreset myPreset = new LootrunPreset();

    public event UnityAction<LootrunPreset> onPresetChanged;

    public Dictionary<string, Item> allItems = new Dictionary<string, Item>();

    [SerializeField] private TMP_Dropdown moonsDropdown;
    [SerializeField] private TMP_Dropdown weatherDropdown;
    [SerializeField] private Toggle beesToggle;
    [SerializeField] private Toggle specialsToggle;
    [SerializeField] private Toggle randomSeedToggle;
    [SerializeField] private Toggle endlessToggle;
    [SerializeField] private Toggle cruiserOnStartToggle;
    [SerializeField] private TMP_InputField seedInput;
    [SerializeField] private TMP_InputField moneyInput;
    [SerializeField] private CanvasGroup seedCG;

    [Space]

    [SerializeField] private GameObject itemCellPrefab;
    [SerializeField] private Transform itemsHolder;
    public GameObject itemSelectMenu;
    [SerializeField] private Transform itemSelectMenuParent;

    private void Start()
    {
        var weatherOptions = new List<TMP_Dropdown.OptionData>();
        var weatherNames = Enum.GetNames(typeof(LevelWeatherType));

        weatherOptions.Add(new TMP_Dropdown.OptionData("Random"));
        foreach (var name in weatherNames)
        {
            weatherOptions.Add(new TMP_Dropdown.OptionData(name));
        }

        weatherDropdown.AddOptions(weatherOptions);
        weatherDropdown.RefreshShownValue();

        List<TMP_Dropdown.OptionData> moonsOptions = new List<TMP_Dropdown.OptionData>
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

        moonsDropdown.AddOptions(moonsOptions);
        moonsDropdown.RefreshShownValue();

        var allItemsList = Resources.FindObjectsOfTypeAll<GrabbableObject>();

        foreach (var item in allItemsList)
        {
            if (allItems.ContainsKey(item.itemProperties.itemName)) continue;

            allItems.Add(item.itemProperties.itemName, item.itemProperties);

            var itemSelect = Instantiate(itemCellPrefab, itemSelectMenuParent);
            var cellScript = itemSelect.GetComponent<LootrunItemCell>();

            cellScript.Setup(item.itemProperties, 1, true);
            cellScript.onSelectClicked += OnCellSelected;
        }

        SetToPreset(myPreset);
    }

    public void AddItemCell(Item item, int quantity = 1)
    {
        var itemCell = Instantiate(itemCellPrefab, itemsHolder);
        var cellScript = itemCell.GetComponent<LootrunItemCell>();

        cellScript.Setup(item, quantity);
        cellScript.onCellChanged += OnItemCellChanged;
        cellScript.onCellDelete += OnItemCellDeleted;
    }

    public void SetToPreset(LootrunPreset lootrunPreset)
    {
        myPreset = lootrunPreset;

        weatherDropdown.value = myPreset.weatherType == -2 ? 0 : myPreset.weatherType + 1;
        weatherDropdown.RefreshShownValue();

        string currentMoonName = LootrunBase.MoonIDToName(myPreset.moon);
        for (int i = 0; i < moonsDropdown.options.Count; i++)
            if (moonsDropdown.options[i].text == currentMoonName)
                moonsDropdown.value = i;
        moonsDropdown.RefreshShownValue();

        beesToggle.isOn = myPreset.countBees;
        specialsToggle.isOn = myPreset.countSpecials;
        randomSeedToggle.isOn = myPreset.seed != -1;
        endlessToggle.isOn = myPreset.isEndless;
        cruiserOnStartToggle.isOn = myPreset.cruiserOnStart;

        seedInput.text = myPreset.seed == -1 ? string.Empty : myPreset.seed.ToString();
        moneyInput.text = myPreset.money.ToString();

        foreach (Transform child in itemsHolder)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in myPreset.items)
        {
            if (!allItems.ContainsKey(item.itemName)) continue;

            AddItemCell(allItems[item.itemName], item.quantity);
        }

        UpdateSeedInputVisibility();
    }

    void OnItemCellChanged(LootrunItemCell cell)
    {
        for (int i = 0; i < myPreset.items.Count; i++)
        {
            if (myPreset.items[i].itemName == cell.itemData.itemName)
                myPreset.items[i] = cell.itemData;
        }
    }

    void OnItemCellDeleted(LootrunItemCell cell)
    {
        myPreset.items.RemoveAll(x => x.itemName == cell.itemData.itemName);

        Destroy(cell.gameObject);
    }

    void UpdateSeedInputVisibility()
    {
        seedCG.alpha = myPreset.seed == -1 ? .2f : 1f;
        seedCG.interactable = myPreset.seed != -1;
    }

    void SendChanges() { onPresetChanged?.Invoke(myPreset); }

    public void OnMoonChanged(int newMoon)
    {
        myPreset.moon = newMoon;

        SendChanges();
    }

    public void OnWeatherChanged(int newWeather)
    {
        if (!Enum.TryParse(weatherDropdown.options[newWeather].text, out LevelWeatherType type)){
            myPreset.weatherType = -2;
        }
        else
            myPreset.weatherType = (int)type;

        SendChanges();
    }

    public void OnBeesChanged(bool bees)
    {
        myPreset.countBees = bees;

        SendChanges();
    }

    public void OnCruiserOnStartChanged(bool cruiserOnStart)
    {
        myPreset.cruiserOnStart = cruiserOnStart;

        SendChanges();
    }

    public void OnSpecialsChanged(bool specials)
    {
        myPreset.countSpecials = specials;

        SendChanges();
    }

    public void OnEndlessChanged(bool endless)
    {
        myPreset.isEndless = endless;

        UpdateSeedInputVisibility();

        SendChanges();
    }

    public void OnRandomSeedChanged(bool rs)
    {
        myPreset.seed = !rs ? 0 : -1;

        seedInput.text = "0";

        UpdateSeedInputVisibility();

        SendChanges();
    }

    public void OnSeedInputChanged(string newSeed)
    {
        if (!int.TryParse(newSeed, out var seed)) return;

        myPreset.seed = seed;

        UpdateSeedInputVisibility();

        SendChanges();
    }

    public void OnMoneyInputChanged(string newMoney)
    {
        if (!int.TryParse(newMoney, out var money)) return;

        myPreset.money = money;

        SendChanges();
    }

    public void OnAddNewItembtn()
    {
        itemSelectMenu.SetActive(true);

        foreach (Transform item in itemSelectMenuParent)
        {
            var cell = item.GetComponent<LootrunItemCell>();

            var doesExistInInventory = myPreset.items.FindIndex(x => x.itemName == cell.itemData.itemName);

            if (doesExistInInventory != -1)
                cell.gameObject.SetActive(false);
            else
                cell.gameObject.SetActive(true);
        }
    }

    void OnCellSelected(LootrunItemCell cell)
    {
        itemSelectMenu.SetActive(false);

        myPreset.items.Add(cell.itemData);

        AddItemCell(allItems[cell.itemData.itemName], cell.itemData.quantity);

        SendChanges();
    }

    public void OnSelectionExitClicked()
    {
        itemSelectMenu.SetActive(false);
    }
}
