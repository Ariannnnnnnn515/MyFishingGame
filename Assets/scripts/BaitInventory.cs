using Fishing.Core.Data;
using System;
using UnityEngine;

public class BaitInventory : MonoBehaviour
{
    [SerializeField] private BaitData currentBait;
    [SerializeField] private int usesRemaining;

    public event Action OnBaitChanged;

    public BaitData CurrentBait => currentBait;
    public int UsesRemaining => usesRemaining;

    public void AddBait(BaitData bait)
    {
        if (bait == null)
        {
            Debug.LogWarning("Попытка добавить null наживку!");
            return;
        }

        // Если наживка та же - просто добавляем использования
        if (currentBait == bait)
        {
            usesRemaining += bait.usesPerPurchase;
        }
        else
        {
            // Если наживка другая - меняем на новую
            currentBait = bait;
            usesRemaining = bait.usesPerPurchase;
        }

        OnBaitChanged?.Invoke();
        Debug.Log($"Добавлена наживка {bait.baitName}. Осталось использований: {usesRemaining}");
    }

    public float UseBait()
    {
        if (currentBait == null || usesRemaining <= 0)
        {
            Debug.LogWarning("Нет наживки для использования!");
            return 1f; // Возвращаем стандартный множитель
        }

        usesRemaining--;
        OnBaitChanged?.Invoke();
        Debug.Log($"Использована наживка {currentBait.baitName}. Осталось: {usesRemaining}");
        return currentBait.biteSpeedMultiplier;
    }

    public void ResetBait()
    {
        usesRemaining = 0;
        OnBaitChanged?.Invoke();
        Debug.Log("Наживка сброшена");
    }

    public bool HasBait()
    {
        return currentBait != null && usesRemaining > 0;
    }

    public int GetUsesRemaining()
    {
        return usesRemaining;
    }
}