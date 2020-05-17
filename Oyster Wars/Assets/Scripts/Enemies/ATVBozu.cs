using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using UnityEditor;
public enum ATVBozuState
{
    Idle, Charge, Fall, Shooting, Jump, Sweep, TailWhip, Circle, CurveApproach, TailWhipCoolDown
}

public class ATVBozu : Enemy, ICharacterController
{
    public float driftDeceleration;
    public float bozuTurnSpeed;
    public float bozuChargeSpeed;
    public float chargeTurnSpeed;
    public float startChargingDistance;
    public float startShootingDistance;
    public float maxTurnDistance;
    public float stateTimer;
    public float runLength;
    public float dropTimer;
    public float dropFrequency;
    public bool hasDropped;
    public float Gravity;
    public float idleWaitTime;
    public float endChargeTime;
    public float circleWidth;
    public float shootTime;
    public float shootFrequency;
    public float sweepTime;
    public float sweepShootFrequency;
    public float sweepVerticalDistanceMax;
    public float driftFactor;
    public float tailWhipStartDistance;
    public float tailWhipSpeed;
    public float tailWhipWidth;
    public float tailWhipActiveWidth;
    public float tailWhipTime;
    public float tailWhipMoveSpeed;
    public float curveApproachAngle;
    public float curveApproachDivision;
    public float activeDrift;
    public float driftIncrease;
    public bool hasCharged;

    public GameObject characterMesh;
    public Animator animator;

    public KinematicCharacterMotor Motor;
    public AudioSource motor;
    public ATVBozuState CurrentATVBozuState { get; private set; }
    private void Start()
    {
        hasCharged = false;
        cam = GameObject.Find("Camera").GetComponent<Camera>();
        wadeCamera = GameObject.Find("Camera").GetComponent<WadeCamera>();
        wadeMachine = GameObject.Find("Wade").GetComponent<WadeMachine>();
        Motor.CharacterController = this;
        turnSpeed = bozuTurnSpeed;
        TransitionToState(ATVBozuState.Idle);
        health = startHealth;
        motor = GetComponent<AudioSource>();
    }


    public void TransitionToState(ATVBozuState newState)
    {
        ATVBozuState tmpInitialState = CurrentATVBozuState;
        OnStateExit(tmpInitialState, newState);
        CurrentATVBozuState = newState;
        OnStateEnter(newState, tmpInitialState);
    }


