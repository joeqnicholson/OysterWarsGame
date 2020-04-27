using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float delay = 3f;
    float countdown;
    public GameObject wade;

    void Start()
    {
        wade = GameObject.Find("Wade");

        Physics.IgnoreCollision(GetComponent<Collider>(), wade.GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if(countdown <= 0f)
        {
            Explode();
        }
    }

    void Explode()
    {

    }
}
