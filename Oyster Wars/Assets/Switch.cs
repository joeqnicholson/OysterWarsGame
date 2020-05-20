using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Switch : MonoBehaviour
{
    public Animator anim;
    public Animator doorAnim;
    public bool raiseDoor;
    public bool canUse;

    

    private void Update()
    {

        if (canUse)
        {
            Gamepad gamepad = InputSystem.GetDevice<Gamepad>();
            if (gamepad.buttonNorth.wasPressedThisFrame)
            {
                anim.SetBool("Switch", true);
                doorAnim.SetBool("Rise", true);
                raiseDoor = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        WadeMachine wade = other.GetComponent<WadeMachine>();
        if(wade != null)
        {
            canUse = true;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        WadeMachine wade = other.GetComponent<WadeMachine>();
        if (wade != null)
        {
            canUse = false;
        }
    }
}
