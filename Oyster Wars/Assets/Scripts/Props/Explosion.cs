using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Start is called before the first frame update

    public float delay = 2.1f;
    float counter;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        if(counter > delay)
        {
            Destroy(gameObject);
        }
    }
}
