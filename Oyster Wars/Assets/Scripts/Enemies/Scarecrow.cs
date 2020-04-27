using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

public enum ScarecrowState
{
    Idle, Charge
}

public class Scarecrow : Enemy, ICharacterController
{
    public float scareCrowTurnSpeed;
    public float scareCrowMoveSpeed;
    public float scareCrowChargeSpeed;
    public float startChargingDistance;
    public float maxTurnDistance;
    public float ChargeTimer;
    public float ChargeLength;


    public KinematicCharacterMotor Motor;
    public ScarecrowState CurrentScarecrowState { get; private set; }
    public Transform wade;
    private void Start()
    {
        Motor.CharacterController = this;
        turnSpeed = scareCrowTurnSpeed;
        TransitionToState(ScarecrowState.Idle);
        wade = GameObject.Find("Wade").transform;
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

                    break;
                }
            case ScarecrowState.Charge:
                {
                    ChargeTimer = 0;

                    break;
                }
        }
    }

    public void OnStateExit(ScarecrowState state, ScarecrowState toState)
    {
        switch (state)
        {

        }
    }

    public void Update()
    {
        switch (CurrentScarecrowState)
        {
            case ScarecrowState.Idle:
                {

                    if (Vector3.Distance(transform.position, wade.position) < startChargingDistance)
                    {
                        TransitionToState(ScarecrowState.Charge);
                    }

                    break;
                }
            case ScarecrowState.Charge:
                {
                    ChargeTimer += Time.deltaTime;

                    if(ChargeTimer > ChargeLength)
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
                    if (Vector3.Distance(transform.position, wade.position) < maxTurnDistance)
                    {
                        Vector3 lockOnRotation = (wade.position - transform.position);
                        Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                        lookRotation.x = 0;
                        lookRotation.z = 0;
                        currentRotation = Quaternion.Slerp(currentRotation, lookRotation, turnSpeed * Time.deltaTime);
                    }

                    break;
                }
            case ScarecrowState.Charge:
                {

                    

                    Vector3 lockOnRotation = (wade.position - transform.position);
                    Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
                    lookRotation.x = 0;
                    lookRotation.z = 0;

                    if (Vector3.Distance(transform.position, wade.position) < startChargingDistance)
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
                    
                    if (Vector3.Distance(transform.position, wade.position) < startChargingDistance)
                    {
                        scareCrowMoveSpeed = Mathf.Lerp(scareCrowMoveSpeed, scareCrowChargeSpeed, 12 * Time.deltaTime);
                        
                    }
                    else
                    {
                        scareCrowMoveSpeed = Mathf.Lerp(scareCrowMoveSpeed, scareCrowChargeSpeed / 5, 12 * Time.deltaTime);
                    }

                    currentVelocity = Motor.CharacterForward * scareCrowMoveSpeed;

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
        //if (IgnoredColliders.Count == 0)
        //{
        //    return true;
        //}

        //if (IgnoredColliders.Contains(coll))
        //{
        //    return false;
        //}

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
