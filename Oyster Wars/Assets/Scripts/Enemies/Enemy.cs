using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController;
using System;

public class Enemy : MonoBehaviour
{
    public float health;
    public float startHealth;
    public float moveSpeed;
    public float turnSpeed;
    public float throwForceDown;
    public List<Collider> IgnoredColliders = new List<Collider>();
    public Image healthBar;
    public Canvas healthCanvas;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
