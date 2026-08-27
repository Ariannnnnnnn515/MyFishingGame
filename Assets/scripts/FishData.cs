using System.Collections.Generic;
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

        [Header("Экономика")]
        [Min(1)] public int pricePerKilogram = 20;

        // ========= НОВАЯ СИСТЕМА ПРЕДПОЧТЕНИЙ НАЖИВОК =========
        [Header("Предпочтения наживок")]
        [Tooltip("Настройка вероятности поклевки для каждой наживки")]
        public List<FishBaitPreference> baitPreferences;

        /// <summary>
        /// Получить шанс поклевки для конкретной наживки
        /// </summary>
        /// <param name="bait">Наживка, которую проверяем</param>
        /// <returns>Шанс в процентах (0-100), 0 если наживка не подходит</returns>
        public int GetChanceForBait(BaitData bait)
        {
            if (baitPreferences == null || baitPreferences.Count == 0)
                return 0;

            foreach (var pref in baitPreferences)
            {
                if (pref.bait == bait)
                {
                    // Ограничиваем значение от 0 до 100 для безопасности
                    return Mathf.Clamp(pref.chance, 0, 100);
                }
            }
            return 0;
        }

        /// <summary>
        /// Проверяет, есть ли у рыбы предпочтения вообще
        /// </summary>
        public bool HasBaitPreferences()
        {
            return baitPreferences != null && baitPreferences.Count > 0;
        }
    }
}