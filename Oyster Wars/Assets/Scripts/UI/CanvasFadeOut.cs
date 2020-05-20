using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasFadeOut : MonoBehaviour
{
    public float jimboFadeSpeed;
    public float jimboFade;
    public float jimboHit;
    public float jimboTimer;
    public float jimbo;
    public bool switched = true;
    public bool start;
    public CanvasGroup im;
    public CanvasGroup jim;
    public GameObject jim2;
    float fadeOut;
    
    // Start is called before the first frame update
    void Start()
    {
        jim = jim2.GetComponent<CanvasGroup>();
        im = GetComponent<CanvasGroup>();
        jim.alpha = 0.0f;
        jimbo = 0;
        fadeOut = 1;
        start = false;
    }

    // Update is called once per frame
    void Update()
    {
        jimboTimer += Time.deltaTime;

        if(jimboTimer > jimboHit && switched)
        {
            
            jimbo = 1;
            jim.alpha = 1.0f;
            switched = false;
        }

        if(jimboTimer > jimboFade)
        {
            jimbo -= jimboFadeSpeed * Time.deltaTime;
        }

        if(im.alpha == 0)
        {
            Gamepad gamepad = InputSystem.GetDevice<Gamepad>();
            if (gamepad.startButton.wasPressedThisFrame)
            {
                start = true;
            }
            if (start)
            {
                fadeOut -= jimboFadeSpeed * Time.deltaTime * 1.5f;
                jim.alpha = fadeOut;
                if (jim.alpha == 0)
                {
                    SceneManager.LoadScene(1);
                }
            }
        }


        im.alpha = (jimbo);
    }
}
