using Fishing.Core;
using Fishing.Core.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fishing.Systems
{
    /// <summary>
    /// Система ожидания и обработки поклевки.
    /// Управляет временем ожидания, проверкой условий и генерацией поклевки.
    /// </summary>
    public class BiteSystem : MonoBehaviour
    {
        [Header("Настройки поклевки")]
        [SerializeField] private float minWaitTime = 3f;
        [SerializeField] private float maxWaitTime = 15f;
        [SerializeField] private float biteCheckInterval = 0.5f;
        [SerializeField] private float missChance = 0.1f; // 10% шанс пустой поклевки

        [Header("Влияние наживки")]
        [Tooltip("Множитель скорости поклевки от типа наживки")]
        [SerializeField] private float baitSpeedMultiplier = 1.0f;

        [Header("Ссылки")]
        [SerializeField] private FishingSpotData currentSpot;
        [SerializeField] private BaitData currentBait; // Текущая наживка на крючке

        // События
        public event Action OnBiteReady; // Поклевка готова
        public event Action OnBiteMiss;  // Пустая поклевка
        public event Action<BaitData> OnBaitChanged; // Смена наживки

        // Состояние
        private FishingController fishingController;
        private Coroutine waitingCoroutine;
        private bool isWaiting;
        private float currentWaitTime;
        private float timer;

        // Словарь для хранения модификаторов наживок (можно расширить)
        private Dictionary<BaitData, float> baitModifiers = new Dictionary<BaitData, float>();

        private void Awake()
        {
            // Инициализация дефолтных модификаторов
            // Позже можно загружать из JSON или ScriptableObject
            // Сейчас просто оставляем пустым
        }

        /// <summary>
        /// Инициализация системы (вызывается из FishingController)
        /// </summary>
        public void Initialize(FishingController controller)
        {
            fishingController = controller;
        }

        /// <summary>
        /// Установить текущую наживку
        /// </summary>
        public void SetCurrentBait(BaitData bait)
        {
            if (bait == null)
            {
                Debug.LogWarning("BiteSystem: Попытка установить null наживку");
                return;
            }

            currentBait = bait;
            OnBaitChanged?.Invoke(bait);
            Debug.Log($"BiteSystem: Установлена наживка {bait.baitName}");

            // Если ожидание активно, обновляем таймер с учетом новой наживки
            if (isWaiting)
            {
                // Пересчитываем время ожидания
                float remainingTime = currentWaitTime - timer;
                float newRemainingTime = remainingTime / baitSpeedMultiplier;
                // Можно перезапустить ожидание с новым временем
                StopWaiting();
                StartWaiting(currentSpot);
            }
        }

        /// <summary>
        /// Обновить наживку (вызывается из FishingController)
        /// </summary>
        public void UpdateBait(BaitData newBait)
        {
            if (newBait != null && currentBait != newBait)
            {
                SetCurrentBait(newBait);
            }
        }

        /// <summary>
        /// Начать ожидание поклевки
        /// </summary>
        public void StartWaiting(FishingSpotData spot)
        {
            if (spot == null)
            {
                Debug.LogError("BiteSystem: Не передан FishingSpotData!");
                return;
            }

            if (currentBait == null)
            {
                Debug.LogWarning("BiteSystem: Нет наживки! Ожидание отменено.");
                return;
            }

            // Останавливаем предыдущее ожидание
            StopWaiting();

            currentSpot = spot;
            isWaiting = true;

            // Рассчитываем время ожидания с учетом наживки и места
            currentWaitTime = CalculateWaitTime(spot);

            // Запускаем корутину ожидания
            waitingCoroutine = StartCoroutine(WaitingRoutine());

            Debug.Log($"BiteSystem: Начато ожидание ({currentWaitTime:F1}с) с наживкой {currentBait.baitName}");
        }

        /// <summary>
        /// Остановить ожидание
        /// </summary>
        public void StopWaiting()
        {
            if (waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }

            isWaiting = false;
            timer = 0f;
            Debug.Log("BiteSystem: Ожидание остановлено");
        }

        /// <summary>
        /// Корутина ожидания поклевки
        /// </summary>
        private IEnumerator WaitingRoutine()
        {
            timer = 0f;

            while (timer < currentWaitTime)
            {
                timer += Time.deltaTime;

                // Опционально: проверяем, есть ли еще наживка
                if (currentBait != null && PlayerBaitInventory.Instance != null)
                {
                    int baitCount = PlayerBaitInventory.Instance.GetBaitCount(currentBait);
                    if (baitCount <= 0)
                    {
                        Debug.LogWarning("BiteSystem: Наживка закончилась во время ожидания!");
                        StopWaiting();
                        yield break;
                    }
                }

                yield return null;
            }

            // Время ожидания истекло - проверяем поклевку
            CheckBite();
        }

        /// <summary>
        /// Проверка поклевки
        /// </summary>
        private void CheckBite()
        {
            if (!isWaiting || currentSpot == null)
                return;

            // Проверяем шанс пустой поклевки
            float randomValue = UnityEngine.Random.value;

            if (randomValue < missChance)
            {
                // Пустая поклевка
                OnBiteMiss?.Invoke();
                Debug.Log("BiteSystem: Пустая поклевка (рыба ушла)");

                // Перезапускаем ожидание через некоторое время
                StartCoroutine(RestartWaitingAfterMiss());
                return;
            }

            // Успешная поклевка
            Debug.Log($"BiteSystem: ПОКЛЕВКА! Наживка: {currentBait?.baitName ?? "отсутствует"}");

            // Останавливаем ожидание перед вызовом события
            isWaiting = false;

            // Вызываем событие для FishingController
            OnBiteReady?.Invoke();
        }

        /// <summary>
        /// Перезапуск ожидания после пустой поклевки
        /// </summary>
        private IEnumerator RestartWaitingAfterMiss()
        {
            yield return new WaitForSeconds(1f);

            if (isWaiting && currentSpot != null)
            {
                // Проверяем наличие наживки
                if (currentBait == null)
                {
                    Debug.LogWarning("BiteSystem: Нет наживки для перезапуска ожидания");
                    yield break;
                }

                // Перезапускаем ожидание
                StartWaiting(currentSpot);
            }
        }

        /// <summary>
        /// Рассчитать время ожидания
        /// </summary>
        private float CalculateWaitTime(FishingSpotData spot)
        {
            // Базовое время от настроек
            float baseTime = UnityEngine.Random.Range(minWaitTime, maxWaitTime);

            // Модификатор от места рыбалки (если есть)
            float spotModifier = 1f;
            if (spot != null && spot.biteSpeedMultiplier > 0)
            {
                spotModifier = spot.biteSpeedMultiplier;
            }

            // Модификатор от наживки
            float baitModifier = GetBaitModifier(currentBait);

            // Итоговое время
            float finalTime = baseTime * spotModifier / (baitModifier * baitSpeedMultiplier);

            // Ограничиваем время разумными пределами
            return Mathf.Clamp(finalTime, 0.5f, 30f);
        }

        /// <summary>
        /// Получить модификатор наживки
        /// </summary>
        private float GetBaitModifier(BaitData bait)
        {
            if (bait == null)
                return 1f;

            // Если есть модификатор в словаре - используем его
            if (baitModifiers.ContainsKey(bait))
                return baitModifiers[bait];

            // Можно добавить логику на основе названия или тегов
            // Например, по умолчанию все наживки ускоряют клев на 20%
            return 1.2f;
        }

        /// <summary>
        /// Получить текущий прогресс ожидания (0-1)
        /// </summary>
        public float GetWaitProgress()
        {
            if (!isWaiting || currentWaitTime <= 0)
                return 0f;

            return Mathf.Clamp01(timer / currentWaitTime);
        }

        /// <summary>
        /// Проверка, идет ли ожидание
        /// </summary>
        public bool IsWaiting()
        {
            return isWaiting;
        }

        /// <summary>
        /// Получить текущую наживку
        /// </summary>
        public BaitData GetCurrentBait()
        {
            return currentBait;
        }

        /// <summary>
        /// Добавить модификатор для наживки (для расширения)
        /// </summary>
        public void AddBaitModifier(BaitData bait, float modifier)
        {
            if (bait == null)
                return;

            if (baitModifiers.ContainsKey(bait))
                baitModifiers[bait] = modifier;
            else
                baitModifiers.Add(bait, modifier);
        }

        /// <summary>
        /// Сбросить систему (вызывается при завершении рыбалки)
        /// </summary>
        public void ResetSystem()
        {
            StopWaiting();
            currentSpot = null;
            // currentBait оставляем, так как он может использоваться дальше
        }

        private void OnDestroy()
        {
            StopWaiting();
        }
    }
}