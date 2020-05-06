using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController;
using UnityEngine.SceneManagement;
using System;
using System.Data.SqlTypes;

public enum WadeState
{
    Idle, Walk, Jump, Crouch, LedgeGrab, WallSlide, AirAction, Drive, LockOn, Dash, Hit, Death
}

//[RequireComponent(typeof(WadeInputs))]  
public partial class WadeMachine : MonoBehaviour, ICharacterController
{
    public float gunTurnz;
    public float gunTurny;
    public float gunTurnx;

    [Header("Active Wade Stats")]
    public float moveSpeed;
    public Vector3 wadeVelocity;
    public float verticalMoveSpeed;
    public bool canDrive;
    public float health = 100;
    public float startHealth = 100;
    public int pearls = 0;
    public float stateTimer;

    [Header("Walking")]
    public float correctedWalkSpeed;
    public float walkSpeed;
    public float walkAccel;
    public float walkDecel;
    public float turnSpeedFast;
    public float turnSpeedSlow;
    public float turnSpeed;
    public float walkSpeedsTransitionTime;
    public float footStepFrequency;


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
    public bool hasShotInAir;
    public float airShotAnimationMatch;
    public Vector3 boatRelativeJumpMove;
    public bool takeBoatVelocity;



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
    public float boatDashDecel;
    public Vector3 cachedTurnDirection;
    public Vector3 cachedDirection;
    Vector3 targetDash;

    [Header("Hit")]
    public float recoveryTime;
    public float deathTime;
    public bool hasDied;



    [Header("Shooting")]
    public float bullets;
    public float clipSize;
    public float boltActionTime;
    public float shotTimer;
    public float reloadSpeed;
    public float reloadWalkSpeed;
    public bool reloading;
    public bool canShoot;
    public bool triggerInUse;
    public float LockOnFollowTime;
    public float LockOnWalkSpeed;
    public bool aiming;
    public bool lockedOn = false;
    public bool canLockOn = false;
    public float aimingWalkSpeed;

    [Header("Grenades")]
    public float throwForce;
    public float throwForceUp;
    public float throwForceDown;
    

    [Header("WallSlide")]
    public float wallSlideSpeed;
    public float wallHitAngle;
    public float wallHitInputAngle;
    public float wallTimerStart;
    public float wallInputTimer;
    public bool comingFromWallSlide;

    [Header("CamStats")]
    public GameObject camObject;
    public Transform cam;
    Vector3 camF;
    Vector3 camR;
    Vector3 intent;
    public float followRate = 5f;
    public float verticalMovementMax = 5f;
    public float angle;
    

    float blendAngle;

    public WadeState CurrentWadeState { get; private set; }

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
    public GameObject grenade;
    public List<Collider> IgnoredColliders = new List<Collider>();
    public AudioSource audio;
    public WadeSound wadeSound;
    public Image healthBar;
    public Image ammoBar;
    public Text pearlsCount;
    public PauseMenu pause;
    public GameObject gameOver;
    public GameObject boat;
    Transform cubeTarget;

    public string stateString;
    public float timeScale;





    private void Start()
    {

        pause = GameObject.Find("Pause").GetComponent<PauseMenu>();
        pause.enabled = false;
        health = startHealth;
        bullets = clipSize;
        canShoot = true;
        bullets = clipSize;
        Gravity = Mathf.Clamp(Gravity, -80, 0);
        // Assign to motor
        Motor.CharacterController = this;
        PlayerAnimator = PlayerMesh.GetComponent<Animator>();
        ledge = GetComponent<LedgeDetect>();
        input = GetComponent<WadeInputs>();
        TransitionToState(WadeState.Idle);
        audio = GetComponent<AudioSource>();
        wadeSound = GetComponent<WadeSound>();
        boat = GameObject.Find("WadeBoat");

        //input = PlayerTarget.GetComponent<WadeInputs>();

    }

    /// <summary>
    /// Handles movement state transitions and enter/exit callbacks
    /// </summary>
    public void TransitionToState(WadeState newState)
    {
        WadeState tmpInitialState = CurrentWadeState;
        OnStateExit(tmpInitialState, newState);
        CurrentWadeState = newState;
        OnStateEnter(newState, tmpInitialState);
    }

