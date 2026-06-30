using UnityEngine;

namespace Fishing.Core.Data
{
    /// <summary>
    /// Настройка конкретного вида рыбы. 
    /// Используется для генерации экземпляров IFishable.
    /// </summary>
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
        public float escapeSpeed = 0.3f; // Скорость усталости при рывках

        [Header("Визуал")]
        public GameObject fishPrefab;      // Модель рыбы (для спавна в руки)
        public Sprite iconInUI;

        [Header("Награда")]
        public int experienceReward = 10;
    }
}