using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WadeMachine : MonoBehaviour
{

    public class JumpProfile
    {
        public bool CanSlash = true;
        public bool CanPogo = true;
        public float AirSpeed = 9;
        public bool CanControlHeight = false;
        public float JumpHeight;
        public float InitialForwardVelocity;
        public float timeToJumpApex = 0.5f;
        public float Acceleration = 8;
        public string Animation;
        public string FallAnimation;
        public float CrossFadeTime;
    }

    public JumpProfile currentJumpProfile;

    private JumpProfile jumpStandard = new JumpProfile
    {
        Animation = "Jump",
        CanControlHeight = true,
        JumpHeight = 5f,
        timeToJumpApex = .4f
    };

    private JumpProfile jumpHigh = new JumpProfile
    {
        JumpHeight = 8.0f,
        timeToJumpApex = .5f,
        Acceleration = 13.0f,
        Animation = "HighJump"
    };

    private JumpProfile jumpSideFlip = new JumpProfile
    {
        JumpHeight = 5.0f,
    };

    private JumpProfile jumpLong = new JumpProfile
    {
        JumpHeight = 3.4f,
        InitialForwardVelocity = 7.5f,
        timeToJumpApex = .6f,
        CrossFadeTime = 0.1f,
        Animation = "LongJump"
    };

    private JumpProfile jumpWall = new JumpProfile
    {
        JumpHeight = 3.4f,
        InitialForwardVelocity = 4.4f,
        timeToJumpApex = .4f,
        Animation = "WallJump"
    };

    private JumpProfile jumpSlash = new JumpProfile
    {
        CanSlash = false,
        JumpHeight = 0.8f,
    };

    private JumpProfile jumpSlashInterruption = new JumpProfile
    {
        CanControlHeight = true,
        JumpHeight = 3.0f,
        Acceleration = 40.0f,
        InitialForwardVelocity = 5.0f,
        timeToJumpApex = .4f,
        CanSlash = false,
        AirSpeed = 9,
    };

    private JumpProfile jumpSlamInteruption = new JumpProfile
    {
        CanControlHeight = true,
        JumpHeight = 3.5f,
        Acceleration = 30.0f,
        AirSpeed = 9,
        timeToJumpApex = .4f,
        CanPogo = false,
    };

    private JumpProfile sparkExit = new JumpProfile

    {
        JumpHeight = 2,
        timeToJumpApex = 0.3f,
        InitialForwardVelocity = 0
    };

    private JumpProfile fallProfile = new JumpProfile

    {
        JumpHeight = 0,
        timeToJumpApex = 0,
        InitialForwardVelocity = 0,
        Animation = "Fall"
    };






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

    //private Vector3 LocalAirMovement()
    //{
    //    Vector3 intent;
    //    Vector3 camU;

    //    camF = cam.forward;
    //    camR = cam.right;

    //    camF.y = 0;
    //    camR.y = 0;


    //    camU.y = 0;

    //    intent = (camF * 3 * input.Current.MoveInput.z + camR * 3 * input.Current.MoveInput.x) + Vector3.up * 2;

    //    return intent;

    //}

    private Vector3 LocalMovement()
    {
        Vector3 intent;

        camF = cam.forward;
        camR = cam.right;

        camF.y = 0;
        camR.y = 0;

        camF = camF.normalized;
        camR = camR.normalized;


        
        intent = (camF * _moveInputVector.z + camR * _moveInputVector.x);

        return intent;
    }


    private Vector3 airLocalMovement(int num)
    {
        Vector3 intent;

        camF = cam.forward;
        camR = cam.right;

        camF.y = 0;
        camR.y = 0;

        camF = camF.normalized;
        camR = camR.normalized;


        if(num == 1)
        {
            intent = (camF * 1);
        }
        else
        {
            intent = (camF * _moveInputVector.z + camR * _moveInputVector.x);
        }

        return intent;
    }

    void Shooting()
    {
        if (Input.GetButtonDown("LockOn")) { num++; }

        //shotTimer += Time.deltaTime;

        if (num % 2 == 0) { lockedOn = true; }
        else { lockedOn = false; }

        if (!lockedOn)
        {
            gun.rotation = transform.rotation;
        }
        else
        {
            Vector3 lockOnRotation = (lockOnTarget.position - gun.position);
            Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);
            gun.rotation = lookRotation;
        }

        //if (input.Current.ShootInput && canShoot && shotTimer > 0.5f)
        //{
        //    GameObject clone;
        //    clone = Instantiate(bullet, firePoint.position, gun.rotation);
        //    Destroy(clone, 3f);
        //    shotTimer = 0;
        //}
    }


}