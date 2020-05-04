using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float delay = 3f;
    public float blastRadius;
    public int damage = 30;
    float countdown = 0;
    public bool enemyGrenade;

    public GameObject wade;
    public GameObject explosionEffect;

    void Start()
    {
        wade = GameObject.Find("Wade");

        Physics.IgnoreCollision(GetComponent<Collider>(), wade.GetComponent<CapsuleCollider>());
    }

    // Update is called once per frame
    void Update()
    {
        countdown += Time.deltaTime;
        if(countdown >= delay)
        {
            Explode();
        }
    }

    void Explode()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Enemy enemy = nearbyObject.GetComponent<Enemy>();
            WadeMachine wade = nearbyObject.GetComponent<WadeMachine>();
            if(enemy != null)
            {
                if (!enemyGrenade)
                {
                    enemy.DoTakeDamage(damage);
                }
                
            }

            if(wade != null)
            {
                wade.DoTakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
    public void IgnoreCollisions(Collider collider)
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), collider);
    }
}
