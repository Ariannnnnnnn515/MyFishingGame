using System;
using UnityEngine;
using Fishing.Core.Interfaces;
using Fishing.Core.Data;
using Fishing.Systems;
using System.Collections.Generic;

namespace Fishing.Core
{
    public class FishingController : MonoBehaviour
    {
        public static FishingController Instance { get; private set; }

        [SerializeField] private CastingSystem castingSystem;
        [SerializeField] private BiteSystem biteSystem;
        [SerializeField] private ReelingSystem reelingSystem;

        [Header("Система наживок")]
        [SerializeField] private BaitData currentBait;
        [SerializeField] private List<FishData> allFishInGame;

        [Header("Настройки рыбалки")]
        [SerializeField] private FishingSpotData currentSpot;
        [SerializeField] private FishData currentFishData;
        public IFishable CurrentFish { get; private set; }

        public event Action<FishData> OnFishHooked;
        public event Action<FishData, float> OnFishLanded;
        public event Action OnFishEscaped;
        public event Action<BaitData> OnBaitChanged;

        private bool isFishingInProgress;
        private bool hasEnoughBait;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            // Инициализация подсистем
            if (castingSystem != null)
                castingSystem.Initialize(this);
            else
                Debug.LogError("CastingSystem не назначен в FishingController!");

            if (biteSystem != null)
                biteSystem.Initialize(this);
            else
                Debug.LogError("BiteSystem не назначен в FishingController!");

            if (reelingSystem != null)
                reelingSystem.Initialize(this);
            else
                Debug.LogError("ReelingSystem не назначен в FishingController!");

            // Проверяем наличие наживки с задержкой
            Invoke(nameof(CheckAndSetDefaultBaitDelayed), 0.3f);
        }

        private void CheckAndSetDefaultBaitDelayed()
        {
            Debug.Log("Проверка стартовой наживки...");
            CheckAndSetDefaultBait();

            if (currentBait == null)
            {
                Debug.LogWarning("Повторная попытка установки наживки через 0.5с...");
                Invoke(nameof(CheckAndSetDefaultBait), 0.5f);
            }
            else
            {
                Debug.Log($"FishingController инициализирован. Наживка: {currentBait.baitName}");
            }
        }

        private void CheckAndSetDefaultBait()
        {
            if (PlayerBaitInventory.Instance == null)
            {
                Debug.LogWarning("PlayerBaitInventory.Instance == null!");
                return;
            }

            var ownedBaits = PlayerBaitInventory.Instance.GetOwnedBaits();
            if (ownedBaits.Count > 0)
            {
                SetCurrentBait(ownedBaits[0]);
                Debug.Log($"Автоматически выбрана наживка: {ownedBaits[0].baitName}");
            }
            else
            {
                Debug.LogWarning("У игрока нет наживки! Нужно купить или получить.");
                currentBait = null;
                hasEnoughBait = false;
            }
        }

        public void SetCurrentBait(BaitData newBait)
        {
            if (newBait == null)
            {
                currentBait = null;
                hasEnoughBait = false;
                OnBaitChanged?.Invoke(null);
                return;
            }

            if (PlayerBaitInventory.Instance == null)
            {
                Debug.LogError("PlayerBaitInventory.Instance == null!");
                return;
            }

            int count = PlayerBaitInventory.Instance.GetBaitCount(newBait);
            if (count > 0)
            {
                currentBait = newBait;
                hasEnoughBait = true;
                OnBaitChanged?.Invoke(newBait);
                Debug.Log($"Выбрана наживка: {newBait.baitName} (осталось: {count})");

                if (!isFishingInProgress && biteSystem != null)
                {
                    biteSystem.SetCurrentBait(newBait);
                }
            }
            else
            {
                Debug.Log($"Нет наживки {newBait.baitName} в инвентаре!");
                TrySwitchToAvailableBait();
            }
        }

        public BaitData GetCurrentBait()
        {
            return currentBait;
        }

        public bool HasEnoughBait()
        {
            if (currentBait == null)
                return false;

            return PlayerBaitInventory.Instance != null &&
                   PlayerBaitInventory.Instance.GetBaitCount(currentBait) > 0;
        }

