using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Класс-конфиг для рыбы (прикрепите его к ScriptableObject или используйте как есть)
[System.Serializable]
public class FishConfig
{
    public string fishName = "Окунь";
    public float fishWeight = 1.5f;
    public GameObject fishPrefab;  // Префаб рыбы
    public Sprite fishIcon;        // Иконка (опционально)
}

public class FishCatchUI : MonoBehaviour
{
    [Header("Компоненты сцены")]
    [SerializeField] private Transform playerTransform;      // Игрок
    [SerializeField] private GameObject fishDisplayRoot;     // Родитель для рыбы
    [SerializeField] private Image backgroundDimImage;       // Затемнение
    [SerializeField] private TextMeshProUGUI fishInfoText;   // Текст с инфой
    
    [Header("Кнопки")]
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private Button keepButton;
    [SerializeField] private Button releaseButton;
    
    [Header("Настройки затемнения")]
    [SerializeField] private Color dimColor = new Color(0, 0, 0, 0.6f);
    [SerializeField] private float fadeDuration = 0.3f;
    
    [Header("Настройки позиции рыбы")]
    [SerializeField] private float spawnDistance = 1.5f;    // Дистанция перед игроком
    [SerializeField] private Vector3 spawnOffset = Vector3.zero; // Смещение

    private GameObject currentFishInstance;
    private FishConfig currentFishConfig;
    private bool isUIActive = false;

    void Start()
    {
        // Инициализация: всё скрыто
        if (backgroundDimImage != null)
        {
            backgroundDimImage.color = new Color(dimColor.r, dimColor.g, dimColor.b, 0);
            backgroundDimImage.gameObject.SetActive(false);
        }
        
        if (fishDisplayRoot != null)
            fishDisplayRoot.SetActive(false);
            
        if (fishInfoText != null)
            fishInfoText.gameObject.SetActive(false);

        if (buttonContainer != null)
            buttonContainer.SetActive(false);

        // Назначаем обработчики кнопок
        if (keepButton != null)
            keepButton.onClick.AddListener(OnKeepFish);
            
        if (releaseButton != null)
            releaseButton.onClick.AddListener(OnReleaseFish);
    }

    // ГЛАВНЫЙ МЕТОД: вызывайте его, когда поймали рыбу, передавая конфиг
    public void ShowCaughtFish(FishConfig fishConfig)
    {
        if (isUIActive) return;
        if (fishConfig == null)
        {
            Debug.LogError("FishCatchUI: конфиг рыбы не передан!");
            return;
        }

        currentFishConfig = fishConfig;

        // 1. Создаем рыбу из префаба из конфига
        if (fishConfig.fishPrefab != null && playerTransform != null)
        {
            if (currentFishInstance != null)
                Destroy(currentFishInstance);
                
            // Вычисляем позицию перед игроком
            Vector3 spawnPos = playerTransform.position + playerTransform.forward * spawnDistance + spawnOffset;
            Quaternion spawnRot = Quaternion.LookRotation(playerTransform.position - spawnPos);
            
            currentFishInstance = Instantiate(fishConfig.fishPrefab, spawnPos, spawnRot);
            
            if (fishDisplayRoot != null)
            {
                currentFishInstance.transform.SetParent(fishDisplayRoot.transform);
            }
        }
        else
        {
            Debug.LogWarning("FishCatchUI: префаб рыбы не назначен в конфиге!");
        }

        // 2. Обновляем текст из конфига
        if (fishInfoText != null)
        {
            fishInfoText.text = $"{fishConfig.fishName}\nВес: {fishConfig.fishWeight:F1} кг";
            fishInfoText.gameObject.SetActive(true);
        }

        // 3. Показываем кнопки
        if (buttonContainer != null)
            buttonContainer.SetActive(true);

        // 4. Показываем корневой объект
        if (fishDisplayRoot != null)
            fishDisplayRoot.SetActive(true);

        // 5. Затемняем фон
        if (backgroundDimImage != null)
        {
            backgroundDimImage.gameObject.SetActive(true);
            StartCoroutine(FadeBackground(0, dimColor.a, fadeDuration));
        }

        isUIActive = true;
    }

    // --- Обработчики кнопок ---
    private void OnKeepFish()
    {
        if (currentFishConfig != null)
        {
            Debug.Log($"Рыба '{currentFishConfig.fishName}' весом {currentFishConfig.fishWeight} кг отправлена в садок!");
            // TODO: Добавьте логику сохранения рыбы в инвентарь
        }
        HideCaughtFish();
    }

    private void OnReleaseFish()
    {
        if (currentFishConfig != null)
        {
            Debug.Log($"Рыба '{currentFishConfig.fishName}' отпущена обратно в воду.");
            // TODO: Добавьте анимацию отпускания
        }
        HideCaughtFish();
    }

    // Скрытие UI
    public void HideCaughtFish()
    {
        if (!isUIActive) return;

        if (backgroundDimImage != null)
        {
            StartCoroutine(FadeBackground(dimColor.a, 0, fadeDuration));
        }

        if (fishDisplayRoot != null)
            fishDisplayRoot.SetActive(false);
            
        if (fishInfoText != null)
            fishInfoText.gameObject.SetActive(false);

        if (buttonContainer != null)
            buttonContainer.SetActive(false);
            
        if (currentFishInstance != null)
        {
            Destroy(currentFishInstance);
            currentFishInstance = null;
        }

        currentFishConfig = null;
        isUIActive = false;
    }

    // Корутина для плавного затемнения
    private System.Collections.IEnumerator FadeBackground(float startAlpha, float targetAlpha, float duration)
    {
        if (backgroundDimImage == null) yield break;
        
        float elapsed = 0;
        Color color = backgroundDimImage.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            color.a = newAlpha;
            backgroundDimImage.color = color;
            yield return null;
        }
        
        color.a = targetAlpha;
        backgroundDimImage.color = color;
    }
}