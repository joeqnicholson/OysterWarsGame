using System.Collections;
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
        JumpHeight = 4f,
        timeToJumpApex = .4f,
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
        ammoBar.fillAmount = (bullets / clipSize);
    }

    private void FollowBlock()
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


    private float lockOnAngles()
    {


        camF = cam.forward;
        camR = cam.right;

        camF.y = 0;
        camR.y = 0;

        camF = camF.normalized;
        camR = camR.normalized;


        return Vector3.SignedAngle(gun.forward, Motor.CharacterForward, camF);

    }

    public void DoTakeDamage(float damage)
    {
        if (CurrentWadeState != WadeState.Hit && CurrentWadeState != WadeState.Death)
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

    void ShootControls()
    {
        if (Input.GetAxisRaw("RightTrigger") != 0)
        {
            if (triggerInUse == false)
            {
                if (canShoot)
                {
                    Instantiate(bullet, gun.position, gun.rotation);
                    bullets -= 1;
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

        if (bullets < clipSize)
        {
            if (Input.GetButtonDown("Action"))
            {
                reloading = true;
            }

            if (Input.GetButtonUp("Action"))
            {
                reloading = false;
                shotTimer = 0;
            }
        }
    }

    void Shooting()
    {
        shotTimer += Time.deltaTime;

        if (reloading || boltActionTime > shotTimer)
        {
            canShoot = false;
        }
        else
        {
            canShoot = true;
        }

        if (bullets == 0)
        {
            canShoot = false;
        }

        if (shotTimer > reloadSpeed && reloading)
        {
            bullets += 1;
            shotTimer = 0;
        }

        if (bullets == clipSize && reloading)
        {
            bullets = clipSize;
            reloading = false;
        }


        


        if (reloading )
        {
            correctedWalkSpeed = Mathf.Lerp(correctedWalkSpeed, reloadWalkSpeed, walkSpeedsTransitionTime * Time.deltaTime);
        }
        else if (aiming)
        {
            correctedWalkSpeed = Mathf.Lerp(correctedWalkSpeed, aimingWalkSpeed, walkSpeedsTransitionTime * Time.deltaTime);
        }
        else if (lockedOn)
        {
            correctedWalkSpeed = Mathf.Lerp(correctedWalkSpeed, LockOnWalkSpeed, walkSpeedsTransitionTime * Time.deltaTime);
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



        //if (!lockedOn)
        //{

            if (stateString == "Jump" && currentJumpProfile == frontFlip)
            {
                gun.rotation = Quaternion.Euler(transform.localRotation.eulerAngles.x + gunTurnx, transform.localRotation.eulerAngles.y + gunTurny, transform.localRotation.eulerAngles.z + gunTurnx );

            }
            else
            {
                gun.rotation = transform.rotation;
            }
        //}
        //else
        //{
        //    if (stateString == "Jump" && currentJumpProfile == frontFlip)
        //    {
        //        gun.rotation = Quaternion.Euler(transform.localRotation.eulerAngles.x + gunTurnx, transform.localRotation.eulerAngles.y + gunTurny, transform.localRotation.eulerAngles.z + gunTurnx);

        //    }
        //    else
        //    {
        //        Vector3 lockOnRotation = (cubeTarget.position - gun.position);
        //        Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
        //        Debug.DrawLine(gun.position, cubeTarget.position, Color.red);
        //        gun.rotation = lookRotation;
        //    }


        //}

        //if (input.Current.ShootInput && canShoot && shotTimer > 0.5f)
        //{
        //    GameObject clone;
        //    clone = Instantiate(bullet, firePoint.position, gun.rotation);
        //    Destroy(clone, 3f);
        //    shotTimer = 0;
        //}
    }

    void DoAiming()
    {
        if (Input.GetAxisRaw("LeftTrigger") != 0)
        {
            aiming = true;
        }
        if (Input.GetAxisRaw("LeftTrigger") == 0)
        {
            aiming = false;
        }
    }
}



