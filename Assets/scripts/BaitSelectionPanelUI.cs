using Fishing.Core;
using Fishing.Core.Data;
using System.Collections.Generic;
using UnityEngine;

public class BaitSelectionPanelUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject slotPrefab;
    public Transform slotContainer;
    public FishingController fishingController;

    private List<BaitSlotUI> currentSlots = new List<BaitSlotUI>();

    private void OnEnable()
    {
        UpdateUI();
        if (PlayerBaitInventory.Instance != null)
            PlayerBaitInventory.Instance.OnInventoryChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (PlayerBaitInventory.Instance != null)
            PlayerBaitInventory.Instance.OnInventoryChanged -= UpdateUI;
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // ЭТОТ МЕТОД ДОЛЖЕН БЫТЬ!
    public void TogglePanel()
    {
        if (gameObject.activeSelf)
            ClosePanel();
        else
            OpenPanel();
    }

    public void UpdateUI()
    {
        // ... остальной код (как в предыдущем ответе) ...
    }

    public void RefreshSelection()
    {
        // ... остальной код ...
    }
}