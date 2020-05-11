using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDestroy : MonoBehaviour
{
    public float time;
    float currentTime = 0;
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= time)
        {
            Destroy(gameObject);
        }
    }
}
