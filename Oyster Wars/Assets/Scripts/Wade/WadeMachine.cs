using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;



public enum CharacterState
{
    Idle, Walk, Jump, Crouch, LedgeGrab, WallSlide, AirAction, Drive, LockOn, Dash
}

//[RequireComponent(typeof(WadeInputs))]  
public partial class WadeMachine : MonoBehaviour, ICharacterController
{



    [Header("Active Wade Stats")]
    public float moveSpeed;
    public Vector3 wadeVelocity;
    public float verticalMoveSpeed;
    public bool canDrive;



    [Header("Walking")]
    public float correctedWalkSpeed;
    public float walkSpeed;
    public float walkAccel;
    public float walkDecel;
    public float turnSpeedFast;
    public float turnSpeedSlow;
    public float turnSpeed;
    public float walkSpeedsTransitionTime;


    [Header("Jumping")]
    public Vector3 planarMoveDirection;
    public float fallGravity;
    public float gravitySwitchRate = 8;
    public float gravSwitchStart;
    public float Gravity;
    public float maxGravity;
    public float lockOnAngle;
    public float minJumpHeight;
    float maxJumpVelocity;
    float minJumpVelocity;
    public float maxVM;
    public float minVM;



    [Header("AirActions")]
    public float slashInteruptionTimer;
    public float slamTimer;
    public float slamStartTime;
    public float slashTurnSpeed;

    [Header("Dash")]
    public float dashLength;
    public float dashSpeed;
    public float dashAccel;
    public float dashTimer;
    public Vector3 cachedTurnDirection;
    public Vector3 cachedDirection;
    Vector3 targetDash;




    [Header("Shooting")]
    public int bullets;
    public int clipSize;
    public float boltActionTime;
    public float shotTimer;
    public float reloadSpeed;
    public float reloadWalkSpeed;
    public bool reloading;
    public bool canShoot;
    public bool triggerInUse;
    public float LockOnFollowTime;
    public float LockOnWalkSpeed;
    public bool lockedOn = false;


    [Header("WallSlide")]
    public float wallSlideSpeed;
    public float wallHitAngle;
    public float wallHitInputAngle;
    public float wallTimerStart;
    public float wallInputTimer;
    public bool comingFromWallSlide;

    [Header("CamStats")]
    public GameObject camera;
    public Transform cam;
    public Vector3 camF;
    public Vector3 camR;
    public Vector3 intent;
    public float followRate = 5f;
    public float verticalMovementMax = 5f;
    public float angle;



    float num = 1;

    public float linearDrag = 5;

    public float blendAngle;

    public CharacterState CurrentCharacterState { get; private set; }

    [Header("Draggables")]
    public WadeInputs input;
    public KinematicCharacterMotor Motor;
    public Animator PlayerAnimator;
    public GameObject PlayerMesh;
    public Transform lockOnTarget;
    public Transform gun;
    public Transform gunParentBone;
    public LedgeDetect ledge;
    public Transform camFollow;
    public GameObject bullet;
    public Vehicle wadeboat;
    public List<Collider> IgnoredColliders = new List<Collider>();
    public AudioSource audio;

    public string stateString;
    public float timeScale;





    private void Start()
    {
        canShoot = true;
        bullets = clipSize;
        Gravity = Mathf.Clamp(Gravity, -80, 0);
        // Assign to motor
        Motor.CharacterController = this;
        PlayerAnimator = PlayerMesh.GetComponent<Animator>();
        ledge = GetComponent<LedgeDetect>();
        input = GetComponent<WadeInputs>();
        TransitionToState(CharacterState.Idle);
        wadeboat = GameObject.Find("wadeboat").GetComponent<Vehicle>();
        audio = GetComponent<AudioSource>();
        //input = PlayerTarget.GetComponent<WadeInputs>();
        
    }

    /// <summary>
    /// Handles movement state transitions and enter/exit callbacks
    /// </summary>
    public void TransitionToState(CharacterState newState)
    {
        CharacterState tmpInitialState = CurrentCharacterState;
        OnStateExit(tmpInitialState, newState);
        CurrentCharacterState = newState;
        OnStateEnter(newState, tmpInitialState);
    }

