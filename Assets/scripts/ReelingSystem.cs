using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fishing.Core;
using Fishing.Core.Interfaces;

namespace Fishing.Systems
{
    public class ReelingSystem : MonoBehaviour
    {
        [Header("Настройки мини-игры")]
        [SerializeField] private float fightDuration = 10f;
        [SerializeField] private float targetZoneSize = 0.3f;
        [SerializeField] private float tensionMultiplier = 0.7f;

        [Header("Интерфейс")]
        [SerializeField] private GameObject reelingUI;
        [SerializeField] private Slider tensionSlider;
        [SerializeField] private TMP_Text hintText;

        private FishingController controller;
        private IFishable currentFish;
        private float fightTimer;
        private bool isFighting;

        public float CurrentTension { get; private set; }
        public float FishResistance => currentFish?.CurrentResistance ?? 0f;

        public void Initialize(FishingController fishingController)
        {
            controller = fishingController;
        }

        public void StartFight(IFishable fish)
        {
            currentFish = fish;
            fightTimer = 0f;
            CurrentTension = 0f;
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

            float wantedTension = Input.GetMouseButton(0) ? 1f : 0f;

            CurrentTension = Mathf.MoveTowards(
                CurrentTension,
                wantedTension,
                tensionMultiplier * Time.deltaTime
            );

            float halfZone = targetZoneSize / 2f;
            bool isInTargetZone =
                CurrentTension >= FishResistance - halfZone &&
                CurrentTension <= FishResistance + halfZone;

            UpdateUI(isInTargetZone);

            if (isInTargetZone)
            {
                if (currentFish.ApplyTension(CurrentTension))
                {
                    OnFishTired();
                    return;
                }
            }
            else
            {
                currentFish.ApplyTension(0f);
            }

            fightTimer += Time.deltaTime;

            if (fightTimer >= fightDuration)
            {
                controller.OnFishEscape();
                StopFight();
            }
        }

        private void UpdateUI(bool isInTargetZone)
        {
            if (tensionSlider != null)
                tensionSlider.value = CurrentTension;

            if (hintText == null)
                return;

            string instruction;

            if (isInTargetZone)
                instruction = "Держи так!";
            else if (CurrentTension < FishResistance)
                instruction = "Зажми ЛКМ — натяни леску";
            else
                instruction = "Отпусти ЛКМ — ослабь леску";

            hintText.text =
                $"Леска: {CurrentTension:F2} | Цель: {FishResistance:F2}\n" +
                instruction;
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

            if (tensionSlider != null)
                tensionSlider.value = 0f;

            if (reelingUI != null)
                reelingUI.SetActive(false);
        }

        public void ForceEscape()
        {
            if (isFighting)
                controller.OnFishEscape();

            StopFight();
        }
    }
}