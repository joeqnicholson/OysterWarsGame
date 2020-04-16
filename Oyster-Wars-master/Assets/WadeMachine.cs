using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;



public enum CharacterState
{
    Idle, Walk, Jump, Crouch, LedgeGrab, WallSlide, Pogo, SparkJump
}

//[RequireComponent(typeof(WadeInputs))]   
public partial class WadeMachine : MonoBehaviour, ICharacterController
{

    public ParticleSystem Sparks;

    public float sparkJumpAccel;
    public float sparkJumpPlanarSpeed;
    public float sparkVerticalDifference;

    public float maxVM;
    public float smaxVM;

    public float minVM;
    public float sparkExitGravity;
    public bool fromSpark;
    public float sparkTimer;
    public float sparkTimerReset;
    public float sparkRate;
    public float sparkIncreaseRate;
    public float wallSlideSpeed;
    public float wallHitAngle;
    public float wallHitInputAngle;
    public float wallTimerStart;
    public KinematicCharacterMotor Motor;
    public Transform cam;
    public Vector3 camF;
    public Vector3 camR;
    public Vector3 intent;
    public Vector3 moveDirection;
    public WadeInputs input;
    public float angle;
    public float verticalMoveSpeed;
    public float walkSpeed;
    public float walkAccel;
    public float walkDecel;
    public float Gravity;
    public Animator PlayerAnimator;
    public GameObject PlayerMesh;
    public string stateString;
    bool canTurn;
    float num = 1;
    bool lockedOn = false;
    public Transform lockOnTarget;
    public Transform gun;
    public float crouchDecel;
    public Vector3 planarMoveDirection;
    public float linearDrag = 5;
    public LedgeDetect ledge;
    public float maxGravity;
    public float gravitySwitchRate = 8;
    public float wallInputTimer;
    public bool comingFromWallSlide;
    public float blendAngle;
    public Vector3 cachedDirection;

    public CharacterState CurrentCharacterState { get; private set; }

    [Header("Draggables")]
    public float minJumpHeight;
    float maxJumpVelocity;
    float minJumpVelocity;
    public Vector3 wadeVelocity;

    public float moveSpeed;

    public Vector3 _moveInputVector;
    private Vector3 _lookInputVector;

