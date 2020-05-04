using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController;
using System;

public class Enemy : MonoBehaviour
{
    public float health;
    public Camera cam;
    public WadeCamera wadeCamera;
    public float startHealth;
    public float moveSpeed;
    public float turnSpeed;
    public float throwForceDown;
    public List<Collider> IgnoredColliders = new List<Collider>();
    public Image healthBar;
    public Canvas healthCanvas;
    public bool added = false;
    public Vector3 enemyPosition;

    public void DoTakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            wadeCamera.enemiesOnScreen.Remove(transform);
            Destroy(gameObject);
        }
    }

    public void DoTargeting()
    {
        //First Create A Vector3 With Dimensions Based On The Camera's Viewport
         enemyPosition = cam.WorldToViewportPoint(gameObject.transform.position);

        //If The X And Y Values Are Between 0 And 1, The Enemy Is On Screen
        bool onScreen = enemyPosition.z > 0 && enemyPosition.z < 50 && enemyPosition.x > 0 && enemyPosition.x < 1 && enemyPosition.y > 0 && enemyPosition.y < 1;

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
}
