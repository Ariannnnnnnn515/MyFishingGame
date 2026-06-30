using UnityEngine;
using Fishing.Core.Interfaces;
using Fishing.Core;

namespace Fishing.Systems
{
    /// <summary>
    /// Интерактивная мини-игра: игрок должен удерживать ползунок 
    /// в определённой зоне, чтобы утомить рыбу.
    /// </summary>
    public class ReelingSystem : MonoBehaviour
    {
        [Header("Параметры игры")]
        [SerializeField] private float fightDuration = 5f; // Макс. время борьбы
        [SerializeField] private float targetZoneSize = 0.3f; // Размер "золотой зоны"
        [SerializeField] private float tensionMultiplier = 1.5f;

        private FishingController controller;
        private IFishable currentFish;
        private float fightTimer;
        private bool isFighting;

        // Для UI (будет использоваться внешним скриптом)
        public float CurrentTension { get; private set; }
        public float FishResistance => currentFish?.CurrentResistance ?? 0f;

        public void Initialize(FishingController controller) => this.controller = controller;

        /// <summary>
        /// Начать борьбу с рыбой.
        /// </summary>
        public void StartFight(IFishable fish)
        {
            currentFish = fish;
            fightTimer = 0f;
            isFighting = true;
            Debug.Log("Началась мини-игра вываживания!");
        }

        private void Update()
        {
            if (!isFighting || currentFish == null) return;

            // Получаем ввод игрока (ось или кнопка)
            float playerInput = Input.GetAxis("Vertical"); // Или использовать Input System

            // Преобразуем в силу натяжения (0-1)
            CurrentTension = Mathf.Clamp01(Mathf.Abs(playerInput) * tensionMultiplier);

            // Проверяем, находится ли игрок в "золотой зоне"
            bool isInTargetZone = (CurrentTension >= FishResistance - targetZoneSize / 2) &&
                                  (CurrentTension <= FishResistance + targetZoneSize / 2);

            if (isInTargetZone)
            {
                // Правильное натяжение - утомляем рыбу
                if (currentFish.ApplyTension(CurrentTension))
                {
                    // Рыба устала!
                    OnFishTired();
                    return;
                }
            }
            else
            {
                // Игрок ошибается - рыба восстанавливается (ApplyTension с отрицательным эффектом)
                currentFish.ApplyTension(0f); // Даём рыбе отдых
            }

            // Таймер борьбы (если время вышло - рыба уходит)
            fightTimer += Time.deltaTime;
            if (fightTimer >= fightDuration)
            {
                controller.OnFishEscape();
                StopFight();
            }
        }

        private void OnFishTired()
        {
            controller.OnFishTired();
            StopFight();
        }

        public void StopFight()
        {
            isFighting = false;
            currentFish = null;
            CurrentTension = 0f;
        }

        public void ForceEscape()
        {
            if (isFighting)
                controller.OnFishEscape();
            StopFight();
        }
    }
}