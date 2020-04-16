using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatMove : MonoBehaviour
{
    public Vector3 movementVector;

    [Range(0, 1)] public float movementFactor;
    public float period = 2f;

    Vector3 startingPos;

    void Start()
    {
        startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float cycles = Time.time / period;

        const float taau = Mathf.PI * 2f;
        float rawSineWave = Mathf.Sin(cycles * taau);

        movementFactor = rawSineWave / 2f + 0.5f;
        Vector3 offset = movementVector * movementFactor;
        transform.position = startingPos + offset;
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {

    }
}