    public void OnStateEnter(ATVBozuState state, ATVBozuState fromState)
    {
        stateTimer = 0;
        dropTimer = 0;

        switch (state)
        {
            case ATVBozuState.Idle:
                {
                    //animator.SetBool("Idle", true);

                    break;
                }
            case ATVBozuState.Shooting:
                {
                    Shoot("notflat");
                    animator.SetBool("Shoot", true);


                    break;
                }
            case ATVBozuState.Sweep:
                {

                    animator.SetBool("Sweep", true);
                    break;
                }
            case ATVBozuState.Jump:
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

                    animator.SetBool("Jump", true);

                    break;
                }
            case ATVBozuState.Charge:
                {
                    animator.SetBool("Charge", true);

                    break;
                }
            case ATVBozuState.CurveApproach:
                {
                    animator.SetBool("Turn", true);

                    break;
                }
            case ATVBozuState.Circle:
                {
                    animator.SetBool("Turn", true);

                    break;
                }
            case ATVBozuState.TailWhip:
                {
                    animator.SetBool("Whip", true);
                    activeDrift = 0;
                    tailWhipActiveWidth = curveApproachAngle;

                    break;
                }
            case ATVBozuState.TailWhipCoolDown:
                {
                    activeDrift = 0;
                    tailWhipActiveWidth = transform.rotation.eulerAngles.y;
                    break;
                } 
            case ATVBozuState.Fall:
                {
                    verticalMoveSpeed = 8;
                    stateTimer = 0;
                    break;
                }
        }
    }

    public void OnStateExit(ATVBozuState state, ATVBozuState toState)
    {
        switch (state)
        {
            case ATVBozuState.Idle:
                {
                    //animator.SetBool("Idle", false);
                    break;
                }
            case ATVBozuState.Shooting:
                {
                    animator.SetBool("Shoot", false);
                    break;
                }
            case ATVBozuState.Sweep:
                {
                    animator.SetBool("Sweep", false);
                    break;
                }
            case ATVBozuState.Jump:
                {
                    timeSinceLastJump = 0;
                    verticalMoveSpeed = 0;
                    animator.SetBool("Jump", false);
                    break;
                }
            case ATVBozuState.Charge:
                {
                    animator.SetBool("Charge", false);
                    break;
                }
            case ATVBozuState.CurveApproach:
                {
                    animator.SetBool("Turn", false);

                    break;
                }
            case ATVBozuState.Circle:
                {
                    animator.SetBool("Turn", false);

                    break;
                }
            case ATVBozuState.TailWhip:
                {
                    activeDrift = 0;
                    tailWhipActiveWidth = curveApproachAngle;
                    break;
                }
            case ATVBozuState.TailWhipCoolDown:
                {
                    animator.SetBool("Whip", false);
                    
                    tailWhipActiveWidth = curveApproachAngle;
                    break;
                }
            case ATVBozuState.Fall:
                {
                    verticalMoveSpeed = 0;
                    break;
                }
        }
    }

    public void Update()
    {
        if (WadeDistance() < 150)
        {
            motor.volume = moveSpeed / speed - .4f;
            motor.pitch = moveSpeed / speed + .5f;
        }
        

        curveApproachAngle = WadeDistance() / curveApproachDivision;
        stateString = CurrentATVBozuState.ToString();
        timeSinceLastJump += Time.deltaTime;
        DoTargeting();
        DoHealth();

        groundFar = DetectForwardGroundFar();
        groundFront = DetectForwardGround();

        stateTimer += Time.deltaTime;
        dropTimer += Time.deltaTime;

        switch (CurrentATVBozuState)
        {
            case ATVBozuState.Idle:
                {
                    
                    
                    if(WadeDistance() < 150)
                    {
                        if (!hasCharged)
                        {
                            hasCharged = true;
                            TransitionToState(ATVBozuState.Charge);
                        }
                        else if (stateTimer > idleWaitTime && WadeDistance() > startChargingDistance)
                        {
                            int randomStateChange = RandomInt(1, 2);
                            switch (randomStateChange)
                            {
                                case 1:
                                    {
                                        TransitionToState(ATVBozuState.Circle);
                                        break;
                                    }
                                case 2:
                                    {
                                        TransitionToState(ATVBozuState.CurveApproach);
                                        break;
                                    }
                            }
                        }
                        else if (stateTimer > idleWaitTime)
                        {
                            TransitionToState(ATVBozuState.Charge);
                        }
                    }
                    break;
                }
            case ATVBozuState.Shooting:
                {
                    
                    if(dropTimer > shootFrequency)
                    {
                        Shoot("notflat");
                        dropTimer = 0;
                    }

                    if (stateTimer > shootTime)
                    {
                        TransitionToState(ATVBozuState.Idle);
                    }

                    if (WadeDistance() < wadeCloseBy && timeSinceLastJump > jumpCoolDown)
                    {
                        TransitionToState(ATVBozuState.Jump);
                    }

                    break;
                }

            case ATVBozuState.Charge:
                {
                    if (DetectForwardWall())
                    {
                        TransitionToState(ATVBozuState.Idle);
                    }

                    if (dropTimer > dropFrequency && WadeDistance() < startChargingDistance - 8)
                    {
                        GameObject tempGrenade = Instantiate(grenade, transform.position + -transform.forward * 3 + Vector3.up, transform.rotation) as GameObject;
                        CapsuleCollider grenadeCollider = tempGrenade.GetComponent<CapsuleCollider>();
                        Grenade grenadeSc = tempGrenade.GetComponent<Grenade>();
                        grenadeSc.IgnoreCollisions(GetComponent<Collider>());
                        grenadeSc.enemyGrenade = true;
                        IgnoredColliders.Add(grenadeCollider);
                        Rigidbody tempGrenadeRB = tempGrenade.GetComponent<Rigidbody>();
                        tempGrenadeRB.AddForce(new Vector3(Random.Range(-26f, 26f), 0, Random.Range(-26f, 26f)) * throwForceDown, ForceMode.Impulse);
                        //tempGrenadeRB.AddForce(Vector3.up * throwForceDown, ForceMode.Impulse);
                        dropTimer = 0;
                    }



                    //if (!DetectForwardGround())
                    //{
                    //    TransitionToState(ATVBozuState.Idle);
                    //}

                    if(stateTimer > runLength)
                    {
                        TransitionToState(ATVBozuState.Idle);
                    }

                    

                    break;
                }
            case ATVBozuState.Fall:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(ATVBozuState.Idle);
                    }
                    break;
                }
            case ATVBozuState.CurveApproach:
                {
                    if(WadeDistance() < tailWhipStartDistance)
                    {
                        TransitionToState(ATVBozuState.TailWhip);
                    }
                    break;
                }
            case ATVBozuState.TailWhip:
                {
                    if (stateTimer > tailWhipTime)
                    {
                        TransitionToState(ATVBozuState.TailWhipCoolDown);
                    }
                    break;
                }
            case ATVBozuState.TailWhipCoolDown:
                {
                    if (stateTimer > tailWhipTime + 2)
                    {
                        TransitionToState(ATVBozuState.Idle);
                    }
                    break;
                }
            case ATVBozuState.Circle:
                {
                    if(dropTimer > dropFrequency * 10)
                    {
                        animator.SetBool("Toss", true);
                        GrenadeThrow();
                        dropTimer = 0;
                    }
                    else
                    {
                        animator.SetBool("Toss", false);
                    }

                    

                    if (stateTimer > runLength * 2)
                    {
                        TransitionToState(ATVBozuState.Idle);
                    }
                    break;
                }
        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        switch (CurrentATVBozuState)
        {
            case ATVBozuState.Idle:
                {
                    currentRotation = Quaternion.Slerp(currentRotation, WadeLookRotation("flat"), turnSpeed / 2 * Time.deltaTime);
                    break;
                }
            case ATVBozuState.Shooting:
                {
                    currentRotation = Quaternion.Slerp(currentRotation, WadeLookRotation("flat"), turnSpeed * 2 * Time.deltaTime);
                    break;
                }
            case ATVBozuState.Sweep:
                {
                    currentRotation = Quaternion.Slerp(currentRotation, WadeLookRotation("flat"), turnSpeed * 1.5f * Time.deltaTime);
                    break;
                }
            case ATVBozuState.CurveApproach:
                {
                    Quaternion wadeRight = Quaternion.Euler(WadeLookRotation("flat").eulerAngles.x, WadeLookRotation("flat").eulerAngles.y + curveApproachAngle, WadeLookRotation("flat").eulerAngles.z);

                    currentRotation = Quaternion.Slerp(currentRotation, wadeRight , turnSpeed * Time.deltaTime);
                    break;
                }
            case ATVBozuState.Circle:
                {

                    Quaternion wadeRight = Quaternion.Euler(WadeLookRotation("flat").eulerAngles.x, WadeLookRotation("flat").eulerAngles.y + circleWidth, WadeLookRotation("flat").eulerAngles.z);


                    currentRotation = Quaternion.Slerp(currentRotation, wadeRight, turnSpeed * 2 * Time.deltaTime);
                    break;
                }
            case ATVBozuState.TailWhip:
                {
                    
                        tailWhipActiveWidth -= tailWhipSpeed * deltaTime;
                    

                    Quaternion wadeRight = Quaternion.Euler(WadeLookRotation("flat").eulerAngles.x, WadeLookRotation("flat").eulerAngles.y + tailWhipActiveWidth, WadeLookRotation("flat").eulerAngles.z);

                    currentRotation = Quaternion.Slerp(currentRotation, wadeRight, turnSpeed * Time.deltaTime);

                    break;
                }
            case ATVBozuState.TailWhipCoolDown:
                {

                    tailWhipActiveWidth -= tailWhipSpeed * deltaTime;


                    Quaternion wadeRight = Quaternion.Euler(WadeLookRotation("flat").eulerAngles.x, tailWhipActiveWidth, WadeLookRotation("flat").eulerAngles.z);

                    currentRotation = Quaternion.Slerp(currentRotation, wadeRight, turnSpeed * Time.deltaTime);

                    break;
                }
            case ATVBozuState.Charge:
                {

                    currentRotation = Quaternion.Slerp(currentRotation, WadeLookRotation("flat"), chargeTurnSpeed * Time.deltaTime) ;


                    break;
                }
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (CurrentATVBozuState)
        {
            case ATVBozuState.Idle:
                {
                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(ATVBozuState.Fall);
                    }

                    activeDrift = Mathf.Lerp(activeDrift, 0, driftDeceleration * Time.deltaTime);
                    moveSpeed = Mathf.Lerp(moveSpeed, 0, deceleration * Time.deltaTime);
                    currentVelocity = Motor.CharacterForward * moveSpeed + Motor.CharacterRight * activeDrift;
                    break;
                }
            case ATVBozuState.Charge:
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, speed, acceleration * Time.deltaTime);


                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        verticalMoveSpeed += Gravity * Time.deltaTime;

                    }
                    else
                    {
                        verticalMoveSpeed = 0;
                    }

                    currentVelocity = Motor.CharacterForward * moveSpeed + Vector3.up * verticalMoveSpeed;

                    break;
                }
            case ATVBozuState.TailWhip:
                {

                    activeDrift = Mathf.Lerp(activeDrift, driftFactor, driftIncrease * Time.deltaTime);
                    
                    
                    

                    moveSpeed = Mathf.Lerp(moveSpeed, speed, acceleration * Time.deltaTime);

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(ATVBozuState.Fall);
                    }

                    

                    currentVelocity = Motor.CharacterForward * tailWhipMoveSpeed + Motor.CharacterRight * activeDrift;

                    break;
                }
            case ATVBozuState.TailWhipCoolDown:
                {

                    activeDrift = Mathf.Lerp(activeDrift, driftFactor, driftIncrease * Time.deltaTime);




                    moveSpeed = Mathf.Lerp(moveSpeed, speed, acceleration * Time.deltaTime);

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(ATVBozuState.Fall);
                    }



                    currentVelocity = Motor.CharacterForward * tailWhipMoveSpeed + Motor.CharacterRight * activeDrift;

                    break;
                }
            case ATVBozuState.Circle:
                {

                    activeDrift = Mathf.Lerp(activeDrift, driftFactor /2, driftIncrease * Time.deltaTime);


                    moveSpeed = Mathf.Lerp(moveSpeed, speed, acceleration * Time.deltaTime);

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(ATVBozuState.Fall);
                    }



                    currentVelocity = Motor.CharacterForward * tailWhipMoveSpeed / 1.5f + Motor.CharacterRight * (activeDrift / 2);

                    break;
                }

            case ATVBozuState.CurveApproach:
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, speed, acceleration * Time.deltaTime);

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(ATVBozuState.Fall);
                    }

                    currentVelocity = Motor.CharacterForward * moveSpeed;

                    break;
                }
            case ATVBozuState.Fall:
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
