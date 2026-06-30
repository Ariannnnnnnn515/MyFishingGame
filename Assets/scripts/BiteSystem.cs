using System.Collections;
using UnityEngine;
using Fishing.Core.Data;
using Fishing.Core;

namespace Fishing.Systems
{
    /// <summary>
    /// Симуляция ожидания поклёвки. 
    /// Использует рандом с учётом веса рыбы из пула.
    /// </summary>
    public class BiteSystem : MonoBehaviour
    {
        [Header("Временные параметры")]
        [SerializeField] private float minWaitTime = 3f;
        [SerializeField] private float maxWaitTime = 20f;

        private FishingController controller;
        private Coroutine waitingCoroutine;
        private FishingSpotData currentSpot;

        public void Initialize(FishingController controller) => this.controller = controller;

        /// <summary>
        /// Начать ожидание поклёвки в указанной зоне.
        /// </summary>
        public void StartWaiting(FishingSpotData spot)
        {
            currentSpot = spot;
            if (waitingCoroutine != null) StopCoroutine(waitingCoroutine);
            waitingCoroutine = StartCoroutine(WaitForBite());
        }

        private IEnumerator WaitForBite()
        {
            // Случайное время ожидания
            float waitTime = Random.Range(minWaitTime, maxWaitTime) / currentSpot.biteChanceModifier;
            yield return new WaitForSeconds(waitTime);

            // Выбираем рыбу из пула по весам
            FishData selectedFish = SelectFishFromPool();
            if (selectedFish != null)
            {
                // Добавляем шанс "пустой поклёвки" (10%)
                if (Random.value < 0.1f)
                {
                    Debug.Log("Пустая поклёвка...");
                    StartWaiting(currentSpot); // Перезапускаем ожидание
                    yield break;
                }

                controller.OnBiteOccurred(selectedFish);
            }
            else
            {
                Debug.LogWarning("В пуле нет рыбы! Проверьте FishingSpotData.");
                StartWaiting(currentSpot);
            }
        }

        /// <summary>
        /// Выбор рыбы на основе весов (spawnWeight).
        /// </summary>
        private FishData SelectFishFromPool()
        {
            if (currentSpot.fishPool.Length == 0) return null;

            int totalWeight = 0;
            foreach (var entry in currentSpot.fishPool)
                totalWeight += entry.spawnWeight;

            int randomPoint = Random.Range(0, totalWeight);
            foreach (var entry in currentSpot.fishPool)
            {
                if (randomPoint < entry.spawnWeight)
                    return entry.fishData;
                randomPoint -= entry.spawnWeight;
            }
            return currentSpot.fishPool[0].fishData;
        }

        public void StopWaiting()
        {
            if (waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }
        }
    }
}