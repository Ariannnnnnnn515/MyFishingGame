using System;
using UnityEngine;
using Fishing.Core.Interfaces;
using Fishing.Core.Data;
using Fishing.Systems;
using System.Collections.Generic;

namespace Fishing.Core
{
    /// <summary>
    /// Главный контроллер рыбалки. Синглтон.
    /// Управляет системами заброса, поклевки и вываживания.
    /// </summary>
    public class FishingController : MonoBehaviour
    {
        public static FishingController Instance { get; private set; }

        // Ссылки на подсистемы (назначаются в Inspector или через Find)
        [SerializeField] private CastingSystem castingSystem;
        [SerializeField] private BiteSystem biteSystem;
        [SerializeField] private ReelingSystem reelingSystem;

        [Header("Система наживок")]
        [SerializeField] private BaitData currentBait; // Текущая выбранная наживка
        [SerializeField] private List<FishData> allFishInGame; // ВСЕ рыбы в игре (для выбора)

        [Header("Настройки рыбалки")]
        [SerializeField] private FishingSpotData currentSpot;
        [SerializeField] private FishData currentFishData; // Выбранная рыба (для отладки)
        public IFishable CurrentFish { get; private set; } // Экземпляр рыбы в бою

        // События для UI и других систем
        public event Action<FishData> OnFishHooked;   // Рыба клюнула
        public event Action<FishData, float> OnFishLanded; // Поймали
        public event Action OnFishEscaped; // Сорвалась
        public event Action<BaitData> OnBaitChanged; // Сменилась наживка

        private bool isFishingInProgress;
        private bool hasEnoughBait; // Флаг, что наживка есть

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
            castingSystem.Initialize(this);
            biteSystem.Initialize(this);
            reelingSystem.Initialize(this);

            // Проверяем, есть ли стартовая наживка у игрока
            CheckAndSetDefaultBait();
        }

        /// <summary>
        /// Проверяет и устанавливает наживку по умолчанию (если есть в инвентаре)
        /// </summary>
        private void CheckAndSetDefaultBait()
        {
            var ownedBaits = PlayerBaitInventory.Instance.GetOwnedBaits();
            if (ownedBaits.Count > 0)
            {
                // Берем первую доступную наживку
                SetCurrentBait(ownedBaits[0]);
            }
            else
            {
                Debug.LogWarning("У игрока нет наживки! Нужно купить или получить.");
                currentBait = null;
                hasEnoughBait = false;
            }
        }

        /// <summary>
        /// Установить текущую наживку
        /// </summary>
        public void SetCurrentBait(BaitData newBait)
        {
            if (newBait == null)
            {
                currentBait = null;
                hasEnoughBait = false;
                OnBaitChanged?.Invoke(null);
                return;
            }

            // Проверяем, есть ли такая наживка в инвентаре
            int count = PlayerBaitInventory.Instance.GetBaitCount(newBait);
            if (count > 0)
            {
                currentBait = newBait;
                hasEnoughBait = true;
                OnBaitChanged?.Invoke(newBait);
                Debug.Log($"Выбрана наживка: {newBait.baitName}");

                // Если рыбалка не активна, обновляем систему ожидания поклевки
                if (!isFishingInProgress && biteSystem != null)
                {
                    biteSystem.UpdateBait(newBait);
                }
            }
            else
            {
                Debug.Log($"Нет наживки {newBait.baitName} в инвентаре!");
                // Если текущая наживка закончилась, пытаемся найти другую
                TrySwitchToAvailableBait();
            }
        }

