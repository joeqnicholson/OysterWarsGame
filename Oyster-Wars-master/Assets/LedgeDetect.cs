using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDetect : MonoBehaviour
{
    public Transform topl;
    public Transform topr;
    public Transform bottoml;
    public Transform bottomr;
    public Transform middle;
    public Transform middleh;

    public bool toplHit;
    public bool toprHit;
    public bool bottomlHit;
    public bool bottomrHit;
    public bool middleHit;
    public bool middlehHit;
    public Vector3 toplNorm;
    public Vector3 toprNorm;
    public Vector3 bottomlNorm;
    public Vector3 bottomrNorm;
    public Vector3 middleNorm;
    public Vector3 middlehNorm;
    public float toplDist;
    public float toprDist;
    public float bottomlDist;
    public float bottomrDist;
    public float middleDist;
    public float middlehDist;
    public bool canGrab;


    public float hitDistTop;
    public float hitDistBottom;
    public float hitDistMiddle;
    public float hitDistMiddleh;



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit topLeft;
        RaycastHit topRight;
        RaycastHit bottomLeft;
        RaycastHit bottomRight;
        RaycastHit middleCast;
        RaycastHit middlehCast;

        //Debug.DrawRay(topr.position, Vector3.down * hitDistTop, Color.cyan);
        //Debug.DrawRay(topl.position, Vector3.down * hitDistTop, Color.cyan);
        //Debug.DrawRay(bottoml.position, transform.forward * hitDistBottom, Color.black);
        //Debug.DrawRay(bottomr.position, transform.forward * hitDistBottom, Color.black);
        //Debug.DrawRay(middle.position, transform.forward * hitDistMiddle, Color.red);
        //Debug.DrawRay(middleh.position, transform.forward * hitDistMiddleh, Color.red);




        if (Physics.Raycast(topr.position, Vector3.down, out topLeft, hitDistTop, layerMask))
        {
            toplHit = true;
            toplDist = topLeft.distance;
            toplNorm = topLeft.normal;
        }
        else
        {
            toplHit = false;
        }
        if (Physics.Raycast(topl.position, Vector3.down, out topRight, hitDistTop, layerMask))
        {
            toprHit = true;
            toprDist = topRight.distance;
            toprNorm = topRight.normal;
        }
        else
        {
            toprHit = false;
        }
        if (Physics.Raycast(bottoml.position, transform.forward, out bottomLeft, hitDistBottom, layerMask))
        {
            bottomlHit = true;
            bottomlDist = bottomLeft.distance;
            bottomlNorm = bottomLeft.normal;
        }
        else
        {
            bottomlHit = false;
        }
        if (Physics.Raycast(bottomr.position, transform.forward, out bottomRight, hitDistBottom, layerMask))
        {
            bottomrHit = true;
            bottomrDist = bottomRight.distance;
            bottomrNorm = bottomRight.normal;
        }
        else
        {
            bottomrHit = false;
        }
        if (Physics.Raycast(middle.position, transform.forward, out middleCast, hitDistBottom, layerMask))
        {
            middleHit = true;
            middleDist = middleCast.distance;
            middleNorm = middleCast.normal;
        }
        else
        {
            middleHit = false;
        }
        if (Physics.Raycast(middleh.position, transform.forward, out middlehCast, hitDistBottom, layerMask))
        {
            middlehHit = true;
            middlehDist = middlehCast.distance;
            middlehNorm = middlehCast.normal;
        }
        else
        {
            middlehHit = false;
        }


        if (toplHit || toprHit)
        {
            canGrab = true;
        }
        else
        {
            canGrab = false;
        }
    }
}
