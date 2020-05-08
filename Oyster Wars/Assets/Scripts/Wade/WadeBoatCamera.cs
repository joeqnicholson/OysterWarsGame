using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WadeBoatCamera : MonoBehaviour
{
    public float orbitSpeed;
    public float tilt;
    public float heading;
    public Transform target;
    public float distance;
    public float offsetY;

    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {

        tilt = Mathf.Clamp(tilt, 10, 50);
        
            if (Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > 0.01f) { tilt += -Input.GetAxisRaw("Mouse Y") * orbitSpeed * Time.deltaTime; }

            if (Mathf.Abs(Input.GetAxisRaw("Mouse X")) > 0.01f) { heading += -Input.GetAxisRaw("Mouse X") * orbitSpeed * Time.deltaTime; }



        Quaternion desiredRot = Quaternion.Euler(tilt, heading, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 12f * Time.deltaTime);

        Vector3 desiredPos = target.position - transform.forward * distance;
        transform.position = desiredPos + Vector3.up * offsetY;

    }
}