        /// <summary>
        /// Попытаться переключиться на другую доступную наживку
        /// </summary>
        private void TrySwitchToAvailableBait()
        {
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

        /// <summary>
        /// Получить текущую наживку
        /// </summary>
        public BaitData GetCurrentBait()
        {
            return currentBait;
        }

        /// <summary>
        /// Выполнить заброс в указанную точку.
        /// </summary>
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

            // Проверка наличия наживки
            if (currentBait == null)
            {
                Debug.LogWarning("Нет выбранной наживки! Нельзя закинуть удочку.");
                // Можно показать UI сообщение
                return;
            }

            // Проверка количества наживки
            int baitCount = PlayerBaitInventory.Instance.GetBaitCount(currentBait);
            if (baitCount <= 0)
            {
                Debug.LogWarning($"Наживка {currentBait.baitName} закончилась!");
                hasEnoughBait = false;
                TrySwitchToAvailableBait();
                return;
            }

            isFishingInProgress = true;
            currentSpot = spot;
            hasEnoughBait = true;

            // Передаем наживку в систему поклевки
            biteSystem.SetCurrentBait(currentBait);

            castingSystem.StartCast(targetPosition, OnCastComplete);
        }

        /// <summary>
        /// Колбэк после завершения заброса (поплавок в воде).
        /// </summary>
        private void OnCastComplete()
        {
            Debug.Log($"Заброс выполнен. Наживка: {currentBait?.baitName ?? "отсутствует"}");
            biteSystem.StartWaiting(currentSpot);
        }

        /// <summary>
        /// Обработка поклевки от BiteSystem
        /// </summary>
        public void OnBiteOccurred()
        {
            if (!isFishingInProgress || currentBait == null)
            {
                Debug.Log("Поклевка отменена: нет активной рыбалки или наживки");
                return;
            }

            // Проверяем, есть ли наживка в инвентаре
            if (!PlayerBaitInventory.Instance.SpendBait(currentBait, 1))
            {
                Debug.LogWarning($"Наживка {currentBait.baitName} не найдена в инвентаре!");
                hasEnoughBait = false;
                TrySwitchToAvailableBait();
                return;
            }

            // Выбираем рыбу на основе текущей наживки
            FishData fishData = GetRandomFishByBait(currentBait);

            if (fishData == null)
            {
                Debug.Log($"На наживку {currentBait.baitName} ничего не клюнуло!");
                // Небольшая задержка перед повторным ожиданием
                Invoke(nameof(ResumeWaitingAfterMiss), 1.5f);
                return;
            }

            // Успешная поклевка!
            currentFishData = fishData;
            CurrentFish = new FishInstance(fishData);

            CurrentFish.OnHooked();
            OnFishHooked?.Invoke(fishData);
            reelingSystem.StartFight(CurrentFish);
        }

        /// <summary>
        /// Возобновить ожидание после пустой поклевки
        /// </summary>
        private void ResumeWaitingAfterMiss()
        {
            if (isFishingInProgress && currentSpot != null)
            {
                biteSystem.StartWaiting(currentSpot);
            }
        }

        /// <summary>
        /// Выбрать случайную рыбу на основе наживки
        /// </summary>
        private FishData GetRandomFishByBait(BaitData bait)
        {
            if (bait == null || allFishInGame == null || allFishInGame.Count == 0)
            {
                Debug.LogWarning("Нет данных о рыбах!");
                return null;
            }

            // Собираем всех рыб с шансом на эту наживку
            List<FishData> possibleFish = new List<FishData>();
            List<int> chances = new List<int>();

            foreach (FishData fish in allFishInGame)
            {
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

            // Рандомный выбор с учетом вероятности
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
                    Debug.Log($"Шансы: {possibleFish[i].fishName} = {chances[i]}%");
                    return possibleFish[i];
                }
            }

            return possibleFish[0]; // На всякий случай
        }

        /// <summary>
        /// Обработка успешной поимки от ReelingSystem
        /// </summary>
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

        /// <summary>
        /// Обработка схода рыбы
        /// </summary>
        public void OnFishEscape()
        {
            if (!isFishingInProgress)
                return;

            CurrentFish?.OnEscape();
            ResetFishingSystems();
            OnFishEscaped?.Invoke();
            Debug.Log("Рыба сорвалась или ушла.");
        }

        /// <summary>
        /// Сброс всех систем
        /// </summary>
        private void ResetFishingSystems()
        {
            biteSystem.StopWaiting();
            reelingSystem.StopFight();
            castingSystem.ResetCast();

            CurrentFish = null;
            currentFishData = null;
            currentSpot = null;
            isFishingInProgress = false;
        }

        // Внутренняя реализация IFishable на основе FishData
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