using System;
using UnityEngine;
using Fishing.Core.Interfaces;
using Fishing.Core.Data;
using Fishing.Systems;

namespace Fishing.Core
{
    /// <summary>
    /// ������� ���������� �������. ��������.
    /// ������������ ������� ������, ������� � �����������.
    /// </summary>
    public class FishingController : MonoBehaviour
    {
        public static FishingController Instance { get; private set; }

        // ������ �� ���������� (�������� ����� Inspector ��� Find)
        [SerializeField] private CastingSystem castingSystem;
        [SerializeField] private BiteSystem biteSystem;
        [SerializeField] private ReelingSystem reelingSystem;

        [Header("������� ������")]
        [SerializeField] private FishingSpotData currentSpot;
        [SerializeField] private FishData currentFishData; // ������� ���� (�� �������)
        public IFishable CurrentFish { get; private set; } // ��������� ���� � ���

        public event Action<FishData> OnFishHooked;   // ������� ��� UI/�����
        public event Action<FishData> OnFishLanded;
        public event Action OnFishEscaped;
        private bool isFishingInProgress;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            // ������������� ���������
            castingSystem.Initialize(this);
            biteSystem.Initialize(this);
            reelingSystem.Initialize(this);
        }

        /// <summary>
        /// ������������ ������ � ��������� �����.
        /// </summary>
        public void PerformCast(Vector3 targetPosition, FishingSpotData spot)
        {
            if (isFishingInProgress)
            {
                Debug.LogWarning("������� ������� ������� �������!");
                return;
            }

            if (spot == null)
            {
                Debug.LogError("��� ������� �� ������� FishingSpotData!");
                return;
            }

            isFishingInProgress = true;
            currentSpot = spot;
            castingSystem.StartCast(targetPosition, OnCastComplete);
        }

        /// <summary>
        /// ������ ����� ���������� ������ (����� ����� � ����).
        /// </summary>
        private void OnCastComplete()
        {
            Debug.Log("����� � ����. ��� �������...");
            biteSystem.StartWaiting(currentSpot);
        }

        /// <summary>
        /// ���������� �� BiteSystem ��� �������.
        /// </summary>
        public void OnBiteOccurred(FishData fishData)
        {
            // ������ �� ����������� �������� ����� ������.
            if (!isFishingInProgress || fishData == null)
                return;

            currentFishData = fishData;
            CurrentFish = new FishInstance(fishData);

            CurrentFish.OnHooked();
            OnFishHooked?.Invoke(fishData);
            reelingSystem.StartFight(CurrentFish);
        }

        /// <summary>
        /// ���������� �� ReelingSystem, ����� ���� ��������.
        /// </summary>
        public void OnFishTired()
        {
            if (CurrentFish == null || currentFishData == null)
                return;

            CurrentFish.State = FishState.Landed;
            FishData landedFish = currentFishData;

            Debug.Log($"���� {landedFish.fishName} �������!");

            // ����� ����� ����� ���������� ����-����
            // ... ��� ��� ��� ������ ��������� ����-���� ...
            ResetFishingSystems();
            OnFishLanded?.Invoke(landedFish);
        }

        public void OnFishEscape()
        {
            if (!isFishingInProgress)
                return;

            CurrentFish?.OnEscape();
            ResetFishingSystems();
            OnFishEscaped?.Invoke();
            Debug.Log("������� ��������� ��� �����.");
        }

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

        // ������� ���������� IFishable �� ������ FishData
        private class FishInstance : IFishable
        {
            private FishData data;
            private float maxResistance;
            private float currentTiredness = 0f;

            public string SpeciesId => data.name;
            public FishState State { get; set; }
            public float CurrentResistance => Mathf.Clamp(maxResistance, 0.25f, 0.8f);

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
                // ReelingSystem ������� �������� ������ ���� ������ � ������ ����.
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