    /// <summary>
    /// Event when entering a state
    /// </summary>
    public void OnStateEnter(WadeState state, WadeState fromState)
    {
        triggerInUse = true;
        switch (state)
        {
            case WadeState.Idle:
                {
                    stateTimer = 0;
                    PlayerAnimator.SetBool("Idle", true);
                    break;
                }
            case WadeState.Walk:
                {
                    stateTimer = 0;
                    PlayerAnimator.SetBool("Walk", true);
                    break;
                }
            case WadeState.Jump:
                {
                    stateTimer = 0;

                    PlayerAnimator.SetBool(currentJumpProfile.Animation, true);
                    Motor.ForceUnground();

                    if (!lockedOn && fromState != WadeState.Dash)
                    {
                        cachedDirection = Motor.CharacterForward;
                    }
                    else
                    {
                        cachedDirection = LocalMovement();
                    }

                    hasShotInAir = false;

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
            case WadeState.LedgeGrab:
                {
                    PlayerAnimator.SetBool("LedgeGrab", true);
                    break;
                }
            case WadeState.WallSlide:
                {
                    PlayerAnimator.SetBool("WallSlide", true);
                    wallInputTimer = wallTimerStart;
                    wallHitAngle = Vector3.Angle(Motor.CharacterForward, ledge.middleNorm);
                    break;
                }
            case WadeState.AirAction:
                {
                    stateTimer = 0;
                    if (lockedOn)
                    {
                        if(LocalMovement().magnitude > .05f)
                        {
                            cachedTurnDirection = LocalMovement();
                        }
                        else
                        {
                            cachedTurnDirection = transform.forward;
                        }
                    }
                    
                    PlayerAnimator.SetBool(currentAirProfile.Animation, true);

                    slashInteruptionTimer = 0;
                    
                    break;

                }
            case WadeState.Dash:
                {
                    //if (lockedOn)
                    //{
                    //    cachedDirection = LocalMovement();
                    //}
                    //else
                    //{
                    //    cachedDirection = transform.forward;
                    //}

                    if(LocalMovement().magnitude < 0.1f)
                    {
                        cachedDirection = transform.forward;
                    }
                    else
                    {
                        cachedDirection = LocalMovement();
                    }

                    
                    stateTimer = 0;
                    bullets -= 1;
                    shotTimer = 0;
                    moveSpeed += dashSpeed;
                    PlayerAnimator.SetBool("Dash", true);
                    break;
                }
            case WadeState.Death:
                {
                    stateTimer = 0;
                    hasDied = false;
                    break;
                }
            case WadeState.Drive:
                {
                    boat.GetComponent<WadeBoatController>().controlled = true;
                    break;
                }
        }
    }

    /// <summary>
    /// Event when exiting a state
    /// </summary>
    public void OnStateExit(WadeState state, WadeState toState)
    {
        switch (state)
        {
            case WadeState.Idle:
                {

                    PlayerAnimator.SetBool("Idle", false);
                    break;
                }
            case WadeState.Walk:
                {
                    PlayerAnimator.SetBool("Walk", false);

                    break;
                }
            case WadeState.Jump:
                {
                    PlayerAnimator.SetBool(currentJumpProfile.Animation, false);


                    if(toState == WadeState.Idle)
                    {
                        takeBoatVelocity = false;
                    }
                    

                    if (toState != WadeState.AirAction)
                    {
                        verticalMoveSpeed = 0;
                    }

                    if (toState == WadeState.LedgeGrab)
                    {
                        moveSpeed = 0;
                    }
                    break;
                }
            case WadeState.LedgeGrab:
                {
                    PlayerAnimator.SetBool("LedgeGrab", false);
                    break;
                }
            case WadeState.WallSlide:
                {
                    wallHitAngle = 0;
                    wallHitInputAngle = 0;
                    PlayerAnimator.SetBool("WallSlide", false);
                    break;
                }
            case WadeState.AirAction:
                {
                    slamTimer = 0;
                    slashInteruptionTimer = 0;
                    if (toState == WadeState.Idle || toState == WadeState.Walk)
                    {
                        verticalMoveSpeed = 0;
                    }

                    PlayerAnimator.SetBool(currentAirProfile.Animation, false);
                    break;
                }
            case WadeState.Dash:
                {
                    PlayerAnimator.SetBool("Dash", false);
                    dashTimer = 0;
                    break;
                }
            case WadeState.Drive:
                {
                    triggerInUse = true;
                    boat.GetComponent<WadeBoatController>().controlled = false;
                    break;
                }
        }
    }

    public void Update()
    {
        if (takeBoatVelocity)
        {
            boatRelativeJumpMove = boat.GetComponent<WadeBoatController>().Motor.Velocity;
        }
        else
        {
            boatRelativeJumpMove = Vector3.zero;
        }
        pearlsCount.text = pearls.ToString();
        cubeTarget = camObject.GetComponent<WadeCamera>().lockOnInstance.transform;
        DoUI();
        DoAiming();
        blendAngle = Vector3.Angle(transform.forward, LocalMovement()) * input.Current.MoveInput.x;
        angle = Vector3.Angle(transform.forward, LocalMovement());
        stateString = CurrentWadeState.ToString();

        //Time.timeScale = timeScale;
        PlayerAnimator.SetFloat("Speed", input.Current.MoveInput.magnitude);
        wadeVelocity = Motor.Velocity;
        Shooting();
        PlayerAnimator.SetFloat("LockOnAngle", lockOnAngle);
        FollowBlock();

        //Debug.DrawRay(transform.position, LocalMovement() + Vector3.up, Color.blue);
        verticalMoveSpeed = Mathf.Clamp(verticalMoveSpeed, minVM, maxVM);
        switch (CurrentWadeState)
        {
            case WadeState.Idle:
                {
                    if (Input.GetButtonDown("B"))
                    {
                        if (canShoot)
                        {
                            TransitionToState(WadeState.Dash);
                        }
                        
                    }
                    ShootControls();

                    if (Input.GetButtonDown("RightBumper"))
                    {
                        GameObject tempGrenade = Instantiate(grenade, transform.position + transform.forward + Vector3.up, transform.rotation) as GameObject;
                        Rigidbody tempGrenadeRB = tempGrenade.GetComponent<Rigidbody>();
                        CapsuleCollider grenadeCollider = tempGrenade.GetComponent<CapsuleCollider>();
                        IgnoredColliders.Add(grenadeCollider);
                        tempGrenadeRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);
                        tempGrenadeRB.AddForce(Vector3.up * throwForceUp, ForceMode.Impulse);
                    }

                    if (canDrive)
                    {
                        if (Input.GetButtonDown("Action"))
                        {

                            TransitionToState(WadeState.Drive);
                        }
                    }

                    if(Input.GetButtonDown("LeftBumper"))
                    {
                        if(lockedOn == true)
                        {
                            //PlayerAnimator.SetBool("LockOn", false);
                            lockedOn = false;
                        }
                        else
                        {
                            if (canLockOn)
                            {
                                lockedOn = true;
                            }
                            //PlayerAnimator.SetBool("LockOn", true);
                            
                        }
                    }
                    

                    if (!reloading)
                    {
                        if (Input.GetButtonDown("Jump"))
                        {
                            currentJumpProfile = jumpStandard;
                            TransitionToState(WadeState.Jump);
                        }
                    }

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        currentJumpProfile = groundFall;
                        TransitionToState(WadeState.Jump);
                    }

                    break;

                }
            case WadeState.Walk:
                {
                    stateTimer += Time.deltaTime;

                    if(stateTimer > footStepFrequency)
                    {
                        wadeSound.PlayFootSteps();
                        stateTimer = 0;
                    }

                    if (Input.GetButtonDown("RightBumper"))
                    {
                        GameObject tempGrenade = Instantiate(grenade, transform.position + transform.forward + Vector3.up, transform.rotation) as GameObject;
                        Rigidbody tempGrenadeRB = tempGrenade.GetComponent<Rigidbody>();
                        CapsuleCollider grenadeCollider = tempGrenade.GetComponent<CapsuleCollider>();
                        IgnoredColliders.Add(grenadeCollider);
                        tempGrenadeRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);
                        tempGrenadeRB.AddForce(Vector3.up * throwForceUp, ForceMode.Impulse);
                    }

                    if (Input.GetButtonDown("B"))
                    {
                        if (canShoot)
                        {
                            TransitionToState(WadeState.Dash);
                        }
                    }

                    ShootControls();

                    if (canDrive)
                    {
                        if (Input.GetButtonDown("Action"))
                        {
 
                            TransitionToState(WadeState.Drive);
                        }
                    }

                    if (Input.GetButtonDown("LeftBumper"))
                    {
                        if (lockedOn == true)
                        {
                            //PlayerAnimator.SetBool("LockOn", false);
                            lockedOn = false;
                        }
                        else
                        {
                            //PlayerAnimator.SetBool("LockOn", true);
                            if (canLockOn)
                            {
                                lockedOn = true;
                            }
                        }
                    }

                    PlayerAnimator.SetFloat("Angle", blendAngle);

                    if (!reloading)
                    {
                        if (Input.GetButtonDown("Jump"))
                        {
                            currentJumpProfile = jumpStandard;
                            TransitionToState(WadeState.Jump);
                        }
                    }


                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        currentJumpProfile = groundFall;
                        TransitionToState(WadeState.Jump);
                    }

