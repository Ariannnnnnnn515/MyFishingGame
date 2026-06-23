using UnityEngine;

public class PlayerMovement : MonoBehaviour  
{
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");

    [Header("Move")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float speed = 5;
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float gravityForce = 9.81f;

    [Header("Look")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float mouseSensivity = 2f;
    [SerializeField] private float lookLimit = 90f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float xRotation = 0;
    private float verticalVelocity = 0;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
        Look();
    }

    private void Move()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);
        direction = transform.rotation * direction;

        // Упрощено: убрать эффект пыли  
        bool isRunning = direction != Vector3.zero && characterController.isGrounded;

        if (characterController.isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                verticalVelocity = jumpForce;
            }
            else  
            {
                verticalVelocity = -1f;
            }
        }
        else  
        {
            verticalVelocity -= gravityForce * Time.deltaTime;
        }

        var velocity = direction * speed + verticalVelocity * Vector3.up;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookLimit, lookLimit);

        mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}