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

    private GameObject currentFishModel;
    private FishConfig currentCaughtFish;
    private Coroutine rotationCoroutine;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("Камера не назначена в инспекторе и не найдена камера с тегом MainCamera!");
            }
        }

        this.gameObject.SetActive(false);
    }

    public void ShowCatchResult(FishConfig caughtFish)
    {
        currentCaughtFish = caughtFish;

        if (caughtFish != null)
        {
            // ===== ИЗМЕНЕНИЯ ЗДЕСЬ =====
            // Выводим название и вес в одном текстовом поле
            if (nameText != null)
            {
                // Форматируем вывод с названием и весом
                nameText.text = $"{caughtFish.fishName}\n{caughtFish.fishWeight:F2} кг.";
            }

            // Второе текстовое поле можно скрыть
            if (weightText != null)
            {
                weightText.text = ""; // Очищаем или можно скрыть: weightText.gameObject.SetActive(false);
            }
            // ===== КОНЕЦ ИЗМЕНЕНИЙ =====
        }
        else
        {
            Debug.LogError("FishCatchUI: Передан пустой конфиг рыбы!");
            return;
        }

        this.gameObject.SetActive(true);
        SpawnFishModel(caughtFish.fishPrefab);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.gameObject.SetActive(false);
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