using Fishing.Core;
using Fishing.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaitSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private GameObject activeMarker;

    private BaitData myBait;
    private FishingController fishingController;

    public void Setup(BaitData bait, int count, FishingController controller)
    {
        if (bait == null)
        {
            Debug.LogError("BaitSlotUI: BaitData == null!");
            return;
        }

        myBait = bait;
        fishingController = controller;

        // Заполняем данные
        if (iconImage != null)
        {
            // Иконка может отсутствовать — просто оставляем пустую
            if (bait.baitIcon != null)
                iconImage.sprite = bait.baitIcon;
            // Если иконки нет — ничего не делаем, просто оставляем пустое место
        }

        if (nameText != null)
            nameText.text = bait.baitName;

        UpdateCount(count);

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClick);
        }

        UpdateSelection(false);
    }

    public void UpdateSelection(bool isSelected)
    {
        if (activeMarker != null)
            activeMarker.SetActive(isSelected);
    }

    public void UpdateCount(int newCount)
    {
        if (countText != null)
            countText.text = newCount.ToString();
    }

    public BaitData GetBaitData() => myBait;

    private void OnSlotClick()
    {
        if (fishingController == null || myBait == null) return;

        if (PlayerBaitInventory.Instance != null &&
            PlayerBaitInventory.Instance.GetBaitCount(myBait) <= 0)
        {
            Debug.Log($"Наживка {myBait.baitName} закончилась!");
            return;
        }

        fishingController.SetCurrentBait(myBait);

        BaitSelectionPanelUI panel = GetComponentInParent<BaitSelectionPanelUI>();
        if (panel != null)
            panel.RefreshSelection();
    }
}