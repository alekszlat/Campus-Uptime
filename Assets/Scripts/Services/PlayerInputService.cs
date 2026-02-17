using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputService : MonoBehaviour
{
    private PlayerInput playerInput;
    private Vector2 moveDir;

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
    }
    private void OnDisable()
    {
        playerInput.Default.Disable();
        playerInput.Default.Movement.started -= OnTestButton;
        playerInput.Default.Movement.performed -= GetMovement;

    }
    public void GetMovement(InputAction.CallbackContext callback)
    {
        moveDir = callback.ReadValue<Vector2>();
    }

    public Vector2 GetDirection()
    {
        return moveDir;
    }
   
    void Update()
    {
       
    }

    public void OnTestButton(InputAction.CallbackContext context)
    {
        Debug.Log("A");
    }
}
