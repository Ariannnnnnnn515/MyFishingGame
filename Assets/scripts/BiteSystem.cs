using UnityEngine;
using Fishing.Core.Data;
using Fishing.Core;
using System.Collections;

namespace Fishing.Systems
{
    public class BiteSystem : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private FishingController fishingController;

        [Header("Настройки поклевки")]
        [SerializeField] private BaitData currentBait; // ← ЗАПОЛНЯЕТСЯ АВТОМАТИЧЕСКИ!
        [SerializeField] private float waitingTimeMin = 3f;
        [SerializeField] private float waitingTimeMax = 8f;
        [SerializeField] private float biteChance = 0.7f; // 70%

        private FishingSpotData currentSpot;
        private bool isWaiting;
        private Coroutine waitingCoroutine;

        /// <summary>
        /// Установить текущую наживку (вызывается из FishingController)
        /// </summary>
        public void SetCurrentBait(BaitData bait)
        {
            currentBait = bait;
            Debug.Log($"BiteSystem: Установлена наживка: {bait?.baitName ?? "null"}");
        }

        /// <summary>
        /// Начать ожидание поклевки
        /// </summary>
        public void StartWaiting(FishingSpotData spot)
        {
            if (isWaiting)
                return;

            currentSpot = spot;
            isWaiting = true;

            if (waitingCoroutine != null)
                StopCoroutine(waitingCoroutine);

            waitingCoroutine = StartCoroutine(WaitingRoutine());
        }

        /// <summary>
        /// Остановить ожидание поклевки
        /// </summary>
        public void StopWaiting()
        {
            isWaiting = false;

            if (waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }
        }

        private IEnumerator WaitingRoutine()
        {
            while (isWaiting)
            {
                // Ждем случайное время
                float waitTime = Random.Range(waitingTimeMin, waitingTimeMax);
                yield return new WaitForSeconds(waitTime);

                // Проверяем, есть ли наживка
                if (currentBait == null)
                {
                    Debug.Log("BiteSystem: Нет наживки! Поклевка невозможна.");
                    continue;
                }

                // Проверяем шанс поклевки
                float roll = Random.value;
                if (roll <= biteChance)
                {
                    Debug.Log($"BiteSystem: ПОКЛЕВКА! Наживка: {currentBait.baitName}");

                    // Сообщаем FishingController о поклевке
                    if (fishingController != null)
                        fishingController.OnBiteOccurred();
                    else
                        Debug.LogError("BiteSystem: FishingController не назначен!");

                    // Если поклевка была, начинаем ожидание заново
                    // (это зависит от логики - можно сразу остановить ожидание)
                    if (isWaiting)
                    {
                        // Останавливаем ожидание после поклевки
                        StopWaiting();
                        yield break;
                    }
                }
                else
                {
                    Debug.Log($"BiteSystem: Поклевка не произошла (шанс: {biteChance * 100}%, выпало: {roll})");
                }
            }
        }

        public void Initialize(FishingController controller)
        {
            fishingController = controller;
            Debug.Log("BiteSystem инициализирован");
        }
    }
}