using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using KinematicCharacterController;
using System;

public class Enemy : MonoBehaviour
{
    public string stateString;
    public float health;
    public Camera cam;
    public WadeCamera wadeCamera;
    public float speed;
    public float acceleration;
    public float deceleration;
    public float startHealth;
    public float moveSpeed;
    public float verticalMoveSpeed;
    public float turnSpeed;
    public float throwForceDown;
    public List<Collider> IgnoredColliders = new List<Collider>();
    public Image healthBar;
    public Canvas healthCanvas;
    public bool added = false;
    public Vector3 enemyPosition;
    public Transform wade;
    public GameObject grenade;
    public GameObject bullet;
    public Transform firePoint;
    public float wadeCloseBy;
    public WadeMachine wadeMachine;
    public Transform groundDetection;
    public Transform groundDetectionFar;
    public float hitPower;
    public bool groundFar;
    public bool groundFront;
    public EnemySound enemysound;
    public GameObject pearlBag;


    [Header("Jump")]
    public float jumpHeight;
    public float timeToJumpApex;
    public float maxGravity;
    public float downMultiplier;
    public float initialForwardVelocity;
    public bool jumpForwards;
    public float gravSwitchStart;
    public float gravitySwitchRate;
    public float timeSinceLastJump;
    public float jumpCoolDown;
 

    public float WadeDistance()
    {
        wade = GameObject.Find("Wade").transform;
        return Vector3.Distance(transform.position, wade.position);
    }

    public float WadeVerticalDistance()
    {
        return Mathf.Abs(transform.position.y - wade.position.y);
    }

    public Quaternion WadeLookRotation(string flat)
    {
        Vector3 lockOnRotation = (wade.position - transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(lockOnRotation);

        if(flat == "flat")
        {
            lookRotation.x = 0;
            lookRotation.z = 0;
            return lookRotation;
        }
        else
        {
            return lookRotation;
        }


    }

    public void DoTakeDamage(float damage)
    {
        enemysound.PlayHit();
        health -= damage;
        if (health <= 0)
        {
            Instantiate(pearlBag, transform.position, transform.rotation);
            wadeCamera.enemiesOnScreen.Remove(transform);
            Destroy(gameObject);
        }
    }

    public int RandomInt(int min, int max)
    {
        return Random.Range(min, max + 1);
    }

    public void DoTargeting()
    {
        //First Create A Vector3 With Dimensions Based On The Camera's Viewport
         enemyPosition = cam.WorldToViewportPoint(gameObject.transform.position);

        //If The X And Y Values Are Between 0 And 1, The Enemy Is On Screen
        bool onScreen = enemyPosition.z > 0 && enemyPosition.z < 90 && enemyPosition.x > 0 && enemyPosition.x < 1 && enemyPosition.y > 0 && enemyPosition.y < 1;

        if (onScreen && !added && enemyPosition.z < 40)
        {
            wadeCamera.enemiesOnScreen.Add(transform);
            added = true;
        }

        if (!wadeCamera.enemiesOnScreen.Contains(transform))
        {
            added = false;
        }

        //else if (!onScreen)
        //{
        //    wadeCamera.enemiesOnScreen.Remove(transform);
        //    added = false;
        //}
        //If The Enemy Is On Screen Add It To The List Of Nearby Enemies Only Once
    }


    public void GrenadeThrow()
    {
        GameObject tempGrenade = Instantiate(grenade, transform.position + transform.forward + Vector3.down, WadeLookRotation("notflat")) as GameObject;
        CapsuleCollider grenadeCollider = tempGrenade.GetComponent<CapsuleCollider>();
        Grenade grenadeSc = tempGrenade.GetComponent<Grenade>();
        grenadeSc.IgnoreCollisions(GetComponent<Collider>());
        grenadeSc.enemyGrenade = true;
        IgnoredColliders.Add(grenadeCollider);
        Rigidbody tempGrenadeRB = tempGrenade.GetComponent<Rigidbody>();
        tempGrenadeRB.AddForce(tempGrenade.transform.forward * (WadeDistance() + 1), ForceMode.Impulse);
    }

    public void Shoot(string rotation)
    {
        enemysound.PlayRifleShot();

        if(rotation == "flat")
        {
            GameObject tempBullet = Instantiate(bullet, firePoint.position, WadeLookRotation("flat")) as GameObject;
        }
        else if(rotation == "notflat")
        {
            GameObject tempBullet = Instantiate(bullet, firePoint.position, WadeLookRotation("notflat")) as GameObject;
        }
        else if(rotation == "verticalaim")
        {
            GameObject tempBullet = Instantiate(bullet, firePoint.position, firePoint.rotation) as GameObject;
        }
        
    }

    public bool DetectForwardGround()
    {
        RaycastHit hit;
        return (Physics.Raycast(groundDetection.position, Vector3.down, out hit, 5));
    }

    public bool DetectForwardWall()
    {
        RaycastHit hit;
        return (Physics.Raycast(groundDetection.position, Vector3.forward, out hit, 5));
    }

    public bool DetectForwardGroundFar()
    {
        RaycastHit hit;
        return (Physics.Raycast(groundDetectionFar.position, Vector3.down, out hit, 7));
    }

    public bool DetectObstacle()
    {
        RaycastHit hit;
        return (Physics.Raycast(groundDetection.position, Vector3.forward, out hit, 5));
    }

    private void OnTriggerEnter(Collider other)
    {
        WadeMachine wade = other.GetComponent<WadeMachine>();
        if (wade != null)
        {
            wade.DoTakeDamage(hitPower);
        }
        print("dfdfs");
    }

}
