using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

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
        textMove = new Vector3(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y"), 0);
        textMove = Vector3.ClampMagnitude(textMove, 1.0f);

        if(textMove.magnitude > .1)
        {
            print("work");
        }

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = -textMove * 30;
    }
}
