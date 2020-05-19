using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WadeInputs : MonoBehaviour
{
    public Controls controls;
    public PlayerInput Current;

    // Use this for initialization
    Vector2 mouseInput;
    Vector2 moveInput;
    private void Awake()
    {
        controls = new Controls();
    }
    void Start()
    {

        Current = new PlayerInput();
    }

    // Update is called once per frame
    void Update()
    {
        // Retrieve our current WASD or Arrow Key input
        // Using GetAxisRaw removes any kind of gravity or filtering being applied to the input
        // Ensuring that we are getting either -1, 0 or 1
        moveInput = Vector2.ClampMagnitude(moveInput, 1.0f);


        
        
        controls.PlayerControls.Camera.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        controls.PlayerControls.Camera.canceled += ctx => mouseInput = Vector2.zero;
        controls.PlayerControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.PlayerControls.Move.canceled += ctx => moveInput = Vector2.zero;



        bool jumpInput = controls.PlayerControls.X.triggered;
        bool lockOnInput = controls.PlayerControls.LeftBumper.triggered;
        bool triangleInput = controls.PlayerControls.Triangle.triggered;
        bool circleInput = controls.PlayerControls.Circle.triggered;
        bool leftBumper = controls.PlayerControls.LeftBumper.triggered;
        bool shootInput = controls.PlayerControls.RightTrigger.triggered;
        bool slashInput = controls.PlayerControls.Square.triggered;
        bool rightBumperInput = controls.PlayerControls.RightBumper.triggered;
        bool rightTriggerInput = controls.PlayerControls.RightTrigger.triggered;
        bool leftBumperInput = controls.PlayerControls.LeftBumper.triggered;
        bool leftTriggerInput = controls.PlayerControls.LeftTrigger.triggered;


        Current = new PlayerInput()
        {
            MoveInput = moveInput,
            MouseInput = mouseInput,
            JumpInput = jumpInput,
            LockOnInput = lockOnInput,
            ShootInput = shootInput,
            SlashInput = slashInput,
            TriangleInput = triangleInput,
            CircleInput = circleInput,
            RightBumperInput = rightBumperInput,
            RightTriggerInput = rightTriggerInput,
            LeftBumperInput = leftBumperInput,
            LeftTriggerInput = leftTriggerInput,
        };
    }

    void OnEnable()
    {
        controls.PlayerControls.Enable();
    }

    void OnDisable()
    {
        controls.PlayerControls.Disable();
    }

}

public struct PlayerInput
{
    public Vector2 MoveInput;
    public Vector2 MouseInput;
    public bool JumpInput;
    //public bool CrouchInput;
    public bool LockOnInput;
    public bool ShootInput;
    public bool SlashInput;
    public bool CircleInput;
    public bool LeftBumperInput;
    public bool LeftTriggerInput;
    public bool RightBumperInput;
    public bool RightTriggerInput;
    public bool TriangleInput;
    //public bool CrouchInputDown;
}

