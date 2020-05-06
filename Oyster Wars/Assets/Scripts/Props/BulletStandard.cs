using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletStandard : MonoBehaviour
{
    public float speed;
    public float hitDist;
    public Rigidbody rb;
    public int damage;
    public bool wadesBullet;
    public GameObject explosionEffect;
    public CapsuleCollider boat;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        boat = GameObject.Find("WadeBoat").GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        WadeMachine wade = other.gameObject.GetComponent<WadeMachine>();

        BulletStandard bullet = other.gameObject.GetComponent<BulletStandard>();

        if (wade != null)
        {
            if (!wadesBullet)
            {
                wade.DoTakeDamage(damage);
                Instantiate(explosionEffect, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }

        if (enemy != null)
        {
            enemy.DoTakeDamage(damage);
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }


        if(wade == null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }   
    }
}
