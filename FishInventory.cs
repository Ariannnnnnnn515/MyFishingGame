using System;
using System.Collections.Generic;
using UnityEngine;
using Fishing.Core.Data;

private const string InventoryKey = "FishingGame.Inventory";

[Serializable]
private class InventorySaveData
{
    public List<CaughtFish> fish = new List<CaughtFish>();
}

public class CaughtFish
{
    public string fishName;
    public float weight;
    public int price;

    public CaughtFish(FishData fishData, float fishWeight)
    {
        fishName = fishData.fishName;
        weight = fishWeight;
        price = Mathf.Max(1,
            Mathf.RoundToInt(fishWeight * fishData.pricePerKilogram));
    }
}

public class FishInventory : MonoBehaviour
{
    [SerializeField]
    private List<CaughtFish> caughtFish =
        new List<CaughtFish>();

    public event Action OnInventoryChanged;

    public int FishCount => caughtFish.Count;

    public int TotalValue
    {
        get
        {
            int total = 0;

            foreach (CaughtFish fish in caughtFish)
                total += fish.price;

            return total;
        }
    }

    public void AddFish(FishData fishData, float weight)
    {
        CaughtFish newFish = new CaughtFish(fishData, weight);
        caughtFish.Add(newFish);
        OnInventoryChanged?.Invoke();

        Debug.Log(
            $"В инвентаре рыб: {FishCount}. " +
            $"Стоимость улова: {TotalValue} монет."
        );
    }

    public int SellAll()
    {
        int money = TotalValue;
        caughtFish.Clear();
        OnInventoryChanged?.Invoke();
        return money;
    }

    public void Clear()
    {
        caughtFish.Clear();
        OnInventoryChanged?.Invoke();
    }
}
