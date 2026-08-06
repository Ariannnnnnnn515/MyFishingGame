using TMPro;
using UnityEngine;
using Fishing.Core;
using Fishing.Core.Data;

public class PlayerFishingInput : MonoBehaviour
{
    [Header("Рыбалка")]
    [SerializeField] private FishingSpotData currentFishingSpot;
    [SerializeField] private FishCatchUI catchUI;
    [SerializeField] private Transform castTarget;
    [SerializeField] private float maxCastDistance = 100f;

    [Header("Интерфейс")]
    [SerializeField] private TMP_Text statusText;

    private FishingController fishingController;
    private bool isFishingActive;

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
        ShowStatus($"Поклёвка! {fish.fishName} на крючке!");
    }

    private void OnFishLanded(FishData fish)
    {
        float weight = Random.Range(fish.weightMin, fish.weightMax);
        isFishingActive = false;
        ShowStatus($"Поймана рыба: {fish.fishName}, {weight:F1} кг!");
    }

    private void OnFishEscaped()
    {
        isFishingActive = false;
        ShowStatus("Рыбалка завершена. Можно сделать новый заброс.");
    }

    private void ShowStatus(string message)
    {
        Debug.Log(message);

        if (statusText != null)
            statusText.text = message;
    }
    public void OnFishCaught(FishConfig caughtFish)
    {
        // ... ваша логика, например, остановка времени и т.д. ...
        
        // Проверяем, что UI существует, и показываем результат
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