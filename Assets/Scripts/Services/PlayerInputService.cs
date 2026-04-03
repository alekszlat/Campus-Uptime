using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputService : MonoBehaviour
{
    private PlayerInput playerInput;
    private Vector2 moveDir;
    private bool isInteracting;
    void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Default.Enable();
    }
    void Start()
    {
        moveDir = Vector2.zero;
    }

    private void OnEnable()
    {
        playerInput.Default.Movement.performed += GetMovement;
        playerInput.Default.testButton.started += OnTestButton;
        playerInput.Default.Interact.started += SetupIsInteracting;
     
    }
    private void OnDisable()
    {
        playerInput.Default.Disable();
        playerInput.Default.Movement.started -= OnTestButton;
        playerInput.Default.Movement.performed -= GetMovement;
        playerInput.Default.Interact.started -= SetupIsInteracting;

    }
    public void GetMovement(InputAction.CallbackContext callback)
    {
        moveDir = callback.ReadValue<Vector2>();
    }
    private void SetupIsInteracting(InputAction.CallbackContext callback)
    {
        isInteracting = true; 
    }
    public Vector2 GetDirection()
    {
        return moveDir;
    }
    public bool GetIsInteracting()
    {
        return isInteracting;
    }

    void LateUpdate()
    {
        isInteracting = false;
    }
    void Update()
    {
       
    }

    public void OnTestButton(InputAction.CallbackContext context)
    {
        Debug.Log("A");
    }
}
