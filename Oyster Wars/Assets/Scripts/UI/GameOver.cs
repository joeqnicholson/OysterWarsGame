using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameOver : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Button retry;
    public Button mainMenu;
    public float fadeInTimer;
    public EventSystem events;
    public RectTransform skullTransform;
    public bool isPause;
    
    void Start()
    {
        if (isPause)
        {
            Time.timeScale = 0;
            canvasGroup.alpha = 1;
            retry.enabled = true;
            mainMenu.enabled = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            retry.enabled = false;
            mainMenu.enabled = false;
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject music = GameObject.Find("MusicTrigger");
        

        if (!isPause)
        {
            canvasGroup.alpha += fadeInTimer * Time.deltaTime;
            Destroy(music);

            if (canvasGroup.alpha > .5f)
            {
                retry.enabled = true;
                mainMenu.enabled = true;
                float skullPosition = events.currentSelectedGameObject.GetComponent<RectTransform>().anchoredPosition.y;
                skullTransform.anchoredPosition = new Vector2(skullTransform.anchoredPosition.x, skullPosition);
            }
        }
        else
        {
            float skullPosition = events.currentSelectedGameObject.GetComponent<RectTransform>().anchoredPosition.y;
            skullTransform.anchoredPosition = new Vector2(skullTransform.anchoredPosition.x, skullPosition);
        }
        

    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
    
    public void MainMenuPause()
    {
        SceneManager.LoadScene(0);
    }
    public void Continue()
    {
        Time.timeScale = 1;
        WadeMachine wade = GameObject.Find("Wade").GetComponent<WadeMachine>();
        wade.inPause = false;
        Destroy(gameObject);  
    }
    public void RestartPause()
    {
        Time.timeScale = 1;
        WadeMachine wade = GameObject.Find("Wade").GetComponent<WadeMachine>();
        wade.inPause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Destroy(gameObject);
    }
}
