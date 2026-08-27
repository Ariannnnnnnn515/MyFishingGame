using Fishing.Core.Data;
using TMPro;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("Системы")]
    [SerializeField] private FishInventory fishInventory;
    [SerializeField] private PlayerWallet playerWallet;
    [SerializeField] private PlayerBaitInventory playerBaitInventory; // <-- ДОБАВЛЕНО!

    [Header("Интерфейс магазина")]
    [SerializeField] private GameObject shopRoot;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text fishText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text baitText; // Текст для отображения наживки

    [Header("Кнопки покупки наживок")]
    [SerializeField] private BaitData baitTesto; // Ссылка на ассет Теста
    [SerializeField] private BaitData baitCherv; // Ссылка на ассет Червя
    [SerializeField] private int baitTestoPrice = 15;
    [SerializeField] private int baitChervPrice = 25;

    private void Start()
    {
        // Подписываемся на события
        if (fishInventory != null)
            fishInventory.OnInventoryChanged += RefreshUI;

        if (playerWallet != null)
            playerWallet.OnCoinsChanged += RefreshUI;

        if (playerBaitInventory != null)
            playerBaitInventory.OnInventoryChanged += RefreshUI;

        shopRoot.SetActive(false);
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (fishInventory != null)
            fishInventory.OnInventoryChanged -= RefreshUI;

        if (playerWallet != null)
            playerWallet.OnCoinsChanged -= RefreshUI;

        if (playerBaitInventory != null)
            playerBaitInventory.OnInventoryChanged -= RefreshUI;

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
        // Обновляем монеты
        if (coinsText != null)
            coinsText.text = $"Монеты: {playerWallet.Coins}";

        // Обновляем информацию о рыбе
        if (fishText != null)
        {
            fishText.text =
                $"Рыб: {fishInventory.FishCount}\n" +
                $"Стоимость: {fishInventory.TotalValue}";
        }

        // ========== НОВАЯ СИСТЕМА ОТОБРАЖЕНИЯ НАЖИВОК ==========
        if (baitText != null)
        {
            // Получаем количество наживок
            int testoCount = playerBaitInventory.GetBaitCount(baitTesto);
            int chervCount = playerBaitInventory.GetBaitCount(baitCherv);

            // Формируем текст
            string baitInfo = "=== НАЖИВКИ ===\n";
            baitInfo += $"🍞 Тесто: {testoCount} шт.\n";
            baitInfo += $"🐛 Червь: {chervCount} шт.\n";
            baitInfo += "━━━━━━━━━━━━━━━\n";

            baitText.text = baitInfo;
        }
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);

        if (messageText != null)
            messageText.text = message;

        RefreshUI();
    }

    // Методы покупки наживок
    public void BuyTesto()
    {
        BuyBait(baitTesto, baitTestoPrice);
    }

    public void BuyCherv()
    {
        BuyBait(baitCherv, baitChervPrice);
    }

    private void BuyBait(BaitData bait, int price)
    {
        if (bait == null)
        {
            ShowMessage("Данные наживки не назначены.");
            return;
        }

        if (!playerWallet.TrySpendCoins(price))
        {
            ShowMessage($"Недостаточно монет. Нужно: {price}");
            return;
        }

        playerBaitInventory.AddBait(bait, 1);
        ShowMessage($"Куплено: {bait.baitName} (1 шт.)");
        RefreshUI();
    }

    // Метод для покупки нескольких штук (например, 5)
    public void BuyTesto5()
    {
        BuyBait(baitTesto, baitTestoPrice * 5, 5);
    }

    public void BuyCherv5()
    {
        BuyBait(baitCherv, baitChervPrice * 5, 5);
    }

    private void BuyBait(BaitData bait, int price, int amount)
    {
        if (bait == null)
        {
            ShowMessage("Данные наживки не назначены.");
            return;
        }

        if (!playerWallet.TrySpendCoins(price))
        {
            ShowMessage($"Недостаточно монет. Нужно: {price}");
            return;
        }

        playerBaitInventory.AddBait(bait, amount);
        ShowMessage($"Куплено: {bait.baitName} ({amount} шт.)");
        RefreshUI();
    }
}