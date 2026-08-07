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
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private Button keepButton;
    [SerializeField] private Button releaseButton;

    [Header("3D Model Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float spawnDistance = 3f;
    [SerializeField] private Vector3 modelOffset = new Vector3(0, -0.5f, 0);
    [SerializeField] private Vector3 modelRotation = new Vector3(0, 180, 0);
    [SerializeField] private Vector3 modelScale = Vector3.one * 1.5f;

    // Ссылка на PauseUI (будет назначаться в инспекторе или находиться автоматически)
    private PauseUI pauseUI;

    private GameObject currentFishModel;
    private FishConfig currentCaughtFish;
    private Coroutine rotationCoroutine;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                Debug.LogError("Камера не назначена в инспекторе и не найдена камера с тегом MainCamera!");
        }

        // Находим PauseUI в сцене
        pauseUI = FindObjectOfType<PauseUI>();
        if (pauseUI == null)
            Debug.LogWarning("PauseUI не найден в сцене!");

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

        // Настройка текста
        if (nameText != null)
        {
            nameText.gameObject.SetActive(true);
            nameText.text = $"{caughtFish.fishName}\n{caughtFish.fishWeight:F2} кг.";
            nameText.color = Color.white;
            nameText.fontSize = 36;
            Debug.Log($"nameText.text = {nameText.text}");
        }
        else
        {
            Debug.LogError("nameText == null! Не назначен в инспекторе!");
        }

        if (weightText != null)
        {
            weightText.gameObject.SetActive(false);
        }

        // Активируем панель
        this.gameObject.SetActive(true);

        // Создаём 3D-модель рыбы
        SpawnFishModel(caughtFish.fishPrefab);

        // ВСЕГДА показываем курсор при поимке рыбы
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"UI показан для рыбы: {caughtFish.fishName}");
    }

    private void SpawnFishModel(GameObject fishPrefab)
    {
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

        Vector3 spawnPosition = playerCamera.transform.position +
                                playerCamera.transform.forward * spawnDistance +
                                modelOffset;

        currentFishModel = Instantiate(fishPrefab, spawnPosition, Quaternion.identity);
        currentFishModel.transform.LookAt(playerCamera.transform);
        currentFishModel.transform.Rotate(modelRotation);
        currentFishModel.transform.localScale = modelScale;

        if (this.gameObject.activeInHierarchy)
        {
            rotationCoroutine = StartCoroutine(RotateFishModel());
        }
        else
        {
            
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

        // Проверяем, открыто ли меню паузы
        bool isPauseOpen = pauseUI != null && pauseUI.IsPauseActive();

        if (isPauseOpen)
        {
            // Если меню паузы открыто - курсор должен быть виден
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Иначе - скрываем курсор для игры
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        this.gameObject.SetActive(false);
        Debug.Log("Панель закрыта");
    }

    private void OnDisable()
    {
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