using UnityEngine;

[CreateAssetMenu(fileName = "NewBait", menuName = "Fishing/Bait Data")]
public class BaitData : ScriptableObject
{
    public string baitName = "Тесто";
    [Min(1)] public int price = 15;
    [Min(1)] public int usesPerPurchase = 5;

    [Tooltip("Во сколько раз быстрее происходит поклёвка")]
    [Range(1f, 3f)] public float biteSpeedMultiplier = 1.7f;
}