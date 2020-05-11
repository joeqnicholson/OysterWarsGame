using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public int placeInList;

    private void OnTriggerEnter(Collider other)
    {
        WadeMachine wade = other.GetComponent<WadeMachine>();

        if(wade != null)
        {
            wade.ChangeWeapon(placeInList);
            Destroy(gameObject);
        }

        

    }

}
