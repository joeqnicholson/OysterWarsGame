using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float delay = 3f;
    float countdown = 0;

    public GameObject wade;
    public GameObject explosionEffect;

    void Start()
    {
        wade = GameObject.Find("Wade");

        Physics.IgnoreCollision(GetComponent<Collider>(), wade.GetComponent<Collider>());
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


        Destroy(gameObject);
    }
}
