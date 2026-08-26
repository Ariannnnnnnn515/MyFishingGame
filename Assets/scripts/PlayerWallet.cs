using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int startingCoins = 50;

    public event Action OnCoinsChanged;
    public int Coins { get; private set; }

    private void Awake()
    {
        Coins = Mathf.Max(0, startingCoins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        Coins += amount;
        OnCoinsChanged?.Invoke();
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0 || Coins < amount)
            return false;

        Coins -= amount;
        OnCoinsChanged?.Invoke();
        return true;
    }

    public void ResetWallet()
    {
        Coins = Mathf.Max(0, startingCoins);
        OnCoinsChanged?.Invoke();
    }

    internal bool TrySpendCoins(object price)
    {
        throw new NotImplementedException();
    }
}