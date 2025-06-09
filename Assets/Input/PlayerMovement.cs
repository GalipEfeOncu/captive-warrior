using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private InputSystem playerInputActions;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float fallMultiplier = 5f;

    [Header("Camera & Look Settings")] 
    [SerializeField] Transform cameraTransform;

    private bool isRunning = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        playerInputActions = new InputSystem();
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += OnJump;
        playerInputActions.Player.Run.started += ctx => isRunning = true;
        playerInputActions.Player.Run.canceled += ctx => isRunning = false;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
        playerInputActions.Player.Jump.performed -= OnJump;
        playerInputActions.Player.Run.started -= ctx => isRunning = true;
        playerInputActions.Player.Run.canceled -= ctx => isRunning = false;
    }

    private void FixedUpdate()
    {
        float currentSpeed = isRunning ? moveSpeed * 1.5f : moveSpeed;

        // Hareket inputunu al (WASD veya sol analog)
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        // Kameranin forward ve right vektorlerini al
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Y eksenini sifirla, sadece yatay duzlemde hareket etsin
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Inputa gore kameraya bagli hareket yonu hesapla
        Vector3 moveDirection = camForward * inputVector.y + camRight * inputVector.x;

        // Hareket kuvvetini uygula
        rb.AddForce(moveDirection * currentSpeed, ForceMode.VelocityChange);

        // Eger karakter dusuyorsa ekstra yercekimi uygula
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Vector3.up * (Physics.gravity.y * (fallMultiplier - 1)), ForceMode.Acceleration);
        }
    }

    private void LateUpdate()
    {
        // Kameranin ileri yonunu al
        Vector3 camForward = cameraTransform.forward;

        // Yukari-asagi bileeni sifirla, sadece yatay dondurme yap
        camForward.y = 0f;
        camForward.Normalize();

        // Eğer kamera bir yere bakiyorsa karakteri kameranin baktigi yone dogru dondur
        if (camForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            transform.rotation = targetRotation;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            //rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