                    break;
                }
            case WadeState.Jump:
                {
                    stateTimer += Time.deltaTime;
                    if (Input.GetButtonDown("RightBumper"))
                    {
                        GameObject tempGrenade = Instantiate(grenade, transform.position + transform.forward + Vector3.up, transform.rotation) as GameObject;
                        CapsuleCollider grenadeCollider = tempGrenade.GetComponent<CapsuleCollider>();
                        IgnoredColliders.Add(grenadeCollider);
                        Rigidbody tempGrenadeRB = tempGrenade.GetComponent<Rigidbody>();
                        tempGrenadeRB.AddForce(Vector3.down * throwForceDown, ForceMode.Impulse);

                    }
                    if (Input.GetButtonDown("LeftBumper"))
                    {
                        if (lockedOn == true)
                        {
                            lockedOn = false;
                        }
                        else
                        {
                            if (canLockOn)
                            {
                                lockedOn = true;
                            }
                        }
                    }

                    if (currentJumpProfile == frontFlip && !hasShotInAir && stateTimer > airShotAnimationMatch)
                    {
                        Instantiate(bullet, gun.position, gun.rotation);
                        bullets -= 1;
                        shotTimer = 0;

                        wadeSound.PlayRifleShot();
                        hasShotInAir = true;
                    }

                    if (Input.GetButtonDown("B"))
                    {
                       TransitionToState(WadeState.Dash);
                    }

                    if (input.Current.SlashInput)
                    {
                        currentAirProfile = slashProfile;
                        TransitionToState(WadeState.AirAction);
                    }

                    if (ledge.canGrab && verticalMoveSpeed <= 2)
                    {
                        TransitionToState(WadeState.LedgeGrab);
                    }

                    else if (ledge.middlehNorm.y < .1f && verticalMoveSpeed <= 0 && ledge.middlehHit)
                    {
                        wallHitInputAngle = Vector3.Angle(LocalMovement(), ledge.middlehNorm);
                        wallHitAngle = Vector3.Angle(Motor.CharacterForward, ledge.middlehNorm);

                        if (wallHitInputAngle > 150)
                        {
                            TransitionToState(WadeState.WallSlide);
                        }
                        if (input.Current.JumpInput)
                        {
                            currentJumpProfile = jumpWall;
                            TransitionToState(WadeState.WallSlide);
                        }
                    }

                    ShootControls();

                    break;
                }
            case WadeState.LedgeGrab:
                {
                    if (Input.GetButtonDown("Jump"))
                    {
                        currentJumpProfile = jumpStandard;
                        TransitionToState(WadeState.Jump);
                    }

                    break;

                }
            case WadeState.WallSlide:
                {
                    if (ledge.canGrab)
                    {
                        TransitionToState(WadeState.LedgeGrab);
                    }

                    if (input.Current.JumpInput)
                    {
                        currentJumpProfile = jumpWall;
                        TransitionToState(WadeState.Jump);
                    }

                    if (wallHitInputAngle < 130)
                    {
                        wallInputTimer -= 10 * Time.deltaTime;

                        if (input.Current.JumpInput)
                        {
                            currentJumpProfile = jumpWall;
                            TransitionToState(WadeState.Jump);
                        }

                        if (wallInputTimer <= 0)
                        {
                            currentJumpProfile = airFall;
                            TransitionToState(WadeState.Jump);
                        }
                    }

                    break;

                }
            case WadeState.AirAction:
                {
                    if (currentAirProfile == slashProfile)
                    {
                        slashInteruptionTimer += 1 * Time.deltaTime;
                        slamTimer += 1 * Time.deltaTime;

                        if (Input.GetAxisRaw("RightTrigger") != 0)
                        {
                            if (canShoot && !triggerInUse)
                            {
                                currentJumpProfile = frontFlip;
                                TransitionToState(WadeState.Jump);

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
            case WadeState.Drive:
                {
                    if (Input.GetButtonDown("Action"))
                    {
                        TransitionToState(WadeState.Idle);
                    }

                    if (Input.GetButtonDown("Jump"))
                    {
                        currentJumpProfile = jumpStandard;
                        TransitionToState(WadeState.Jump);
                    }



                    break;

                }
            case WadeState.Dash:
                {
                    if (Input.GetButtonDown("RightBumper"))
                    {
                        GameObject tempGrenade = Instantiate(grenade, transform.position + transform.forward + Vector3.up, transform.rotation) as GameObject;
                        CapsuleCollider grenadeCollider = tempGrenade.GetComponent<CapsuleCollider>();
                        IgnoredColliders.Add(grenadeCollider);
                        Rigidbody tempGrenadeRB = tempGrenade.GetComponent<Rigidbody>();
                        tempGrenadeRB.AddForce(Vector3.down * throwForceDown, ForceMode.Impulse);
                    }

                    if (dashTimer > dashLength)
                    {
                        if (input.Current.SlashInput)
                        {
                            currentAirProfile = slashProfile;
                            TransitionToState(WadeState.AirAction);
                        }
                    }
                    if(moveSpeed <= walkSpeed + 0.1)
                    {
                        if (Motor.GroundingStatus.FoundAnyGround)
                        {
                            TransitionToState(WadeState.Idle);
                        }
                        else
                        {
                            currentJumpProfile = groundFall;
                            TransitionToState(WadeState.Jump);
                        }
                        
                    }
                    break;

                }
            case WadeState.Hit:
                {
                    stateTimer += Time.deltaTime;
                    if(stateTimer > recoveryTime)
                    {
                        TransitionToState(WadeState.Idle);
                    }
                    break;
                }
            case WadeState.Death:
                {


                    GameObject pause = GameObject.Find("Pause");
                    Destroy(pause);

                    stateTimer += Time.deltaTime;
                    if(stateTimer > deathTime && !hasDied)
                    {
                        Instantiate(gameOver);
                        hasDied = true;
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
        if (other.gameObject.name == ("Mover"))
        {
            print("yay");
            takeBoatVelocity = true;
            boatRelativeJumpMove = boat.GetComponent<WadeBoatController>().Motor.Velocity;
        }

        if (other.gameObject.name == ("BoatTrigger"))
        {
            
            canDrive = true;
        }

        Pearl pearl = other.GetComponent<Pearl>();

        if(pearl != null)
        {
            wadeSound.PlayPearlPickup();
        }   
        
        
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == ("BoatTrigger"))
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
        switch (CurrentWadeState)
        {
            case WadeState.Idle:
                {
                    //if (lockedOn)
                    //{
                    //    Vector3 lockOnRotation = (cubeTarget.position - transform.position);
                    //    Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                    //    lookRotation.x = 0;
                    //    lookRotation.z = 0;
                    //    currentRotation = Quaternion.Slerp(currentRotation, lookRotation, LockOnFollowTime * Time.deltaTime);
                    //}
                    //currentRotation = currentRotation;
                    break;
                }
            case WadeState.Walk:
                {
                    if (input.Current.MoveInput.magnitude > .1f)
                    {
                        turnSpeed = Mathf.Lerp(turnSpeedSlow, turnSpeedFast, moveSpeed);
                        Quaternion desiredAngle = Quaternion.LookRotation(LocalMovement());
                        currentRotation = Quaternion.Lerp(currentRotation, desiredAngle, turnSpeed * Time.deltaTime);
                    }
                    //else if(lockedOn)
                    //{
                    //    Vector3 lockOnRotation = (cubeTarget.position - transform.position);
                    //    Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                    //    lookRotation.x = 0;
                    //    lookRotation.z = 0;
                    //    currentRotation = Quaternion.Slerp(currentRotation, lookRotation, LockOnFollowTime * Time.deltaTime);
                    //}

                    break;
                }
            case WadeState.Jump:
                {
                    Vector3 angles = transform.localRotation.eulerAngles;

                    currentRotation = Quaternion.Euler(angles.x, Mathf.Abs(angles.y % 360), angles.z);

                    break;
                }
            case WadeState.WallSlide:
                {
                    currentRotation = Quaternion.Euler(0, wallHitAngle + transform.localEulerAngles.y, 0);
                    break;
                }
            case WadeState.Drive:
                {
                    currentRotation = boat.transform.rotation;
                    break;

                }
            case WadeState.AirAction:
                {
                    //if (lockedOn)
                    //{
                    //    if(LocalMovement().magnitude > 0.1f)
                    //    {
                    //        Quaternion desiredAngle = Quaternion.LookRotation(cachedTurnDirection);
                    //        currentRotation = Quaternion.Slerp(currentRotation, desiredAngle, slashTurnSpeed);
                    //    }
                    //}
                    break;
                }
            case WadeState.Dash:
                {

                    Quaternion desiredAngle = Quaternion.LookRotation(cachedDirection);
                    currentRotation = Quaternion.Lerp(currentRotation, desiredAngle, slashTurnSpeed * Time.deltaTime);
                    break;
                }
            case WadeState.Hit:
                {
                    
                    //currentRotation = currentRotation;
                    break;
                }
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (CurrentWadeState)
        {
            case WadeState.Idle:
                {
                    if (input.Current.MoveInput != Vector3.zero)
                    {
                        TransitionToState(WadeState.Walk);
                    }

                    moveSpeed = Mathf.Lerp(moveSpeed, 0, walkDecel * Time.deltaTime);
                    //moveSpeed = 0;
                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * moveSpeed, 10 * deltaTime);
                    break;
                }
            case WadeState.Walk:
                {
                    Gravity = 0;
                    if (input.Current.MoveInput == Vector3.zero)
                    {
                        TransitionToState(WadeState.Idle);
                    }

                    moveSpeed = Mathf.Lerp(moveSpeed, correctedWalkSpeed, walkAccel * Time.deltaTime);


                    //if (lockedOn)
                    //{
                    //    currentVelocity = Vector3.Lerp(currentVelocity, LocalMovement() * moveSpeed, 10 * deltaTime);
                    //}
                    //else
                    //{
                        currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * LocalMovement().magnitude * moveSpeed, 10 * deltaTime);
                    //}                    
                    break;
                }
            case WadeState.Jump:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        if (LocalMovement().magnitude > 0.02f)
                        {
                            TransitionToState(WadeState.Walk);
                        }
                        else
                        {
                            TransitionToState(WadeState.Idle);

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

                    if (verticalMoveSpeed <= gravSwitchStart && currentJumpProfile.Animation != "Fall")
                    {
                        Gravity = Mathf.Lerp(Gravity, maxGravity, gravitySwitchRate * Time.deltaTime);
                    }

                    verticalMoveSpeed += Gravity * Time.deltaTime;


                    if (ledge.middleHit)
                    {
                        moveSpeed = 0;
                    }

                    currentVelocity = cachedDirection * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection + boatRelativeJumpMove;



                    break;

                }
            case WadeState.LedgeGrab:
                {
                    currentVelocity = Vector3.zero;
                    break;
                }
            case WadeState.WallSlide:
                {
                    moveSpeed = 0;
                    verticalMoveSpeed = wallSlideSpeed;
                    currentVelocity = Vector3.down * verticalMoveSpeed;

                    wallHitInputAngle = Vector3.Angle(LocalMovement(), ledge.middleNorm);
                    wallHitAngle = Vector3.Angle(Motor.CharacterForward, ledge.middleNorm);

                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(WadeState.Idle);
                    }
                    break;
                }
            case WadeState.AirAction:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        if (LocalMovement().magnitude > 0.02f)
                        {
                            TransitionToState(WadeState.Walk);
                        }
                        else
                        {
                            TransitionToState(WadeState.Idle);

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

                        currentVelocity = cachedDirection * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection + boatRelativeJumpMove;
                    }
                    
                    break;

                }
            case WadeState.Drive:
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, 0, walkDecel * Time.deltaTime);
                    //moveSpeed = 0;
                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * moveSpeed, 10 * deltaTime);
                    break;
                }
            case WadeState.Dash:
                {
                    dashTimer += 1 * Time.deltaTime;
                    if(dashTimer > dashLength)
                    {
                        moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, dashAccel * deltaTime);
                    }

                    boatRelativeJumpMove = Vector3.Lerp(boatRelativeJumpMove, Vector3.zero, boatDashDecel * deltaTime);

                    
                    currentVelocity = cachedDirection * moveSpeed + boatRelativeJumpMove; ;
                    break;
                }
            case WadeState.Hit:
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, 0, walkDecel * Time.deltaTime);
                    //moveSpeed = 0;
                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * moveSpeed, 10 * deltaTime);
                    break;
                }
            case WadeState.Death:
                {

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        Gravity = -40;
                    }
                    else
                    {
                        Gravity = 0;
                        verticalMoveSpeed = 0;
                    }

                    verticalMoveSpeed += Gravity;

                    moveSpeed = Mathf.Lerp(moveSpeed, 0, walkDecel * 4 * Time.deltaTime);
                    //moveSpeed = 0;
                    currentVelocity = transform.forward * moveSpeed + Motor.CharacterUp * verticalMoveSpeed;
                    break;
                }
        }

    }

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
