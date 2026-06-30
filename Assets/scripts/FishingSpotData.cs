using UnityEngine;
using System.Collections.Generic;

namespace Fishing.Core.Data
{
    /// <summary>
    /// Зона ловли (у воды). Определяет, какая рыба водится и шансы.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFishingSpot", menuName = "Fishing/Spot Data")]
    public class FishingSpotData : ScriptableObject
    {
        public string spotName = "Лесное озеро";

        [System.Serializable]
        public struct FishPoolEntry
        {
            public FishData fishData;
            [Range(0, 100)] public int spawnWeight; // Вес для случайного выбора
        }

        public FishPoolEntry[] fishPool;
        public float biteChanceModifier = 1.0f; // Множитель клёва (погода/время)

        [Header("Визуальные эффекты")]
        public ParticleSystem rippleEffect;
        public AudioClip waterSplash;
    }
}