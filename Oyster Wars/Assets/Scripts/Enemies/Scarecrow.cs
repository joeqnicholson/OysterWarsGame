using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

public enum ScarecrowState
{
    Idle, Charge, Fall, Shooting
}

public class Scarecrow : Enemy, ICharacterController
{
    public float scareCrowTurnSpeed;
    public float scareCrowMoveSpeed;
    public float scareCrowChargeSpeed;
    public float startChargingDistance;
    public float maxTurnDistance;
    public float stateTimer;
    public float ChargeLength;
    public float dropTimer;
    public float dropFrequency;
    public bool hasDropped;
    public float Gravity;
    public float verticalMoveSpeed;
    public float idleWaitTime;
    public float endChargeTime;
    public float wadeDistance;
    public GameObject characterMesh;
    public Animator animator;

    public KinematicCharacterMotor Motor;
    public ScarecrowState CurrentScarecrowState { get; private set; }
    public Transform wade;
    public GameObject grenade;
    public GameObject bullet;
    private void Start()
    {
        cam = GameObject.Find("Camera").GetComponent<Camera>();
        wadeCamera = GameObject.Find("Camera").GetComponent<WadeCamera>();
        
        Motor.CharacterController = this;
        turnSpeed = scareCrowTurnSpeed;
        TransitionToState(ScarecrowState.Shooting);
        health = startHealth;
    }


    public void TransitionToState(ScarecrowState newState)
    {
        ScarecrowState tmpInitialState = CurrentScarecrowState;
        OnStateExit(tmpInitialState, newState);
        CurrentScarecrowState = newState;
        OnStateEnter(newState, tmpInitialState);
    }


