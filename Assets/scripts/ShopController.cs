using TMPro;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("Системы")]
    [SerializeField] private FishInventory fishInventory;
    [SerializeField] private PlayerWallet playerWallet;

    [Header("Интерфейс магазина")]
    [SerializeField] private GameObject shopRoot;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text fishText;
    [SerializeField] private TMP_Text messageText;

    private void Start()
    {
        fishInventory.OnInventoryChanged += RefreshUI;
        playerWallet.OnCoinsChanged += RefreshUI;

        shopRoot.SetActive(false);
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (fishInventory != null)
            fishInventory.OnInventoryChanged -= RefreshUI;

        if (playerWallet != null)
            playerWallet.OnCoinsChanged -= RefreshUI;

        Time.timeScale = 1f;
    }

    public void OpenShop()
    {
        shopRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
        RefreshUI();
    }

    public void CloseShop()
    {
        shopRoot.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }

    public void SellAllFish()
    {
        int fishCount = fishInventory.FishCount;
        int money = fishInventory.SellAll();

        if (money <= 0)
        {
            ShowMessage("В инвентаре нет рыбы.");
            return;
        }

        playerWallet.AddCoins(money);
        ShowMessage($"Продано рыб: {fishCount}. Получено: {money} монет.");
    }

    private void RefreshUI()
    {
        if (coinsText != null)
            coinsText.text = $"Монеты: {playerWallet.Coins}";

        if (fishText != null)
        {
            fishText.text =
                $"Рыб: {fishInventory.FishCount}\n" +
                $"Стоимость: {fishInventory.TotalValue}";
        }
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);

        if (messageText != null)
            messageText.text = message;

        RefreshUI();
    }
}