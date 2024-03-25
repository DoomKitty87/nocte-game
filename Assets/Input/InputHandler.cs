using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset _playerInput;

    [Header("Action Map Name")] 
    [SerializeField] private string DefaultActionMap = "Player";
    private string _currentActionMap;

    [HideInInspector] public InputAction PLAYER_moveAction;
    [HideInInspector] public InputAction PLAYER_lookAction;
    [HideInInspector] public InputAction PLAYER_jumpAction;
    [HideInInspector] public InputAction PLAYER_sprintAction;
    [HideInInspector] public InputAction PLAYER_crouchAction;
    [HideInInspector] public InputAction PLAYER_shootAction;
    [HideInInspector] public InputAction PLAYER_grappleAction;
    [HideInInspector] public InputAction PLAYER_scanAction;
    [HideInInspector] public InputAction PLAYER_consoleAction;
    [HideInInspector] public InputAction PLAYER_noclipAction;
    [HideInInspector] public InputAction PLAYER_interactAction;
    [HideInInspector] public InputAction PLAYER_overlayAction;
    [HideInInspector] public InputAction PLAYER_verticalMoveAction;

    [HideInInspector] public InputAction DRIVING_moveAction;
    [HideInInspector] public InputAction DRIVING_leaveAction;
    [HideInInspector] public InputAction DRIVING_overlayAction;
    [HideInInspector] public InputAction DRIVING_consoleAction;
    [HideInInspector] public InputAction DRIVING_scanAction;
    [HideInInspector] public InputAction DRIVING_lookAction;

    public Vector2 PLAYER_MoveVector { get; private set; }
    public Vector2 PLAYER_LookVector { get; private set; }
    public bool PLAYER_Jump { get; private set; }
    public bool PLAYER_Sprint { get; private set; }
    public bool PLAYER_Crouch { get; private set; }
    public bool PLAYER_Shoot { get; private set; }
    public bool PLAYER_Grapple { get; private set; }
    public bool PLAYER_Scan { get; private set; }
    public bool PLAYER_Console { get; private set; }
    public bool PLAYER_Noclip { get; private set; }
    public bool PLAYER_Interact { get; private set; }
    public bool PLAYER_Overlay { get; private set; }
    public float PLAYER_VerticalMoveVector { get; private set; }

    public Vector2 DRIVING_MoveVector { get; private set; }
    public Vector2 Driving_LookVector { get; private set; }
    public bool DRIVING_Leave { get; private set; }
    public bool DRIVING_Overlay { get; private set; }
    public bool DRIVING_Console { get; private set; }
    public bool DRIVING_Scan { get; private set; }

    public Vector2 GENERAL_MoveVector { 
        get { return _currentActionMap == "Player" ? PLAYER_MoveVector : DRIVING_MoveVector; }
    }

    public Vector2 GENERAL_LookVector { 
        get { return _currentActionMap == "Player" ? PLAYER_LookVector : Driving_LookVector; }
    }

    public bool GENERAL_Overlay { 
        get { return _currentActionMap == "Player" ? PLAYER_Overlay : DRIVING_Overlay; }
    }

    public bool GENERAL_Console { 
        get { return _currentActionMap == "Player" ? PLAYER_Console : DRIVING_Console; }
    }

    public bool GENERAL_Scan { 
        get { return _currentActionMap == "Player" ? PLAYER_Scan : DRIVING_Scan; }
    }

    private static InputHandler instance;

    public static InputHandler Instance { get {return instance; } }

    private void Awake() {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }

        _currentActionMap = DefaultActionMap;

        var playerInputAction = _playerInput.FindActionMap(DefaultActionMap);

        PLAYER_moveAction = playerInputAction.FindAction("Movement");
        PLAYER_lookAction = playerInputAction.FindAction("Look");
        PLAYER_jumpAction = playerInputAction.FindAction("Jump");
        PLAYER_sprintAction = playerInputAction.FindAction("Sprint");
        PLAYER_crouchAction = playerInputAction.FindAction("Crouch");
        PLAYER_shootAction = playerInputAction.FindAction("Shoot");
        PLAYER_grappleAction = playerInputAction.FindAction("Grapple");
        PLAYER_scanAction = playerInputAction.FindAction("Scan");
        PLAYER_consoleAction = playerInputAction.FindAction("Console");
        PLAYER_noclipAction = playerInputAction.FindAction("Noclip");
        PLAYER_interactAction = playerInputAction.FindAction("Interact");
        PLAYER_overlayAction = playerInputAction.FindAction("Overlay");
        PLAYER_verticalMoveAction = playerInputAction.FindAction("VerticalMovement");

        var drivingInputAction = _playerInput.FindActionMap("Driving");

        DRIVING_moveAction = drivingInputAction.FindAction("Movement");
        DRIVING_lookAction = drivingInputAction.FindAction("Look");
        DRIVING_leaveAction = drivingInputAction.FindAction("Leave");
        DRIVING_overlayAction = drivingInputAction.FindAction("Overlay");
        DRIVING_consoleAction = drivingInputAction.FindAction("Console");
        DRIVING_scanAction = drivingInputAction.FindAction("Scan");

        RegisterInputActions();
    }

    private void RegisterInputActions() {
        PLAYER_moveAction.performed += context => PLAYER_MoveVector = context.ReadValue<Vector2>();
        PLAYER_moveAction.canceled += context => PLAYER_MoveVector = Vector2.zero;

        PLAYER_lookAction.performed += context => PLAYER_LookVector = context.ReadValue<Vector2>();
        PLAYER_lookAction.canceled += context => PLAYER_LookVector = Vector2.zero;

        PLAYER_sprintAction.performed += context => PLAYER_Sprint = true;
        PLAYER_sprintAction.canceled += context => PLAYER_Sprint = false;

        PLAYER_jumpAction.performed += context => PLAYER_Jump = true;
        PLAYER_jumpAction.canceled += context => PLAYER_Jump = false;
        
        PLAYER_crouchAction.performed += context => PLAYER_Crouch = true;
        PLAYER_crouchAction.canceled += context => PLAYER_Crouch = false;
        
        PLAYER_shootAction.performed += context => PLAYER_Shoot = true;
        PLAYER_shootAction.canceled += context => PLAYER_Shoot = false;
        
        PLAYER_grappleAction.performed += context => PLAYER_Grapple = true;
        PLAYER_grappleAction.canceled += context => PLAYER_Grapple = false;

        PLAYER_scanAction.performed += context => PLAYER_Scan = true;
        PLAYER_scanAction.canceled += context => PLAYER_Scan = false;

        PLAYER_consoleAction.performed += context => PLAYER_Console = true;
        PLAYER_consoleAction.canceled += context => PLAYER_Console = false;

        PLAYER_noclipAction.performed += context => PLAYER_Noclip = true;
        PLAYER_noclipAction.canceled += context => PLAYER_Noclip = false;

        PLAYER_interactAction.performed += context => PLAYER_Interact = true;
        PLAYER_interactAction.canceled += context => PLAYER_Interact = false;

        PLAYER_overlayAction.performed += context => PLAYER_Overlay = true;
        PLAYER_overlayAction.canceled += context => PLAYER_Overlay = false;

        PLAYER_verticalMoveAction.performed += context => PLAYER_VerticalMoveVector = context.ReadValue<float>();
        PLAYER_verticalMoveAction.canceled += context => PLAYER_VerticalMoveVector = 0;

        DRIVING_moveAction.performed += context => DRIVING_MoveVector = context.ReadValue<Vector2>();
        DRIVING_moveAction.canceled += context => DRIVING_MoveVector = Vector2.zero;

        DRIVING_lookAction.performed += context => Driving_LookVector = context.ReadValue<Vector2>();
        DRIVING_lookAction.canceled += context => Driving_LookVector = Vector2.zero;

        DRIVING_leaveAction.performed += context => DRIVING_Leave = true;
        DRIVING_leaveAction.canceled += context => DRIVING_Leave = false;

        DRIVING_overlayAction.performed += context => DRIVING_Overlay = true;
        DRIVING_overlayAction.canceled += context => DRIVING_Overlay = false;

        DRIVING_consoleAction.performed += context => DRIVING_Console = true;
        DRIVING_consoleAction.canceled += context => DRIVING_Console = false;

        DRIVING_scanAction.performed += context => DRIVING_Scan = true;
        DRIVING_scanAction.canceled += context => DRIVING_Scan = false;
    }

    private void OnEnable() {
        PLAYER_moveAction.Enable();
        PLAYER_lookAction.Enable();
        PLAYER_jumpAction.Enable();
        PLAYER_sprintAction.Enable();
        PLAYER_crouchAction.Enable();
        PLAYER_shootAction.Enable();
        PLAYER_grappleAction.Enable();
        PLAYER_scanAction.Enable();
        PLAYER_consoleAction.Enable();
        PLAYER_noclipAction.Enable();
        PLAYER_interactAction.Enable();
        PLAYER_overlayAction.Enable();
        PLAYER_verticalMoveAction.Enable();
        DRIVING_moveAction.Enable();
        DRIVING_lookAction.Enable();
        DRIVING_leaveAction.Enable();
        DRIVING_overlayAction.Enable();
        DRIVING_consoleAction.Enable();
        DRIVING_scanAction.Enable();
    }

    private void OnDisable() {
        PLAYER_moveAction.Disable();
        PLAYER_lookAction.Disable();
        PLAYER_jumpAction.Disable();
        PLAYER_sprintAction.Disable();
        PLAYER_crouchAction.Disable();
        PLAYER_shootAction.Disable();
        PLAYER_grappleAction.Disable();
        PLAYER_scanAction.Disable();
        PLAYER_consoleAction.Disable();
        PLAYER_noclipAction.Disable();
        PLAYER_interactAction.Disable();
        PLAYER_overlayAction.Disable();
        PLAYER_verticalMoveAction.Disable();
        DRIVING_moveAction.Disable();
        DRIVING_lookAction.Disable();
        DRIVING_leaveAction.Disable();
        DRIVING_overlayAction.Disable();
        DRIVING_consoleAction.Disable();
        DRIVING_scanAction.Disable();
    }



    public void SwitchActiveInputMap(string actionMapName) {
        _playerInput.FindActionMap(_currentActionMap).Disable();
        _playerInput.FindActionMap(actionMapName).Enable();
        _currentActionMap = actionMapName;
    }
}
