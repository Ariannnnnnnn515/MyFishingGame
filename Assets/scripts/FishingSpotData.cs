using UnityEngine;

namespace Fishing.Core.Data
{
    /// <summary>
    /// Данные о месте рыбалки
    /// </summary>
    [CreateAssetMenu(fileName = "NewFishingSpot", menuName = "Fishing/Spot Data")]
    public class FishingSpotData : ScriptableObject
    {
        public string spotName = "Озеро";
        public float biteSpeedMultiplier = 1f; // 1 = стандартная скорость, >1 быстрее
        public Vector3 spotPosition;
        public float radius = 5f;

        [Header("Доступные рыбы")]
        public FishData[] availableFish;
    }
}