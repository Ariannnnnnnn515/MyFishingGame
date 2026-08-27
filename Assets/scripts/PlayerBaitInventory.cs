using Fishing.Core.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBaitInventory : MonoBehaviour
{
    public static PlayerBaitInventory Instance;

    [Header("Стартовый набор")]
    public BaitData starterBait;
    public int starterAmount = 15;

    private Dictionary<BaitData, int> baitCounts = new Dictionary<BaitData, int>();

    public event System.Action OnInventoryChanged;

    private void Awake()
    {
        // Синглтон
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Инициализация словаря
        baitCounts = new Dictionary<BaitData, int>();
    }

    private void Start()
    {
        // Добавляем стартовую наживку
        if (starterBait != null)
        {
            AddBait(starterBait, starterAmount);
            Debug.Log($"Добавлена стартовая наживка: {starterBait.baitName} x{starterAmount}");
        }
        else
        {
            Debug.LogWarning("Starter Bait не назначен в PlayerBaitInventory!");
        }
    }

    public void AddBait(BaitData bait, int amount)
    {
        if (bait == null)
        {
            Debug.LogWarning("Попытка добавить null наживку!");
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"Количество должно быть больше 0! (было: {amount})");
            return;
        }

        if (baitCounts.ContainsKey(bait))
        {
            baitCounts[bait] += amount;
        }
        else
        {
            baitCounts[bait] = amount;
        }

        Debug.Log($"Добавлена наживка: {bait.baitName} +{amount} (всего: {baitCounts[bait]})");
        OnInventoryChanged?.Invoke();
    }

    public bool SpendBait(BaitData bait, int amount = 1)
    {
        if (bait == null)
        {
            Debug.LogWarning("Попытка потратить null наживку!");
            return false;
        }

        if (!baitCounts.ContainsKey(bait) || baitCounts[bait] < amount)
        {
            Debug.LogWarning($"Недостаточно наживки {bait.baitName}! (нужно: {amount}, есть: {GetBaitCount(bait)})");
            return false;
        }

        baitCounts[bait] -= amount;

        if (baitCounts[bait] <= 0)
        {
            baitCounts.Remove(bait);
            Debug.Log($"Наживка {bait.baitName} закончилась!");
        }
        else
        {
            Debug.Log($"Потрачена наживка: {bait.baitName} -1 (осталось: {baitCounts[bait]})");
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetBaitCount(BaitData bait)
    {
        if (bait == null)
            return 0;

        return baitCounts.ContainsKey(bait) ? baitCounts[bait] : 0;
    }

    public List<BaitData> GetOwnedBaits()
    {
        return baitCounts.Keys.ToList();
    }

    public bool HasBait(BaitData bait)
    {
        return GetBaitCount(bait) > 0;
    }

    public bool HasAnyBait()
    {
        return baitCounts.Count > 0 && baitCounts.Values.Any(count => count > 0);
    }

    public void ClearInventory()
    {
        baitCounts.Clear();
        OnInventoryChanged?.Invoke();
        Debug.Log("Инвентарь наживок очищен!");
    }
}