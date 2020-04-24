using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletStandard : MonoBehaviour
{
    public float speed;
    public Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = transform.forward * speed;
    }
}
