using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset _playerInput;

    [Header("Action Map Name")] 
    [SerializeField] private string ACTION_MAP_NAME = "Player";

    [Header("Action Name Reference")]
    [SerializeField] private string MOVE = "Movement";
    [SerializeField] private string LOOK = "Look";
    [SerializeField] private string JUMP = "Jump";
    [SerializeField] private string SPRINT = "Sprint";
    [SerializeField] private string CROUCH = "Crouch";
    [SerializeField] private string SHOOT = "Shoot";
    [SerializeField] private string GRAPPLE = "Grapple";
    [SerializeField] private string SCAN = "Scan";
    [SerializeField] private string CONSOLE = "Console";
    [SerializeField] private string NOCLIP = "Noclip";
    [SerializeField] private string INTERACT = "Interact";
    [SerializeField] private string OVERLAY = "Overlay";
    [SerializeField] private string VERTICALMOVE = "VerticalMovement";

    public InputAction _moveAction;
    public InputAction _lookAction;
    public InputAction _jumpAction;
    public InputAction _sprintAction;
    public InputAction _crouchAction;
    public InputAction _shootAction;
    public InputAction _grappleAction;
    public InputAction _scanAction;
    public InputAction _consoleAction;
    public InputAction _noclipAction;
    public InputAction _interactAction;
    public InputAction _overlayAction;
    public InputAction _verticalMoveAction;

    public Vector2 MoveVector { get; private set; }
    public Vector2 LookVector { get; private set; }
    public bool Jump { get; private set; }
    public bool Sprint { get; private set; }
    public bool Crouch { get; private set; }
    public bool Shoot { get; private set; }
    public bool Grapple { get; private set; }
    public bool Scan { get; private set; }
    public bool Console { get; private set; }
    public bool Noclip { get; private set; }
    public bool Interact { get; private set; }
    public bool Overlay { get; private set; }
    public float VerticalMoveVector { get; private set; }

    public static InputHandler Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }

        var inputAction = _playerInput.FindActionMap(ACTION_MAP_NAME);

        _moveAction = inputAction.FindAction(MOVE);
        _lookAction = inputAction.FindAction(LOOK);
        _jumpAction = inputAction.FindAction(JUMP);
        _sprintAction = inputAction.FindAction(SPRINT);
        _crouchAction = inputAction.FindAction(CROUCH);
        _shootAction = inputAction.FindAction(SHOOT);
        _grappleAction = inputAction.FindAction(GRAPPLE);
        _scanAction = inputAction.FindAction(SCAN);
        _consoleAction = inputAction.FindAction(CONSOLE);
        _grappleAction = inputAction.FindAction(GRAPPLE);
        _noclipAction = inputAction.FindAction(NOCLIP);
        _interactAction = inputAction.FindAction(INTERACT);
        _overlayAction = inputAction.FindAction(OVERLAY);
        _verticalMoveAction = inputAction.FindAction(VERTICALMOVE);

        RegisterInputActions();
    }

    private void RegisterInputActions() {
        _moveAction.performed += context => MoveVector = context.ReadValue<Vector2>();
        _moveAction.canceled += context => MoveVector = Vector2.zero;

        _lookAction.performed += context => LookVector = context.ReadValue<Vector2>();
        _lookAction.canceled += context => LookVector = Vector2.zero;

        _sprintAction.performed += context => Sprint = true;
        _sprintAction.canceled += context => Sprint = false;

        _jumpAction.performed += context => Jump = true;
        _jumpAction.canceled += context => Jump = false;
        
        _crouchAction.performed += context => Crouch = true;
        _crouchAction.canceled += context => Crouch = false;
        
        _shootAction.performed += context => Shoot = true;
        _shootAction.canceled += context => Shoot = false;
        
        _grappleAction.performed += context => Grapple = true;
        _grappleAction.canceled += context => Grapple = false;

        _scanAction.performed += context => Scan = true;
        _scanAction.canceled += context => Scan = false;

        _consoleAction.performed += context => Console = true;
        _consoleAction.canceled += context => Console = false;

        _noclipAction.performed += context => Noclip = true;
        _noclipAction.canceled += context => Noclip = false;

        _interactAction.performed += context => Interact = true;
        _interactAction.canceled += context => Interact = false;

        _overlayAction.performed += context => Overlay = true;
        _overlayAction.canceled += context => Overlay = false;

        _verticalMoveAction.performed += context => VerticalMoveVector = context.ReadValue<float>();
        _verticalMoveAction.canceled += context => VerticalMoveVector = 0;
    }

    private void OnEnable() {
        _moveAction.Enable();
        _lookAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
        _crouchAction.Enable();
        _shootAction.Enable();
        _grappleAction.Enable();
        _scanAction.Enable();
        _consoleAction.Enable();
        _noclipAction.Enable();
        _interactAction.Enable();
        _overlayAction.Enable();
        _verticalMoveAction.Enable();
    }

    private void OnDisable() {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
        _sprintAction.Disable();
        _crouchAction.Disable();
        _shootAction.Disable();
        _grappleAction.Disable();
        _scanAction.Disable();
        _consoleAction.Disable();
        _noclipAction.Disable();
        _interactAction.Disable();
        _overlayAction.Disable();
        _verticalMoveAction.Disable();
    }
}
