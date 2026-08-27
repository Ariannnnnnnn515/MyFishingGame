using UnityEngine;

namespace Fishing.Core.Data // <-- ÎÁßÇÀÒÅËÜÍÎ!
{
    [CreateAssetMenu(fileName = "NewBait", menuName = "Fishing/Bait Data")]
    public class BaitData : ScriptableObject
    {
        public string baitName;
        public Sprite baitIcon;
        public GameObject baitPrefab;
        public int basePrice;
        public int usesPerPurchase;
        public float biteSpeedMultiplier;
    }
}