        private void TrySwitchToAvailableBait()
        {
            if (PlayerBaitInventory.Instance == null)
                return;

            var ownedBaits = PlayerBaitInventory.Instance.GetOwnedBaits();
            if (ownedBaits.Count > 0)
            {
                SetCurrentBait(ownedBaits[0]);
            }
            else
            {
                currentBait = null;
                hasEnoughBait = false;
                OnBaitChanged?.Invoke(null);
                Debug.LogWarning("Все наживки закончились!");
            }
        }

        public void PerformCast(Vector3 targetPosition, FishingSpotData spot)
        {
            if (isFishingInProgress)
            {
                Debug.LogWarning("Уже идет рыбалка!");
                return;
            }

            if (spot == null)
            {
                Debug.LogError("Не передан FishingSpotData!");
                return;
            }

            if (currentBait == null)
            {
                Debug.LogWarning("Нет выбранной наживки! Нельзя закинуть удочку.");
                TrySwitchToAvailableBait();
                if (currentBait == null)
                {
                    Debug.LogError("Нет доступных наживок!");
                    return;
                }
            }

            if (PlayerBaitInventory.Instance == null)
            {
                Debug.LogError("PlayerBaitInventory.Instance == null!");
                return;
            }

            int baitCount = PlayerBaitInventory.Instance.GetBaitCount(currentBait);
            if (baitCount <= 0)
            {
                Debug.LogWarning($"Наживка {currentBait.baitName} закончилась!");
                hasEnoughBait = false;
                TrySwitchToAvailableBait();
                if (currentBait == null)
                {
                    Debug.LogError("Нет доступных наживок!");
                    return;
                }
            }

            isFishingInProgress = true;
            currentSpot = spot;
            hasEnoughBait = true;

            if (biteSystem != null)
                biteSystem.SetCurrentBait(currentBait);

            if (castingSystem != null)
            {
                Debug.Log($"Заброс с наживкой: {currentBait.baitName}");
                castingSystem.StartCast(targetPosition, OnCastComplete);
            }
            else
            {
                Debug.LogError("CastingSystem не назначен!");
                isFishingInProgress = false;
            }
        }

        private void OnCastComplete()
        {
            Debug.Log($"Заброс выполнен. Наживка: {currentBait?.baitName ?? "отсутствует"}");
            if (biteSystem != null)
                biteSystem.StartWaiting(currentSpot);
            else
                Debug.LogError("BiteSystem не назначен!");
        }

        public void OnBiteOccurred()
        {
            Debug.Log($"OnBiteOccurred вызван! isFishingInProgress={isFishingInProgress}, currentBait={currentBait?.baitName ?? "null"}");

            if (!isFishingInProgress)
            {
                Debug.Log("Поклевка отменена: нет активной рыбалки");
                return;
            }

            if (currentBait == null)
            {
                Debug.Log("Поклевка отменена: нет наживки");
                return;
            }

            if (PlayerBaitInventory.Instance == null)
            {
                Debug.LogError("PlayerBaitInventory.Instance == null!");
                return;
            }

            // Тратим наживку
            if (!PlayerBaitInventory.Instance.SpendBait(currentBait, 1))
            {
                Debug.LogWarning($"Наживка {currentBait.baitName} не найдена в инвентаре!");
                hasEnoughBait = false;
                TrySwitchToAvailableBait();
                return;
            }

            // Выбираем рыбу на основе наживки
            FishData fishData = GetRandomFishByBait(currentBait);

            if (fishData == null)
            {
                Debug.Log($"На наживку {currentBait.baitName} ничего не клюнуло!");
                Invoke(nameof(ResumeWaitingAfterMiss), 1.5f);
                return;
            }

            // УСПЕШНАЯ ПОКЛЕВКА!
            Debug.Log($"УСПЕШНАЯ ПОКЛЕВКА! Рыба: {fishData.fishName}");

            currentFishData = fishData;
            CurrentFish = new FishInstance(fishData);
            CurrentFish.OnHooked();

            OnFishHooked?.Invoke(fishData);

            // ЗАПУСК МИНИ-ИГРЫ
            if (reelingSystem != null)
            {
                Debug.Log($"ЗАПУСК ReelingSystem.StartFight() для рыбы: {fishData.fishName}");
                reelingSystem.StartFight(CurrentFish);
            }
            else
            {
                Debug.LogError("ReelingSystem НЕ НАЗНАЧЕН! Ищем автоматически...");
                // ИСПРАВЛЕНО: Используем FindFirstObjectByType вместо FindObjectOfType
                reelingSystem = FindFirstObjectByType<ReelingSystem>();
                if (reelingSystem != null)
                {
                    Debug.Log("ReelingSystem найден автоматически!");
                    reelingSystem.Initialize(this);
                    reelingSystem.StartFight(CurrentFish);
                }
                else
                {
                    Debug.LogError("ReelingSystem не найден! Рыба поймана автоматически.");
                    OnFishTired();
                }
            }
        }

