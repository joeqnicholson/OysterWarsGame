using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class TextMovement : MonoBehaviour
{
    public WadeInputs input;
    Vector2 textMove;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textMove =input.Current.MoveInput;
        textMove = Vector2.ClampMagnitude(textMove, .5f);

        if(textMove.magnitude > .1)
        {
            print("work");
        }

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = -textMove * 10;
    }
}
