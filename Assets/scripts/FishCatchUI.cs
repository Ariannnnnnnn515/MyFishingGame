using UnityEngine;
using UnityEngine.UI;
using TMPro; // Если используете TextMeshPro

// Это упрощенная модель вашего конфига. У вас может быть свой класс.
// Главное, чтобы в нем были поля для имени, веса и спрайта/префаба.
[System.Serializable]
public class FishConfig
{
    public string fishName;
    public float fishWeight;
    public Sprite fishSprite; // Или GameObject fishPrefab для 3D
    // ... другие ваши поля
}

public class FishCatchUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image fishImage;       // Ссылка на компонент Image для рыбы
    [SerializeField] private TMP_Text nameText;      // Ссылка на TextMeshPro
    [SerializeField] private TMP_Text weightText;    // Ссылка на TextMeshPro
    [SerializeField] private Button keepButton;      // Ссылка на кнопку "В садок"
    [SerializeField] private Button releaseButton;   // Ссылка на кнопку "Отпустить"

    // Этот метод будет вызван из вашего основного скрипта рыбалки,
    // когда рыба будет выужена.
    public void ShowCatchResult(FishConfig caughtFish)
    {
        // 1. Заполняем UI данными из конфига
        if (caughtFish != null)
        {
            // Устанавливаем спрайт рыбы
            if (fishImage != null && caughtFish.fishSprite != null)
            {
                fishImage.sprite = caughtFish.fishSprite;
                fishImage.gameObject.SetActive(true); // Убеждаемся, что изображение активно
            }
            else
            {
                // Если спрайта нет, скрываем изображение, чтобы не было пустоты
                if (fishImage != null) fishImage.gameObject.SetActive(false);
            }

            // Устанавливаем имя и вес
            if (nameText != null) nameText.text = caughtFish.fishName;
            if (weightText != null) weightText.text = $"Вес: {caughtFish.fishWeight:F2} кг."; // Форматируем вес
        }

        // 2. Активируем панель
        this.gameObject.SetActive(true);
    }

    // Этот метод вызывается кнопкой "В садок"
    public void OnKeepButtonClick()
    {
        Debug.Log("Рыба помещена в садок!");
        // --- ВАЖНО: Здесь будет ваша логика добавления рыбы в садок/инвентарь ---
        // Например: InventoryManager.Instance.AddFish(currentFishData);
        
        // Закрываем панель
        this.gameObject.SetActive(false);
    }

    // Этот метод вызывается кнопкой "Отпустить"
    public void OnReleaseButtonClick()
    {
        Debug.Log("Рыба отпущена!");
        // --- ВАЖНО: Здесь будет ваша логика отпускания рыбы ---
        // Можно ничего не делать, просто закрыть панель, или добавить анимацию уплывания.

        // Закрываем панель
        this.gameObject.SetActive(false);
    }
}