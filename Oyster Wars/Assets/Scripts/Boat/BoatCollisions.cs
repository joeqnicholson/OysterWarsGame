using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatCollisions : MonoBehaviour
{
    public GameObject wadeboat;

        void Start()
        {

        wadeboat = GameObject.Find("WadeBoat");

            Physics.IgnoreCollision(GetComponent<Collider>(), wadeboat.GetComponent<CapsuleCollider>());
        }
}


