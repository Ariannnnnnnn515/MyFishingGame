using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private InputActionReference toggle;
    [SerializeField] private GameObject root;
    [SerializeField] private FishCatchUI fishCatchUI; // Ссылка на UI рыбы

    private bool isPauseActive = false;

    public bool IsPauseActive()
    {
        return isPauseActive;
    }

    private void OnEnable()
    {
        toggle.action.started += OnToggle;
        toggle.action.performed += OnToggle;
        toggle.action.Enable();
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Проверяем, не открыт ли UI рыбы
            if (fishCatchUI != null && fishCatchUI.gameObject.activeSelf)
            {
                Debug.Log("Нельзя открыть меню паузы, пока открыт UI рыбы!");
                return;
            }

            if (root.activeSelf)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }

    private void OnDisable()
    {
        toggle.action.Disable();
        toggle.action.started -= OnToggle;
        toggle.action.performed -= OnToggle;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPauseActive = false;
        SceneManager.LoadScene(Scenes.MainMenu);
    }

    public void BackToGame()
    {
        Close();
    }

    private void Open()
    {
        root.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        isPauseActive = true;
    }

    private void Close()
    {
        root.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        isPauseActive = false;
    }
}