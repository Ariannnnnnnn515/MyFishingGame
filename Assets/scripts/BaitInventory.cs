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
            return;

        if (currentBait != bait)
        {
            currentBait = bait;
            usesRemaining = 0;
        }

        usesRemaining += bait.usesPerPurchase;
        OnBaitChanged?.Invoke();
    }

    public float UseBait()
    {
        if (currentBait == null || usesRemaining <= 0)
            return 1f;

        usesRemaining--;
        OnBaitChanged?.Invoke();
        return currentBait.biteSpeedMultiplier;
    }

    public void ResetBait()
    {
        usesRemaining = 0;
        OnBaitChanged?.Invoke();
    }
}