    public void OnStateEnter(ScarecrowState state, ScarecrowState fromState)
    {
        switch (state)
        {
            case ScarecrowState.Idle:
                {
                    animator.SetBool("Idle", true);
                    dropTimer = 0;
                    stateTimer = 0;
                    break;
                }
            case ScarecrowState.Shooting:
                {

                    Vector3 lockOnRotation = (wade.position - transform.position);
                    Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);

                    if(wadeDistance < startChargingDistance + 6)
                    {
                        GameObject tempBullet = Instantiate(bullet, transform.position + transform.forward * 4, lookRotation) as GameObject;
                    }
                    

                    animator.SetBool("Shoot", true);
                    dropTimer = 0;
                    stateTimer = 0;

                    break;
                }
            case ScarecrowState.Charge:
                {
                    hasDropped = false;
                    stateTimer = 0;

                    break;
                }
            case ScarecrowState.Fall:
                {
                    stateTimer = 0;
                    break;
                }
        }
    }

    public void OnStateExit(ScarecrowState state, ScarecrowState toState)
    {
        switch (state)
        {
            case ScarecrowState.Idle:
                {
                    animator.SetBool("Idle", false);
                    break;
                }
            case ScarecrowState.Shooting:
                {
                    animator.SetBool("Shoot", false);
                    break;
                }
            case ScarecrowState.Fall:
                {
                    verticalMoveSpeed = 0;
                    break;
                }
        }
    }

    public void Update()
    {
        wadeDistance = Vector3.Distance(transform.position, wade.position);

        DoTargeting();
        DoHealth();
        

        switch (CurrentScarecrowState)
        {
            case ScarecrowState.Idle:
                {
                    stateTimer += Time.deltaTime;
                    dropTimer += Time.deltaTime;
                    Vector3 lockOnRotation = (wade.position - transform.position);
                    Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                    lookRotation.x = 0;
                    lookRotation.z = 0;

                    if (dropTimer > dropFrequency * 3f && wadeDistance < startChargingDistance)
                    {
                        GameObject tempGrenade = Instantiate(grenade, transform.position + transform.forward + Vector3.up, lookRotation) as GameObject;
                        CapsuleCollider grenadeCollider = tempGrenade.GetComponent<CapsuleCollider>();
                        Grenade grenadeSc = tempGrenade.GetComponent<Grenade>();
                        grenadeSc.IgnoreCollisions(GetComponent<Collider>());
                        grenadeSc.enemyGrenade = true;
                        IgnoredColliders.Add(grenadeCollider);
                        Rigidbody tempGrenadeRB = tempGrenade.GetComponent<Rigidbody>();
                        tempGrenadeRB.AddForce(tempGrenade.transform.forward * (wadeDistance - 8), ForceMode.Impulse);
                        dropTimer = 0;
                    }

                    if (stateTimer > idleWaitTime)
                    {
                        TransitionToState(ScarecrowState.Shooting);
                    }
                    
                    break;
                }
            case ScarecrowState.Shooting:
                {
                    stateTimer += Time.deltaTime;
                    dropTimer += Time.deltaTime;
                    

                    if (stateTimer > idleWaitTime)
                    {
                        TransitionToState(ScarecrowState.Idle);
                    }

                    break;
                }
            case ScarecrowState.Charge:
                {
                    stateTimer += Time.deltaTime;
                    dropTimer += Time.deltaTime;

                    if(stateTimer > ChargeLength)
                    {
                        TransitionToState(ScarecrowState.Idle);
                    }

                    if (dropTimer > dropFrequency && Vector3.Distance(transform.position, wade.position) < startChargingDistance - 8)
                    {
                        
                        
                        GameObject tempGrenade = Instantiate(grenade, transform.position + transform.forward + Vector3.up, transform.rotation) as GameObject;
                        CapsuleCollider grenadeCollider = tempGrenade.GetComponent<CapsuleCollider>();
                        Grenade grenadeSc = tempGrenade.GetComponent<Grenade>();
                        grenadeSc.IgnoreCollisions(GetComponent<Collider>());
                        grenadeSc.enemyGrenade = true;
                        IgnoredColliders.Add(grenadeCollider);
                        Rigidbody tempGrenadeRB = tempGrenade.GetComponent<Rigidbody>();
                        tempGrenadeRB.AddForce(new Vector3(Random.Range(-12f,12f),0, Random.Range(-12f, 12f)) * throwForceDown, ForceMode.Impulse);
                        dropTimer = 0;
                    }

                    break;
                }
            case ScarecrowState.Fall:
                {
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(ScarecrowState.Idle);
                    }
                    break;
                }
        }
    }

   
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        switch (CurrentScarecrowState)
        {
            case ScarecrowState.Idle:
                {
                    
                        Vector3 lockOnRotation = (wade.position - transform.position);
                        Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                        lookRotation.x = 0;
                        lookRotation.z = 0;
                        currentRotation = Quaternion.Slerp(currentRotation, lookRotation, turnSpeed * 2 * Time.deltaTime);


                    break;
                }
            case ScarecrowState.Shooting:
                {

                    Vector3 lockOnRotation = (wade.position - transform.position);
                    Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                    lookRotation.x = 0;
                    lookRotation.z = 0;
                    currentRotation = Quaternion.Slerp(currentRotation, lookRotation, turnSpeed * 2 * Time.deltaTime);


                    break;
                }
            case ScarecrowState.Charge:
                {

                    Vector3 lockOnRotation = (wade.position - transform.position);
                    Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                    lookRotation.x = 0;
                    lookRotation.z = 0;

                    if (Vector3.Distance(transform.position, wade.position) < startChargingDistance + 5)
                    {
                        currentRotation = Quaternion.Slerp(currentRotation, lookRotation, turnSpeed / 10 * Time.deltaTime);
                    }
                    else
                    {
                        currentRotation = Quaternion.Slerp(currentRotation, lookRotation, turnSpeed / 3 * Time.deltaTime);
                    }

                    

                    break;
                }
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (CurrentScarecrowState)
        {
            case ScarecrowState.Idle:
                {
                    scareCrowMoveSpeed = Mathf.Lerp(scareCrowMoveSpeed, 0, 12 * Time.deltaTime);
                    currentVelocity = Motor.CharacterForward * scareCrowMoveSpeed;
                    break;
                }
             case ScarecrowState.Charge:
                {

                    if (stateTimer < endChargeTime)
                    {
                        scareCrowMoveSpeed = Mathf.Lerp(scareCrowMoveSpeed, scareCrowChargeSpeed, 12 * Time.deltaTime);
                        
                    }
                    else
                    {
                        scareCrowMoveSpeed = Mathf.Lerp(scareCrowMoveSpeed, scareCrowChargeSpeed / 5, 12 * Time.deltaTime);
                    }

                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        TransitionToState(ScarecrowState.Fall);
                    }

                    currentVelocity = Motor.CharacterForward * scareCrowMoveSpeed;

                    break;
                }
            case ScarecrowState.Fall:
                {
                    verticalMoveSpeed -= Gravity;

                    currentVelocity = Motor.CharacterForward * scareCrowMoveSpeed + Vector3.up * verticalMoveSpeed;
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
