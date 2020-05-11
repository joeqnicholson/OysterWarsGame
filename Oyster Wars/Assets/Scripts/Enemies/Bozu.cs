using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using UnityEditorInternal;

public enum BozuState
{
    Idle, Run, Fall, Shooting, Jump, Sweep
}

public class Bozu : Enemy, ICharacterController
{
    public float bozuTurnSpeed;
    public float bozuRunSpeed;
    public float startRunningDistance;
    public float startShootingDistance;
    public float maxTurnDistance;
    public float stateTimer;
    public float runLength;
    public float dropTimer;
    public float dropFrequency;
    public bool hasDropped;
    public float Gravity;
    public float idleWaitTime;
    public float endRunTime;

    public float shootTime;
    public float shootFrequency;
    public float sweepTime;
    public float sweepShootFrequency;
    public float sweepVerticalDistanceMax;

    public GameObject characterMesh;
    public Animator animator;

    public KinematicCharacterMotor Motor;
    public BozuState CurrentBozuState { get; private set; }
    

    private void Start()
    {
        cam = GameObject.Find("Camera").GetComponent<Camera>();
        wade =  GameObject.Find("Wade").transform;
        wadeCamera = GameObject.Find("Camera").GetComponent<WadeCamera>();
        wadeMachine = GameObject.Find("Wade").GetComponent<WadeMachine>();
        Motor.CharacterController = this;
        turnSpeed = bozuTurnSpeed;
        TransitionToState(BozuState.Idle);
        health = startHealth;
    }


    public void TransitionToState(BozuState newState)
    {
        BozuState tmpInitialState = CurrentBozuState;
        OnStateExit(tmpInitialState, newState);
        CurrentBozuState = newState;
        OnStateEnter(newState, tmpInitialState);
    }


    public void OnStateEnter(BozuState state, BozuState fromState)
    {
        switch (state)
        {
            case BozuState.Idle:
                {
                    animator.SetBool("Idle", true);
                    dropTimer = 0;
                    stateTimer = 0;
                    break;
                }
            case BozuState.Shooting:
                {
                    Shoot("notflat");
                    animator.SetBool("Shoot", true);
                    dropTimer = 0;
                    stateTimer = 0;

                    break;
                }
            case BozuState.Sweep:
                {
                    dropTimer = 0;
                    stateTimer = 0;
                    animator.SetBool("Sweep", true);
                    break;
                }
            case BozuState.Jump:
                {
                    GrenadeThrow();

                    float jumpVelocity;
                    Motor.ForceUnground();
                    Gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
                    maxGravity = Gravity * downMultiplier;
                    jumpVelocity = Mathf.Abs(Gravity) * timeToJumpApex;
                    verticalMoveSpeed = jumpVelocity;

                    if (jumpForwards)
                    {
                        moveSpeed += initialForwardVelocity;
                    }
                    
                    dropTimer = 0;
                    stateTimer = 0;
                    animator.SetBool("Jump", true);

                    break;
                }
            case BozuState.Run:
                {
                    animator.SetBool("Run", true);
                    stateTimer = 0;
                    break;
                }
            case BozuState.Fall:
                {
                    stateTimer = 0;
                    break;
                }
        }
    }

    public void OnStateExit(BozuState state, BozuState toState)
    {
        switch (state)
        {
            case BozuState.Idle:
                {
                    animator.SetBool("Idle", false);
                    break;
                }
            case BozuState.Shooting:
                {
                    animator.SetBool("Shoot", false);
                    break;
                }
            case BozuState.Sweep:
                {
                    animator.SetBool("Sweep", false);
                    break;
                }
            case BozuState.Jump:
                {
                    timeSinceLastJump = 0;
                    verticalMoveSpeed = 0;
                    animator.SetBool("Jump", false);
                    break;
                }
            case BozuState.Run:
                {
                    animator.SetBool("Run", false);
                    break;
                }
            case BozuState.Fall:
                {
                    verticalMoveSpeed = 0;
                    break;
                }
        }
    }

