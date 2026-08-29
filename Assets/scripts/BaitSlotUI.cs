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
    [SerializeField] private GameObject activeMarker; // Объект с текстом "Активна"

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

        // Заполняем UI
        if (iconImage != null)
            iconImage.sprite = bait.baitIcon;

        if (nameText != null)
            nameText.text = bait.baitName;

        UpdateCount(count);

        // Назначаем обработчик кнопки
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClick);
        }

        // Скрываем маркер "Активна"
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
        if (fishingController == null || myBait == null)
        {
            Debug.LogError("FishingController или BaitData == null!");
            return;
        }

        // Проверяем наличие наживки
        if (PlayerBaitInventory.Instance != null &&
            PlayerBaitInventory.Instance.GetBaitCount(myBait) <= 0)
        {
            Debug.Log($"Наживка {myBait.baitName} закончилась!");
            return;
        }

        // Выбираем наживку
        fishingController.SetCurrentBait(myBait);

        // Обновляем панель
        BaitSelectionPanelUI panel = GetComponentInParent<BaitSelectionPanelUI>();
        if (panel != null)
            panel.RefreshSelection();
    }
}