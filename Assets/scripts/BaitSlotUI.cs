using Fishing.Core;
using Fishing.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaitSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;
    public GameObject selectedMarker;

    private BaitData myBait;
    private FishingController fishingController;

    public void Setup(BaitData bait, int count, FishingController controller)
    {
        if (bait == null)
        {
            Debug.LogError("BaitSlotUI: передан null BaitData!");
            return;
        }

        myBait = bait;
        fishingController = controller;

        // Заполняем UI
        if (iconImage != null)
            iconImage.sprite = bait.baitIcon;

        if (nameText != null)
            nameText.text = bait.baitName;

        UpdateCount(count);
        UpdateSelection(false);

        // Назначаем обработчик кнопки
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClick);
        }
    }

    public void UpdateSelection(bool isSelected)
    {
        if (selectedMarker != null)
            selectedMarker.SetActive(isSelected);
    }

    public void UpdateCount(int newCount)
    {
        if (countText != null)
            countText.text = newCount.ToString();
    }

    /// <summary>
    /// Получить данные наживки для этого слота
    /// </summary>
    public BaitData GetBaitData()
    {
        return myBait;
    }

    private void OnSlotClick()
    {
        if (fishingController == null)
        {
            Debug.LogError("FishingController не назначен!");
            return;
        }

        if (myBait == null)
        {
            Debug.LogError("BaitData не назначен!");
            return;
        }

        // Проверяем, есть ли эта наживка в инвентаре
        if (PlayerBaitInventory.Instance != null)
        {
            int count = PlayerBaitInventory.Instance.GetBaitCount(myBait);
            if (count <= 0)
            {
                Debug.Log($"Наживка {myBait.baitName} закончилась!");
                return;
            }
        }

        // Устанавливаем наживку
        fishingController.SetCurrentBait(myBait);

        // Обновляем UI выбора
        BaitSelectionPanelUI panel = GetComponentInParent<BaitSelectionPanelUI>();
        if (panel != null)
        {
            panel.RefreshSelection();
        }
    }

    public void RefreshSelection()
    {
        if (fishingController != null && myBait != null)
        {
            BaitData currentBait = fishingController.GetCurrentBait();
            UpdateSelection(currentBait == myBait);
        }
    }
}