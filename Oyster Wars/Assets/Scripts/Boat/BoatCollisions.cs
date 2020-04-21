using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatCollisions : MonoBehaviour
{
    public GameObject wade;

        void Start()
        {

        wade = GameObject.Find("Wade");

            Physics.IgnoreCollision(GetComponent<Collider>(), wade.GetComponent<Collider>());
        }
}


