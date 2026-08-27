using UnityEngine;

namespace Fishing.Core.Data
{
    [CreateAssetMenu(fileName = "NewBait", menuName = "Fishing/Bait Data")]
    public class BaitData : ScriptableObject
    {
        [Header("Основное")]
        public string baitName = "Тесто";
        public Sprite baitIcon;
        public GameObject baitPrefab;
        public int basePrice = 10;

        [Header("Игровые параметры")]
        public int usesPerPurchase = 15;
        public float biteSpeedMultiplier = 1.0f;
    }
}