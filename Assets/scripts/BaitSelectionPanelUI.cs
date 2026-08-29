using Fishing.Core;
using Fishing.Core.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaitSelectionPanelUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private FishingController fishingController;

    [Header("Настройки панели")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;

    private List<BaitSlotUI> currentSlots = new List<BaitSlotUI>();
    private bool isPanelOpen = false;

    public static BaitSelectionPanelUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void Start()
    {
        if (panelRoot == null)
        {
            Debug.LogError($"BaitSelectionPanelUI: panelRoot не назначен на объекте {gameObject.name}!");
            return;
        }

        panelRoot.SetActive(false);
        isPanelOpen = false;
        Debug.Log($"BaitSelectionPanelUI: инициализирован на {gameObject.name}, panelRoot = {panelRoot.name}");
    }

    private void OnEnable()
    {
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
        Debug.Log($"BaitSelectionPanelUI: OpenPanel() на {gameObject.name}");

        if (panelRoot == null)
        {
            Debug.LogError("BaitSelectionPanelUI: panelRoot == null!");
            return;
        }

        panelRoot.SetActive(true);
        isPanelOpen = true;
        Debug.Log($"BaitSelectionPanelUI: panelRoot.activeSelf = {panelRoot.activeSelf}");
        UpdateUI();
    }

    public void ClosePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
            isPanelOpen = false;
            Debug.Log("BaitSelectionPanelUI: Панель закрыта");
        }
    }

    public void TogglePanel()
    {
        if (isPanelOpen) ClosePanel();
        else OpenPanel();
    }

    public void UpdateUI()
    {
        if (!isPanelOpen)
        {
            Debug.Log("BaitSelectionPanelUI: Панель закрыта, обновление пропущено");
            return;
        }

        // Очищаем старые кнопки
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }
        currentSlots.Clear();

        if (PlayerBaitInventory.Instance == null)
        {
            Debug.LogError("PlayerBaitInventory.Instance == null!");
            return;
        }

        List<BaitData> ownedBaits = PlayerBaitInventory.Instance.GetOwnedBaits();

        if (ownedBaits.Count == 0)
        {
            CreateEmptyMessage();
            return;
        }

        // Создаём кнопки для каждой наживки
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

        RefreshSelection();
    }

    public void RefreshSelection()
    {
        if (fishingController == null) return;

        BaitData currentBait = fishingController.GetCurrentBait();
        foreach (var slot in currentSlots)
        {
            if (slot != null)
                slot.UpdateSelection(currentBait == slot.GetBaitData());
        }
    }

    private void CreateEmptyMessage()
    {
        GameObject emptyMsg = new GameObject("EmptyMessage");
        emptyMsg.transform.SetParent(slotParent);
        emptyMsg.transform.localScale = Vector3.one;

        var text = emptyMsg.AddComponent<TextMeshProUGUI>();
        text.text = "Нет доступных наживок\nКупите в магазине!";
        text.fontSize = 18;
        text.color = Color.gray;
        text.alignment = TextAlignmentOptions.Center;

        var rect = emptyMsg.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 60);
        rect.anchoredPosition = Vector2.zero;
    }

    public bool IsPanelOpen()
    {
        return isPanelOpen;
    }
}