        private void ResumeWaitingAfterMiss()
        {
            if (isFishingInProgress && currentSpot != null && biteSystem != null)
            {
                biteSystem.StartWaiting(currentSpot);
            }
        }

        private FishData GetRandomFishByBait(BaitData bait)
        {
            if (bait == null || allFishInGame == null || allFishInGame.Count == 0)
            {
                Debug.LogWarning("Нет данных о рыбах!");
                return null;
            }

            List<FishData> possibleFish = new List<FishData>();
            List<int> chances = new List<int>();

            foreach (FishData fish in allFishInGame)
            {
                if (fish == null) continue;

                int chance = fish.GetChanceForBait(bait);
                if (chance > 0)
                {
                    possibleFish.Add(fish);
                    chances.Add(chance);
                }
            }

            if (possibleFish.Count == 0)
            {
                Debug.Log($"На наживку {bait.baitName} не клюет ни одна рыба!");
                return null;
            }

            int totalChance = 0;
            foreach (int c in chances)
                totalChance += c;

            int randomValue = UnityEngine.Random.Range(0, totalChance);
            int cumulative = 0;

            for (int i = 0; i < chances.Count; i++)
            {
                cumulative += chances[i];
                if (randomValue < cumulative)
                {
                    Debug.Log($"Выбрана рыба: {possibleFish[i].fishName} (шанс: {chances[i]}%)");
                    return possibleFish[i];
                }
            }

            return possibleFish[0];
        }

        public void OnFishTired()
        {
            if (CurrentFish == null || currentFishData == null)
                return;

            CurrentFish.State = FishState.Landed;
            FishData landedFish = currentFishData;
            float landedWeight = CurrentFish.Weight;

            Debug.Log($"Поймана рыба {landedFish.fishName}, {landedWeight:F1} кг!");
            ResetFishingSystems();
            OnFishLanded?.Invoke(landedFish, landedWeight);
        }

        public void OnFishEscape()
        {
            if (!isFishingInProgress)
                return;

            CurrentFish?.OnEscape();
            ResetFishingSystems();
            OnFishEscaped?.Invoke();
            Debug.Log("Рыба сорвалась или ушла.");
        }

        private void ResetFishingSystems()
        {
            if (biteSystem != null)
                biteSystem.StopWaiting();

            if (reelingSystem != null)
                reelingSystem.StopFight();

            if (castingSystem != null)
                castingSystem.ResetCast();

            CurrentFish = null;
            currentFishData = null;
            currentSpot = null;
            isFishingInProgress = false;
            Debug.Log("Системы рыбалки сброшены.");
        }

        private class FishInstance : IFishable
        {
            private FishData data;
            private float maxResistance;
            private float currentTiredness = 0f;
            public float Weight { get; }

            public string SpeciesId => data.name;
            public FishState State { get; set; }
            public float CurrentResistance => Mathf.Clamp(maxResistance, 0.25f, 0.8f);

            public FishInstance(FishData data)
            {
                this.data = data;
                Weight = UnityEngine.Random.Range(data.weightMin, data.weightMax);
                maxResistance = data.baseResistance * Weight;
                State = FishState.Hooked;
            }

            public void OnHooked() => State = FishState.Fighting;
            public void OnEscape() => State = FishState.Idle;

            public bool ApplyTension(float tensionPower)
            {
                if (tensionPower > 0f)
                    currentTiredness += Time.deltaTime * data.escapeSpeed;
                else
                    currentTiredness -= Time.deltaTime * 0.15f;

                currentTiredness = Mathf.Clamp01(currentTiredness);

                if (currentTiredness >= 1f)
                {
                    State = FishState.Tired;
                    return true;
                }

                return false;
            }
        }
    }
}