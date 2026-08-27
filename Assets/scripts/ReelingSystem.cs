using Fishing.Core;
using Fishing.Core.Data;
using Fishing.Core.Interfaces;
using System;
using UnityEngine;

namespace Fishing.Systems
{
    /// <summary>
    /// Система вываживания рыбы (мини-игра)
    /// </summary>
    public class ReelingSystem : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private FishingController fishingController;

        [Header("Настройки мини-игры")]
        [SerializeField] private float minFightTime = 3f;
        [SerializeField] private float maxFightTime = 8f;
        [SerializeField] private float tensionMultiplier = 1.5f;
        [SerializeField] private float escapeChance = 0.05f; // 5% шанс схода

        [Header("UI (опционально)")]
        [SerializeField] private GameObject fightUI;
        [SerializeField] private UnityEngine.UI.Slider tensionSlider;
        [SerializeField] private UnityEngine.UI.Slider fishTirednessSlider;

        private IFishable currentFish;
        private bool isFighting;
        private float fightTimer;
        private float targetFightTime;
        private bool isFishTired;

        public event Action<IFishable> OnFightStarted;
        public event Action<IFishable> OnFightEnded;
        public event Action<IFishable> OnFishEscaped;

        private void Awake()
        {
            if (fightUI != null)
                fightUI.SetActive(false);
        }

        private void Update()
        {
            if (!isFighting || currentFish == null)
                return;

            // Обновляем таймер
            fightTimer += Time.deltaTime;

            // Получаем ввод игрока (зажимаем кнопку)
            float tension = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0) ? 1f : 0f;
            tension *= tensionMultiplier;

            // Применяем натяжение к рыбе
            bool tired = currentFish.ApplyTension(tension);

            // Обновляем UI
            UpdateUI(tension, tired);

            // Проверяем, устала ли рыба
            if (tired)
            {
                OnFishTired();
                return;
            }

            // Проверка на сход рыбы (если игрок не держит кнопку)
            if (tension <= 0.1f && fightTimer > 1f)
            {
                float escapeRoll = UnityEngine.Random.value;
                if (escapeRoll < escapeChance)
                {
                    Debug.Log("Рыба сорвалась! (игрок ослабил натяжение)");
                    OnFishEscape();
                    return;
                }
            }

            // Проверка на автоматический сход (если слишком долго)
            if (fightTimer > targetFightTime)
            {
                Debug.Log("Рыба сорвалась! (слишком долго)");
                OnFishEscape();
                return;
            }
        }

        /// <summary>
        /// Запустить мини-игру вываживания
        /// </summary>
        public void StartFight(IFishable fish)
        {
            if (fish == null)
            {
                Debug.LogError("ReelingSystem: Попытка начать бой с null рыбой!");
                return;
            }

            if (isFighting)
            {
                Debug.LogWarning("ReelingSystem: Уже идет бой с рыбой!");
                return;
            }

            Debug.Log($"ReelingSystem.StartFight() вызван для рыбы: {fish.SpeciesId}");

            currentFish = fish;
            isFighting = true;
            isFishTired = false;
            fightTimer = 0f;
            targetFightTime = UnityEngine.Random.Range(minFightTime, maxFightTime);

            // Показываем UI
            if (fightUI != null)
                fightUI.SetActive(true);

            // Сбрасываем UI
            UpdateUI(0f, false);

            OnFightStarted?.Invoke(fish);
            Debug.Log($"Мини-игра началась! Рыба: {fish.SpeciesId}. Требуется утомить за {targetFightTime:F1}с");
        }

        /// <summary>
        /// Остановить мини-игру
        /// </summary>
        public void StopFight()
        {
            if (!isFighting)
                return;

            isFighting = false;
            currentFish = null;

            if (fightUI != null)
                fightUI.SetActive(false);

            Debug.Log("ReelingSystem: мини-игра остановлена");
        }

        /// <summary>
        /// Обработка успешной поимки
        /// </summary>
        private void OnFishTired()
        {
            if (!isFighting || currentFish == null)
                return;

            isFighting = false;
            isFishTired = true;

            Debug.Log($"ReelingSystem: Рыба {currentFish.SpeciesId} утомлена! Успешная поимка!");

            if (fightUI != null)
                fightUI.SetActive(false);

            OnFightEnded?.Invoke(currentFish);

            // Сообщаем FishingController о поимке
            if (fishingController != null)
            {
                fishingController.OnFishTired();
            }
            else
            {
                Debug.LogError("ReelingSystem: FishingController не назначен!");
            }

            currentFish = null;
        }

        /// <summary>
        /// Обработка схода рыбы
        /// </summary>
        private void OnFishEscape()
        {
            if (!isFighting || currentFish == null)
                return;

            isFighting = false;

            Debug.Log($"ReelingSystem: Рыба {currentFish.SpeciesId} сорвалась!");

            if (fightUI != null)
                fightUI.SetActive(false);

            OnFishEscaped?.Invoke(currentFish);

            // Сообщаем FishingController о сходе
            if (fishingController != null)
            {
                fishingController.OnFishEscape();
            }
            else
            {
                Debug.LogError("ReelingSystem: FishingController не назначен!");
            }

            currentFish = null;
        }

        /// <summary>
        /// Обновить UI мини-игры
        /// </summary>
        private void UpdateUI(float tension, bool tired)
        {
            if (!isFighting || currentFish == null)
                return;

            // Обновляем слайдеры
            if (tensionSlider != null)
            {
                tensionSlider.value = Mathf.Clamp01(tension / 2f);
            }

            if (fishTirednessSlider != null)
            {
                // Усталость рыбы (используем время боя)
                float tiredness = Mathf.Clamp01(fightTimer / targetFightTime);
                fishTirednessSlider.value = tiredness;

                // Меняем цвет, если рыба близка к поимке
                if (tiredness > 0.8f)
                {
                    var color = fishTirednessSlider.fillRect.GetComponent<UnityEngine.UI.Image>();
                    if (color != null)
                        color.color = Color.green;
                }
            }
        }

        /// <summary>
        /// Инициализация системы
        /// </summary>
        public void Initialize(FishingController controller)
        {
            fishingController = controller;
            Debug.Log("ReelingSystem инициализирован");
        }

        /// <summary>
        /// Проверка, идет ли бой
        /// </summary>
        public bool IsFighting()
        {
            return isFighting;
        }

        /// <summary>
        /// Получить текущую рыбу
        /// </summary>
        public IFishable GetCurrentFish()
        {
            return currentFish;
        }

        /// <summary>
        /// Получить прогресс боя (0-1)
        /// </summary>
        public float GetFightProgress()
        {
            if (!isFighting || targetFightTime <= 0)
                return 0f;

            return Mathf.Clamp01(fightTimer / targetFightTime);
        }
    }
}