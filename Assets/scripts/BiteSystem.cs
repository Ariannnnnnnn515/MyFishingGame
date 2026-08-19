using System.Collections;
using UnityEngine;
using Fishing.Core.Data;
using Fishing.Core;

namespace Fishing.Systems
{
    /// <summary>
    /// ��������� �������� �������. 
    /// ���������� ������ � ������ ���� ���� �� ����.
    /// </summary>
    public class BiteSystem : MonoBehaviour
    {
        [Header("��������� ���������")]
        [SerializeField] private float minWaitTime = 3f;
        [SerializeField] private float maxWaitTime = 20f;

        private FishingController controller;
        private Coroutine waitingCoroutine;
        private FishingSpotData currentSpot;

        [Header("Наживка")]
        [SerializeField] private BaitInventory baitInventory;

        private float currentBaitModifier = 1f;

        public void Initialize(FishingController controller) => this.controller = controller;

        /// <summary>
        /// ������ �������� ������� � ��������� ����.
        /// </summary>
        public void StartWaiting(FishingSpotData spot)
        {
            currentSpot = spot;
            currentBaitModifier = baitInventory != null
                ? baitInventory.UseBait()
                : 1f;

            if (waitingCoroutine != null)
                StopCoroutine(waitingCoroutine);

            waitingCoroutine = StartCoroutine(WaitForBite());
        }

        private IEnumerator WaitForBite()
        {
            while (currentSpot != null)
            {
                float modifier = Mathf.Max(
    0.1f,
    currentSpot.biteChanceModifier * currentBaitModifier
);
                float waitTime = Random.Range(minWaitTime, maxWaitTime) / modifier;
                yield return new WaitForSeconds(waitTime);

                FishData selectedFish = SelectFishFromPool();

                if (selectedFish == null)
                {
                    Debug.LogWarning("В точке ловли нет рыбы!");
                    continue;
                }

                if (Random.value < 0.1f)
                {
                    Debug.Log("Ложная поклёвка. Ждём ещё...");
                    continue;
                }

                waitingCoroutine = null;
                controller.OnBiteOccurred(selectedFish);
                yield break;
            }

            waitingCoroutine = null;
        }

        /// <summary>
        /// ����� ���� �� ������ ����� (spawnWeight).
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

            currentSpot = null; currentBaitModifier = 1f;
        }
    }
}