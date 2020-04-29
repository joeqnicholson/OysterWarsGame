using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMovement : MonoBehaviour
{
    Vector2 textMove;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textMove = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        textMove = Vector2.ClampMagnitude(textMove, 1.0f);

        transform.position = textMove;
    }
}
