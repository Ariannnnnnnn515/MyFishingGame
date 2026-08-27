using System.Collections.Generic;
using UnityEngine;

namespace Fishing.Core.Data
{
    [CreateAssetMenu(fileName = "NewFish", menuName = "Fishing/Fish Data")]
    public class FishData : ScriptableObject
    {
        [Header("Основное")]
        public string fishName = "Окунь";
        public float weightMin = 0.2f;
        public float weightMax = 1.5f;

        [Header("Поведение")]
        [Tooltip("Базовое сопротивление (0-1). Умножается на вес.")]
        public float baseResistance = 0.5f;
        public float escapeSpeed = 0.3f;

        [Header("Визуал")]
        public GameObject fishPrefab;
        public Sprite iconInUI;

        [Header("Награда")]
        public int experienceReward = 10;

        [Header("Экономика")]
        [Min(1)] public int pricePerKilogram = 20;

        [Header("Предпочтения наживок")]
        public List<FishBaitPreference> baitPreferences;

        public int GetChanceForBait(BaitData bait)
        {
            if (baitPreferences == null || baitPreferences.Count == 0)
                return 0;

            foreach (var pref in baitPreferences)
            {
                if (pref.bait == bait)
                {
                    return Mathf.Clamp(pref.chance, 0, 100);
                }
            }
            return 0;
        }

        public bool HasBaitPreferences()
        {
            return baitPreferences != null && baitPreferences.Count > 0;
        }
    }
}