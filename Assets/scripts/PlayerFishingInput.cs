using TMPro;
using UnityEngine;
using Fishing.Core;
using Fishing.Core.Data;

public class PlayerFishingInput : MonoBehaviour
{
    [Header("Рыбалка")]
    [SerializeField] private FishCatchUI catchUI;
    [SerializeField] private FishingSpotData currentFishingSpot;

    [SerializeField] private Transform castTarget;
    [SerializeField] private float maxCastDistance = 100f;

    [Header("Интерфейс")]
    [SerializeField] private TMP_Text statusText;

    [Header("Экономика")]
    [SerializeField] private FishInventory fishInventory;

    private FishingController fishingController;
    private bool isFishingActive;
    private FishData currentFishData; // Сохраняем данные о пойманной рыбе

    private void Start()
    {
        fishingController = FishingController.Instance;

        if (fishingController == null)
        {
            Debug.LogError("FishingController не найден на сцене!");
            enabled = false;
            return;
        }

        fishingController.OnFishHooked += OnFishHooked;
        fishingController.OnFishLanded += OnFishLanded;
        fishingController.OnFishEscaped += OnFishEscaped;

        ShowStatus("Наведи курсор на воду и нажми ПКМ для заброса.");
    }

    private void Update()
    {
        // Заброс только по правой кнопке мыши
        if (Input.GetMouseButtonDown(1) && !isFishingActive)
            PerformCast();

        if (Input.GetKeyDown(KeyCode.R) && isFishingActive)
            fishingController.OnFishEscape();
    }

    private void PerformCast()
    {
        if (currentFishingSpot == null)
        {
            Debug.LogError("В PlayerFishingInput не назначен Current Fishing Spot!");
            return;
        }

        Vector3 targetPosition = castTarget != null
            ? castTarget.position
            : GetMouseTarget();

        if (targetPosition == Vector3.zero)
        {
            Debug.LogWarning("Не удалось определить цель для заброса!");
            return;
        }

        fishingController.PerformCast(targetPosition, currentFishingSpot);
        isFishingActive = true;
        ShowStatus("Заброс выполнен. Ждём поклёвку...");
    }

    private Vector3 GetMouseTarget()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Камера с тегом MainCamera не найдена!");
            return Vector3.zero;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int waterLayer = LayerMask.GetMask("Water");

        if (Physics.Raycast(ray, out RaycastHit hit, maxCastDistance, waterLayer))
            return hit.point;

        return Vector3.zero;
    }

    private void OnDestroy()
    {
        if (fishingController == null)
            return;

        fishingController.OnFishHooked -= OnFishHooked;
        fishingController.OnFishLanded -= OnFishLanded;
        fishingController.OnFishEscaped -= OnFishEscaped;
    }

    private void OnFishHooked(FishData fish)
    {
        isFishingActive = true;
        currentFishData = fish; // Сохраняем данные о рыбе
        ShowStatus($"Поклёвка! {fish.fishName} на крючке!");
    }

    private void OnFishLanded(FishData fish, float weight)
    {
        isFishingActive = false;

        if (fishInventory != null)
            fishInventory.AddFish(fish, weight);
        else
            Debug.LogError("В PlayerFishingInput не назначен FishInventory!");

        ShowStatus($"Поймана рыба: {fish.fishName}, {weight:F1} кг!");
        OnFishCaught(fish);
    }

    private void OnFishEscaped()
    {
        isFishingActive = false;
        currentFishData = null;
        ShowStatus("Рыбалка завершена. Можно сделать новый заброс.");
    }

    private void ShowStatus(string message)
    {
        Debug.Log(message);

        if (statusText != null)
            statusText.text = message;
    }

    /// <summary>
    /// Показывает UI с результатом поимки рыбы
    /// </summary>
    /// <param name="fishData">Данные о пойманной рыбе (FishData)</param>
    public void OnFishCaught(FishData fishData)
    {
        if (fishData == null)
        {
            Debug.LogError("OnFishCaught: Переданы пустые данные о рыбе!");
            return;
        }

        // Проверяем, что UI существует
        if (catchUI == null)
        {
            Debug.LogError("FishCatchUI не назначен в инспекторе!");
            return;
        }

        // КОНВЕРТИРУЕМ FishData В FishConfig (для совместимости с вашим UI)
        FishConfig config = new FishConfig
        {
            fishName = fishData.fishName,
            fishWeight = Random.Range(fishData.weightMin, fishData.weightMax),
            fishPrefab = fishData.fishPrefab // Предполагаю, что в FishData есть поле fishPrefab
        };

        // Показываем результат
        catchUI.ShowCatchResult(config);
    }

    // Перегрузка метода для прямого использования FishConfig (если понадобится)
    public void OnFishCaught(FishConfig caughtFish)
    {
        if (caughtFish == null)
        {
            Debug.LogError("OnFishCaught: Передан пустой конфиг рыбы!");
            return;
        }

        if (catchUI != null)
        {
            catchUI.ShowCatchResult(caughtFish);
        }
        else
        {
            Debug.LogError("FishCatchUI не назначен в инспекторе!");
        }
    }
}