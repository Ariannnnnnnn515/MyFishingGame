using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private InputActionReference toggle;
    [SerializeField] private GameObject root; 
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
        Time.timeScale = 0;
    }

    private void Close()
    {
        root.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;

    }
}
