using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletStandard : MonoBehaviour
{
    public float speed;
    public float hitDist;
    public Rigidbody rb;
    public int damage;
    public GameObject explosionEffect;
    public SphereCollider boat;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boat = GameObject.Find("boatsphere").GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        //RaycastHit hit;
        //if(Physics.Raycast(transform.position, transform.forward, out hit, hitDist))
        //{
        //    Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();
        //    WadeMachine wade = hit.collider.gameObject.GetComponent<WadeMachine>();

        //    if (wade != null)
        //    {
        //        wade.DoTakeDamage(damage);
        //    }

        //    if (enemy != null)
        //    {
        //        enemy.DoTakeDamage(damage);
        //    }

        //    if(hit.collider.gameObject.name == "boatsphere")
        //    {
        //        print("YAY");
        //    }
        //    else
        //    {
        //        Instantiate(explosionEffect, transform.position, transform.rotation);
        //        Destroy(gameObject);
        //    }
            
        //}




        rb.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        WadeMachine wade = other.gameObject.GetComponent<WadeMachine>();

        BulletStandard bullet = other.gameObject.GetComponent<BulletStandard>();

        if (wade != null)
        {
            wade.DoTakeDamage(damage);
        }

        if (enemy != null)
        {
            enemy.DoTakeDamage(damage);
        }

        if (other.gameObject.name == "boatsphere" || bullet != null)
        {
            print("YAY");
        }
        else
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }

    }
}
