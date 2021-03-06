﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WadeMachine : MonoBehaviour
{

  

    public class JumpProfile
    {
        public bool CanSlash = true;
        public bool CanPogo = true;
        public float AirSpeed = 8;
        public bool CanControlHeight = false;
        public float JumpHeight;
        public float InitialForwardVelocity;
        public float timeToJumpApex = 0.5f;
        public float Acceleration = 8;
        public string Animation;
        public string FallAnimation;
        public float CrossFadeTime;
        public float gravityMultiplier = 1.8f;
        public bool inAir;
        public int planarMultiplier = 0;
    }

    public JumpProfile currentJumpProfile;

    private JumpProfile jumpStandard = new JumpProfile
    {
        Animation = "Jump",
        CanControlHeight = true,
        JumpHeight = 4.5f,
        timeToJumpApex = .463f,
    };
    
    
    private JumpProfile dashLongJump = new JumpProfile
    {
        Animation = "DashLongJump",
        CanControlHeight = true,
        JumpHeight = 4.5f,
        timeToJumpApex = .463f,
    };

    private JumpProfile boatLong = new JumpProfile
    {
        Animation = "DashLongJump",
        CanControlHeight = true,
        JumpHeight = 4.5f,
        timeToJumpApex = .463f,
        InitialForwardVelocity = 17,
    };



    private JumpProfile jumpWall = new JumpProfile
    {
        JumpHeight = 3.4f,
        InitialForwardVelocity = 4.4f,
        timeToJumpApex = .4f,
        Animation = "WallJump"
    };

    private JumpProfile sparkSlam = new JumpProfile
    {
        JumpHeight = 4.4f,
        InitialForwardVelocity = 2.4f,
        timeToJumpApex = .5f,
        Animation = "SparkSlam",
        gravityMultiplier = 6,
    };


    private JumpProfile frontFlip = new JumpProfile
    {
        CanControlHeight = true,
        JumpHeight = 4.5f,
        InitialForwardVelocity = 3.0f,
        AirSpeed = 10,
        timeToJumpApex = .45f,
        Animation = "FrontFlip"

    };

    private JumpProfile airFall = new JumpProfile
    {
        JumpHeight = 0,
        timeToJumpApex = 0,
        InitialForwardVelocity = 0,
        Animation = "Fall",
        inAir = true,
        planarMultiplier = 1,
    };
    private JumpProfile groundFall = new JumpProfile
    {
        JumpHeight = 0,
        timeToJumpApex = 0,
        InitialForwardVelocity = 0,
        Animation = "Fall",
    };

    public class AirProfile
    {
        public string Animation;
        public float DashLength = 0;

    }

    public AirProfile currentAirProfile;

    private AirProfile slashProfile = new AirProfile
    {
        Animation = "AirSlash"
    };

    public void DoUI()
    {
        healthBar.fillAmount = (health / startHealth);
    }
    private void DoUpdateVariables()
    {
        if (takeBoatVelocity)
        {
            boatRelativeJumpMove = boat.GetComponent<WadeBoatController>().Motor.Velocity;
        }
        else
        {
            boatRelativeJumpMove = Vector3.zero;
        }

        if (spawnInBoat)
        {
            lastStablePosition = boat.transform.position;
        }

        pearlsCount.text = pearls.ToString();

        cubeTarget = camObject.GetComponent<WadeCamera>().lockOnInstance.transform;

        blendAngle = Vector3.Angle(transform.forward, LocalMovement()) * input.Current.MoveInput.x;

        angle = Vector3.Angle(transform.forward, LocalMovement());

        stateString = CurrentWadeState.ToString();

        wadeVelocity = Motor.Velocity;
    }
    private void DoFollowBlock()
    {
        float jumpPosY;

        if (Mathf.Abs(transform.position.y - camFollow.position.y) > verticalMovementMax || (verticalMoveSpeed < 0 && Mathf.Abs(transform.position.y - camFollow.position.y) > verticalMovementMax))
        {
            jumpPosY = Mathf.Lerp(camFollow.position.y, transform.position.y, followRate * Time.deltaTime);
        }
        else
        {
            jumpPosY = Mathf.Lerp(camFollow.position.y, transform.position.y, followRate * 2 * Time.deltaTime);
        }

        float jumpPosZ = Mathf.Lerp(camFollow.position.z, transform.position.z, followRate * Time.deltaTime);

        float jumpPosX = Mathf.Lerp(camFollow.position.x, transform.position.x, followRate * Time.deltaTime);



        camFollow.position = new Vector3(jumpPosX, jumpPosY, jumpPosZ);
        camFollow.rotation = transform.rotation;
    }

    public void ChangeWeapon(int bulletNum)
    {
        bullet = bullets[bulletNum];
    }

    public Vector3 LocalMovement()
    {
        Vector3 intent;

        camF = cam.forward;
        camR = cam.right;

        camF.y = 0;
        camR.y = 0;

        camF = camF.normalized;
        camR = camR.normalized;



        intent = (camF * input.Current.MoveInput.z + camR * input.Current.MoveInput.x);

        return intent;
    }


    public void DoTakeDamage(float damage)
    {
        

        if (CurrentWadeState != WadeState.Hit && CurrentWadeState != WadeState.Death)
        {
            if (!invincible)
            {
                health -= damage;
                if (health > 0)
                {
                    TransitionToState(WadeState.Hit);
                }
                else
                {
                    TransitionToState(WadeState.Death);
                }
            }
        }       
    }


    public void DoInvincibility()
    {
        if (invincibilityTimer > invincibilityTime)
        {
            invincible = false;
        }
        else
        {
            invincible = true;
        }

        if (invincible)
        {

            if (flashTimer >= flashTime)
            {
                PlayerSkinnedMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
            else
            {
                PlayerSkinnedMesh.GetComponent<SkinnedMeshRenderer>().enabled = false;
            }

            if (flashTimer >= flashTime * 2)
            {
                flashTimer = 0;
            }
        }
        else
        {
            PlayerSkinnedMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
    }

    void ShootControls()
    {
        if (aimUp)
        {
            aimHeight = aimHeightDown;
        }
        else if(currentJumpProfile == frontFlip && CurrentWadeState == WadeState.Jump)
        {
            aimHeight = aimHeightUp;
        }
        else
        {
            aimHeight = 0;
        }

        if (Input.GetAxisRaw("RightTrigger") != 0)
        {
            if (triggerInUse == false)
            {
                if (canShoot)
                {
                    Instantiate(bullet, gun.position + Vector3.up * aimHeight, gun.rotation);
                    shotTimer = 0;
                    triggerInUse = true;
                    wadeSound.PlayRifleShot();
                }
            }
        }
        if (Input.GetAxisRaw("RightTrigger") == 0)
        {
            triggerInUse = false;
        }

        
    }

    void DoShooting()
    {
        shotTimer += Time.deltaTime;

        if (boltActionTime > shotTimer)
        {
            canShoot = false;
        }
        else
        {
            canShoot = true;
        }


        


        if (aiming)
        {
            correctedWalkSpeed = Mathf.Lerp(correctedWalkSpeed, aimingWalkSpeed, walkSpeedsTransitionTime * Time.deltaTime);
        }
        else
        {
            correctedWalkSpeed = Mathf.Lerp(correctedWalkSpeed, walkSpeed, walkSpeedsTransitionTime * Time.deltaTime);
        }

        if(camObject.GetComponent<WadeCamera>().enemiesOnScreen.Count == 0)
        {
            canLockOn = false;
            lockedOn = false;
        }
        else
        {
            canLockOn = true;
        }

            if (stateString == "Jump" && currentJumpProfile == frontFlip)
            {
                gun.rotation = Quaternion.Euler(transform.localRotation.eulerAngles.x + 45, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);

            }
            else if (aimUp)
            {
                gun.rotation = Quaternion.Euler(transform.eulerAngles.x - 35, transform.eulerAngles.y, transform.eulerAngles.z) ;
            }
            else
            {
                gun.rotation = transform.rotation;
            }

    }

    void DoAiming()
    {
        if (Input.GetAxisRaw("LeftTrigger") != 0)
        {
            if (CurrentWadeState == WadeState.Idle || CurrentWadeState == WadeState.Walk)
            {
                PlayerAnimator.SetBool("Aim", true);
                aiming = true;
            }
            else
            {
                PlayerAnimator.SetBool("Aim", false);
                aiming = false;
            }
        }
        if (Input.GetAxisRaw("LeftTrigger") == 0)
        {
            PlayerAnimator.SetBool("Aim", false);
            aiming = false;
        }
    }
}



