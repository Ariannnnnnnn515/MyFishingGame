using Fishing.Core;
using Fishing.Core.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaitSelectionPanelUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject slotPrefab; // Префаб кнопки-слота
    [SerializeField] private Transform slotParent; // Родительский объект для кнопок (просто пустой GameObject)
    [SerializeField] private FishingController fishingController;

    [Header("Настройки панели")]
    [SerializeField] private GameObject panelRoot; // Корневой объект панели (отключается/включается)
    [SerializeField] private Button closeButton; // Кнопка "Закрыть"

    private List<BaitSlotUI> currentSlots = new List<BaitSlotUI>();

    private void Start()
    {
        // Привязываем кнопку закрытия
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // Панель изначально скрыта
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

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

    /// <summary>
    /// Открыть панель
    /// </summary>
    public void OpenPanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
        UpdateUI();
    }

    /// <summary>
    /// Закрыть панель
    /// </summary>
    public void ClosePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    /// <summary>
    /// Обновить список наживок
    /// </summary>
    public void UpdateUI()
    {
        // 1. Удаляем старые кнопки
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }
        currentSlots.Clear();

        // 2. Проверяем инвентарь
        if (PlayerBaitInventory.Instance == null)
        {
            Debug.LogError("PlayerBaitInventory.Instance == null!");
            return;
        }

        // 3. Получаем список наживок игрока
        List<BaitData> ownedBaits = PlayerBaitInventory.Instance.GetOwnedBaits();

        // 4. Если наживок нет - показываем сообщение
        if (ownedBaits.Count == 0)
        {
            CreateEmptyMessage();
            return;
        }

        // 5. Создаём кнопки для каждой наживки
        foreach (BaitData bait in ownedBaits)
        {
            if (bait == null) continue;

            int count = PlayerBaitInventory.Instance.GetBaitCount(bait);
            GameObject newSlot = Instantiate(slotPrefab, slotParent);

            BaitSlotUI slotUI = newSlot.GetComponent<BaitSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(bait, count, fishingController);
                currentSlots.Add(slotUI);
            }
        }

        // 6. Обновляем отметки "Активна"
        RefreshSelection();
    }

    /// <summary>
    /// Обновить состояние выбора для всех слотов
    /// </summary>
    public void RefreshSelection()
    {
        if (fishingController == null) return;

        BaitData currentBait = fishingController.GetCurrentBait();
        foreach (var slot in currentSlots)
        {
            slot.UpdateSelection(currentBait == slot.GetBaitData());
        }
    }

    private void CreateEmptyMessage()
    {
        GameObject emptyMsg = new GameObject("EmptyMessage");
        emptyMsg.transform.SetParent(slotParent);
        emptyMsg.transform.localScale = Vector3.one;

        var text = emptyMsg.AddComponent<TextMeshProUGUI>();
        text.text = "Нет доступных наживок";
        text.fontSize = 20;
        text.color = Color.gray;
        text.alignment = TextAlignmentOptions.Center;

        var rect = emptyMsg.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        rect.anchoredPosition = Vector2.zero;
    }
}