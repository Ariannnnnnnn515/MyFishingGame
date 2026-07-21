using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private InputActionReference toggle;
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
            gameObject.SetActive(!gameObject.activeSelf);
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

    }
}
