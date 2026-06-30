using System;
using UnityEngine;
using Fishing.Core.Interfaces;
using Fishing.Core.Data;
using Fishing.Systems;

namespace Fishing.Core
{
    /// <summary>
    /// Главный контроллер рыбалки. Синглтон.
    /// Координирует системы броска, поклёвки и вываживания.
    /// </summary>
    public class FishingController : MonoBehaviour
    {
        public static FishingController Instance { get; private set; }

        // Ссылки на подсистемы (инжектим через Inspector или Find)
        [SerializeField] private CastingSystem castingSystem;
        [SerializeField] private BiteSystem biteSystem;
        [SerializeField] private ReelingSystem reelingSystem;

        [Header("Текущие данные")]
        [SerializeField] private FishingSpotData currentSpot;
        [SerializeField] private FishData currentFishData; // Целевая рыба (до поклёвки)
        public IFishable CurrentFish { get; private set; } // Экземпляр рыбы в бою

        public event Action<FishData> OnFishHooked;   // Событие для UI/Аудио
        public event Action<FishData> OnFishLanded;
        public event Action OnFishEscaped;

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
        }

        /// <summary>
        /// Инициировать бросок в указанную точку.
        /// </summary>
        public void PerformCast(Vector3 targetPosition, FishingSpotData spot)
        {
            if (CurrentFish != null && CurrentFish.State != FishState.Landed)
            {
                Debug.LogWarning("Нельзя бросить, пока рыба на крючке!");
                return;
            }

            currentSpot = spot;
            castingSystem.StartCast(targetPosition, OnCastComplete);
        }

        /// <summary>
        /// Колбэк после завершения броска (леска упала в воду).
        /// </summary>
        private void OnCastComplete()
        {
            Debug.Log("Леска в воде. Ждём поклёвку...");
            biteSystem.StartWaiting(currentSpot);
        }

        /// <summary>
        /// Вызывается из BiteSystem при поклёвке.
        /// </summary>
        public void OnBiteOccurred(FishData fishData)
        {
            currentFishData = fishData;
            // Создаём экземпляр рыбы (например, через Factory или простой new)
            CurrentFish = new FishInstance(fishData); // Реализуем ниже

            CurrentFish.OnHooked();
            OnFishHooked?.Invoke(fishData);

            // Переключаемся в мини-игру вываживания
            reelingSystem.StartFight(CurrentFish);
        }

        /// <summary>
        /// Вызывается из ReelingSystem, когда рыба вымотана.
        /// </summary>
        public void OnFishTired()
        {
            // Логика подсечки и финального вываживания
            CurrentFish.State = FishState.Landed;
            OnFishLanded?.Invoke(currentFishData);

            // Спавн модели рыбы в руках игрока (вызов извне)
            Debug.Log($"Рыба {currentFishData.fishName} поймана!");

            // Сброс состояния
            CurrentFish = null;
        }

        public void OnFishEscape()
        {
            CurrentFish?.OnEscape();
            OnFishEscaped?.Invoke();
            CurrentFish = null;
            Debug.Log("Рыба сорвалась!");
        }

        // Простая реализация IFishable на основе FishData
        private class FishInstance : IFishable
        {
            private FishData data;
            private float maxResistance;
            private float currentTiredness = 0f;

            public string SpeciesId => data.name;
            public FishState State { get; set; }
            public float CurrentResistance => Mathf.Lerp(0.2f, 1f, currentTiredness / data.escapeSpeed);

            public FishInstance(FishData data)
            {
                this.data = data;
                maxResistance = data.baseResistance * UnityEngine.Random.Range(data.weightMin, data.weightMax);
                State = FishState.Hooked;
            }

            public void OnHooked() => State = FishState.Fighting;
            public void OnEscape() => State = FishState.Idle;

            public bool ApplyTension(float tensionPower)
            {
                // tensionPower = сила натяжения от игрока (0-1)
                // Если tensionPower > сопротивления, рыба устаёт
                if (tensionPower > CurrentResistance)
                {
                    currentTiredness += Time.deltaTime * 0.5f;
                }
                else
                {
                    // Если игрок ослабляет, рыба восстанавливает силы (чуть-чуть)
                    currentTiredness -= Time.deltaTime * 0.1f;
                }

                currentTiredness = Mathf.Clamp01(currentTiredness);

                if (currentTiredness >= 1f)
                {
                    State = FishState.Tired;
                    return true; // Рыба устала
                }
                return false;
            }
        }
    }
}