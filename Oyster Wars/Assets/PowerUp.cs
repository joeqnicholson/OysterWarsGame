using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public int placeInList;
    public bool isHealth;

    private void OnTriggerEnter(Collider other)
    {
        WadeMachine wade = other.GetComponent<WadeMachine>();

        if(wade != null)
        {
            if (!isHealth)
            {
                wade.ChangeWeapon(placeInList);
                Destroy(gameObject);
            }
            else
            {
                wade.health = wade.startHealth;
                Destroy(gameObject);
            }
            
        }

        

    }

}
