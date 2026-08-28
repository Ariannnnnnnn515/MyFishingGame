using UnityEngine;
using Fishing.Core.Interfaces;
using Fishing.Core;
using TMPro;

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
        [SerializeField] private float fightDuration = 10f;
        [SerializeField] private float targetZoneSize = 0.3f;
        [SerializeField] private float tensionMultiplier = 0.7f;

        [Header("Интерфейс")]
        [SerializeField] private GameObject reelingUI;
        [SerializeField] private UnityEngine.UI.Slider tensionSlider;
        [SerializeField] private TMP_Text hintText;

        private IFishable currentFish;
        private float fightTimer;
        private bool isFighting;
        private float currentTension;

        public float CurrentTension => currentTension;
        public float FishResistance => currentFish?.CurrentResistance ?? 0f;

        public void Initialize(FishingController controller)
        {
            fishingController = controller;
            Debug.Log("ReelingSystem инициализирован (классический режим)");
        }

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

            Debug.Log($"ReelingSystem: СТАРТ БОЯ! Рыба: {fish.SpeciesId}");

            currentFish = fish;
            fightTimer = 0f;
            currentTension = 0f;
            isFighting = true;

            if (reelingUI != null)
                reelingUI.SetActive(true);

            UpdateUI(false);
            Debug.Log("Мини-игра началась: удерживай и отпускай ЛКМ.");
        }

        private void Update()
        {
            if (!isFighting || currentFish == null)
                return;

            // Ввод игрока: ЛКМ или пробел
            float wantedTension = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) ? 1f : 0f;

            // Плавно меняем натяжение
            currentTension = Mathf.MoveTowards(
                currentTension,
                wantedTension,
                tensionMultiplier * Time.deltaTime
            );

            // Проверяем, находится ли натяжение в целевой зоне
            float halfZone = targetZoneSize / 2f;
            bool isInTargetZone = currentTension >= FishResistance - halfZone &&
                                  currentTension <= FishResistance + halfZone;

            // Обновляем UI
            UpdateUI(isInTargetZone);

            // Применяем натяжение к рыбе
            if (isInTargetZone)
            {
                // Если в зоне - утомляем рыбу
                if (currentFish.ApplyTension(currentTension))
                {
                    OnFishTired();
                    return;
                }
            }
            else
            {
                // Если вне зоны - рыба отдыхает
                currentFish.ApplyTension(0f);
            }

            // Обновляем таймер
            fightTimer += Time.deltaTime;

            // Если время вышло - рыба сорвалась
            if (fightTimer >= fightDuration)
            {
                Debug.Log("Рыба сорвалась! (время вышло)");
                OnFishEscape();
            }
        }

        private void UpdateUI(bool isInTargetZone)
        {
            if (tensionSlider != null)
                tensionSlider.value = currentTension;

            if (hintText == null)
                return;

            string instruction;
            if (isInTargetZone)
            {
                instruction = "Держи так!";
                hintText.color = Color.green;
            }
            else if (currentTension < FishResistance)
            {
                instruction = "Зажми ЛКМ — натяни леску";
                hintText.color = Color.yellow;
            }
            else
            {
                instruction = "Отпусти ЛКМ — ослабь леску";
                hintText.color = Color.yellow;
            }

            hintText.text =
                $"Леска: {currentTension:F2} | Цель: {FishResistance:F2}\n" +
                $"{instruction}";
        }

        private void OnFishTired()
        {
            if (!isFighting || currentFish == null)
                return;

            isFighting = false;
            Debug.Log($"ReelingSystem: Рыба {currentFish.SpeciesId} утомлена! Успех!");

            if (reelingUI != null)
                reelingUI.SetActive(false);

            if (fishingController != null)
                fishingController.OnFishTired();
            else
                Debug.LogError("ReelingSystem: FishingController не назначен!");

            currentFish = null;
        }

        private void OnFishEscape()
        {
            if (!isFighting || currentFish == null)
                return;

            isFighting = false;
            Debug.Log($"ReelingSystem: Рыба {currentFish.SpeciesId} сорвалась!");

            if (reelingUI != null)
                reelingUI.SetActive(false);

            if (fishingController != null)
                fishingController.OnFishEscape();
            else
                Debug.LogError("ReelingSystem: FishingController не назначен!");

            currentFish = null;
        }

        public void StopFight()
        {
            if (!isFighting)
                return;

            isFighting = false;
            currentFish = null;
            currentTension = 0f;

            if (tensionSlider != null)
                tensionSlider.value = 0f;

            if (reelingUI != null)
                reelingUI.SetActive(false);

            Debug.Log("ReelingSystem: мини-игра остановлена");
        }

        public bool IsFighting() => isFighting;
        public IFishable GetCurrentFish() => currentFish;
    }
}