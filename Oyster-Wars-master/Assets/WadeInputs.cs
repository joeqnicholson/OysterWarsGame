using UnityEngine;
using System.Collections;

public class WadeInputs : MonoBehaviour
{

    public PlayerInput Current;

    // Use this for initialization
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
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 sparkMoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

        moveInput = Vector3.ClampMagnitude(moveInput, 1.0f);

        Vector2 mouseInput = new Vector2(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));

        bool jumpInput = Input.GetButtonDown("Jump");
        bool crouchInput = Input.GetButton("Crouch");
        bool crouchInputDown = Input.GetButtonDown("Crouch");
        bool lockOnInput = Input.GetButtonDown("LockOn");
        bool shootInput = Input.GetButtonDown("Shoot");
        bool slashInput = Input.GetButtonDown("Slash");

        Current = new PlayerInput()
        {
            MoveInput = moveInput,
            SparkMoveInput = sparkMoveInput,
            MouseInput = mouseInput,
            JumpInput = jumpInput,
            CrouchInput = crouchInput,
            CrouchInputDown = crouchInputDown,
            LockOnInput = lockOnInput,
            ShootInput = shootInput,
            SlashInput = slashInput
        };
    }
}

public struct PlayerInput
{
    public Vector3 MoveInput;
    public Vector3 SparkMoveInput;
    public Vector2 MouseInput;
    public bool JumpInput;
    public bool CrouchInput;
    public bool LockOnInput;
    public bool ShootInput;
    public bool SlashInput;
    public bool CrouchInputDown;
}
