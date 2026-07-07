using UnityEngine;
using Fishing.Core;
using Fishing.Core.Data;

public class PlayerFishingInput : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private FishingSpotData currentFishingSpot; // Назначь в инспекторе
    [SerializeField] private Transform castTarget; // Точка на воде (можно кликать мышью)
    [SerializeField] private float maxCastDistance = 10f;

    private FishingController fishingController;
    private bool isFishingActive = false;

    private void Start()
    {
        fishingController = FishingController.Instance;
        if (fishingController == null)
            Debug.LogError("FishingController не найден на сцене!");
    }

    private void Update()
    {
        // === УПРАВЛЕНИЕ ===

        // 1. Бросок удочки (ПКМ или пробел)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
        {
            if (!isFishingActive)
            {
                PerformCast();
            }
        }

        // 2. Экстренный сброс (клавиша R) - если хочешь прервать рыбалку
        if (Input.GetKeyDown(KeyCode.R))
        {
            fishingController.OnFishEscape();
            isFishingActive = false;
        }
    }

    /// <summary>
    /// Выполнить бросок в точку цели (по клику мыши или в заданную точку)
    /// </summary>
    private void PerformCast()
    {
        // Вариант А: Бросок в заранее заданную точку (например, центр озера)
        Vector3 targetPosition = castTarget != null ? castTarget.position : GetMouseTarget();

        // Проверяем, что точка находится в воде (можно добавить Raycast)
        if (targetPosition == Vector3.zero)
        {
            Debug.LogWarning("Не удалось определить цель для броска!");
            return;
        }

        // ВЫЗОВ №1: Главный метод броска
        fishingController.PerformCast(targetPosition, currentFishingSpot);
        isFishingActive = true;

        Debug.Log($"Бросок выполнен в точку: {targetPosition}");
    }

    /// <summary>
    /// Получить позицию под курсором мыши (Raycast)
    /// </summary>
    private Vector3 GetMouseTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Используем слой "Water" для определения поверхности воды
        int waterLayer = LayerMask.GetMask("Water");

        if (Physics.Raycast(ray, out hit, maxCastDistance, waterLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    // Подписка на события FishingController (для UI и обратной связи)
    private void OnEnable()
    {
        if (fishingController != null)
        {
            fishingController.OnFishHooked += OnFishHooked;
            fishingController.OnFishLanded += OnFishLanded;
            fishingController.OnFishEscaped += OnFishEscaped;
        }
    }

    private void OnDisable()
    {
        if (fishingController != null)
        {
            fishingController.OnFishHooked -= OnFishHooked;
            fishingController.OnFishLanded -= OnFishLanded;
            fishingController.OnFishEscaped -= OnFishEscaped;
        }
    }

    // === ОБРАТНАЯ СВЯЗЬ ДЛЯ ИГРОКА ===

    private void OnFishHooked(FishData fish)
    {
        Debug.Log($"🐟 Поклёвка! {fish.fishName} на крючке! Начинай вываживание!");
        isFishingActive = true;
        // Можно включить UI-индикатор мини-игры
    }

    private void OnFishLanded(FishData fish)
    {
        Debug.Log($"🎣 ПОЙМАНА! {fish.fishName} весом {Random.Range(fish.weightMin, fish.weightMax):F1} кг!");
        isFishingActive = false;
        // Показать награду, добавить в инвентарь
    }

    private void OnFishEscaped()
    {
        Debug.Log("❌ Рыба сорвалась! Попробуй снова.");
        isFishingActive = false;
    }
}