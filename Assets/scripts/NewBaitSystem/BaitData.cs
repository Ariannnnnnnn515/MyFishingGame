using UnityEngine;

[CreateAssetMenu(fileName = "NewBait", menuName = "Fishing Game/Bait Data")]
public class BaitData : ScriptableObject
{
    public string baitName;
    public Sprite baitIcon;
    public GameObject baitPrefab; // 3D модель наживки на крючке (опционально)
    public int basePrice; // цена в магазине
    internal float biteSpeedMultiplier;
    internal int usesPerPurchase;
    internal int price;
}