    /// <summary>
    /// Event when entering a state
    /// </summary>
    public void OnStateEnter(CharacterState state, CharacterState fromState)
    {
        switch (state)
        {
            case CharacterState.Idle:
                {

                    PlayerAnimator.SetBool("Idle", true);
                    break;
                }
            case CharacterState.Walk:
                {
                    PlayerAnimator.SetBool("Walk", true);
                    break;
                }
            case CharacterState.Jump:
                {
                    PlayerAnimator.SetBool(currentJumpProfile.Animation, true);
                    Motor.ForceUnground();

                    if (!lockedOn && fromState != CharacterState.Dash)
                    {
                        cachedDirection = Motor.CharacterForward;
                    }
                    else
                    {
                        cachedDirection = LocalMovement();
                    }


                    planarMoveDirection = planarMoveDirection * currentJumpProfile.planarMultiplier;

                    if (currentJumpProfile == groundFall)
                    {
                        Gravity = -fallGravity;
                    }

                    if (currentJumpProfile.Animation != "Fall")
                    {
                        Gravity = -(2 * currentJumpProfile.JumpHeight) / Mathf.Pow(currentJumpProfile.timeToJumpApex, 2);
                        maxGravity = Gravity * currentJumpProfile.gravityMultiplier;
                        maxJumpVelocity = Mathf.Abs(Gravity) * currentJumpProfile.timeToJumpApex;
                        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);
                        verticalMoveSpeed = maxJumpVelocity;
                        moveSpeed += currentJumpProfile.InitialForwardVelocity;
                    }
                    break;

                }
            case CharacterState.LedgeGrab:
                {
                    PlayerAnimator.SetBool("LedgeGrab", true);
                    break;
                }
            case CharacterState.WallSlide:
                {
                    PlayerAnimator.SetBool("WallSlide", true);
                    wallInputTimer = wallTimerStart;
                    wallHitAngle = Vector3.Angle(Motor.CharacterForward, ledge.middleNorm);
                    break;
                }
            case CharacterState.AirAction:
                {
                    if (lockedOn)
                    {
                        if(LocalMovement().magnitude > .05f)
                        {
                            cachedTurnDirection = LocalMovement();
                        }
                    }
                    
                    PlayerAnimator.SetBool(currentAirProfile.Animation, true);

                    slashInteruptionTimer = 0;
                    
                    break;

                }
            case CharacterState.LockOn:
                {
                    PlayerAnimator.SetBool("LockOn", true);
                    break;
                }
            case CharacterState.Dash:
                {
                    if (lockedOn)
                    {
                        cachedDirection = LocalMovement();
                    }
                    else
                    {
                        cachedDirection = transform.forward;
                    }

                    moveSpeed += dashSpeed;
                    PlayerAnimator.SetBool("Dash", true);
                    break;
                }

        }
    }

    /// <summary>
    /// Event when exiting a state
    /// </summary>
    public void OnStateExit(CharacterState state, CharacterState toState)
    {
        switch (state)
        {
            case CharacterState.Idle:
                {

                    PlayerAnimator.SetBool("Idle", false);
                    break;
                }
            case CharacterState.Walk:
                {
                    PlayerAnimator.SetBool("Walk", false);

                    break;
                }
            case CharacterState.Jump:
                {
                    PlayerAnimator.SetBool(currentJumpProfile.Animation, false);

                    if (toState != CharacterState.AirAction)
                    {
                        verticalMoveSpeed = 0;
                    }

                    if (toState == CharacterState.LedgeGrab)
                    {
                        moveSpeed = 0;
                    }
                    break;
                }

            case CharacterState.LedgeGrab:
                {
                    PlayerAnimator.SetBool("LedgeGrab", false);
                    break;
                }
            case CharacterState.WallSlide:
                {
                    wallHitAngle = 0;
                    wallHitInputAngle = 0;
                    PlayerAnimator.SetBool("WallSlide", false);
                    break;
                }
            case CharacterState.AirAction:
                {
                    slamTimer = 0;
                    slashInteruptionTimer = 0;
                    if (toState == CharacterState.Idle || toState == CharacterState.Walk)
                    {
                        verticalMoveSpeed = 0;
                    }

                    PlayerAnimator.SetBool(currentAirProfile.Animation, false);
                    break;
                }
            case CharacterState.LockOn:
                {
                    PlayerAnimator.SetBool("LockOn", false);
                    break;

                }
            case CharacterState.Dash:
                {
                    PlayerAnimator.SetBool("Dash", false);
                    dashTimer = 0;
                    break;
                }
        }
    }



    /// <summary>
    /// This is called every frame by MyPlayer in order to tell the character what its inputs are
    /// </summary>
    ///



    public void Update()
    {
        blendAngle = Vector3.Angle(transform.forward, LocalMovement()) * input.Current.MoveInput.x;
        angle = Vector3.Angle(transform.forward, LocalMovement());
        stateString = CurrentCharacterState.ToString();

        Time.timeScale = timeScale;
        PlayerAnimator.SetFloat("Speed", input.Current.MoveInput.magnitude);
        wadeVelocity = Motor.Velocity;
        Shooting();
        PlayerAnimator.SetFloat("LockOnAngle", lockOnAngle);
        FollowBlock();

        //Debug.DrawRay(transform.position, LocalMovement() + Vector3.up, Color.blue);
        verticalMoveSpeed = Mathf.Clamp(verticalMoveSpeed, minVM, maxVM);
        switch (CurrentCharacterState)
        {
            case CharacterState.Idle:
                {
                    if (Input.GetButtonDown("B"))
                    {
                        TransitionToState(CharacterState.Dash);
                    }
                    ShootControls();

                    if (canDrive)
                    {
                        if (Input.GetButtonDown("Action"))
                        {
                            wadeboat.controllable = true;
                            TransitionToState(CharacterState.Drive);
                        }
                    }

                    if(Input.GetButtonDown("LeftBumper"))
                    {
                        if(lockedOn == true)
                        {
                            PlayerAnimator.SetBool("LockOn", false);
                            lockedOn = false;
                        }
                        else
                        {
                            PlayerAnimator.SetBool("LockOn", true);
                            lockedOn = true;
                        }
                    }
                    

                    if (!reloading)
                    {
                        if (Input.GetButtonDown("Jump"))
                        {
                            currentJumpProfile = jumpStandard;
                            TransitionToState(CharacterState.Jump);
                        }
                    }

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        currentJumpProfile = groundFall;
                        TransitionToState(CharacterState.Jump);
                    }

                    break;

                }
            case CharacterState.Walk:
                {
                    if (Input.GetButtonDown("B"))
                    {
                        TransitionToState(CharacterState.Dash);
                    }

                    ShootControls();

                    if (canDrive)
                    {
                        if (Input.GetButtonDown("Action"))
                        {
                            wadeboat.controllable = true;
                            TransitionToState(CharacterState.Drive);
                        }
                    }

                    if (Input.GetButtonDown("LeftBumper"))
                    {
                        if (lockedOn == true)
                        {
                            PlayerAnimator.SetBool("LockOn", false);
                            lockedOn = false;
                        }
                        else
                        {
                            PlayerAnimator.SetBool("LockOn", true);
                            lockedOn = true;
                        }
                    }

                    PlayerAnimator.SetFloat("Angle", blendAngle);

                    if (!reloading)
                    {
                        if (Input.GetButtonDown("Jump"))
                        {
                            currentJumpProfile = jumpStandard;
                            TransitionToState(CharacterState.Jump);
                        }
                    }


                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        currentJumpProfile = groundFall;
                        TransitionToState(CharacterState.Jump);
                    }

                    break;
                }
            case CharacterState.Jump:
                {
                    if (Input.GetButtonDown("LeftBumper"))
                    {
                        if (lockedOn == true)
                        {
                            lockedOn = false;
                        }
                        else
                        {
                            lockedOn = true;
                        }
                    }

                    if (Input.GetButtonDown("B"))
                    {
                        TransitionToState(CharacterState.Dash);
                    }

                    if (input.Current.SlashInput)
                    {
                        currentAirProfile = slashProfile;
                        TransitionToState(CharacterState.AirAction);
                    }

                    if (ledge.canGrab && verticalMoveSpeed <= 2)
                    {
                        TransitionToState(CharacterState.LedgeGrab);
                    }

                    else if (ledge.middlehNorm.y < .1f && verticalMoveSpeed <= 0 && ledge.middlehHit)
                    {
                        wallHitInputAngle = Vector3.Angle(LocalMovement(), ledge.middlehNorm);
                        wallHitAngle = Vector3.Angle(Motor.CharacterForward, ledge.middlehNorm);

                        if (wallHitInputAngle > 150)
                        {
                            TransitionToState(CharacterState.WallSlide);
                        }
                        if (input.Current.JumpInput)
                        {
                            currentJumpProfile = jumpWall;
                            TransitionToState(CharacterState.WallSlide);
                        }
                    }

                    break;
                }

            case CharacterState.LedgeGrab:
                {
                    if (Input.GetButtonDown("Jump"))
                    {
                        currentJumpProfile = jumpStandard;
                        TransitionToState(CharacterState.Jump);
                    }

                    break;

                }
            case CharacterState.WallSlide:
                {
                    if (ledge.canGrab)
                    {
                        TransitionToState(CharacterState.LedgeGrab);
                    }

                    if (input.Current.JumpInput)
                    {
                        currentJumpProfile = jumpWall;
                        TransitionToState(CharacterState.Jump);
                    }

                    if (wallHitInputAngle < 130)
                    {
                        wallInputTimer -= 10 * Time.deltaTime;

                        if (input.Current.JumpInput)
                        {
                            currentJumpProfile = jumpWall;
                            TransitionToState(CharacterState.Jump);
                        }

                        if (wallInputTimer <= 0)
                        {
                            currentJumpProfile = airFall;
                            TransitionToState(CharacterState.Jump);
                        }
                    }

                    break;

                }
            case CharacterState.AirAction:
                {
                    if (currentAirProfile == slashProfile)
                    {
                        slashInteruptionTimer += 1 * Time.deltaTime;
                        slamTimer += 1 * Time.deltaTime;

                        //if (Input.GetButtonUp("Slash"))
                        //{
                        //    slamTimer -= 4000f;
                        //}

                        //{
                        //    currentJumpProfile = sparkSlam;
                        //    TransitionToState(CharacterState.Jump);
                        //}

                        if (Input.GetAxisRaw("RightTrigger") != 0 && Input.GetButton("Slash"))
                        {
                            if (triggerInUse == false)
                            {
                                if (canShoot)
                                {
                                    Instantiate(bullet, gun.position, gun.rotation);
                                    bullets -= 1;
                                    shotTimer = 0;
                                    currentJumpProfile = sparkSlam;
                                    TransitionToState(CharacterState.Jump);
                                    audio.Play();
                                    triggerInUse = true;
                                }
                            }
                        }
                        if (Input.GetAxisRaw("RightTrigger") != 0)
                        {
                            if (canShoot)
                            {
                                Instantiate(bullet, gun.position, gun.rotation);
                                bullets -= 1;
                                shotTimer = 0;
                                currentJumpProfile = frontFlip;
                                TransitionToState(CharacterState.Jump);
                                audio.Play();
                                triggerInUse = true;
                            }
                        }
                        if (Input.GetAxisRaw("RightTrigger") == 0)
                        {
                            triggerInUse = false;
                        }
                    }

                    break;

                }
            case CharacterState.Drive:
                {
                    if (Input.GetButtonDown("Action"))
                    {
                        TransitionToState(CharacterState.Idle);
                        wadeboat.controllable = false;
                    }

                    break;

                }
            case CharacterState.LockOn:
                {
                    ShootControls();

                    if (Input.GetButtonDown("B"))
                    {
                        cachedDirection = LocalMovement();
                        TransitionToState(CharacterState.Dash);
                    }

                    if (Input.GetButtonDown("Jump"))
                    {
                        cachedDirection = LocalMovement();
                        currentJumpProfile = jumpStandard;
                        TransitionToState(CharacterState.Jump);
                    }

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        cachedDirection = LocalMovement();
                        currentJumpProfile = groundFall;
                        TransitionToState(CharacterState.Jump);
                    }

                    if (Input.GetButtonDown("LeftBumper"))
                    {
                        TransitionToState(CharacterState.Idle);
                    }

                    break;

                }
            case CharacterState.Dash:
                {
                    if(dashTimer > dashLength)
                    {
                        if (input.Current.SlashInput)
                        {
                            currentAirProfile = slashProfile;
                            TransitionToState(CharacterState.AirAction);
                        }
                    }
                    if(moveSpeed <= walkSpeed + 0.1)
                    {
                        if (Motor.GroundingStatus.FoundAnyGround)
                        {
                            TransitionToState(CharacterState.Idle);
                        }
                        else
                        {
                            currentJumpProfile = groundFall;
                            TransitionToState(CharacterState.Jump);
                        }
                        
                    }
                    break;

                }

        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>

    public void OnTriggerEnter(Collider other)
    {
       
            canDrive = true;
       
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == ("DriveTrigger"))
        {
            canDrive = false;
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now.
    /// This is the ONLY place where you should set the character's rotation
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        switch (CurrentCharacterState)
        {
            case CharacterState.Idle:
                {
                    if (lockedOn)
                    {
                        Vector3 lockOnRotation = (lockOnTarget.position - transform.position);
                        Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                        lookRotation.x = 0;
                        lookRotation.z = 0;
                        currentRotation = Quaternion.Slerp(currentRotation, lookRotation, LockOnFollowTime * Time.deltaTime);
                    }
                    //currentRotation = currentRotation;
                    break;
                }
            case CharacterState.Walk:
                {
                    if (input.Current.MoveInput.magnitude > .1f && !lockedOn)
                    {
                        turnSpeed = Mathf.Lerp(turnSpeedSlow, turnSpeedFast, moveSpeed);
                        Quaternion desiredAngle = Quaternion.LookRotation(LocalMovement());
                        currentRotation = Quaternion.Lerp(currentRotation, desiredAngle, turnSpeed * Time.deltaTime);
                    }
                    else
                    {
                        Vector3 lockOnRotation = (lockOnTarget.position - transform.position);
                        Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                        lookRotation.x = 0;
                        lookRotation.z = 0;
                        currentRotation = Quaternion.Slerp(currentRotation, lookRotation, LockOnFollowTime * Time.deltaTime);
                    }

                    break;
                }
            case CharacterState.Jump:
                {
                    Vector3 angles = transform.localRotation.eulerAngles;

                    currentRotation = Quaternion.Euler(angles.x, Mathf.Abs(angles.y % 360), angles.z);

                    break;
                }
            case CharacterState.WallSlide:
                {
                    currentRotation = Quaternion.Euler(0, wallHitAngle + transform.localEulerAngles.y, 0);
                    break;
                }
            case CharacterState.Drive:
                {


                    break;

                }
            case CharacterState.LockOn:
                {
                    Vector3 lockOnRotation = (lockOnTarget.position - transform.position);
                    Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                    lookRotation.x = 0;
                    lookRotation.z = 0;
                    currentRotation = Quaternion.Slerp(currentRotation, lookRotation, LockOnFollowTime * Time.deltaTime);
                    break;
                }
            case CharacterState.AirAction:
                {
                    if (lockedOn)
                    {
                        if(LocalMovement().magnitude > 0.1f)
                        {
                            Quaternion desiredAngle = Quaternion.LookRotation(cachedTurnDirection);
                            currentRotation = Quaternion.Slerp(currentRotation, desiredAngle, slashTurnSpeed);
                        }
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now.
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    ///

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (CurrentCharacterState)
        {
            case CharacterState.Idle:
                {
                    if (input.Current.MoveInput != Vector3.zero)
                    {
                        TransitionToState(CharacterState.Walk);
                    }

                    moveSpeed = Mathf.Lerp(moveSpeed, 0, walkDecel * Time.deltaTime);
                    //moveSpeed = 0;
                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * moveSpeed, 10 * deltaTime);
                    break;
                }
            case CharacterState.Walk:
                {
                    Gravity = 0;
                    if (input.Current.MoveInput == Vector3.zero)
                    {
                        TransitionToState(CharacterState.Idle);
                    }

                    moveSpeed = Mathf.Lerp(moveSpeed, correctedWalkSpeed, walkAccel * Time.deltaTime);


                    if (lockedOn)
                    {
                        currentVelocity = Vector3.Lerp(currentVelocity, LocalMovement() * moveSpeed, 10 * deltaTime);
                    }
                    else
                    {
                        currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * LocalMovement().magnitude * moveSpeed, 10 * deltaTime);
                    }                    
                    break;
                }
            case CharacterState.Jump:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        if (LocalMovement().magnitude > 0.02f)
                        {
                            TransitionToState(CharacterState.Walk);
                        }
                        else
                        {
                            TransitionToState(CharacterState.Idle);

                        }
                    }



                    if (currentJumpProfile.CanControlHeight)
                    {
                        if (currentJumpProfile == jumpStandard && Input.GetButtonUp("Jump") && verticalMoveSpeed > minJumpVelocity)
                        {
                            verticalMoveSpeed = minJumpVelocity;
                        }
                    }

                    float jumpAccel;
                    float jumpAirSpeed;

                    if (input.Current.MoveInput != Vector3.zero)
                    {
                        if ((angle) < 50 && moveSpeed > 4)
                        {
                            jumpAirSpeed = 0.5f;
                            jumpAccel = currentJumpProfile.Acceleration;
                        }
                        else if ((angle) > 100 && currentJumpProfile == frontFlip)
                        {
                            jumpAirSpeed = 6;
                            jumpAccel = currentJumpProfile.Acceleration;
                            moveSpeed = Mathf.Lerp(moveSpeed, 0, ((2 - (180f / angle)) / 3f) * Time.deltaTime);
                        }
                        else
                        {
                            jumpAirSpeed = currentJumpProfile.AirSpeed;
                            jumpAccel = currentJumpProfile.Acceleration;
                        }
                        planarMoveDirection = Vector3.MoveTowards(planarMoveDirection, LocalMovement() * jumpAirSpeed, jumpAccel * Time.deltaTime);
                    }
                    else
                    {

                        moveSpeed = Mathf.Lerp(moveSpeed, 1, linearDrag * Time.deltaTime);

                    }



                    if (verticalMoveSpeed <= gravSwitchStart && currentJumpProfile.Animation != "Fall")
                    {
                        Gravity = Mathf.Lerp(Gravity, maxGravity, gravitySwitchRate * Time.deltaTime);
                    }

                    verticalMoveSpeed += Gravity * Time.deltaTime;


                    if (ledge.middleHit)
                    {
                        moveSpeed = 0;
                    }

                    currentVelocity = cachedDirection * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection;



                    break;

                }
            case CharacterState.LedgeGrab:
                {
                    currentVelocity = Vector3.zero;
                    break;
                }
            case CharacterState.WallSlide:
                {
                    moveSpeed = 0;
                    verticalMoveSpeed = wallSlideSpeed;
                    currentVelocity = Vector3.down * verticalMoveSpeed;

                    wallHitInputAngle = Vector3.Angle(LocalMovement(), ledge.middleNorm);
                    wallHitAngle = Vector3.Angle(Motor.CharacterForward, ledge.middleNorm);

                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(CharacterState.Idle);
                    }
                    break;
                }
            case CharacterState.AirAction:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        if (LocalMovement().magnitude > 0.02f)
                        {
                            TransitionToState(CharacterState.Walk);
                        }
                        else
                        {
                            TransitionToState(CharacterState.Idle);

                        }
                    }

                    float jumpAccel;
                    float jumpAirSpeed;
                    float angle = Vector3.Angle(transform.forward, LocalMovement());

                if(currentAirProfile == slashProfile)
                    {
                        if (input.Current.MoveInput != Vector3.zero)
                        {
                            if (Vector3.Angle(transform.forward, LocalMovement()) < 50 && moveSpeed > 4)
                            {
                                jumpAirSpeed = 0.5f;
                                jumpAccel = currentJumpProfile.Acceleration;
                            }
                            else
                            {
                                jumpAirSpeed = currentJumpProfile.AirSpeed;
                                jumpAccel = currentJumpProfile.Acceleration;
                            }
                            planarMoveDirection = Vector3.MoveTowards(planarMoveDirection, LocalMovement() * jumpAirSpeed, jumpAccel * Time.deltaTime);
                        }
                        else
                        {
                            moveSpeed = Mathf.MoveTowards(moveSpeed, 0, linearDrag * deltaTime);
                            planarMoveDirection = Vector3.MoveTowards(planarMoveDirection, Vector3.zero, 10 * Time.deltaTime);
                        }

                        if (verticalMoveSpeed <= 0 && currentJumpProfile.Animation != "Fall")
                        {
                            Gravity = Mathf.Lerp(Gravity, maxGravity, gravitySwitchRate * Time.deltaTime);
                        }

                        verticalMoveSpeed += Gravity * Time.deltaTime;

                        if (ledge.middleHit)
                        {
                            moveSpeed = 0;
                        }

                        currentVelocity = cachedDirection * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection;
                    }
                    
                    break;

                }
            case CharacterState.Drive:
                {
                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * moveSpeed, 10 * deltaTime);
                    moveSpeed = Mathf.Lerp(moveSpeed, 0, walkDecel  * Time.deltaTime);
                    break;

                }
            case CharacterState.LockOn:
                {

                    moveSpeed = Mathf.Lerp(moveSpeed, correctedWalkSpeed, walkAccel * Time.deltaTime);
                    currentVelocity = Vector3.Lerp(currentVelocity, LocalMovement() * moveSpeed, 10 * deltaTime);
                    break;
                }
            case CharacterState.Dash:
                {
                    dashTimer += 1 * Time.deltaTime;
                    if(dashTimer > dashLength)
                    {
                        moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, dashAccel * deltaTime);
                    }

                    
                    currentVelocity = cachedDirection * moveSpeed;
                    break;
                }
        }

    }



    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        if(IgnoredColliders.Count == 0)
        {
            return true;
        }

        if (IgnoredColliders.Contains(coll))
        {
            return false;
        }

        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void AddVelocity(Vector3 velocity)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

}
