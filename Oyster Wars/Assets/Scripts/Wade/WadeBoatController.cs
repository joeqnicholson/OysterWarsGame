using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;

public enum WadeBoatState
{
    Move, Idle
}

//[RequireComponent(typeof(WadeInputs))]  
public class WadeBoatController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;
    public bool controlled;
    public float accelerationInput;
    public float acceleration;
    public float deceleration;
    public float engineSpeed;
    public float moveSpeed;
    public float turnSpeed;
    public float turnAcceleration;
    public float boatYrotation;
    public float turnInput;
    public string stateString;
    public float leanFactor;
    public float leanSpeed;
    public float boatZrotation;
    public float boatXrotation;
    public Vector3 currentVelocity;
    public WadeBoatState CurrentWadeBoatState { get; private set; }

    public List<Collider> IgnoredColliders = new List<Collider>();
   

    private void Start()
    {
        TransitionToState(WadeBoatState.Idle);
        Motor.CharacterController = this;
        
    }

    public void TransitionToState(WadeBoatState newState)
    {
        WadeBoatState tmpInitialState = CurrentWadeBoatState;
        OnStateExit(tmpInitialState, newState);
        CurrentWadeBoatState = newState;
        OnStateEnter(newState, tmpInitialState);
    }



    /// <summary>
    /// Event when entering a state
    /// </summary>
    public void OnStateEnter(WadeBoatState state, WadeBoatState fromState)
    {
        switch (CurrentWadeBoatState)
        {
            case WadeBoatState.Idle:
                {
                   
                    break;
                }

            case WadeBoatState.Move:
                {
                    break;
                }
        }
    }

    /// <summary>
    /// Event when exiting a state
    /// </summary>
    public void OnStateExit(WadeBoatState state, WadeBoatState toState)
    {
       
    }

    public void Update()
    {
        currentVelocity = Motor.Velocity;
        stateString = CurrentWadeBoatState.ToString();


        if (controlled)
        {
            accelerationInput = Input.GetAxisRaw("RightTrigger");
            turnInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            turnInput = 0;
            accelerationInput = 0;
        }
        

    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>

    public void OnTriggerEnter(Collider other)
    {
       
    }

    public void OnTriggerExit(Collider other)
    {
      
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
        switch (CurrentWadeBoatState)
        {
            case WadeBoatState.Idle:
                {
                    boatZrotation = Mathf.Lerp(boatZrotation, -turnInput * leanFactor, leanSpeed * deltaTime);
                    boatXrotation = Mathf.Lerp(boatXrotation, -accelerationInput * leanFactor / 2, leanSpeed * deltaTime);
                    boatYrotation += turnInput * Mathf.Lerp( 0, turnSpeed * 1.6f, turnAcceleration * deltaTime);

                    Quaternion desiredRotation = Quaternion.Euler(boatXrotation, boatYrotation, boatZrotation);
                    currentRotation = Quaternion.Slerp(currentRotation, desiredRotation, turnAcceleration * deltaTime);
                    break;
                }
                

            case WadeBoatState.Move:
                {
                    boatZrotation = Mathf.Lerp(boatZrotation, -turnInput * leanFactor, leanSpeed * deltaTime);
                    boatXrotation = Mathf.Lerp(boatXrotation, -accelerationInput * leanFactor / 2, (leanSpeed / 2) * deltaTime);
                    boatYrotation += turnInput * Mathf.Lerp(0, turnSpeed, turnAcceleration * deltaTime);

                    Quaternion desiredRotation = Quaternion.Euler(boatXrotation, boatYrotation, boatZrotation);
                    currentRotation = Quaternion.Slerp(currentRotation, desiredRotation, turnAcceleration * deltaTime);
                    break;
                }
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (CurrentWadeBoatState)
        {
            case WadeBoatState.Idle:
                {
                    if(accelerationInput > 0)
                    {
                        TransitionToState(WadeBoatState.Move);
                    }

                    Vector3 moveDirection = new Vector3(transform.forward.x, 0, transform.forward.z);

                    moveSpeed = Mathf.Lerp(moveSpeed, 0, deceleration * deltaTime);
                    currentVelocity = Vector3.Lerp(currentVelocity, moveDirection * moveSpeed, 10 *Time.deltaTime);
                    break;
                }

            case WadeBoatState.Move:
                {
                    if (accelerationInput == 0)
                    {
                        TransitionToState(WadeBoatState.Idle);
                    }

                    Vector3 moveDirection = new Vector3(transform.forward.x, 0, transform.forward.z);

                    moveSpeed = Mathf.Lerp(moveSpeed, engineSpeed, acceleration * deltaTime);
                    currentVelocity = Vector3.Lerp(currentVelocity, moveDirection * engineSpeed, acceleration * Time.deltaTime);
                    break;
                }
        }

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

}
