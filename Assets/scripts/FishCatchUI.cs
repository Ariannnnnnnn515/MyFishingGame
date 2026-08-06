using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class FishConfig
{
    public string fishName;
    public float fishWeight;
    public GameObject fishPrefab;  // 3D-модель рыбы
}

public class FishCatchUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private Button keepButton;
    [SerializeField] private Button releaseButton;

    [Header("3D Model Settings")]
    [SerializeField] private float spawnDistance = 3f;  // Расстояние от камеры
    [SerializeField] private Vector3 modelOffset = new Vector3(0, -0.5f, 0); // Смещение модели
    [SerializeField] private Vector3 modelRotation = new Vector3(0, 180, 0); // Поворот модели (чтобы смотрела на игрока)
    [SerializeField] private Vector3 modelScale = Vector3.one * 1.5f;

    private GameObject currentFishModel;  // Ссылка на созданную модель
    private FishConfig currentCaughtFish; // Данные о пойманной рыбе
    private Camera playerCamera;          // Главная камера игрока

    private void Awake()
    {
        // Находим главную камеру
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("Не найдена камера с тегом MainCamera!");
        }

        // Изначально панель выключена
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Показывает результат поимки рыбы
    /// </summary>
    public void ShowCatchResult(FishConfig caughtFish)
    {
        currentCaughtFish = caughtFish;

        if (caughtFish != null)
        {
            // Заполняем текстовую информацию
            if (nameText != null)
                nameText.text = caughtFish.fishName;
            if (weightText != null)
                weightText.text = $"Вес: {caughtFish.fishWeight:F2} кг.";
        }
        else
        {
            Debug.LogError("FishCatchUI: Передан пустой конфиг рыбы!");
            return;
        }

        // Создаём 3D-модель перед камерой
        SpawnFishModel(caughtFish.fishPrefab);

        // Активируем курсор для взаимодействия с кнопками
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Показываем панель
        this.gameObject.SetActive(true);

        // Опционально: замедляем или останавливаем время
        // Time.timeScale = 0f;
    }

    /// <summary>
    /// Создаёт 3D-модель рыбы перед камерой игрока
    /// </summary>
    private void SpawnFishModel(GameObject fishPrefab)
    {
        // Удаляем старую модель, если она есть
        if (currentFishModel != null)
        {
            Destroy(currentFishModel);
            currentFishModel = null;
        }

        if (fishPrefab == null)
        {
            Debug.LogWarning("Префаб рыбы не назначен в конфиге!");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("Камера игрока не найдена!");
            return;
        }

        // Вычисляем позицию перед камерой
        Vector3 spawnPosition = playerCamera.transform.position + 
                                playerCamera.transform.forward * spawnDistance + 
                                modelOffset;

        // Создаём модель
        currentFishModel = Instantiate(fishPrefab, spawnPosition, Quaternion.identity);

        // Поворачиваем модель к камере (чтобы игрок видел её спереди)
        currentFishModel.transform.LookAt(playerCamera.transform);
        currentFishModel.transform.Rotate(modelRotation);

        // Устанавливаем масштаб
        currentFishModel.transform.localScale = modelScale;

        // Добавляем эффект вращения для красоты
        StartCoroutine(RotateFishModel());
    }

    /// <summary>
    /// Корутина для плавного вращения модели
    /// </summary>
    private IEnumerator RotateFishModel()
    {
        float rotationSpeed = 20f;
        while (currentFishModel != null && this.gameObject.activeSelf)
        {
            currentFishModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// Обработчик кнопки "В садок"
    /// </summary>
    public void OnKeepButtonClick()
    {
        if (currentCaughtFish == null) return;

        Debug.Log($"Рыба '{currentCaughtFish.fishName}' помещена в садок!");
        // TODO: Здесь будет ваша логика сохранения рыбы

        ClosePanel();
    }

    /// <summary>
    /// Обработчик кнопки "Отпустить"
    /// </summary>
    public void OnReleaseButtonClick()
    {
        if (currentCaughtFish == null) return;

        Debug.Log($"Рыба '{currentCaughtFish.fishName}' отпущена на волю!");
        // TODO: Здесь может быть анимация уплывания

        ClosePanel();
    }

    /// <summary>
    /// Закрывает панель и очищает ресурсы
    /// </summary>
    private void ClosePanel()
    {
        // Удаляем модель
        if (currentFishModel != null)
        {
            Destroy(currentFishModel);
            currentFishModel = null;
        }

        // Возвращаем курсор в обычный режим
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Возвращаем время, если останавливали
        // Time.timeScale = 1f;

        // Выключаем панель
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// При выключении панели очищаем модель
    /// </summary>
    private void OnDisable()
    {
        if (currentFishModel != null)
        {
            Destroy(currentFishModel);
            currentFishModel = null;
        }
    }
}