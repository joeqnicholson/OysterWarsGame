using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pearl : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        WadeMachine wade = other.GetComponent<WadeMachine>();
        if(wade != null)
        {
            wade.pearls += 1;

            Destroy(gameObject);
        }

    }
}
