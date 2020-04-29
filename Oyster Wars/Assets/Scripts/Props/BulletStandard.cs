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


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, hitDist))
        {
            Enemy enemy = hit.collider.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.DoTakeDamage(damage);
            }
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }



        rb.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if(enemy != null)
        {
            enemy.health -= damage;
        }


        BulletStandard bullet = other.GetComponent<BulletStandard>();
        if(bullet != null)
        {

        }
        else
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }

        
    }
}
