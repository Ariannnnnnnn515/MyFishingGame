using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerBaitInventory : MonoBehaviour
{
    public static PlayerBaitInventory Instance; // Простой синглтон

    // Словарь для хранения: наживка -> количество
    private Dictionary<BaitData, int> baitCounts = new Dictionary<BaitData, int>();

    [Header("Стартовый набор")]
    public BaitData starterBait; // Сюда назначишь "Тесто"
    public int starterAmount = 15;

    // Событие, чтобы обновлять UI при изменении
    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Добавляем стартовую наживку
        if (starterBait != null)
        {
            AddBait(starterBait, starterAmount);
        }
    }

    // Добавить наживку (например, после покупки)
    public void AddBait(BaitData bait, int amount)
    {
        if (baitCounts.ContainsKey(bait))
            baitCounts[bait] += amount;
        else
            baitCounts[bait] = amount;

        OnInventoryChanged?.Invoke();
    }

    // Потратить наживку
    public bool SpendBait(BaitData bait, int amount = 1)
    {
        if (!baitCounts.ContainsKey(bait) || baitCounts[bait] < amount)
            return false;

        baitCounts[bait] -= amount;

        if (baitCounts[bait] <= 0)
            baitCounts.Remove(bait);

        OnInventoryChanged?.Invoke();
        return true;
    }

    // Получить количество
    public int GetBaitCount(BaitData bait)
    {
        return baitCounts.ContainsKey(bait) ? baitCounts[bait] : 0;
    }

    // Получить список всех наживок, которые есть у игрока (для UI)
    public List<BaitData> GetOwnedBaits()
    {
        return baitCounts.Keys.ToList();
    }
}