using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class FishConfig
{
    public string fishName;
    public float fishWeight;
    public GameObject fishPrefab;
}

public class FishCatchUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;      // Основной текст (название + вес)
    [SerializeField] private TMP_Text weightText;    // Доп. текст (можно не использовать)
    [SerializeField] private Button keepButton;
    [SerializeField] private Button releaseButton;

    [Header("3D Model Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float spawnDistance = 3f;
    [SerializeField] private Vector3 modelOffset = new Vector3(0, -0.5f, 0);
    [SerializeField] private Vector3 modelRotation = new Vector3(0, 180, 0);
    [SerializeField] private Vector3 modelScale = Vector3.one * 1.5f;

    private GameObject currentFishModel;
    private FishConfig currentCaughtFish;
    private Coroutine rotationCoroutine;

    private void Awake()
    {
        // Находим камеру, если не назначена
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                Debug.LogError("Камера не назначена в инспекторе и не найдена камера с тегом MainCamera!");
        }

        // Панель изначально выключена
        this.gameObject.SetActive(false);
    }

    public void ShowCatchResult(FishConfig caughtFish)
    {
        if (caughtFish == null)
        {
            Debug.LogError("FishCatchUI: Передан пустой конфиг рыбы!");
            return;
        }

        currentCaughtFish = caughtFish;

        // ========== НАСТРОЙКА ТЕКСТА ==========
        // 1. Проверяем наличие nameText и weightText
        if (nameText != null)
        {
            // Включаем объект (на случай, если был выключен)
            nameText.gameObject.SetActive(true);

            // Устанавливаем текст с названием и весом
            nameText.text = $"{caughtFish.fishName}\n{caughtFish.fishWeight:F2} кг.";

            // Принудительно задаём видимые параметры (чтобы точно отображалось)
            nameText.color = Color.white;       // Белый цвет (или любой другой контрастный)
            nameText.fontSize = 36;              // Размер шрифта (подберите под свой UI)

            // Лог для проверки
            Debug.Log($"nameText.text = {nameText.text}");
        }
        else
        {
            Debug.LogError("nameText == null! Не назначен в инспекторе!");
        }

        if (weightText != null)
        {
            // Можно скрыть второе поле, если оно не нужно
            weightText.gameObject.SetActive(false);
            // Или очистить: weightText.text = "";
        }
        // =======================================

        // 2. Активируем панель (ОБЯЗАТЕЛЬНО до создания модели)
        this.gameObject.SetActive(true);

        // 3. Создаём 3D-модель рыбы
        SpawnFishModel(caughtFish.fishPrefab);

        // 4. Показываем курсор для кнопок
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"UI показан для рыбы: {caughtFish.fishName}");
    }

    private void SpawnFishModel(GameObject fishPrefab)
    {
        // Удаляем старую модель
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
            Debug.LogError("Камера игрока не назначена!");
            return;
        }

        // Вычисляем позицию перед камерой
        Vector3 spawnPosition = playerCamera.transform.position +
                                playerCamera.transform.forward * spawnDistance +
                                modelOffset;

        // Создаём модель
        currentFishModel = Instantiate(fishPrefab, spawnPosition, Quaternion.identity);
        currentFishModel.transform.LookAt(playerCamera.transform);
        currentFishModel.transform.Rotate(modelRotation);
        currentFishModel.transform.localScale = modelScale;

        // Запускаем вращение только если объект активен
        if (this.gameObject.activeInHierarchy)
        {
            rotationCoroutine = StartCoroutine(RotateFishModel());
        }
        else
        {
            // Если вдруг неактивен — активируем принудительно
            Debug.LogWarning("Объект неактивен при создании модели, активируем!");
            this.gameObject.SetActive(true);
            rotationCoroutine = StartCoroutine(RotateFishModel());
        }
    }

    private IEnumerator RotateFishModel()
    {
        float rotationSpeed = 20f;
        while (currentFishModel != null && this.gameObject.activeSelf)
        {
            currentFishModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // === КНОПКИ ===
    public void OnKeepButtonClick()
    {
        if (currentCaughtFish == null) return;
        Debug.Log($"Рыба '{currentCaughtFish.fishName}' помещена в садок!");
        ClosePanel();
    }

    public void OnReleaseButtonClick()
    {
        if (currentCaughtFish == null) return;
        Debug.Log($"Рыба '{currentCaughtFish.fishName}' отпущена!");
        ClosePanel();
    }

    private void ClosePanel()
    {
        // Останавливаем вращение
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }

        // Удаляем модель
        if (currentFishModel != null)
        {
            Destroy(currentFishModel);
            currentFishModel = null;
        }

        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Выключаем панель
        this.gameObject.SetActive(false);

        Debug.Log("Панель закрыта");
    }

    private void OnDisable()
    {
        // Очистка при выключении
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
        if (currentFishModel != null)
        {
            Destroy(currentFishModel);
            currentFishModel = null;
        }
    }
}