    [Header("Draggables")]
    public Transform camFollow;
    public float followRate = 5f;
    public float verticalMovementMax = 5f;
    public float timeScale;



private void Start()
    {

        Gravity = Mathf.Clamp(Gravity, -80, 0);
        // Assign to motor
        Motor.CharacterController = this;
        PlayerAnimator = PlayerMesh.GetComponent<Animator>();
        ledge = GetComponent<LedgeDetect>();
        input = GetComponent<WadeInputs>();
        TransitionToState(CharacterState.Idle);
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
                    sparkTimer = sparkTimerReset;
                    canTurn = false;
                    break;
                }
            case CharacterState.Walk:
                {

                    canTurn = true;
                    break;
                }
            case CharacterState.Jump:
                {
                    PlayerAnimator.SetBool(currentJumpProfile.Animation, true);


                    Motor.ForceUnground();

                    if (currentJumpProfile != fallProfile)
                    {
                        fromSpark = false;
                        planarMoveDirection = Vector3.zero;
                        Gravity = -(2 * currentJumpProfile.JumpHeight) / Mathf.Pow(currentJumpProfile.timeToJumpApex, 2);
                        maxGravity = Gravity * 1.3f;
                        maxJumpVelocity = Mathf.Abs(Gravity) * currentJumpProfile.timeToJumpApex;
                        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);
                        verticalMoveSpeed = maxJumpVelocity;
                        moveSpeed += currentJumpProfile.InitialForwardVelocity;
                    }
                    else if (fromState == CharacterState.SparkJump && currentJumpProfile == fallProfile)
                    {
                        Gravity = sparkExitGravity;
                        maxGravity = Gravity * 1.3f;
                        fromSpark = true;
                    }
                    else if (currentJumpProfile == fallProfile && (fromState != CharacterState.Pogo) )
                    {
                        fromSpark = true;
                        Gravity = -40;
                        maxGravity = Gravity * 1.3f;
                        maxJumpVelocity = Mathf.Abs(Gravity) * currentJumpProfile.timeToJumpApex;
                        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);
                        verticalMoveSpeed = maxJumpVelocity;
                        moveSpeed += currentJumpProfile.InitialForwardVelocity;
                    }
                    

                    break;

                }
            case CharacterState.Crouch:
                {
                    PlayerAnimator.SetBool("Crouch", true);

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
            case CharacterState.Pogo:
                {

                    PlayerAnimator.SetBool("Pogo", true);
                    break;
                }
            case CharacterState.SparkJump:
                {
                    cachedDirection = transform.rotation.eulerAngles;
                    Sparks.Play();
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
                    break;
                }
            case CharacterState.Jump:
                {
                    PlayerAnimator.SetBool(currentJumpProfile.Animation, false);
                    //planarMoveDirection = Vector3.zero;
                    if(toState != CharacterState.Pogo)
                    {
                        verticalMoveSpeed = 0;
                    }

                    if(toState == CharacterState.LedgeGrab)
                    {
                        moveSpeed = 0;
                    }
                    break;
                }
            case CharacterState.Crouch:
                {
                    PlayerAnimator.SetBool("Crouch", false);
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
            case CharacterState.Pogo:
                {
                    if (toState == CharacterState.Idle)
                    {
                        verticalMoveSpeed = 0;
                    }

                    PlayerAnimator.SetBool("Pogo", false);
                    break;
                }
            case CharacterState.SparkJump:
                {
                    Sparks.Stop();
                    PlayerAnimator.SetBool("SparkJump", false);
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
        angle = Vector3.Angle(transform.forward, LocalMovement()) * input.Current.MoveInput.x;
        blendAngle = Vector3.Angle(transform.forward, LocalMovement()) * input.Current.MoveInput.x;


        Time.timeScale = timeScale;
        PlayerAnimator.SetFloat("Speed", input.Current.MoveInput.magnitude);
        wadeVelocity = Motor.Velocity;
        Shooting();
        FollowBlock();
        _moveInputVector = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")), 1f);

        //Debug.DrawRay(transform.position, LocalMovement() + Vector3.up, Color.blue);
        
        switch (CurrentCharacterState)
        {
            case CharacterState.Idle:
                {
                    verticalMoveSpeed = Mathf.Clamp(verticalMoveSpeed, minVM, maxVM);

                    if (Input.GetButtonDown("Jump"))
                    {
                        currentJumpProfile = jumpStandard;
                        TransitionToState(CharacterState.Jump);
                    }
                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        currentJumpProfile = fallProfile;
                        TransitionToState(CharacterState.Jump);
                    }
                    if (Input.GetButtonDown("Crouch"))
                    {
                        TransitionToState(CharacterState.Crouch);
                    }
                    break;

                }
            case CharacterState.Walk:
                {

                    PlayerAnimator.SetFloat("Angle", blendAngle);

                    if (Input.GetButtonDown("Jump"))
                    {
                        currentJumpProfile = jumpStandard;
                        TransitionToState(CharacterState.Jump);
                    }
                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        currentJumpProfile = fallProfile;
                        TransitionToState(CharacterState.Jump);
                    }
                    if (Input.GetButtonDown("Crouch") || Input.GetButton("Crouch"))
                    {
                        TransitionToState(CharacterState.Crouch);
                    }
                    
                    break;
                }
            case CharacterState.Jump:
                {
                    if (input.Current.CrouchInputDown)
                    {
                        TransitionToState(CharacterState.Pogo);
                    }
                    

                    if (ledge.canGrab && verticalMoveSpeed <= 2)
                    {
                        TransitionToState(CharacterState.LedgeGrab);
                    }
                    else if(ledge.middlehNorm.y < .1f && verticalMoveSpeed <= 0 && ledge.middlehHit)
                    {
                        wallHitInputAngle = Vector3.Angle(LocalMovement(), ledge.middlehNorm);
                        wallHitAngle = Vector3.Angle(Motor.CharacterForward, ledge.middlehNorm);

                        if(wallHitInputAngle > 150)
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
            case CharacterState.Crouch:
                {
                    if (Input.GetButtonDown("Jump"))
                    {
                        if(Mathf.Abs(Motor.Velocity.x) > 1.0f|| Mathf.Abs(Motor.Velocity.z) > 1.0f)
                        {
                            currentJumpProfile = jumpLong;
                            TransitionToState(CharacterState.Jump);
                        }
                        else
                        {
                            currentJumpProfile = jumpHigh;
                            moveSpeed += currentJumpProfile.InitialForwardVelocity;
                            TransitionToState(CharacterState.Jump);
                        }
                    }

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        currentJumpProfile = fallProfile;
                        TransitionToState(CharacterState.Jump);
                    }

                    if (Input.GetButtonUp("Crouch"))
                    {
                        TransitionToState(CharacterState.Idle);
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
                            currentJumpProfile = fallProfile;
                            TransitionToState(CharacterState.Jump);
                        }


                    }
                    
                    break;
                }
            case CharacterState.Pogo:
                {
                    

                    if (input.Current.ShootInput)
                    {
                        TransitionToState(CharacterState.SparkJump);
                    }
                    break;
                }
            case CharacterState.SparkJump:
                {
                    verticalMoveSpeed = Mathf.Clamp(verticalMoveSpeed, minVM, smaxVM);

                    sparkTimer -= Time.deltaTime;
                    if (Input.GetButtonUp("Shoot") || sparkTimer <= 0)
                    {
                        currentJumpProfile = fallProfile;
                        TransitionToState(CharacterState.Jump);
                    }
                    break;
                }
                //case CharacterState.Jump:
        }

        


        stateString = CurrentCharacterState.ToString();
        
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>

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
                    //currentRotation = currentRotation;
                    break;
                }
            case CharacterState.Walk:
                {
                    if(input.Current.MoveInput.magnitude > .1f)
                    {
                        Quaternion desiredAngle = Quaternion.LookRotation(LocalMovement()); ;
                        currentRotation = Quaternion.Lerp(currentRotation, desiredAngle, 19 * Time.deltaTime);
                    }
                    
                    break;
                }
            case CharacterState.Jump:
                {
                    Vector3 angles = transform.localRotation.eulerAngles;

                    currentRotation = Quaternion.Euler(angles.x, Mathf.Abs(angles.y % 360), angles.z);



                    //currentRotation = currentRotation;
                    break;
                }
            case CharacterState.Crouch:
                {
                    //currentRotation = Quaternion.FromToRotation(transform.up, ledge.bottomrNorm) * transform.rotation;
                    break;
                }
            case CharacterState.WallSlide:
                {
                    currentRotation = Quaternion.Euler(0, wallHitAngle + transform.localEulerAngles.y, 0);
                    break;
                }
            case CharacterState.SparkJump:
                {
                    //if (input.Current.MoveInput.magnitude > .1f)
                    //{

                    //    Quaternion desiredAngle = Quaternion.LookRotation(-airLocalMovement(2) + Vector3.up);

                    //    Quaternion newDesire = Quaternion.Euler(desiredAngle.eulerAngles.x, cachedDirection.y, desiredAngle.eulerAngles.z);
                    //    currentRotation = Quaternion.Lerp(currentRotation, desiredAngle, 19 * Time.deltaTime);
                    //}
                    //else
                    //{
                    //    Quaternion desiredAngle = Quaternion.LookRotation(airLocalMovement(1));
                    //    currentRotation = Quaternion.Lerp(currentRotation, desiredAngle, 19 * Time.deltaTime);
                    //}

                    break;
                }
                //case CharacterState.Jump:
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
                    

                    moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, walkAccel * Time.deltaTime);
                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * LocalMovement().magnitude * moveSpeed, 10 * deltaTime);
                    break;
                }
            case CharacterState.Jump:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(CharacterState.Idle);
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

                    if (input.Current.MoveInput != Vector3.zero && fromSpark == false)
                    {
                        if ((angle) < 50 && moveSpeed > 4)
                        {
                            jumpAirSpeed = 0.5f;
                            jumpAccel = currentJumpProfile.Acceleration;
                        }
                        else if ((angle) > 100 && currentJumpProfile == jumpLong)
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
                    


                    if (verticalMoveSpeed <= 0 && currentJumpProfile != fallProfile) { Gravity = Mathf.Lerp(Gravity, maxGravity, gravitySwitchRate * Time.deltaTime);  }

                    if(fromSpark == true)
                    {
                        Gravity = Mathf.Lerp(Gravity, maxGravity, gravitySwitchRate * Time.deltaTime);
                    }

                    verticalMoveSpeed += Gravity * Time.deltaTime;


                    //currentVelocity += (transform.up * verticalMoveSpeed) - Vector3.Project(currentVelocity, transform.up);
                    if (ledge.middleHit)
                    {
                        moveSpeed = 0;
                    }
                    
                        currentVelocity = Motor.CharacterForward * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection;

                    

                    break;

                }
            case CharacterState.Crouch:
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, 0, crouchDecel * Time.deltaTime);
                    currentVelocity = Vector3.Lerp(currentVelocity, transform.forward * moveSpeed, 10 * deltaTime);
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
            case CharacterState.Pogo:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(CharacterState.Idle);
                    }

                    float jumpAccel;
                    float jumpAirSpeed;
                    float angle = Vector3.Angle(transform.forward, LocalMovement());


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

                    if (Input.GetButtonUp("Crouch"))
                    {
                        currentJumpProfile = fallProfile;
                        TransitionToState(CharacterState.Jump);
                    }


                    if (verticalMoveSpeed <= 0 && currentJumpProfile != fallProfile) { Gravity = Mathf.Lerp(Gravity, maxGravity, gravitySwitchRate * Time.deltaTime); }

                    verticalMoveSpeed += Gravity * Time.deltaTime;


                    //currentVelocity += (transform.up * verticalMoveSpeed) - Vector3.Project(currentVelocity, transform.up);
                    if (ledge.middleHit)
                    {
                        moveSpeed = 0;
                    }

                    currentVelocity = Motor.CharacterForward * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection;
                    //currentVelocity = transform.up * 5;
                    break;

                }
            case CharacterState.SparkJump:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(CharacterState.Idle);
                    }

                    float jumpAccel = sparkJumpAccel;
                    float jumpAirSpeed = sparkJumpPlanarSpeed;


                    if (input.Current.MoveInput != Vector3.zero)
                    {
                        if(angle > 110)
                        {
                            moveSpeed = Mathf.Lerp(moveSpeed, 0, ((2 - (180f / angle)) / 3f) * Time.deltaTime);
                        }

                        if(angle < 50 && wadeVelocity.magnitude > 2)
                        {
                            jumpAirSpeed = sparkJumpPlanarSpeed / 4;
                        }
                        
                        planarMoveDirection = Vector3.MoveTowards(planarMoveDirection, LocalMovement() * jumpAirSpeed, jumpAccel * Time.deltaTime);
                    }
                    

                    if (Input.GetButtonUp("Crouch"))
                    {
                        currentJumpProfile = fallProfile;
                        TransitionToState(CharacterState.Jump);
                    }


                    if (verticalMoveSpeed <= 0 && currentJumpProfile != fallProfile) { Gravity = Mathf.Lerp(Gravity, maxGravity, gravitySwitchRate * Time.deltaTime); }

                    verticalMoveSpeed += Gravity * Time.deltaTime;


                    //currentVelocity += (transform.up * verticalMoveSpeed) - Vector3.Project(currentVelocity, transform.up);
                    if (ledge.middleHit)
                    {
                        moveSpeed = 0;
                    }

                    float upIncrease;
                    float correctedUpIncrease;
                    upIncrease = Mathf.Lerp(0, sparkRate * (Gravity / -80), sparkIncreaseRate * deltaTime);
                    correctedUpIncrease = upIncrease - ((upIncrease * sparkVerticalDifference) * (LocalMovement().magnitude));

                    verticalMoveSpeed += correctedUpIncrease;

                    
                    currentVelocity = Motor.CharacterForward * moveSpeed + Motor.CharacterUp * verticalMoveSpeed + planarMoveDirection;
                    //currentVelocity = transform.up * 5;
                    break;

                }
        }
        //currentVelocity = moveDirection;

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