    public void Update()
    {
        stateString = CurrentBozuState.ToString();
        timeSinceLastJump += Time.deltaTime;
        DoTargeting();
        DoHealth();

        groundFar = DetectForwardGroundFar();
        groundFront = DetectForwardGround();

        switch (CurrentBozuState)
        {
            case BozuState.Idle:
                {
                    stateTimer += Time.deltaTime;

                    if (stateTimer > idleWaitTime && WadeDistance() < startShootingDistance)
                    {
                        int randomStateChange = RandomInt(1, 5);
                        switch (randomStateChange)
                        {
                            case 1:
                                {
                                    TransitionToState(BozuState.Shooting);
                                    break;
                                }
                            case 2:
                                {
                                    if(WadeVerticalDistance() > sweepVerticalDistanceMax && wadeMachine.stateString != "Jump" && WadeDistance() < startShootingDistance - 5)
                                    {
                                        TransitionToState(BozuState.Shooting);
                                    }
                                    else
                                    {
                                        TransitionToState(BozuState.Sweep);
                                    }
                                        
                                    break;
                                }
                            case 3:
                                {
                                    jumpForwards = false;
                                    TransitionToState(BozuState.Jump);
                                    break;
                                }
                            case 4:
                                {
                                    TransitionToState(BozuState.Run);
                                    break;
                                }
                            case 5:
                                {
                                    TransitionToState(BozuState.Run);
                                    break;
                                }
                        }
                    }
                    else if(stateTimer > idleWaitTime && WadeDistance() < startRunningDistance)
                    {
                        TransitionToState(BozuState.Run);
                        break;
                    }


                    if(WadeDistance() < wadeCloseBy && timeSinceLastJump > jumpCoolDown)
                    {
                        if (DetectForwardGroundFar())
                        {
                            jumpForwards = true;
                        }
                        else
                        {
                            jumpForwards = false;
                        }
                        
                        TransitionToState(BozuState.Jump);
                    }

                    break;
                }
            case BozuState.Shooting:
                {
                    stateTimer += Time.deltaTime;
                    dropTimer += Time.deltaTime;
                    
                    if(dropTimer > shootFrequency)
                    {
                        Shoot("notflat");
                        dropTimer = 0;
                    }

                    if (stateTimer > shootTime)
                    {
                        TransitionToState(BozuState.Idle);
                    }

                    if (WadeDistance() < wadeCloseBy && timeSinceLastJump > jumpCoolDown)
                    {
                        TransitionToState(BozuState.Jump);
                    }

                    break;
                }
            case BozuState.Sweep:
                {
                    stateTimer += Time.deltaTime;
                    dropTimer += Time.deltaTime;

                    if(dropTimer > sweepShootFrequency)
                    {
                        Shoot("verticalaim");
                        dropTimer = 0;
                    }

                    if (stateTimer > sweepTime)
                    {
                        TransitionToState(BozuState.Idle);
                    }

                    if (WadeDistance() < wadeCloseBy && timeSinceLastJump > jumpCoolDown)
                    {
                        TransitionToState(BozuState.Jump);
                    }

                    break;
                }
            case BozuState.Jump:
                {
                    stateTimer += Time.deltaTime;
                    dropTimer += Time.deltaTime;

                    if (dropTimer > dropFrequency)
                    {
                        GrenadeThrow();
                        dropTimer = 0;
                    }

                    break;
                }
            case BozuState.Run:
                {
                    stateTimer += Time.deltaTime;


                    if (!DetectForwardGround())
                    {
                        TransitionToState(BozuState.Idle);
                    }

                    if(WadeDistance() < 5)
                    {
                        if (DetectForwardGroundFar())
                        {
                            jumpForwards = false;
                            TransitionToState(BozuState.Jump);
                        }
                        else
                        {
                            TransitionToState(BozuState.Shooting);
                        }

                        
                    }

                    if(stateTimer > runLength)
                    {
                        TransitionToState(BozuState.Idle);
                    }

                    

                    break;
                }
            case BozuState.Fall:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(BozuState.Idle);
                    }
                    break;
                }
        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        switch (CurrentBozuState)
        {
            case BozuState.Idle:
                {
                    currentRotation = Quaternion.Slerp(currentRotation, WadeLookRotation("flat"), turnSpeed * 2 * Time.deltaTime);
                    break;
                }
            case BozuState.Shooting:
                {
                    currentRotation = Quaternion.Slerp(currentRotation, WadeLookRotation("flat"), turnSpeed * 2 * Time.deltaTime);
                    break;
                }
            case BozuState.Sweep:
                {
                    currentRotation = Quaternion.Slerp(currentRotation, WadeLookRotation("flat"), turnSpeed * 1.5f * Time.deltaTime);
                    break;
                }
            case BozuState.Run:
                {
                  currentRotation = Quaternion.Slerp(currentRotation, Quaternion.Euler( 0 , WadeLookRotation("flat").eulerAngles.y + Random.Range(0, 6), 0), (turnSpeed/2) * Time.deltaTime);
                  break;
                }
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (CurrentBozuState)
        {
            case BozuState.Idle:
                {
                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(BozuState.Fall);
                    }
                     
                    moveSpeed = Mathf.Lerp(moveSpeed, 0, deceleration * Time.deltaTime);
                    currentVelocity = Motor.CharacterForward * moveSpeed;
                    break;
                }
            case BozuState.Shooting:
                {
                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(BozuState.Fall);
                    }

                    moveSpeed = Mathf.Lerp(moveSpeed, 0, 12 * Time.deltaTime);
                    currentVelocity = Motor.CharacterForward * moveSpeed;
                    break;
                }
            case BozuState.Sweep:
                {
                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(BozuState.Fall);
                    }

                    moveSpeed = Mathf.Lerp(moveSpeed, 0, 12 * Time.deltaTime);
                    currentVelocity = Motor.CharacterForward * moveSpeed;
                    break;
                }
            case BozuState.Run:
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, speed, acceleration * Time.deltaTime);

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(BozuState.Fall);
                    }

                    currentVelocity = Motor.CharacterForward * moveSpeed;

                    break;
                }
            case BozuState.Jump:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        {
                            TransitionToState(BozuState.Idle);
                        }
                    }

                    if (verticalMoveSpeed <= gravSwitchStart)
                    {
                        Gravity = Mathf.Lerp(Gravity, maxGravity, gravitySwitchRate * Time.deltaTime);
                    }

                    verticalMoveSpeed += Gravity * Time.deltaTime;

                    currentVelocity = Motor.CharacterForward * moveSpeed + Motor.CharacterUp * verticalMoveSpeed;

                    break;
                }
            case BozuState.Fall:
                {
                    verticalMoveSpeed += Gravity;

                    currentVelocity = Motor.CharacterForward * moveSpeed + Vector3.up * verticalMoveSpeed;
                    break;
                }
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }


    public void OnTriggerEnter(Collider other)
    {

    }

    public void OnTriggerExit(Collider other)
    {

    }

    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (IgnoredColliders.Count == 0)
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


    public void DoHealth()
    {
        Vector3 lockOnRotation = (wade.position - healthCanvas.transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
        healthCanvas.transform.rotation = Quaternion.Slerp(healthCanvas.transform.rotation, lookRotation, turnSpeed * 2 * Time.deltaTime);
        healthBar.fillAmount = (health / startHealth);
    }

    
}
