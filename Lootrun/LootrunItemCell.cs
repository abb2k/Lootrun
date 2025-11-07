using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lootrun.types;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using Lootrun;


public class LootrunItemCell : MonoBehaviour
{
    public LootrunItemQuantity itemData = new LootrunItemQuantity();

    public event UnityAction<LootrunItemCell> onCellDelete;
    public event UnityAction<LootrunItemCell> onCellChanged;
    public event UnityAction<LootrunItemCell> onSelectClicked;

    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image itemIcon;

    [SerializeField] private List<GameObject> selectMode;
    [SerializeField] private List<GameObject> countMode;

    public void Setup(Item item, int quantity, bool selectMode = false)
    {
        if (item == null) return;

        itemData.itemName = item.itemName;
        itemData.quantity = quantity;

        nameText.text = item.itemName;
        itemIcon.sprite = item.itemIcon;
        amountText.text = itemData.quantity.ToString();

        var toDisable = !selectMode ? this.selectMode : this.countMode;
        var toEnable = selectMode ? this.selectMode : this.countMode;

        toDisable.ForEach(x => x.SetActive(false));
        toEnable.ForEach(x => x.SetActive(true));
    }

    public void OnMinusClicked()
    {
        itemData.quantity -= 1;
        if (itemData.quantity < 0) itemData.quantity = 0;

        amountText.text = itemData.quantity.ToString();

        onCellChanged?.Invoke(this);
    }

    public void OnPlusClicked()
    {
        itemData.quantity += 1;
        if (itemData.quantity > 10) itemData.quantity = 10;

        amountText.text = itemData.quantity.ToString();

        onCellChanged?.Invoke(this);
    }

    public void OnCellDelete()
    {
        onCellDelete?.Invoke(this);
    }

    public void OnCellSelected()
    {
        onSelectClicked?.Invoke(this);
    }
}
