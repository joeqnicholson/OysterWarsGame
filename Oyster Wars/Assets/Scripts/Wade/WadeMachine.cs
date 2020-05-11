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
    Idle, Walk, Jump, Crouch, LedgeGrab, WallSlide, AirAction, Drive, LockOn, Dash, Hit, Death, Slide
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
    public float boatDecel;
    public Vector3 boatMovement;
    public float health = 100;
    public float startHealth = 100;
    public int pearls = 0;
    public float stateTimer;
    public Vector3 lastStablePosition;
    public Quaternion lastStableRotation;
    public float waterDamage;

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
    public float walkVelocityAccel;
    public float walkVelocityDecel;
    public float footStepTimer;


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
    public bool fromHit;
    public bool spawnInBoat;
    public bool fromBoat;
    public bool maintainBoatOnGround;
    public float boatGroundTime;

    [Header("AirActions")]
    public float slashInteruptionTimer;
    public float slamTimer;
    public float slamStartTime;
    public float slashTurnSpeed;

    [Header("Slide")]
    public float slideTimer;
    public float slideDecel;
    public float turnAroundMaxSpeed;
    public float slideAngle;
    public float afterSlideTurnSpeed;
    public float slideSpeedThreshold;
    public bool fromSlide;
    public float canSlideTime;


    [Header("Dash")]
    public float dashLength;
    public float dashSpeed;
    public float dashAccel;
    public float dashTimer;
    public float boatDashDecel;
    public float dashEndSpeedIncrease;
    public bool hasDashed;
    public Vector3 cachedTurnDirection;
    public Vector3 cachedDirection;
    Vector3 targetDash;

    [Header("Hit")]
    public float recoveryTime;
    public float deathTime;
    public bool invincible;
    public bool hasDied;
    public float invincibilityTimer;
    public float invincibilityTime;
    public float flashTime;
    public float flashTimer;

    [Header("Shooting")]
    public float boltActionTime;
    public float shotTimer;
    public bool canShoot;
    public bool triggerInUse;
    public bool aiming;
    public bool lockedOn = false;
    public bool canLockOn = false;
    public float aimingWalkSpeed;
    public bool hasFlipped;

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
    public GameObject PlayerSkinnedMesh;
    public Transform lockOnTarget;
    public Transform gun;
    public Transform gunParentBone;
    public LedgeDetect ledge;
    public Transform camFollow;
    public GameObject bullet;
    public GameObject grenade;
    public List<Collider> IgnoredColliders = new List<Collider>();
    public List<GameObject> bullets = new List<GameObject>();
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
        bullet = bullets[0];
        pause = GameObject.Find("Pause").GetComponent<PauseMenu>();
        pause.enabled = false;
        health = startHealth;
        canShoot = true;
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
        boatDecel = boat.GetComponent<WadeBoatController>().deceleration;

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
        stateTimer = 0;
        triggerInUse = true;
        switch (state)
        {
            case WadeState.Idle:
                {
                    boatRelativeJumpMove = Vector3.zero;
                    if (fromBoat)
                    {
                        maintainBoatOnGround = true;
                    }
                    else
                    {
                        maintainBoatOnGround = false;
                    }



                    hasDashed = false;
                    hasFlipped = false;
                    PlayerAnimator.SetBool("Idle", true);
                    break;
                }
            case WadeState.Walk:
                {
                    boatRelativeJumpMove = Vector3.zero;

                    if(fromState == WadeState.Slide)
                    {
                        moveSpeed = -turnAroundMaxSpeed + 10;
                        fromSlide = true;
                        Motor.SetRotation(Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 180, transform.eulerAngles.x));
                    }
                    else
                    {
                        fromSlide = false;
                    }

                    

                    if (fromBoat)
                    {
                        maintainBoatOnGround = true;
                    }
                    else
                    {
                        maintainBoatOnGround = false;
                    }
                    hasDashed = false;
                    hasFlipped = false;
                    PlayerAnimator.SetBool("Walk", true);
                    break;
                }
            case WadeState.Jump:
                {
                    

                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        if (!takeBoatVelocity)
                        {
                            lastStablePosition = transform.position + (transform.forward * -2) + (Vector3.up * 3);
                            lastStableRotation = transform.rotation;
                            spawnInBoat = false;
                            fromBoat = false;
                        }
                        else
                        {
                            fromBoat = true;
                            spawnInBoat = true;
                        }
                        
                    }

                    if (fromState == WadeState.Idle || fromState == WadeState.Walk)
                    {
                        if(currentJumpProfile == groundFall)
                        {
                            if(moveSpeed > 3)
                            {
                                moveSpeed -= walkSpeed / 2.5f;
                            }
                        }
                        lastStablePosition = transform.position + (transform.forward * -2) + (Vector3.up * 3);
                        lastStableRotation = transform.rotation;
                    }

                    PlayerAnimator.SetBool(currentJumpProfile.Animation, true);

                    Motor.ForceUnground();

                    cachedDirection = Motor.CharacterForward;

                    hasShotInAir = false;

                    planarMoveDirection = planarMoveDirection * currentJumpProfile.planarMultiplier;

                    if(currentJumpProfile == frontFlip) 
                    {
                        hasFlipped = true; 
                    }

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

                        if(fromState != WadeState.AirAction)
                        {
                            boatMovement = boatRelativeJumpMove;
                        }
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
                    hasDashed = true;
                    boatMovement = Vector3.zero;
                    if(LocalMovement().magnitude < 0.1f)
                    {
                        cachedDirection = transform.forward;
                    }
                    else
                    {
                        cachedDirection = LocalMovement();
                    }
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
            case WadeState.Hit:
                {
                    bullet = bullets[0];
                   invincibilityTimer = 0;
                   break;
                }
            case WadeState.Drive:
                {
                    boat.GetComponent<WadeBoatController>().controlled = true;
                    break;
                }
            case WadeState.Slide:
                {
                    PlayerAnimator.SetBool("Slide", true);
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


                    fromHit = false;
                    PlayerAnimator.SetBool(currentJumpProfile.Animation, false);

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
                    takeBoatVelocity = false;
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
            case WadeState.Slide:
                {
                    PlayerAnimator.SetBool("Slide", false);
                    break;
                }
        }
    }

    public void Update()
    {
        invincibilityTimer += Time.deltaTime;
        flashTimer += Time.deltaTime;
        stateTimer += Time.deltaTime;
        DoInvincibility();
        DoUI();
        DoAiming();
        DoShooting();
        DoFollowBlock();
        DoUpdateVariables();

        //Debug.DrawRay(transform.position, LocalMovement() + Vector3.up, Color.blue);
        verticalMoveSpeed = Mathf.Clamp(verticalMoveSpeed, minVM, maxVM);
        switch (CurrentWadeState)
        {
            case WadeState.Idle:
                {
                    if (Input.GetButtonDown("Slash"))
                    {
                        PlayerAnimator.SetBool("Slash", true);
                    }

                    if (Input.GetButtonUp("Slash"))
                    {
                        PlayerAnimator.SetBool("Slash", false);
                    }

                    if (Input.GetButtonDown("Jump"))
                    {
                        if (maintainBoatOnGround && stateTimer < boatGroundTime)
                        {
                            moveSpeed += boatMovement.magnitude;
                        }
                        currentJumpProfile = jumpStandard;
                        TransitionToState(WadeState.Jump);
                    }

                    if (Input.GetButtonDown("B"))
                    {
                        TransitionToState(WadeState.Dash);
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

                    if (!Motor.GroundingStatus.IsStableOnGround)
                    {
                        currentJumpProfile = groundFall;
                        TransitionToState(WadeState.Jump);
                    }

                    break;

                }
            case WadeState.Walk:
                {


                    if (Input.GetButtonDown("Slash"))
                    {
                        PlayerAnimator.SetBool("Slash", true);
                    }

                    if (Input.GetButtonUp("Slash"))
                    {
                        PlayerAnimator.SetBool("Slash", false);
                    }

                    stateTimer += Time.deltaTime;
                    footStepTimer += Time.deltaTime;

                    if(stateTimer > footStepFrequency)
                    {
                        wadeSound.PlayFootSteps();
                        footStepTimer = 0;
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

                    if(angle > slideAngle && moveSpeed > slideSpeedThreshold && stateTimer > canSlideTime)
                    {
                        TransitionToState(WadeState.Slide);
                    }



                    if (Input.GetButtonDown("Jump"))
                    {
                        if (maintainBoatOnGround && stateTimer < boatGroundTime)
                        {
                            moveSpeed += boatMovement.magnitude;
                        }
                        currentJumpProfile = jumpStandard;
                        TransitionToState(WadeState.Jump);
                    }


                    if (!Motor.GroundingStatus.IsStableOnGround)
                    {
                        currentJumpProfile = groundFall;
                        TransitionToState(WadeState.Jump);
                    }

                    break;
                }
            case WadeState.Jump:
                {
                    if (currentJumpProfile.CanControlHeight)
                    {
                        if (currentJumpProfile == jumpStandard && Input.GetButtonUp("Jump") && verticalMoveSpeed > minJumpVelocity)
                        {
                            verticalMoveSpeed = minJumpVelocity;
                        }
                    }


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

                        shotTimer = 0;

                        wadeSound.PlayRifleShot();
                        hasShotInAir = true;
                    }

                    if (Input.GetButtonDown("B"))
                    {
                        if (!hasDashed)
                        {
                            TransitionToState(WadeState.Dash);
                        }
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
                            if (!triggerInUse && !hasFlipped)
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

                    if(moveSpeed <= walkSpeed + dashEndSpeedIncrease)
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            if (Input.GetButtonDown("Jump"))
                            {
                                hasDashed = false;
                                moveSpeed = walkSpeed + dashEndSpeedIncrease - 2;
                                currentJumpProfile = dashLongJump;
                                TransitionToState(WadeState.Jump);
                            }
                        }
                    }

                    
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            if (moveSpeed <= walkSpeed + 0.1)
                            {
                                TransitionToState(WadeState.Idle);
                            }
                        }
                        else
                        {
                            if (moveSpeed <= walkSpeed + 3)
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
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            TransitionToState(WadeState.Idle);
                        }
                        else
                        {
                            currentJumpProfile = airFall;
                            fromHit = true;
                            TransitionToState(WadeState.Jump);
                        }
                        
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
        PowerUp power = other.GetComponent<PowerUp>();

        if (pearl||power)
        {
            wadeSound.PlayPearlPickup();
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if(enemy)
        {
            DoTakeDamage(enemy.hitPower);
        }

        if(other.gameObject.layer == 4)
        {
            if (health > waterDamage)
            {
                Motor.SetPositionAndRotation(lastStablePosition, lastStableRotation);
                moveSpeed = 0;
                verticalMoveSpeed = 0;
                planarMoveDirection = Vector3.zero;
                boatMovement = Vector3.zero;
            }
            
            
            DoTakeDamage(waterDamage);
        }
        
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == ("Mover"))
        { 
            takeBoatVelocity = false;
            boatRelativeJumpMove = boat.GetComponent<WadeBoatController>().Motor.Velocity;
        }

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
                    turnSpeed = turnSpeedSlow;
                    break;
                }
            case WadeState.Walk:
                {
                    if (input.Current.MoveInput.magnitude > .1f)
                    {   if(fromSlide && stateTimer < 0.1)
                        {
                            //turnSpeed = afterSlideTurnSpeed;
                        }
                        else
                        {
                            turnSpeed = Mathf.Lerp(turnSpeed, turnSpeedFast, moveSpeed * Time.deltaTime);
                        }

                        Quaternion desiredAngle = Quaternion.LookRotation(LocalMovement());
                        currentRotation = Quaternion.Lerp(currentRotation, desiredAngle, turnSpeed * Time.deltaTime);
                    }
                    else
                    {
                        
                    }

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
                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * moveSpeed, walkVelocityDecel * deltaTime);
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

                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * LocalMovement().magnitude * moveSpeed, walkVelocityAccel * deltaTime);
                  
                    break;
                }
            case WadeState.Slide:
                {
                    Gravity = 0;
                    
                    if(stateTimer > slideTimer)
                    {
                        moveSpeed = Mathf.Lerp(moveSpeed, turnAroundMaxSpeed, slideDecel * Time.deltaTime);
                    }
                    

                    if (moveSpeed <= turnAroundMaxSpeed + .5f)
                    {
                        TransitionToState(WadeState.Walk);
                    }

                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * moveSpeed, 10 * deltaTime);

                    break;
                }
            case WadeState.Jump:
                {
                    if (Motor.GroundingStatus.IsStableOnGround)
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

                    if (input.Current.MoveInput != Vector3.zero && !fromHit)
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

                    boatMovement = Vector3.Lerp(boatMovement, Vector3.zero, boatDecel * deltaTime);

                    if (ledge.middleHit)
                    {
                        moveSpeed = 0;
                    }

                    currentVelocity = cachedDirection * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection + boatMovement;



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

                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        TransitionToState(WadeState.Idle);
                    }
                    break;
                }
            case WadeState.AirAction:
                {
                    if (Motor.GroundingStatus.IsStableOnGround)
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

                        boatMovement = Vector3.Lerp(boatMovement, Vector3.zero, boatDecel * deltaTime);


                        currentVelocity = cachedDirection * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection + boatMovement;
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
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, dashAccel * deltaTime);
                        }
                        else
                        {
                            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed + 2, dashAccel * deltaTime);
                        }
                        
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

                    if (!Motor.GroundingStatus.IsStableOnGround)
                    {
                        Gravity = -12;
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
