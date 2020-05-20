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
        controls.PlayerControls.Camera.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        controls.PlayerControls.Camera.canceled += ctx => mouseInput = Vector2.zero;
        controls.PlayerControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.PlayerControls.Move.canceled += ctx => moveInput = Vector2.zero;
    }
    void Start()
    {
        Current = new PlayerInput();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Retrieve our current WASD or Arrow Key input
        // Using GetAxisRaw removes any kind of gravity or filtering being applied to the input
        // Ensuring that we are getting either -1, 0 or 1
        moveInput = Vector2.ClampMagnitude(moveInput, 1.0f);


        Current = new PlayerInput()
        {

            MoveInput = moveInput,
            MouseInput = mouseInput,

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

}

