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
        WadeBoatController boat = other.gameObject.GetComponent<WadeBoatController>();
        BulletStandard bullet = other.gameObject.GetComponent<BulletStandard>();


        if (wade)
        {
            if (!wadesBullet)
            {
                wade.DoTakeDamage(damage);
                Instantiate(explosionEffect, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }

        if (enemy)
        {
            if (wadesBullet)
            {
                enemy.DoTakeDamage(damage);
                Instantiate(explosionEffect, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }


        if(!wade && !bullet && !boat)
        {
            if(other.gameObject.name != "BoatTrigger" && other.gameObject.name != "Mover")
            {
                Instantiate(explosionEffect, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }   
    }
}
