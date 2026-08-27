using Fishing.Core;
using Fishing.Core.Data;
using System.Collections.Generic;
using UnityEngine;

public class BaitSelectionPanelUI : MonoBehaviour
{
    public static BaitSelectionPanelUI Instance { get; private set; } // <-- ДОБАВИЛИ СИНГЛТОН

    public GameObject slotPrefab;
    public Transform slotContainer;
    public FishingController fishingController;

    private List<BaitSlotUI> currentSlots = new List<BaitSlotUI>();

    private void Awake() // <-- ДОБАВИЛИ ДЛЯ СИНГЛТОНА
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (PlayerBaitInventory.Instance != null)
        {
            UpdateUI();
            PlayerBaitInventory.Instance.OnInventoryChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (PlayerBaitInventory.Instance != null)
            PlayerBaitInventory.Instance.OnInventoryChanged -= UpdateUI;
    }

    public void UpdateUI()
    {
        if (slotContainer == null || slotPrefab == null)
        {
            Debug.LogError("BaitSelectionPanelUI: slotContainer или slotPrefab не назначены!");
            return;
        }

        // Очищаем старые слоты
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        currentSlots.Clear();

        if (PlayerBaitInventory.Instance == null)
        {
            Debug.LogError("PlayerBaitInventory.Instance == null!");
            return;
        }

        // Получаем все наживки игрока
        List<BaitData> ownedBaits = PlayerBaitInventory.Instance.GetOwnedBaits();

        foreach (BaitData bait in ownedBaits)
        {
            if (bait == null) continue;

            int count = PlayerBaitInventory.Instance.GetBaitCount(bait);
            GameObject newSlot = Instantiate(slotPrefab, slotContainer);

            BaitSlotUI slotUI = newSlot.GetComponent<BaitSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(bait, count, fishingController);
                currentSlots.Add(slotUI);
            }
        }
    }

    // Обновляем маркеры выбранной наживки
    public void RefreshSelection()
    {
        if (fishingController == null)
        {
            Debug.LogError("FishingController не назначен!");
            return;
        }

        BaitData currentBait = fishingController.GetCurrentBait(); // Используем метод, а не поле

        foreach (var slot in currentSlots)
        {
            if (slot != null)
            {
                slot.UpdateSelection(slot.GetMyBait() == currentBait); // Используем метод доступа
            }
        }
    }
}